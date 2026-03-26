namespace Operis_API.Modules.Users.Infrastructure;

public sealed class SystemSettingEntity
{
    public string Key { get; init; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
