namespace Operis_API.Modules.Documents.Infrastructure;

public interface IDocumentObjectStorage
{
    Task StoreAsync(string objectKey, Stream content, long size, string contentType, CancellationToken cancellationToken);
    Task<Stream> OpenReadAsync(string objectKey, CancellationToken cancellationToken);
}
