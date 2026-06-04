using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class TripService(
    RoutArrDbContext db,
    IRoutArrAuditService audit,
    DispatchAssignmentService dispatchAssignment,
    DispatchNotificationEnqueueService notificationEnqueueService,
    IntegrationOutboxEnqueueService integrationOutboxEnqueueService,
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
            .Include(x => x.DispatchReleaseSnapshot)
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

    public async Task<TripDetailResponse> GetByTripNumberAsync(
        Guid tenantId,
        string tripNumber,
        CancellationToken cancellationToken = default)
    {
        var normalizedTripNumber = tripNumber.Trim();
        if (string.IsNullOrWhiteSpace(normalizedTripNumber))
        {
            throw new StlApiException("trip.trip_number_required", "Trip number is required.", 400);
        }

        var trip = await db.Trips
            .AsNoTracking()
            .Include(x => x.Loads)
            .Include(x => x.DispatchReleaseSnapshot)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.TripNumber == normalizedTripNumber,
                cancellationToken);

        if (trip is null)
        {
            throw new StlApiException("trip.not_found", "Trip was not found.", 404);
        }

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

        await integrationOutboxEnqueueService.TryEnqueueTripCreatedAsync(entity, cancellationToken);

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
            .Include(x => x.DispatchReleaseSnapshot)
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

        var previousDriverPersonId = trip.AssignedDriverPersonId;
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

        if (!string.IsNullOrWhiteSpace(previousDriverPersonId)
            && !string.Equals(previousDriverPersonId, driverPersonId, StringComparison.Ordinal))
        {
            await notificationEnqueueService.TryEnqueueDriverAssignmentChangedAsync(
                trip,
                cancellationToken);
        }

        await integrationOutboxEnqueueService.TryEnqueueDriverAssignmentChangedAsync(
            trip,
            driverPersonId,
            cancellationToken);

        await integrationOutboxEnqueueService.TryEnqueueComplianceOverridePerformedAsync(
            trip,
            "driver",
            driverPersonId,
            GetAssignDriverOverrideKinds(request),
            cancellationToken);

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
            .Include(x => x.DispatchReleaseSnapshot)
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

        await integrationOutboxEnqueueService.TryEnqueueEquipmentAssignmentChangedAsync(
            trip,
            vehicleRefKey,
            cancellationToken);

        await integrationOutboxEnqueueService.TryEnqueueComplianceOverridePerformedAsync(
            trip,
            "equipment",
            vehicleRefKey,
            GetAssignVehicleOverrideKinds(request),
            cancellationToken);

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
            .Include(x => x.DispatchReleaseSnapshot)
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

        DispatchReleasePreviewSnapshot? releasePreview = null;
        if (string.Equals(normalized, TripDispatchStatuses.Dispatched, StringComparison.OrdinalIgnoreCase))
        {
            releasePreview = await BuildDispatchReleasePreviewAsync(
                tenantId,
                trip,
                cancellationToken);

            if (!releasePreview.CanRelease)
            {
                throw new StlApiException(
                    "dispatch.release_blocked",
                    "Trip cannot be released for dispatch because one or more pre-dispatch checks failed.",
                    409,
                    releasePreview);
            }
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

        TripDispatchReleaseSnapshot? releaseSnapshot = trip.DispatchReleaseSnapshot;
        if (string.Equals(normalized, TripDispatchStatuses.Dispatched, StringComparison.OrdinalIgnoreCase))
        {
            trip.DispatchedAt ??= now;
            if (trip.DispatchReleaseSnapshot is null && releasePreview is not null)
            {
                releaseSnapshot = CreateReleaseSnapshotEntity(
                    tenantId,
                    actorUserId,
                    trip.Id,
                    now,
                    releasePreview);
                db.TripDispatchReleaseSnapshots.Add(releaseSnapshot);
            }
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

            if (string.Equals(normalized, TripDispatchStatuses.Dispatched, StringComparison.OrdinalIgnoreCase))
            {
                await integrationOutboxEnqueueService.TryEnqueueTripReleasedAsync(trip, cancellationToken);
                await integrationOutboxEnqueueService.TryEnqueueTripDispatchedAsync(trip, cancellationToken);
            }
            else if (string.Equals(normalized, TripDispatchStatuses.InProgress, StringComparison.OrdinalIgnoreCase))
            {
                await integrationOutboxEnqueueService.TryEnqueueTripStartedAsync(trip, cancellationToken);
            }
            else if (string.Equals(normalized, TripDispatchStatuses.Completed, StringComparison.OrdinalIgnoreCase))
            {
                await integrationOutboxEnqueueService.TryEnqueueTripCompletedAsync(trip, cancellationToken);
            }
            else if (string.Equals(normalized, TripDispatchStatuses.Cancelled, StringComparison.OrdinalIgnoreCase))
            {
                await integrationOutboxEnqueueService.TryEnqueueTripCancelledAsync(trip, cancellationToken);
            }
        }

        return MapDetail(trip);
    }

    public async Task<TripDetailResponse> AcceptDriverAssignmentAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid tripId,
        string actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var trip = await db.Trips
            .Include(x => x.Loads)
            .Include(x => x.DispatchReleaseSnapshot)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == tripId, cancellationToken);

        if (trip is null)
        {
            throw new StlApiException("trip.not_found", "Trip was not found.", 404);
        }

        if (!string.Equals(trip.AssignedDriverPersonId?.Trim(), actorPersonId.Trim(), StringComparison.Ordinal))
        {
            throw new StlApiException(
                "driver_portal.not_assigned",
                "You can only execute trips assigned to you.",
                403);
        }

        if (!string.Equals(trip.DispatchStatus, TripDispatchStatuses.Assigned, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "driver_portal.accept_requires_assigned",
                "Accept is only available while the trip is assigned.",
                400);
        }

        if (trip.AcceptedAt.HasValue)
        {
            return MapDetail(trip);
        }

        var now = DateTimeOffset.UtcNow;
        trip.AcceptedAt = now;
        trip.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "trip.accept",
            tenantId,
            actorUserId,
            "trip",
            trip.Id.ToString(),
            "accepted",
            cancellationToken: cancellationToken);

        await integrationOutboxEnqueueService.TryEnqueueTripAcceptedAsync(trip, cancellationToken);
        await notificationEnqueueService.TryEnqueueTripAcceptedAsync(trip, cancellationToken);

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
            .Include(x => x.DispatchReleaseSnapshot)
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
            trip.AcceptedAt,
            trip.DispatchedAt,
            trip.StartedAt,
            trip.CompletedAt,
            trip.ClosedAt,
            trip.CancelledAt);

    public async Task<TripDetailResponse> AcknowledgeDriverCloseAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid tripId,
        string actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var trip = await db.Trips
            .Include(x => x.Loads)
            .Include(x => x.DispatchReleaseSnapshot)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == tripId, cancellationToken);

        if (trip is null)
        {
            throw new StlApiException("trip.not_found", "Trip was not found.", 404);
        }

        if (!string.Equals(trip.AssignedDriverPersonId?.Trim(), actorPersonId.Trim(), StringComparison.Ordinal))
        {
            throw new StlApiException(
                "driver_portal.not_assigned",
                "You can only execute trips assigned to you.",
                403);
        }

        if (!string.Equals(trip.DispatchStatus, TripDispatchStatuses.Completed, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "driver_portal.close_requires_completed",
                "Close is only available after the trip is completed.",
                400);
        }

        if (trip.ClosedAt.HasValue)
        {
            return MapDetail(trip);
        }

        var now = DateTimeOffset.UtcNow;
        trip.ClosedAt = now;
        trip.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "driver_portal.trip.close",
            tenantId,
            actorUserId,
            "trip",
            trip.Id.ToString(),
            "acknowledged",
            cancellationToken: cancellationToken);

        return MapDetail(trip);
    }

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
            trip.AcceptedAt,
            trip.DispatchedAt,
            trip.StartedAt,
            trip.CompletedAt,
            trip.ClosedAt,
            trip.CancelledAt,
            trip.DispatchReleaseSnapshot is null
                ? null
                : new TripDispatchReleaseSnapshotResponse(
                    trip.DispatchReleaseSnapshot.Id,
                    trip.DispatchReleaseSnapshot.ReleasedAt,
                    trip.DispatchReleaseSnapshot.ReleasedByUserId,
                    trip.DispatchReleaseSnapshot.DriverCanAssign,
                    trip.DispatchReleaseSnapshot.VehicleCanAssign,
                    trip.DispatchReleaseSnapshot.HasMissingExternalData,
                    trip.DispatchReleaseSnapshot.HasStaleExternalData,
                    trip.DispatchReleaseSnapshot.Summary));

    private async Task<DispatchReleasePreviewSnapshot> BuildDispatchReleasePreviewAsync(
        Guid tenantId,
        Trip trip,
        CancellationToken cancellationToken)
    {
        var driverPreview = await dispatchAssignment.PreviewAsync(
            tenantId,
            new DispatchAssignmentPreviewRequest(
                trip.Id,
                DispatchAssignmentService.AssignmentKinds.Driver,
                trip.AssignedDriverPersonId,
                null),
            cancellationToken);

        DispatchAssignmentPreviewResponse? vehiclePreview = null;
        if (!string.IsNullOrWhiteSpace(trip.VehicleRefKey))
        {
            vehiclePreview = await dispatchAssignment.PreviewAsync(
                tenantId,
                new DispatchAssignmentPreviewRequest(
                    trip.Id,
                    DispatchAssignmentService.AssignmentKinds.Vehicle,
                    null,
                    trip.VehicleRefKey),
                cancellationToken);
        }

        var canRelease = driverPreview.CanAssign && (vehiclePreview?.CanAssign ?? true);
        var hasMissingExternalData =
            driverPreview.ConflictSummary?.HasMissingExternalData == true
            || vehiclePreview?.ConflictSummary?.HasMissingExternalData == true;
        var hasStaleExternalData =
            driverPreview.ConflictSummary?.HasStaleExternalData == true
            || vehiclePreview?.ConflictSummary?.HasStaleExternalData == true;
        var summary = BuildDispatchReleaseSummary(
            canRelease,
            hasMissingExternalData,
            hasStaleExternalData,
            driverPreview,
            vehiclePreview);

        return new DispatchReleasePreviewSnapshot(
            canRelease,
            hasMissingExternalData,
            hasStaleExternalData,
            summary,
            driverPreview,
            vehiclePreview);
    }

    private static string BuildDispatchReleaseSummary(
        bool canRelease,
        bool hasMissingExternalData,
        bool hasStaleExternalData,
        DispatchAssignmentPreviewResponse driverPreview,
        DispatchAssignmentPreviewResponse? vehiclePreview)
    {
        var mode = canRelease ? "releasable" : "blocked";
        var warnings = new List<string>();
        if (hasMissingExternalData)
        {
            warnings.Add("missing external data");
        }

        if (hasStaleExternalData)
        {
            warnings.Add("stale external data");
        }

        var driverState = driverPreview.CanAssign ? "driver ok" : "driver blocked";
        var vehicleState = vehiclePreview is null
            ? "vehicle check skipped"
            : vehiclePreview.CanAssign
                ? "vehicle ok"
                : "vehicle blocked";
        var suffix = warnings.Count == 0 ? string.Empty : $"; warnings: {string.Join(", ", warnings)}";

        return $"{mode}; {driverState}; {vehicleState}{suffix}";
    }

    private static TripDispatchReleaseSnapshot CreateReleaseSnapshotEntity(
        Guid tenantId,
        Guid actorUserId,
        Guid tripId,
        DateTimeOffset releasedAt,
        DispatchReleasePreviewSnapshot preview)
    {
        var snapshotPayload = JsonSerializer.Serialize(new
        {
            evaluatedAt = releasedAt,
            preview.CanRelease,
            preview.HasMissingExternalData,
            preview.HasStaleExternalData,
            preview.DriverPreview,
            preview.VehiclePreview,
        });

        return new TripDispatchReleaseSnapshot
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TripId = tripId,
            ReleasedByUserId = actorUserId,
            ReleasedAt = releasedAt,
            DriverCanAssign = preview.DriverPreview.CanAssign,
            VehicleCanAssign = preview.VehiclePreview?.CanAssign ?? true,
            HasMissingExternalData = preview.HasMissingExternalData,
            HasStaleExternalData = preview.HasStaleExternalData,
            Summary = preview.Summary,
            SnapshotJson = snapshotPayload,
        };
    }

    private static string BuildAssignDriverAuditResult(string driverPersonId, AssignTripDriverRequest request)
    {
        var overrides = GetAssignDriverOverrideKinds(request);

        return overrides.Count == 0
            ? driverPersonId
            : $"{driverPersonId} (override:{string.Join(',', overrides)})";
    }

    private static string BuildAssignVehicleAuditResult(string? vehicleRefKey, AssignTripVehicleRequest request)
    {
        var key = vehicleRefKey ?? string.Empty;
        var overrides = GetAssignVehicleOverrideKinds(request);

        return overrides.Count == 0
            ? key
            : $"{key} (override:{string.Join(',', overrides)})";
    }

    private static IReadOnlyList<string> GetAssignDriverOverrideKinds(AssignTripDriverRequest request)
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

        return overrides;
    }

    private static IReadOnlyList<string> GetAssignVehicleOverrideKinds(AssignTripVehicleRequest request)
    {
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

        return overrides;
    }

    private sealed record DispatchReleasePreviewSnapshot(
        bool CanRelease,
        bool HasMissingExternalData,
        bool HasStaleExternalData,
        string Summary,
        DispatchAssignmentPreviewResponse DriverPreview,
        DispatchAssignmentPreviewResponse? VehiclePreview);
}
