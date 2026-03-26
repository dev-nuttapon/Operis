namespace Operis_API.Modules.Users.Infrastructure;

public sealed class PermissionMatrixEntryEntity
{
    public Guid Id { get; init; }
    public string RoleKeycloakName { get; set; } = string.Empty;
    public string PermissionKey { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
    public DateTimeOffset AppliedAt { get; set; }
    public string AppliedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
