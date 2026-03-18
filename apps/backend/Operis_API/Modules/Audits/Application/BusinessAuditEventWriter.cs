using System.Security.Claims;
using System.Text.Json;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Audits.Infrastructure;

namespace Operis_API.Modules.Audits.Application;

public sealed class BusinessAuditEventWriter(OperisDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    : IBusinessAuditEventWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task AppendAsync(
        string module,
        string eventType,
        string entityType,
        string? entityId,
        string? summary,
        string? reason,
        object? metadata,
        CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var user = httpContext?.User;

        var actorUserId = user?.FindFirstValue("sub") ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);
        var actorEmail = user?.FindFirstValue(ClaimTypes.Email) ?? user?.FindFirstValue("preferred_username");
        var actorDisplayName = user?.FindFirstValue("name") ?? actorEmail ?? actorUserId;

        var entry = new BusinessAuditEventEntity
        {
            Id = Guid.NewGuid(),
            Module = TrimToMax(module, 64) ?? string.Empty,
            EventType = TrimToMax(eventType, 64) ?? string.Empty,
            EntityType = TrimToMax(entityType, 64) ?? string.Empty,
            EntityId = TrimToMax(entityId, 64),
            Summary = TrimToMax(summary, 512),
            Reason = TrimToMax(reason, 512),
            ActorUserId = TrimToMax(actorUserId, 64),
            ActorEmail = TrimToMax(actorEmail, 128),
            ActorDisplayName = TrimToMax(actorDisplayName, 128),
            MetadataJson = Serialize(metadata),
            OccurredAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.BusinessAuditEvents.Add(entry);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string? Serialize(object? value) =>
        value is null ? null : JsonSerializer.Serialize(value, SerializerOptions);

    private static string? TrimToMax(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Length <= max ? value : value[..max];
    }
}
