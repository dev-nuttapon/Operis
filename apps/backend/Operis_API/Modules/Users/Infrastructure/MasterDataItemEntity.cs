namespace Operis_API.Modules.Users.Infrastructure;

public sealed class MasterDataItemEntity
{
    public Guid Id { get; init; }
    public string Domain { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public int DisplayOrder { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}
