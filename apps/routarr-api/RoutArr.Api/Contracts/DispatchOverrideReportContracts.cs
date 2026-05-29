namespace RoutArr.Api.Contracts;

public sealed record DispatchOverrideReportCountItem(string Key, int Count);

public sealed record DispatchOverrideReportEntry(
    Guid AuditEventId,
    Guid? ActorUserId,
    string Action,
    string TargetType,
    string? TargetId,
    string Result,
    IReadOnlyList<string> OverrideKinds,
    DateTimeOffset OccurredAt);

public sealed record DispatchOverrideReportSummaryResponse(
    DateTimeOffset GeneratedAt,
    string Scope,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    int TotalOverrideCount,
    int DriverAssignmentOverrideCount,
    int VehicleAssignmentOverrideCount,
    IReadOnlyList<DispatchOverrideReportCountItem> OverrideKindCounts,
    IReadOnlyList<DispatchOverrideReportEntry> RecentOverrides);
