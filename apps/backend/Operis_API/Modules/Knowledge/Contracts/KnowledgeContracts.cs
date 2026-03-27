using Microsoft.AspNetCore.Mvc;

namespace Operis_API.Modules.Knowledge.Contracts;

public sealed record LessonLearnedListQuery(
    [FromQuery] Guid? ProjectId,
    [FromQuery] string? LessonType,
    [FromQuery] string? OwnerUserId,
    [FromQuery] string? Status,
    [FromQuery] string? Search,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 25);

public sealed record LessonLearnedItem(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Title,
    string Summary,
    string LessonType,
    string OwnerUserId,
    string Status,
    string? SourceRef,
    string? Context,
    string? WhatHappened,
    string? WhatToRepeat,
    string? WhatToAvoid,
    IReadOnlyList<string> LinkedEvidence,
    DateTimeOffset? PublishedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateLessonLearnedRequest(
    Guid ProjectId,
    string Title,
    string Summary,
    string LessonType,
    string OwnerUserId,
    string? SourceRef,
    string? Context,
    string? WhatHappened,
    string? WhatToRepeat,
    string? WhatToAvoid,
    IReadOnlyList<string>? LinkedEvidence);

public sealed record UpdateLessonLearnedRequest(
    string Title,
    string Summary,
    string LessonType,
    string OwnerUserId,
    string Status,
    string? SourceRef,
    string? Context,
    string? WhatHappened,
    string? WhatToRepeat,
    string? WhatToAvoid,
    IReadOnlyList<string>? LinkedEvidence);

public sealed record PublishLessonLearnedRequest(
    string? SourceRef,
    string? Context,
    string? Summary,
    IReadOnlyList<string>? LinkedEvidence);
