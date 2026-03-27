using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Knowledge.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Knowledge.Application;

public sealed class KnowledgeQueries(OperisDbContext dbContext) : IKnowledgeQueries
{
    public async Task<PagedResult<LessonLearnedItem>> ListLessonsLearnedAsync(LessonLearnedListQuery query, CancellationToken cancellationToken)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 25 : Math.Min(query.PageSize, 100);
        var baseQuery =
            from lesson in dbContext.LessonsLearned.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on lesson.ProjectId equals project.Id
            select new { lesson, project.Name };

        if (query.ProjectId.HasValue) baseQuery = baseQuery.Where(x => x.lesson.ProjectId == query.ProjectId.Value);
        if (!string.IsNullOrWhiteSpace(query.LessonType)) baseQuery = baseQuery.Where(x => x.lesson.LessonType == query.LessonType.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.OwnerUserId)) baseQuery = baseQuery.Where(x => x.lesson.OwnerUserId == query.OwnerUserId.Trim());
        if (!string.IsNullOrWhiteSpace(query.Status)) baseQuery = baseQuery.Where(x => x.lesson.Status == query.Status.Trim().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = $"%{query.Search.Trim()}%";
            baseQuery = baseQuery.Where(x => EF.Functions.ILike(x.lesson.Title, search) || EF.Functions.ILike(x.Name, search));
        }

        var total = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderByDescending(x => x.lesson.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new LessonLearnedItem(
                x.lesson.Id,
                x.lesson.ProjectId,
                x.Name,
                x.lesson.Title,
                x.lesson.Summary,
                x.lesson.LessonType,
                x.lesson.OwnerUserId,
                x.lesson.Status,
                x.lesson.SourceRef,
                x.lesson.Context,
                x.lesson.WhatHappened,
                x.lesson.WhatToRepeat,
                x.lesson.WhatToAvoid,
                ParseEvidence(x.lesson.LinkedEvidenceJson),
                x.lesson.PublishedAt,
                x.lesson.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<LessonLearnedItem>(items, total, page, pageSize);
    }

    public async Task<LessonLearnedItem?> GetLessonLearnedAsync(Guid lessonId, CancellationToken cancellationToken)
    {
        return await (
            from lesson in dbContext.LessonsLearned.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on lesson.ProjectId equals project.Id
            where lesson.Id == lessonId
            select new LessonLearnedItem(
                lesson.Id,
                lesson.ProjectId,
                project.Name,
                lesson.Title,
                lesson.Summary,
                lesson.LessonType,
                lesson.OwnerUserId,
                lesson.Status,
                lesson.SourceRef,
                lesson.Context,
                lesson.WhatHappened,
                lesson.WhatToRepeat,
                lesson.WhatToAvoid,
                ParseEvidence(lesson.LinkedEvidenceJson),
                lesson.PublishedAt,
                lesson.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IReadOnlyList<string> ParseEvidence(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
