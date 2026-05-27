using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public static class DispatchAssignmentRules
{
    public static IEnumerable<DriverAvailability> FindBlockingDriverAvailability(
        string personId,
        DateTimeOffset? tripStart,
        DateTimeOffset? tripEnd,
        IEnumerable<DriverAvailability> records)
    {
        if (!tripStart.HasValue || string.IsNullOrWhiteSpace(personId))
        {
            yield break;
        }

        var rangeStart = tripStart.Value;
        var rangeEnd = tripEnd ?? tripStart.Value;

        foreach (var record in records)
        {
            if (!string.Equals(record.PersonId, personId.Trim(), StringComparison.Ordinal))
            {
                continue;
            }

            if (!DriverAvailabilityRules.IsBlockingStatus(record.AvailabilityStatus))
            {
                continue;
            }

            if (DriverAvailabilityRules.Overlaps(
                    record.StartsAt,
                    record.EndsAt,
                    rangeStart,
                    rangeEnd))
            {
                yield return record;
            }
        }
    }

    public static IEnumerable<EquipmentAvailability> FindBlockingEquipmentAvailability(
        string vehicleRefKey,
        DateTimeOffset? tripStart,
        DateTimeOffset? tripEnd,
        IEnumerable<EquipmentAvailability> records)
    {
        if (!tripStart.HasValue || string.IsNullOrWhiteSpace(vehicleRefKey))
        {
            yield break;
        }

        var normalizedKey = vehicleRefKey.Trim();
        var rangeStart = tripStart.Value;
        var rangeEnd = tripEnd ?? tripStart.Value;

        foreach (var record in records)
        {
            if (!string.Equals(record.VehicleRefKey, normalizedKey, StringComparison.Ordinal))
            {
                continue;
            }

            if (!EquipmentAvailabilityRules.IsBlockingStatus(record.AvailabilityStatus))
            {
                continue;
            }

            if (EquipmentAvailabilityRules.Overlaps(
                    record.StartsAt,
                    record.EndsAt,
                    rangeStart,
                    rangeEnd))
            {
                yield return record;
            }
        }
    }

    public static IEnumerable<Trip> FindOverlappingDriverTrips(
        Trip targetTrip,
        string personId,
        IEnumerable<Trip> trips)
    {
        if (!targetTrip.ScheduledStartAt.HasValue || string.IsNullOrWhiteSpace(personId))
        {
            yield break;
        }

        var rangeStart = targetTrip.ScheduledStartAt.Value;
        var rangeEnd = targetTrip.ScheduledEndAt ?? targetTrip.ScheduledStartAt.Value;
        var normalizedPersonId = personId.Trim();

        foreach (var other in trips)
        {
            if (other.Id == targetTrip.Id)
            {
                continue;
            }

            if (!string.Equals(other.AssignedDriverPersonId, normalizedPersonId, StringComparison.Ordinal))
            {
                continue;
            }

            if (!DriverAvailabilityRules.TripQualifiesForConflict(other))
            {
                continue;
            }

            if (DriverAvailabilityRules.Overlaps(
                    rangeStart,
                    rangeEnd,
                    other.ScheduledStartAt,
                    other.ScheduledEndAt))
            {
                yield return other;
            }
        }
    }

    public static IEnumerable<Trip> FindOverlappingVehicleTrips(
        Trip targetTrip,
        string vehicleRefKey,
        IEnumerable<Trip> trips)
    {
        if (!targetTrip.ScheduledStartAt.HasValue || string.IsNullOrWhiteSpace(vehicleRefKey))
        {
            yield break;
        }

        var normalizedKey = vehicleRefKey.Trim();
        var rangeStart = targetTrip.ScheduledStartAt.Value;
        var rangeEnd = targetTrip.ScheduledEndAt ?? targetTrip.ScheduledStartAt.Value;

        foreach (var other in trips)
        {
            if (other.Id == targetTrip.Id)
            {
                continue;
            }

            if (!string.Equals(other.VehicleRefKey, normalizedKey, StringComparison.Ordinal))
            {
                continue;
            }

            if (!EquipmentAvailabilityRules.TripQualifiesForConflict(other))
            {
                continue;
            }

            if (EquipmentAvailabilityRules.Overlaps(
                    rangeStart,
                    rangeEnd,
                    other.ScheduledStartAt,
                    other.ScheduledEndAt))
            {
                yield return other;
            }
        }
    }

    public static bool HasBlockingConflicts(
        IReadOnlyList<DriverAvailability> blockingAvailability,
        IReadOnlyList<EquipmentAvailability> blockingEquipment,
        IReadOnlyList<Trip> overlappingTrips) =>
        blockingAvailability.Count > 0
        || blockingEquipment.Count > 0
        || overlappingTrips.Count > 0;
}
