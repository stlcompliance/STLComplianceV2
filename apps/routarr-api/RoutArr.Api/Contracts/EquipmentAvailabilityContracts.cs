namespace RoutArr.Api.Contracts;

public sealed record EquipmentAvailabilitySummaryResponse(
    Guid AvailabilityId,
    string VehicleRefKey,
    string AvailabilityStatus,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string Reason,
    string Notes,
    bool HasConflict,
    int ConflictingTripCount,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record EquipmentAvailabilityDetailResponse(
    Guid AvailabilityId,
    string VehicleRefKey,
    string AvailabilityStatus,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string Reason,
    string Notes,
    bool HasConflict,
    IReadOnlyList<EquipmentAvailabilityTripConflictResponse> ConflictingTrips,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record EquipmentAvailabilityTripConflictResponse(
    Guid TripId,
    string TripNumber,
    string Title,
    string DispatchStatus,
    DateTimeOffset? ScheduledStartAt,
    DateTimeOffset? ScheduledEndAt);

public sealed record CreateEquipmentAvailabilityRequest(
    string VehicleRefKey,
    string AvailabilityStatus,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string? Reason,
    string? Notes);

public sealed record UpdateEquipmentAvailabilityRequest(
    string? AvailabilityStatus,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    string? Reason,
    string? Notes);

public sealed record EquipmentAvailabilityPanelSummary(
    int RecordCount,
    int UnavailableCount,
    int LimitedCount,
    int AvailableCount,
    int ConflictCount);

public sealed record EquipmentAvailabilityPanelRow(
    Guid AvailabilityId,
    string VehicleRefKey,
    string AvailabilityStatus,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string Reason,
    bool HasConflict,
    int ConflictingTripCount,
    IReadOnlyList<EquipmentAvailabilityTripConflictResponse> ConflictingTrips);

public sealed record EquipmentAvailabilityPanelResponse(
    string Scope,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    EquipmentAvailabilityPanelSummary Summary,
    IReadOnlyList<EquipmentAvailabilityPanelRow> Records,
    DateTimeOffset GeneratedAt);
