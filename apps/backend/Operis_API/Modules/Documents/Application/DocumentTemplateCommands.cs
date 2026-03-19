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
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var template = new DocumentTemplateEntity
        {
            Id = templateId,
            Name = command.Name.Trim(),
            CreatedByUserId = command.ActorUserId,
            CreatedAt = now,
            IsDeleted = false,
        };

        dbContext.DocumentTemplates.Add(template);

        var items = existingDocuments.Select((documentId, index) => new DocumentTemplateItemEntity
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            DocumentId = documentId,
            DisplayOrder = index + 1,
        });

        dbContext.DocumentTemplateItems.AddRange(items);

        await dbContext.SaveChangesAsync(cancellationToken);

        await TryAppendBusinessEventAsync(
            "template_create",
            template.Id,
            template.Name,
            new { template.Name, DocumentCount = existingDocuments.Count },
            cancellationToken);

        await TryAppendHistoryAsync(
            template.Id,
            "create",
            null,
            new { template.Name, DocumentIds = existingDocuments },
            template.Name,
            null,
            new { DocumentCount = existingDocuments.Count },
            cancellationToken);

        return new DocumentTemplateResponse(
            template.Id,
            template.Name,
            existingDocuments,
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
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var oldItems = await dbContext.DocumentTemplateItems
            .Where(x => x.TemplateId == command.TemplateId)
            .ToListAsync(cancellationToken);

        if (oldItems.Count > 0)
        {
            dbContext.DocumentTemplateItems.RemoveRange(oldItems);
        }

        var updated = template with { Name = command.Name.Trim() };
        dbContext.DocumentTemplates.Update(updated);

        var items = existingDocuments.Select((documentId, index) => new DocumentTemplateItemEntity
        {
            Id = Guid.NewGuid(),
            TemplateId = command.TemplateId,
            DocumentId = documentId,
            DisplayOrder = index + 1,
        });

        dbContext.DocumentTemplateItems.AddRange(items);

        await dbContext.SaveChangesAsync(cancellationToken);

        await TryAppendBusinessEventAsync(
            "template_update",
            updated.Id,
            updated.Name,
            new { updated.Name, DocumentCount = existingDocuments.Count },
            cancellationToken);

        var previousDocumentIds = oldItems.Select(x => x.DocumentId).ToList();
        await TryAppendHistoryAsync(
            updated.Id,
            "update",
            new { template.Name, DocumentIds = previousDocumentIds },
            new { updated.Name, DocumentIds = existingDocuments },
            updated.Name,
            null,
            new { DocumentCount = existingDocuments.Count },
            cancellationToken);

        return new DocumentTemplateResponse(
            updated.Id,
            updated.Name,
            existingDocuments,
            updated.CreatedAt);
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
