using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Shared.ActivityLogging;

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

        var activityEntity = new ActivityLogEntity
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

        var auditEntity = new AuditLogEntity
        {
            Id = activityEntity.Id,
            OccurredAt = activityEntity.OccurredAt,
            Module = activityEntity.Module,
            Action = activityEntity.Action,
            EntityType = activityEntity.EntityType,
            EntityId = activityEntity.EntityId,
            ActorType = activityEntity.ActorType,
            ActorUserId = activityEntity.ActorUserId,
            ActorEmail = activityEntity.ActorEmail,
            ActorDisplayName = activityEntity.ActorDisplayName,
            DepartmentId = activityEntity.DepartmentId,
            TenantId = activityEntity.TenantId,
            RequestId = activityEntity.RequestId,
            TraceId = activityEntity.TraceId,
            CorrelationId = activityEntity.CorrelationId,
            HttpMethod = activityEntity.HttpMethod,
            RequestPath = activityEntity.RequestPath,
            IpAddress = activityEntity.IpAddress,
            UserAgent = activityEntity.UserAgent,
            Status = activityEntity.Status,
            StatusCode = activityEntity.StatusCode,
            ErrorCode = activityEntity.ErrorCode,
            ErrorMessage = activityEntity.ErrorMessage,
            Reason = activityEntity.Reason,
            Source = activityEntity.Source,
            BeforeJson = activityEntity.BeforeJson,
            AfterJson = activityEntity.AfterJson,
            ChangesJson = activityEntity.ChangesJson,
            MetadataJson = activityEntity.MetadataJson,
            IsSensitive = activityEntity.IsSensitive,
            RetentionClass = activityEntity.RetentionClass,
            CreatedAt = activityEntity.CreatedAt
        };

        dbContext.Set<ActivityLogEntity>().Add(activityEntity);
        dbContext.Set<AuditLogEntity>().Add(auditEntity);
    }

    private static string? Serialize(object? value)
    {
        return value is null ? null : JsonSerializer.Serialize(value, SerializerOptions);
    }
}
