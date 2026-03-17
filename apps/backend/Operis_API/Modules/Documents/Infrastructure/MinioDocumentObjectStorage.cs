using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Operis_API.Modules.Documents.Infrastructure;

public sealed class MinioDocumentObjectStorage : IDocumentObjectStorage
{
    private readonly IMinioClient client;
    private readonly DocumentStorageOptions options;

    public MinioDocumentObjectStorage(IOptions<DocumentStorageOptions> optionsAccessor)
    {
        options = optionsAccessor.Value;

        var (host, port, hasPort) = ParseEndpoint(options.Endpoint);
        var builder = new MinioClient()
            .WithCredentials(options.AccessKey, options.SecretKey);

        if (hasPort && port.HasValue)
        {
            builder = builder.WithEndpoint(host, port.Value);
        }
        else
        {
            builder = builder.WithEndpoint(host);
        }

        if (options.UseSsl)
        {
            builder = builder.WithSSL(true);
        }

        client = builder.Build();
    }

    public async Task StoreAsync(string objectKey, Stream content, long size, string contentType, CancellationToken cancellationToken)
    {
        try
        {
            // IMPORTANT: S3 Signature V4 needs to calculate the payload hash.
            // Multipart form streams are non-seekable. Copying to MemoryStream ensures 
            // the SDK can read it for hashing AND then read it again for transmission.
            using var memoryStream = new MemoryStream();
            await content.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            var args = new PutObjectArgs()
                .WithBucket(options.BucketName)
                .WithObject(objectKey)
                .WithStreamData(memoryStream)
                .WithObjectSize(memoryStream.Length)
                .WithContentType(contentType);

            await client.PutObjectAsync(args, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception($"MinIO Storage Error: {ex.Message}. Endpoint: {options.Endpoint}, Bucket: {options.BucketName}, Key: {objectKey}", ex);
        }
    }

    public async Task<Stream> OpenReadAsync(string objectKey, CancellationToken cancellationToken)
    {
        var memory = new MemoryStream();
        await client.GetObjectAsync(
            new GetObjectArgs()
                .WithBucket(options.BucketName)
                .WithObject(objectKey)
                .WithCallbackStream(stream => stream.CopyTo(memory)),
            cancellationToken);
        memory.Position = 0;
        return memory;
    }

    private static (string host, int? port, bool hasPort) ParseEndpoint(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException("Minio endpoint is not configured.");
        }

        if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
        {
            var hasPort = !uri.IsDefaultPort && uri.Port > 0;
            return (uri.Host, hasPort ? uri.Port : null, hasPort);
        }

        var parts = endpoint.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2 && int.TryParse(parts[1], out var port))
        {
            return (parts[0], port, true);
        }

        return (endpoint, null, false);
    }
}
