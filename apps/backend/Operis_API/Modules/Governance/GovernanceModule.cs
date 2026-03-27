using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Operis_API.Modules.Governance.Application;
using Operis_API.Modules.Governance.Contracts;
using Operis_API.Shared.Contracts;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;

namespace Operis_API.Modules.Governance;

public sealed class GovernanceModule : IModule
{
    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IGovernanceQueries, GovernanceQueries>();
        services.AddScoped<IGovernanceCommands, GovernanceCommands>();
        services.AddScoped<IGovernanceOperationsQueries, GovernanceOperationsQueries>();
        services.AddScoped<IGovernanceOperationsCommands, GovernanceOperationsCommands>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/governance")
            .WithTags("Governance")
            .RequireAuthorization();

        group.MapGet("/process-assets", ListProcessAssetsAsync);
        group.MapGet("/process-assets/{processAssetId:guid}", GetProcessAssetAsync);
        group.MapPost("/process-assets", CreateProcessAssetAsync);
        group.MapPut("/process-assets/{processAssetId:guid}", UpdateProcessAssetAsync);
        group.MapPost("/process-assets/{processAssetId:guid}/versions", CreateProcessAssetVersionAsync);
        group.MapPut("/process-assets/{processAssetId:guid}/versions/{versionId:guid}", UpdateProcessAssetVersionAsync);
        group.MapPut("/process-assets/{processAssetId:guid}/versions/{versionId:guid}/submit-review", SubmitProcessAssetVersionReviewAsync);
        group.MapPut("/process-assets/{processAssetId:guid}/versions/{versionId:guid}/approve", ApproveProcessAssetVersionAsync);
        group.MapPut("/process-assets/{processAssetId:guid}/versions/{versionId:guid}/activate", ActivateProcessAssetVersionAsync);
        group.MapPut("/process-assets/{processAssetId:guid}/deprecate", DeprecateProcessAssetAsync);

        group.MapGet("/qa-checklists", ListQaChecklistsAsync);
        group.MapGet("/qa-checklists/{qaChecklistId:guid}", GetQaChecklistAsync);
        group.MapPost("/qa-checklists", CreateQaChecklistAsync);
        group.MapPut("/qa-checklists/{qaChecklistId:guid}", UpdateQaChecklistAsync);
        group.MapPut("/qa-checklists/{qaChecklistId:guid}/approve", ApproveQaChecklistAsync);
        group.MapPut("/qa-checklists/{qaChecklistId:guid}/activate", ActivateQaChecklistAsync);
        group.MapPut("/qa-checklists/{qaChecklistId:guid}/deprecate", DeprecateQaChecklistAsync);

        group.MapGet("/project-plans", ListProjectPlansAsync);
        group.MapGet("/project-plans/{projectPlanId:guid}", GetProjectPlanAsync);
        group.MapPost("/project-plans", CreateProjectPlanAsync);
        group.MapPut("/project-plans/{projectPlanId:guid}", UpdateProjectPlanAsync);
        group.MapPut("/project-plans/{projectPlanId:guid}/submit-review", SubmitProjectPlanReviewAsync);
        group.MapPut("/project-plans/{projectPlanId:guid}/approve", ApproveProjectPlanAsync);
        group.MapPut("/project-plans/{projectPlanId:guid}/baseline", BaselineProjectPlanAsync);
        group.MapPut("/project-plans/{projectPlanId:guid}/supersede", SupersedeProjectPlanAsync);

        group.MapGet("/stakeholders", ListStakeholdersAsync);
        group.MapGet("/stakeholders/{stakeholderId:guid}", GetStakeholderAsync);
        group.MapPost("/stakeholders", CreateStakeholderAsync);
        group.MapPut("/stakeholders/{stakeholderId:guid}", UpdateStakeholderAsync);
        group.MapPut("/stakeholders/{stakeholderId:guid}/archive", ArchiveStakeholderAsync);

