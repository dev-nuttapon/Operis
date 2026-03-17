using System.Security.Claims;
using System.Text.Json;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Documents.Infrastructure;

namespace Operis_API.Modules.Documents.Application;

public sealed class DocumentHistoryWriter(OperisDbContext dbContext, IHttpContextAccessor httpContextAccessor)
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task AppendAsync(
        Guid documentId,
        string eventType,
        object? before,
        object? after,
        string? summary,
        string? reason,
        string? status,
        int? statusCode,
        string? source,
        object? metadata,
        CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var user = httpContext?.User;

        var actorUserId = user?.FindFirstValue("sub") ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);
        var actorEmail = user?.FindFirstValue(ClaimTypes.Email) ?? user?.FindFirstValue("preferred_username");
        var actorDisplayName = user?.FindFirstValue("name") ?? actorEmail ?? actorUserId;

        var entry = new DocumentHistoryEntity
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            EventType = TrimToMax(eventType, 64) ?? string.Empty,
            Summary = TrimToMax(summary, 512),
            Reason = TrimToMax(reason, 512),
            ActorUserId = TrimToMax(actorUserId, 64),
            ActorEmail = TrimToMax(actorEmail, 128),
            ActorDisplayName = TrimToMax(actorDisplayName, 128),
            Status = TrimToMax(status, 24),
            StatusCode = statusCode,
            Source = TrimToMax(source, 64),
            BeforeJson = Serialize(before),
            AfterJson = Serialize(after),
            MetadataJson = Serialize(metadata),
            OccurredAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.DocumentHistories.Add(entry);
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
