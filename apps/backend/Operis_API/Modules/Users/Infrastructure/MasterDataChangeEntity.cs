namespace Operis_API.Modules.Users.Infrastructure;

public sealed class MasterDataChangeEntity
{
    public Guid Id { get; init; }
    public Guid MasterDataItemId { get; set; }
    public string ChangeType { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public DateTimeOffset ChangedAt { get; set; }
    public string Reason { get; set; } = string.Empty;
}
