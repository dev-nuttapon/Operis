using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Shared.Auditing;

namespace Operis_API.Modules.Users.Application;

public sealed class UserPreferenceCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter) : IUserPreferenceCommands
{
    public async Task<UserPreferenceCommandResult> UpdateCurrentUserPreferencesAsync(string currentUserId, UpdateUserPreferencesRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == currentUserId, cancellationToken);
        if (user is null)
        {
            return new UserPreferenceCommandResult(UserPreferenceCommandStatus.NotFound);
        }

        var before = new
        {
            user.Id,
            user.PreferredLanguage,
            user.PreferredTheme
        };
        user.PreferredLanguage = NormalizeLanguage(request.PreferredLanguage);
        user.PreferredTheme = NormalizeTheme(request.PreferredTheme);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "users",
            Action: "update_preferences",
            EntityType: "user",
            EntityId: user.Id,
            StatusCode: StatusCodes.Status204NoContent,
            DepartmentId: user.DepartmentId,
            Before: before,
            After: new
            {
                user.Id,
                user.PreferredLanguage,
                user.PreferredTheme
            },
            Changes: new
            {
                user.PreferredLanguage,
                user.PreferredTheme
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new UserPreferenceCommandResult(UserPreferenceCommandStatus.Success);
    }

    private static string? NormalizeLanguage(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().ToLowerInvariant();
        return normalized.Length > 16 ? normalized[..16] : normalized;
    }

    private static string? NormalizeTheme(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().ToLowerInvariant();
        return normalized is "light" or "dark" or "system" ? normalized : null;
    }
}
