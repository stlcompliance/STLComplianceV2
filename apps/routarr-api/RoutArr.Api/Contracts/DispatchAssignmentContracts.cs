namespace RoutArr.Api.Contracts;

public sealed record DispatchAssignmentAvailabilityConflict(
    Guid AvailabilityId,
    string AvailabilityStatus,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string Reason);

public sealed record DispatchAssignmentTripConflict(
    Guid TripId,
    string TripNumber,
    string Title,
    string DispatchStatus,
    DateTimeOffset? ScheduledStartAt,
    DateTimeOffset? ScheduledEndAt);

public sealed record DispatchAssignmentPreviewRequest(
    Guid TripId,
    string AssignmentKind,
    string? DriverPersonId,
    string? VehicleRefKey);

public sealed record DispatchAssignmentConflictSummary(
    int DriverAvailabilityBlocks,
    int EquipmentAvailabilityBlocks,
    int OverlappingTrips,
    bool EligibilityBlocking,
    bool EligibilityWarning,
    bool DispatchabilityBlocking,
    bool DispatchabilityWarning,
    bool WorkflowGateBlocking,
    bool WorkflowGateWarning);

public sealed record DispatchAssignmentPreviewResponse(
    Guid TripId,
    string AssignmentKind,
    bool CanAssign,
    bool HasBlockingConflicts,
    IReadOnlyList<DispatchAssignmentAvailabilityConflict> BlockingDriverAvailability,
    IReadOnlyList<DispatchAssignmentAvailabilityConflict> BlockingEquipmentAvailability,
    IReadOnlyList<DispatchAssignmentTripConflict> OverlappingTrips,
    DispatchAssignmentEligibilitySummary? DriverEligibility = null,
    DispatchAssignmentDispatchabilitySummary? AssetDispatchability = null,
    DispatchAssignmentWorkflowGateSummary? WorkflowGates = null,
    DispatchAssignmentConflictSummary? ConflictSummary = null,
    IReadOnlyList<string>? ValidationMessages = null,
    string? PrimaryBlockCode = null);

public sealed record DispatchBoardBulkAssignmentItem(
    Guid TripId,
    string AssignmentKind,
    string? DriverPersonId,
    string? VehicleRefKey);

public sealed record DispatchBoardBulkAssignmentPreviewRequest(
    IReadOnlyList<DispatchBoardBulkAssignmentItem> Items);

public sealed record DispatchBoardBulkAssignmentItemPreview(
    Guid TripId,
    string AssignmentKind,
    DispatchAssignmentPreviewResponse Preview);

public sealed record DispatchBoardBulkAssignmentPreviewResponse(
    int ItemCount,
    int CanAssignCount,
    int BlockedCount,
    IReadOnlyList<DispatchBoardBulkAssignmentItemPreview> Items);

public sealed record DispatchAssignmentAuditEntry(
    Guid Id,
    Guid? ActorUserId,
    string Action,
    string TargetType,
    string? TargetId,
    string Result,
    DateTimeOffset OccurredAt);

public sealed record DispatchAssignmentAuditListResponse(
    IReadOnlyList<DispatchAssignmentAuditEntry> Entries);
