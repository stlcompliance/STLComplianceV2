using System.Text.Json.Serialization;

namespace RoutArr.Api.Contracts;

public sealed record TripLoadSummaryResponse(
    Guid LoadId,
    string LoadKey,
    string Description,
    string LoadType,
    string Status,
    int SequenceNumber,
    string OriginLabel,
    string DestinationLabel,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record TripSummaryResponse(
    Guid TripId,
    string TripNumber,
    string Title,
    string DispatchStatus,
    string? AssignedDriverPersonId,
    string? VehicleRefKey,
    DateTimeOffset? ScheduledStartAt,
    DateTimeOffset? ScheduledEndAt,
    int LoadCount,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? AssignedAt,
    DateTimeOffset? AcceptedAt,
    DateTimeOffset? DispatchedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? ClosedAt,
    DateTimeOffset? CancelledAt,
    Guid? VendorOrderId,
    Guid? BrokerOrderId,
    string? DispatchBlockReason,
    string? VendorReadinessStatusSnapshot,
    decimal? VendorQuantityReadySnapshot,
    decimal? VendorOrderedQuantitySnapshot,
    DateTimeOffset? VendorExpectedReadyAtSnapshot,
    DateTimeOffset? VendorConfirmedReadyAtSnapshot,
    DateTimeOffset? DispatchOverrideAt,
    string? DispatchOverrideReason,
    IReadOnlyList<DispatchBlockResponse> DispatchBlocks);

public sealed record TripDetailResponse(
    Guid TripId,
    string TripNumber,
    string Title,
    string Description,
    string DispatchStatus,
    string? AssignedDriverPersonId,
    string? VehicleRefKey,
    DateTimeOffset? ScheduledStartAt,
    DateTimeOffset? ScheduledEndAt,
    IReadOnlyList<TripLoadSummaryResponse> Loads,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? AssignedAt,
    DateTimeOffset? AcceptedAt,
    DateTimeOffset? DispatchedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? ClosedAt,
    DateTimeOffset? CancelledAt,
    Guid? VendorOrderId,
    Guid? BrokerOrderId,
    string? DispatchBlockReason,
    string? VendorReadinessStatusSnapshot,
    decimal? VendorQuantityReadySnapshot,
    decimal? VendorOrderedQuantitySnapshot,
    DateTimeOffset? VendorExpectedReadyAtSnapshot,
    DateTimeOffset? VendorConfirmedReadyAtSnapshot,
    DateTimeOffset? ReleasedForDispatchAt,
    Guid? ReleasedForDispatchByEventId,
    DateTimeOffset? DispatchOverrideAt,
    string? DispatchOverrideByPersonId,
    string? DispatchOverrideReason,
    IReadOnlyList<DispatchBlockResponse> DispatchBlocks,
    TripDispatchReleaseSnapshotResponse? DispatchReleaseSnapshot = null);

public sealed record TripDispatchReleaseSnapshotResponse(
    Guid SnapshotId,
    DateTimeOffset ReleasedAt,
    Guid ReleasedByUserId,
    bool DriverCanAssign,
    bool VehicleCanAssign,
    bool HasMissingExternalData,
    bool HasStaleExternalData,
    string Summary);

public sealed record CreateTripLoadRequest(
    string LoadKey,
    string Description,
    string LoadType,
    int SequenceNumber,
    string OriginLabel,
    string DestinationLabel);

public sealed record CreateTripRequest
{
    [JsonConstructor]
    public CreateTripRequest(
        string Title,
        string Description,
        string? VehicleRefKey,
        Guid? VendorOrderId,
        Guid? BrokerOrderId,
        DateTimeOffset? ScheduledStartAt,
        DateTimeOffset? ScheduledEndAt,
        IReadOnlyList<CreateTripLoadRequest>? Loads)
    {
        this.Title = Title;
        this.Description = Description;
        this.VehicleRefKey = VehicleRefKey;
        this.VendorOrderId = VendorOrderId;
        this.BrokerOrderId = BrokerOrderId;
        this.ScheduledStartAt = ScheduledStartAt;
        this.ScheduledEndAt = ScheduledEndAt;
        this.Loads = Loads;
    }

    public CreateTripRequest(
        string title,
        string description,
        string? vehicleRefKey,
        DateTimeOffset? scheduledStartAt,
        DateTimeOffset? scheduledEndAt,
        IReadOnlyList<CreateTripLoadRequest>? loads)
        : this(
            title,
            description,
            vehicleRefKey,
            null,
            null,
            scheduledStartAt,
            scheduledEndAt,
            loads)
    {
    }

    public string Title { get; init; }

    public string Description { get; init; }

    public string? VehicleRefKey { get; init; }

    public Guid? VendorOrderId { get; init; }

    public Guid? BrokerOrderId { get; init; }

    public DateTimeOffset? ScheduledStartAt { get; init; }

    public DateTimeOffset? ScheduledEndAt { get; init; }

    public IReadOnlyList<CreateTripLoadRequest>? Loads { get; init; }
}

public sealed record AssignTripDriverRequest(
    string DriverPersonId,
    string? DriverDisplayName = null,
    bool IgnoreAvailabilityConflicts = false,
    bool IgnoreEligibilityBlocks = false,
    bool IgnoreWorkflowGateBlocks = false);

public sealed record AssignTripVehicleRequest(
    string? VehicleRefKey,
    bool IgnoreAvailabilityConflicts = false,
    bool IgnoreDispatchabilityBlocks = false,
    bool IgnoreWorkflowGateBlocks = false);

public sealed record UpdateTripDispatchStatusRequest(string DispatchStatus);
