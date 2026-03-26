using Operis_API.Modules.Audits.Application;

namespace Operis_API.Tests.Support;

internal sealed class FakeBusinessAuditEventWriter : IBusinessAuditEventWriter
{
    public Task AppendAsync(string module, string eventType, string entityType, string? entityId, string? summary, string? reason, object? metadata, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
