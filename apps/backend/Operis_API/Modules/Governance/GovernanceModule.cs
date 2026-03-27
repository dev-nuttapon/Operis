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
        group.MapGet("/compliance-dashboard", GetComplianceDashboardAsync);
        group.MapGet("/compliance-dashboard/drilldown", GetComplianceDrilldownAsync);
        group.MapPut("/compliance-dashboard/preferences", UpdateComplianceDashboardPreferencesAsync);
        group.MapGet("/management-reviews", ListManagementReviewsAsync);
        group.MapGet("/management-reviews/{id:guid}", GetManagementReviewAsync);
        group.MapPost("/management-reviews", CreateManagementReviewAsync);
        group.MapPut("/management-reviews/{id:guid}", UpdateManagementReviewAsync);
        group.MapPost("/management-reviews/{id:guid}/transition", TransitionManagementReviewAsync);
        group.MapGet("/policies", ListPoliciesAsync);
        group.MapPost("/policies", CreatePolicyAsync);
        group.MapPut("/policies/{id:guid}", UpdatePolicyAsync);
        group.MapPost("/policies/{id:guid}/transition", TransitionPolicyAsync);
        group.MapGet("/policy-campaigns", ListPolicyCampaignsAsync);
        group.MapPost("/policy-campaigns", CreatePolicyCampaignAsync);
        group.MapPut("/policy-campaigns/{id:guid}", UpdatePolicyCampaignAsync);
        group.MapPost("/policy-campaigns/{id:guid}/transition", TransitionPolicyCampaignAsync);
        group.MapGet("/policy-acknowledgements", ListPolicyAcknowledgementsAsync);
        group.MapPost("/policy-acknowledgements", CreatePolicyAcknowledgementAsync);
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
        group.MapGet("/architecture-records", ListArchitectureRecordsAsync);
        group.MapGet("/architecture-records/{id:guid}", GetArchitectureRecordAsync);
        group.MapPost("/architecture-records", CreateArchitectureRecordAsync);
        group.MapPut("/architecture-records/{id:guid}", UpdateArchitectureRecordAsync);
        group.MapGet("/design-reviews", ListDesignReviewsAsync);
        group.MapPost("/design-reviews", CreateDesignReviewAsync);
        group.MapPut("/design-reviews/{id:guid}", UpdateDesignReviewAsync);
        group.MapGet("/integration-reviews", ListIntegrationReviewsAsync);
        group.MapPost("/integration-reviews", CreateIntegrationReviewAsync);
        group.MapPut("/integration-reviews/{id:guid}", UpdateIntegrationReviewAsync);

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

    private static async Task<IResult> GetComplianceDashboardAsync(ClaimsPrincipal principal, [AsParameters] ComplianceDashboardQuery query, IGovernanceOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksAnyPermission(principal, permissionMatrix, Permissions.Governance.ComplianceRead, Permissions.Governance.ComplianceManage))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read compliance dashboard data.");
        }

        return Results.Ok(await queries.GetComplianceDashboardAsync(query, ResolveActor(principal), cancellationToken));
    }

    private static async Task<IResult> GetComplianceDrilldownAsync(ClaimsPrincipal principal, [AsParameters] ComplianceDashboardDrilldownQuery query, IGovernanceOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksAnyPermission(principal, permissionMatrix, Permissions.Governance.ComplianceRead, Permissions.Governance.ComplianceManage))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read compliance dashboard data.");
        }

        return Results.Ok(await queries.GetComplianceDrilldownAsync(query, cancellationToken));
    }

    private static async Task<IResult> UpdateComplianceDashboardPreferencesAsync(ClaimsPrincipal principal, UpdateComplianceDashboardPreferencesRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Governance.ComplianceManage))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to manage compliance dashboard preferences.");
        }

        var userId = ResolveActor(principal) ?? "unknown";
        var result = await commands.UpdateComplianceDashboardPreferencesAsync(request, userId, ResolveActor(principal), cancellationToken);
        return result.Status switch
        {
            GovernanceCommandStatus.Success => Results.Ok(result.Value),
            GovernanceCommandStatus.ValidationError => BadRequestWithCode(result.ErrorMessage, result.ErrorCode),
            GovernanceCommandStatus.NotFound => NotFoundWithCode(result.ErrorMessage, result.ErrorCode),
            GovernanceCommandStatus.Conflict => ConflictWithCode(result.ErrorMessage, result.ErrorCode),
            _ => ProblemWithCode("Request failed.", result.ErrorMessage, result.ErrorCode, StatusCodes.Status500InternalServerError, ApiErrorCodes.InternalFailure)
        };
    }

    private static async Task<IResult> ListManagementReviewsAsync(ClaimsPrincipal principal, [AsParameters] ManagementReviewListQuery query, IGovernanceOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksAnyPermission(principal, permissionMatrix, Permissions.Governance.ManagementReviewRead, Permissions.Governance.ManagementReviewManage, Permissions.Governance.ManagementReviewApprove))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read management reviews.");
        }

        return Results.Ok(await queries.ListManagementReviewsAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetManagementReviewAsync(ClaimsPrincipal principal, Guid id, IGovernanceOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksAnyPermission(principal, permissionMatrix, Permissions.Governance.ManagementReviewRead, Permissions.Governance.ManagementReviewManage, Permissions.Governance.ManagementReviewApprove))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read management reviews.");
        }

        var item = await queries.GetManagementReviewAsync(id, cancellationToken);
        return item is null ? NotFoundWithCode() : Results.Ok(item);
    }

    private static async Task<IResult> CreateManagementReviewAsync(ClaimsPrincipal principal, CreateManagementReviewRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.ManagementReviewManage, "You do not have permission to manage management reviews.", () => commands.CreateManagementReviewAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateManagementReviewAsync(ClaimsPrincipal principal, Guid id, UpdateManagementReviewRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.ManagementReviewManage, "You do not have permission to manage management reviews.", () => commands.UpdateManagementReviewAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> TransitionManagementReviewAsync(ClaimsPrincipal principal, Guid id, TransitionManagementReviewRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        var targetStatus = request.TargetStatus.Trim().ToLowerInvariant();
        var needsApprove = targetStatus is "closed" or "archived";
        if (needsApprove)
        {
            return await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.ManagementReviewApprove, "You do not have permission to approve management reviews.", () => commands.TransitionManagementReviewAsync(id, request, ResolveActor(principal), cancellationToken));
        }

        return await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.ManagementReviewManage, "You do not have permission to manage management reviews.", () => commands.TransitionManagementReviewAsync(id, request, ResolveActor(principal), cancellationToken));
    }

    private static async Task<IResult> ListPoliciesAsync(ClaimsPrincipal principal, [AsParameters] PolicyListQuery query, IGovernanceOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksAnyPermission(principal, permissionMatrix, Permissions.Governance.PolicyRead, Permissions.Governance.PolicyManage, Permissions.Governance.PolicyApprove))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read policies.");
        }

        return Results.Ok(await queries.ListPoliciesAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreatePolicyAsync(ClaimsPrincipal principal, CreatePolicyRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.PolicyManage, "You do not have permission to manage policies.", () => commands.CreatePolicyAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdatePolicyAsync(ClaimsPrincipal principal, Guid id, UpdatePolicyRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.PolicyManage, "You do not have permission to manage policies.", () => commands.UpdatePolicyAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> TransitionPolicyAsync(ClaimsPrincipal principal, Guid id, TransitionPolicyRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.PolicyApprove, "You do not have permission to approve policies.", () => commands.TransitionPolicyAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListPolicyCampaignsAsync(ClaimsPrincipal principal, [AsParameters] PolicyCampaignListQuery query, IGovernanceOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksAnyPermission(principal, permissionMatrix, Permissions.Governance.PolicyRead, Permissions.Governance.PolicyManage, Permissions.Governance.PolicyApprove))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read policy campaigns.");
        }

        return Results.Ok(await queries.ListPolicyCampaignsAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreatePolicyCampaignAsync(ClaimsPrincipal principal, CreatePolicyCampaignRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.PolicyManage, "You do not have permission to manage policy campaigns.", () => commands.CreatePolicyCampaignAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdatePolicyCampaignAsync(ClaimsPrincipal principal, Guid id, UpdatePolicyCampaignRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.PolicyManage, "You do not have permission to manage policy campaigns.", () => commands.UpdatePolicyCampaignAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> TransitionPolicyCampaignAsync(ClaimsPrincipal principal, Guid id, TransitionPolicyCampaignRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.PolicyApprove, "You do not have permission to approve policy campaigns.", () => commands.TransitionPolicyCampaignAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListPolicyAcknowledgementsAsync(ClaimsPrincipal principal, [AsParameters] PolicyAcknowledgementListQuery query, IGovernanceOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksAnyPermission(principal, permissionMatrix, Permissions.Governance.PolicyRead, Permissions.Governance.PolicyManage, Permissions.Governance.PolicyApprove))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read policy acknowledgements.");
        }

        return Results.Ok(await queries.ListPolicyAcknowledgementsAsync(query, ResolveActor(principal), cancellationToken));
    }

    private static async Task<IResult> CreatePolicyAcknowledgementAsync(ClaimsPrincipal principal, CreatePolicyAcknowledgementRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksAnyPermission(principal, permissionMatrix, Permissions.Governance.PolicyRead, Permissions.Governance.PolicyManage, Permissions.Governance.PolicyApprove))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to acknowledge policies.");
        }

        return await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.PolicyRead, "You do not have permission to acknowledge policies.", () => commands.CreatePolicyAcknowledgementAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);
    }

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

    private static async Task<IResult> ListArchitectureRecordsAsync(ClaimsPrincipal principal, [AsParameters] ArchitectureRecordListQuery query, IGovernanceOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Governance.ArchitectureRead))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read architecture records.");
        }

        return Results.Ok(await queries.ListArchitectureRecordsAsync(query, cancellationToken));
    }

    private static async Task<IResult> GetArchitectureRecordAsync(ClaimsPrincipal principal, Guid id, IGovernanceOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ReadSingleAsync(principal, permissionMatrix, Permissions.Governance.ArchitectureRead, "You do not have permission to read architecture records.", () => queries.GetArchitectureRecordAsync(id, cancellationToken));

    private static async Task<IResult> CreateArchitectureRecordAsync(ClaimsPrincipal principal, CreateArchitectureRecordRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.ArchitectureManage, "You do not have permission to manage architecture records.", () => commands.CreateArchitectureRecordAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateArchitectureRecordAsync(ClaimsPrincipal principal, Guid id, UpdateArchitectureRecordRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.ArchitectureManage, "You do not have permission to manage architecture records.", () => commands.UpdateArchitectureRecordAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListDesignReviewsAsync(ClaimsPrincipal principal, [AsParameters] DesignReviewListQuery query, IGovernanceOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Governance.DesignReviewRead))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read design reviews.");
        }

        return Results.Ok(await queries.ListDesignReviewsAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateDesignReviewAsync(ClaimsPrincipal principal, CreateDesignReviewRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.DesignReviewManage, "You do not have permission to manage design reviews.", () => commands.CreateDesignReviewAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateDesignReviewAsync(ClaimsPrincipal principal, Guid id, UpdateDesignReviewRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.DesignReviewManage, "You do not have permission to manage design reviews.", () => commands.UpdateDesignReviewAsync(id, request, ResolveActor(principal), cancellationToken));

    private static async Task<IResult> ListIntegrationReviewsAsync(ClaimsPrincipal principal, [AsParameters] IntegrationReviewListQuery query, IGovernanceOperationsQueries queries, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken)
    {
        if (LacksPermission(principal, permissionMatrix, Permissions.Governance.IntegrationReviewRead))
        {
            return ForbiddenWithCode("Forbidden.", "You do not have permission to read integration reviews.");
        }

        return Results.Ok(await queries.ListIntegrationReviewsAsync(query, cancellationToken));
    }

    private static async Task<IResult> CreateIntegrationReviewAsync(ClaimsPrincipal principal, CreateIntegrationReviewRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.IntegrationReviewManage, "You do not have permission to manage integration reviews.", () => commands.CreateIntegrationReviewAsync(request, ResolveActor(principal), cancellationToken), StatusCodes.Status201Created);

    private static async Task<IResult> UpdateIntegrationReviewAsync(ClaimsPrincipal principal, Guid id, UpdateIntegrationReviewRequest request, IGovernanceOperationsCommands commands, IPermissionMatrix permissionMatrix, CancellationToken cancellationToken) =>
        await ExecuteAsync(principal, permissionMatrix, Permissions.Governance.IntegrationReviewManage, "You do not have permission to manage integration reviews.", () => commands.UpdateIntegrationReviewAsync(id, request, ResolveActor(principal), cancellationToken));

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

    private static bool LacksAnyPermission(ClaimsPrincipal principal, IPermissionMatrix permissionMatrix, params string[] permissions) =>
        !permissions.Any(permission => permissionMatrix.HasPermission(principal, permission));

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
