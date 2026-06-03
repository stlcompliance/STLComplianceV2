namespace MaintainArr.Api.Contracts;

public sealed record AssetTelematicsIngestionResponse(
    Guid AssetId,
    string AssetTag,
    string AssetName,
    int TotalCount,
    int Limit,
    int ProcessedCount,
    int IgnoredCount,
    int DefectCount,
    IReadOnlyList<AssetTelematicsIngestionEventResponse> Items);

public sealed record AssetTelematicsIngestionEventResponse(
    Guid InboundEventId,
    Guid SourceEventId,
    string SourceProduct,
    string EventKind,
    string Outcome,
    string Summary,
    string? VehicleRefKey,
    string? TripNumber,
    string? IncidentType,
    string? IncidentSeverity,
    string? DvirResult,
    Guid? CreatedDefectId,
    Guid CorrelationId,
    DateTimeOffset OccurredAt,
    DateTimeOffset CreatedAt);
