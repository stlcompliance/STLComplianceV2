using System.Globalization;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class AssetReservationService(
    MaintainArrDbContext db,
    AssetReadinessService assetReadinessService,
    StaffArrSiteReferenceService staffArrSites,
    StaffArrPersonLookupClient staffArrPersonLookupClient,
    TrainArrQualificationCheckClient trainArrQualificationCheckClient,
    IMaintainArrAuditService audit)
{
    private static readonly HashSet<string> NonReservableLifecycleStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "retired",
        "disposed",
    };

    public async Task<IReadOnlyList<AssetReservationResponse>> ListAsync(
        Guid tenantId,
        Guid? assetId = null,
        string? status = null,
        bool? activeOnly = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = Math.Clamp(limit ?? 25, 1, 100);
        var query = db.AssetReservations
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (assetId.HasValue)
        {
            query = query.Where(x => x.AssetId == assetId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = NormalizeStatus(status);
            query = query.Where(x => x.Status == normalizedStatus);
        }

        if (activeOnly == true)
        {
            query = query.Where(x => !AssetReservationStatuses.Terminal.Contains(x.Status));
        }

        var reservations = await query
            .OrderByDescending(x => x.RequestedStartAt)
            .ThenByDescending(x => x.CreatedAt)
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);

        AssetReadinessResponse? assetReadiness = null;
        if (assetId.HasValue)
        {
            assetReadiness = await assetReadinessService.GetAsync(tenantId, assetId.Value, cancellationToken);
        }

        var results = new List<AssetReservationResponse>(reservations.Count);
        foreach (var reservation in reservations)
        {
            results.Add(await BuildResponseAsync(tenantId, reservation, cancellationToken, assetReadiness));
        }

        return results;
    }

    public async Task<AssetReservationResponse> GetAsync(
        Guid tenantId,
        Guid reservationId,
        CancellationToken cancellationToken = default)
    {
        var reservation = await GetReservationAsync(tenantId, reservationId, cancellationToken);
        return await BuildResponseAsync(tenantId, reservation, cancellationToken);
    }

    public async Task<AssetReservationResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid assetId,
        CreateAssetReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assetId, cancellationToken);
        if (asset is null)
        {
            throw new StlApiException("asset_reservations.asset_not_found", "Asset was not found.", 404);
        }

        if (NonReservableLifecycleStatuses.Contains(asset.LifecycleStatus))
        {
            throw new StlApiException(
                "asset_reservations.asset_not_reservable",
                $"Asset {asset.AssetTag} is {asset.LifecycleStatus} and cannot be reserved.",
                409);
        }

        var purpose = NormalizeRequiredText(request.Purpose, 256, "Purpose");
        var requestedStartAt = request.RequestedStartAt;
        var requestedEndAt = request.RequestedEndAt;
        ValidateReservationWindow(requestedStartAt, requestedEndAt);

        var pickupSite = await ResolveSiteSnapshotAsync(tenantId, request.PickupLocationRef, true, cancellationToken);
        var returnSite = string.IsNullOrWhiteSpace(request.ReturnLocationRef)
            ? pickupSite
            : await ResolveSiteSnapshotAsync(tenantId, request.ReturnLocationRef, true, cancellationToken);

        var operatorPersonId = NormalizeRequiredPersonId(request.OperatorPersonId, "Operator person id");
        var driverPersonId = string.IsNullOrWhiteSpace(request.DriverPersonId)
            ? operatorPersonId
            : NormalizeRequiredPersonId(request.DriverPersonId, "Driver person id");
        var notes = NormalizeOptionalText(request.Notes, 2048);
        var capacityNotes = NormalizeOptionalText(request.CapacityNotes, 512);
        var equipmentNotes = NormalizeOptionalText(request.EquipmentNotes, 512);
        var requestedByPersonId = NormalizeRequiredPersonId(actorPersonId, "Requester person id");
        var requestedByDisplayName = await ResolvePersonDisplayNameAsync(tenantId, requestedByPersonId, cancellationToken);
        var operatorDisplayName = await ResolvePersonDisplayNameAsync(tenantId, operatorPersonId, cancellationToken);
        var driverDisplayName = driverPersonId == operatorPersonId
            ? operatorDisplayName
            : await ResolvePersonDisplayNameAsync(tenantId, driverPersonId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var reservationNumber = await GenerateReservationNumberAsync(tenantId, cancellationToken);
        var entity = new AssetReservation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetId = asset.Id,
            AssetTag = asset.AssetTag,
            AssetName = asset.Name,
            ReservationNumber = reservationNumber,
            Status = AssetReservationStatuses.Requested,
            Purpose = purpose,
            RequestedStartAt = requestedStartAt,
            RequestedEndAt = requestedEndAt,
            PickupLocationRef = pickupSite is { OrgUnitId: Guid pickupOrgUnitId } ? pickupOrgUnitId.ToString("D") : null,
            PickupLocationNameSnapshot = pickupSite?.Name,
            ReturnLocationRef = returnSite is { OrgUnitId: Guid returnOrgUnitId } ? returnOrgUnitId.ToString("D") : null,
            ReturnLocationNameSnapshot = returnSite?.Name,
            CapacityNotes = capacityNotes,
            EquipmentNotes = equipmentNotes,
            OperatorPersonId = operatorPersonId,
            OperatorDisplayNameSnapshot = operatorDisplayName,
            DriverPersonId = driverPersonId,
            DriverDisplayNameSnapshot = driverDisplayName,
            RequestedByPersonId = requestedByPersonId,
            RequestedByDisplayNameSnapshot = requestedByDisplayName,
            Notes = notes,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.AssetReservations.Add(entity);
        await AddStatusEventAsync(
            entity,
            now,
            requestedByPersonId,
            requestedByDisplayName,
            AssetReservationEventTypes.Requested,
            string.Empty,
            AssetReservationStatuses.Requested,
            $"Reservation {reservationNumber} was requested.",
            notes,
            null);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "reservation.create",
            tenantId,
            actorUserId,
            requestedByPersonId,
            "asset_reservation",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return await BuildResponseAsync(tenantId, entity, cancellationToken);
    }

    public async Task<AssetReservationResponse> ApproveAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid reservationId,
        ReservationActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var reservation = await GetReservationAsync(tenantId, reservationId, cancellationToken);
        EnsureStatusAllowed(reservation, AssetReservationStatuses.Requested, AssetReservationStatuses.Approved);
        if (string.Equals(reservation.Status, AssetReservationStatuses.Approved, StringComparison.OrdinalIgnoreCase))
        {
            return await BuildResponseAsync(tenantId, reservation, cancellationToken);
        }

        var actorDisplayName = await ResolvePersonDisplayNameAsync(tenantId, NormalizeRequiredPersonId(actorPersonId, "Actor person id"), cancellationToken);
        var occurredAt = request.OccurredAt ?? DateTimeOffset.UtcNow;
        ApplyApproval(reservation, occurredAt, request);
        await AddStatusEventAsync(
            reservation,
            occurredAt,
            NormalizeRequiredPersonId(actorPersonId, "Actor person id"),
            actorDisplayName,
            AssetReservationEventTypes.Approved,
            AssetReservationStatuses.Requested,
            AssetReservationStatuses.Approved,
            $"Reservation {reservation.ReservationNumber} was approved.",
            NormalizeOptionalText(request.Notes, 1024),
            null);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "reservation.approve",
            tenantId,
            actorUserId,
            actorPersonId,
            "asset_reservation",
            reservation.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return await BuildResponseAsync(tenantId, reservation, cancellationToken);
    }

    public async Task<AssetReservationResponse> ReserveAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid reservationId,
        ReservationActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var reservation = await GetReservationAsync(tenantId, reservationId, cancellationToken);
        EnsureStatusAllowed(reservation, AssetReservationStatuses.Requested, AssetReservationStatuses.Approved);
        await EnsureCanReserveAsync(tenantId, reservation, cancellationToken);

        var actorDisplayName = await ResolvePersonDisplayNameAsync(tenantId, NormalizeRequiredPersonId(actorPersonId, "Actor person id"), cancellationToken);
        var occurredAt = request.OccurredAt ?? DateTimeOffset.UtcNow;
        var fromStatus = reservation.Status;
        if (string.Equals(reservation.Status, AssetReservationStatuses.Requested, StringComparison.OrdinalIgnoreCase))
        {
            ApplyApproval(reservation, occurredAt, request);
            await AddStatusEventAsync(
                reservation,
                occurredAt,
                NormalizeRequiredPersonId(actorPersonId, "Actor person id"),
                actorDisplayName,
                AssetReservationEventTypes.Approved,
                fromStatus,
                AssetReservationStatuses.Approved,
                $"Reservation {reservation.ReservationNumber} was auto-approved.",
                NormalizeOptionalText(request.Notes, 1024),
                null);
            fromStatus = reservation.Status;
        }

        ApplyReserved(reservation, occurredAt, request);
        await AddStatusEventAsync(
            reservation,
            occurredAt,
            NormalizeRequiredPersonId(actorPersonId, "Actor person id"),
            actorDisplayName,
            AssetReservationEventTypes.Reserved,
            fromStatus,
            AssetReservationStatuses.Reserved,
            $"Reservation {reservation.ReservationNumber} was reserved.",
            NormalizeOptionalText(request.Notes, 1024),
            null);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "reservation.reserve",
            tenantId,
            actorUserId,
            actorPersonId,
            "asset_reservation",
            reservation.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return await BuildResponseAsync(tenantId, reservation, cancellationToken);
    }

    public async Task<AssetReservationResponse> CheckOutAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid reservationId,
        ReservationActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var reservation = await GetReservationAsync(tenantId, reservationId, cancellationToken);
        EnsureStatusAllowed(reservation, AssetReservationStatuses.Approved, AssetReservationStatuses.Reserved);
        await EnsureCanReserveAsync(tenantId, reservation, cancellationToken);

        var actorDisplayName = await ResolvePersonDisplayNameAsync(tenantId, NormalizeRequiredPersonId(actorPersonId, "Actor person id"), cancellationToken);
        var occurredAt = request.OccurredAt ?? DateTimeOffset.UtcNow;
        ApplyCheckedOut(reservation, occurredAt, request);
        await AddStatusEventAsync(
            reservation,
            occurredAt,
            NormalizeRequiredPersonId(actorPersonId, "Actor person id"),
            actorDisplayName,
            AssetReservationEventTypes.CheckedOut,
            reservation.Status,
            AssetReservationStatuses.CheckedOut,
            $"Reservation {reservation.ReservationNumber} was checked out.",
            NormalizeOptionalText(request.Notes, 1024),
            request.MeterReading);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "reservation.checkout",
            tenantId,
            actorUserId,
            actorPersonId,
            "asset_reservation",
            reservation.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return await BuildResponseAsync(tenantId, reservation, cancellationToken);
    }

    public async Task<AssetReservationResponse> StartUseAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid reservationId,
        ReservationActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var reservation = await GetReservationAsync(tenantId, reservationId, cancellationToken);
        EnsureStatusAllowed(reservation, AssetReservationStatuses.CheckedOut, AssetReservationStatuses.InUse);
        await EnsureCanReserveAsync(tenantId, reservation, cancellationToken);

        var actorDisplayName = await ResolvePersonDisplayNameAsync(tenantId, NormalizeRequiredPersonId(actorPersonId, "Actor person id"), cancellationToken);
        var occurredAt = request.OccurredAt ?? DateTimeOffset.UtcNow;
        if (string.Equals(reservation.Status, AssetReservationStatuses.InUse, StringComparison.OrdinalIgnoreCase))
        {
            return await BuildResponseAsync(tenantId, reservation, cancellationToken);
        }

        ApplyInUse(reservation, occurredAt, request);
        await AddStatusEventAsync(
            reservation,
            occurredAt,
            NormalizeRequiredPersonId(actorPersonId, "Actor person id"),
            actorDisplayName,
            AssetReservationEventTypes.InUse,
            AssetReservationStatuses.CheckedOut,
            AssetReservationStatuses.InUse,
            $"Reservation {reservation.ReservationNumber} moved into use.",
            NormalizeOptionalText(request.Notes, 1024),
            request.MeterReading);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "reservation.start_use",
            tenantId,
            actorUserId,
            actorPersonId,
            "asset_reservation",
            reservation.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return await BuildResponseAsync(tenantId, reservation, cancellationToken);
    }

    public async Task<AssetReservationResponse> ReturnAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid reservationId,
        ReservationActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var reservation = await GetReservationAsync(tenantId, reservationId, cancellationToken);
        EnsureStatusAllowed(reservation, AssetReservationStatuses.CheckedOut, AssetReservationStatuses.InUse);

        var actorDisplayName = await ResolvePersonDisplayNameAsync(tenantId, NormalizeRequiredPersonId(actorPersonId, "Actor person id"), cancellationToken);
        var occurredAt = request.OccurredAt ?? DateTimeOffset.UtcNow;
        if (request.MeterReading.HasValue
            && reservation.CheckOutMeterReading.HasValue
            && request.MeterReading.Value < reservation.CheckOutMeterReading.Value)
        {
            throw new StlApiException(
                "asset_reservations.invalid_meter",
                "Return meter reading must be greater than or equal to the checkout meter reading.",
                400);
        }

        ApplyReturned(reservation, occurredAt, request);
        await AddStatusEventAsync(
            reservation,
            occurredAt,
            NormalizeRequiredPersonId(actorPersonId, "Actor person id"),
            actorDisplayName,
            AssetReservationEventTypes.Returned,
            reservation.Status,
            AssetReservationStatuses.Returned,
            $"Reservation {reservation.ReservationNumber} was returned.",
            NormalizeOptionalText(request.Notes, 1024),
            request.MeterReading);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "reservation.return",
            tenantId,
            actorUserId,
            actorPersonId,
            "asset_reservation",
            reservation.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return await BuildResponseAsync(tenantId, reservation, cancellationToken);
    }

    public async Task<AssetReservationResponse> InspectAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid reservationId,
        ReservationActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var reservation = await GetReservationAsync(tenantId, reservationId, cancellationToken);
        EnsureStatusAllowed(reservation, AssetReservationStatuses.Returned, AssetReservationStatuses.Inspection);

        var actorDisplayName = await ResolvePersonDisplayNameAsync(tenantId, NormalizeRequiredPersonId(actorPersonId, "Actor person id"), cancellationToken);
        var occurredAt = request.OccurredAt ?? DateTimeOffset.UtcNow;
        if (string.Equals(reservation.Status, AssetReservationStatuses.Inspection, StringComparison.OrdinalIgnoreCase))
        {
            return await BuildResponseAsync(tenantId, reservation, cancellationToken);
        }

        ApplyInspection(reservation, occurredAt, request);
        await AddStatusEventAsync(
            reservation,
            occurredAt,
            NormalizeRequiredPersonId(actorPersonId, "Actor person id"),
            actorDisplayName,
            AssetReservationEventTypes.Inspection,
            reservation.Status,
            AssetReservationStatuses.Inspection,
            $"Reservation {reservation.ReservationNumber} entered inspection.",
            NormalizeOptionalText(request.Notes, 1024),
            request.MeterReading);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "reservation.inspect",
            tenantId,
            actorUserId,
            actorPersonId,
            "asset_reservation",
            reservation.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return await BuildResponseAsync(tenantId, reservation, cancellationToken);
    }

    public async Task<AssetReservationResponse> CloseAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid reservationId,
        ReservationActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var reservation = await GetReservationAsync(tenantId, reservationId, cancellationToken);
        EnsureStatusAllowed(
            reservation,
            AssetReservationStatuses.Returned,
            AssetReservationStatuses.Inspection,
            AssetReservationStatuses.Reserved);

        var actorDisplayName = await ResolvePersonDisplayNameAsync(tenantId, NormalizeRequiredPersonId(actorPersonId, "Actor person id"), cancellationToken);
        var occurredAt = request.OccurredAt ?? DateTimeOffset.UtcNow;
        if (string.Equals(reservation.Status, AssetReservationStatuses.Closed, StringComparison.OrdinalIgnoreCase))
        {
            return await BuildResponseAsync(tenantId, reservation, cancellationToken);
        }

        ApplyClosed(reservation, occurredAt, request);
        await AddStatusEventAsync(
            reservation,
            occurredAt,
            NormalizeRequiredPersonId(actorPersonId, "Actor person id"),
            actorDisplayName,
            AssetReservationEventTypes.Closed,
            reservation.Status,
            AssetReservationStatuses.Closed,
            $"Reservation {reservation.ReservationNumber} was closed.",
            NormalizeOptionalText(request.Notes, 1024),
            request.MeterReading);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "reservation.close",
            tenantId,
            actorUserId,
            actorPersonId,
            "asset_reservation",
            reservation.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return await BuildResponseAsync(tenantId, reservation, cancellationToken);
    }

    public async Task<AssetReservationResponse> CancelAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid reservationId,
        ReservationActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var reservation = await GetReservationAsync(tenantId, reservationId, cancellationToken);
        EnsureStatusAllowed(
            reservation,
            AssetReservationStatuses.Requested,
            AssetReservationStatuses.Approved,
            AssetReservationStatuses.Reserved);

        var actorDisplayName = await ResolvePersonDisplayNameAsync(tenantId, NormalizeRequiredPersonId(actorPersonId, "Actor person id"), cancellationToken);
        var occurredAt = request.OccurredAt ?? DateTimeOffset.UtcNow;
        ApplyCanceled(reservation, occurredAt, request);
        await AddStatusEventAsync(
            reservation,
            occurredAt,
            NormalizeRequiredPersonId(actorPersonId, "Actor person id"),
            actorDisplayName,
            AssetReservationEventTypes.Canceled,
            reservation.Status,
            AssetReservationStatuses.Canceled,
            $"Reservation {reservation.ReservationNumber} was canceled.",
            NormalizeOptionalText(request.Notes, 1024),
            request.MeterReading);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "reservation.cancel",
            tenantId,
            actorUserId,
            actorPersonId,
            "asset_reservation",
            reservation.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return await BuildResponseAsync(tenantId, reservation, cancellationToken);
    }

    public async Task<AssetReservationResponse> NoShowAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid reservationId,
        ReservationActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var reservation = await GetReservationAsync(tenantId, reservationId, cancellationToken);
        EnsureStatusAllowed(
            reservation,
            AssetReservationStatuses.Requested,
            AssetReservationStatuses.Approved,
            AssetReservationStatuses.Reserved);

        var occurredAt = request.OccurredAt ?? DateTimeOffset.UtcNow;
        if (occurredAt < reservation.RequestedStartAt)
        {
            throw new StlApiException(
                "asset_reservations.invalid_no_show",
                "No-show cannot be recorded before the reservation start time.",
                400);
        }

        var actorDisplayName = await ResolvePersonDisplayNameAsync(tenantId, NormalizeRequiredPersonId(actorPersonId, "Actor person id"), cancellationToken);
        ApplyNoShow(reservation, occurredAt, request);
        await AddStatusEventAsync(
            reservation,
            occurredAt,
            NormalizeRequiredPersonId(actorPersonId, "Actor person id"),
            actorDisplayName,
            AssetReservationEventTypes.NoShow,
            reservation.Status,
            AssetReservationStatuses.NoShow,
            $"Reservation {reservation.ReservationNumber} was marked no-show.",
            NormalizeOptionalText(request.Notes, 1024),
            request.MeterReading);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "reservation.no_show",
            tenantId,
            actorUserId,
            actorPersonId,
            "asset_reservation",
            reservation.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return await BuildResponseAsync(tenantId, reservation, cancellationToken);
    }

    private async Task<AssetReservation> GetReservationAsync(
        Guid tenantId,
        Guid reservationId,
        CancellationToken cancellationToken)
    {
        var reservation = await db.AssetReservations
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == reservationId, cancellationToken);
        if (reservation is null)
        {
            throw new StlApiException("asset_reservations.not_found", "Reservation was not found.", 404);
        }

        return reservation;
    }

    private async Task<AssetReservationResponse> BuildResponseAsync(
        Guid tenantId,
        AssetReservation reservation,
        CancellationToken cancellationToken,
        AssetReadinessResponse? readiness = null)
    {
        var readinessTask = readiness is null
            ? assetReadinessService.GetAsync(tenantId, reservation.AssetId, cancellationToken)
            : Task.FromResult(readiness);
        var conflictsTask = LoadConflictsAsync(tenantId, reservation, cancellationToken);
        var qualificationChecksTask = BuildQualificationChecksAsync(tenantId, reservation, cancellationToken);
        var timelineTask = LoadTimelineAsync(tenantId, reservation.Id, cancellationToken);

        await Task.WhenAll(readinessTask, conflictsTask, qualificationChecksTask, timelineTask);

        readiness = readinessTask.Result;
        var conflicts = conflictsTask.Result;
        var qualificationChecks = qualificationChecksTask.Result;
        var timeline = timelineTask.Result;
        var decision = BuildDecision(reservation, readiness, conflicts, qualificationChecks);

        return new AssetReservationResponse(
            reservation.Id,
            reservation.AssetId,
            reservation.AssetTag,
            reservation.AssetName,
            reservation.ReservationNumber,
            reservation.Status,
            reservation.Purpose,
            reservation.RequestedStartAt,
            reservation.RequestedEndAt,
            reservation.PickupLocationRef,
            reservation.PickupLocationNameSnapshot,
            reservation.ReturnLocationRef,
            reservation.ReturnLocationNameSnapshot,
            reservation.CapacityNotes,
            reservation.EquipmentNotes,
            reservation.OperatorPersonId,
            reservation.OperatorDisplayNameSnapshot,
            reservation.DriverPersonId,
            reservation.DriverDisplayNameSnapshot,
            reservation.RequestedByPersonId,
            reservation.RequestedByDisplayNameSnapshot,
            reservation.Notes,
            reservation.CheckOutMeterReading,
            reservation.ReturnMeterReading,
            reservation.ApprovedAt,
            reservation.ReservedAt,
            reservation.CheckedOutAt,
            reservation.InUseAt,
            reservation.ReturnedAt,
            reservation.InspectedAt,
            reservation.ClosedAt,
            reservation.CanceledAt,
            reservation.NoShowAt,
            reservation.CancelReason,
            reservation.NoShowReason,
            reservation.InspectionNotes,
            reservation.DamageNotes,
            reservation.ChargeNotes,
            readiness.ReadinessStatus,
            readiness.ReadinessBasis,
            decision.Status,
            decision.Summary,
            decision.Detail,
            conflicts.Count,
            conflicts,
            qualificationChecks,
            timeline,
            reservation.CreatedByUserId,
            reservation.CreatedAt,
            reservation.UpdatedAt);
    }

    private async Task<IReadOnlyList<AssetReservationConflictResponse>> LoadConflictsAsync(
        Guid tenantId,
        AssetReservation reservation,
        CancellationToken cancellationToken)
    {
        var conflicts = await db.AssetReservations
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && x.AssetId == reservation.AssetId
                && x.Id != reservation.Id
                && x.Status != AssetReservationStatuses.Closed
                && x.Status != AssetReservationStatuses.Canceled
                && x.Status != AssetReservationStatuses.NoShow
                && x.RequestedStartAt < reservation.RequestedEndAt
                && x.RequestedEndAt > reservation.RequestedStartAt)
            .OrderBy(x => x.RequestedStartAt)
            .ToListAsync(cancellationToken);

        return conflicts
            .Select(x => new AssetReservationConflictResponse(
                x.Id,
                x.ReservationNumber,
                x.Status,
                x.Purpose,
                x.RequestedStartAt,
                x.RequestedEndAt,
                $"Conflicts with reservation {x.ReservationNumber} from {x.RequestedStartAt:yyyy-MM-dd HH:mm} to {x.RequestedEndAt:yyyy-MM-dd HH:mm}."))
            .ToList();
    }

    private async Task<IReadOnlyList<AssetReservationQualificationCheckResponse>> BuildQualificationChecksAsync(
        Guid tenantId,
        AssetReservation reservation,
        CancellationToken cancellationToken)
    {
        if (!trainArrQualificationCheckClient.IsConfigured)
        {
            return [];
        }

        var checks = new List<AssetReservationQualificationCheckResponse>();

        var operatorCheck = await BuildQualificationCheckAsync(
            tenantId,
            reservation.OperatorPersonId,
            "operator",
            cancellationToken);
        if (operatorCheck is not null)
        {
            checks.Add(operatorCheck);
        }

        if (!string.IsNullOrWhiteSpace(reservation.DriverPersonId)
            && !string.Equals(reservation.DriverPersonId, reservation.OperatorPersonId, StringComparison.OrdinalIgnoreCase))
        {
            var driverCheck = await BuildQualificationCheckAsync(
                tenantId,
                reservation.DriverPersonId,
                "driver",
                cancellationToken);
            if (driverCheck is not null)
            {
                checks.Add(driverCheck);
            }
        }

        return checks;
    }

    private async Task<AssetReservationQualificationCheckResponse?> BuildQualificationCheckAsync(
        Guid tenantId,
        string? personId,
        string role,
        CancellationToken cancellationToken)
    {
        var normalizedPersonId = NormalizeOptionalPersonId(personId);
        if (string.IsNullOrWhiteSpace(normalizedPersonId))
        {
            return null;
        }

        var displayName = await ResolvePersonDisplayNameAsync(tenantId, normalizedPersonId, cancellationToken);

        if (!Guid.TryParse(normalizedPersonId, out var staffarrPersonId))
        {
            return new AssetReservationQualificationCheckResponse(
                role,
                normalizedPersonId,
                displayName,
                trainArrQualificationCheckClient.TechnicianQualificationKey,
                "invalid",
                "invalid_person_id",
                "Person id must be a StaffArr GUID when TrainArr qualification checks are enabled.");
        }

        var check = await trainArrQualificationCheckClient.CheckTechnicianAsync(
            tenantId,
            staffarrPersonId,
            cancellationToken);

        return check is null
            ? null
            : new AssetReservationQualificationCheckResponse(
                role,
                normalizedPersonId,
                displayName,
                check.QualificationKey,
                check.Outcome,
                check.ReasonCode,
                check.Message);
    }

    private async Task<IReadOnlyList<AssetReservationTimelineEventResponse>> LoadTimelineAsync(
        Guid tenantId,
        Guid reservationId,
        CancellationToken cancellationToken)
    {
        return await db.AssetReservationStatusEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetReservationId == reservationId)
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.CreatedAt)
            .Take(8)
            .Select(x => new AssetReservationTimelineEventResponse(
                x.Id,
                x.EventType,
                x.FromStatus,
                x.ToStatus,
                x.Message,
                x.ActorPersonId,
                x.ActorDisplayNameSnapshot,
                x.Notes,
                x.MeterReading,
                x.OccurredAt,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    private ReservationDecision BuildDecision(
        AssetReservation reservation,
        AssetReadinessResponse readiness,
        IReadOnlyList<AssetReservationConflictResponse> conflicts,
        IReadOnlyList<AssetReservationQualificationCheckResponse> qualificationChecks)
    {
        var blockedReasons = new List<string>();
        var warningReasons = new List<string>();

        if (!string.Equals(readiness.ReadinessStatus, "ready", StringComparison.OrdinalIgnoreCase))
        {
            blockedReasons.Add($"asset readiness is {Humanize(readiness.ReadinessStatus)}");
        }

        foreach (var blocker in readiness.Blockers)
        {
            blockedReasons.Add(blocker.Message);
        }

        foreach (var conflict in conflicts)
        {
            blockedReasons.Add(conflict.Message);
        }

        foreach (var check in qualificationChecks)
        {
            if (IsPermissiveQualificationOutcome(check.Outcome))
            {
                if (string.Equals(check.Outcome, "warn", StringComparison.OrdinalIgnoreCase))
                {
                    warningReasons.Add(check.Message);
                }
                continue;
            }

            blockedReasons.Add($"{Humanize(check.Role)} qualification: {check.Message}");
        }

        if (IsTerminalStatus(reservation.Status))
        {
            return new ReservationDecision(
                "clear",
                $"Reservation {reservation.ReservationNumber} is {Humanize(reservation.Status)}.",
                BuildTerminalDecisionDetail(reservation));
        }

        if (blockedReasons.Count > 0)
        {
            return new ReservationDecision(
                "blocked",
                "Reservation has blockers.",
                string.Join(" ", blockedReasons.Distinct(StringComparer.OrdinalIgnoreCase).Take(4)));
        }

        if (warningReasons.Count > 0 || string.Equals(reservation.Status, AssetReservationStatuses.Requested, StringComparison.OrdinalIgnoreCase))
        {
            var summary = string.Equals(reservation.Status, AssetReservationStatuses.Requested, StringComparison.OrdinalIgnoreCase)
                ? "Reservation is waiting for approval."
                : "Reservation can proceed with warnings.";
            var detail = warningReasons.Count > 0
                ? string.Join(" ", warningReasons.Distinct(StringComparer.OrdinalIgnoreCase).Take(3))
                : "No blockers were found, but the reservation still needs approval before it can be reserved.";
            return new ReservationDecision("watch", summary, detail);
        }

        return new ReservationDecision(
            "clear",
            BuildStatusSummary(reservation.Status),
            BuildStatusDetail(reservation.Status, readiness));
    }

    private async Task EnsureCanReserveAsync(
        Guid tenantId,
        AssetReservation reservation,
        CancellationToken cancellationToken)
    {
        var readiness = await assetReadinessService.GetAsync(tenantId, reservation.AssetId, cancellationToken);
        var conflicts = await LoadConflictsAsync(tenantId, reservation, cancellationToken);
        var qualificationChecks = await BuildQualificationChecksAsync(tenantId, reservation, cancellationToken);
        var decision = BuildDecision(reservation, readiness, conflicts, qualificationChecks);

        if (string.Equals(decision.Status, "blocked", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "asset_reservations.blocked",
                decision.Detail,
                409);
        }
    }

    private async Task<string> ResolvePersonDisplayNameAsync(
        Guid tenantId,
        string personId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(personId))
        {
            return string.Empty;
        }

        var normalizedPersonId = personId.Trim();
        if (Guid.TryParse(normalizedPersonId, out var staffarrPersonId))
        {
            var lookup = await staffArrPersonLookupClient.TryLookupAsync(tenantId, staffarrPersonId, cancellationToken);
            if (lookup is not null && !string.IsNullOrWhiteSpace(lookup.DisplayName))
            {
                return lookup.DisplayName.Trim();
            }
        }

        var cached = await db.StaffPersonRefs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.StaffarrPersonId == normalizedPersonId, cancellationToken);
        return cached?.DisplayNameSnapshot?.Trim() ?? normalizedPersonId;
    }

    private async Task<(Guid? OrgUnitId, string? Name)?> ResolveSiteSnapshotAsync(
        Guid tenantId,
        string? siteRef,
        bool required,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(siteRef))
        {
            if (required)
            {
                throw new StlApiException(
                    "asset_reservations.site_required",
                    "Pickup location is required for asset reservations.",
                    400);
            }

            return null;
        }

        var site = await staffArrSites.RequireActiveSiteAsync(tenantId, ParseSiteId(siteRef), cancellationToken);
        return (site.OrgUnitId, site.Name);
    }

    private static Guid ParseSiteId(string value)
    {
        if (!Guid.TryParse(value.Trim(), out var siteId))
        {
            throw new StlApiException(
                "asset_reservations.invalid_site_ref",
                "Reservation location must be a StaffArr site GUID.",
                400);
        }

        return siteId;
    }

    private async Task<string> GenerateReservationNumberAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var datePart = DateTimeOffset.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var sequence = await db.AssetReservations.CountAsync(x => x.TenantId == tenantId, cancellationToken) + 1;
        return $"RV-{datePart}-{sequence:0000}";
    }

    private static void ValidateReservationWindow(DateTimeOffset requestedStartAt, DateTimeOffset requestedEndAt)
    {
        if (requestedStartAt >= requestedEndAt)
        {
            throw new StlApiException(
                "asset_reservations.invalid_window",
                "Requested start must be before requested end.",
                400);
        }
    }

    private static void EnsureStatusAllowed(AssetReservation reservation, params string[] allowedStatuses)
    {
        if (allowedStatuses.Any(status => string.Equals(status, reservation.Status, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        throw new StlApiException(
            "asset_reservations.invalid_status",
            $"Reservation {reservation.ReservationNumber} is {reservation.Status} and cannot perform that action.",
            409);
    }

    private static void ApplyApproval(AssetReservation reservation, DateTimeOffset occurredAt, ReservationActionRequest request)
    {
        reservation.Status = AssetReservationStatuses.Approved;
        reservation.ApprovedAt = reservation.ApprovedAt ?? occurredAt;
        AppendReservationNotes(reservation, request.Notes);
        reservation.UpdatedAt = occurredAt;
    }

    private static void ApplyReserved(AssetReservation reservation, DateTimeOffset occurredAt, ReservationActionRequest request)
    {
        reservation.Status = AssetReservationStatuses.Reserved;
        reservation.ApprovedAt ??= occurredAt;
        reservation.ReservedAt = occurredAt;
        AppendReservationNotes(reservation, request.Notes);
        reservation.UpdatedAt = occurredAt;
    }

    private static void ApplyCheckedOut(AssetReservation reservation, DateTimeOffset occurredAt, ReservationActionRequest request)
    {
        reservation.Status = AssetReservationStatuses.CheckedOut;
        reservation.CheckedOutAt = occurredAt;
        reservation.CheckOutMeterReading = request.MeterReading ?? reservation.CheckOutMeterReading;
        AppendReservationNotes(reservation, request.Notes);
        reservation.UpdatedAt = occurredAt;
    }

    private static void ApplyInUse(AssetReservation reservation, DateTimeOffset occurredAt, ReservationActionRequest request)
    {
        reservation.Status = AssetReservationStatuses.InUse;
        reservation.InUseAt = occurredAt;
        AppendReservationNotes(reservation, request.Notes);
        reservation.UpdatedAt = occurredAt;
    }

    private static void ApplyReturned(AssetReservation reservation, DateTimeOffset occurredAt, ReservationActionRequest request)
    {
        reservation.Status = AssetReservationStatuses.Returned;
        reservation.ReturnedAt = occurredAt;
        reservation.ReturnMeterReading = request.MeterReading ?? reservation.ReturnMeterReading;
        reservation.DamageNotes = NormalizeOptionalText(request.Notes, 1024);
        reservation.ChargeNotes = UpdateOptionalText(reservation.ChargeNotes, request.ChargeNotes, 1024);
        AppendReservationNotes(reservation, request.Notes);
        reservation.UpdatedAt = occurredAt;
    }

    private static void ApplyInspection(AssetReservation reservation, DateTimeOffset occurredAt, ReservationActionRequest request)
    {
        reservation.Status = AssetReservationStatuses.Inspection;
        reservation.InspectedAt = occurredAt;
        reservation.InspectionNotes = NormalizeOptionalText(request.Notes, 1024);
        reservation.ChargeNotes = UpdateOptionalText(reservation.ChargeNotes, request.ChargeNotes, 1024);
        AppendReservationNotes(reservation, request.Notes);
        reservation.UpdatedAt = occurredAt;
    }

    private static void ApplyClosed(AssetReservation reservation, DateTimeOffset occurredAt, ReservationActionRequest request)
    {
        reservation.Status = AssetReservationStatuses.Closed;
        reservation.ClosedAt = occurredAt;
        reservation.ChargeNotes = UpdateOptionalText(reservation.ChargeNotes, request.ChargeNotes, 1024);
        AppendReservationNotes(reservation, request.Notes);
        reservation.UpdatedAt = occurredAt;
    }

    private static void ApplyCanceled(AssetReservation reservation, DateTimeOffset occurredAt, ReservationActionRequest request)
    {
        reservation.Status = AssetReservationStatuses.Canceled;
        reservation.CanceledAt = occurredAt;
        reservation.CancelReason = NormalizeOptionalText(request.Notes, 512);
        AppendReservationNotes(reservation, request.Notes);
        reservation.UpdatedAt = occurredAt;
    }

    private static void ApplyNoShow(AssetReservation reservation, DateTimeOffset occurredAt, ReservationActionRequest request)
    {
        reservation.Status = AssetReservationStatuses.NoShow;
        reservation.NoShowAt = occurredAt;
        reservation.NoShowReason = NormalizeOptionalText(request.Notes, 512);
        AppendReservationNotes(reservation, request.Notes);
        reservation.UpdatedAt = occurredAt;
    }

    private async Task AddStatusEventAsync(
        AssetReservation reservation,
        DateTimeOffset occurredAt,
        string? actorPersonId,
        string? actorDisplayNameSnapshot,
        string eventType,
        string fromStatus,
        string toStatus,
        string message,
        string? notes,
        decimal? meterReading)
    {
        db.AssetReservationStatusEvents.Add(new AssetReservationStatusEvent
        {
            Id = Guid.NewGuid(),
            TenantId = reservation.TenantId,
            AssetReservationId = reservation.Id,
            EventType = eventType,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            Message = message,
            ActorPersonId = string.IsNullOrWhiteSpace(actorPersonId) ? null : actorPersonId.Trim(),
            ActorDisplayNameSnapshot = string.IsNullOrWhiteSpace(actorDisplayNameSnapshot) ? null : actorDisplayNameSnapshot.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            MeterReading = meterReading,
            OccurredAt = occurredAt,
            CreatedAt = occurredAt,
        });
        await Task.CompletedTask;
    }

    private static void AppendReservationNotes(AssetReservation reservation, string? notes)
    {
        var normalizedNotes = NormalizeOptionalText(notes, 1024);
        if (string.IsNullOrWhiteSpace(normalizedNotes))
        {
            return;
        }

        reservation.Notes = string.IsNullOrWhiteSpace(reservation.Notes)
            ? normalizedNotes
            : $"{reservation.Notes.Trim()}\n{normalizedNotes}";
    }

    private static string? UpdateOptionalText(string? currentValue, string? nextValue, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(nextValue))
        {
            return currentValue;
        }

        return NormalizeOptionalText(nextValue, maxLength);
    }

    private static string BuildTerminalDecisionDetail(AssetReservation reservation)
    {
        var statusLabel = Humanize(reservation.Status);
        var details = new List<string> { $"Reservation is {statusLabel}." };
        if (reservation.ReturnedAt.HasValue)
        {
            details.Add($"Returned at {reservation.ReturnedAt:yyyy-MM-dd HH:mm}.");
        }

        if (reservation.ClosedAt.HasValue)
        {
            details.Add($"Closed at {reservation.ClosedAt:yyyy-MM-dd HH:mm}.");
        }

        if (reservation.CanceledAt.HasValue)
        {
            details.Add($"Canceled at {reservation.CanceledAt:yyyy-MM-dd HH:mm}.");
        }

        if (reservation.NoShowAt.HasValue)
        {
            details.Add($"No-show recorded at {reservation.NoShowAt:yyyy-MM-dd HH:mm}.");
        }

        return string.Join(" ", details);
    }

    private static string BuildStatusSummary(string status) =>
        status switch
        {
            AssetReservationStatuses.Approved => "Reservation is approved and waiting for checkout.",
            AssetReservationStatuses.Reserved => "Reservation is reserved and ready for checkout.",
            AssetReservationStatuses.CheckedOut => "Reservation is checked out.",
            AssetReservationStatuses.InUse => "Reservation is in use.",
            AssetReservationStatuses.Returned => "Reservation has been returned and is waiting for inspection.",
            AssetReservationStatuses.Inspection => "Reservation inspection is in progress.",
            _ => $"Reservation is {Humanize(status)}.",
        };

    private static string BuildStatusDetail(string status, AssetReadinessResponse readiness) =>
        status switch
        {
            AssetReservationStatuses.Approved => "Approval has been captured. The reservation can move to the reserved state when the asset is ready.",
            AssetReservationStatuses.Reserved => "The reservation is allocated to this asset and can move to checkout once the handoff starts.",
            AssetReservationStatuses.CheckedOut => "The asset has left custody for this reservation.",
            AssetReservationStatuses.InUse => "The asset is actively in use for the reservation window.",
            AssetReservationStatuses.Returned => "The asset has been returned and should be inspected before the reservation closes.",
            AssetReservationStatuses.Inspection => "The return inspection is in progress.",
            _ => $"Readiness basis: {Humanize(readiness.ReadinessBasis)}.",
        };

    private static bool IsTerminalStatus(string status) =>
        AssetReservationStatuses.Terminal.Contains(status);

    private static bool IsPermissiveQualificationOutcome(string outcome) =>
        string.Equals(outcome, "allow", StringComparison.OrdinalIgnoreCase)
        || string.Equals(outcome, "warn", StringComparison.OrdinalIgnoreCase)
        || string.Equals(outcome, "waived", StringComparison.OrdinalIgnoreCase);

    private static string Humanize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Replace('_', ' ').Replace('-', ' ').Trim();
    }

    private static string NormalizeRequiredText(string? value, int maxLength, string label)
    {
        var normalized = NormalizeOptionalText(value, maxLength);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new StlApiException(
                "asset_reservations.required_field",
                $"{label} is required.",
                400);
        }

        return normalized;
    }

    private static string? NormalizeOptionalText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new StlApiException(
                "asset_reservations.text_too_long",
                $"Field value must be {maxLength} characters or fewer.",
                400);
        }

        return normalized;
    }

    private static string NormalizeRequiredPersonId(string? value, string label)
    {
        var normalized = NormalizeOptionalPersonId(value);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new StlApiException(
                "asset_reservations.required_person",
                $"{label} is required.",
                400);
        }

        return normalized;
    }

    private static string? NormalizeOptionalPersonId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length is < 8 or > 128)
        {
            throw new StlApiException(
                "asset_reservations.invalid_person",
                "Person id must be between 8 and 128 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeStatus(string value) => value.Trim().ToLowerInvariant();

    private sealed record ReservationDecision(string Status, string Summary, string Detail);
}
