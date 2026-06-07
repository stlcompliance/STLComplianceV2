namespace RoutArr.Api.Contracts;

public sealed record TripEtaResponse(
    Guid TripId,
    DateTimeOffset? Eta,
    string? EtaSource,
    string? Confidence,
    string? Reason,
    DateTimeOffset? UpdatedAt,
    Guid? UpdatedByPersonId);

public sealed record UpdateTripEtaRequest(
    Guid TripId,
    DateTimeOffset? Eta,
    string? EtaSource,
    string? Confidence,
    string? Reason);
