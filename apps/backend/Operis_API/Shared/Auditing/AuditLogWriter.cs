using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Operis_API.Infrastructure.Persistence;

namespace Operis_API.Shared.Auditing;

public sealed class AuditLogWriter(
    OperisDbContext dbContext,
    IHttpContextAccessor httpContextAccessor) : IAuditLogWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public void Append(AuditLogEntry entry)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var user = httpContext?.User;

        var actorUserId = entry.ActorUserId
            ?? user?.FindFirstValue("sub")
            ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);
        var actorEmail = entry.ActorEmail
            ?? user?.FindFirstValue(ClaimTypes.Email)
            ?? user?.FindFirstValue("preferred_username");
        var actorDisplayName = entry.ActorDisplayName
            ?? user?.FindFirstValue("name")
            ?? actorEmail
            ?? actorUserId;

        var entity = new AuditLogEntity
        {
            Id = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
            Module = entry.Module,
            Action = entry.Action,
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            ActorType = entry.ActorType,
            ActorUserId = actorUserId,
            ActorEmail = actorEmail,
            ActorDisplayName = actorDisplayName,
            DepartmentId = entry.DepartmentId,
            TenantId = entry.TenantId,
            RequestId = httpContext?.TraceIdentifier,
            TraceId = System.Diagnostics.Activity.Current?.TraceId.ToString(),
            CorrelationId = httpContext?.Request.Headers["X-Correlation-Id"].FirstOrDefault(),
            HttpMethod = httpContext?.Request.Method,
            RequestPath = httpContext?.Request.Path.Value,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
            Status = entry.Status,
            StatusCode = entry.StatusCode,
            ErrorCode = entry.ErrorCode,
            ErrorMessage = entry.ErrorMessage,
            Reason = entry.Reason,
            Source = entry.Source,
            BeforeJson = Serialize(entry.Before),
            AfterJson = Serialize(entry.After),
            ChangesJson = Serialize(entry.Changes),
            MetadataJson = Serialize(entry.Metadata),
            IsSensitive = entry.IsSensitive,
            RetentionClass = entry.RetentionClass,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Set<AuditLogEntity>().Add(entity);
    }

    private static string? Serialize(object? value)
    {
        return value is null ? null : JsonSerializer.Serialize(value, SerializerOptions);
    }
}
