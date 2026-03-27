namespace Operis_API.Modules.Knowledge.Infrastructure;

public sealed record LessonLearnedEntity
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string LessonType { get; init; } = string.Empty;
    public string OwnerUserId { get; init; } = string.Empty;
    public string Status { get; init; } = "draft";
    public string? SourceRef { get; init; }
    public string? Context { get; init; }
    public string? WhatHappened { get; init; }
    public string? WhatToRepeat { get; init; }
    public string? WhatToAvoid { get; init; }
    public string? LinkedEvidenceJson { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
