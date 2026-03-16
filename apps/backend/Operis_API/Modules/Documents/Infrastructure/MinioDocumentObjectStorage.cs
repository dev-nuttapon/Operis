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
        client = new MinioClient()
            .WithEndpoint(options.Endpoint)
            .WithCredentials(options.AccessKey, options.SecretKey)
            .WithSSL(options.UseSsl)
            .Build();
    }

    public async Task StoreAsync(string objectKey, Stream content, long size, string contentType, CancellationToken cancellationToken)
    {
        await EnsureBucketExistsAsync(cancellationToken);

        await client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(options.BucketName)
                .WithObject(objectKey)
                .WithStreamData(content)
                .WithObjectSize(size)
                .WithContentType(contentType),
            cancellationToken);
    }

    public async Task<Stream> OpenReadAsync(string objectKey, CancellationToken cancellationToken)
    {
        await EnsureBucketExistsAsync(cancellationToken);

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

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        var exists = await client.BucketExistsAsync(new BucketExistsArgs().WithBucket(options.BucketName), cancellationToken);
        if (!exists)
        {
            await client.MakeBucketAsync(new MakeBucketArgs().WithBucket(options.BucketName), cancellationToken);
        }
    }
}
