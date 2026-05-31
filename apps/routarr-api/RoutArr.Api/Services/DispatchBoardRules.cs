using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public static class DispatchBoardRules
{
    public static bool IsLateTrip(Trip trip, DateTimeOffset now)
    {
        if (!TripDispatchStatuses.Active.Contains(trip.DispatchStatus))
        {
            return false;
        }

        if (trip.ScheduledStartAt.HasValue && trip.ScheduledStartAt.Value < now)
        {
            return trip.DispatchStatus is TripDispatchStatuses.Planned
                or TripDispatchStatuses.Assigned
                or TripDispatchStatuses.Dispatched;
        }

        if (trip.ScheduledEndAt.HasValue
            && trip.ScheduledEndAt.Value < now
            && trip.DispatchStatus is not TripDispatchStatuses.Completed
                and not TripDispatchStatuses.Cancelled)
        {
            return true;
        }

        return false;
    }

    public static bool IsAtRiskTrip(Trip trip, DateTimeOffset now)
    {
        if (!TripDispatchStatuses.Active.Contains(trip.DispatchStatus))
        {
            return false;
        }

        var riskWindowEnd = now.AddHours(2);

        if (trip.ScheduledEndAt.HasValue
            && trip.ScheduledEndAt.Value >= now
            && trip.ScheduledEndAt.Value <= riskWindowEnd)
        {
            return true;
        }

        if (trip.ScheduledStartAt.HasValue
            && trip.ScheduledStartAt.Value >= now
            && trip.ScheduledStartAt.Value <= riskWindowEnd
            && trip.DispatchStatus is TripDispatchStatuses.Planned or TripDispatchStatuses.Assigned)
        {
            return true;
        }

        return false;
    }

    public static int CountMissingRequiredProof(
        Trip trip,
        TripExecutionSettingsSnapshot settings,
        IReadOnlySet<string>? proofTypes)
    {
        if (!TripDispatchStatuses.Active.Contains(trip.DispatchStatus))
        {
            return 0;
        }

        var missing = 0;
        if (settings.RequirePickupProofBeforeStart
            && trip.DispatchStatus is TripDispatchStatuses.Planned or TripDispatchStatuses.Assigned or TripDispatchStatuses.Dispatched
            && proofTypes?.Contains(TripProofTypes.Pickup) != true)
        {
            missing++;
        }

        if (settings.RequireDeliveryProofBeforeComplete
            && trip.DispatchStatus is not TripDispatchStatuses.Completed and not TripDispatchStatuses.Cancelled
            && proofTypes?.Contains(TripProofTypes.Delivery) != true)
        {
            missing++;
        }

        return missing;
    }
}
