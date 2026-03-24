using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Audits.Application;
using Operis_API.Modules.Documents.Contracts;
using Operis_API.Modules.Documents.Infrastructure;

namespace Operis_API.Modules.Documents.Application;

public sealed class DocumentTemplateCommands(
    OperisDbContext dbContext,
    IBusinessAuditEventWriter auditEventWriter,
    DocumentTemplateHistoryWriter historyWriter) : IDocumentTemplateCommands
{
    public async Task<DocumentTemplateResponse> CreateTemplateAsync(
        DocumentTemplateCreateCommand command,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var templateId = Guid.NewGuid();
        var documentIds = command.DocumentIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (documentIds.Count == 0)
        {
            documentIds = [];
        }

        var existingDocuments = await dbContext.Documents
            .AsNoTracking()
            .Where(x => !x.IsDeleted && documentIds.Contains(x.Id))
            .Select(x => new { x.Id, x.PublishedVersionId })
            .ToListAsync(cancellationToken);
        var existingDocumentIds = existingDocuments.Select(x => x.Id).ToList();
        var documentVersionById = existingDocuments
            .ToDictionary(x => x.Id, x => x.PublishedVersionId);

        var template = new DocumentTemplateEntity
        {
            Id = templateId,
            Name = command.Name.Trim(),
            CreatedByUserId = command.ActorUserId,
            CreatedAt = now,
            IsDeleted = false,
        };

        dbContext.DocumentTemplates.Add(template);

        var items = documentIds.Select((documentId, index) => new DocumentTemplateItemEntity
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            DocumentId = documentId,
            DocumentVersionId = documentVersionById.GetValueOrDefault(documentId),
            DisplayOrder = index + 1,
        });

        dbContext.DocumentTemplateItems.AddRange(items);

        await dbContext.SaveChangesAsync(cancellationToken);

        await TryAppendBusinessEventAsync(
            "template_create",
            template.Id,
            template.Name,
            new { template.Name, DocumentCount = existingDocumentIds.Count },
            cancellationToken);

        await TryAppendHistoryAsync(
            template.Id,
            "create",
            null,
            new { template.Name, DocumentIds = existingDocumentIds },
            template.Name,
            null,
            new { DocumentCount = existingDocumentIds.Count },
            cancellationToken);

        var responseItems = await LoadTemplateItemsAsync(template.Id, cancellationToken);
        return new DocumentTemplateResponse(
            template.Id,
            template.Name,
            responseItems.Select(item => item.DocumentId).ToList(),
            responseItems,
            template.CreatedAt);
    }

    public async Task<DocumentTemplateResponse> UpdateTemplateAsync(
        DocumentTemplateUpdateCommand command,
        CancellationToken cancellationToken)
    {
        var template = await dbContext.DocumentTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == command.TemplateId, cancellationToken);

        if (template is null)
        {
            throw new InvalidOperationException("Template not found.");
        }

        var documentIds = command.DocumentIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var existingDocuments = await dbContext.Documents
            .AsNoTracking()
            .Where(x => !x.IsDeleted && documentIds.Contains(x.Id))
            .Select(x => new { x.Id, x.PublishedVersionId })
            .ToListAsync(cancellationToken);
        var existingDocumentIds = existingDocuments.Select(x => x.Id).ToList();
        var documentVersionById = existingDocuments
            .ToDictionary(x => x.Id, x => x.PublishedVersionId);

        var oldItems = await dbContext.DocumentTemplateItems
            .Where(x => x.TemplateId == command.TemplateId)
            .ToListAsync(cancellationToken);

        if (oldItems.Count > 0)
        {
            dbContext.DocumentTemplateItems.RemoveRange(oldItems);
        }

        var updated = template with { Name = command.Name.Trim() };
        dbContext.DocumentTemplates.Update(updated);

        var items = documentIds.Select((documentId, index) => new DocumentTemplateItemEntity
        {
            Id = Guid.NewGuid(),
            TemplateId = command.TemplateId,
            DocumentId = documentId,
            DocumentVersionId = documentVersionById.GetValueOrDefault(documentId),
            DisplayOrder = index + 1,
        });

        dbContext.DocumentTemplateItems.AddRange(items);

        await dbContext.SaveChangesAsync(cancellationToken);

        await TryAppendBusinessEventAsync(
            "template_update",
            updated.Id,
            updated.Name,
            new { updated.Name, DocumentCount = existingDocumentIds.Count },
            cancellationToken);

        var previousDocumentIds = oldItems.Select(x => x.DocumentId).ToList();
        await TryAppendHistoryAsync(
            updated.Id,
            "update",
            new { template.Name, DocumentIds = previousDocumentIds },
            new { updated.Name, DocumentIds = existingDocumentIds },
            updated.Name,
            null,
            new { DocumentCount = existingDocumentIds.Count },
            cancellationToken);

        var responseItems = await LoadTemplateItemsAsync(updated.Id, cancellationToken);
        return new DocumentTemplateResponse(
            updated.Id,
            updated.Name,
            responseItems.Select(item => item.DocumentId).ToList(),
            responseItems,
            updated.CreatedAt);
    }

    private async Task<IReadOnlyList<DocumentTemplateItemResponse>> LoadTemplateItemsAsync(
        Guid templateId,
        CancellationToken cancellationToken)
    {
        return await (from item in dbContext.DocumentTemplateItems.AsNoTracking()
                join doc in dbContext.Documents.AsNoTracking() on item.DocumentId equals doc.Id
                join version in dbContext.DocumentVersions.AsNoTracking() on item.DocumentVersionId equals version.Id into versions
                from version in versions.DefaultIfEmpty()
                where item.TemplateId == templateId
                orderby item.DisplayOrder
                select new DocumentTemplateItemResponse(
                    item.DocumentId,
                    item.DocumentVersionId,
                    doc.DocumentName,
                    version != null ? version.VersionCode : null,
                    version != null ? (int?)version.Revision : null))
            .ToListAsync(cancellationToken);
    }

    public async Task<DocumentTemplateItemResponse> RefreshTemplateItemVersionAsync(
        DocumentTemplateItemRefreshCommand command,
        CancellationToken cancellationToken)
    {
        var template = await dbContext.DocumentTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == command.TemplateId, cancellationToken);

        if (template is null)
        {
            throw new InvalidOperationException("Template not found.");
        }

        var document = await dbContext.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == command.DocumentId, cancellationToken);

        if (document is null)
        {
            throw new InvalidOperationException("Document does not exist.");
        }

        Guid? targetVersionId = null;
        if (command.DocumentVersionId is not null)
        {
            var exists = await dbContext.DocumentVersions
                .AsNoTracking()
                .AnyAsync(
                    x => !x.IsDeleted
                         && x.Id == command.DocumentVersionId
                         && x.DocumentId == command.DocumentId,
                    cancellationToken);

            if (!exists)
            {
                throw new InvalidOperationException("Document version does not exist.");
            }

            targetVersionId = command.DocumentVersionId;
        }
        else
        {
            if (document.PublishedVersionId is null)
            {
                throw new InvalidOperationException("Document must have a published version.");
            }

            targetVersionId = document.PublishedVersionId;
        }

        var item = await dbContext.DocumentTemplateItems
            .FirstOrDefaultAsync(x => x.TemplateId == command.TemplateId && x.DocumentId == command.DocumentId, cancellationToken);

        if (item is null)
        {
            throw new InvalidOperationException("Template item not found.");
        }

        var updated = item with { DocumentVersionId = targetVersionId };
        dbContext.DocumentTemplateItems.Update(updated);
        await dbContext.SaveChangesAsync(cancellationToken);

        await TryAppendBusinessEventAsync(
            "template_refresh_version",
            template.Id,
            template.Name,
            new { template.Name, DocumentId = command.DocumentId, DocumentVersionId = updated.DocumentVersionId },
            cancellationToken);

        await TryAppendHistoryAsync(
            template.Id,
            "update_version",
            new { DocumentId = command.DocumentId, DocumentVersionId = item.DocumentVersionId },
            new { DocumentId = command.DocumentId, DocumentVersionId = updated.DocumentVersionId },
            template.Name,
            null,
            new { DocumentId = command.DocumentId },
            cancellationToken);

        var refreshed = await LoadTemplateItemsAsync(command.TemplateId, cancellationToken);
        var responseItem = refreshed.FirstOrDefault(x => x.DocumentId == command.DocumentId);
        return responseItem ?? new DocumentTemplateItemResponse(
            command.DocumentId,
            updated.DocumentVersionId,
            document.DocumentName,
            null,
            null);
    }

    private async Task TryAppendBusinessEventAsync(
        string eventType,
        Guid templateId,
        string? summary,
        object? metadata,
        CancellationToken cancellationToken)
    {
        try
        {
            await auditEventWriter.AppendAsync(
                module: "documents",
                eventType: eventType,
                entityType: "document_template",
                entityId: templateId.ToString(),
                summary: summary,
                reason: null,
                metadata: metadata,
                cancellationToken: cancellationToken);
        }
        catch
        {
            // Best-effort audit event
        }
    }

    private async Task TryAppendHistoryAsync(
        Guid templateId,
        string eventType,
        object? before,
        object? after,
        string? summary,
        string? reason,
        object? metadata,
        CancellationToken cancellationToken)
    {
        try
        {
            await historyWriter.AppendAsync(
                templateId,
                eventType,
                before,
                after,
                summary,
                reason,
                metadata,
                cancellationToken);
        }
        catch
        {
            // Best-effort history write
        }
    }
}
