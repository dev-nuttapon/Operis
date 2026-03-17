using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Documents.Contracts;
using Operis_API.Modules.Documents.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Documents.Application;

public sealed class DocumentCommands(
    OperisDbContext dbContext,
    IAuditLogWriter auditLogWriter,
    IOptions<DocumentStorageOptions> optionsAccessor) : IDocumentCommands
{
    public async Task<DocumentUploadResult> CreateDocumentAsync(DocumentCreateCommand request, CancellationToken cancellationToken)
    {
        var options = optionsAccessor.Value;

        if (string.IsNullOrWhiteSpace(request.DocumentName))
        {
            return DocumentUploadResult.Fail(ApiErrorCodes.Documents.NameRequired, "A document name is required.");
        }

        var normalizedDocumentName = request.DocumentName.Trim();

        var entity = new DocumentEntity
        {
            Id = Guid.NewGuid(),
            DocumentName = normalizedDocumentName,
            FileName = string.Empty,
            ObjectKey = null,
            BucketName = options.BucketName,
            ContentType = "application/octet-stream",
            SizeBytes = 0,
            UploadedByUserId = request.CreatedByUserId,
            UploadedAt = DateTimeOffset.UtcNow
        };

        dbContext.Documents.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new DocumentListItem(entity.Id, entity.DocumentName, entity.FileName, entity.ContentType, entity.SizeBytes, entity.UploadedByUserId, entity.UploadedAt);

        auditLogWriter.Append(new AuditLogEntry(
            Module: "documents",
            Action: "create",
            EntityType: "document",
            EntityId: entity.Id.ToString(),
            StatusCode: StatusCodes.Status201Created,
            After: new
            {
                entity.Id,
                entity.DocumentName,
                entity.FileName,
                entity.ContentType,
                entity.SizeBytes,
                entity.UploadedByUserId,
                entity.UploadedAt
            }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return DocumentUploadResult.Success(response);
    }
}
