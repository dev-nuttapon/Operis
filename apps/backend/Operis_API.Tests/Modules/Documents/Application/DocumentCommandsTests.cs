using System.Text;
using Microsoft.Extensions.Options;
using Operis_API.Modules.Documents.Application;
using Operis_API.Modules.Documents.Infrastructure;
using Operis_API.Shared.Contracts;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Documents.Application;

public sealed class DocumentCommandsTests
{
    [Fact]
    public async Task UploadDocumentAsync_WhenFileExtensionIsNotAllowed_ReturnsValidationErrorWithoutPersisting()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var auditLogWriter = new FakeAuditLogWriter();
        var storage = new FakeDocumentObjectStorage();
        var sut = new DocumentCommands(
            dbContext,
            storage,
            auditLogWriter,
            Options.Create(new DocumentStorageOptions
            {
                BucketName = "documents",
                MaxFileSizeBytes = 10 * 1024 * 1024
            }));

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("not an allowed document"));

        var result = await sut.UploadDocumentAsync(
            new DocumentUploadRequest("malware.exe", "application/octet-stream", stream.Length, "user-1"),
            stream,
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ApiErrorCodes.Documents.FileTypeNotAllowed, result.ErrorCode);
        Assert.Empty(dbContext.Documents);
        Assert.Empty(auditLogWriter.Entries);
        Assert.Empty(storage.StoredObjectKeys);
    }

    private sealed class FakeDocumentObjectStorage : IDocumentObjectStorage
    {
        public List<string> StoredObjectKeys { get; } = [];

        public Task StoreAsync(string objectKey, Stream content, long size, string contentType, CancellationToken cancellationToken)
        {
            StoredObjectKeys.Add(objectKey);
            return Task.CompletedTask;
        }

        public Task<Stream> OpenReadAsync(string objectKey, CancellationToken cancellationToken) =>
            Task.FromResult<Stream>(new MemoryStream());
    }
}
