using Operis_API.Shared.Auditing;

namespace Operis_API.Tests.Support;

internal sealed class FakeAuditLogWriter : IAuditLogWriter
{
    public List<AuditLogEntry> Entries { get; } = [];

    public void Append(AuditLogEntry entry)
    {
        Entries.Add(entry);
    }
}
