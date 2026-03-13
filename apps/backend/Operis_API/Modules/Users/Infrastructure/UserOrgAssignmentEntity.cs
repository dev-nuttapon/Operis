namespace Operis_API.Modules.Users.Infrastructure;

public sealed class UserOrgAssignmentEntity
{
    public Guid Id { get; init; }
    public string UserId { get; set; } = string.Empty;
    public Guid? DivisionId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? PositionId { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsDivisionHead { get; set; }
    public bool IsDepartmentHead { get; set; }
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset? EndAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
