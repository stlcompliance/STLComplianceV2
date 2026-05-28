namespace NexArr.Api.Contracts;

public sealed record PlatformAuditPackageFilter(
    Guid? TenantId = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    string? Action = null,
    string? Result = null,
    string? TargetType = null,
    Guid? ActorUserId = null,
    string? ProductKey = null);

public sealed record PlatformAuditPackageAppliedFiltersResponse(
    Guid? TenantId,
    DateTimeOffset? From,
    DateTimeOffset? To,
    string? Action,
    string? Result,
    string? TargetType,
    Guid? ActorUserId,
    string? ProductKey);

public sealed record PlatformAuditPackageFilterOptionsResponse(
    IReadOnlyList<string> Actions,
    IReadOnlyList<string> Results,
    IReadOnlyList<string> TargetTypes,
    IReadOnlyList<string> ProductKeys);

public sealed record PlatformAuditPackageBreakdownItem(
    string Key,
    int Count);

public sealed record PlatformAuditPackageExportSummaryResponse(
    PlatformAuditPackageAppliedFiltersResponse Filters,
    PlatformAuditPackageCountsResponse Counts,
    IReadOnlyList<PlatformAuditPackageBreakdownItem> ByResult,
    IReadOnlyList<PlatformAuditPackageBreakdownItem> ByAction,
    DateTimeOffset GeneratedAt);
