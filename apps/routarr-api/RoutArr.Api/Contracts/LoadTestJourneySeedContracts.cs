namespace RoutArr.Api.Contracts;

public sealed record LoadTestJourneySeedResponse(
    Guid SubjectPersonId,
    Guid TripId,
    bool TripCreated,
    DateTimeOffset? ScheduledStartAt,
    DateTimeOffset? ScheduledEndAt);
