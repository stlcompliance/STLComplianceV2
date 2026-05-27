using RoutArr.Api.Contracts;
using RoutArr.Api.Entities;



namespace RoutArr.Api.Services;



public static class BulkDispatchRules

{

    public const int MaxBatchItems = 100;



    public static int NormalizeBatchSize(int? count) =>

        Math.Clamp(count ?? 0, 0, MaxBatchItems);



    public static bool HasAnyAction(string? driverPersonId, string? vehicleRefKey, string? dispatchStatus) =>

        !string.IsNullOrWhiteSpace(driverPersonId)

        || vehicleRefKey is not null

        || !string.IsNullOrWhiteSpace(dispatchStatus);



    public static (bool CanTransition, string? ErrorCode, string? ErrorMessage) PreviewStatusTransition(

        Trip trip,

        string? targetStatus,

        bool canManageAny,

        string? effectiveDriverPersonId)

    {

        if (string.IsNullOrWhiteSpace(targetStatus))

        {

            return (true, null, null);

        }



        var status = targetStatus.Trim();

        if (!TripDispatchStatuses.All.Contains(status))

        {

            return (

                false,

                "trip.invalid_status",

                "Dispatch status must be planned, assigned, dispatched, in_progress, completed, or cancelled.");

        }



        var normalized = status.ToLowerInvariant();

        if (!TripDispatchStatusRules.CanTransition(trip.DispatchStatus, normalized))

        {

            return (

                false,

                "trip.invalid_transition",

                $"Cannot transition trip from {trip.DispatchStatus} to {normalized}.");

        }



        if (string.Equals(normalized, TripDispatchStatuses.Assigned, StringComparison.OrdinalIgnoreCase)

            && string.IsNullOrWhiteSpace(effectiveDriverPersonId))

        {

            return (

                false,

                "trip.driver_required",

                "A driver must be assigned before moving to assigned status.");

        }



        if (string.Equals(normalized, TripDispatchStatuses.Dispatched, StringComparison.OrdinalIgnoreCase)

            && string.IsNullOrWhiteSpace(effectiveDriverPersonId))

        {

            return (

                false,

                "trip.driver_required",

                "A driver must be assigned before dispatch.");

        }



        if (string.Equals(normalized, TripDispatchStatuses.InProgress, StringComparison.OrdinalIgnoreCase)

            && string.IsNullOrWhiteSpace(effectiveDriverPersonId))

        {

            return (

                false,

                "trip.driver_required",

                "A driver must be assigned before starting a trip.");

        }



        if (string.Equals(normalized, TripDispatchStatuses.Cancelled, StringComparison.OrdinalIgnoreCase)

            && !canManageAny)

        {

            return (

                false,

                "auth.forbidden",

                "Trip cancellation requires routarr.dispatch.manage scope.");

        }



        return (true, null, null);

    }



    public static Trip CloneForSimulation(Trip trip) =>

        new()

        {

            Id = trip.Id,

            TenantId = trip.TenantId,

            TripNumber = trip.TripNumber,

            Title = trip.Title,

            Description = trip.Description,

            DispatchStatus = trip.DispatchStatus,

            AssignedDriverPersonId = trip.AssignedDriverPersonId,

            VehicleRefKey = trip.VehicleRefKey,

            ScheduledStartAt = trip.ScheduledStartAt,

            ScheduledEndAt = trip.ScheduledEndAt,

            CreatedByUserId = trip.CreatedByUserId,

            CreatedAt = trip.CreatedAt,

            UpdatedAt = trip.UpdatedAt,

            AssignedAt = trip.AssignedAt,

            DispatchedAt = trip.DispatchedAt,

            StartedAt = trip.StartedAt,

            CompletedAt = trip.CompletedAt,

            CancelledAt = trip.CancelledAt,

        };



    public static void ApplySimulation(BulkDispatchActionItem item, Trip trip)

    {

        if (!string.IsNullOrWhiteSpace(item.DriverPersonId))

        {

            trip.AssignedDriverPersonId = item.DriverPersonId.Trim();

            if (string.Equals(trip.DispatchStatus, TripDispatchStatuses.Planned, StringComparison.OrdinalIgnoreCase))

            {

                trip.DispatchStatus = TripDispatchStatuses.Assigned;

            }

        }



        if (item.VehicleRefKey is not null)

        {

            trip.VehicleRefKey = string.IsNullOrWhiteSpace(item.VehicleRefKey)

                ? null

                : item.VehicleRefKey.Trim();

        }



        if (!string.IsNullOrWhiteSpace(item.DispatchStatus))

        {

            trip.DispatchStatus = item.DispatchStatus.Trim().ToLowerInvariant();

        }

    }

}


