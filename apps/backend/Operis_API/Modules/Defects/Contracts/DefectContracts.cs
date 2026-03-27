using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Defects.Contracts;

public sealed record DefectListQuery(Guid? ProjectId, string? Severity, string? Status, string? OwnerUserId, string? Search, int? Page, int? PageSize);
public sealed record NonConformanceListQuery(Guid? ProjectId, string? Status, string? OwnerUserId, string? Search, int? Page, int? PageSize);

public sealed record DefectListItem(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Code,
    string Title,
    string Severity,
    string OwnerUserId,
    string Status,
    string? DetectedInPhase,
    DateTimeOffset UpdatedAt);

public sealed record NonConformanceListItem(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Code,
    string Title,
    string SourceType,
    string OwnerUserId,
    string Status,
    string? CorrectiveActionRef,
    DateTimeOffset UpdatedAt);

public sealed record DefectDetailResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Code,
    string Title,
    string Description,
    string Severity,
    string OwnerUserId,
    string Status,
    string? DetectedInPhase,
    string? ResolutionSummary,
    string? CorrectiveActionRef,
    IReadOnlyList<string> AffectedArtifactRefs,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record NonConformanceDetailResponse(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Code,
    string Title,
    string Description,
    string SourceType,
    string OwnerUserId,
    string Status,
    string? CorrectiveActionRef,
    string? RootCause,
    string? ResolutionSummary,
    string? AcceptedDisposition,
    IReadOnlyList<string> LinkedFindingRefs,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateDefectRequest(Guid ProjectId, string Code, string Title, string Description, string Severity, string OwnerUserId, string? DetectedInPhase, string? CorrectiveActionRef, IReadOnlyList<string>? AffectedArtifactRefs);
public sealed record UpdateDefectRequest(string Title, string Description, string Severity, string OwnerUserId, string? DetectedInPhase, string? CorrectiveActionRef, IReadOnlyList<string>? AffectedArtifactRefs, string Status);
public sealed record ResolveDefectRequest(string ResolutionSummary, string? CorrectiveActionRef);
public sealed record CloseDefectRequest(string ResolutionSummary);

public sealed record CreateNonConformanceRequest(Guid ProjectId, string Code, string Title, string Description, string SourceType, string OwnerUserId, string? CorrectiveActionRef, string? RootCause, IReadOnlyList<string>? LinkedFindingRefs);
public sealed record UpdateNonConformanceRequest(string Title, string Description, string SourceType, string OwnerUserId, string? CorrectiveActionRef, string? RootCause, string? ResolutionSummary, string? AcceptedDisposition, IReadOnlyList<string>? LinkedFindingRefs, string Status);
public sealed record CloseNonConformanceRequest(string? CorrectiveActionRef, string? AcceptedDisposition, string? ResolutionSummary);

public sealed record DefectCommandResponse(Guid Id, string Code, string Status);
public sealed record NonConformanceCommandResponse(Guid Id, string Code, string Status);
