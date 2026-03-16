using System.IO;
using System.Security.Claims;
using System.Text.Json;
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

        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            try
            {
                await next(context);
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

            if (context.Response.StatusCode >= StatusCodes.Status400BadRequest)
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(responseBody, leaveOpen: true);
                var bodyText = await reader.ReadToEndAsync();

                string? errorCode = null;
                string? errorMessage = null;

                if (context.Response.ContentType?.Contains("json") == true && !string.IsNullOrWhiteSpace(bodyText))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(bodyText);
                        if (doc.RootElement.TryGetProperty("code", out var code)) errorCode = code.GetString();
                        if (doc.RootElement.TryGetProperty("detail", out var detail)) errorMessage = detail.GetString();
                        if (doc.RootElement.TryGetProperty("message", out var msg)) errorMessage ??= msg.GetString();
                    }
                    catch { /* Not a valid JSON or different schema */ }
                }

                auditLogWriter.Append(BuildEntry(
                    context, 
                    context.Response.StatusCode, 
                    errorCode, 
                    errorMessage ?? (string.IsNullOrWhiteSpace(bodyText) ? null : bodyText)));
                
                await dbContext.SaveChangesAsync(context.RequestAborted);
            }

            // Copy the content back to the original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
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
