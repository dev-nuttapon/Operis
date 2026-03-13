namespace Operis_API.Modules.Users.Infrastructure;

public sealed class UserProjectAssignmentEntity
{
    public Guid Id { get; init; }
    public string UserId { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public Guid ProjectRoleId { get; set; }
    public string? ReportsToUserId { get; set; }
    public bool IsPrimary { get; set; }
    public string Status { get; set; } = "Active";
    public string? ChangeReason { get; set; }
    public Guid? ReplacedByAssignmentId { get; set; }
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset? EndAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
