using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Learning.Contracts;
using Operis_API.Modules.Learning.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Learning.Application;

public sealed class LearningCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    ILearningQueries queries) : ILearningCommands
{
    private static readonly string[] CourseStatuses = ["draft", "active", "retired"];
    private static readonly string[] RequirementStatuses = ["active", "archived"];
    private static readonly string[] CompletionStatuses = ["assigned", "completed"];
    private static readonly string[] CompetencyReviewStatuses = ["planned", "in_progress", "completed", "archived"];

    public async Task<LearningCommandResult<TrainingCourseResponse>> CreateTrainingCourseAsync(CreateTrainingCourseRequest request, string? actor, CancellationToken cancellationToken)
    {
        var title = NormalizeRequired(request.Title, 256);
        if (title is null)
        {
            return Validation<TrainingCourseResponse>(ApiErrorCodes.TrainingCourseTitleRequired, "Training course title is required.");
        }

        var courseCode = NormalizeOptional(request.CourseCode, 128);
        if (!string.IsNullOrWhiteSpace(courseCode)
            && await dbContext.TrainingCourses.AnyAsync(x => x.CourseCode == courseCode, cancellationToken))
        {
            return Conflict<TrainingCourseResponse>(ApiErrorCodes.TrainingCourseCodeDuplicate, "Training course code already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new TrainingCourseEntity
        {
            Id = Guid.NewGuid(),
            CourseCode = courseCode,
            Title = title,
            Description = NormalizeOptional(request.Description, 4000),
            Provider = NormalizeOptional(request.Provider, 256),
            DeliveryMode = NormalizeOptional(request.DeliveryMode, 128),
            AudienceScope = NormalizeOptional(request.AudienceScope, 256),
            ValidityMonths = Math.Max(request.ValidityMonths, 0),
            Status = "draft",
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.TrainingCourses.Add(entity);
        await PersistWithAuditAsync("create", "training_course", entity.Id.ToString(), StatusCodes.Status201Created, entity, cancellationToken);
        return await SuccessCourseAsync(entity.Id, cancellationToken);
    }

    public async Task<LearningCommandResult<TrainingCourseResponse>> UpdateTrainingCourseAsync(Guid courseId, UpdateTrainingCourseRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TrainingCourses.SingleOrDefaultAsync(x => x.Id == courseId, cancellationToken);
        if (entity is null)
        {
            return NotFound<TrainingCourseResponse>(ApiErrorCodes.TrainingCourseNotFound, "Training course not found.");
        }

        if (entity.Status == "retired")
        {
            return Validation<TrainingCourseResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Retired training courses cannot be edited.");
        }

        var title = NormalizeRequired(request.Title, 256);
        if (title is null)
        {
            return Validation<TrainingCourseResponse>(ApiErrorCodes.TrainingCourseTitleRequired, "Training course title is required.");
        }

        var courseCode = NormalizeOptional(request.CourseCode, 128);
        if (!string.Equals(entity.CourseCode, courseCode, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(courseCode)
            && await dbContext.TrainingCourses.AnyAsync(x => x.CourseCode == courseCode, cancellationToken))
        {
            return Conflict<TrainingCourseResponse>(ApiErrorCodes.TrainingCourseCodeDuplicate, "Training course code already exists.");
        }

        entity.CourseCode = courseCode;
        entity.Title = title;
        entity.Description = NormalizeOptional(request.Description, 4000);
        entity.Provider = NormalizeOptional(request.Provider, 256);
        entity.DeliveryMode = NormalizeOptional(request.DeliveryMode, 128);
        entity.AudienceScope = NormalizeOptional(request.AudienceScope, 256);
        entity.ValidityMonths = Math.Max(request.ValidityMonths, 0);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await PersistWithAuditAsync("update", "training_course", entity.Id.ToString(), StatusCodes.Status200OK, entity, cancellationToken);
        return await SuccessCourseAsync(entity.Id, cancellationToken);
    }

    public async Task<LearningCommandResult<TrainingCourseResponse>> TransitionTrainingCourseAsync(Guid courseId, TransitionTrainingCourseRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TrainingCourses.SingleOrDefaultAsync(x => x.Id == courseId, cancellationToken);
        if (entity is null)
        {
            return NotFound<TrainingCourseResponse>(ApiErrorCodes.TrainingCourseNotFound, "Training course not found.");
        }

        var targetStatus = NormalizeStatus(request.TargetStatus, CourseStatuses);
        if (!IsValidCourseTransition(entity.Status, targetStatus))
        {
            return Validation<TrainingCourseResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Training course transition is invalid.");
        }

        entity.Status = targetStatus;
        entity.ActivatedAt = targetStatus == "active" ? entity.ActivatedAt ?? DateTimeOffset.UtcNow : entity.ActivatedAt;
        entity.RetiredAt = targetStatus == "retired" ? DateTimeOffset.UtcNow : entity.RetiredAt;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await PersistWithAuditAsync("transition", "training_course", entity.Id.ToString(), StatusCodes.Status200OK, entity, cancellationToken);
        return await SuccessCourseAsync(entity.Id, cancellationToken);
    }

    public async Task<LearningCommandResult<RoleTrainingRequirementResponse>> CreateRoleTrainingRequirementAsync(CreateRoleTrainingRequirementRequest request, string? actor, CancellationToken cancellationToken)
    {
        if (request.ProjectRoleId == Guid.Empty)
        {
            return Validation<RoleTrainingRequirementResponse>(ApiErrorCodes.TrainingRequirementRoleRequired, "Training requirement role is required.");
        }

        if (!await dbContext.TrainingCourses.AnyAsync(x => x.Id == request.CourseId, cancellationToken))
        {
            return NotFound<RoleTrainingRequirementResponse>(ApiErrorCodes.TrainingCourseNotFound, "Training course not found.");
        }

        if (!await dbContext.ProjectRoles.AnyAsync(x => x.Id == request.ProjectRoleId && x.DeletedAt == null, cancellationToken))
        {
            return NotFound<RoleTrainingRequirementResponse>(ApiErrorCodes.ProjectRoleRequired, "Project role not found.");
        }

        if (await dbContext.RoleTrainingRequirements.AnyAsync(x => x.CourseId == request.CourseId && x.ProjectRoleId == request.ProjectRoleId, cancellationToken))
        {
            return Conflict<RoleTrainingRequirementResponse>(ApiErrorCodes.TrainingRequirementExists, "Training requirement already exists for this role.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new RoleTrainingRequirementEntity
        {
            Id = Guid.NewGuid(),
            CourseId = request.CourseId,
            ProjectRoleId = request.ProjectRoleId,
            RequiredWithinDays = Math.Max(request.RequiredWithinDays, 0),
            RenewalIntervalMonths = Math.Max(request.RenewalIntervalMonths, 0),
            Status = "active",
            Notes = NormalizeOptional(request.Notes, 2000),
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.RoleTrainingRequirements.Add(entity);
        await PersistWithAuditAsync("create", "role_training_requirement", entity.Id.ToString(), StatusCodes.Status201Created, entity, cancellationToken);
        return await SuccessRequirementAsync(entity.Id, cancellationToken);
    }

    public async Task<LearningCommandResult<RoleTrainingRequirementResponse>> UpdateRoleTrainingRequirementAsync(Guid requirementId, UpdateRoleTrainingRequirementRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.RoleTrainingRequirements.SingleOrDefaultAsync(x => x.Id == requirementId, cancellationToken);
        if (entity is null)
        {
            return NotFound<RoleTrainingRequirementResponse>(ApiErrorCodes.TrainingRequirementNotFound, "Training requirement not found.");
        }

        if (request.ProjectRoleId == Guid.Empty)
        {
            return Validation<RoleTrainingRequirementResponse>(ApiErrorCodes.TrainingRequirementRoleRequired, "Training requirement role is required.");
        }

        if (!await dbContext.TrainingCourses.AnyAsync(x => x.Id == request.CourseId, cancellationToken))
        {
            return NotFound<RoleTrainingRequirementResponse>(ApiErrorCodes.TrainingCourseNotFound, "Training course not found.");
        }

        if (!await dbContext.ProjectRoles.AnyAsync(x => x.Id == request.ProjectRoleId && x.DeletedAt == null, cancellationToken))
        {
            return NotFound<RoleTrainingRequirementResponse>(ApiErrorCodes.ProjectRoleRequired, "Project role not found.");
        }

        if ((entity.CourseId != request.CourseId || entity.ProjectRoleId != request.ProjectRoleId)
            && await dbContext.RoleTrainingRequirements.AnyAsync(x => x.CourseId == request.CourseId && x.ProjectRoleId == request.ProjectRoleId, cancellationToken))
        {
            return Conflict<RoleTrainingRequirementResponse>(ApiErrorCodes.TrainingRequirementExists, "Training requirement already exists for this role.");
        }

        entity.CourseId = request.CourseId;
        entity.ProjectRoleId = request.ProjectRoleId;
        entity.RequiredWithinDays = Math.Max(request.RequiredWithinDays, 0);
        entity.RenewalIntervalMonths = Math.Max(request.RenewalIntervalMonths, 0);
        entity.Status = NormalizeStatus(request.Status, RequirementStatuses);
        entity.Notes = NormalizeOptional(request.Notes, 2000);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await PersistWithAuditAsync("update", "role_training_requirement", entity.Id.ToString(), StatusCodes.Status200OK, entity, cancellationToken);
        return await SuccessRequirementAsync(entity.Id, cancellationToken);
    }

    public async Task<LearningCommandResult<TrainingCompletionResponse>> RecordTrainingCompletionAsync(RecordTrainingCompletionRequest request, string? actor, CancellationToken cancellationToken)
    {
        if (!await dbContext.TrainingCourses.AnyAsync(x => x.Id == request.CourseId, cancellationToken))
        {
            return NotFound<TrainingCompletionResponse>(ApiErrorCodes.TrainingCourseNotFound, "Training course not found.");
        }

        if (!await dbContext.ProjectRoles.AnyAsync(x => x.Id == request.ProjectRoleId && x.DeletedAt == null, cancellationToken))
        {
            return NotFound<TrainingCompletionResponse>(ApiErrorCodes.ProjectRoleRequired, "Project role not found.");
        }

        if (!await dbContext.Projects.AnyAsync(x => x.Id == request.ProjectId, cancellationToken))
        {
            return NotFound<TrainingCompletionResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        var status = NormalizeStatus(request.Status, CompletionStatuses);
        if (status == "completed" && !request.CompletionDate.HasValue)
        {
            return Validation<TrainingCompletionResponse>(ApiErrorCodes.TrainingCompletionDateRequired, "Training completion date is required.");
        }

        var requirement = await dbContext.RoleTrainingRequirements.AsNoTracking()
            .SingleOrDefaultAsync(x => x.CourseId == request.CourseId && x.ProjectRoleId == request.ProjectRoleId, cancellationToken);
        if (requirement is null)
        {
            return NotFound<TrainingCompletionResponse>(ApiErrorCodes.TrainingRequirementNotFound, "Training requirement not found.");
        }

        var existing = await dbContext.TrainingCompletions.SingleOrDefaultAsync(
            x => x.CourseId == request.CourseId
                 && x.ProjectRoleId == request.ProjectRoleId
                 && x.ProjectId == request.ProjectId
                 && x.UserId == request.UserId,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var assignedAt = request.AssignedAt ?? existing?.AssignedAt ?? now;
        var dueAt = request.DueAt ?? existing?.DueAt ?? assignedAt.AddDays(Math.Max(requirement.RequiredWithinDays, 0));
        var validityMonths = await dbContext.TrainingCourses.Where(x => x.Id == request.CourseId).Select(x => x.ValidityMonths).SingleAsync(cancellationToken);
        var expiryAt = status == "completed" ? ComputeExpiry(request.CompletionDate, requirement.RenewalIntervalMonths, validityMonths) : null;

        if (existing is null)
        {
            existing = new TrainingCompletionEntity
            {
                Id = Guid.NewGuid(),
                CourseId = request.CourseId,
                ProjectRoleId = request.ProjectRoleId,
                ProjectId = request.ProjectId,
                UserId = request.UserId.Trim(),
                Status = status,
                AssignedAt = assignedAt,
                DueAt = dueAt,
                CompletionDate = request.CompletionDate,
                ExpiryDate = expiryAt,
                EvidenceRef = NormalizeOptional(request.EvidenceRef, 512),
                Notes = NormalizeOptional(request.Notes, 2000),
                CreatedAt = now,
                UpdatedAt = now
            };
            dbContext.TrainingCompletions.Add(existing);
            await PersistWithAuditAsync("create", "training_completion", existing.Id.ToString(), StatusCodes.Status201Created, existing, cancellationToken);
        }
        else
        {
            existing.Status = status;
            existing.AssignedAt = assignedAt;
            existing.DueAt = dueAt;
            existing.CompletionDate = request.CompletionDate;
            existing.ExpiryDate = expiryAt;
            existing.EvidenceRef = NormalizeOptional(request.EvidenceRef, 512);
            existing.Notes = NormalizeOptional(request.Notes, 2000);
            existing.UpdatedAt = now;
            await PersistWithAuditAsync("update", "training_completion", existing.Id.ToString(), StatusCodes.Status200OK, existing, cancellationToken);
        }

        return await SuccessCompletionAsync(existing.Id, cancellationToken);
    }

    public async Task<LearningCommandResult<TrainingCompletionResponse>> UpdateTrainingCompletionAsync(Guid completionId, UpdateTrainingCompletionRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TrainingCompletions.SingleOrDefaultAsync(x => x.Id == completionId, cancellationToken);
        if (entity is null)
        {
            return NotFound<TrainingCompletionResponse>(ApiErrorCodes.TrainingCompletionNotFound, "Training completion not found.");
        }

        var status = NormalizeStatus(request.Status, CompletionStatuses);
        if (status == "completed" && !request.CompletionDate.HasValue)
        {
            return Validation<TrainingCompletionResponse>(ApiErrorCodes.TrainingCompletionDateRequired, "Training completion date is required.");
        }

        var requirement = await dbContext.RoleTrainingRequirements.AsNoTracking()
            .SingleOrDefaultAsync(x => x.CourseId == entity.CourseId && x.ProjectRoleId == entity.ProjectRoleId, cancellationToken);
        if (requirement is null)
        {
            return NotFound<TrainingCompletionResponse>(ApiErrorCodes.TrainingRequirementNotFound, "Training requirement not found.");
        }

        var validityMonths = await dbContext.TrainingCourses.Where(x => x.Id == entity.CourseId).Select(x => x.ValidityMonths).SingleAsync(cancellationToken);
        entity.Status = status;
        entity.DueAt = request.DueAt ?? entity.DueAt;
        entity.CompletionDate = request.CompletionDate;
        entity.ExpiryDate = status == "completed" ? ComputeExpiry(request.CompletionDate, requirement.RenewalIntervalMonths, validityMonths) : null;
        entity.EvidenceRef = NormalizeOptional(request.EvidenceRef, 512);
        entity.Notes = NormalizeOptional(request.Notes, 2000);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await PersistWithAuditAsync("update", "training_completion", entity.Id.ToString(), StatusCodes.Status200OK, entity, cancellationToken);
        return await SuccessCompletionAsync(entity.Id, cancellationToken);
    }

    public async Task<LearningCommandResult<CompetencyReviewResponse>> CreateCompetencyReviewAsync(CreateCompetencyReviewRequest request, string? actor, CancellationToken cancellationToken)
    {
        if (request.ProjectId.HasValue && !await dbContext.Projects.AnyAsync(x => x.Id == request.ProjectId.Value, cancellationToken))
        {
            return NotFound<CompetencyReviewResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new CompetencyReviewEntity
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId.Trim(),
            ProjectId = request.ProjectId,
            ReviewPeriod = NormalizeRequired(request.ReviewPeriod, 128) ?? now.ToString("yyyy-MM"),
            ReviewerUserId = NormalizeRequired(request.ReviewerUserId, 128) ?? string.Empty,
            Status = "planned",
            Summary = NormalizeOptional(request.Summary, 2000),
            PlannedAt = request.PlannedAt,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.CompetencyReviews.Add(entity);
        await PersistWithAuditAsync("create", "competency_review", entity.Id.ToString(), StatusCodes.Status201Created, entity, cancellationToken);
        return await SuccessCompetencyReviewAsync(entity.Id, cancellationToken);
    }

    public async Task<LearningCommandResult<CompetencyReviewResponse>> UpdateCompetencyReviewAsync(Guid reviewId, UpdateCompetencyReviewRequest request, string? actor, CancellationToken cancellationToken)
    {
        var entity = await dbContext.CompetencyReviews.SingleOrDefaultAsync(x => x.Id == reviewId, cancellationToken);
        if (entity is null)
        {
            return NotFound<CompetencyReviewResponse>(ApiErrorCodes.CompetencyReviewNotFound, "Competency review not found.");
        }

        entity.ReviewPeriod = NormalizeRequired(request.ReviewPeriod, 128) ?? entity.ReviewPeriod;
        entity.ReviewerUserId = NormalizeRequired(request.ReviewerUserId, 128) ?? entity.ReviewerUserId;
        entity.Status = NormalizeStatus(request.Status, CompetencyReviewStatuses);
        entity.PlannedAt = request.PlannedAt;
        entity.CompletedAt = entity.Status == "completed" ? request.CompletedAt ?? DateTimeOffset.UtcNow : request.CompletedAt;
        entity.Summary = NormalizeOptional(request.Summary, 2000);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await PersistWithAuditAsync("update", "competency_review", entity.Id.ToString(), StatusCodes.Status200OK, entity, cancellationToken);
        return await SuccessCompetencyReviewAsync(entity.Id, cancellationToken);
    }

    private async Task<LearningCommandResult<TrainingCourseResponse>> SuccessCourseAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var response = await queries.ListTrainingCoursesAsync(new TrainingCourseListQuery(null, null, 1, 500), cancellationToken);
        return Success(response.Items.First(x => x.Id == courseId));
    }

    private async Task<LearningCommandResult<RoleTrainingRequirementResponse>> SuccessRequirementAsync(Guid requirementId, CancellationToken cancellationToken)
    {
        var response = await queries.ListRoleTrainingRequirementsAsync(new RoleTrainingMatrixQuery(null, null, null, null, null, 1, 500), cancellationToken);
        return Success(response.Items.First(x => x.Id == requirementId));
    }

    private async Task<LearningCommandResult<TrainingCompletionResponse>> SuccessCompletionAsync(Guid completionId, CancellationToken cancellationToken)
    {
        var response = await queries.ListTrainingCompletionsAsync(new TrainingCompletionListQuery(null, null, null, null, null, false, null, 1, 2000), cancellationToken);
        return Success(response.Items.First(x => x.Id == completionId || x.Id == Guid.Empty));
    }

    private async Task<LearningCommandResult<CompetencyReviewResponse>> SuccessCompetencyReviewAsync(Guid reviewId, CancellationToken cancellationToken)
    {
        var response = await queries.ListCompetencyReviewsAsync(new CompetencyReviewListQuery(null, null, null, null, 1, 500), cancellationToken);
        return Success(response.Items.First(x => x.Id == reviewId));
    }

    private async Task PersistWithAuditAsync(string action, string entityType, string entityId, int statusCode, object after, CancellationToken cancellationToken)
    {
        auditLogWriter.Append(new AuditLogEntry("learning", action, entityType, entityId, StatusCode: statusCode, After: after));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool IsValidCourseTransition(string current, string next) =>
        (NormalizeKey(current), next) switch
        {
            ("draft", "draft") => true,
            ("draft", "active") => true,
            ("active", "active") => true,
            ("active", "retired") => true,
            ("retired", "retired") => true,
            _ => false
        };

    private static string NormalizeStatus(string? value, IReadOnlyList<string> allowed)
    {
        var normalized = NormalizeKey(value) ?? allowed[0];
        return allowed.Contains(normalized, StringComparer.Ordinal) ? normalized : allowed[0];
    }

    private static string? NormalizeRequired(string? value, int maxLength) =>
        string.IsNullOrWhiteSpace(value) ? null : NormalizeOptional(value, maxLength);

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static string? NormalizeKey(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static DateTimeOffset? ComputeExpiry(DateTimeOffset? completionDate, int renewalIntervalMonths, int validityMonths)
    {
        if (!completionDate.HasValue)
        {
            return null;
        }

        var months = renewalIntervalMonths > 0 ? renewalIntervalMonths : Math.Max(validityMonths, 0);
        return months > 0 ? completionDate.Value.AddMonths(months) : null;
    }

    private static LearningCommandResult<T> Success<T>(T value) => new(LearningCommandStatus.Success, value);
    private static LearningCommandResult<T> Validation<T>(string code, string message) => new(LearningCommandStatus.ValidationError, default, message, code);
    private static LearningCommandResult<T> NotFound<T>(string code, string message) => new(LearningCommandStatus.NotFound, default, message, code);
    private static LearningCommandResult<T> Conflict<T>(string code, string message) => new(LearningCommandStatus.Conflict, default, message, code);
}
