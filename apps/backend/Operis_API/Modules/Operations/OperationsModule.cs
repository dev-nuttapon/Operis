using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Operis_API.Modules.Operations.Application;
using Operis_API.Modules.Operations.Contracts;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;

namespace Operis_API.Modules.Operations;

public sealed class OperationsModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IOperationsQueries, OperationsQueries>();
        services.AddScoped<IOperationsCommands, OperationsCommands>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var accessReviews = endpoints.MapGroup("/api/v1/access-reviews").WithTags("Operations").RequireAuthorization();
        accessReviews.MapGet("/", ListAccessReviewsAsync);
        accessReviews.MapPost("/", CreateAccessReviewAsync);
        accessReviews.MapPut("/{id:guid}", UpdateAccessReviewAsync);
        accessReviews.MapPut("/{id:guid}/approve", ApproveAccessReviewAsync);

        var accessRecertifications = endpoints.MapGroup("/api/v1/access-recertifications").WithTags("Operations").RequireAuthorization();
        accessRecertifications.MapGet("/", ListAccessRecertificationsAsync);
        accessRecertifications.MapPost("/", CreateAccessRecertificationAsync);
        accessRecertifications.MapGet("/{id:guid}", GetAccessRecertificationAsync);
        accessRecertifications.MapPut("/{id:guid}", UpdateAccessRecertificationAsync);
        accessRecertifications.MapPost("/{id:guid}/decisions", AddAccessRecertificationDecisionAsync);
        accessRecertifications.MapPut("/{id:guid}/complete", CompleteAccessRecertificationAsync);

        var securityIncidents = endpoints.MapGroup("/api/v1/security-incidents").WithTags("Operations").RequireAuthorization();
        securityIncidents.MapGet("/", ListSecurityIncidentsAsync);
        securityIncidents.MapPost("/", CreateSecurityIncidentAsync);
        securityIncidents.MapGet("/{id:guid}", GetSecurityIncidentAsync);
        securityIncidents.MapPut("/{id:guid}", UpdateSecurityIncidentAsync);

        var vulnerabilities = endpoints.MapGroup("/api/v1/vulnerabilities").WithTags("Operations").RequireAuthorization();
        vulnerabilities.MapGet("/", ListVulnerabilitiesAsync);
        vulnerabilities.MapPost("/", CreateVulnerabilityAsync);
        vulnerabilities.MapPut("/{id:guid}", UpdateVulnerabilityAsync);

        var secretRotations = endpoints.MapGroup("/api/v1/secret-rotations").WithTags("Operations").RequireAuthorization();
        secretRotations.MapGet("/", ListSecretRotationsAsync);
        secretRotations.MapPost("/", CreateSecretRotationAsync);
        secretRotations.MapPut("/{id:guid}", UpdateSecretRotationAsync);

        var privilegedAccessEvents = endpoints.MapGroup("/api/v1/privileged-access-events").WithTags("Operations").RequireAuthorization();
        privilegedAccessEvents.MapGet("/", ListPrivilegedAccessEventsAsync);
        privilegedAccessEvents.MapPost("/", CreatePrivilegedAccessEventAsync);
        privilegedAccessEvents.MapPut("/{id:guid}", UpdatePrivilegedAccessEventAsync);

        var classificationPolicies = endpoints.MapGroup("/api/v1/classification-policies").WithTags("Operations").RequireAuthorization();
        classificationPolicies.MapGet("/", ListClassificationPoliciesAsync);
        classificationPolicies.MapPost("/", CreateClassificationPolicyAsync);
        classificationPolicies.MapPut("/{id:guid}", UpdateClassificationPolicyAsync);

        var backupEvidence = endpoints.MapGroup("/api/v1/backup-evidence").WithTags("Operations").RequireAuthorization();
        backupEvidence.MapGet("/", ListBackupEvidenceAsync);
        backupEvidence.MapPost("/", CreateBackupEvidenceAsync);

        var restoreVerifications = endpoints.MapGroup("/api/v1/restore-verifications").WithTags("Operations").RequireAuthorization();
        restoreVerifications.MapGet("/", ListRestoreVerificationsAsync);
        restoreVerifications.MapPost("/", CreateRestoreVerificationAsync);

        var drDrills = endpoints.MapGroup("/api/v1/dr-drills").WithTags("Operations").RequireAuthorization();
        drDrills.MapGet("/", ListDrDrillsAsync);
        drDrills.MapPost("/", CreateDrDrillAsync);
        drDrills.MapPut("/{id:guid}", UpdateDrDrillAsync);

        var legalHolds = endpoints.MapGroup("/api/v1/legal-holds").WithTags("Operations").RequireAuthorization();
        legalHolds.MapGet("/", ListLegalHoldsAsync);
        legalHolds.MapPost("/", CreateLegalHoldAsync);
        legalHolds.MapPut("/{id:guid}/release", ReleaseLegalHoldAsync);

        var capa = endpoints.MapGroup("/api/v1/capa").WithTags("Operations").RequireAuthorization();
        capa.MapGet("/", ListCapaRecordsAsync);
        capa.MapPost("/", CreateCapaRecordAsync);
        capa.MapGet("/{id:guid}", GetCapaRecordAsync);
        capa.MapPut("/{id:guid}", UpdateCapaRecordAsync);
        capa.MapPost("/{id:guid}/actions", AddCapaActionAsync);
        capa.MapPut("/{id:guid}/verify", VerifyCapaAsync);
        capa.MapPut("/{id:guid}/close", CloseCapaAsync);

        var escalations = endpoints.MapGroup("/api/v1/escalations").WithTags("Operations").RequireAuthorization();
        escalations.MapGet("/", ListEscalationEventsAsync);
        escalations.MapPost("/", CreateEscalationEventAsync);

        var securityReviews = endpoints.MapGroup("/api/v1/security-reviews").WithTags("Operations").RequireAuthorization();
        securityReviews.MapGet("/", ListSecurityReviewsAsync);
        securityReviews.MapPost("/", CreateSecurityReviewAsync);
        securityReviews.MapPut("/{id:guid}", UpdateSecurityReviewAsync);

        var dependencies = endpoints.MapGroup("/api/v1/external-dependencies").WithTags("Operations").RequireAuthorization();
        dependencies.MapGet("/", ListExternalDependenciesAsync);
        dependencies.MapPost("/", CreateExternalDependencyAsync);
        dependencies.MapPut("/{id:guid}", UpdateExternalDependencyAsync);

        var suppliers = endpoints.MapGroup("/api/v1/suppliers").WithTags("Operations").RequireAuthorization();
        suppliers.MapGet("/", ListSuppliersAsync);
        suppliers.MapPost("/", CreateSupplierAsync);
        suppliers.MapGet("/{id:guid}", GetSupplierAsync);
        suppliers.MapPut("/{id:guid}", UpdateSupplierAsync);

        var supplierAgreements = endpoints.MapGroup("/api/v1/supplier-agreements").WithTags("Operations").RequireAuthorization();
        supplierAgreements.MapGet("/", ListSupplierAgreementsAsync);
        supplierAgreements.MapPost("/", CreateSupplierAgreementAsync);
        supplierAgreements.MapPut("/{id:guid}", UpdateSupplierAgreementAsync);

        var audits = endpoints.MapGroup("/api/v1/configuration-audits").WithTags("Operations").RequireAuthorization();
        audits.MapGet("/", ListConfigurationAuditsAsync);
        audits.MapPost("/", CreateConfigurationAuditAsync);

        return endpoints;
    }

    private static async Task<IResult> ListAccessReviewsAsync(ClaimsPrincipal principal, [AsParameters] AccessReviewListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read access reviews.");
        }

        return Results.Ok(await queries.ListAccessReviewsAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateAccessReviewAsync(ClaimsPrincipal principal, CreateAccessReviewRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage access reviews.", () => commands.CreateAccessReviewAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateAccessReviewAsync(ClaimsPrincipal principal, Guid id, UpdateAccessReviewRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage access reviews.", () => commands.UpdateAccessReviewAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ApproveAccessReviewAsync(ClaimsPrincipal principal, Guid id, ApproveAccessReviewRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Approve, "You do not have permission to approve access reviews.", () => commands.ApproveAccessReviewAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListAccessRecertificationsAsync(ClaimsPrincipal principal, [AsParameters] AccessRecertificationListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read access recertifications.");
        }

        return Results.Ok(await queries.ListAccessRecertificationsAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetAccessRecertificationAsync(ClaimsPrincipal principal, Guid id, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read access recertifications.");
        }

        var detail = await queries.GetAccessRecertificationAsync(id, cancellationToken);
        return detail is null
            ? Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, ApiErrorCodes.ResourceNotFound, "Access recertification not found.", "Access recertification not found."))
            : Results.Ok(detail);
    }

    private static async Task<IResult> CreateAccessRecertificationAsync(ClaimsPrincipal principal, CreateAccessRecertificationRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage access recertifications.", () => commands.CreateAccessRecertificationAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateAccessRecertificationAsync(ClaimsPrincipal principal, Guid id, UpdateAccessRecertificationRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage access recertifications.", () => commands.UpdateAccessRecertificationAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> AddAccessRecertificationDecisionAsync(ClaimsPrincipal principal, Guid id, AddAccessRecertificationDecisionRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage access recertification decisions.", () => commands.AddAccessRecertificationDecisionAsync(id, request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> CompleteAccessRecertificationAsync(ClaimsPrincipal principal, Guid id, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Approve, "You do not have permission to complete access recertifications.", () => commands.CompleteAccessRecertificationAsync(id, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListSecurityIncidentsAsync(ClaimsPrincipal principal, [AsParameters] SecurityIncidentListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read security incidents.");
        }

        return Results.Ok(await queries.ListSecurityIncidentsAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetSecurityIncidentAsync(ClaimsPrincipal principal, Guid id, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read security incidents.");
        }

        var detail = await queries.GetSecurityIncidentAsync(id, cancellationToken);
        return detail is null
            ? Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, ApiErrorCodes.ResourceNotFound, "Security incident not found.", "Security incident not found."))
            : Results.Ok(detail);
    }

    private static async Task<IResult> CreateSecurityIncidentAsync(ClaimsPrincipal principal, CreateSecurityIncidentRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage security incidents.", () => commands.CreateSecurityIncidentAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateSecurityIncidentAsync(ClaimsPrincipal principal, Guid id, UpdateSecurityIncidentRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage security incidents.", () => commands.UpdateSecurityIncidentAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListVulnerabilitiesAsync(ClaimsPrincipal principal, [AsParameters] VulnerabilityListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read vulnerabilities.");
        }

        return Results.Ok(await queries.ListVulnerabilitiesAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateVulnerabilityAsync(ClaimsPrincipal principal, CreateVulnerabilityRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage vulnerabilities.", () => commands.CreateVulnerabilityAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateVulnerabilityAsync(ClaimsPrincipal principal, Guid id, UpdateVulnerabilityRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage vulnerabilities.", () => commands.UpdateVulnerabilityAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListSecretRotationsAsync(ClaimsPrincipal principal, [AsParameters] SecretRotationListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read secret rotations.");
        }

        return Results.Ok(await queries.ListSecretRotationsAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateSecretRotationAsync(ClaimsPrincipal principal, CreateSecretRotationRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage secret rotations.", () => commands.CreateSecretRotationAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateSecretRotationAsync(ClaimsPrincipal principal, Guid id, UpdateSecretRotationRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage secret rotations.", () => commands.UpdateSecretRotationAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListPrivilegedAccessEventsAsync(ClaimsPrincipal principal, [AsParameters] PrivilegedAccessEventListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read privileged access events.");
        }

        return Results.Ok(await queries.ListPrivilegedAccessEventsAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreatePrivilegedAccessEventAsync(ClaimsPrincipal principal, CreatePrivilegedAccessEventRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage privileged access events.", () => commands.CreatePrivilegedAccessEventAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdatePrivilegedAccessEventAsync(ClaimsPrincipal principal, Guid id, UpdatePrivilegedAccessEventRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage privileged access events.", () => commands.UpdatePrivilegedAccessEventAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListClassificationPoliciesAsync(ClaimsPrincipal principal, [AsParameters] ClassificationPolicyListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read classification policies.");
        }

        return Results.Ok(await queries.ListClassificationPoliciesAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateClassificationPolicyAsync(ClaimsPrincipal principal, CreateClassificationPolicyRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage classification policies.", () => commands.CreateClassificationPolicyAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateClassificationPolicyAsync(ClaimsPrincipal principal, Guid id, UpdateClassificationPolicyRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage classification policies.", () => commands.UpdateClassificationPolicyAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListBackupEvidenceAsync(ClaimsPrincipal principal, [AsParameters] BackupEvidenceListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read backup evidence.");
        }

        return Results.Ok(await queries.ListBackupEvidenceAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateBackupEvidenceAsync(ClaimsPrincipal principal, CreateBackupEvidenceRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage backup evidence.", () => commands.CreateBackupEvidenceAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> ListRestoreVerificationsAsync(ClaimsPrincipal principal, [AsParameters] RestoreVerificationListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read restore verifications.");
        }

        return Results.Ok(await queries.ListRestoreVerificationsAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateRestoreVerificationAsync(ClaimsPrincipal principal, CreateRestoreVerificationRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage restore verifications.", () => commands.CreateRestoreVerificationAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> ListDrDrillsAsync(ClaimsPrincipal principal, [AsParameters] DrDrillListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read DR drills.");
        }

        return Results.Ok(await queries.ListDrDrillsAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateDrDrillAsync(ClaimsPrincipal principal, CreateDrDrillRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage DR drills.", () => commands.CreateDrDrillAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateDrDrillAsync(ClaimsPrincipal principal, Guid id, UpdateDrDrillRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage DR drills.", () => commands.UpdateDrDrillAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListLegalHoldsAsync(ClaimsPrincipal principal, [AsParameters] LegalHoldListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read legal holds.");
        }

        return Results.Ok(await queries.ListLegalHoldsAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateLegalHoldAsync(ClaimsPrincipal principal, CreateLegalHoldRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage legal holds.", () => commands.CreateLegalHoldAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> ReleaseLegalHoldAsync(ClaimsPrincipal principal, Guid id, ReleaseLegalHoldRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Approve, "You do not have permission to release legal holds.", () => commands.ReleaseLegalHoldAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListCapaRecordsAsync(ClaimsPrincipal principal, [AsParameters] CapaRecordListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read CAPA records.");
        }

        return Results.Ok(await queries.ListCapaRecordsAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetCapaRecordAsync(ClaimsPrincipal principal, Guid id, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read CAPA records.");
        }

        var detail = await queries.GetCapaRecordAsync(id, cancellationToken);
        return detail is null
            ? Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, ApiErrorCodes.ResourceNotFound, "CAPA record not found.", "CAPA record not found."))
            : Results.Ok(detail);
    }

    private static async Task<IResult> CreateCapaRecordAsync(ClaimsPrincipal principal, CreateCapaRecordRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage CAPA records.", () => commands.CreateCapaRecordAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateCapaRecordAsync(ClaimsPrincipal principal, Guid id, UpdateCapaRecordRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage CAPA records.", () => commands.UpdateCapaRecordAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> AddCapaActionAsync(ClaimsPrincipal principal, Guid id, CreateCapaActionRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage CAPA actions.", () => commands.AddCapaActionAsync(id, request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> VerifyCapaAsync(ClaimsPrincipal principal, Guid id, VerifyCapaRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Approve, "You do not have permission to verify CAPA records.", () => commands.VerifyCapaAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> CloseCapaAsync(ClaimsPrincipal principal, Guid id, CloseCapaRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Approve, "You do not have permission to close CAPA records.", () => commands.CloseCapaAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListEscalationEventsAsync(ClaimsPrincipal principal, [AsParameters] EscalationEventListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read escalation history.");
        }

        return Results.Ok(await queries.ListEscalationEventsAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateEscalationEventAsync(ClaimsPrincipal principal, CreateEscalationEventRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage escalation history.", () => commands.CreateEscalationEventAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> ListSecurityReviewsAsync(ClaimsPrincipal principal, [AsParameters] SecurityReviewListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read security reviews.");
        }

        return Results.Ok(await queries.ListSecurityReviewsAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateSecurityReviewAsync(ClaimsPrincipal principal, CreateSecurityReviewRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage security reviews.", () => commands.CreateSecurityReviewAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateSecurityReviewAsync(ClaimsPrincipal principal, Guid id, UpdateSecurityReviewRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage security reviews.", () => commands.UpdateSecurityReviewAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListExternalDependenciesAsync(ClaimsPrincipal principal, [AsParameters] ExternalDependencyListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read external dependencies.");
        }

        return Results.Ok(await queries.ListExternalDependenciesAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateExternalDependencyAsync(ClaimsPrincipal principal, CreateExternalDependencyRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage external dependencies.", () => commands.CreateExternalDependencyAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateExternalDependencyAsync(ClaimsPrincipal principal, Guid id, UpdateExternalDependencyRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage external dependencies.", () => commands.UpdateExternalDependencyAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListSuppliersAsync(ClaimsPrincipal principal, [AsParameters] SupplierListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read suppliers.");
        }

        return Results.Ok(await queries.ListSuppliersAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetSupplierAsync(ClaimsPrincipal principal, Guid id, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read suppliers.");
        }

        var detail = await queries.GetSupplierAsync(id, cancellationToken);
        return detail is null
            ? Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, ApiErrorCodes.ResourceNotFound, "Supplier not found.", "Supplier not found."))
            : Results.Ok(detail);
    }

    private static async Task<IResult> CreateSupplierAsync(ClaimsPrincipal principal, CreateSupplierRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage suppliers.", () => commands.CreateSupplierAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateSupplierAsync(ClaimsPrincipal principal, Guid id, UpdateSupplierRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage suppliers.", () => commands.UpdateSupplierAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListSupplierAgreementsAsync(ClaimsPrincipal principal, [AsParameters] SupplierAgreementListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read supplier agreements.");
        }

        return Results.Ok(await queries.ListSupplierAgreementsAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateSupplierAgreementAsync(ClaimsPrincipal principal, CreateSupplierAgreementRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage supplier agreements.", () => commands.CreateSupplierAgreementAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateSupplierAgreementAsync(ClaimsPrincipal principal, Guid id, UpdateSupplierAgreementRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage supplier agreements.", () => commands.UpdateSupplierAgreementAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListConfigurationAuditsAsync(ClaimsPrincipal principal, [AsParameters] ConfigurationAuditListQuery query, IOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (!permissionMatrix.HasPermission(principal, Permissions.Operations.Read))
        {
            return Forbidden("You do not have permission to read configuration audits.");
        }

        return Results.Ok(await queries.ListConfigurationAuditsAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateConfigurationAuditAsync(ClaimsPrincipal principal, CreateConfigurationAuditRequest request, IOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Operations.Manage, "You do not have permission to manage configuration audits.", () => commands.CreateConfigurationAuditAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> ExecuteAsync<T>(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission, string forbiddenDetail, Func<Task<OperationsCommandResult<T>>> action, int successStatusCode = StatusCodes.Status200OK)
    {
        if (!permissionMatrix.HasPermission(principal, permission))
        {
            return Forbidden(forbiddenDetail);
        }

        var result = await action();
        return result.Status switch
        {
            OperationsCommandStatus.Success when successStatusCode == StatusCodes.Status201Created => Results.Created(string.Empty, result.Value),
            OperationsCommandStatus.Success => Results.Ok(result.Value),
            OperationsCommandStatus.NotFound => Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, result.ErrorCode ?? ApiErrorCodes.ResourceNotFound, "Resource not found.", result.ErrorMessage)),
            OperationsCommandStatus.ValidationError => Results.BadRequest(ApiProblemDetailsFactory.Create(StatusCodes.Status400BadRequest, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Validation failed.", result.ErrorMessage)),
            OperationsCommandStatus.Conflict => Results.Conflict(ApiProblemDetailsFactory.Create(StatusCodes.Status409Conflict, result.ErrorCode ?? ApiErrorCodes.RequestValidationFailed, "Request conflict.", result.ErrorMessage)),
            _ => Results.Problem(ApiProblemDetailsFactory.Create(StatusCodes.Status500InternalServerError, ApiErrorCodes.InternalFailure, "Request failed.", result.ErrorMessage))
        };
    }

    private static string? ResolveActor(ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.Email)
        ?? principal.FindFirstValue("preferred_username")
        ?? principal.FindFirstValue("sub")
        ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

    private static IResult Forbidden(string detail) =>
        Results.Json(ApiProblemDetailsFactory.Create(StatusCodes.Status403Forbidden, "forbidden", "Forbidden.", detail), statusCode: StatusCodes.Status403Forbidden);
}
