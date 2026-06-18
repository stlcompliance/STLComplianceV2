using STLCompliance.Shared.Integration;

namespace StaffArr.Api.Contracts;

public sealed record TimekeepingProfileResponse(
    Guid Id,
    Guid PersonId,
    string WorkerNumber,
    string? DefaultLegalEntityRef,
    string? DefaultSiteRef,
    string? DefaultDepartmentRef,
    string? DefaultPositionRef,
    Guid? DefaultSupervisorPersonId,
    Guid? PayPolicyId,
    string PayrollEligibilityStatus,
    string TimeEntryMode,
    bool OvertimeEligible,
    bool RequiresMealBreakAttestation,
    bool RequiresEndOfShiftAttestation,
    bool AllowMobileClock,
    bool AllowKioskClock,
    bool AllowManualCorrections,
    Guid? DefaultLaborAllocationTemplateId,
    DateOnly EffectiveStartDate,
    DateOnly? EffectiveEndDate,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UpsertTimekeepingProfileRequest(
    Guid PersonId,
    string WorkerNumber,
    string? DefaultLegalEntityRef,
    string? DefaultSiteRef,
    string? DefaultDepartmentRef,
    string? DefaultPositionRef,
    Guid? DefaultSupervisorPersonId,
    Guid? PayPolicyId,
    string PayrollEligibilityStatus,
    string TimeEntryMode,
    bool OvertimeEligible,
    bool RequiresMealBreakAttestation,
    bool RequiresEndOfShiftAttestation,
    bool AllowMobileClock,
    bool AllowKioskClock,
    bool AllowManualCorrections,
    Guid? DefaultLaborAllocationTemplateId,
    DateOnly EffectiveStartDate,
    DateOnly? EffectiveEndDate,
    string Status);

public sealed record PayPolicyResponse(
    Guid Id,
    string Name,
    string? Description,
    string JurisdictionRefs,
    string? RoundingPolicy,
    string? MealBreakPolicy,
    string? RestBreakPolicy,
    string? OvertimePolicy,
    string? DoubleTimePolicy,
    string? HolidayPolicy,
    string? ShiftDifferentialPolicy,
    string? TravelTimePolicy,
    string? StandbyCalloutPolicy,
    string? ApprovalPolicy,
    string? CorrectionPolicy,
    string? AttestationPolicy,
    string? ComplianceRulepackRefs,
    DateOnly EffectiveStartDate,
    DateOnly? EffectiveEndDate,
    string Status);

public sealed record UpsertPayPolicyRequest(
    string Name,
    string? Description,
    string JurisdictionRefs,
    string? RoundingPolicy,
    string? MealBreakPolicy,
    string? RestBreakPolicy,
    string? OvertimePolicy,
    string? DoubleTimePolicy,
    string? HolidayPolicy,
    string? ShiftDifferentialPolicy,
    string? TravelTimePolicy,
    string? StandbyCalloutPolicy,
    string? ApprovalPolicy,
    string? CorrectionPolicy,
    string? AttestationPolicy,
    string? ComplianceRulepackRefs,
    DateOnly EffectiveStartDate,
    DateOnly? EffectiveEndDate,
    string Status);

public sealed record PayCodeResponse(
    Guid Id,
    string Code,
    string DisplayName,
    string Category,
    bool CountsTowardWorkedHours,
    bool CountsTowardOvertimeBase,
    bool RequiresAllocation,
    bool RequiresApproval,
    bool RequiresReason,
    bool Active,
    DateOnly EffectiveStartDate,
    DateOnly? EffectiveEndDate);

public sealed record UpsertPayCodeRequest(
    string Code,
    string DisplayName,
    string Category,
    bool CountsTowardWorkedHours,
    bool CountsTowardOvertimeBase,
    bool RequiresAllocation,
    bool RequiresApproval,
    bool RequiresReason,
    bool Active,
    DateOnly EffectiveStartDate,
    DateOnly? EffectiveEndDate);

public sealed record CreateClockEventRequest(
    Guid PersonId,
    string SourceProductKey,
    string SourceDeviceType,
    string? SourceDeviceId,
    string EventType,
    DateTimeOffset EventTimestamp,
    string Timezone,
    string? GeoPoint,
    string? SiteRef,
    string? LocationRef,
    string? SourceRef,
    Guid? EnteredByPersonId,
    string? Notes);

public sealed record ClockEventResponse(
    Guid Id,
    Guid PersonId,
    string SourceProductKey,
    string SourceDeviceType,
    string? SourceDeviceId,
    string EventType,
    DateTimeOffset EventTimestamp,
    DateTimeOffset CapturedTimestamp,
    string Timezone,
    string? SiteRef,
    string? LocationRef,
    string? SourceRef,
    string ImmutableAuditHash,
    IReadOnlyList<string> AnomalyFlags);

public sealed record SubmitFieldCompanionClockEventRequest(
    string EventType,
    DateTimeOffset EventTimestamp,
    DateTimeOffset? CapturedAt,
    string Timezone,
    string? SourceDeviceId,
    string? GeoPoint,
    string? SiteRef,
    string? LocationRef,
    string? Notes,
    string IdempotencyKey);

public sealed record FieldCompanionClockEventResponse(
    Guid Id,
    string EventType,
    DateTimeOffset EventTimestamp,
    DateTimeOffset CapturedTimestamp,
    string Timezone,
    string? SourceDeviceId,
    string? GeoPoint,
    string? SiteRef,
    string? LocationRef,
    string? Notes,
    IReadOnlyList<string> AnomalyFlags);

public sealed record FieldCompanionClockStatusResponse(
    string CurrentState,
    FieldCompanionClockEventResponse? LatestEvent,
    IReadOnlyList<FieldCompanionClockEventResponse> RecentEvents);

public sealed record FieldCompanionClockSubmissionResponse(
    Guid ClockEventId,
    bool Created,
    bool ConflictDetected,
    string Status,
    string CurrentState,
    FieldCompanionClockEventResponse Event);

public sealed record WorkSessionResponse(
    Guid Id,
    Guid PersonId,
    DateOnly SessionDate,
    DateTimeOffset StartTime,
    DateTimeOffset? EndTime,
    string Timezone,
    string Status,
    string SourceType,
    string PrimarySourceProductKey,
    string? PrimarySourceRef,
    string? SiteRef,
    string? LocationRef,
    Guid? SupervisorPersonId,
    int CalculatedDurationMinutes,
    int PaidDurationMinutes,
    int UnpaidBreakMinutes,
    bool RequiresReview,
    IReadOnlyList<string> AnomalyFlags,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UpsertWorkSessionRequest(
    Guid PersonId,
    DateOnly SessionDate,
    DateTimeOffset StartTime,
    DateTimeOffset? EndTime,
    string Timezone,
    string Status,
    string SourceType,
    string PrimarySourceProductKey,
    string? PrimarySourceRef,
    string? SiteRef,
    string? LocationRef,
    Guid? SupervisorPersonId,
    int UnpaidBreakMinutes,
    bool RequiresReview);

public sealed record TimeEntryResponse(
    Guid Id,
    Guid PersonId,
    Guid? WorkSessionId,
    Guid TimesheetPeriodId,
    DateOnly EntryDate,
    DateTimeOffset? StartTime,
    DateTimeOffset? EndTime,
    int DurationMinutes,
    Guid PayCodeId,
    Guid? PayPolicyId,
    string Classification,
    string SourceProductKey,
    string? SourceRef,
    string SourceConfidence,
    string? Description,
    bool RequiresApproval,
    string ApprovalStatus,
    string PayrollLockStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<LaborAllocationResponse> Allocations);

public sealed record UpsertTimeEntryRequest(
    Guid PersonId,
    Guid? WorkSessionId,
    Guid TimesheetPeriodId,
    DateOnly EntryDate,
    DateTimeOffset? StartTime,
    DateTimeOffset? EndTime,
    int DurationMinutes,
    Guid PayCodeId,
    Guid? PayPolicyId,
    string Classification,
    string SourceProductKey,
    string? SourceRef,
    string SourceConfidence,
    string? Description,
    bool RequiresApproval,
    string ApprovalStatus,
    IReadOnlyList<UpsertLaborAllocationRequest>? Allocations);

public sealed record LaborAllocationResponse(
    Guid Id,
    decimal AllocationPercent,
    int AllocationMinutes,
    string ProductKey,
    string CostObjectType,
    string CostObjectRef,
    string LegalEntityRef,
    string SiteRef,
    string DepartmentRef,
    string? CustomerRef,
    string? OrderRef,
    string? AssetRef,
    string? WorkOrderRef,
    string? TripRef,
    string? RouteRef,
    string? WarehouseTaskRef,
    string? TrainingSessionRef,
    string? QualityCaseRef,
    string? ProjectRef,
    string? GlDimensionSnapshot);

public sealed record UpsertLaborAllocationRequest(
    decimal AllocationPercent,
    int AllocationMinutes,
    string ProductKey,
    string CostObjectType,
    string CostObjectRef,
    string LegalEntityRef,
    string SiteRef,
    string DepartmentRef,
    string? CustomerRef,
    string? OrderRef,
    string? AssetRef,
    string? WorkOrderRef,
    string? TripRef,
    string? RouteRef,
    string? WarehouseTaskRef,
    string? TrainingSessionRef,
    string? QualityCaseRef,
    string? ProjectRef,
    string? GlDimensionSnapshot);

public sealed record TimesheetPeriodResponse(
    Guid Id,
    Guid PersonId,
    string PayrollCalendarRef,
    DateOnly PeriodStartDate,
    DateOnly PeriodEndDate,
    string Status,
    DateTimeOffset? SubmittedAt,
    DateTimeOffset? ApprovedAt,
    Guid? ApprovedByPersonId,
    DateTimeOffset? PayrollReadyAt,
    DateTimeOffset? ExportedAt,
    int TotalWorkedMinutes,
    int TotalPaidMinutes,
    int TotalUnpaidMinutes,
    int OvertimeMinutes,
    int ExceptionCount,
    string AttestationStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<TimeEntryResponse> Entries);

public sealed record CreateTimesheetPeriodRequest(
    Guid PersonId,
    string PayrollCalendarRef,
    DateOnly PeriodStartDate,
    DateOnly PeriodEndDate);

public sealed record TimeExceptionResponse(
    Guid Id,
    Guid PersonId,
    Guid TimesheetPeriodId,
    Guid? WorkSessionId,
    Guid? TimeEntryId,
    string Severity,
    string ExceptionType,
    string Message,
    string SourceProductKey,
    string? SourceRef,
    string ResolutionStatus,
    Guid? ResolvedByPersonId,
    DateTimeOffset? ResolvedAt,
    string? ResolutionNotes,
    DateTimeOffset CreatedAt);

public sealed record ResolveTimeExceptionRequest(string ResolutionNotes);

public sealed record TimeCorrectionResponse(
    Guid Id,
    Guid PersonId,
    string TargetType,
    Guid TargetId,
    Guid RequestedByPersonId,
    string ReasonCode,
    string ReasonText,
    string OldSnapshot,
    string NewSnapshot,
    string ApprovalStatus,
    Guid? ApprovedByPersonId,
    DateTimeOffset? ApprovedAt,
    DateTimeOffset CreatedAt);

public sealed record CreateTimeCorrectionRequest(
    Guid PersonId,
    string TargetType,
    Guid TargetId,
    Guid RequestedByPersonId,
    string ReasonCode,
    string ReasonText,
    string OldSnapshot,
    string NewSnapshot);

public sealed record ApproveTimeCorrectionRequest(string? ReviewerNotes);

public sealed record TimeAttestationResponse(
    Guid Id,
    Guid PersonId,
    Guid TimesheetPeriodId,
    string AttestationType,
    string StatementText,
    string Response,
    DateTimeOffset AttestedAt,
    Guid AttestedByPersonId,
    string SourceDeviceType,
    string SourceProductKey,
    DateTimeOffset CreatedAt);

public sealed record CreateTimeAttestationRequest(
    Guid PersonId,
    Guid TimesheetPeriodId,
    string AttestationType,
    string StatementText,
    string Response,
    Guid AttestedByPersonId,
    string SourceDeviceType,
    string SourceProductKey);

public sealed record LaborEvidenceIngestRequest(
    Guid TenantId,
    string SourceProductKey,
    string SourceEntityType,
    string SourceEntityId,
    Guid PersonId,
    string ActivityType,
    string? SuggestedPayCodeKey,
    DateTimeOffset? StartedAt,
    DateTimeOffset? EndedAt,
    int? DurationMinutes,
    string Timezone,
    string? SiteRef,
    string? LocationRef,
    string? LegalEntityRef,
    IReadOnlyList<StlProductObjectReference>? CostObjectRefs,
    string Confidence,
    string? Notes,
    DateTimeOffset EmittedAt,
    string IdempotencyKey);

public sealed record LaborEvidenceIngestResponse(
    Guid TimeEntryId,
    Guid TimesheetPeriodId,
    bool Created,
    bool ConflictDetected,
    string Status);

public sealed record PayrollReadySnapshotResponse(
    Guid SnapshotId,
    Guid TenantId,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<PayrollReadyTimesheetResponse> Timesheets);

public sealed record PayrollReadyTimesheetResponse(
    Guid TimesheetPeriodId,
    Guid PersonId,
    string WorkerNumber,
    string? DefaultLegalEntityRef,
    string PayrollCalendarRef,
    DateOnly PeriodStartDate,
    DateOnly PeriodEndDate,
    string Status,
    DateTimeOffset? PayrollReadyAt,
    string SnapshotHash,
    IReadOnlyList<PayrollReadyTimeEntryResponse> Entries);

public sealed record PayrollReadyTimeEntryResponse(
    Guid TimeEntryId,
    DateOnly EntryDate,
    int DurationMinutes,
    string Classification,
    string PayCode,
    string PayCodeDisplayName,
    string SourceProductKey,
    string? SourceRef,
    IReadOnlyList<LaborAllocationResponse> Allocations);
