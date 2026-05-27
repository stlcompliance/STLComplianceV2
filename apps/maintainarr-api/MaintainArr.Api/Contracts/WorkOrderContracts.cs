namespace MaintainArr.Api.Contracts;

public sealed record WorkOrderSummaryResponse(
    Guid WorkOrderId,
    string WorkOrderNumber,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    Guid? DefectId,
    Guid? PmScheduleId,
    string Title,
    string Priority,
    string Status,
    string Source,
    string? AssignedTechnicianPersonId,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? CancelledAt);

public sealed record WorkOrderDetailResponse(
    Guid WorkOrderId,
    string WorkOrderNumber,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    Guid? DefectId,
    string? DefectTitle,
    Guid? PmScheduleId,
    string? PmScheduleName,
    string Title,
    string Description,
    string Priority,
    string Status,
    string Source,
    string? AssignedTechnicianPersonId,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? CancelledAt);

public sealed record CreateWorkOrderRequest(
    Guid AssetId,
    string Title,
    string Description,
    string Priority,
    string? AssignedTechnicianPersonId,
    Guid? PmScheduleId);

public sealed record CreateWorkOrderFromDefectRequest(
    string? Title,
    string? Description,
    string? Priority,
    string? AssignedTechnicianPersonId);

public sealed record UpdateWorkOrderRequest(
    string? Title,
    string? Description,
    string? Priority,
    string? AssignedTechnicianPersonId);

public sealed record UpdateWorkOrderStatusRequest(string Status);

public sealed record PmWorkOrderGenerationResult(
    Guid WorkOrderId,
    string WorkOrderNumber,
    bool LinkedExisting);
