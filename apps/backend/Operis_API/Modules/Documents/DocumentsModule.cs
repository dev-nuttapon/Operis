using Microsoft.EntityFrameworkCore;
using Operis_API.Modules.Documents.Application;
using Operis_API.Modules.Documents.Contracts;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;
using System.Text.Json;
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
        services.AddScoped<IDocumentHistoryQueries, DocumentHistoryQueries>();
        services.AddScoped<DocumentHistoryWriter>();
        services.AddScoped<IDocumentTemplateQueries, DocumentTemplateQueries>();
        services.AddScoped<IDocumentTemplateCommands, DocumentTemplateCommands>();
        services.AddScoped<IDocumentTemplateHistoryQueries, DocumentTemplateHistoryQueries>();
        services.AddScoped<DocumentTemplateHistoryWriter>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/documents")
            .WithTags("Documents")
            .RequireAuthorization();

        group.MapGet("/", ListDocumentsAsync)
            .WithName("Documents_List");
        group.MapPost("/lookup", LookupDocumentsAsync)
            .WithName("Documents_Lookup");
        group.MapPost("/", CreateDocumentAsync)
            .WithName("Documents_Upload");
        group.MapGet("/{documentId:guid}/versions", ListDocumentVersionsAsync)
            .WithName("Documents_ListVersions");
        group.MapPost("/{documentId:guid}/versions", CreateDocumentVersionAsync)
            .DisableAntiforgery()
            .WithName("Documents_CreateVersion");
        group.MapDelete("/{documentId:guid}/versions/{versionId:guid}", DeleteDocumentVersionAsync)
            .WithName("Documents_DeleteVersion");
        group.MapPost("/{documentId:guid}/versions/{versionId:guid}/publish", PublishDocumentVersionAsync)
            .WithName("Documents_PublishVersion");
        group.MapPost("/{documentId:guid}/versions/unpublish", UnpublishDocumentVersionAsync)
            .WithName("Documents_UnpublishVersion");
        group.MapPut("/{documentId:guid}", UpdateDocumentAsync)
            .WithName("Documents_Update");
        group.MapDelete("/{documentId:guid}", DeleteDocumentAsync)
            .WithName("Documents_Delete");
        group.MapGet("/{documentId:guid}/download", DownloadDocumentAsync)
            .WithName("Documents_Download");
        group.MapGet("/{documentId:guid}/history", ListDocumentHistoryAsync)
            .WithName("Documents_History");
        group.MapGet("/templates", ListDocumentTemplatesAsync)
            .WithName("Documents_ListTemplates");
        group.MapPost("/templates", CreateDocumentTemplateAsync)
            .WithName("Documents_CreateTemplate");
        group.MapGet("/templates/{templateId:guid}", GetDocumentTemplateAsync)
            .WithName("Documents_GetTemplate");
        group.MapPut("/templates/{templateId:guid}", UpdateDocumentTemplateAsync)
            .WithName("Documents_UpdateTemplate");
        group.MapPost("/templates/{templateId:guid}/items/{documentId:guid}/refresh-version", RefreshDocumentTemplateItemVersionAsync)
            .WithName("Documents_TemplateRefreshItemVersion");
        group.MapGet("/templates/{templateId:guid}/history", ListDocumentTemplateHistoryAsync)
            .WithName("Documents_TemplateHistory");

        return endpoints;
    }

    private static async Task<IResult> ListDocumentsAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentQueries queries,
        CancellationToken cancellationToken,
        string? search = null,
        int page = 1,
        int pageSize = 10)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Read))
        {
            return Results.Forbid();
        }

        var items = await queries.ListDocumentsAsync(new DocumentListQuery(search, page, pageSize), cancellationToken);
        return Results.Ok(items);
    }

    private static async Task<IResult> LookupDocumentsAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentQueries queries,
        DocumentLookupRequest request,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Read))
        {
            return Results.Forbid();
        }

        var documentIds = request.DocumentIds ?? Array.Empty<Guid>();
        if (documentIds.Count == 0)
        {
            return Results.Ok(Array.Empty<DocumentListItem>());
        }

        var items = await queries.GetDocumentsByIdsAsync(documentIds, cancellationToken);
        return Results.Ok(items);
    }

    private static async Task<IResult> CreateDocumentAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentCommands commands,
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Upload)
            && !permissionMatrix.HasPermission(principal, Permissions.Documents.ManageVersions))
        {
            return Results.Forbid();
        }

        var documentName = await ExtractDocumentNameAsync(request, cancellationToken);
        if (string.IsNullOrWhiteSpace(documentName))
        {
            return BadRequestWithCode("A document name is required.", ApiErrorCodes.Documents.NameRequired);
        }

        var result = await commands.CreateDocumentAsync(
            new DocumentCreateCommand(
                documentName,
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

    private static async Task<IResult> UpdateDocumentAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentCommands commands,
        Guid documentId,
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Upload))
        {
            return Results.Forbid();
        }

        var documentName = await ExtractDocumentNameAsync(request, cancellationToken);
        if (string.IsNullOrWhiteSpace(documentName))
        {
            return BadRequestWithCode("A document name is required.", ApiErrorCodes.Documents.NameRequired);
        }

        var result = await commands.UpdateDocumentAsync(
            new DocumentUpdateCommand(
                documentId,
                documentName,
                principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)),
            cancellationToken);

        return result.Succeeded
            ? Results.Ok(result.Document)
            : BadRequestWithCode(result.ErrorMessage, result.ErrorCode);
    }

    private static async Task<IResult> DeleteDocumentAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentCommands commands,
        Guid documentId,
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.DeleteDraft))
        {
            return Results.Forbid();
        }

        var reason = await ExtractDeleteReasonAsync(request, cancellationToken);
        if (string.IsNullOrWhiteSpace(reason))
        {
            return BadRequestWithCode("A delete reason is required.", ApiErrorCodes.Documents.DeleteReasonRequired);
        }

        var result = await commands.DeleteDocumentAsync(
            new DocumentDeleteCommand(
                documentId,
                principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier),
                reason),
            cancellationToken);

        return result.Succeeded
            ? Results.NoContent()
            : BadRequestWithCode(result.ErrorMessage, result.ErrorCode);
    }

    private static async Task<IResult> DeleteDocumentVersionAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentCommands commands,
        Guid documentId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.ManageVersions))
        {
            return Results.Forbid();
        }

        var result = await commands.DeleteDocumentVersionAsync(
            new DocumentVersionDeleteCommand(
                documentId,
                versionId,
                principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)),
            cancellationToken);

        return result.Succeeded
            ? Results.NoContent()
            : BadRequestWithCode(result.ErrorMessage, result.ErrorCode);
    }

    private static async Task<IResult> PublishDocumentVersionAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentCommands commands,
        Guid documentId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Publish))
        {
            return Results.Forbid();
        }

        var result = await commands.PublishDocumentVersionAsync(
            new DocumentVersionPublishCommand(
                documentId,
                versionId,
                principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)),
            cancellationToken);

        return result.Succeeded
            ? Results.NoContent()
            : BadRequestWithCode(result.ErrorMessage, result.ErrorCode);
    }

    private static async Task<IResult> UnpublishDocumentVersionAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentCommands commands,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Publish))
        {
            return Results.Forbid();
        }

        var result = await commands.UnpublishDocumentVersionAsync(
            new DocumentVersionUnpublishCommand(
                documentId,
                principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)),
            cancellationToken);

        return result.Succeeded
            ? Results.NoContent()
            : BadRequestWithCode(result.ErrorMessage, result.ErrorCode);
    }

    private static async Task<IResult> ListDocumentVersionsAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentQueries queries,
        Guid documentId,
        CancellationToken cancellationToken,
        string? search = null,
        int page = 1,
        int pageSize = 10)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Read))
        {
            return Results.Forbid();
        }

        var versions = await queries.ListDocumentVersionsAsync(new DocumentVersionListQuery(documentId, search, page, pageSize), cancellationToken);
        return Results.Ok(versions);
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

    private static async Task<IResult> ListDocumentHistoryAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentHistoryQueries queries,
        Guid documentId,
        CancellationToken cancellationToken,
        string? search = null,
        int page = 1,
        int pageSize = 10)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.ActivityLogs.Read))
        {
            return Results.Forbid();
        }

        var items = await queries.ListAsync(new DocumentHistoryListQuery(documentId, search, page, pageSize), cancellationToken);
        return Results.Ok(items);
    }

    private static async Task<IResult> ListDocumentTemplatesAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentTemplateQueries queries,
        CancellationToken cancellationToken,
        string? search = null,
        int page = 1,
        int pageSize = 10)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Read))
        {
            return Results.Forbid();
        }

        var result = await queries.ListTemplatesAsync(new DocumentTemplateListQuery(search, page, pageSize), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateDocumentTemplateAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentTemplateCommands commands,
        IDocumentTemplateQueries queries,
        DocumentTemplateCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Upload))
        {
            return Results.Forbid();
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequestWithCode("A template name is required.", ApiErrorCodes.RequestValidationFailed);
        }

        var validation = await queries.ValidateTemplateDocumentsAsync(request.DocumentIds, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequestWithCode(validation.ErrorMessage, ApiErrorCodes.RequestValidationFailed);
        }

        var result = await commands.CreateTemplateAsync(
            new DocumentTemplateCreateCommand(
                request.Name,
                request.DocumentIds,
                principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)),
            cancellationToken);

        return Results.Created($"/api/v1/documents/templates/{result.Id}", result);
    }

    private static async Task<IResult> GetDocumentTemplateAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentTemplateQueries queries,
        Guid templateId,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Read))
        {
            return Results.Forbid();
        }

        var result = await queries.GetTemplateAsync(templateId, cancellationToken);
        return result is null ? NotFoundWithCode() : Results.Ok(result);
    }

    private static async Task<IResult> UpdateDocumentTemplateAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentTemplateCommands commands,
        IDocumentTemplateQueries queries,
        Guid templateId,
        DocumentTemplateUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Upload))
        {
            return Results.Forbid();
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequestWithCode("A template name is required.", ApiErrorCodes.RequestValidationFailed);
        }

        var existing = await queries.GetTemplateAsync(templateId, cancellationToken);
        if (existing is null)
        {
            return NotFoundWithCode();
        }

        var validation = await queries.ValidateTemplateDocumentsAsync(request.DocumentIds, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequestWithCode(validation.ErrorMessage, ApiErrorCodes.RequestValidationFailed);
        }

        var result = await commands.UpdateTemplateAsync(
            new DocumentTemplateUpdateCommand(
                templateId,
                request.Name,
                request.DocumentIds,
                principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)),
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> RefreshDocumentTemplateItemVersionAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentTemplateCommands commands,
        IDocumentTemplateQueries queries,
        Guid templateId,
        Guid documentId,
        DocumentTemplateItemVersionUpdateRequest? request,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Upload)
            && !permissionMatrix.HasPermission(principal, Permissions.Documents.ManageVersions))
        {
            return Results.Forbid();
        }

        var existing = await queries.GetTemplateAsync(templateId, cancellationToken);
        if (existing is null)
        {
            return NotFoundWithCode();
        }

        try
        {
            var result = await commands.RefreshTemplateItemVersionAsync(
                new DocumentTemplateItemRefreshCommand(
                    templateId,
                    documentId,
                    request?.DocumentVersionId,
                    principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)),
                cancellationToken);
            return Results.Ok(result);
        }
        catch (InvalidOperationException ex) when (string.Equals(ex.Message, "Template not found.", StringComparison.Ordinal))
        {
            return NotFoundWithCode();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequestWithCode(ex.Message, ApiErrorCodes.RequestValidationFailed);
        }
    }

    private static async Task<IResult> ListDocumentTemplateHistoryAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentTemplateHistoryQueries queries,
        Guid templateId,
        CancellationToken cancellationToken,
        string? search = null,
        int page = 1,
        int pageSize = 10)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.ActivityLogs.Read))
        {
            return Results.Forbid();
        }

        var items = await queries.ListAsync(new DocumentTemplateHistoryListQuery(templateId, search, page, pageSize), cancellationToken);
        return Results.Ok(items);
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

    private static async Task<string?> ExtractDocumentNameAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var rawBody = await ReadRawBodyAsync(request, cancellationToken);
        return ExtractStringFromBody(rawBody, "documentName", "document_name");
    }

    private static async Task<string?> ExtractDeleteReasonAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var rawBody = await ReadRawBodyAsync(request, cancellationToken);
        var reason = ExtractStringFromBody(rawBody, "reason");
        if (!string.IsNullOrWhiteSpace(reason))
        {
            return reason;
        }

        if (request.Query.TryGetValue("reason", out var reasonValues))
        {
            return reasonValues.ToString();
        }

        return null;
    }

    private static async Task<string?> ReadRawBodyAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        request.EnableBuffering();
        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);
        request.Body.Position = 0;
        return rawBody;
    }

    private static string? ExtractStringFromBody(string? rawBody, params string[] propertyNames)
    {
        if (string.IsNullOrWhiteSpace(rawBody))
        {
            return null;
        }

        if (TryExtractStringFromJson(rawBody, propertyNames, out var value))
        {
            return value;
        }

        var trimmed = rawBody.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    private static bool TryExtractStringFromJson(string rawBody, string[] propertyNames, out string? value)
    {
        value = null;
        try
        {
            using var document = JsonDocument.Parse(rawBody);
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.String)
            {
                var stringValue = root.GetString();
                if (!string.IsNullOrWhiteSpace(stringValue) && LooksLikeJson(stringValue))
                {
                    return TryExtractStringFromJson(stringValue, propertyNames, out value);
                }

                value = stringValue;
                return true;
            }

            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (var propertyName in propertyNames)
                {
                    if (root.TryGetProperty(propertyName, out var propValue))
                    {
                        value = propValue.GetString();
                        return true;
                    }
                }
            }
        }
        catch (JsonException)
        {
            value = null;
        }

        return false;
    }

    private static bool LooksLikeJson(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        return trimmed.StartsWith("{") && trimmed.EndsWith("}");
    }
}
