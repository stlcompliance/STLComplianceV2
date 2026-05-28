using RoutArr.Api.Contracts;
using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public static class TripCompletionRollupBuilder
{
    public static TripCompletionRollupComputation Build(
        Trip trip,
        IReadOnlyList<DispatchRoute> routes,
        DateTimeOffset asOfUtc)
    {
        var stops = routes.SelectMany(x => x.Stops).ToList();
        var routeCount = routes.Count;
        var completedRouteCount = routes.Count(x =>
            string.Equals(x.RouteStatus, RouteStatuses.Completed, StringComparison.OrdinalIgnoreCase));
        var stopCount = stops.Count;
        var completedStopCount = stops.Count(x =>
            string.Equals(x.StopStatus, RouteStopStatuses.Completed, StringComparison.OrdinalIgnoreCase));
        var skippedStopCount = stops.Count(x =>
            string.Equals(x.StopStatus, RouteStopStatuses.Skipped, StringComparison.OrdinalIgnoreCase));
        var pendingStopCount = stopCount - completedStopCount - skippedStopCount;
        var loadCount = trip.Loads.Count;
        var deliveredLoadCount = trip.Loads.Count(x =>
            string.Equals(x.Status, TripLoadStatuses.Delivered, StringComparison.OrdinalIgnoreCase));
        var pendingLoadCount = loadCount - deliveredLoadCount;

        var events = BuildEvents(trip, routes, stops);
        var durationMinutes = TripCompletionRollupRules.ComputeDurationMinutes(trip.StartedAt, trip.CompletedAt);

        var summary = new TripCompletionSummaryResponse(
            trip.Id,
            trip.TripNumber,
            trip.Title,
            trip.DispatchStatus,
            trip.AssignedDriverPersonId,
            trip.VehicleRefKey,
            trip.ScheduledStartAt,
            trip.ScheduledEndAt,
            trip.StartedAt,
            trip.CompletedAt,
            trip.CancelledAt,
            durationMinutes,
            routeCount,
            completedRouteCount,
            stopCount,
            completedStopCount,
            skippedStopCount,
            pendingStopCount,
            loadCount,
            deliveredLoadCount,
            pendingLoadCount,
            trip.UpdatedAt,
            asOfUtc,
            IsMaterialized: false);

        return new TripCompletionRollupComputation(summary, events);
    }

    private static IReadOnlyList<TripCompletionEventResponse> BuildEvents(
        Trip trip,
        IReadOnlyList<DispatchRoute> routes,
        IReadOnlyList<RouteStop> stops)
    {
        var events = new List<(DateTimeOffset OccurredAt, int Order, TripCompletionEventResponse Event)>();

        AddTripMilestone(events, trip.AssignedAt, 10, TripCompletionEventKinds.TripAssigned, "Driver assigned", trip);
        AddTripMilestone(events, trip.DispatchedAt, 20, TripCompletionEventKinds.TripDispatched, "Trip dispatched", trip);
        AddTripMilestone(events, trip.StartedAt, 30, TripCompletionEventKinds.TripStarted, "Trip started", trip);

        if (string.Equals(trip.DispatchStatus, TripDispatchStatuses.Completed, StringComparison.OrdinalIgnoreCase))
        {
            AddTripMilestone(
                events,
                trip.CompletedAt,
                90,
                TripCompletionEventKinds.TripCompleted,
                "Trip completed",
                trip);
        }

        if (string.Equals(trip.DispatchStatus, TripDispatchStatuses.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            AddTripMilestone(
                events,
                trip.CancelledAt,
                95,
                TripCompletionEventKinds.TripCancelled,
                "Trip cancelled",
                trip);
        }

        foreach (var route in routes.OrderBy(x => x.RouteNumber))
        {
            if (string.Equals(route.RouteStatus, RouteStatuses.Completed, StringComparison.OrdinalIgnoreCase)
                && route.CompletedAt.HasValue)
            {
                events.Add((
                    route.CompletedAt.Value,
                    50,
                    new TripCompletionEventResponse(
                        TripCompletionEventKinds.RouteCompleted,
                        $"Route {route.RouteNumber} completed",
                        route.Title,
                        route.CompletedAt.Value,
                        0,
                        "route",
                        route.Id.ToString())));
            }
        }

        foreach (var stop in stops.OrderBy(x => x.SequenceNumber))
        {
            if (string.Equals(stop.StopStatus, RouteStopStatuses.Completed, StringComparison.OrdinalIgnoreCase)
                && stop.CompletedAt.HasValue)
            {
                events.Add((
                    stop.CompletedAt.Value,
                    60 + stop.SequenceNumber,
                    new TripCompletionEventResponse(
                        TripCompletionEventKinds.StopCompleted,
                        $"Stop {stop.SequenceNumber} completed",
                        stop.Label,
                        stop.CompletedAt.Value,
                        stop.SequenceNumber,
                        "stop",
                        stop.Id.ToString())));
            }
            else if (string.Equals(stop.StopStatus, RouteStopStatuses.Skipped, StringComparison.OrdinalIgnoreCase))
            {
                var occurredAt = stop.CompletedAt ?? stop.UpdatedAt;
                events.Add((
                    occurredAt,
                    70 + stop.SequenceNumber,
                    new TripCompletionEventResponse(
                        TripCompletionEventKinds.StopSkipped,
                        $"Stop {stop.SequenceNumber} skipped",
                        stop.Label,
                        occurredAt,
                        stop.SequenceNumber,
                        "stop",
                        stop.Id.ToString())));
            }
        }

        return events
            .OrderBy(x => x.OccurredAt)
            .ThenBy(x => x.Order)
            .Select((x, index) => x.Event with { SequenceNumber = index + 1 })
            .ToList();
    }

    private static void AddTripMilestone(
        ICollection<(DateTimeOffset OccurredAt, int Order, TripCompletionEventResponse Event)> events,
        DateTimeOffset? occurredAt,
        int order,
        string eventKind,
        string title,
        Trip trip)
    {
        if (!occurredAt.HasValue)
        {
            return;
        }

        events.Add((
            occurredAt.Value,
            order,
            new TripCompletionEventResponse(
                eventKind,
                title,
                trip.Title,
                occurredAt.Value,
                0,
                "trip",
                trip.Id.ToString())));
    }
}

public sealed record TripCompletionRollupComputation(
    TripCompletionSummaryResponse Summary,
    IReadOnlyList<TripCompletionEventResponse> Events);
