using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Knowledge.Contracts;
using Operis_API.Modules.Knowledge.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Knowledge.Application;

public sealed class KnowledgeCommands(OperisDbContext dbContext, IAuditLogWriter auditLogWriter, IKnowledgeQueries queries) : IKnowledgeCommands
{
    private static readonly string[] LessonStatuses = ["draft", "reviewed", "published", "archived"];

    public async Task<KnowledgeCommandResult<LessonLearnedItem>> CreateLessonLearnedAsync(CreateLessonLearnedRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Projects.AnyAsync(x => x.Id == request.ProjectId, cancellationToken))
        {
            return NotFound(ApiErrorCodes.ProjectNotFound, "Project not found.");
        }

        var summary = NormalizeRequired(request.Summary, 4000);
        if (summary is null)
        {
            return Validation(ApiErrorCodes.LessonSummaryRequired, "Lesson summary is required.");
        }

        var entity = new LessonLearnedEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Title = NormalizeRequired(request.Title, 512) ?? "Untitled lesson",
            Summary = summary,
            LessonType = NormalizeKey(request.LessonType, 128) ?? "general",
            OwnerUserId = NormalizeRequired(request.OwnerUserId, 128) ?? string.Empty,
            Status = "draft",
            SourceRef = NormalizeOptional(request.SourceRef, 512),
            Context = NormalizeOptional(request.Context, 4000),
            WhatHappened = NormalizeOptional(request.WhatHappened, 4000),
            WhatToRepeat = NormalizeOptional(request.WhatToRepeat, 4000),
            WhatToAvoid = NormalizeOptional(request.WhatToAvoid, 4000),
            LinkedEvidenceJson = SerializeEvidence(request.LinkedEvidence),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.LessonsLearned.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("create", entity.Id, 201, new { entity.Title, entity.Status });
        return Success((await queries.GetLessonLearnedAsync(entity.Id, cancellationToken))!);
    }

    public async Task<KnowledgeCommandResult<LessonLearnedItem>> UpdateLessonLearnedAsync(Guid lessonId, UpdateLessonLearnedRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.LessonsLearned.SingleOrDefaultAsync(x => x.Id == lessonId, cancellationToken);
        if (entity is null) return NotFound(ApiErrorCodes.ResourceNotFound, "Lesson not found.");

        var nextStatus = NormalizeKey(request.Status, 32);
        if (string.IsNullOrWhiteSpace(nextStatus) || !LessonStatuses.Contains(nextStatus) || !IsValidTransition(entity.Status, nextStatus))
        {
            return Validation(ApiErrorCodes.InvalidWorkflowTransition, "Lesson transition is invalid.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Title = NormalizeRequired(request.Title, 512) ?? entity.Title,
            Summary = NormalizeRequired(request.Summary, 4000) ?? entity.Summary,
            LessonType = NormalizeKey(request.LessonType, 128) ?? entity.LessonType,
            OwnerUserId = NormalizeRequired(request.OwnerUserId, 128) ?? entity.OwnerUserId,
            Status = nextStatus,
            SourceRef = NormalizeOptional(request.SourceRef, 512),
            Context = NormalizeOptional(request.Context, 4000),
            WhatHappened = NormalizeOptional(request.WhatHappened, 4000),
            WhatToRepeat = NormalizeOptional(request.WhatToRepeat, 4000),
            WhatToAvoid = NormalizeOptional(request.WhatToAvoid, 4000),
            LinkedEvidenceJson = SerializeEvidence(request.LinkedEvidence),
            PublishedAt = nextStatus switch
            {
                "published" => entity.PublishedAt ?? DateTimeOffset.UtcNow,
                "archived" => entity.PublishedAt,
                _ => null
            },
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("update", entity.Id, 200, new { Status = nextStatus });
        return Success((await queries.GetLessonLearnedAsync(entity.Id, cancellationToken))!);
    }

    public async Task<KnowledgeCommandResult<LessonLearnedItem>> PublishLessonLearnedAsync(Guid lessonId, PublishLessonLearnedRequest request, string? actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.LessonsLearned.SingleOrDefaultAsync(x => x.Id == lessonId, cancellationToken);
        if (entity is null) return NotFound(ApiErrorCodes.ResourceNotFound, "Lesson not found.");
        if (!IsValidTransition(entity.Status, "published"))
        {
            return Validation(ApiErrorCodes.InvalidWorkflowTransition, "Lesson transition is invalid.");
        }

        var context = NormalizeOptional(request.Context, 4000) ?? entity.Context;
        var summary = NormalizeOptional(request.Summary, 4000) ?? entity.Summary;
        var sourceRef = NormalizeOptional(request.SourceRef, 512) ?? entity.SourceRef;
        var evidence = request.LinkedEvidence?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList()
            ?? DeserializeEvidence(entity.LinkedEvidenceJson);

        if (string.IsNullOrWhiteSpace(context))
        {
            return Validation(ApiErrorCodes.LessonContextRequired, "Lesson context is required.");
        }

        if (string.IsNullOrWhiteSpace(summary))
        {
            return Validation(ApiErrorCodes.LessonSummaryRequired, "Lesson summary is required.");
        }

        if (string.IsNullOrWhiteSpace(sourceRef) && evidence.Count == 0)
        {
            return Validation(ApiErrorCodes.LessonSourceRequired, "Lesson source reference or linked evidence is required.");
        }

        dbContext.Entry(entity).CurrentValues.SetValues(entity with
        {
            Context = context,
            Summary = summary!,
            SourceRef = sourceRef,
            LinkedEvidenceJson = SerializeEvidence(evidence),
            Status = "published",
            PublishedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        AppendAudit("publish", entity.Id, 200, new { entity.Title, Status = "published" });
        return Success((await queries.GetLessonLearnedAsync(entity.Id, cancellationToken))!);
    }

    private void AppendAudit(string action, Guid entityId, int statusCode, object metadata) =>
        auditLogWriter.Append(new AuditLogEntry("knowledge", action, "lesson_learned", entityId.ToString(), StatusCode: statusCode, Metadata: metadata));

    private static bool IsValidTransition(string current, string next) =>
        (NormalizeKey(current, 32), NormalizeKey(next, 32)) switch
        {
            ("draft", "draft") => true,
            ("draft", "reviewed") => true,
            ("reviewed", "reviewed") => true,
            ("reviewed", "published") => true,
            ("published", "published") => true,
            ("published", "archived") => true,
            ("archived", "archived") => true,
            _ => false
        };

    private static string? NormalizeRequired(string? value, int maxLength) => string.IsNullOrWhiteSpace(value) ? null : NormalizeOptional(value, maxLength);
    private static string? NormalizeOptional(string? value, int maxLength) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().Length <= maxLength ? value.Trim() : value.Trim()[..maxLength];
    private static string? NormalizeKey(string? value, int maxLength) => NormalizeOptional(value, maxLength)?.ToLowerInvariant();
    private static string? SerializeEvidence(IEnumerable<string>? evidence) => JsonSerializer.Serialize(evidence?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList() ?? []);
    private static List<string> DeserializeEvidence(string? json) => string.IsNullOrWhiteSpace(json) ? [] : (JsonSerializer.Deserialize<List<string>>(json) ?? []);
    private static KnowledgeCommandResult<LessonLearnedItem> Success(LessonLearnedItem value) => new(KnowledgeCommandStatus.Success, value);
    private static KnowledgeCommandResult<LessonLearnedItem> Validation(string code, string message) => new(KnowledgeCommandStatus.ValidationError, default, code, message);
    private static KnowledgeCommandResult<LessonLearnedItem> NotFound(string code, string message) => new(KnowledgeCommandStatus.NotFound, default, code, message);
}
