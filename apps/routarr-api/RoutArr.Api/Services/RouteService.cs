using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class RouteService(
    RoutArrDbContext db,
    IRoutArrAuditService audit,
    IntegrationOutboxEnqueueService integrationOutbox,
    DispatchNotificationEnqueueService notificationEnqueueService,
    StaffArrSiteReferenceService staffArrSites)
{
    public async Task<IReadOnlyList<RouteSummaryResponse>> ListAsync(
        Guid tenantId,
        bool viewAll,
        Guid? actorUserId,
        string? actorPersonId,
        Guid? tripId = null,
        string? routeStatus = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.Routes
            .AsNoTracking()
            .Include(x => x.Stops)
            .Include(x => x.Trip)
            .Where(x => x.TenantId == tenantId);

        if (tripId.HasValue)
        {
            query = query.Where(x => x.TripId == tripId.Value);
        }

        if (!string.IsNullOrWhiteSpace(routeStatus))
        {
            query = query.Where(x => x.RouteStatus == routeStatus);
        }

        if (!viewAll && actorUserId.HasValue)
        {
            var personId = actorPersonId?.Trim();
            query = query.Where(x =>
                x.CreatedByUserId == actorUserId.Value
                || (x.Trip != null
                    && (x.Trip.CreatedByUserId == actorUserId.Value
                        || (personId != null
                            && x.Trip.AssignedDriverPersonId != null
                            && x.Trip.AssignedDriverPersonId == personId))));
        }

        var routes = await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return routes.Select(MapSummary).ToList();
    }

    public async Task<RouteDetailResponse> GetAsync(
        Guid tenantId,
        Guid routeId,
        CancellationToken cancellationToken = default)
    {
        var route = await GetRouteEntityAsync(tenantId, routeId, cancellationToken);
        return MapDetail(route);
    }

    public async Task<IReadOnlyList<RouteSummaryResponse>> ListTemplatesAsync(
        Guid tenantId,
        bool viewAll,
        Guid? actorUserId,
        CancellationToken cancellationToken = default)
    {
        var query = db.Routes
            .AsNoTracking()
            .Include(x => x.Stops)
            .Where(x => x.TenantId == tenantId && x.TripId == null);

        if (!viewAll && actorUserId.HasValue)
        {
            query = query.Where(x => x.CreatedByUserId == actorUserId.Value);
        }

        var routes = await query
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        return routes.Select(MapSummary).ToList();
    }

    public async Task<RouteDetailResponse> GetTemplateAsync(
        Guid tenantId,
        Guid routeId,
        CancellationToken cancellationToken = default)
    {
        var route = await GetRouteEntityAsync(tenantId, routeId, cancellationToken);
        if (route.TripId is not null)
        {
            throw new StlApiException(
                "route_template.not_found",
                "Route template was not found.",
                404);
        }

        return MapDetail(route);
    }

    public Task<RouteDetailResponse> CreateTemplateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateRouteTemplateRequest request,
        CancellationToken cancellationToken = default) =>
        CreateAsync(
            tenantId,
            actorUserId,
            new CreateRouteRequest(
                request.Title,
                request.Description,
                null,
                request.Stops),
            cancellationToken);

    public async Task<RouteAccessContext> GetAccessContextAsync(
        Guid tenantId,
        Guid routeId,
        CancellationToken cancellationToken = default)
    {
        var route = await db.Routes
            .AsNoTracking()
            .Include(x => x.Trip)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == routeId, cancellationToken);

        if (route is null)
        {
            throw new StlApiException("route.not_found", "Route was not found.", 404);
        }

        return new RouteAccessContext(
            route.CreatedByUserId,
            route.Trip?.CreatedByUserId,
            route.Trip?.AssignedDriverPersonId);
    }

    public async Task<RouteDetailResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateRouteRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateTitle(request.Title);

        if (request.TripId.HasValue)
        {
            await EnsureTripExistsAsync(tenantId, request.TripId.Value, cancellationToken);
            await EnsureTripNotAlreadyLinkedAsync(tenantId, request.TripId.Value, null, cancellationToken);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new DispatchRoute
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RouteNumber = await GenerateRouteNumberAsync(tenantId, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            RouteStatus = RouteStatuses.Draft,
            TripId = request.TripId,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        if (request.Stops is { Count: > 0 })
        {
            foreach (var stopRequest in request.Stops.OrderBy(x => x.SequenceNumber))
            {
                entity.Stops.Add(await CreateStopEntityAsync(tenantId, entity.Id, stopRequest, now, cancellationToken));
            }
        }

        db.Routes.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "route.create",
            tenantId,
            actorUserId,
            "route",
            entity.Id.ToString(),
            entity.RouteStatus,
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueRouteCreatedAsync(entity, cancellationToken);

        return MapDetail(entity);
    }

    public async Task<RouteDetailResponse> LinkTripAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid routeId,
        LinkRouteTripRequest request,
        CancellationToken cancellationToken = default)
    {
        var route = await db.Routes
            .Include(x => x.Stops)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == routeId, cancellationToken);

        if (route is null)
        {
            throw new StlApiException("route.not_found", "Route was not found.", 404);
        }

        if (!RouteStatuses.Editable.Contains(route.RouteStatus))
        {
            throw new StlApiException(
                "route.not_editable",
                "Routes can only be linked to trips while draft or planned.",
                400);
        }

        await EnsureTripExistsAsync(tenantId, request.TripId, cancellationToken);
        await EnsureTripNotAlreadyLinkedAsync(tenantId, request.TripId, route.Id, cancellationToken);

        route.TripId = request.TripId;
        route.UpdatedAt = DateTimeOffset.UtcNow;

        if (string.Equals(route.RouteStatus, RouteStatuses.Draft, StringComparison.OrdinalIgnoreCase)
            && route.Stops.Count > 0)
        {
            route.RouteStatus = RouteStatuses.Planned;
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "route.link_trip",
            tenantId,
            actorUserId,
            "route",
            route.Id.ToString(),
            request.TripId.ToString(),
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueRouteUpdatedAsync(route, "Route linked to trip", cancellationToken);

        return MapDetail(route);
    }

    public async Task<RouteDetailResponse> ReorderStopsAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid routeId,
        ReorderRouteStopsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.StopIds is not { Count: > 0 })
        {
            throw new StlApiException("route_stop.reorder_required", "At least one stop id is required.", 400);
        }

        var route = await db.Routes
            .Include(x => x.Stops)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == routeId, cancellationToken);

        if (route is null)
        {
            throw new StlApiException("route.not_found", "Route was not found.", 404);
        }

        if (!RouteStatuses.Editable.Contains(route.RouteStatus))
        {
            throw new StlApiException(
                "route.not_editable",
                "Stop order can only be changed while the route is draft or planned.",
                400);
        }

        var stopIds = request.StopIds.ToList();
        if (stopIds.Distinct().Count() != stopIds.Count)
        {
            throw new StlApiException("route_stop.duplicate_ids", "Stop ids must be unique.", 400);
        }

        var routeStopIds = route.Stops.Select(x => x.Id).ToHashSet();
        if (!stopIds.All(routeStopIds.Contains))
        {
            throw new StlApiException(
                "route_stop.invalid_ids",
                "All stop ids must belong to the route.",
                400);
        }

        if (stopIds.Count != route.Stops.Count)
        {
            throw new StlApiException(
                "route_stop.incomplete_order",
                "Reorder must include every stop on the route.",
                400);
        }

        var now = DateTimeOffset.UtcNow;
        for (var index = 0; index < stopIds.Count; index++)
        {
            var stop = route.Stops.First(x => x.Id == stopIds[index]);
            stop.SequenceNumber = index + 1;
            stop.UpdatedAt = now;
        }

        route.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "route.reorder_stops",
            tenantId,
            actorUserId,
            "route",
            route.Id.ToString(),
            string.Join(',', stopIds),
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueRouteUpdatedAsync(route, "Route stops reordered", cancellationToken);

        return MapDetail(route);
    }

    public async Task<RouteDetailResponse> AddStopAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid routeId,
        AddRouteStopRequest request,
        CancellationToken cancellationToken = default)
    {
        var route = await db.Routes
            .Include(x => x.Stops)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == routeId, cancellationToken);

        if (route is null)
        {
            throw new StlApiException("route.not_found", "Route was not found.", 404);
        }

        if (!RouteStatuses.Editable.Contains(route.RouteStatus))
        {
            throw new StlApiException(
                "route.not_editable",
                "Stops can only be added while the route is draft or planned.",
                400);
        }

        var now = DateTimeOffset.UtcNow;
        route.Stops.Add(await CreateStopEntityAsync(
            tenantId,
            route.Id,
            new CreateRouteStopRequest(
                request.StopKey,
                request.Label,
                request.AddressLabel,
                request.StopType,
                request.SequenceNumber,
                request.GeofenceAnchorLatitude,
                request.GeofenceAnchorLongitude,
                request.GeofenceRadiusMeters,
                request.ScheduledArrivalAt,
                request.StaffarrSiteOrgUnitId),
            now,
            cancellationToken));
        route.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "route_stop.add",
            tenantId,
            actorUserId,
            "route_stop",
            route.Stops.Last().Id.ToString(),
            route.Id.ToString(),
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueRouteUpdatedAsync(route, "Route stop added", cancellationToken);

        return MapDetail(route);
    }

    public async Task<RouteStopSummaryResponse> CheckStopGeofenceAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid stopId,
        CheckRouteStopGeofenceRequest request,
        CancellationToken cancellationToken = default)
    {
        var stop = await db.RouteStops
            .Include(x => x.Route)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == stopId, cancellationToken);

        if (stop is null)
        {
            throw new StlApiException("route_stop.not_found", "Route stop was not found.", 404);
        }

        if (stop.GeofenceAnchorLatitude is null || stop.GeofenceAnchorLongitude is null)
        {
            throw new StlApiException(
                "route_stop.geofence_missing",
                "Route stop must have a geofence anchor before it can be checked.",
                400);
        }

        var distanceMeters = CalculateGeofenceDistanceMeters(
            stop.GeofenceAnchorLatitude.Value,
            stop.GeofenceAnchorLongitude.Value,
            request.ReportedLatitude,
            request.ReportedLongitude);
        var radiusMeters = stop.GeofenceRadiusMeters ?? 250;
        var result = distanceMeters <= radiusMeters ? "inside" : distanceMeters <= radiusMeters * 1.5m ? "nearby" : "outside";
        var now = DateTimeOffset.UtcNow;

        stop.LastGeofenceCheckAt = now;
        stop.LastGeofenceResult = result;
        stop.LastGeofenceDistanceMeters = decimal.Round(distanceMeters, 2);
        stop.LastGeofenceReportedLatitude = request.ReportedLatitude;
        stop.LastGeofenceReportedLongitude = request.ReportedLongitude;
        stop.UpdatedAt = now;
        stop.Route.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "route_stop.geofence_check",
            tenantId,
            actorUserId,
            "route_stop",
            stop.Id.ToString(),
            result,
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueRouteUpdatedAsync(stop.Route, "Route stop geofence checked", cancellationToken);

        return MapStop(stop);
    }

    public async Task<RouteDetailResponse> OptimizeStopsAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid routeId,
        CancellationToken cancellationToken = default)
    {
        var route = await db.Routes
            .Include(x => x.Stops)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == routeId, cancellationToken);

        if (route is null)
        {
            throw new StlApiException("route.not_found", "Route was not found.", 404);
        }

        if (!RouteStatuses.Editable.Contains(route.RouteStatus))
        {
            throw new StlApiException(
                "route.not_editable",
                "Route stop order can only be optimized while the route is draft or planned.",
                400);
        }

        var currentOrder = route.Stops
            .OrderBy(x => x.SequenceNumber)
            .ToList();

        if (currentOrder.Count < 2)
        {
            return MapDetail(route);
        }

        var optimizedOrder = BuildOptimizedStopOrder(currentOrder);
        if (currentOrder.Select(x => x.Id).SequenceEqual(optimizedOrder.Select(x => x.Id)))
        {
            return MapDetail(route);
        }

        var now = DateTimeOffset.UtcNow;
        for (var index = 0; index < optimizedOrder.Count; index++)
        {
            var stop = optimizedOrder[index];
            stop.SequenceNumber = index + 1;
            stop.UpdatedAt = now;
        }

        route.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "route.optimize_stops",
            tenantId,
            actorUserId,
            "route",
            route.Id.ToString(),
            string.Join(',', optimizedOrder.Select(x => x.StopKey)),
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueRouteUpdatedAsync(route, "Route stops optimized", cancellationToken);

        return MapDetail(route);
    }

    public async Task<RouteStopSummaryResponse> UpdateStopStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid stopId,
        UpdateRouteStopStatusRequest request,
        bool canManageAny,
        string? actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var status = request.StopStatus?.Trim() ?? string.Empty;
        if (!RouteStopStatuses.All.Contains(status))
        {
            throw new StlApiException(
                "route_stop.invalid_status",
                "Stop status must be pending, arrived, completed, or skipped.",
                400);
        }

        var stop = await db.RouteStops
            .Include(x => x.Route)
            .ThenInclude(x => x!.Trip)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == stopId, cancellationToken);

        if (stop is null)
        {
            throw new StlApiException("route_stop.not_found", "Route stop was not found.", 404);
        }

        var normalized = status.ToLowerInvariant();
        if (!RouteStopStatusRules.CanTransition(stop.StopStatus, normalized))
        {
            throw new StlApiException(
                "route_stop.invalid_transition",
                $"Cannot transition stop from {stop.StopStatus} to {normalized}.",
                400);
        }

        if (!canManageAny)
        {
            EnsureDriverCanUpdateStop(stop.Route, actorUserId, actorPersonId);
        }

        if (normalized is RouteStopStatuses.Arrived or RouteStopStatuses.Completed)
        {
            await EnsurePriorStopsCompleteAsync(stop, normalized, cancellationToken);
        }

        var now = DateTimeOffset.UtcNow;
        var previousStatus = stop.StopStatus;
        stop.StopStatus = normalized;
        stop.UpdatedAt = now;

        if (string.Equals(normalized, RouteStopStatuses.Arrived, StringComparison.OrdinalIgnoreCase))
        {
            stop.ArrivedAt ??= now;
        }

        if (string.Equals(normalized, RouteStopStatuses.Completed, StringComparison.OrdinalIgnoreCase))
        {
            stop.CompletedAt ??= now;
            stop.ArrivedAt ??= now;
        }

        stop.Route.UpdatedAt = now;

        if (string.Equals(normalized, RouteStopStatuses.Arrived, StringComparison.OrdinalIgnoreCase)
            && string.Equals(stop.Route.RouteStatus, RouteStatuses.Planned, StringComparison.OrdinalIgnoreCase))
        {
            stop.Route.RouteStatus = RouteStatuses.Active;
            stop.Route.ActivatedAt ??= now;
        }

        await MaybeCompleteRouteAsync(stop.Route, now, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "route_stop.status",
            tenantId,
            actorUserId,
            "route_stop",
            stop.Id.ToString(),
            normalized,
            cancellationToken: cancellationToken);

        if (!string.Equals(previousStatus, normalized, StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(normalized, RouteStopStatuses.Arrived, StringComparison.OrdinalIgnoreCase))
            {
                await integrationOutbox.TryEnqueueStopArrivedAsync(stop, cancellationToken);
            }
            else if (string.Equals(normalized, RouteStopStatuses.Completed, StringComparison.OrdinalIgnoreCase))
            {
                await integrationOutbox.TryEnqueueStopCompletedAsync(stop, cancellationToken);
            }
            else if (string.Equals(normalized, RouteStopStatuses.Skipped, StringComparison.OrdinalIgnoreCase))
            {
                await integrationOutbox.TryEnqueueStopMissedAsync(stop, cancellationToken);
            }
        }

        return MapStop(stop);
    }

    public async Task<RouteDetailResponse> UpdateRouteStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid routeId,
        UpdateRouteStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var status = request.RouteStatus?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!RouteStatuses.All.Contains(status))
        {
            throw new StlApiException(
                "route.invalid_status",
                "Route status must be draft, planned, active, completed, or cancelled.",
                400);
        }

        var route = await db.Routes
            .Include(x => x.Stops)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == routeId, cancellationToken);

        if (route is null)
        {
            throw new StlApiException("route.not_found", "Route was not found.", 404);
        }

        if (string.Equals(route.RouteStatus, RouteStatuses.Completed, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(status, RouteStatuses.Completed, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "route.completed_terminal",
                "Completed routes cannot be moved to another status.",
                400);
        }

        var previousStatus = route.RouteStatus;
        if (string.Equals(previousStatus, status, StringComparison.OrdinalIgnoreCase))
        {
            return MapDetail(route);
        }

        var now = DateTimeOffset.UtcNow;
        route.RouteStatus = status;
        route.UpdatedAt = now;

        if (string.Equals(status, RouteStatuses.Active, StringComparison.OrdinalIgnoreCase))
        {
            route.ActivatedAt ??= now;
        }
        else if (string.Equals(status, RouteStatuses.Completed, StringComparison.OrdinalIgnoreCase))
        {
            route.CompletedAt ??= now;
        }
        else if (string.Equals(status, RouteStatuses.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            route.CancelledAt ??= now;
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "route.status",
            tenantId,
            actorUserId,
            "route",
            route.Id.ToString(),
            status,
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueRouteUpdatedAsync(route, $"Route status changed to {status}", cancellationToken);

        if (string.Equals(status, RouteStatuses.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            await notificationEnqueueService.TryEnqueueRouteCancelledAsync(route, cancellationToken);
        }

        return MapDetail(route);
    }

    public async Task<IReadOnlyList<RouteStopSummaryResponse>> ListStopsAsync(
        Guid tenantId,
        Guid routeId,
        CancellationToken cancellationToken = default)
    {
        var route = await GetRouteEntityAsync(tenantId, routeId, cancellationToken);
        return route.Stops
            .OrderBy(x => x.SequenceNumber)
            .Select(MapStop)
            .ToList();
    }

    private async Task EnsurePriorStopsCompleteAsync(
        RouteStop stop,
        string targetStatus,
        CancellationToken cancellationToken)
    {
        var priorIncomplete = await db.RouteStops
            .AnyAsync(
                x => x.TenantId == stop.TenantId
                    && x.RouteId == stop.RouteId
                    && x.SequenceNumber < stop.SequenceNumber
                    && !RouteStopStatuses.Terminal.Contains(x.StopStatus),
                cancellationToken);

        if (priorIncomplete)
        {
            throw new StlApiException(
                "route_stop.sequence_blocked",
                "Earlier stops must be completed or skipped before advancing this stop.",
                400);
        }

        if (string.Equals(targetStatus, RouteStopStatuses.Completed, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(stop.StopStatus, RouteStopStatuses.Arrived, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "route_stop.arrival_required",
                "A stop must be marked arrived before it can be completed.",
                400);
        }
    }

    private async Task MaybeCompleteRouteAsync(DispatchRoute route, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var stops = route.Stops.Count > 0
            ? route.Stops
            : await db.RouteStops
                .Where(x => x.TenantId == route.TenantId && x.RouteId == route.Id)
                .ToListAsync(cancellationToken);

        if (stops.Count == 0)
        {
            return;
        }

        if (stops.All(x => RouteStopStatuses.Terminal.Contains(x.StopStatus)))
        {
            route.RouteStatus = RouteStatuses.Completed;
            route.CompletedAt ??= now;
        }
    }

    private static void EnsureDriverCanUpdateStop(DispatchRoute route, Guid actorUserId, string? actorPersonId)
    {
        if (route.Trip is null)
        {
            if (route.CreatedByUserId != actorUserId)
            {
                throw new StlApiException(
                    "auth.forbidden",
                    "You can only update stops on routes you created or are assigned to drive.",
                    403);
            }

            return;
        }

        var isAssignedDriver = !string.IsNullOrWhiteSpace(route.Trip.AssignedDriverPersonId)
            && !string.IsNullOrWhiteSpace(actorPersonId)
            && string.Equals(route.Trip.AssignedDriverPersonId, actorPersonId, StringComparison.Ordinal);

        if (!isAssignedDriver && route.Trip.CreatedByUserId != actorUserId && route.CreatedByUserId != actorUserId)
        {
            throw new StlApiException(
                "auth.forbidden",
                "You can only update stops on routes you created or are assigned to drive.",
                403);
        }
    }

    private async Task EnsureTripExistsAsync(
        Guid tenantId,
        Guid tripId,
        CancellationToken cancellationToken)
    {
        var exists = await db.Trips.AnyAsync(x => x.TenantId == tenantId && x.Id == tripId, cancellationToken);
        if (!exists)
        {
            throw new StlApiException("trip.not_found", "Trip was not found.", 404);
        }
    }

    private async Task EnsureTripNotAlreadyLinkedAsync(
        Guid tenantId,
        Guid tripId,
        Guid? excludingRouteId,
        CancellationToken cancellationToken)
    {
        var linked = await db.Routes.AnyAsync(
            x => x.TenantId == tenantId
                && x.TripId == tripId
                && (!excludingRouteId.HasValue || x.Id != excludingRouteId.Value),
            cancellationToken);

        if (linked)
        {
            throw new StlApiException(
                "route.trip_already_linked",
                "This trip is already linked to another route.",
                400);
        }
    }

    private async Task<DispatchRoute> GetRouteEntityAsync(
        Guid tenantId,
        Guid routeId,
        CancellationToken cancellationToken)
    {
        var route = await db.Routes
            .AsNoTracking()
            .Include(x => x.Stops)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == routeId, cancellationToken);

        if (route is null)
        {
            throw new StlApiException("route.not_found", "Route was not found.", 404);
        }

        return route;
    }

    private async Task<RouteStop> CreateStopEntityAsync(
        Guid tenantId,
        Guid routeId,
        CreateRouteStopRequest request,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        ValidateStopKey(request.StopKey);
        ValidateStopType(request.StopType);
        var site = await staffArrSites.ResolveOptionalSiteAsync(
            tenantId,
            request.StaffarrSiteOrgUnitId,
            cancellationToken);

        return new RouteStop
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RouteId = routeId,
            StopKey = request.StopKey.Trim(),
            Label = request.Label?.Trim() ?? string.Empty,
            AddressLabel = request.AddressLabel?.Trim() ?? string.Empty,
            StaffarrSiteOrgUnitId = site?.OrgUnitId,
            StaffarrSiteNameSnapshot = site?.Name ?? string.Empty,
            StopType = NormalizeStopType(request.StopType),
            StopStatus = RouteStopStatuses.Pending,
            SequenceNumber = request.SequenceNumber,
            GeofenceAnchorLatitude = request.GeofenceAnchorLatitude,
            GeofenceAnchorLongitude = request.GeofenceAnchorLongitude,
            GeofenceRadiusMeters = request.GeofenceRadiusMeters,
            ScheduledArrivalAt = request.ScheduledArrivalAt,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    private async Task<string> GenerateRouteNumberAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var datePart = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            var candidate = $"RT-{datePart}-{suffix}";
            var exists = await db.Routes.AnyAsync(
                x => x.TenantId == tenantId && x.RouteNumber == candidate,
                cancellationToken);
            if (!exists)
            {
                return candidate;
            }
        }

        return $"RT-{datePart}-{Guid.NewGuid():N}".ToUpperInvariant();
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new StlApiException("route.title_required", "Route title is required.", 400);
        }

        if (title.Trim().Length > 256)
        {
            throw new StlApiException("route.title_too_long", "Route title must be 256 characters or fewer.", 400);
        }
    }

    private static void ValidateStopKey(string stopKey)
    {
        if (string.IsNullOrWhiteSpace(stopKey))
        {
            throw new StlApiException("route_stop.key_required", "Stop key is required.", 400);
        }
    }

    private static void ValidateStopType(string stopType)
    {
        if (!RouteStopTypes.All.Contains(stopType))
        {
            throw new StlApiException(
                "route_stop.invalid_type",
                "Stop type must be pickup, delivery, waypoint, or depot.",
                400);
        }
    }

    private static string NormalizeStopType(string stopType) =>
        stopType.Trim().ToLowerInvariant();

    private static RouteSummaryResponse MapSummary(DispatchRoute route) =>
        new(
            route.Id,
            route.RouteNumber,
            route.Title,
            route.RouteStatus,
            route.TripId,
            route.Stops.Count,
            route.CreatedByUserId,
            route.CreatedAt,
            route.UpdatedAt,
            route.ActivatedAt,
            route.CompletedAt,
            route.CancelledAt);

    private static RouteDetailResponse MapDetail(DispatchRoute route) =>
        new(
            route.Id,
            route.RouteNumber,
            route.Title,
            route.Description,
            route.RouteStatus,
            route.TripId,
            route.Stops
                .OrderBy(x => x.SequenceNumber)
                .Select(MapStop)
                .ToList(),
            route.CreatedByUserId,
            route.CreatedAt,
            route.UpdatedAt,
            route.ActivatedAt,
            route.CompletedAt,
            route.CancelledAt);

    private static IReadOnlyList<RouteStop> BuildOptimizedStopOrder(IReadOnlyList<RouteStop> stops)
    {
        var scheduledStops = stops
            .Where(x => x.ScheduledArrivalAt.HasValue)
            .OrderBy(x => x.ScheduledArrivalAt)
            .ThenBy(x => x.SequenceNumber)
            .ThenBy(x => x.StopKey, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var unscheduledStops = stops
            .Where(x => !x.ScheduledArrivalAt.HasValue)
            .OrderBy(x => x.SequenceNumber)
            .ThenBy(x => x.StopKey, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var optimized = new List<RouteStop>(stops.Count);
        optimized.AddRange(scheduledStops);
        optimized.AddRange(unscheduledStops);
        return optimized;
    }

    private static RouteStopSummaryResponse MapStop(RouteStop stop) =>
        new(
            stop.Id,
            stop.StopKey,
            stop.Label,
            stop.AddressLabel,
            stop.StaffarrSiteOrgUnitId,
            stop.StaffarrSiteNameSnapshot,
            stop.StopType,
            stop.StopStatus,
            stop.SequenceNumber,
            stop.GeofenceAnchorLatitude,
            stop.GeofenceAnchorLongitude,
            stop.GeofenceRadiusMeters,
            stop.LastGeofenceCheckAt,
            stop.LastGeofenceResult,
            stop.LastGeofenceDistanceMeters,
            stop.LastGeofenceReportedLatitude,
            stop.LastGeofenceReportedLongitude,
            stop.ScheduledArrivalAt,
            stop.ArrivedAt,
            stop.CompletedAt,
            stop.CreatedAt,
            stop.UpdatedAt);

    private static decimal CalculateGeofenceDistanceMeters(
        decimal anchorLatitude,
        decimal anchorLongitude,
        decimal reportedLatitude,
        decimal reportedLongitude)
    {
        const double earthRadiusMeters = 6_371_000d;

        static double ToRadians(double value) => value * Math.PI / 180d;

        var anchorLat = ToRadians((double)anchorLatitude);
        var anchorLon = ToRadians((double)anchorLongitude);
        var reportedLat = ToRadians((double)reportedLatitude);
        var reportedLon = ToRadians((double)reportedLongitude);

        var deltaLat = reportedLat - anchorLat;
        var deltaLon = reportedLon - anchorLon;
        var a = Math.Pow(Math.Sin(deltaLat / 2d), 2d)
            + Math.Cos(anchorLat) * Math.Cos(reportedLat) * Math.Pow(Math.Sin(deltaLon / 2d), 2d);
        var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
        return (decimal)(earthRadiusMeters * c);
    }
}

public sealed record RouteAccessContext(
    Guid RouteCreatedByUserId,
    Guid? TripCreatedByUserId,
    string? TripAssignedDriverPersonId);
