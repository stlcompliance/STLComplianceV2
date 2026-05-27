using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public static class DispatchWorkflowGateContextBuilder
{
    public static IReadOnlyDictionary<string, string> BuildTripContext(
        Trip trip,
        string assignmentKind,
        string? driverPersonId,
        string? vehicleRefKey)
    {
        var context = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["sourceProduct"] = "routarr",
            ["assignmentKind"] = assignmentKind,
            ["tripId"] = trip.Id.ToString(),
            ["tripNumber"] = trip.TripNumber,
            ["dispatchStatus"] = trip.DispatchStatus,
            ["loadCount"] = trip.Loads.Count.ToString(),
            ["hasHazmatLoad"] = HasHazmatLoad(trip).ToString().ToLowerInvariant(),
        };

        if (!string.IsNullOrWhiteSpace(driverPersonId))
        {
            context["personId"] = driverPersonId.Trim();
        }
        else if (!string.IsNullOrWhiteSpace(trip.AssignedDriverPersonId))
        {
            context["personId"] = trip.AssignedDriverPersonId.Trim();
        }

        if (!string.IsNullOrWhiteSpace(vehicleRefKey))
        {
            context["vehicleRefKey"] = vehicleRefKey.Trim();
        }
        else if (!string.IsNullOrWhiteSpace(trip.VehicleRefKey))
        {
            context["vehicleRefKey"] = trip.VehicleRefKey.Trim();
        }

        if (trip.ScheduledStartAt.HasValue)
        {
            context["scheduledStartAt"] = trip.ScheduledStartAt.Value.ToString("O");
        }

        if (trip.ScheduledEndAt.HasValue)
        {
            context["scheduledEndAt"] = trip.ScheduledEndAt.Value.ToString("O");
        }

        if (trip.Loads.Count > 0)
        {
            context["loadTypes"] = string.Join(
                ",",
                trip.Loads
                    .Select(x => x.LoadType)
                    .Distinct(StringComparer.OrdinalIgnoreCase));
        }

        return context;
    }

    public static bool HasHazmatLoad(Trip trip) =>
        trip.Loads.Any(IsHazmatLoad);

    private static bool IsHazmatLoad(TripLoad load)
    {
        if (ContainsHazmatToken(load.LoadType))
        {
            return true;
        }

        if (ContainsHazmatToken(load.LoadKey))
        {
            return true;
        }

        return ContainsHazmatToken(load.Description);
    }

    private static bool ContainsHazmatToken(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && value.Contains("hazmat", StringComparison.OrdinalIgnoreCase);
}
