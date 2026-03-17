using Microsoft.EntityFrameworkCore;
using Operis_API.Modules.Documents.Application;
using Operis_API.Modules.Documents.Contracts;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;
using System.Security.Claims;

namespace Operis_API.Modules.Documents;

public sealed class DocumentsModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<Infrastructure.DocumentStorageOptions>(configuration.GetSection(Infrastructure.DocumentStorageOptions.SectionName));
        services.AddScoped<Infrastructure.IDocumentObjectStorage, Infrastructure.MinioDocumentObjectStorage>();
        services.AddScoped<IDocumentQueries, DocumentQueries>();
        services.AddScoped<IDocumentCommands, DocumentCommands>();
        services.AddScoped<IDocumentDownloads, DocumentDownloads>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/documents")
            .WithTags("Documents")
            .RequireAuthorization();

        group.MapGet("/", ListDocumentsAsync)
            .WithName("Documents_List");
        group.MapPost("/", CreateDocumentAsync)
            .WithName("Documents_Upload");
        group.MapPost("/{documentId:guid}/versions", CreateDocumentVersionAsync)
            .DisableAntiforgery()
            .WithName("Documents_CreateVersion");
        group.MapGet("/{documentId:guid}/download", DownloadDocumentAsync)
            .WithName("Documents_Download");

        return endpoints;
    }

    private static async Task<IResult> ListDocumentsAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentQueries queries,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Read))
        {
            return Results.Forbid();
        }

        var items = await queries.ListDocumentsAsync(cancellationToken);
        return Results.Ok(items);
    }

    private static async Task<IResult> CreateDocumentAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentCommands commands,
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Upload))
        {
            return Results.Forbid();
        }

        var payload = await request.ReadFromJsonAsync<DocumentCreateRequest>(cancellationToken: cancellationToken);
        if (payload is null)
        {
            return BadRequestWithCode("A document name is required.", ApiErrorCodes.Documents.NameRequired);
        }

        var result = await commands.CreateDocumentAsync(
            new DocumentCreateCommand(
                payload.DocumentName,
                principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)),
            cancellationToken);

        return result.Succeeded
            ? Results.Created($"/api/v1/documents/{result.Document!.Id}", result.Document)
            : BadRequestWithCode(result.ErrorMessage, result.ErrorCode);
    }

    private static async Task<IResult> CreateDocumentVersionAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentCommands commands,
        Guid documentId,
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.ManageVersions))
        {
            return Results.Forbid();
        }

        if (!request.HasFormContentType)
        {
            return BadRequestWithCode("Request must be multipart/form-data.", ApiErrorCodes.RequestValidationFailed);
        }

        var form = await request.ReadFormAsync(cancellationToken);
        var file = form.Files.GetFile("file");
        var versionCode = form.TryGetValue("versionCode", out var codeValues) ? codeValues.ToString() : null;

        if (file is null)
        {
            return BadRequestWithCode("A file is required.", ApiErrorCodes.Documents.FileRequired);
        }

        await using var stream = file.OpenReadStream();
        var result = await commands.CreateDocumentVersionAsync(
            new DocumentVersionCreateCommand(
                documentId,
                versionCode ?? string.Empty,
                file.FileName,
                file.ContentType,
                file.Length,
                principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)),
            stream,
            cancellationToken);

        return result.Succeeded
            ? Results.Created($"/api/v1/documents/{documentId}/versions/{result.Version!.Id}", result.Version)
            : BadRequestWithCode(result.ErrorMessage, result.ErrorCode);
    }

    private static async Task<IResult> DownloadDocumentAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentDownloads downloads,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Read))
        {
            return Results.Forbid();
        }

        var result = await downloads.GetDownloadAsync(documentId, cancellationToken);
        if (result is null)
        {
            return NotFoundWithCode();
        }

        return Results.File(result.Content, result.ContentType, result.FileName);
    }

    private static IResult BadRequestWithCode(string? detail, string? code = null) =>
        Results.BadRequest(ApiProblemDetailsFactory.Create(
            StatusCodes.Status400BadRequest,
            code ?? ApiErrorCodes.RequestValidationFailed,
            "Bad Request",
            detail));

    private static IResult NotFoundWithCode(string? detail = null) =>
        Results.NotFound(ApiProblemDetailsFactory.Create(
            StatusCodes.Status404NotFound,
            ApiErrorCodes.ResourceNotFound,
            "Not Found",
            detail));
}