        group.MapGet("/tailoring-records", ListTailoringRecordsAsync);
        group.MapGet("/tailoring-records/{tailoringRecordId:guid}", GetTailoringRecordAsync);
        group.MapPost("/tailoring-records", CreateTailoringRecordAsync);
        group.MapPut("/tailoring-records/{tailoringRecordId:guid}", UpdateTailoringRecordAsync);
        group.MapPut("/tailoring-records/{tailoringRecordId:guid}/submit", SubmitTailoringRecordAsync);
        group.MapPut("/tailoring-records/{tailoringRecordId:guid}/approve", ApproveTailoringRecordAsync);
        group.MapPut("/tailoring-records/{tailoringRecordId:guid}/apply", ApplyTailoringRecordAsync);
        group.MapPut("/tailoring-records/{tailoringRecordId:guid}/archive", ArchiveTailoringRecordAsync);
        group.MapGet("/raci-maps", ListRaciMapsAsync);
        group.MapPost("/raci-maps", CreateRaciMapAsync);
        group.MapPut("/raci-maps/{id:guid}", UpdateRaciMapAsync);
        group.MapGet("/approval-evidence", ListApprovalEvidenceAsync);
        group.MapGet("/workflow-overrides", ListWorkflowOverridesAsync);
        group.MapPost("/workflow-overrides", RejectWorkflowOverrideMutationAsync);
        group.MapPut("/workflow-overrides/{id:guid}", RejectWorkflowOverrideMutationAsync);
        group.MapGet("/sla-rules", ListSlaRulesAsync);
        group.MapPost("/sla-rules", CreateSlaRuleAsync);
        group.MapPut("/sla-rules/{id:guid}", UpdateSlaRuleAsync);
        group.MapGet("/retention-policies", ListRetentionPoliciesAsync);
        group.MapPost("/retention-policies", CreateRetentionPolicyAsync);
        group.MapPut("/retention-policies/{id:guid}", UpdateRetentionPolicyAsync);

