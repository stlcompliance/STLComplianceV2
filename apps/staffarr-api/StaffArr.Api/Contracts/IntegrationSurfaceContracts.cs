namespace StaffArr.Api.Contracts;

public sealed record PersonReadinessCheckRequest(Guid PersonId);

public sealed record PersonPermissionCheckRequest(
    Guid PersonId,
    IReadOnlyList<string> PermissionKey);

public sealed record PersonAssignmentCheckRequest(
    Guid PersonId,
    IReadOnlyList<string> PermissionKey);

public sealed record CreateRestrictionRequest(
    Guid PersonId,
    string Reason,
    DateTimeOffset? ExpiresAt);

public sealed record PersonHistoryEventRequest(
    Guid PersonId,
    string Action,
    string Result,
    string? ReasonCode = null,
    string? TargetType = null,
    string? TargetId = null);

public sealed record StaffArrAssignmentCheckResponse(
    Guid PersonId,
    bool CanAssign,
    PersonReadinessResponse Readiness,
    EffectivePermissionProjectionResponse PermissionProjection,
    IReadOnlyList<string> BlockingReasons);

public sealed record StaffArrPersonIntegrationSummaryResponse(
    StaffPersonDetailResponse Person,
    PersonReadinessResponse Readiness,
    EffectivePermissionProjectionResponse PermissionProjection,
    TrainarrPersonTrainingHistoryResponse QualificationsSnapshot,
    PersonnelHistorySummaryResponse HistorySummary,
    IReadOnlyList<ReadinessOverrideResponse> ActiveRestrictions);

public sealed record StaffArrRestrictionSnapshotResponse(
    Guid PersonId,
    IReadOnlyList<ReadinessOverrideResponse> ActiveRestrictions,
    IReadOnlyList<ReadinessBlockerResponse> ReadinessBlockers);

public sealed record StaffArrIntegrationLocationResponse(
    Guid LocationId,
    Guid TenantId,
    string LocationNumber,
    string Name,
    string LocationType,
    Guid? ParentLocationId,
    Guid? SiteOrgUnitId,
    string SiteNameSnapshot,
    string ParentPathSnapshot,
    string Status,
    string AllowedProductUsage = "all");
