using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;

namespace Operis_API.Shared.Auditing;

public sealed class AuditFailureLoggingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, OperisDbContext dbContext, IAuditLogWriter auditLogWriter)
    {
        if (!ShouldAudit(context.Request.Path))
        {
            await next(context);
            return;
        }

        try
        {
            await next(context);

            if (context.Response.StatusCode >= StatusCodes.Status400BadRequest)
            {
                auditLogWriter.Append(BuildEntry(context, context.Response.StatusCode));
                await dbContext.SaveChangesAsync(context.RequestAborted);
            }
        }
        catch (Exception ex)
        {
            auditLogWriter.Append(BuildEntry(
                context,
                StatusCodes.Status500InternalServerError,
                "unhandled_exception",
                ex.Message));
            await dbContext.SaveChangesAsync(context.RequestAborted);
            throw;
        }
    }

    private static AuditLogEntry BuildEntry(HttpContext context, int statusCode, string? errorCode = null, string? errorMessage = null)
    {
        var (module, entityType) = ResolveModuleAndEntity(context.Request.Path);
        var actorUserId = context.User.FindFirstValue("sub") ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var actorEmail = context.User.FindFirstValue(ClaimTypes.Email) ?? context.User.FindFirstValue("preferred_username");
        var actorDisplayName = context.User.FindFirstValue("name") ?? actorEmail ?? actorUserId;

        return new AuditLogEntry(
            Module: module,
            Action: context.Request.Method.ToLowerInvariant(),
            EntityType: entityType,
            Status: statusCode is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden ? "denied" : "failed",
            StatusCode: statusCode,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage,
            ActorType: context.User.Identity?.IsAuthenticated == true ? "user" : "anonymous",
            ActorUserId: actorUserId,
            ActorEmail: actorEmail,
            ActorDisplayName: actorDisplayName,
            Metadata: new
            {
                query = context.Request.QueryString.Value
            });
    }

    private static bool ShouldAudit(PathString path)
    {
        return path.HasValue
               && path.Value!.StartsWith("/api/v1/", StringComparison.OrdinalIgnoreCase)
               && !path.Value.StartsWith("/api/v1/health", StringComparison.OrdinalIgnoreCase);
    }

    private static (string module, string entityType) ResolveModuleAndEntity(PathString path)
    {
        var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
        if (segments.Length < 3)
        {
            return ("platform", "request");
        }

        return segments[2] switch
        {
            "users" when segments.Length >= 4 && segments[3] == "departments" => ("users", "department"),
            "users" when segments.Length >= 4 && segments[3] == "job-titles" => ("users", "job_title"),
            "users" when segments.Length >= 4 && segments[3] == "invitations" => ("users", "invitation"),
            "users" when segments.Length >= 4 && segments[3] == "registration-requests" => ("users", "registration_request"),
            "users" => ("users", "user"),
            "documents" => ("documents", "document"),
            "activity-logs" => ("activities", "activity_log"),
            "audit-logs" => ("audits", "audit_log"),
            _ => (segments[2], "request")
        };
    }
}
