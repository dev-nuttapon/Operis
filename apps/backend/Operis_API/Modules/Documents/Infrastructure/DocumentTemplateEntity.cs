namespace Operis_API.Modules.Documents.Infrastructure;

public sealed record DocumentTemplateEntity
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? CreatedByUserId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public bool IsDeleted { get; init; }
    public string? DeletedByUserId { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
    public string? DeletedReason { get; init; }
}

public sealed record DocumentTemplateItemEntity
{
    public Guid Id { get; init; }
    public Guid TemplateId { get; init; }
    public Guid DocumentId { get; init; }
    public Guid? DocumentVersionId { get; init; }
    public int DisplayOrder { get; init; }
}