        return endpoints;
    }

    private static async Task<IResult> ListProcessAssetsAsync(
        ClaimsPrincipal principal,
        [AsParameters] GovernanceListQuery query,
        IGovernanceQueries queries,
        IPermissionMatrix permissionMatrix,
        CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Governance.ProcessLibraryRead))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read process assets.");
        }

        return Results.Ok(await queries.ListProcessAssetsAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetProcessAssetAsync(ClaimsPrincipal principal, Guid processAssetId, IGovernanceQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ReadSingleAsync(principal, permissionMatrix, Permissions.Governance.ProcessLibraryRead, "You do not have permission to read process assets.", () => queries.GetProcessAssetAsync(processAssetId, cancellationToken));

    private static async Task<IResult> CreateProcessAssetAsync(ClaimsPrincipal principal, CreateProcessAssetRequest request, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.ProcessLibraryManage, "You do not have permission to manage process assets.", () => commands.CreateProcessAssetAsync(request, cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateProcessAssetAsync(ClaimsPrincipal principal, Guid processAssetId, UpdateProcessAssetRequest request, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.ProcessLibraryManage, "You do not have permission to manage process assets.", () => commands.UpdateProcessAssetAsync(processAssetId, request, cancellationToken));

    private static async Task<IResult> CreateProcessAssetVersionAsync(ClaimsPrincipal principal, Guid processAssetId, CreateProcessAssetVersionRequest request, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.ProcessLibraryManage, "You do not have permission to manage process assets.", () => commands.CreateProcessAssetVersionAsync(processAssetId, request, cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateProcessAssetVersionAsync(ClaimsPrincipal principal, Guid processAssetId, Guid versionId, UpdateProcessAssetVersionRequest request, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.ProcessLibraryManage, "You do not have permission to manage process assets.", () => commands.UpdateProcessAssetVersionAsync(processAssetId, versionId, request, cancellationToken));

    private static async Task<IResult> SubmitProcessAssetVersionReviewAsync(ClaimsPrincipal principal, Guid processAssetId, Guid versionId, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.ProcessLibraryManage, "You do not have permission to manage process assets.", () => commands.SubmitProcessAssetVersionReviewAsync(processAssetId, versionId, cancellationToken));

    private static async Task<IResult> ApproveProcessAssetVersionAsync(ClaimsPrincipal principal, Guid processAssetId, Guid versionId, ProcessAssetApprovalRequest request, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.ProcessLibraryManage, "You do not have permission to manage process assets.", () => commands.ApproveProcessAssetVersionAsync(processAssetId, versionId, ResolveActor(principal) ?? "unknown", request, cancellationToken));

    private static async Task<IResult> ActivateProcessAssetVersionAsync(ClaimsPrincipal principal, Guid processAssetId, Guid versionId, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.ProcessLibraryManage, "You do not have permission to manage process assets.", () => commands.ActivateProcessAssetVersionAsync(processAssetId, versionId, cancellationToken));

    private static async Task<IResult> DeprecateProcessAssetAsync(ClaimsPrincipal principal, Guid processAssetId, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.ProcessLibraryManage, "You do not have permission to manage process assets.", () => commands.DeprecateProcessAssetAsync(processAssetId, cancellationToken));

    private static async Task<IResult> ListQaChecklistsAsync(ClaimsPrincipal principal, [AsParameters] GovernanceListQuery query, IGovernanceQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Governance.QaChecklistRead))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read QA checklists.");
        }

        return Results.Ok(await queries.ListQaChecklistsAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetQaChecklistAsync(ClaimsPrincipal principal, Guid qaChecklistId, IGovernanceQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ReadSingleAsync(principal, permissionMatrix, Permissions.Governance.QaChecklistRead, "You do not have permission to read QA checklists.", () => queries.GetQaChecklistAsync(qaChecklistId, cancellationToken));

    private static async Task<IResult> CreateQaChecklistAsync(ClaimsPrincipal principal, CreateQaChecklistRequest request, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.QaChecklistManage, "You do not have permission to manage QA checklists.", () => commands.CreateQaChecklistAsync(request, cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateQaChecklistAsync(ClaimsPrincipal principal, Guid qaChecklistId, UpdateQaChecklistRequest request, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.QaChecklistManage, "You do not have permission to manage QA checklists.", () => commands.UpdateQaChecklistAsync(qaChecklistId, request, cancellationToken));

    private static async Task<IResult> ApproveQaChecklistAsync(ClaimsPrincipal principal, Guid qaChecklistId, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.QaChecklistManage, "You do not have permission to manage QA checklists.", () => commands.ApproveQaChecklistAsync(qaChecklistId, cancellationToken));

    private static async Task<IResult> ActivateQaChecklistAsync(ClaimsPrincipal principal, Guid qaChecklistId, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.QaChecklistManage, "You do not have permission to manage QA checklists.", () => commands.ActivateQaChecklistAsync(qaChecklistId, cancellationToken));

    private static async Task<IResult> DeprecateQaChecklistAsync(ClaimsPrincipal principal, Guid qaChecklistId, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.QaChecklistManage, "You do not have permission to manage QA checklists.", () => commands.DeprecateQaChecklistAsync(qaChecklistId, cancellationToken));

    private static async Task<IResult> ListProjectPlansAsync(ClaimsPrincipal principal, [AsParameters] GovernanceListQuery query, IGovernanceQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Governance.ProjectPlanRead))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read project plans.");
        }

        return Results.Ok(await queries.ListProjectPlansAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetProjectPlanAsync(ClaimsPrincipal principal, Guid projectPlanId, IGovernanceQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ReadSingleAsync(principal, permissionMatrix, Permissions.Governance.ProjectPlanRead, "You do not have permission to read project plans.", () => queries.GetProjectPlanAsync(projectPlanId, cancellationToken));

    private static async Task<IResult> CreateProjectPlanAsync(ClaimsPrincipal principal, CreateProjectPlanRequest request, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.ProjectPlanManage, "You do not have permission to manage project plans.", () => commands.CreateProjectPlanAsync(request, cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateProjectPlanAsync(ClaimsPrincipal principal, Guid projectPlanId, UpdateProjectPlanRequest request, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.ProjectPlanManage, "You do not have permission to manage project plans.", () => commands.UpdateProjectPlanAsync(projectPlanId, request, cancellationToken));

    private static async Task<IResult> SubmitProjectPlanReviewAsync(ClaimsPrincipal principal, Guid projectPlanId, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.ProjectPlanManage, "You do not have permission to manage project plans.", () => commands.SubmitProjectPlanReviewAsync(projectPlanId, cancellationToken));

    private static async Task<IResult> ApproveProjectPlanAsync(ClaimsPrincipal principal, Guid projectPlanId, ProjectPlanApprovalRequest request, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.ProjectPlanApprove, "You do not have permission to approve project plans.", () => commands.ApproveProjectPlanAsync(projectPlanId, ResolveActor(principal) ?? "unknown", request, cancellationToken));

    private static async Task<IResult> BaselineProjectPlanAsync(ClaimsPrincipal principal, Guid projectPlanId, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.ProjectPlanApprove, "You do not have permission to approve project plans.", () => commands.BaselineProjectPlanAsync(projectPlanId, cancellationToken));

    private static async Task<IResult> SupersedeProjectPlanAsync(ClaimsPrincipal principal, Guid projectPlanId, ProjectPlanApprovalRequest request, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.ProjectPlanApprove, "You do not have permission to approve project plans.", () => commands.SupersedeProjectPlanAsync(projectPlanId, ResolveActor(principal) ?? "unknown", request, cancellationToken));

    private static async Task<IResult> ListStakeholdersAsync(ClaimsPrincipal principal, [AsParameters] GovernanceListQuery query, IGovernanceQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Governance.StakeholderRead))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read stakeholders.");
        }

        return Results.Ok(await queries.ListStakeholdersAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetStakeholderAsync(ClaimsPrincipal principal, Guid stakeholderId, IGovernanceQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ReadSingleAsync(principal, permissionMatrix, Permissions.Governance.StakeholderRead, "You do not have permission to read stakeholders.", () => queries.GetStakeholderAsync(stakeholderId, cancellationToken));

    private static async Task<IResult> CreateStakeholderAsync(ClaimsPrincipal principal, CreateStakeholderRequest request, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.StakeholderManage, "You do not have permission to manage stakeholders.", () => commands.CreateStakeholderAsync(request, cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateStakeholderAsync(ClaimsPrincipal principal, Guid stakeholderId, UpdateStakeholderRequest request, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.StakeholderManage, "You do not have permission to manage stakeholders.", () => commands.UpdateStakeholderAsync(stakeholderId, request, cancellationToken));

    private static async Task<IResult> ArchiveStakeholderAsync(ClaimsPrincipal principal, Guid stakeholderId, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.StakeholderManage, "You do not have permission to manage stakeholders.", () => commands.ArchiveStakeholderAsync(stakeholderId, cancellationToken));

    private static async Task<IResult> ListTailoringRecordsAsync(ClaimsPrincipal principal, [AsParameters] GovernanceListQuery query, IGovernanceQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Governance.TailoringRead))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read tailoring records.");
        }

        return Results.Ok(await queries.ListTailoringRecordsAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetTailoringRecordAsync(ClaimsPrincipal principal, Guid tailoringRecordId, IGovernanceQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ReadSingleAsync(principal, permissionMatrix, Permissions.Governance.TailoringRead, "You do not have permission to read tailoring records.", () => queries.GetTailoringRecordAsync(tailoringRecordId, cancellationToken));

    private static async Task<IResult> CreateTailoringRecordAsync(ClaimsPrincipal principal, CreateTailoringRecordRequest request, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.TailoringManage, "You do not have permission to manage tailoring records.", () => commands.CreateTailoringRecordAsync(request, cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateTailoringRecordAsync(ClaimsPrincipal principal, Guid tailoringRecordId, UpdateTailoringRecordRequest request, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.TailoringManage, "You do not have permission to manage tailoring records.", () => commands.UpdateTailoringRecordAsync(tailoringRecordId, request, cancellationToken));

    private static async Task<IResult> SubmitTailoringRecordAsync(ClaimsPrincipal principal, Guid tailoringRecordId, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.TailoringManage, "You do not have permission to manage tailoring records.", () => commands.SubmitTailoringRecordAsync(tailoringRecordId, cancellationToken));

    private static async Task<IResult> ApproveTailoringRecordAsync(ClaimsPrincipal principal, Guid tailoringRecordId, TailoringDecisionRequest request, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.TailoringApprove, "You do not have permission to approve tailoring records.", () => commands.ApproveTailoringRecordAsync(tailoringRecordId, ResolveActor(principal) ?? "unknown", request, cancellationToken));

    private static async Task<IResult> ApplyTailoringRecordAsync(ClaimsPrincipal principal, Guid tailoringRecordId, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.TailoringManage, "You do not have permission to manage tailoring records.", () => commands.ApplyTailoringRecordAsync(tailoringRecordId, cancellationToken));

    private static async Task<IResult> ArchiveTailoringRecordAsync(ClaimsPrincipal principal, Guid tailoringRecordId, IGovernanceCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.TailoringManage, "You do not have permission to manage tailoring records.", () => commands.ArchiveTailoringRecordAsync(tailoringRecordId, cancellationToken));

    private static async Task<IResult> ListRaciMapsAsync(ClaimsPrincipal principal, [AsParameters] RaciMapListQuery query, IGovernanceOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Governance.RaciRead))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read RACI maps.");
        }

        return Results.Ok(await queries.ListRaciMapsAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateRaciMapAsync(ClaimsPrincipal principal, CreateRaciMapRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.RaciManage, "You do not have permission to manage RACI maps.", () => commands.CreateRaciMapAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateRaciMapAsync(ClaimsPrincipal principal, Guid id, UpdateRaciMapRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.RaciManage, "You do not have permission to manage RACI maps.", () => commands.UpdateRaciMapAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListApprovalEvidenceAsync(ClaimsPrincipal principal, [AsParameters] ApprovalEvidenceListQuery query, IGovernanceOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Governance.ApprovalEvidenceRead))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read approval evidence.");
        }

        return Results.Ok(await queries.ListApprovalEvidenceAsync(query, cancellationToken));
    }

    private static async Task<IResult> ListWorkflowOverridesAsync(ClaimsPrincipal principal, [AsParameters] WorkflowOverrideListQuery query, IGovernanceOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Governance.OverrideLogRead))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read workflow override logs.");
        }

        return Results.Ok(await queries.ListWorkflowOverridesAsync(query, cancellationToken));
    }

    private static Task<IResult> RejectWorkflowOverrideMutationAsync() =>
        Task.FromResult<IResult>(ConflictWithCode("Workflow override logs are read-only.", ApiErrorCodes.OverrideLogMutationForbidden));

    private static async Task<IResult> ListSlaRulesAsync(ClaimsPrincipal principal, [AsParameters] SlaRuleListQuery query, IGovernanceOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Governance.SlaRead))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read SLA rules.");
        }

        return Results.Ok(await queries.ListSlaRulesAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateSlaRuleAsync(ClaimsPrincipal principal, CreateSlaRuleRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.SlaManage, "You do not have permission to manage SLA rules.", () => commands.CreateSlaRuleAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateSlaRuleAsync(ClaimsPrincipal principal, Guid id, UpdateSlaRuleRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.SlaManage, "You do not have permission to manage SLA rules.", () => commands.UpdateSlaRuleAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListRetentionPoliciesAsync(ClaimsPrincipal principal, [AsParameters] RetentionPolicyListQuery query, IGovernanceOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Governance.RetentionRead))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read retention policies.");
        }

        return Results.Ok(await queries.ListRetentionPoliciesAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateRetentionPolicyAsync(ClaimsPrincipal principal, CreateRetentionPolicyRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.RetentionManage, "You do not have permission to manage retention policies.", () => commands.CreateRetentionPolicyAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateRetentionPolicyAsync(ClaimsPrincipal principal, Guid id, UpdateRetentionPolicyRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.RetentionManage, "You do not have permission to manage retention policies.", () => commands.UpdateRetentionPolicyAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ReadSingleAsync<T>(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission, string forbiddenDetail, Func<Task<T?>> loader)
        where T : class
    {
        if (LacksPermission(principal, permissionMatrix, permission))
        {
            return ForbiddenWithCode("Forbidden.", forbiddenDetail);
        }

        var item = await loader();
        return item is null ? NotFoundWithCode() : Results.Ok(item);
    }

    private static async Task<IResult> ExecuteAsync<T>(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission, string forbiddenDetail, Func<Task<GovernanceCommandResult<T>>> action, int successStatusCode = StatusCodes.Status200OK)
    {
        if (LacksPermission(principal, permissionMatrix, permission))
        {
            return ForbiddenWithCode("Forbidden.", forbiddenDetail);
        }

        var result = await action();
        return result.Status switch
        {
            GovernanceCommandStatus.Success when successStatusCode == StatusCodes.Status201Created => Results.Created(string.Empty, result.Value),
            GovernanceCommandStatus.Success => Results.Ok(result.Value),
            GovernanceCommandStatus.NotFound => NotFoundWithCode(result.ErrorMessage, result.ErrorCode),
            GovernanceCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
            GovernanceCommandStatus.Conflict => ConflictWithCode(result.ErrorMessage, result.ErrorCode),
            _ => ProblemWithCode("Request failed.", result.ErrorMessage, result.ErrorCode, StatusCodes.Status500InternalServerError, ApiErrorCodes.InternalFailure)
        };
    }

    private static string? ResolveActor(ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.Email)
        ?? principal.FindFirstValue("preferred_username")
        ?? principal.FindFirstValue("sub")
        ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

    private static bool LacksPermission(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, string permission) =>
        !permissionMatrix.HasPermission(principal, permission);

    private static IResult ForbiddenWithCode(string title, string detail) =>
        Results.Json(ApiProblemDetailsFactory.Create(StatusCodes.Status403Forbidden, "forbidden", title, detail), statusCode: StatusCodes.Status403Forbidden);

    private static IResult BadRequestWithCode(string? detail, string? code = null) =>
        Results.BadRequest(ApiProblemDetailsFactory.Create(StatusCodes.Status400BadRequest, code ?? ApiErrorCodes.RequestValidationFailed, "Validation failed.", detail));

    private static IResult ConflictWithCode(string? detail, string? code = null) =>
        Results.Conflict(ApiProblemDetailsFactory.Create(StatusCodes.Status409Conflict, code ?? ApiErrorCodes.RequestValidationFailed, "Request conflict.", detail));

    private static IResult NotFoundWithCode(string? detail = null, string? code = null) =>
        Results.NotFound(ApiProblemDetailsFactory.Create(StatusCodes.Status404NotFound, code ?? ApiErrorCodes.ResourceNotFound, "Resource not found.", detail));

    private static IResult ProblemWithCode(string? title, string? detail, string? code, int? statusCode, string fallbackCode) =>
        Results.Problem(ApiProblemDetailsFactory.Create(statusCode ?? StatusCodes.Status500InternalServerError, code ?? fallbackCode, title ?? "Request failed.", detail));
}
