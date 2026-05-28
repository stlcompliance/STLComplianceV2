using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class TripService(
    RoutArrDbContext db,
    IRoutArrAuditService audit,
    DispatchAssignmentService dispatchAssignment,
    DispatchNotificationEnqueueService notificationEnqueueService,
    StaffarrPersonRefService staffarrPersonRefService)
{
    public async Task<IReadOnlyList<TripSummaryResponse>> ListAsync(
        Guid tenantId,
        bool viewAll,
        Guid? actorUserId,
        string? actorPersonId,
        string? dispatchStatus = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.Trips
            .AsNoTracking()
            .Include(x => x.Loads)
            .Where(x => x.TenantId == tenantId);

        if (!viewAll && actorUserId.HasValue)
        {
            var personId = actorPersonId?.Trim();
            query = query.Where(x =>
                x.CreatedByUserId == actorUserId.Value
                || (personId != null
                    && x.AssignedDriverPersonId != null
                    && x.AssignedDriverPersonId == personId));
        }

        if (!string.IsNullOrWhiteSpace(dispatchStatus))
        {
            query = query.Where(x => x.DispatchStatus == dispatchStatus);
        }

        var trips = await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return trips.Select(MapSummary).ToList();
    }

    public async Task<TripDetailResponse> GetAsync(
        Guid tenantId,
        Guid tripId,
        CancellationToken cancellationToken = default)
    {
        var trip = await GetTripEntityAsync(tenantId, tripId, cancellationToken);
        return MapDetail(trip);
    }

    public async Task<TripDetailResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateTripRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateTitle(request.Title);

        var now = DateTimeOffset.UtcNow;
        var entity = new Trip
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TripNumber = await GenerateTripNumberAsync(tenantId, cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            DispatchStatus = TripDispatchStatuses.Planned,
            VehicleRefKey = NormalizeOptionalKey(request.VehicleRefKey),
            ScheduledStartAt = request.ScheduledStartAt,
            ScheduledEndAt = request.ScheduledEndAt,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        if (request.Loads is { Count: > 0 })
        {
            foreach (var loadRequest in request.Loads.OrderBy(x => x.SequenceNumber))
            {
                entity.Loads.Add(CreateLoadEntity(tenantId, entity.Id, loadRequest, now));
            }
        }

        db.Trips.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "trip.create",
            tenantId,
            actorUserId,
            "trip",
            entity.Id.ToString(),
            entity.DispatchStatus,
            cancellationToken: cancellationToken);

        return MapDetail(entity);
    }

    public async Task<TripDetailResponse> AssignDriverAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid tripId,
        AssignTripDriverRequest request,
        CancellationToken cancellationToken = default)
    {
        var driverPersonId = ValidateDriverPersonId(request.DriverPersonId);

        var trip = await db.Trips
            .Include(x => x.Loads)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == tripId, cancellationToken);

        if (trip is null)
        {
            throw new StlApiException("trip.not_found", "Trip was not found.", 404);
        }

        if (!TripDispatchStatuses.Active.Contains(trip.DispatchStatus))
        {
            throw new StlApiException(
                "trip.not_assignable",
                "Drivers can only be assigned to active trips.",
                400);
        }

        await dispatchAssignment.EnsureDriverAssignmentAllowedAsync(
            tenantId,
            trip,
            driverPersonId,
            request.IgnoreAvailabilityConflicts,
            request.IgnoreEligibilityBlocks,
            request.IgnoreWorkflowGateBlocks,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        trip.AssignedDriverPersonId = driverPersonId;
        trip.UpdatedAt = now;
        trip.AssignedAt ??= now;

        if (string.Equals(trip.DispatchStatus, TripDispatchStatuses.Planned, StringComparison.OrdinalIgnoreCase))
        {
            trip.DispatchStatus = TripDispatchStatuses.Assigned;
        }

        await db.SaveChangesAsync(cancellationToken);

        await staffarrPersonRefService.UpsertFromAssignmentAsync(
            tenantId,
            actorUserId,
            driverPersonId,
            request.DriverDisplayName,
            cancellationToken);

        await audit.WriteAsync(
            "trip.assign_driver",
            tenantId,
            actorUserId,
            "trip",
            trip.Id.ToString(),
            BuildAssignDriverAuditResult(driverPersonId, request),
            cancellationToken: cancellationToken);

        if (string.Equals(trip.DispatchStatus, TripDispatchStatuses.Assigned, StringComparison.OrdinalIgnoreCase))
        {
            await notificationEnqueueService.TryEnqueueForTripStatusAsync(
                trip,
                trip.DispatchStatus,
                cancellationToken);
        }

        return MapDetail(trip);
    }

    public async Task<TripDetailResponse> AssignVehicleAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid tripId,
        AssignTripVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        var trip = await db.Trips
            .Include(x => x.Loads)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == tripId, cancellationToken);

        if (trip is null)
        {
            throw new StlApiException("trip.not_found", "Trip was not found.", 404);
        }

        if (!TripDispatchStatuses.Active.Contains(trip.DispatchStatus))
        {
            throw new StlApiException(
                "trip.not_assignable",
                "Equipment can only be assigned to active trips.",
                400);
        }

        var vehicleRefKey = NormalizeOptionalKey(request.VehicleRefKey);
        if (!string.IsNullOrWhiteSpace(vehicleRefKey) && vehicleRefKey.Length > 128)
        {
            throw new StlApiException(
                "trip.vehicle_ref_too_long",
                "Vehicle reference key must be 128 characters or fewer.",
                400);
        }

        await dispatchAssignment.EnsureVehicleAssignmentAllowedAsync(
            tenantId,
            trip,
            vehicleRefKey,
            request.IgnoreAvailabilityConflicts,
            request.IgnoreDispatchabilityBlocks,
            request.IgnoreWorkflowGateBlocks,
            cancellationToken);

        trip.VehicleRefKey = vehicleRefKey;
        trip.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "trip.assign_vehicle",
            tenantId,
            actorUserId,
            "trip",
            trip.Id.ToString(),
            BuildAssignVehicleAuditResult(vehicleRefKey, request),
            cancellationToken: cancellationToken);

        return MapDetail(trip);
    }

    public async Task<TripDetailResponse> UpdateDispatchStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid tripId,
        UpdateTripDispatchStatusRequest request,
        bool canManageAny,
        string? actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var status = request.DispatchStatus?.Trim() ?? string.Empty;
        if (!TripDispatchStatuses.All.Contains(status))
        {
            throw new StlApiException(
                "trip.invalid_status",
                "Dispatch status must be planned, assigned, dispatched, in_progress, completed, or cancelled.",
                400);
        }

        var trip = await db.Trips
            .Include(x => x.Loads)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == tripId, cancellationToken);

        if (trip is null)
        {
            throw new StlApiException("trip.not_found", "Trip was not found.", 404);
        }

        var normalized = status.ToLowerInvariant();
        if (!TripDispatchStatusRules.CanTransition(trip.DispatchStatus, normalized))
        {
            throw new StlApiException(
                "trip.invalid_transition",
                $"Cannot transition trip from {trip.DispatchStatus} to {normalized}.",
                400);
        }

        if (string.Equals(normalized, TripDispatchStatuses.Assigned, StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(trip.AssignedDriverPersonId))
        {
            throw new StlApiException(
                "trip.driver_required",
                "A driver must be assigned before moving to assigned status.",
                400);
        }

        if (string.Equals(normalized, TripDispatchStatuses.Dispatched, StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(trip.AssignedDriverPersonId))
        {
            throw new StlApiException(
                "trip.driver_required",
                "A driver must be assigned before dispatch.",
                400);
        }

        if (string.Equals(normalized, TripDispatchStatuses.InProgress, StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(trip.AssignedDriverPersonId))
        {
            throw new StlApiException(
                "trip.driver_required",
                "A driver must be assigned before starting a trip.",
                400);
        }

        if (!canManageAny)
        {
            EnsureDriverCanTransition(trip, normalized, actorUserId, actorPersonId);
        }

        if (string.Equals(normalized, TripDispatchStatuses.Cancelled, StringComparison.OrdinalIgnoreCase)
            && !canManageAny)
        {
            throw new StlApiException(
                "auth.forbidden",
                "Trip cancellation requires routarr.dispatch.manage scope.",
                403);
        }

        var previousStatus = trip.DispatchStatus;
        var now = DateTimeOffset.UtcNow;
        trip.DispatchStatus = normalized;
        trip.UpdatedAt = now;

        if (string.Equals(normalized, TripDispatchStatuses.Assigned, StringComparison.OrdinalIgnoreCase))
        {
            trip.AssignedAt ??= now;
        }

        if (string.Equals(normalized, TripDispatchStatuses.Dispatched, StringComparison.OrdinalIgnoreCase))
        {
            trip.DispatchedAt ??= now;
        }

        if (string.Equals(normalized, TripDispatchStatuses.InProgress, StringComparison.OrdinalIgnoreCase))
        {
            trip.StartedAt ??= now;
        }

        if (string.Equals(normalized, TripDispatchStatuses.Completed, StringComparison.OrdinalIgnoreCase))
        {
            trip.CompletedAt ??= now;
            trip.StartedAt ??= now;
        }

        if (string.Equals(normalized, TripDispatchStatuses.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            trip.CancelledAt ??= now;
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "trip.status",
            tenantId,
            actorUserId,
            "trip",
            trip.Id.ToString(),
            normalized,
            cancellationToken: cancellationToken);

        if (!string.Equals(previousStatus, normalized, StringComparison.OrdinalIgnoreCase))
        {
            await notificationEnqueueService.TryEnqueueForTripStatusAsync(
                trip,
                normalized,
                cancellationToken);
        }

        return MapDetail(trip);
    }

    private static void EnsureDriverCanTransition(
        Trip trip,
        string targetStatus,
        Guid actorUserId,
        string? actorPersonId)
    {
        var isAssignedDriver = !string.IsNullOrWhiteSpace(trip.AssignedDriverPersonId)
            && !string.IsNullOrWhiteSpace(actorPersonId)
            && string.Equals(trip.AssignedDriverPersonId, actorPersonId, StringComparison.Ordinal);

        if (!isAssignedDriver && trip.CreatedByUserId != actorUserId)
        {
            throw new StlApiException(
                "auth.forbidden",
                "You can only update trips you created or are assigned to drive.",
                403);
        }

        var allowedForDriver = targetStatus is TripDispatchStatuses.Dispatched
            or TripDispatchStatuses.InProgress
            or TripDispatchStatuses.Completed;

        if (!allowedForDriver)
        {
            throw new StlApiException(
                "auth.forbidden",
                "Drivers can only dispatch, start, or complete assigned trips.",
                403);
        }
    }

    private async Task<Trip> GetTripEntityAsync(
        Guid tenantId,
        Guid tripId,
        CancellationToken cancellationToken)
    {
        var trip = await db.Trips
            .AsNoTracking()
            .Include(x => x.Loads)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == tripId, cancellationToken);

        if (trip is null)
        {
            throw new StlApiException("trip.not_found", "Trip was not found.", 404);
        }

        return trip;
    }

    private static TripLoad CreateLoadEntity(
        Guid tenantId,
        Guid tripId,
        CreateTripLoadRequest request,
        DateTimeOffset now)
    {
        ValidateLoadKey(request.LoadKey);
        ValidateLoadType(request.LoadType);

        return new TripLoad
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TripId = tripId,
            LoadKey = request.LoadKey.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            LoadType = NormalizeLoadType(request.LoadType),
            Status = TripLoadStatuses.Pending,
            SequenceNumber = request.SequenceNumber,
            OriginLabel = request.OriginLabel?.Trim() ?? string.Empty,
            DestinationLabel = request.DestinationLabel?.Trim() ?? string.Empty,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    private async Task<string> GenerateTripNumberAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var datePart = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            var candidate = $"TR-{datePart}-{suffix}";
            var exists = await db.Trips.AnyAsync(
                x => x.TenantId == tenantId && x.TripNumber == candidate,
                cancellationToken);
            if (!exists)
            {
                return candidate;
            }
        }

        return $"TR-{datePart}-{Guid.NewGuid():N}".ToUpperInvariant();
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new StlApiException("trip.title_required", "Trip title is required.", 400);
        }

        if (title.Trim().Length > 256)
        {
            throw new StlApiException("trip.title_too_long", "Trip title must be 256 characters or fewer.", 400);
        }
    }

    private static void ValidateLoadKey(string loadKey)
    {
        if (string.IsNullOrWhiteSpace(loadKey))
        {
            throw new StlApiException("trip_load.key_required", "Load key is required.", 400);
        }
    }

    private static void ValidateLoadType(string loadType)
    {
        if (!TripLoadTypes.All.Contains(loadType))
        {
            throw new StlApiException(
                "trip_load.invalid_type",
                "Load type must be general, pickup, or delivery.",
                400);
        }
    }

    private static string ValidateDriverPersonId(string driverPersonId)
    {
        if (string.IsNullOrWhiteSpace(driverPersonId))
        {
            throw new StlApiException("trip.driver_required", "Driver person id is required.", 400);
        }

        var trimmed = driverPersonId.Trim();
        if (trimmed.Length > 128)
        {
            throw new StlApiException(
                "trip.driver_id_too_long",
                "Driver person id must be 128 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string? NormalizeOptionalKey(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeLoadType(string loadType) =>
        loadType.Trim().ToLowerInvariant();

    private static TripSummaryResponse MapSummary(Trip trip) =>
        new(
            trip.Id,
            trip.TripNumber,
            trip.Title,
            trip.DispatchStatus,
            trip.AssignedDriverPersonId,
            trip.VehicleRefKey,
            trip.ScheduledStartAt,
            trip.ScheduledEndAt,
            trip.Loads.Count,
            trip.CreatedByUserId,
            trip.CreatedAt,
            trip.UpdatedAt,
            trip.AssignedAt,
            trip.DispatchedAt,
            trip.StartedAt,
            trip.CompletedAt,
            trip.CancelledAt);

    private static TripDetailResponse MapDetail(Trip trip) =>
        new(
            trip.Id,
            trip.TripNumber,
            trip.Title,
            trip.Description,
            trip.DispatchStatus,
            trip.AssignedDriverPersonId,
            trip.VehicleRefKey,
            trip.ScheduledStartAt,
            trip.ScheduledEndAt,
            trip.Loads
                .OrderBy(x => x.SequenceNumber)
                .Select(load => new TripLoadSummaryResponse(
                    load.Id,
                    load.LoadKey,
                    load.Description,
                    load.LoadType,
                    load.Status,
                    load.SequenceNumber,
                    load.OriginLabel,
                    load.DestinationLabel,
                    load.CreatedAt,
                    load.UpdatedAt))
                .ToList(),
            trip.CreatedByUserId,
            trip.CreatedAt,
            trip.UpdatedAt,
            trip.AssignedAt,
            trip.DispatchedAt,
            trip.StartedAt,
            trip.CompletedAt,
            trip.CancelledAt);

    private static string BuildAssignDriverAuditResult(string driverPersonId, AssignTripDriverRequest request)
    {
        var overrides = new List<string>();
        if (request.IgnoreAvailabilityConflicts)
        {
            overrides.Add("availability");
        }

        if (request.IgnoreEligibilityBlocks)
        {
            overrides.Add("eligibility");
        }

        if (request.IgnoreWorkflowGateBlocks)
        {
            overrides.Add("workflow");
        }

        return overrides.Count == 0
            ? driverPersonId
            : $"{driverPersonId} (override:{string.Join(',', overrides)})";
    }

    private static string BuildAssignVehicleAuditResult(string? vehicleRefKey, AssignTripVehicleRequest request)
    {
        var key = vehicleRefKey ?? string.Empty;
        var overrides = new List<string>();
        if (request.IgnoreAvailabilityConflicts)
        {
            overrides.Add("availability");
        }

        if (request.IgnoreDispatchabilityBlocks)
        {
            overrides.Add("dispatchability");
        }

        if (request.IgnoreWorkflowGateBlocks)
        {
            overrides.Add("workflow");
        }

        return overrides.Count == 0
            ? key
            : $"{key} (override:{string.Join(',', overrides)})";
    }
}
