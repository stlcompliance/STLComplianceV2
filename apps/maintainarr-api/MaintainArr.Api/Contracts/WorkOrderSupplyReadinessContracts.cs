namespace MaintainArr.Api.Contracts;

public sealed record WorkOrderSupplyReadinessBlockerResponse(
    string ReasonCode,
    string Message,
    string SourceEntityType,
    string SourceEntityId,
    string? RelatedEntityId);

public sealed record WorkOrderLineSupplyReadinessResponse(
    Guid DemandLineId,
    int LineNumber,
    Guid? SupplyarrPartId,
    string PartNumber,
    decimal QuantityRequested,
    string LineStatus,
    string? ReadinessStatus,
    string? ReadinessBasis,
    string? SkipReason,
    decimal? QuantityAvailable,
    DateTimeOffset? CalculatedAt,
    IReadOnlyList<WorkOrderSupplyReadinessBlockerResponse> Blockers);

public sealed record WorkOrderSupplyReadinessResponse(
    Guid WorkOrderId,
    string WorkOrderNumber,
    DateTimeOffset GeneratedAt,
    string OverallReadinessStatus,
    int TotalDemandLines,
    int LinesChecked,
    int LinesReady,
    int LinesBlocked,
    int LinesSkipped,
    IReadOnlyList<WorkOrderLineSupplyReadinessResponse> Lines);
