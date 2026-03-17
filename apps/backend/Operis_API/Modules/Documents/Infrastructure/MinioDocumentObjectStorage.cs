using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using System.Net.Http.Headers;

namespace Operis_API.Modules.Documents.Infrastructure;

public sealed class MinioDocumentObjectStorage : IDocumentObjectStorage
{
    private readonly IMinioClient client;
    private readonly DocumentStorageOptions options;
    private readonly bool traceEnabled;

    public MinioDocumentObjectStorage(IOptions<DocumentStorageOptions> optionsAccessor)
    {
        options = optionsAccessor.Value;
        traceEnabled = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Local", StringComparison.OrdinalIgnoreCase);

        if (traceEnabled)
        {
            Console.WriteLine($"[Minio Init] Raw Endpoint='{options.Endpoint}'");
        }

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
        if (traceEnabled)
        {
            var httpClient = new HttpClient(new MinioHttpTraceHandler())
            {
                Timeout = TimeSpan.FromMinutes(2)
            };
            client = client.WithHttpClient(httpClient, disposeHttpClient: true);
        }
        if (traceEnabled && client is MinioClient minioClient)
        {
            minioClient.SetTraceOn(new Minio.Handlers.DefaultRequestLogger());
        }
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

            if (traceEnabled)
            {
                Console.WriteLine($"[Minio Trace] PUT s3://{options.BucketName}/{objectKey}");
            }

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

        if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) && !string.IsNullOrWhiteSpace(uri.Host))
        {
            var hasPort = !uri.IsDefaultPort && uri.Port > 0;
            return (uri.Host, hasPort ? uri.Port : null, hasPort);
        }

        var parts = endpoint.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2 && int.TryParse(parts[1], out var port))
        {
            if (string.IsNullOrWhiteSpace(parts[0]))
            {
                throw new InvalidOperationException($"Minio endpoint has no host: '{endpoint}'.");
            }
            return (parts[0], port, true);
        }

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException("Minio endpoint has no host.");
        }

        return (endpoint, null, false);
    }

    private sealed class MinioHttpTraceHandler : DelegatingHandler
    {
        public MinioHttpTraceHandler()
            : base(new HttpClientHandler())
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var host = request.Headers.Host ?? request.RequestUri?.Authority ?? "<null>";
                var sha = request.Headers.TryGetValues("x-amz-content-sha256", out var shaValues)
                    ? string.Join(",", shaValues)
                    : "<none>";
                var date = request.Headers.TryGetValues("x-amz-date", out var dateValues)
                    ? string.Join(",", dateValues)
                    : "<none>";
                var authHeader = request.Headers.TryGetValues("Authorization", out var authValues)
                    ? string.Join(",", authValues)
                    : "<none>";
                var accessKey = ExtractAccessKey(authHeader);
                var signedHeaders = ExtractSignedHeaders(authHeader);
                var scope = ExtractCredentialScope(authHeader);
                var contentType = request.Content?.Headers.ContentType?.ToString() ?? "<none>";
                var contentLength = request.Content?.Headers.ContentLength?.ToString() ?? "<none>";

                Console.WriteLine("[Minio HttpTrace] " +
                                  $"{request.Method} {request.RequestUri} Host={host} x-amz-date={date} x-amz-content-sha256={sha} " +
                                  $"AccessKey={accessKey} Scope={scope} SignedHeaders={signedHeaders} ContentType={contentType} ContentLength={contentLength}");
            }
            catch
            {
                // Avoid breaking MinIO calls due to trace issues.
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private static string ExtractAccessKey(string authHeader)
        {
            const string marker = "Credential=";
            var idx = authHeader.IndexOf(marker, StringComparison.Ordinal);
            if (idx < 0)
            {
                return "<none>";
            }
            var start = idx + marker.Length;
            var slash = authHeader.IndexOf('/', start);
            if (slash < 0)
            {
                return "<unknown>";
            }
            return authHeader[start..slash];
        }

        private static string ExtractSignedHeaders(string authHeader)
        {
            const string marker = "SignedHeaders=";
            var idx = authHeader.IndexOf(marker, StringComparison.Ordinal);
            if (idx < 0)
            {
                return "<none>";
            }
            var start = idx + marker.Length;
            var end = authHeader.IndexOf(',', start);
            if (end < 0)
            {
                end = authHeader.Length;
            }
            return authHeader[start..end];
        }

        private static string ExtractCredentialScope(string authHeader)
        {
            const string marker = "Credential=";
            var idx = authHeader.IndexOf(marker, StringComparison.Ordinal);
            if (idx < 0)
            {
                return "<none>";
            }
            var start = idx + marker.Length;
            var end = authHeader.IndexOf(',', start);
            if (end < 0)
            {
                end = authHeader.Length;
            }
            return authHeader[start..end];
        }
    }
}
