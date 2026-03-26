using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Audits.Application;
using Operis_API.Modules.Risks.Contracts;
using Operis_API.Modules.Risks.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Risks.Application;

public sealed class RiskCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IBusinessAuditEventWriter businessAuditEventWriter,
    IRiskQueries queries) : IRiskCommands
{
    private static readonly string[] IssueStatuses = ["open", "in_progress", "resolved", "closed"];
    private static readonly string[] IssueActionStatuses = ["open", "in_progress", "completed", "verified"];
    private static readonly string[] IssueSeverities = ["low", "medium", "high", "critical"];

    public async Task<RiskCommandResult<RiskDetailResponse>> CreateRiskAsync(CreateRiskRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var validationError = await ValidateRiskRequestAsync(request.ProjectId, request.Code, request.Title, request.Description, request.OwnerUserId, request.Probability, request.Impact, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var now = DateTimeOffset.UtcNow;
        var risk = new RiskEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Code = normalizedCode,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Probability = request.Probability,
            Impact = request.Impact,
            OwnerUserId = request.OwnerUserId.Trim(),
            MitigationPlan = TrimOrNull(request.MitigationPlan),
            Cause = TrimOrNull(request.Cause),
            Effect = TrimOrNull(request.Effect),
            ContingencyPlan = TrimOrNull(request.ContingencyPlan),
            Status = "draft",
            NextReviewAt = request.NextReviewAt,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Add(risk);
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("create", "risk", risk.Id, StatusCodes.Status201Created, new { risk.Code, risk.Status }, cancellationToken);
        await AppendBusinessEventAsync("risk_created", "risk", risk.Id, actorUserId, "Created risk", null, new { risk.Code, risk.Title }, cancellationToken);
        return await SuccessRiskAsync(risk.Id, cancellationToken);
    }

    public async Task<RiskCommandResult<RiskDetailResponse>> UpdateRiskAsync(Guid riskId, UpdateRiskRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var risk = await dbContext.Set<RiskEntity>().SingleOrDefaultAsync(x => x.Id == riskId, cancellationToken);
        if (risk is null)
        {
            return NotFound<RiskDetailResponse>(ApiErrorCodes.RiskNotFound, "Risk not found.");
        }

        var validationError = await ValidateRiskRequestAsync(risk.ProjectId, risk.Code, request.Title, request.Description, request.OwnerUserId, request.Probability, request.Impact, cancellationToken, riskId);
        if (validationError is not null)
        {
            return validationError;
        }

        dbContext.Entry(risk).CurrentValues.SetValues(risk with
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Probability = request.Probability,
            Impact = request.Impact,
            OwnerUserId = request.OwnerUserId.Trim(),
            MitigationPlan = TrimOrNull(request.MitigationPlan),
            Cause = TrimOrNull(request.Cause),
            Effect = TrimOrNull(request.Effect),
            ContingencyPlan = TrimOrNull(request.ContingencyPlan),
            NextReviewAt = request.NextReviewAt,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("update", "risk", riskId, StatusCodes.Status200OK, null, cancellationToken);
        await AppendBusinessEventAsync("risk_updated", "risk", riskId, actorUserId, "Updated risk", null, null, cancellationToken);
        return await SuccessRiskAsync(riskId, cancellationToken);
    }

    public async Task<RiskCommandResult<RiskDetailResponse>> AssessRiskAsync(Guid riskId, RiskTransitionRequest request, string? actorUserId, CancellationToken cancellationToken) =>
        await TransitionRiskAsync(riskId, "draft", "assessed", request, actorUserId, cancellationToken);

    public async Task<RiskCommandResult<RiskDetailResponse>> MitigateRiskAsync(Guid riskId, RiskTransitionRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var risk = await dbContext.Set<RiskEntity>().SingleOrDefaultAsync(x => x.Id == riskId, cancellationToken);
        if (risk is null)
        {
            return NotFound<RiskDetailResponse>(ApiErrorCodes.RiskNotFound, "Risk not found.");
        }

        var mitigationPlan = TrimOrNull(request.MitigationPlan) ?? TrimOrNull(risk.MitigationPlan);
        if (string.IsNullOrWhiteSpace(risk.OwnerUserId))
        {
            return Validation<RiskDetailResponse>(ApiErrorCodes.RiskOwnerRequired, "Risk owner is required before mitigation.");
        }

        if (string.IsNullOrWhiteSpace(mitigationPlan))
        {
            return Validation<RiskDetailResponse>(ApiErrorCodes.RiskMitigationRequired, "Mitigation plan is required before marking a risk as mitigated.");
        }

        if (!string.Equals(risk.Status, "assessed", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<RiskDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Only assessed risks can be mitigated.");
        }

        dbContext.Entry(risk).CurrentValues.SetValues(risk with
        {
            Status = "mitigated",
            MitigationPlan = mitigationPlan,
            NextReviewAt = request.NextReviewAt ?? risk.NextReviewAt,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        dbContext.Add(new RiskReviewEntity
        {
            Id = Guid.NewGuid(),
            RiskId = risk.Id,
            ReviewedBy = actorUserId ?? risk.OwnerUserId,
            ReviewedAt = DateTimeOffset.UtcNow,
            Decision = "mitigated",
            Notes = TrimOrNull(request.Notes)
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("mitigate", "risk", riskId, StatusCodes.Status200OK, null, cancellationToken, TrimOrNull(request.Notes));
        await AppendBusinessEventAsync("risk_mitigated", "risk", riskId, actorUserId, "Mitigated risk", TrimOrNull(request.Notes), null, cancellationToken);
        return await SuccessRiskAsync(riskId, cancellationToken);
    }

    public async Task<RiskCommandResult<RiskDetailResponse>> CloseRiskAsync(Guid riskId, RiskTransitionRequest request, string? actorUserId, CancellationToken cancellationToken) =>
        await TransitionRiskAsync(riskId, "mitigated", "closed", request, actorUserId, cancellationToken);

    public async Task<RiskCommandResult<IssueDetailResponse>> CreateIssueAsync(CreateIssueRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var validationError = await ValidateIssueRequestAsync(request.ProjectId, request.Code, request.Title, request.Description, request.OwnerUserId, request.Severity, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        if (request.IsSensitive && string.IsNullOrWhiteSpace(request.SensitiveContext))
        {
            return Validation<IssueDetailResponse>(ApiErrorCodes.SensitiveContextRequired, "Sensitive issue context is required.");
        }

        var now = DateTimeOffset.UtcNow;
        var issue = new IssueEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Code = request.Code.Trim().ToUpperInvariant(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            OwnerUserId = request.OwnerUserId.Trim(),
            DueDate = request.DueDate,
            Status = "open",
            Severity = request.Severity.Trim().ToLowerInvariant(),
            RootIssue = TrimOrNull(request.RootIssue),
            Dependencies = TrimOrNull(request.Dependencies),
            IsSensitive = request.IsSensitive,
            SensitiveContext = request.IsSensitive ? request.SensitiveContext?.Trim() : null,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Add(issue);
        await dbContext.SaveChangesAsync(cancellationToken);

        await AppendAuditAsync("create", "issue", issue.Id, StatusCodes.Status201Created, new { issue.Code, issue.Status, issue.IsSensitive }, cancellationToken);
        await AppendBusinessEventAsync("issue_created", "issue", issue.Id, actorUserId, "Created issue", null, new { issue.Code, issue.Title }, cancellationToken);
        return await SuccessIssueAsync(issue.Id, canReadSensitive: true, cancellationToken);
    }

    public async Task<RiskCommandResult<IssueDetailResponse>> UpdateIssueAsync(Guid issueId, UpdateIssueRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var issue = await dbContext.Set<IssueEntity>().SingleOrDefaultAsync(x => x.Id == issueId, cancellationToken);
        if (issue is null)
        {
            return NotFound<IssueDetailResponse>(ApiErrorCodes.IssueNotFound, "Issue not found.");
        }

        var validationError = await ValidateIssueRequestAsync(issue.ProjectId, issue.Code, request.Title, request.Description, request.OwnerUserId, request.Severity, cancellationToken, issueId);
        if (validationError is not null)
        {
            return validationError;
        }

        if (request.IsSensitive && string.IsNullOrWhiteSpace(request.SensitiveContext))
        {
            return Validation<IssueDetailResponse>(ApiErrorCodes.SensitiveContextRequired, "Sensitive issue context is required.");
        }

        var nextStatus = string.IsNullOrWhiteSpace(request.Status) ? issue.Status : request.Status.Trim().ToLowerInvariant();
        if (!IssueStatuses.Contains(nextStatus, StringComparer.OrdinalIgnoreCase))
        {
            return Validation<IssueDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Issue status is invalid.");
        }

        if (string.Equals(issue.Status, "resolved", StringComparison.OrdinalIgnoreCase) && string.Equals(nextStatus, "in_progress", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<IssueDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Resolved issues cannot return to in progress.");
        }

        dbContext.Entry(issue).CurrentValues.SetValues(issue with
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            OwnerUserId = request.OwnerUserId.Trim(),
            DueDate = request.DueDate,
            Severity = request.Severity.Trim().ToLowerInvariant(),
            RootIssue = TrimOrNull(request.RootIssue),
            Dependencies = TrimOrNull(request.Dependencies),
            ResolutionSummary = TrimOrNull(request.ResolutionSummary),
            IsSensitive = request.IsSensitive,
            SensitiveContext = request.IsSensitive ? request.SensitiveContext?.Trim() : null,
            Status = nextStatus,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("update", "issue", issueId, StatusCodes.Status200OK, new { Status = nextStatus, issue.IsSensitive }, cancellationToken);
        await AppendBusinessEventAsync("issue_updated", "issue", issueId, actorUserId, "Updated issue", null, new { Status = nextStatus }, cancellationToken);
        return await SuccessIssueAsync(issueId, canReadSensitive: true, cancellationToken);
    }

    public async Task<RiskCommandResult<IssueDetailResponse>> CreateIssueActionAsync(Guid issueId, CreateIssueActionRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ActionDescription) || string.IsNullOrWhiteSpace(request.AssignedTo))
        {
            return Validation<IssueDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Action description and assignee are required.");
        }

        var issue = await dbContext.Set<IssueEntity>().SingleOrDefaultAsync(x => x.Id == issueId, cancellationToken);
        if (issue is null)
        {
            return NotFound<IssueDetailResponse>(ApiErrorCodes.IssueNotFound, "Issue not found.");
        }

        dbContext.Add(new IssueActionEntity
        {
            Id = Guid.NewGuid(),
            IssueId = issueId,
            ActionDescription = request.ActionDescription.Trim(),
            AssignedTo = request.AssignedTo.Trim(),
            DueDate = request.DueDate,
            Status = "open",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        dbContext.Entry(issue).CurrentValues.SetValues(issue with
        {
            Status = issue.Status == "open" ? "in_progress" : issue.Status,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("create_action", "issue", issueId, StatusCodes.Status200OK, null, cancellationToken);
        await AppendBusinessEventAsync("issue_action_created", "issue", issueId, actorUserId, "Created issue action", null, null, cancellationToken);
        return await SuccessIssueAsync(issueId, canReadSensitive: true, cancellationToken);
    }

    public async Task<RiskCommandResult<IssueDetailResponse>> UpdateIssueActionAsync(Guid issueId, Guid actionId, UpdateIssueActionRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ActionDescription) || string.IsNullOrWhiteSpace(request.AssignedTo))
        {
            return Validation<IssueDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Action description and assignee are required.");
        }

        var issue = await dbContext.Set<IssueEntity>().SingleOrDefaultAsync(x => x.Id == issueId, cancellationToken);
        if (issue is null)
        {
            return NotFound<IssueDetailResponse>(ApiErrorCodes.IssueNotFound, "Issue not found.");
        }

        var action = await dbContext.Set<IssueActionEntity>().SingleOrDefaultAsync(x => x.Id == actionId && x.IssueId == issueId, cancellationToken);
        if (action is null)
        {
            return NotFound<IssueDetailResponse>(ApiErrorCodes.IssueActionNotFound, "Issue action not found.");
        }

        var status = request.Status.Trim().ToLowerInvariant();
        if (!IssueActionStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
        {
            return Validation<IssueDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Issue action status is invalid.");
        }

        dbContext.Entry(action).CurrentValues.SetValues(action with
        {
            ActionDescription = request.ActionDescription.Trim(),
            AssignedTo = request.AssignedTo.Trim(),
            DueDate = request.DueDate,
            Status = status,
            VerificationNote = TrimOrNull(request.VerificationNote),
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("update_action", "issue", issueId, StatusCodes.Status200OK, new { ActionId = actionId, Status = status }, cancellationToken);
        await AppendBusinessEventAsync("issue_action_updated", "issue", issueId, actorUserId, "Updated issue action", null, new { ActionId = actionId, Status = status }, cancellationToken);
        return await SuccessIssueAsync(issueId, canReadSensitive: true, cancellationToken);
    }

    public async Task<RiskCommandResult<IssueDetailResponse>> ResolveIssueAsync(Guid issueId, IssueResolutionRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var issue = await dbContext.Set<IssueEntity>().SingleOrDefaultAsync(x => x.Id == issueId, cancellationToken);
        if (issue is null)
        {
            return NotFound<IssueDetailResponse>(ApiErrorCodes.IssueNotFound, "Issue not found.");
        }

        if (string.Equals(issue.Status, "closed", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<IssueDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Closed issues cannot be resolved again.");
        }

        var openActionsExist = await dbContext.Set<IssueActionEntity>().AsNoTracking()
            .AnyAsync(x => x.IssueId == issueId && x.Status != "completed" && x.Status != "verified", cancellationToken);

        if (openActionsExist)
        {
            return Validation<IssueDetailResponse>(ApiErrorCodes.IssueOpenActionsExist, "Issue cannot be resolved while open actions exist.");
        }

        dbContext.Entry(issue).CurrentValues.SetValues(issue with
        {
            Status = "resolved",
            ResolutionSummary = TrimOrNull(request.ResolutionSummary) ?? issue.ResolutionSummary,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("resolve", "issue", issueId, StatusCodes.Status200OK, null, cancellationToken, TrimOrNull(request.ResolutionSummary));
        await AppendBusinessEventAsync("issue_resolved", "issue", issueId, actorUserId, "Resolved issue", TrimOrNull(request.ResolutionSummary), null, cancellationToken);
        return await SuccessIssueAsync(issueId, canReadSensitive: true, cancellationToken);
    }

    public async Task<RiskCommandResult<IssueDetailResponse>> CloseIssueAsync(Guid issueId, IssueResolutionRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var issue = await dbContext.Set<IssueEntity>().SingleOrDefaultAsync(x => x.Id == issueId, cancellationToken);
        if (issue is null)
        {
            return NotFound<IssueDetailResponse>(ApiErrorCodes.IssueNotFound, "Issue not found.");
        }

        if (!string.Equals(issue.Status, "resolved", StringComparison.OrdinalIgnoreCase))
        {
            return Validation<IssueDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, "Only resolved issues can be closed.");
        }

        dbContext.Entry(issue).CurrentValues.SetValues(issue with
        {
            Status = "closed",
            ResolutionSummary = TrimOrNull(request.ResolutionSummary) ?? issue.ResolutionSummary,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync("close", "issue", issueId, StatusCodes.Status200OK, null, cancellationToken, TrimOrNull(request.ResolutionSummary));
        await AppendBusinessEventAsync("issue_closed", "issue", issueId, actorUserId, "Closed issue", TrimOrNull(request.ResolutionSummary), null, cancellationToken);
        return await SuccessIssueAsync(issueId, canReadSensitive: true, cancellationToken);
    }

    private async Task<RiskCommandResult<RiskDetailResponse>> TransitionRiskAsync(Guid riskId, string fromStatus, string toStatus, RiskTransitionRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var risk = await dbContext.Set<RiskEntity>().SingleOrDefaultAsync(x => x.Id == riskId, cancellationToken);
        if (risk is null)
        {
            return NotFound<RiskDetailResponse>(ApiErrorCodes.RiskNotFound, "Risk not found.");
        }

        if (!string.Equals(risk.Status, fromStatus, StringComparison.OrdinalIgnoreCase))
        {
            return Validation<RiskDetailResponse>(ApiErrorCodes.InvalidWorkflowTransition, $"Only {fromStatus} risks can transition to {toStatus}.");
        }

        dbContext.Entry(risk).CurrentValues.SetValues(risk with
        {
            Status = toStatus,
            NextReviewAt = request.NextReviewAt ?? risk.NextReviewAt,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        dbContext.Add(new RiskReviewEntity
        {
            Id = Guid.NewGuid(),
            RiskId = risk.Id,
            ReviewedBy = actorUserId ?? risk.OwnerUserId,
            ReviewedAt = DateTimeOffset.UtcNow,
            Decision = toStatus,
            Notes = TrimOrNull(request.Notes)
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await AppendAuditAsync(toStatus, "risk", riskId, StatusCodes.Status200OK, null, cancellationToken, TrimOrNull(request.Notes));
        await AppendBusinessEventAsync($"risk_{toStatus}", "risk", riskId, actorUserId, $"{char.ToUpperInvariant(toStatus[0])}{toStatus[1..]} risk", TrimOrNull(request.Notes), null, cancellationToken);
        return await SuccessRiskAsync(riskId, cancellationToken);
    }

    private async Task<RiskCommandResult<RiskDetailResponse>?> ValidateRiskRequestAsync(Guid projectId, string code, string title, string description, string ownerUserId, int probability, int impact, CancellationToken cancellationToken, Guid? existingRiskId = null)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
        {
            return Validation<RiskDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Project, code, title, and description are required.");
        }

        if (string.IsNullOrWhiteSpace(ownerUserId))
        {
            return Validation<RiskDetailResponse>(ApiErrorCodes.RiskOwnerRequired, "Risk owner is required.");
        }

        if (probability is < 1 or > 5 || impact is < 1 or > 5)
        {
            return Validation<RiskDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Probability and impact must be between 1 and 5.");
        }

        var projectExists = await dbContext.Projects.AsNoTracking().AnyAsync(x => x.Id == projectId, cancellationToken);
        if (!projectExists)
        {
            return NotFound<RiskDetailResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        var normalizedCode = code.Trim().ToUpperInvariant();
        var exists = await dbContext.Set<RiskEntity>().AsNoTracking()
            .AnyAsync(x => x.ProjectId == projectId && x.Code == normalizedCode && (!existingRiskId.HasValue || x.Id != existingRiskId.Value), cancellationToken);
        if (exists)
        {
            return Conflict<RiskDetailResponse>(ApiErrorCodes.RiskCodeDuplicate, "Risk code already exists in the project.");
        }

        return null;
    }

    private async Task<RiskCommandResult<IssueDetailResponse>?> ValidateIssueRequestAsync(Guid projectId, string code, string title, string description, string ownerUserId, string severity, CancellationToken cancellationToken, Guid? existingIssueId = null)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
        {
            return Validation<IssueDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Project, code, title, and description are required.");
        }

        if (string.IsNullOrWhiteSpace(ownerUserId))
        {
            return Validation<IssueDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Issue owner is required.");
        }

        var normalizedSeverity = severity.Trim().ToLowerInvariant();
        if (!IssueSeverities.Contains(normalizedSeverity, StringComparer.OrdinalIgnoreCase))
        {
            return Validation<IssueDetailResponse>(ApiErrorCodes.RequestValidationFailed, "Issue severity is invalid.");
        }

        var projectExists = await dbContext.Projects.AsNoTracking().AnyAsync(x => x.Id == projectId, cancellationToken);
        if (!projectExists)
        {
            return NotFound<IssueDetailResponse>(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        var normalizedCode = code.Trim().ToUpperInvariant();
        var exists = await dbContext.Set<IssueEntity>().AsNoTracking()
            .AnyAsync(x => x.ProjectId == projectId && x.Code == normalizedCode && (!existingIssueId.HasValue || x.Id != existingIssueId.Value), cancellationToken);
        if (exists)
        {
            return Conflict<IssueDetailResponse>(ApiErrorCodes.IssueCodeDuplicate, "Issue code already exists in the project.");
        }

        return null;
    }

    private async Task<RiskCommandResult<RiskDetailResponse>> SuccessRiskAsync(Guid riskId, CancellationToken cancellationToken)
    {
        var value = await queries.GetRiskAsync(riskId, cancellationToken);
        return new RiskCommandResult<RiskDetailResponse>(RiskCommandStatus.Success, value);
    }

    private async Task<RiskCommandResult<IssueDetailResponse>> SuccessIssueAsync(Guid issueId, bool canReadSensitive, CancellationToken cancellationToken)
    {
        var value = await queries.GetIssueAsync(issueId, canReadSensitive, cancellationToken);
        return new RiskCommandResult<IssueDetailResponse>(RiskCommandStatus.Success, value);
    }

    private async Task AppendAuditAsync(string action, string entityType, Guid entityId, int statusCode, object? metadata, CancellationToken cancellationToken, string? reason = null)
    {
        auditLogWriter.Append(new AuditLogEntry(
            Module: "risks",
            Action: action,
            EntityType: entityType,
            EntityId: entityId.ToString(),
            StatusCode: statusCode,
            Reason: reason,
            Metadata: metadata,
            IsSensitive: metadata is { }));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task AppendBusinessEventAsync(string eventType, string entityType, Guid entityId, string? actorUserId, string summary, string? reason, object? metadata, CancellationToken cancellationToken)
    {
        try
        {
            await businessAuditEventWriter.AppendAsync("risks", eventType, entityType, entityId.ToString(), summary, reason, new { ActorUserId = actorUserId, Metadata = metadata }, cancellationToken);
        }
        catch
        {
            // Best-effort business audit.
        }
    }

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static RiskCommandResult<T> Validation<T>(string errorCode, string message) =>
        new(RiskCommandStatus.ValidationError, default, message, errorCode);

    private static RiskCommandResult<T> NotFound<T>(string errorCode, string message) =>
        new(RiskCommandStatus.NotFound, default, message, errorCode);

    private static RiskCommandResult<T> Conflict<T>(string errorCode, string message) =>
        new(RiskCommandStatus.Conflict, default, message, errorCode);
}
