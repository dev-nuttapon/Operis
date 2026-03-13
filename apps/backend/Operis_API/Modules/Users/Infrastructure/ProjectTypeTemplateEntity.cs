namespace Operis_API.Modules.Users.Infrastructure;

public sealed class ProjectTypeTemplateEntity
{
    public Guid Id { get; init; }
    public string ProjectType { get; set; } = string.Empty;
    public bool RequireSponsor { get; set; }
    public bool RequirePlannedPeriod { get; set; }
    public bool RequireActiveTeam { get; set; }
    public bool RequirePrimaryAssignment { get; set; }
    public bool RequireReportingRoot { get; set; }
    public bool RequireDocumentCreator { get; set; }
    public bool RequireReviewer { get; set; }
    public bool RequireApprover { get; set; }
    public bool RequireReleaseRole { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? DeletedReason { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
