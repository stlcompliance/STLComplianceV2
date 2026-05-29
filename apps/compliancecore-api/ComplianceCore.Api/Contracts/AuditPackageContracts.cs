namespace ComplianceCore.Api.Contracts;

public sealed record AuditPackageManifestResponse(
    string PackageVersion,
    IReadOnlyList<AuditPackageSectionDescriptor> Sections);

public sealed record AuditPackageSectionDescriptor(
    string Key,
    string FileName,
    string Label,
    string Description);

public sealed record AuditPackageExportResponse(
    Guid PackageId,
    Guid TenantId,
    DateTimeOffset GeneratedAt,
    AuditPackageDateRangeResponse? DateRange,
    AuditPackageCountsResponse Counts,
    IReadOnlyList<AuditEventExportItem> AuditEvents,
    IReadOnlyList<AuditPackageFindingItem> Findings,
    IReadOnlyList<AuditPackageEvaluationRunItem> EvaluationRuns,
    IReadOnlyList<AuditPackageWorkflowGateCheckItem> WorkflowGateChecks,
    IReadOnlyList<AuditPackageWaiverItem> Waivers,
    IReadOnlyList<AuditPackageRulePackItem> RulePacks);

public sealed record AuditPackageDateRangeResponse(
    DateTimeOffset? From,
    DateTimeOffset? To);

public sealed record AuditPackageCountsResponse(
    int AuditEvents,
    int Findings,
    int EvaluationRuns,
    int WorkflowGateChecks,
    int Waivers,
    int RulePacks);

public sealed record AuditEventExportItem(
    Guid AuditEventId,
    Guid? ActorUserId,
    string Action,
    string TargetType,
    string? TargetId,
    string Result,
    string? ReasonCode,
    Guid CorrelationId,
    DateTimeOffset OccurredAt);

public sealed record AuditPackageFindingItem(
    Guid FindingId,
    Guid RulePackId,
    string PackKey,
    Guid? RuleEvaluationRunId,
    string FindingKey,
    string Severity,
    string Status,
    string? RuleKey,
    string? FactKey,
    string Title,
    string Message,
    string ReasonCode,
    DateTimeOffset CreatedAt);

public sealed record AuditPackageEvaluationRunItem(
    Guid EvaluationRunId,
    Guid RulePackId,
    string PackKey,
    Guid? ActorUserId,
    string Status,
    string OverallResult,
    string FactInputsJson,
    string RuleResultsJson,
    Guid? AppliedWaiverId,
    string? AppliedWaiverKey,
    DateTimeOffset CreatedAt);

public sealed record AuditPackageWorkflowGateCheckItem(
    Guid CheckResultId,
    string GateKey,
    Guid RulePackId,
    string PackKey,
    Guid? RuleEvaluationRunId,
    string Outcome,
    string ReasonCode,
    string Message,
    Guid? AppliedWaiverId,
    string? AppliedWaiverKey,
    DateTimeOffset CheckedAt);

public sealed record AuditPackageWaiverItem(
    Guid WaiverId,
    string WaiverKey,
    Guid RulePackId,
    string PackKey,
    string? RuleKey,
    string? GateKey,
    string SubjectScopeKey,
    string ReasonCode,
    string Explanation,
    string Status,
    DateTimeOffset EffectiveAt,
    DateTimeOffset? ExpiresAt,
    Guid? ApprovedByUserId,
    DateTimeOffset? ApprovedAt,
    DateTimeOffset CreatedAt);

public sealed record AuditPackageRulePackItem(
    Guid RulePackId,
    string PackKey,
    string Label,
    string Description,
    int VersionNumber,
    string Status,
    bool IsActive,
    Guid RegulatoryProgramId,
    string ProgramKey,
    bool HasRuleContent,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
