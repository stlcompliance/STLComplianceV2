using RoutArr.Api.Entities;



namespace RoutArr.Api.Services;



public static class DriverAvailabilityRules

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

        DriverAvailabilityStatuses.BlocksAssignment.Contains(status);



    public static bool RecordOverlapsWindow(

        DriverAvailability record,

        DateTimeOffset windowStart,

        DateTimeOffset windowEnd) =>

        record.StartsAt < windowEnd && record.EndsAt > windowStart;



    public static bool TripQualifiesForConflict(Trip trip) =>

        !string.IsNullOrWhiteSpace(trip.AssignedDriverPersonId)

        && TripDispatchStatuses.Active.Contains(trip.DispatchStatus);



    public static IEnumerable<Trip> FindConflictingTrips(

        DriverAvailability record,

        IEnumerable<Trip> tripsForPerson)

    {

        if (!IsBlockingStatus(record.AvailabilityStatus))

        {

            yield break;

        }



        foreach (var trip in tripsForPerson.Where(TripQualifiesForConflict))

        {

            if (Overlaps(record.StartsAt, record.EndsAt, trip.ScheduledStartAt, trip.ScheduledEndAt))

            {

                yield return trip;

            }

        }

    }

}

