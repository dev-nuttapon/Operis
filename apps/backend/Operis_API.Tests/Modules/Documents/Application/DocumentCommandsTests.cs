using System.Text;
using Microsoft.Extensions.Options;
using Operis_API.Modules.Documents.Application;
using Operis_API.Modules.Documents.Contracts;
using Operis_API.Modules.Documents.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Documents.Application;

public sealed class DocumentCommandsTests
{
    [Fact]
    public async Task CreateDocumentVersionAsync_WhenFileExtensionIsNotAllowed_ReturnsValidationErrorWithoutPersisting()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var auditLogWriter = new FakeAuditLogWriter();
        var storage = new FakeDocumentObjectStorage();
        var sut = new DocumentCommands(
            dbContext,
            storage,
            auditLogWriter,
            new FakeBusinessAuditEventWriter(),
            new DocumentHistoryWriter(dbContext, TestHttpContextFactory.CreateAccessor()),
            new FakeWorkflowInstanceCommands(),
            Options.Create(new DocumentStorageOptions
            {
                BucketName = "documents",
                MaxFileSizeBytes = 10 * 1024 * 1024
            }));

        // Create document first
        var docId = Guid.NewGuid();
        dbContext.Documents.Add(new DocumentEntity
        {
            Id = docId,
            Title = "Test",
            DocumentTypeId = null,
            ProjectId = null,
            PhaseCode = "DEV",
            OwnerUserId = "user-1",
            Classification = "internal",
            RetentionClass = "standard",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("not an allowed document"));

        var result = await sut.CreateDocumentVersionAsync(
            new DocumentVersionCreateCommand(docId, "malware.exe", "application/octet-stream", stream.Length, "user-1"),
            stream,
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ApiErrorCodes.Documents.FileTypeNotAllowed, result.ErrorCode);
        Assert.Empty(dbContext.DocumentVersions);
    }

    private sealed class FakeDocumentObjectStorage : IDocumentObjectStorage
    {
        public List<string> ObjectKeys { get; } = [];

        public Task StoreAsync(string objectKey, Stream content, long size, string contentType, CancellationToken cancellationToken)
        {
            ObjectKeys.Add(objectKey);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string objectKey, CancellationToken cancellationToken)
        {
            ObjectKeys.Remove(objectKey);
            return Task.CompletedTask;
        }

        public Task<Stream> OpenReadAsync(string objectKey, CancellationToken cancellationToken) =>
            Task.FromResult<Stream>(new MemoryStream());
    }
}
