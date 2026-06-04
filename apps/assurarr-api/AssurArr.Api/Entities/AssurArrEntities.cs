namespace AssurArr.Api.Entities;

public sealed class AssurArrNonconformance
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "moderate";
    public string Status { get; set; } = "draft";
    public string? SourceProduct { get; set; }
    public string? SourceObjectRef { get; set; }
    public string[] AffectedObjectRefs { get; set; } = [];
    public Guid? OwnerPersonId { get; set; }
    public string[] RecordRefs { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public Guid? ClosedByPersonId { get; set; }
    public string? ClosureSummary { get; set; }
    public string NonconformanceType { get; set; } = "other";
    public string Category { get; set; } = "other";
    public string? CustomerImpact { get; set; }
    public string? SupplierImpact { get; set; }
    public string? SafetyImpact { get; set; }
    public string? ComplianceImpact { get; set; }
    public bool RecurrenceFlag { get; set; }
    public string? RepeatOfNonconformanceRef { get; set; }
    public DateTimeOffset? DueAt { get; set; }
}

public sealed class AssurArrQualityHold
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "moderate";
    public string Status { get; set; } = "draft";
    public string? SourceProduct { get; set; }
    public string? SourceObjectRef { get; set; }
    public string[] AffectedObjectRefs { get; set; } = [];
    public Guid? OwnerPersonId { get; set; }
    public string[] RecordRefs { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public Guid? ClosedByPersonId { get; set; }
    public string? ClosureSummary { get; set; }
    public string HoldType { get; set; } = "other";
    public string HoldScope { get; set; } = "full";
    public string? HoldReason { get; set; }
    public string? ReleaseReason { get; set; }
    public string? RejectionReason { get; set; }
    public string? ConditionalReleaseTerms { get; set; }
    public decimal? QuantityHeld { get; set; }
    public string? UnitOfMeasure { get; set; }
    public string? LotNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTimeOffset? PlacedAt { get; set; }
    public Guid? PlacedByPersonId { get; set; }
    public DateTimeOffset? ReleasedAt { get; set; }
    public Guid? ReleasedByPersonId { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}

public sealed class AssurArrCapa
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "moderate";
    public string Status { get; set; } = "draft";
    public string? SourceProduct { get; set; }
    public string? SourceObjectRef { get; set; }
    public string[] AffectedObjectRefs { get; set; } = [];
    public Guid? OwnerPersonId { get; set; }
    public string[] RecordRefs { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public Guid? ClosedByPersonId { get; set; }
    public string? ClosureSummary { get; set; }
    public string CapaType { get; set; } = "corrective";
    public string SourceType { get; set; } = "manual";
    public Guid? SponsorPersonId { get; set; }
    public string? RootCauseSummary { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public string[] RelatedNonconformanceRefs { get; set; } = [];
    public string[] RelatedAuditFindingRefs { get; set; } = [];
}

public sealed class AssurArrQualityAudit
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "moderate";
    public string Status { get; set; } = "draft";
    public string? SourceProduct { get; set; }
    public string? SourceObjectRef { get; set; }
    public string[] AffectedObjectRefs { get; set; } = [];
    public Guid? OwnerPersonId { get; set; }
    public string[] RecordRefs { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public Guid? ClosedByPersonId { get; set; }
    public string? ClosureSummary { get; set; }
    public string AuditType { get; set; } = "internal";
    public string? AuditScope { get; set; }
    public string[] AuditorPersonIds { get; set; } = [];
    public Guid? LeadAuditorPersonId { get; set; }
    public Guid? StaffArrSiteId { get; set; }
    public Guid? StaffArrLocationId { get; set; }
    public string? SupplierRef { get; set; }
    public string? CustomerRef { get; set; }
    public DateTimeOffset? PlannedStartAt { get; set; }
    public DateTimeOffset? PlannedEndAt { get; set; }
    public DateTimeOffset? ActualStartAt { get; set; }
    public DateTimeOffset? ActualEndAt { get; set; }
    public string[] ChecklistRefs { get; set; } = [];
    public string[] FindingRefs { get; set; } = [];
}

public sealed class AssurArrAuditFinding
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "moderate";
    public string Status { get; set; } = "open";
    public string? SourceProduct { get; set; }
    public string? SourceObjectRef { get; set; }
    public string[] AffectedObjectRefs { get; set; } = [];
    public Guid? OwnerPersonId { get; set; }
    public string[] RecordRefs { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public Guid? ClosedByPersonId { get; set; }
    public string? ClosureSummary { get; set; }
    public string FindingType { get; set; } = "observation";
    public string? AuditRef { get; set; }
    public string? NonconformanceRef { get; set; }
    public string? CapaRef { get; set; }
    public DateTimeOffset? DueAt { get; set; }
}

public sealed class AssurArrQualityStatusSnapshot
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "none";
    public string Status { get; set; } = "unknown";
    public string? SourceProduct { get; set; }
    public string? SourceObjectRef { get; set; }
    public string[] AffectedObjectRefs { get; set; } = [];
    public Guid? OwnerPersonId { get; set; }
    public string[] RecordRefs { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public Guid? ClosedByPersonId { get; set; }
    public string? ClosureSummary { get; set; }
    public string TargetProduct { get; set; } = string.Empty;
    public string TargetObjectRef { get; set; } = string.Empty;
    public string QualityStatus { get; set; } = "unknown";
    public string[] ActiveHoldRefs { get; set; } = [];
    public string[] OpenNonconformanceRefs { get; set; } = [];
    public string[] OpenCapaRefs { get; set; } = [];
    public string[] OpenFindingRefs { get; set; } = [];
    public DateTimeOffset? LastReviewedAt { get; set; }
    public Guid? ReviewedByPersonId { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}

public sealed class AssurArrQualityScorecard
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "none";
    public string Status { get; set; } = "draft";
    public string? SourceProduct { get; set; }
    public string? SourceObjectRef { get; set; }
    public string[] AffectedObjectRefs { get; set; } = [];
    public Guid? OwnerPersonId { get; set; }
    public string[] RecordRefs { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public Guid? ClosedByPersonId { get; set; }
    public string? ClosureSummary { get; set; }
    public string TargetType { get; set; } = "other";
    public string TargetRef { get; set; } = string.Empty;
    public DateTimeOffset PeriodStart { get; set; }
    public DateTimeOffset PeriodEnd { get; set; }
    public decimal? OverallScore { get; set; }
    public string QualityStatus { get; set; } = "unknown";
    public string Trend { get; set; } = "unknown";
    public DateTimeOffset GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = "system";
    public Guid? ReviewedByPersonId { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string[] MetricRefs { get; set; } = [];
}

public sealed class AssurArrQualityReview
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "moderate";
    public string Status { get; set; } = "pending";
    public string? SourceProduct { get; set; }
    public string? SourceObjectRef { get; set; }
    public string[] AffectedObjectRefs { get; set; } = [];
    public Guid? OwnerPersonId { get; set; }
    public string[] RecordRefs { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public Guid? ClosedByPersonId { get; set; }
    public string? ClosureSummary { get; set; }
    public string ReviewType { get; set; } = "nonconformance_review";
    public string? SourceReviewRef { get; set; }
    public Guid? ReviewerPersonId { get; set; }
    public DateTimeOffset? RequestedAt { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public DateTimeOffset? DecisionAt { get; set; }
    public string? DecisionReason { get; set; }
    public string[] RequiredEvidenceRefs { get; set; } = [];
    public string[] SubmittedEvidenceRefs { get; set; } = [];
    public string? Notes { get; set; }
}

public sealed class AssurArrQualityRelease
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "none";
    public string Status { get; set; } = "requested";
    public string? SourceProduct { get; set; }
    public string? SourceObjectRef { get; set; }
    public string[] AffectedObjectRefs { get; set; } = [];
    public Guid? OwnerPersonId { get; set; }
    public string[] RecordRefs { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public Guid? ClosedByPersonId { get; set; }
    public string? ClosureSummary { get; set; }
    public string HoldRef { get; set; } = string.Empty;
    public string ReleaseType { get; set; } = "full";
    public Guid? RequestedByPersonId { get; set; }
    public DateTimeOffset? RequestedAt { get; set; }
    public Guid? ApprovedByPersonId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? ExecutedAt { get; set; }
    public string? Conditions { get; set; }
    public DateTimeOffset? ExpirationAt { get; set; }
    public string[] EvidenceRecordRefs { get; set; } = [];
    public string? Notes { get; set; }
}

public sealed class AssurArrTimelineEvent
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string SubjectType { get; set; } = string.Empty;
    public Guid SubjectId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}
