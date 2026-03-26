using Operis_API.Modules.Audits.Application;

namespace Operis_API.Tests.Support;

internal sealed class FakeBusinessAuditEventWriter : IBusinessAuditEventWriter
{
    public List<(string Module, string EventType, string EntityType, string? EntityId, string? Summary, string? Reason)> Entries { get; } = [];

    public Task AppendAsync(string module, string eventType, string entityType, string? entityId, string? summary, string? reason, object? metadata, CancellationToken cancellationToken)
    {
        Entries.Add((module, eventType, entityType, entityId, summary, reason));
        return Task.CompletedTask;
    }
}
