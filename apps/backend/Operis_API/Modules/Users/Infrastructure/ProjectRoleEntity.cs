namespace Operis_API.Modules.Users.Infrastructure;

public sealed class ProjectRoleEntity
{
    public Guid Id { get; init; }
    public Guid? ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Responsibilities { get; set; }
    public string? AuthorityScope { get; set; }
    public bool CanCreateDocuments { get; set; }
    public bool CanReviewDocuments { get; set; }
    public bool CanApproveDocuments { get; set; }
    public bool CanReleaseDocuments { get; set; }
    public bool IsReviewRole { get; set; }
    public bool IsApprovalRole { get; set; }
    public int DisplayOrder { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? DeletedReason { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
