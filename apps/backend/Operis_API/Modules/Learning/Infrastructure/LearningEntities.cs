namespace Operis_API.Modules.Learning.Infrastructure;

public sealed class TrainingCourseEntity
{
    public Guid Id { get; init; }
    public string? CourseCode { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Provider { get; set; }
    public string? DeliveryMode { get; set; }
    public string? AudienceScope { get; set; }
    public int ValidityMonths { get; set; }
    public string Status { get; set; } = "draft";
    public DateTimeOffset? ActivatedAt { get; set; }
    public DateTimeOffset? RetiredAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class RoleTrainingRequirementEntity
{
    public Guid Id { get; init; }
    public Guid CourseId { get; set; }
    public Guid ProjectRoleId { get; set; }
    public int RequiredWithinDays { get; set; }
    public int RenewalIntervalMonths { get; set; }
    public string Status { get; set; } = "active";
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class TrainingCompletionEntity
{
    public Guid Id { get; init; }
    public Guid CourseId { get; set; }
    public Guid ProjectRoleId { get; set; }
    public Guid ProjectId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Status { get; set; } = "assigned";
    public DateTimeOffset AssignedAt { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public DateTimeOffset? CompletionDate { get; set; }
    public DateTimeOffset? ExpiryDate { get; set; }
    public string? EvidenceRef { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class CompetencyReviewEntity
{
    public Guid Id { get; init; }
    public string UserId { get; set; } = string.Empty;
    public Guid? ProjectId { get; set; }
    public string ReviewPeriod { get; set; } = string.Empty;
    public string ReviewerUserId { get; set; } = string.Empty;
    public string Status { get; set; } = "planned";
    public string? Summary { get; set; }
    public DateTimeOffset PlannedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}
