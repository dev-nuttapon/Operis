namespace Operis_API.Shared.Auditing;

public interface IAuditLogWriter
{
    void Append(AuditLogEntry entry);
}
