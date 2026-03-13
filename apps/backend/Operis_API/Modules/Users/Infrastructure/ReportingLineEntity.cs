namespace Operis_API.Modules.Users.Infrastructure;

public sealed class ReportingLineEntity
{
    public Guid Id { get; init; }
    public string UserId { get; set; } = string.Empty;
    public string ReportsToUserId { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public bool IsPrimary { get; set; }
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
