using RoutArr.Api.Contracts;
using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public static class DispatchCloseoutChecklistRules
{
    public const string DriverAssignedKey = "driver_assigned";

    public const string StopsClosedKey = "stops_closed";

    public const string RoutesClosedKey = "routes_closed";

    public const string ExceptionsClearKey = "exceptions_clear";

    public const string PreTripDvirKey = "pre_trip_dvir";

    public const string PostTripDvirKey = "post_trip_dvir";

    public const string ProofRecordedKey = "proof_recorded";

    public const string TripDispositionReadyKey = "trip_disposition_ready";

    public static DispatchCloseoutTripChecklistResponse BuildTripChecklist(
        Trip trip,
        string tripDisposition,
        int openStopCount,
        int openRouteCount,
        int openExceptionCount,
        bool hasProof,
        bool hasPreTripDvir,
        bool hasPostTripDvir,
        TripCloseoutPlan tripPlan)
    {
        var requiresDriver = RequiresDriverForDisposition(trip, tripDisposition);
        var items = new List<DispatchCloseoutChecklistItem>
        {
            new(
                DriverAssignedKey,
                "Driver assigned",
                !string.IsNullOrWhiteSpace(trip.AssignedDriverPersonId),
                requiresDriver,
                requiresDriver && string.IsNullOrWhiteSpace(trip.AssignedDriverPersonId)
                    ? "Assign a driver before completing this trip."
                    : null),
            new(
                StopsClosedKey,
                "Route stops closed",
                openStopCount == 0,
                Required: true,
                openStopCount > 0
                    ? $"{openStopCount} stop(s) still open on this trip's routes."
                    : null),
            new(
                RoutesClosedKey,
                "Routes closed",
                openRouteCount == 0,
                Required: true,
                openRouteCount > 0
                    ? $"{openRouteCount} route(s) still open for this trip."
                    : null),
            new(
                ExceptionsClearKey,
                "Dispatch exceptions resolved",
                openExceptionCount == 0,
                Required: true,
                openExceptionCount > 0
                    ? $"{openExceptionCount} open exception(s) linked to this trip."
                    : null),
            new(
                PreTripDvirKey,
                "Pre-trip DVIR recorded",
                hasPreTripDvir,
                Required: false,
                hasPreTripDvir ? null : "No pre-trip DVIR on file (recommended before complete closeout)."),
            new(
                PostTripDvirKey,
                "Post-trip DVIR recorded",
                hasPostTripDvir,
                Required: false,
                hasPostTripDvir ? null : "No post-trip DVIR on file (recommended before complete closeout)."),
            new(
                ProofRecordedKey,
                "Trip proof captured",
                hasProof,
                Required: false,
                hasProof ? null : "No proof records on file for this trip."),
            new(
                TripDispositionReadyKey,
                "Trip status can close with selected disposition",
                tripPlan.CanApply,
                Required: true,
                tripPlan.CanApply ? null : tripPlan.BlockMessage),
        };

        var ready = items.Where(x => x.Required).All(x => x.Satisfied);

        return new DispatchCloseoutTripChecklistResponse(
            trip.Id,
            trip.TripNumber,
            trip.DispatchStatus,
            ready,
            items);
    }

    private static bool RequiresDriverForDisposition(Trip trip, string tripDisposition)
    {
        if (!string.Equals(tripDisposition, DispatchCloseoutRules.TripDispositionComplete, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var status = trip.DispatchStatus.Trim().ToLowerInvariant();
        return status is TripDispatchStatuses.Assigned
            or TripDispatchStatuses.Dispatched
            or TripDispatchStatuses.InProgress;
    }
}
