using Microsoft.AspNetCore.Mvc;

namespace Operis_API.Modules.Exceptions.Contracts;

public sealed record WaiverListQuery(
    [FromQuery] Guid? ProjectId,
    [FromQuery] string? ProcessArea,
    [FromQuery] string? Status,
    [FromQuery] bool OnlyExpired = false,
    [FromQuery] string? Search = null,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 25);

public sealed record CompensatingControlResponse(
    Guid Id,
    string ControlCode,
    string Description,
    string OwnerUserId,
    string Status,
    DateTimeOffset UpdatedAt);

public sealed record WaiverReviewResponse(
    Guid Id,
    string ReviewType,
    string OutcomeStatus,
    string ReviewerUserId,
    string? Notes,
    DateTimeOffset ReviewedAt,
    DateTimeOffset? NextReviewAt);

public sealed record WaiverListItemResponse(
    Guid Id,
    string WaiverCode,
    Guid? ProjectId,
    string? ProjectName,
    string ProcessArea,
    string ScopeSummary,
    string RequestedByUserId,
    DateOnly EffectiveFrom,
    DateOnly ExpiresAt,
    bool IsExpired,
    string Status,
    int CompensatingControlCount,
    DateTimeOffset UpdatedAt);

public sealed record WaiverDetailResponse(
    Guid Id,
    string WaiverCode,
    Guid? ProjectId,
    string? ProjectName,
    string ProcessArea,
    string ScopeSummary,
    string RequestedByUserId,
    string Justification,
    DateOnly EffectiveFrom,
    DateOnly ExpiresAt,
    bool IsExpired,
    string Status,
    string? DecisionReason,
    string? DecisionByUserId,
    DateTimeOffset? DecisionAt,
    string? ClosureReason,
    IReadOnlyList<CompensatingControlResponse> CompensatingControls,
    IReadOnlyList<WaiverReviewResponse> Reviews,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CompensatingControlInput(
    string ControlCode,
    string Description,
    string OwnerUserId,
    string Status = "active");

public sealed record CreateWaiverRequest(
    string WaiverCode,
    Guid? ProjectId,
    string ProcessArea,
    string ScopeSummary,
    string RequestedByUserId,
    string Justification,
    DateOnly? EffectiveFrom,
    DateOnly? ExpiresAt,
    IReadOnlyList<CompensatingControlInput>? CompensatingControls);

public sealed record UpdateWaiverRequest(
    Guid? ProjectId,
    string ProcessArea,
    string ScopeSummary,
    string RequestedByUserId,
    string Justification,
    DateOnly? EffectiveFrom,
    DateOnly? ExpiresAt,
    IReadOnlyList<CompensatingControlInput>? CompensatingControls);

public sealed record TransitionWaiverRequest(
    string TargetStatus,
    string? Reason,
    DateTimeOffset? NextReviewAt);
