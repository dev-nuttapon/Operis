namespace Operis_API.Modules.Audits.Application;

public interface IBusinessAuditEventWriter
{
    Task AppendAsync(
        string module,
        string eventType,
        string entityType,
        string? entityId,
        string? summary,
        string? reason,
        object? metadata,
        CancellationToken cancellationToken);
}
