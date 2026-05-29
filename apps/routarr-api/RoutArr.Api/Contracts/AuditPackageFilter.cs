namespace RoutArr.Api.Contracts;

public sealed record AuditPackageFilter(
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    string? Action = null,
    string? Result = null,
    string? TargetType = null,
    Guid? ActorUserId = null);

public sealed record AuditPackageAppliedFiltersResponse(
    DateTimeOffset? From,
    DateTimeOffset? To,
    string? Action,
    string? Result,
    string? TargetType,
    Guid? ActorUserId);

public sealed record AuditPackageFilterOptionsResponse(
    IReadOnlyList<string> Actions,
    IReadOnlyList<string> Results,
    IReadOnlyList<string> TargetTypes,
    IReadOnlyList<Guid> ActorUserIds);

public sealed record AuditPackageBreakdownItem(
    string Key,
    int Count);

public sealed record AuditPackageExportSummaryResponse(
    AuditPackageAppliedFiltersResponse Filters,
    AuditPackageCountsResponse Counts,
    IReadOnlyList<AuditPackageBreakdownItem> ByResult,
    IReadOnlyList<AuditPackageBreakdownItem> ByAction,
    DateTimeOffset GeneratedAt);
