namespace RoutArr.Api.Contracts;

public sealed record LoadTestJourneySeedResponse(
    Guid SubjectPersonId,
    Guid TripId,
    bool TripCreated,
    DateTimeOffset? ScheduledStartAt,
    DateTimeOffset? ScheduledEndAt);

public sealed record LoadTestJourneyTripResponse(
    Guid SubjectPersonId,
    Guid TripId,
    string TripTitle,
    DateTimeOffset? ScheduledStartAt,
    DateTimeOffset? ScheduledEndAt);
