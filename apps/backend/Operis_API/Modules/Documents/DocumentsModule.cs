using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Operis_API.Modules.Documents.Application;
using Operis_API.Modules.Documents.Contracts;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;

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
        services.AddScoped<IDocumentTemplateCacheCommands, DocumentTemplateCacheCommands>();
        services.AddScoped<IDocumentTemplateHistoryQueries, DocumentTemplateHistoryQueries>();
        services.AddScoped<DocumentTemplateHistoryWriter>();
        services.AddSingleton<Infrastructure.IDocumentTemplateCache, Infrastructure.DocumentTemplateCache>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/documents")
            .WithTags("Documents")
            .RequireAuthorization();

        group.MapGet("/", ListDocumentsAsync).WithName("Documents_List");
        group.MapPost("/lookup", LookupDocumentsAsync).WithName("Documents_Lookup");
        group.MapPost("/", CreateDocumentAsync).WithName("Documents_Create");
        group.MapGet("/{documentId:guid}", GetDocumentAsync).WithName("Documents_Get");
        group.MapPut("/{documentId:guid}", UpdateDocumentAsync).WithName("Documents_Update");
        group.MapDelete("/{documentId:guid}", DeleteDocumentAsync).WithName("Documents_Delete");
        group.MapGet("/{documentId:guid}/versions", ListDocumentVersionsAsync).WithName("Documents_ListVersions");
        group.MapPost("/{documentId:guid}/versions", CreateDocumentVersionAsync).DisableAntiforgery().WithName("Documents_CreateVersion");
        group.MapDelete("/{documentId:guid}/versions/{versionId:guid}", DeleteDocumentVersionAsync).WithName("Documents_DeleteVersion");
        group.MapPut("/{documentId:guid}/submit", SubmitDocumentAsync).WithName("Documents_Submit");
        group.MapPut("/{documentId:guid}/approve", ApproveDocumentAsync).WithName("Documents_Approve");
        group.MapPut("/{documentId:guid}/reject", RejectDocumentAsync).WithName("Documents_Reject");
        group.MapPut("/{documentId:guid}/baseline", BaselineDocumentAsync).WithName("Documents_Baseline");
        group.MapPut("/{documentId:guid}/archive", ArchiveDocumentAsync).WithName("Documents_Archive");
        group.MapPost("/{documentId:guid}/links", CreateDocumentLinkAsync).WithName("Documents_CreateLink");
        group.MapGet("/{documentId:guid}/download", DownloadDocumentAsync).WithName("Documents_Download");
        group.MapGet("/{documentId:guid}/history", ListDocumentHistoryAsync).WithName("Documents_History");

        group.MapGet("/types", ListDocumentTypesAsync).WithName("Documents_ListTypes");
        group.MapPost("/types", CreateDocumentTypeAsync).WithName("Documents_CreateType");
        group.MapGet("/types/{documentTypeId:guid}", GetDocumentTypeAsync).WithName("Documents_GetType");
        group.MapPut("/types/{documentTypeId:guid}", UpdateDocumentTypeAsync).WithName("Documents_UpdateType");

        group.MapGet("/templates", ListDocumentTemplatesAsync).WithName("Documents_ListTemplates");
        group.MapPost("/templates/cache/refresh", RefreshDocumentTemplateCacheAsync).WithName("Documents_RefreshTemplateCache");
        group.MapPost("/templates", CreateDocumentTemplateAsync).WithName("Documents_CreateTemplate");
        group.MapGet("/templates/{templateId:guid}", GetDocumentTemplateAsync).WithName("Documents_GetTemplate");
        group.MapPut("/templates/{templateId:guid}", UpdateDocumentTemplateAsync).WithName("Documents_UpdateTemplate");
        group.MapPost("/templates/{templateId:guid}/items/{documentId:guid}/refresh-version", RefreshDocumentTemplateItemVersionAsync).WithName("Documents_TemplateRefreshItemVersion");
        group.MapGet("/templates/{templateId:guid}/history", ListDocumentTemplateHistoryAsync).WithName("Documents_TemplateHistory");

        return endpoints;
    }

    private static async Task<IResult> ListDocumentsAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentQueries queries,
        CancellationToken cancellationToken,
        string? search = null,
        Guid? documentTypeId = null,
        Guid? projectId = null,
        string? phaseCode = null,
        string? status = null,
        string? ownerUserId = null,
        string? classification = null,
        DateTimeOffset? updatedAfter = null,
        int page = 1,
        int pageSize = 10)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Read))
        {
            return Results.Forbid();
        }

        var items = await queries.ListDocumentsAsync(new DocumentListQuery(search, documentTypeId, projectId, phaseCode, status, ownerUserId, classification, updatedAfter, page, pageSize), cancellationToken);
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

        var documentIds = request.DocumentIds ?? [];
        if (documentIds.Count == 0)
        {
            return Results.Ok(Array.Empty<DocumentListItem>());
        }

        return Results.Ok(await queries.GetDocumentsByIdsAsync(documentIds, cancellationToken));
    }

    private static async Task<IResult> CreateDocumentAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentCommands commands,
        DocumentCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Upload))
        {
            return Results.Forbid();
        }

        var result = await commands.CreateDocumentAsync(
            new DocumentCreateCommand(
                request.DocumentTypeId,
                request.ProjectId,
                request.PhaseCode,
                request.OwnerUserId,
                request.Classification,
                request.RetentionClass,
                request.Title,
                request.Tags ?? [],
                GetActorUserId(principal)),
            cancellationToken);

        return result.Succeeded
            ? Results.Created($"/api/v1/documents/{result.Document!.Id}", result.Document)
            : ToProblemResult(result.ErrorMessage, result.ErrorCode);
    }

    private static async Task<IResult> GetDocumentAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentQueries queries,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Read))
        {
            return Results.Forbid();
        }

        var result = await queries.GetDocumentAsync(documentId, cancellationToken);
        return result is null ? NotFoundWithCode() : Results.Ok(result);
    }

    private static async Task<IResult> UpdateDocumentAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentCommands commands,
        Guid documentId,
        DocumentUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Upload))
        {
            return Results.Forbid();
        }

        var result = await commands.UpdateDocumentAsync(
            new DocumentUpdateCommand(
                documentId,
                request.DocumentTypeId,
                request.ProjectId,
                request.PhaseCode,
                request.OwnerUserId,
                request.Classification,
                request.RetentionClass,
                request.Title,
                request.Tags ?? [],
                GetActorUserId(principal)),
            cancellationToken);

        return result.Succeeded ? Results.Ok(result.Document) : ToProblemResult(result.ErrorMessage, result.ErrorCode);
    }

    private static async Task<IResult> DeleteDocumentAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentCommands commands,
        Guid documentId,
        [FromBody] DocumentDeleteRequest request,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.DeleteDraft))
        {
            return Results.Forbid();
        }

        var result = await commands.DeleteDocumentAsync(new DocumentDeleteCommand(documentId, GetActorUserId(principal), request.Reason), cancellationToken);
        return result.Succeeded ? Results.NoContent() : ToProblemResult(result.ErrorMessage, result.ErrorCode);
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

        return Results.Ok(await queries.ListDocumentVersionsAsync(new DocumentVersionListQuery(documentId, search, page, pageSize), cancellationToken));
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
            return BadRequestWithCode("Request must be multipart/form-data.");
        }

        var form = await request.ReadFormAsync(cancellationToken);
        var file = form.Files.GetFile("file");
        if (file is null)
        {
            return BadRequestWithCode("A file is required.", ApiErrorCodes.Documents.FileRequired);
        }

        var fileName = form.TryGetValue("fileName", out var fileNameValues) && !string.IsNullOrWhiteSpace(fileNameValues)
            ? fileNameValues.ToString()
            : file.FileName;
        var mimeType = form.TryGetValue("mimeType", out var mimeTypeValues) && !string.IsNullOrWhiteSpace(mimeTypeValues)
            ? mimeTypeValues.ToString()
            : file.ContentType;

        await using var stream = file.OpenReadStream();
        var result = await commands.CreateDocumentVersionAsync(
            new DocumentVersionCreateCommand(documentId, fileName, mimeType, file.Length, GetActorUserId(principal)),
            stream,
            cancellationToken);

        return result.Succeeded
            ? Results.Created($"/api/v1/documents/{documentId}/versions/{result.Version!.Id}", result.Version)
            : ToProblemResult(result.ErrorMessage, result.ErrorCode);
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

        var result = await commands.DeleteDocumentVersionAsync(new DocumentVersionDeleteCommand(documentId, versionId, GetActorUserId(principal)), cancellationToken);
        return result.Succeeded ? Results.NoContent() : ToProblemResult(result.ErrorMessage, result.ErrorCode);
    }

    private static async Task<IResult> SubmitDocumentAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentCommands commands,
        Guid documentId,
        DocumentApprovalDecisionRequest request,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.ManageVersions))
        {
            return Results.Forbid();
        }

        var result = await commands.SubmitDocumentAsync(new DocumentWorkflowCommand(documentId, GetActorUserId(principal), request.StepName, request.ReviewerUserId, request.DecisionReason), cancellationToken);
        return result.Succeeded ? Results.Ok(result.Document) : ToProblemResult(result.ErrorMessage, result.ErrorCode);
    }

    private static async Task<IResult> ApproveDocumentAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentCommands commands,
        Guid documentId,
        DocumentApprovalDecisionRequest request,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Publish))
        {
            return Results.Forbid();
        }

        var result = await commands.ApproveDocumentAsync(new DocumentWorkflowCommand(documentId, GetActorUserId(principal), request.StepName, request.ReviewerUserId, request.DecisionReason), cancellationToken);
        return result.Succeeded ? Results.Ok(result.Document) : ToProblemResult(result.ErrorMessage, result.ErrorCode);
    }

    private static async Task<IResult> RejectDocumentAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentCommands commands,
        Guid documentId,
        DocumentApprovalDecisionRequest request,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Publish))
        {
            return Results.Forbid();
        }

        var result = await commands.RejectDocumentAsync(new DocumentWorkflowCommand(documentId, GetActorUserId(principal), request.StepName, request.ReviewerUserId, request.DecisionReason), cancellationToken);
        return result.Succeeded ? Results.Ok(result.Document) : ToProblemResult(result.ErrorMessage, result.ErrorCode);
    }

    private static async Task<IResult> BaselineDocumentAsync(
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

        var result = await commands.BaselineDocumentAsync(new DocumentWorkflowCommand(documentId, GetActorUserId(principal)), cancellationToken);
        return result.Succeeded ? Results.Ok(result.Document) : ToProblemResult(result.ErrorMessage, result.ErrorCode);
    }

    private static async Task<IResult> ArchiveDocumentAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentCommands commands,
        Guid documentId,
        [FromBody] DocumentDeleteRequest request,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Deactivate))
        {
            return Results.Forbid();
        }

        var result = await commands.ArchiveDocumentAsync(new DocumentWorkflowCommand(documentId, GetActorUserId(principal), Reason: request.Reason), cancellationToken);
        return result.Succeeded ? Results.Ok(result.Document) : ToProblemResult(result.ErrorMessage, result.ErrorCode);
    }

    private static async Task<IResult> CreateDocumentLinkAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentCommands commands,
        Guid documentId,
        DocumentLinkRequest request,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Upload))
        {
            return Results.Forbid();
        }

        var result = await commands.CreateDocumentLinkAsync(new DocumentLinkCreateCommand(documentId, request.TargetEntityType, request.TargetEntityId, request.LinkType, GetActorUserId(principal)), cancellationToken);
        return result.Succeeded
            ? Results.Created($"/api/v1/documents/{documentId}/links/{result.Link!.Id}", result.Link)
            : ToProblemResult(result.ErrorMessage, result.ErrorCode);
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
        return result is null ? NotFoundWithCode() : Results.File(result.Content, result.ContentType, result.FileName);
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

        return Results.Ok(await queries.ListAsync(new DocumentHistoryListQuery(documentId, search, page, pageSize), cancellationToken));
    }

    private static async Task<IResult> ListDocumentTypesAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentQueries queries,
        CancellationToken cancellationToken,
        string? search = null,
        string? status = null,
        int page = 1,
        int pageSize = 10)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Read))
        {
            return Results.Forbid();
        }

        return Results.Ok(await queries.ListDocumentTypesAsync(new DocumentTypeListQuery(search, status, page, pageSize), cancellationToken));
    }

    private static async Task<IResult> GetDocumentTypeAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentQueries queries,
        Guid documentTypeId,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Read))
        {
            return Results.Forbid();
        }

        var result = await queries.GetDocumentTypeAsync(documentTypeId, cancellationToken);
        return result is null ? NotFoundWithCode() : Results.Ok(result);
    }

    private static async Task<IResult> CreateDocumentTypeAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentCommands commands,
        DocumentTypeCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Deactivate))
        {
            return Results.Forbid();
        }

        var result = await commands.CreateDocumentTypeAsync(new DocumentTypeCreateCommand(request.Code, request.Name, request.ModuleOwner, request.ClassificationDefault, request.RetentionClassDefault, request.ApprovalRequired), cancellationToken);
        return result.Succeeded
            ? Results.Created($"/api/v1/documents/types/{result.DocumentType!.Id}", result.DocumentType)
            : ToProblemResult(result.ErrorMessage, result.ErrorCode);
    }

    private static async Task<IResult> UpdateDocumentTypeAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentCommands commands,
        Guid documentTypeId,
        DocumentTypeUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Deactivate))
        {
            return Results.Forbid();
        }

        var result = await commands.UpdateDocumentTypeAsync(new DocumentTypeUpdateCommand(documentTypeId, request.Code, request.Name, request.ModuleOwner, request.ClassificationDefault, request.RetentionClassDefault, request.ApprovalRequired, request.Status), cancellationToken);
        return result.Succeeded ? Results.Ok(result.DocumentType) : ToProblemResult(result.ErrorMessage, result.ErrorCode);
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

        return Results.Ok(await queries.ListTemplatesAsync(new DocumentTemplateListQuery(search, page, pageSize), cancellationToken));
    }

    private static async Task<IResult> RefreshDocumentTemplateCacheAsync(
        ClaimsPrincipal principal,
        IPermissionMatrix permissionMatrix,
        IDocumentTemplateCacheCommands commands,
        CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Upload) && !permissionMatrix.HasPermission(principal, Permissions.Users.Update))
        {
            return Results.Forbid();
        }

        return Results.Ok(new { Total = await commands.RefreshAsync(cancellationToken) });
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
            return BadRequestWithCode("A template name is required.");
        }

        var validation = await queries.ValidateTemplateDocumentsAsync(request.DocumentIds, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequestWithCode(validation.ErrorMessage);
        }

        var result = await commands.CreateTemplateAsync(new DocumentTemplateCreateCommand(request.Name, request.DocumentIds, GetActorUserId(principal)), cancellationToken);
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

        var existing = await queries.GetTemplateAsync(templateId, cancellationToken);
        if (existing is null)
        {
            return NotFoundWithCode();
        }

        var validation = await queries.ValidateTemplateDocumentsAsync(request.DocumentIds, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequestWithCode(validation.ErrorMessage);
        }

        return Results.Ok(await commands.UpdateTemplateAsync(new DocumentTemplateUpdateCommand(templateId, request.Name, request.DocumentIds, GetActorUserId(principal)), cancellationToken));
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
        if (!permissionMatrix.HasPermission(principal, Permissions.Documents.Upload) && !permissionMatrix.HasPermission(principal, Permissions.Documents.ManageVersions))
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
            return Results.Ok(await commands.RefreshTemplateItemVersionAsync(new DocumentTemplateItemRefreshCommand(templateId, documentId, request?.DocumentVersionId, GetActorUserId(principal)), cancellationToken));
        }
        catch (InvalidOperationException ex) when (string.Equals(ex.Message, "Template not found.", StringComparison.Ordinal))
        {
            return NotFoundWithCode();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequestWithCode(ex.Message);
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

        return Results.Ok(await queries.ListAsync(new DocumentTemplateHistoryListQuery(templateId, search, page, pageSize), cancellationToken));
    }

    private static string? GetActorUserId(ClaimsPrincipal principal) =>
        principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

    private static IResult ToProblemResult(string? detail, string? code)
    {
        var isNotFound = string.Equals(code, ApiErrorCodes.Documents.DocumentNotFound, StringComparison.Ordinal)
            || string.Equals(code, ApiErrorCodes.Documents.DocumentTypeNotFound, StringComparison.Ordinal)
            || string.Equals(code, ApiErrorCodes.ProjectNotFound, StringComparison.Ordinal);

        return isNotFound ? NotFoundWithCode(detail, code) : BadRequestWithCode(detail, code);
    }

    private static IResult BadRequestWithCode(string? detail, string? code = null) =>
        Results.BadRequest(ApiProblemDetailsFactory.Create(StatusCodes.Status400BadRequest, code ?? ApiErrorCodes.RequestValidationFailed, "Bad Request", detail));

    private static IResult NotFoundWithCode(string? detail = null, string? code = null) =>
        Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, code ?? ApiErrorCodes.ResourceNotFound, "Not Found", detail));
}
