using Microsoft.AspNetCore.Mvc;

namespace Operis_API.Modules.Learning.Contracts;

public sealed record TrainingCourseListQuery(
    [FromQuery] string? Search,
    [FromQuery] string? Status,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 25);

public sealed record RoleTrainingMatrixQuery(
    [FromQuery] Guid? ProjectId,
    [FromQuery] Guid? ProjectRoleId,
    [FromQuery] Guid? CourseId,
    [FromQuery] string? Status,
    [FromQuery] string? Search,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 25);

public sealed record TrainingCompletionListQuery(
    [FromQuery] Guid? ProjectId,
    [FromQuery] Guid? ProjectRoleId,
    [FromQuery] Guid? CourseId,
    [FromQuery] string? UserId,
    [FromQuery] string? Status,
    [FromQuery] bool OnlyOverdue = false,
    [FromQuery] string? Search = null,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 25);

public sealed record CompetencyReviewListQuery(
    [FromQuery] Guid? ProjectId,
    [FromQuery] string? UserId,
    [FromQuery] string? Status,
    [FromQuery] string? Search,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 25);

public sealed record ProjectRoleOptionResponse(
    Guid Id,
    Guid? ProjectId,
    string? ProjectName,
    string Name,
    string Status);

public sealed record TrainingCourseResponse(
    Guid Id,
    string? CourseCode,
    string Title,
    string? Description,
    string? Provider,
    string? DeliveryMode,
    string? AudienceScope,
    int ValidityMonths,
    string Status,
    int RequirementCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record RoleTrainingRequirementResponse(
    Guid Id,
    Guid CourseId,
    string CourseTitle,
    string? CourseCode,
    string CourseStatus,
    Guid ProjectRoleId,
    string ProjectRoleName,
    Guid? ProjectId,
    string? ProjectName,
    int RequiredWithinDays,
    int RenewalIntervalMonths,
    string Status,
    string? Notes,
    int AssignedUserCount,
    int OverdueUserCount,
    int ExpiredUserCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record TrainingCompletionResponse(
    Guid Id,
    Guid CourseId,
    string CourseTitle,
    string? CourseCode,
    Guid ProjectRoleId,
    string ProjectRoleName,
    Guid ProjectId,
    string ProjectName,
    string UserId,
    string Status,
    bool IsOverdue,
    bool IsExpired,
    DateTimeOffset AssignedAt,
    DateTimeOffset? DueAt,
    DateTimeOffset? CompletionDate,
    DateTimeOffset? ExpiryDate,
    string? EvidenceRef,
    string? Notes,
    DateTimeOffset UpdatedAt);

public sealed record CompetencyReviewResponse(
    Guid Id,
    string UserId,
    Guid? ProjectId,
    string? ProjectName,
    string ReviewPeriod,
    string ReviewerUserId,
    string Status,
    string? Summary,
    DateTimeOffset PlannedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateTrainingCourseRequest(
    string? CourseCode,
    string Title,
    string? Description,
    string? Provider,
    string? DeliveryMode,
    string? AudienceScope,
    int ValidityMonths);

public sealed record UpdateTrainingCourseRequest(
    string? CourseCode,
    string Title,
    string? Description,
    string? Provider,
    string? DeliveryMode,
    string? AudienceScope,
    int ValidityMonths);

public sealed record TransitionTrainingCourseRequest(
    string TargetStatus,
    string? Reason);

public sealed record CreateRoleTrainingRequirementRequest(
    Guid CourseId,
    Guid ProjectRoleId,
    int RequiredWithinDays,
    int RenewalIntervalMonths,
    string? Notes);

public sealed record UpdateRoleTrainingRequirementRequest(
    Guid CourseId,
    Guid ProjectRoleId,
    int RequiredWithinDays,
    int RenewalIntervalMonths,
    string Status,
    string? Notes);

public sealed record RecordTrainingCompletionRequest(
    Guid CourseId,
    Guid ProjectRoleId,
    Guid ProjectId,
    string UserId,
    string Status,
    DateTimeOffset? AssignedAt,
    DateTimeOffset? DueAt,
    DateTimeOffset? CompletionDate,
    string? EvidenceRef,
    string? Notes);

public sealed record UpdateTrainingCompletionRequest(
    string Status,
    DateTimeOffset? DueAt,
    DateTimeOffset? CompletionDate,
    string? EvidenceRef,
    string? Notes);

public sealed record CreateCompetencyReviewRequest(
    string UserId,
    Guid? ProjectId,
    string ReviewPeriod,
    string ReviewerUserId,
    DateTimeOffset PlannedAt,
    string? Summary);

public sealed record UpdateCompetencyReviewRequest(
    string ReviewPeriod,
    string ReviewerUserId,
    string Status,
    DateTimeOffset PlannedAt,
    DateTimeOffset? CompletedAt,
    string? Summary);
