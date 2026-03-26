namespace Operis_API.Modules.Governance.Infrastructure;

public sealed class StakeholderEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string InfluenceLevel { get; set; } = string.Empty;
    public string ContactChannel { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}
