namespace ComplianceCore.Api.Contracts;

public sealed record RuleChangeEventResponse(
    Guid EventId,
    Guid RulePackId,
    string PackKey,
    string ProgramKey,
    string ChangeType,
    string Summary,
    string? FromStatus,
    string? ToStatus,
    int? FromVersion,
    int? ToVersion,
    string? PreviousContentHash,
    string? NewContentHash,
    string Source,
    Guid? ActorUserId,
    Guid? ScanRunId,
    DateTimeOffset DetectedAt);

public sealed record RuleChangeMonitoringSummaryResponse(
    int TotalEvents,
    int EventsLast24Hours,
    int EventsLast7Days,
    int VersionCreatedCount,
    int StatusChangedCount,
    int ContentUpdatedCount,
    int ScanDetectedCount,
    DateTimeOffset GeneratedAt);

public sealed record PendingRuleChangeScansResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingRuleChangeScanItem> Items);

public sealed record PendingRuleChangeScanItem(
    Guid TenantId,
    Guid RulePackId,
    string PackKey,
    string ProgramKey,
    int VersionNumber,
    string Status,
    string? CurrentContentHash,
    string? SnapshotContentHash);

public sealed record ProcessRuleChangeScanRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record ProcessRuleChangeScanResponse(
    Guid ScanRunId,
    string Status,
    int PacksScannedCount,
    int ChangesDetectedCount,
    IReadOnlyList<RuleChangeEventResponse> DetectedEvents);

public sealed record RuleChangeImpactReportResponse(
    Guid TenantId,
    int TotalImpactedRulePacks,
    int TotalChangeEvents,
    int TotalEvaluationRuns,
    int TotalFindings,
    int TotalWaivers,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<RuleChangeImpactReportItem> RulePacks);

public sealed record RuleChangeImpactReportItem(
    Guid RulePackId,
    string PackKey,
    string ProgramKey,
    string LatestChangeType,
    string LatestSummary,
    int ChangeEventCount,
    int VersionCreatedCount,
    int StatusChangedCount,
    int ContentUpdatedCount,
    int EvaluationRunCount,
    int FindingCount,
    int WaiverCount,
    DateTimeOffset LatestChangedAt);
