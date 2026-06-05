namespace NexArr.Api.Contracts;

public sealed record FieldCompanionFieldWorkOrderTaskLine(
    Guid TaskLineId,
    string Title,
    string Description,
    int SortOrder,
    string Status,
    DateTimeOffset? CompletedAt);

public sealed record FieldCompanionFieldWorkOrderLaborEntry(
    Guid LaborEntryId,
    string PersonId,
    decimal HoursWorked,
    string LaborTypeKey,
    string? Notes,
    DateTimeOffset LoggedAt);

public sealed record FieldCompanionFieldWorkOrderDetailResponse(
    string TaskKey,
    string ProductKey,
    Guid WorkOrderId,
    string WorkOrderNumber,
    string AssetTag,
    string AssetName,
    string Title,
    string Description,
    string Priority,
    string Status,
    IReadOnlyList<FieldCompanionFieldWorkOrderTaskLine> Tasks,
    IReadOnlyList<FieldCompanionFieldWorkOrderLaborEntry> LaborEntries);

public sealed record UpdateFieldCompanionFieldWorkOrderStatusRequest(
    string TaskKey,
    string Status);

public sealed record FieldCompanionFieldWorkOrderStatusResponse(
    string TaskKey,
    string ProductKey,
    Guid WorkOrderId,
    string Status,
    DateTimeOffset UpdatedAt);

public sealed record LogFieldCompanionFieldWorkOrderLaborRequest(
    string TaskKey,
    decimal HoursWorked,
    string LaborTypeKey,
    string? Notes,
    Guid? WorkOrderTaskLineId);

public sealed record FieldCompanionFieldWorkOrderLaborResponse(
    string TaskKey,
    string ProductKey,
    Guid WorkOrderId,
    Guid LaborEntryId,
    decimal HoursWorked,
    string LaborTypeKey,
    string Status,
    DateTimeOffset LoggedAt);

public sealed record MaintainArrWorkOrderDetailUpstreamResponse(
    Guid WorkOrderId,
    string WorkOrderNumber,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    string Title,
    string Description,
    string Priority,
    string Status,
    DateTimeOffset UpdatedAt);

public sealed record MaintainArrWorkOrderTaskLineUpstreamResponse(
    Guid TaskLineId,
    Guid WorkOrderId,
    string Title,
    string Description,
    int SortOrder,
    string Status,
    DateTimeOffset? CompletedAt);

public sealed record MaintainArrWorkOrderLaborEntryUpstreamResponse(
    Guid LaborEntryId,
    Guid WorkOrderId,
    Guid? WorkOrderTaskLineId,
    string PersonId,
    decimal HoursWorked,
    string LaborTypeKey,
    string? Notes,
    DateTimeOffset LoggedAt);

public sealed record MaintainArrUpdateWorkOrderStatusUpstreamRequest(string Status);

public sealed record MaintainArrCreateWorkOrderLaborUpstreamRequest(
    string PersonId,
    decimal HoursWorked,
    string LaborTypeKey,
    Guid? WorkOrderTaskLineId,
    string? Notes);
