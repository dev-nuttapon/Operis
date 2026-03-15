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
    private static readonly HashSet<string> ActivityOnlyEventKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "activities:list:activity_log",
        "documents:list:document",
        "workflows:list:workflow_definition",
        "users:list:user",
        "users:list:invitation",
        "users:list:registration_request",
        "users:list:division",
        "users:list:department",
        "users:list:job_title",
        "users:list:app_role",
        "users:list:project",
        "users:list:project_role",
        "users:list:project_assignment",
        "users:list:project_type_template",
        "users:list:project_type_role_requirement",
        "users:list:project_org_chart",
        "users:list:project_evidence",
        "users:list:project_compliance"
    };

    private static readonly HashSet<string> AuditOnlyEventKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "audits:list:audit_log",
        "users:create:user",
        "users:update:user",
        "users:soft_delete:user",
        "users:create:division",
        "users:update:division",
        "users:soft_delete:division",
        "users:create:department",
        "users:update:department",
        "users:soft_delete:department",
        "users:create:job_title",
        "users:update:job_title",
        "users:soft_delete:job_title",
        "users:create:project",
        "users:update:project",
        "users:soft_delete:project",
        "users:create:project_role",
        "users:update:project_role",
        "users:soft_delete:project_role",
        "users:create:project_assignment",
        "users:update:project_assignment",
        "users:delete:project_assignment",
        "users:create:project_type_template",
        "users:update:project_type_template",
        "users:soft_delete:project_type_template",
        "users:create:project_type_role_requirement",
        "users:update:project_type_role_requirement",
        "users:soft_delete:project_type_role_requirement",
        "users:invite:invitation",
        "users:accept:invitation",
        "users:approve:registration_request",
        "users:reject:registration_request",
        "users:update:user_org_assignment",
        "workflows:create:workflow_definition",
        "workflows:update:workflow_definition",
        "workflows:activate:workflow_definition",
        "workflows:archive:workflow_definition"
    };
    private static readonly HashSet<string> AuditActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "create",
        "update",
        "delete",
        "soft_delete",
        "restore",
        "approve",
        "reject",
        "activate",
        "archive",
        "release",
        "invite",
        "accept",
        "revoke",
        "assign",
        "export",
        "export_evidence"
    };

    private static readonly HashSet<string> AuditEntityTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "user",
        "invitation",
        "registration_request",
        "division",
        "department",
        "job_title",
        "app_role",
        "project",
        "project_role",
        "project_assignment",
        "project_type_template",
        "project_type_role_requirement",
        "user_org_assignment",
        "reporting_line",
        "workflow_definition",
        "audit_log"
    };

    private static readonly HashSet<string> ReadOnlyActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "list",
        "read",
        "search",
        "get"
    };

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

        if (ShouldWriteActivity(entry))
        {
            dbContext.Set<ActivityLogEntity>().Add(activityEntity);
        }

        if (ShouldWriteAudit(entry))
        {
            dbContext.Set<AuditLogEntity>().Add(ToAuditEntity(activityEntity));
        }
    }

    private static string? Serialize(object? value)
    {
        return value is null ? null : JsonSerializer.Serialize(value, SerializerOptions);
    }

    private static AuditLogEntity ToAuditEntity(ActivityLogEntity activityEntity) =>
        new()
        {
            Id = Guid.NewGuid(),
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

    private static bool ShouldWriteActivity(AuditLogEntry entry) =>
        entry.Audience switch
        {
            LogAudience.ActivityOnly => true,
            LogAudience.Both => true,
            LogAudience.AuditOnly => false,
            _ => !ShouldForceAuditOnly(entry)
        };

    private static bool ShouldWriteAudit(AuditLogEntry entry)
    {
        return entry.Audience switch
        {
            LogAudience.AuditOnly => true,
            LogAudience.Both => true,
            LogAudience.ActivityOnly => false,
            _ => ShouldForceAuditOnly(entry) || (!ShouldForceActivityOnly(entry) && IsAuditWorthy(entry))
        };
    }

    private static bool ShouldForceActivityOnly(AuditLogEntry entry) =>
        ActivityOnlyEventKeys.Contains(BuildEventKey(entry));

    private static bool ShouldForceAuditOnly(AuditLogEntry entry) =>
        AuditOnlyEventKeys.Contains(BuildEventKey(entry));

    private static string BuildEventKey(AuditLogEntry entry) =>
        $"{entry.Module}:{entry.Action}:{entry.EntityType}";

    private static bool IsAuditWorthy(AuditLogEntry entry)
    {
        if (string.Equals(entry.EntityType, "activity_log", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (entry.Before is not null || entry.After is not null || entry.Changes is not null)
        {
            return true;
        }

        if (entry.IsSensitive || !string.IsNullOrWhiteSpace(entry.RetentionClass))
        {
            return true;
        }

        if (AuditActions.Contains(entry.Action))
        {
            return true;
        }

        if (AuditEntityTypes.Contains(entry.EntityType) && !ReadOnlyActions.Contains(entry.Action))
        {
            return true;
        }

        if (string.Equals(entry.Status, "failed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(entry.Status, "denied", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}
