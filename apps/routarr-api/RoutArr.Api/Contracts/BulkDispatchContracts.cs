namespace RoutArr.Api.Contracts;



public sealed record BulkDispatchActionItem(

    Guid TripId,

    string? DriverPersonId,

    string? VehicleRefKey,

    string? DispatchStatus);



public sealed record BulkDispatchPreviewRequest(

    IReadOnlyList<BulkDispatchActionItem> Items);



public sealed record BulkDispatchStatusPreview(

    string? TargetStatus,

    bool CanTransition,

    string? ErrorCode,

    string? ErrorMessage);



public sealed record BulkDispatchItemPreview(

    Guid TripId,

    string TripNumber,

    string Title,

    string CurrentDispatchStatus,

    bool CanApply,

    bool HasBlockingConflicts,

    DispatchAssignmentPreviewResponse? DriverPreview,

    DispatchAssignmentPreviewResponse? VehiclePreview,

    BulkDispatchStatusPreview? StatusPreview);



public sealed record BulkDispatchPreviewSummary(

    int Total,

    int CanApplyCount,

    int BlockedCount);



public sealed record BulkDispatchPreviewResponse(

    BulkDispatchPreviewSummary Summary,

    IReadOnlyList<BulkDispatchItemPreview> Items);



public sealed record BulkDispatchApplyRequest(

    IReadOnlyList<BulkDispatchActionItem> Items,

    bool IgnoreAvailabilityConflicts = false,

    bool IgnoreEligibilityBlocks = false,
    bool IgnoreDispatchabilityBlocks = false,
    bool IgnoreWorkflowGateBlocks = false);



public sealed record BulkDispatchApplyItemResult(

    Guid TripId,

    bool Success,

    string? ErrorCode,

    string? ErrorMessage,

    TripDetailResponse? Trip);



public sealed record BulkDispatchApplySummary(

    int Total,

    int SuccessCount,

    int FailureCount);



public sealed record BulkDispatchApplyResponse(

    BulkDispatchApplySummary Summary,

    IReadOnlyList<BulkDispatchApplyItemResult> Results);


