namespace MaintainArr.Api.Contracts;

public sealed record ComplianceReportCountItem(string Key, int Count);

public sealed record ComplianceReportInspectionTotals(
    int TotalRuns,
    int CompletedRuns,
    int PassedRuns,
    int FailedRuns,
    int InProgressRuns,
    int FailedChecklistAnswers,
    decimal PassRatePercent);

public sealed record ComplianceReportDefectTotals(
    int OpenDefectCount,
    int OpenCriticalCount,
    int OpenHighCount,
    int InspectionSourcedOpenCount,
    int ManualSourcedOpenCount);

public sealed record ComplianceReportPmAdherenceTotals(
    int ActiveScheduleCount,
    int OverdueCount,
    int DueCount,
    int ScheduledCount,
    decimal AdherencePercent);

public sealed record ComplianceReportRegulatoryKeyGroup(
    string ComplianceKey,
    string? MaterialKey,
    int LinkedSubjectCount,
    int InspectionTemplateCount,
    int OpenComplianceIssueCount);

public sealed record ComplianceReportTemplateSummaryItem(
    Guid InspectionTemplateId,
    string TemplateKey,
    string TemplateName,
    int RegulatoryKeyCount,
    int CompletedRunCount,
    int FailedRunCount,
    DateTimeOffset? LastFailedAt,
    bool RequiresAttention);

public sealed record ComplianceReportAttentionItem(
    Guid AssetId,
    string AssetTag,
    string AssetName,
    string? SiteRef,
    string IssueType,
    string Message);

public sealed record ComplianceAlertResponse(
    Guid AssetId,
    string AssetTag,
    string AssetName,
    string? SiteRef,
    string AlertType,
    string Severity,
    string Message);

public sealed record ComplianceReportSummaryResponse(
    DateTimeOffset GeneratedAt,
    ComplianceReportInspectionTotals InspectionTotals,
    ComplianceReportDefectTotals DefectTotals,
    ComplianceReportPmAdherenceTotals PmAdherenceTotals,
    int RegulatoryKeyMirrorCount,
    IReadOnlyList<ComplianceReportRegulatoryKeyGroup> RegulatoryKeyGroups,
    IReadOnlyList<ComplianceReportTemplateSummaryItem> TemplateSummaries,
    IReadOnlyList<ComplianceReportAttentionItem> AttentionItems,
    IReadOnlyList<ComplianceReportCountItem> DefectSeverityCounts);

public sealed record ComplianceReportTemplateDetailResponse(
    Guid InspectionTemplateId,
    string TemplateKey,
    string TemplateName,
    ComplianceReportInspectionTotals InspectionTotals,
    IReadOnlyList<ComplianceReportRegulatoryKeyGroup> RegulatoryKeys,
    IReadOnlyList<ComplianceReportCountItem> RecentRunResultCounts);
