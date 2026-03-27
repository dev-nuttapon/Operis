using Microsoft.AspNetCore.Mvc;

namespace Operis_API.Modules.Assessment.Contracts;

public sealed record AssessmentPackageListQuery(
    [FromQuery] Guid? ProjectId,
    [FromQuery] string? ProcessArea,
    [FromQuery] string? Status,
    [FromQuery] string? Search,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 25);

public sealed record AssessmentFindingListQuery(
    [FromQuery] Guid? PackageId,
    [FromQuery] string? Status,
    [FromQuery] string? Search,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 25);

public sealed record ControlCatalogListQuery(
    [FromQuery] Guid? ProjectId,
    [FromQuery] string? ControlSet,
    [FromQuery] string? ProcessArea,
    [FromQuery] string? Status,
    [FromQuery] string? Search,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 25);

public sealed record ControlCoverageListQuery(
    [FromQuery] Guid? ProjectId,
    [FromQuery] string? ControlSet,
    [FromQuery] string? ProcessArea,
    [FromQuery] string? CoverageStatus,
    [FromQuery] string? Search,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 25);

public sealed record ControlMappingListQuery(
    [FromQuery] Guid? ControlId,
    [FromQuery] Guid? ProjectId,
    [FromQuery] string? Status,
    [FromQuery] string? TargetModule,
    [FromQuery] string? Search,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 25);

public sealed record AssessmentEvidenceReferenceResponse(
    string SourceModule,
    string EntityType,
    string EntityId,
    string Title,
    string Status,
    string ProcessArea,
    string Route,
    DateTimeOffset? CapturedAt,
    string? MetadataSummary);

public sealed record AssessmentPackageNoteResponse(
    Guid Id,
    string NoteType,
    string Note,
    string CreatedByUserId,
    DateTimeOffset CreatedAt);

public sealed record AssessmentPackageListItemResponse(
    Guid Id,
    string PackageCode,
    Guid? ProjectId,
    string? ProjectName,
    string? ProjectCode,
    string? ProcessArea,
    string ScopeSummary,
    string Status,
    int EvidenceCount,
    int OpenFindingCount,
    DateTimeOffset UpdatedAt);

public sealed record AssessmentPackageDetailResponse(
    Guid Id,
    string PackageCode,
    Guid? ProjectId,
    string? ProjectName,
    string? ProjectCode,
    string? ProcessArea,
    string ScopeSummary,
    string Status,
    string CreatedByUserId,
    DateTimeOffset? PreparedAt,
    string? PreparedByUserId,
    DateTimeOffset? SharedAt,
    string? SharedByUserId,
    DateTimeOffset? ArchivedAt,
    string? ArchivedByUserId,
    IReadOnlyList<AssessmentEvidenceReferenceResponse> EvidenceReferences,
    IReadOnlyList<AssessmentPackageNoteResponse> Notes,
    IReadOnlyList<AssessmentFindingListItemResponse> Findings,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AssessmentFindingListItemResponse(
    Guid Id,
    Guid PackageId,
    string PackageCode,
    string Title,
    string Severity,
    string Status,
    string EvidenceEntityType,
    string EvidenceEntityId,
    string? OwnerUserId,
    DateTimeOffset UpdatedAt);

public sealed record AssessmentFindingDetailResponse(
    Guid Id,
    Guid PackageId,
    string PackageCode,
    string Title,
    string Description,
    string Severity,
    string Status,
    string EvidenceEntityType,
    string EvidenceEntityId,
    string? EvidenceRoute,
    string? OwnerUserId,
    string? AcceptanceSummary,
    string? ClosureSummary,
    string CreatedByUserId,
    DateTimeOffset? AcceptedAt,
    string? AcceptedByUserId,
    DateTimeOffset? ClosedAt,
    string? ClosedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateAssessmentPackageRequest(
    Guid? ProjectId,
    string? ProcessArea,
    string ScopeSummary);

public sealed record TransitionAssessmentPackageRequest(
    string TargetStatus,
    string? Reason);

public sealed record CreateAssessmentNoteRequest(
    string NoteType,
    string Note);

public sealed record CreateAssessmentFindingRequest(
    Guid PackageId,
    string Title,
    string Description,
    string Severity,
    string EvidenceEntityType,
    string EvidenceEntityId,
    string? EvidenceRoute,
    string? OwnerUserId);

public sealed record TransitionAssessmentFindingRequest(
    string TargetStatus,
    string? Summary);

public sealed record ControlCatalogItemResponse(
    Guid Id,
    string ControlCode,
    string Title,
    string ControlSet,
    string? ProcessArea,
    string Status,
    string? Description,
    Guid? ProjectId,
    string? ProjectName,
    int ActiveMappingCount,
    DateTimeOffset UpdatedAt);

public sealed record CreateControlCatalogItemRequest(
    string ControlCode,
    string Title,
    string ControlSet,
    string? ProcessArea,
    string? Description,
    Guid? ProjectId);

public sealed record UpdateControlCatalogItemRequest(
    string ControlCode,
    string Title,
    string ControlSet,
    string? ProcessArea,
    string Status,
    string? Description,
    Guid? ProjectId);

public sealed record CreateControlMappingRequest(
    Guid ControlId,
    Guid? ProjectId,
    string TargetModule,
    string TargetEntityType,
    string TargetEntityId,
    string TargetRoute,
    string? EvidenceStatus,
    string? Notes);

public sealed record TransitionControlMappingRequest(
    string TargetStatus,
    string? Reason);

public sealed record ControlMappingDetailResponse(
    Guid Id,
    Guid ControlId,
    string ControlCode,
    string ControlTitle,
    Guid? ProjectId,
    string? ProjectName,
    string TargetModule,
    string TargetEntityType,
    string TargetEntityId,
    string TargetRoute,
    string EvidenceStatus,
    string Status,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ControlCoverageItemResponse(
    Guid ControlId,
    string ControlCode,
    string Title,
    string ControlSet,
    string? ProcessArea,
    Guid? ProjectId,
    string? ProjectName,
    string CoverageStatus,
    int ActiveMappingCount,
    int EvidenceCount,
    int GapCount,
    DateTimeOffset GeneratedAt);
