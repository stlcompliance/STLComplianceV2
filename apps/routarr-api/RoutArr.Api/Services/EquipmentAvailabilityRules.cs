using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public static class EquipmentAvailabilityRules
{
    public static bool Overlaps(
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        DateTimeOffset? otherStart,
        DateTimeOffset? otherEnd)
    {
        if (!otherStart.HasValue)
        {
            return false;
        }

        var eventStart = otherStart.Value;
        var eventEnd = otherEnd ?? otherStart.Value;
        return rangeStart < eventEnd && rangeEnd > eventStart;
    }

    public static bool IsBlockingStatus(string status) =>
        EquipmentAvailabilityStatuses.BlocksAssignment.Contains(status);

    public static bool RecordOverlapsWindow(
        EquipmentAvailability record,
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd) =>
        record.StartsAt < windowEnd && record.EndsAt > windowStart;

    public static bool TripQualifiesForConflict(Trip trip) =>
        !string.IsNullOrWhiteSpace(trip.VehicleRefKey)
        && TripDispatchStatuses.Active.Contains(trip.DispatchStatus);

    public static IEnumerable<Trip> FindConflictingTrips(
        EquipmentAvailability record,
        IEnumerable<Trip> tripsForVehicle)
    {
        if (!IsBlockingStatus(record.AvailabilityStatus))
        {
            yield break;
        }

        foreach (var trip in tripsForVehicle.Where(TripQualifiesForConflict))
        {
            if (string.Equals(trip.VehicleRefKey, record.VehicleRefKey, StringComparison.Ordinal)
                && Overlaps(record.StartsAt, record.EndsAt, trip.ScheduledStartAt, trip.ScheduledEndAt))
            {
                yield return trip;
            }
        }
    }
}
