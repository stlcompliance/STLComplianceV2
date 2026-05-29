using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class DriverPortalService(
    RoutArrDbContext db,
    TripService tripService,
    TripExecutionCaptureService captureService,
    TripCaptureAttachmentService attachmentService,
    RoutArrAuthorizationService authorization,
    IRoutArrAuditService audit)
{
    public const string ReadAction = "driver_portal.schedule.read";
    public const string StartAction = "driver_portal.trip.start";
    public const string CompleteAction = "driver_portal.trip.complete";
    public const string CloseAction = "driver_portal.trip.close";
    public const string DispatchAction = "driver_portal.trip.dispatch";

    public async Task<DriverPortalScheduleResponse> GetScheduleAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireDriverPortalRead(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();
        var personId = principal.GetPersonId().ToString();

        var now = DateTimeOffset.UtcNow;
        var todayStart = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero);
        var todayEnd = todayStart.AddDays(1);
        var upcomingEnd = todayStart.AddDays(8);

        var trips = await db.Trips
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && x.AssignedDriverPersonId == personId
                && (TripDispatchStatuses.Active.Contains(x.DispatchStatus)
                    || (x.DispatchStatus == TripDispatchStatuses.Completed && x.ClosedAt == null)))
            .OrderBy(x => x.ScheduledStartAt ?? DateTimeOffset.MaxValue)
            .ThenBy(x => x.TripNumber)
            .ToListAsync(cancellationToken);

        var todayTrips = new List<DriverPortalTripRow>();
        var upcomingTrips = new List<DriverPortalTripRow>();

        var tripIds = trips.Select(x => x.Id).ToList();
        var proofCounts = tripIds.Count == 0
            ? new Dictionary<Guid, int>()
            : await db.TripProofRecords
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && tripIds.Contains(x.TripId))
                .GroupBy(x => x.TripId)
                .Select(g => new { TripId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TripId, x => x.Count, cancellationToken);

        var dvirPhases = await db.TripDvirInspections
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && tripIds.Contains(x.TripId))
            .Select(x => new { x.TripId, x.Phase })
            .ToListAsync(cancellationToken);

        var preTripDvir = dvirPhases
            .Where(x => string.Equals(x.Phase, DvirInspectionPhases.PreTrip, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.TripId)
            .ToHashSet();
        var postTripDvir = dvirPhases
            .Where(x => string.Equals(x.Phase, DvirInspectionPhases.PostTrip, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.TripId)
            .ToHashSet();

        var settingsEntity = await db.TenantTripExecutionSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        var settings = TripExecutionCaptureRules.ResolveSettings(settingsEntity);

        var proofTypeRows = tripIds.Count == 0
            ? []
            : await db.TripProofRecords
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && tripIds.Contains(x.TripId))
                .Select(x => new { x.TripId, x.ProofType })
                .ToListAsync(cancellationToken);

        var proofTypesByTrip = proofTypeRows
            .GroupBy(x => x.TripId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ProofType).ToList());

        var dvirRows = tripIds.Count == 0
            ? []
            : await db.TripDvirInspections
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && tripIds.Contains(x.TripId))
                .Select(x => new { x.TripId, x.Phase, x.Result })
                .ToListAsync(cancellationToken);

        var dvirResultsByTrip = dvirRows
            .GroupBy(x => x.TripId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => (x.Phase, x.Result)).ToList());

        foreach (var trip in trips)
        {
            proofCounts.TryGetValue(trip.Id, out var proofCount);
            proofTypesByTrip.TryGetValue(trip.Id, out var proofTypes);
            proofTypes ??= [];
            dvirResultsByTrip.TryGetValue(trip.Id, out var tripDvirs);
            tripDvirs ??= [];

            var preTripResult = tripDvirs
                .Where(x => string.Equals(x.Phase, DvirInspectionPhases.PreTrip, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Result)
                .FirstOrDefault();
            var postTripResult = tripDvirs
                .Where(x => string.Equals(x.Phase, DvirInspectionPhases.PostTrip, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Result)
                .FirstOrDefault();

            var attachmentState = await attachmentService.BuildAttachmentStateAsync(tenantId, trip.Id, cancellationToken);

            var readiness = TripExecutionCaptureRules.BuildReadiness(
                trip.Id,
                trip.DispatchStatus,
                settings,
                proofTypes.Any(x => string.Equals(x, TripProofTypes.Pickup, StringComparison.OrdinalIgnoreCase)),
                proofTypes.Any(x => string.Equals(x, TripProofTypes.Delivery, StringComparison.OrdinalIgnoreCase)),
                preTripDvir.Contains(trip.Id),
                postTripDvir.Contains(trip.Id),
                preTripResult,
                postTripResult,
                attachmentState);

            var row = MapTripRow(
                trip,
                proofCount,
                preTripDvir.Contains(trip.Id),
                postTripDvir.Contains(trip.Id),
                readiness.CanStartTrip,
                readiness.CanCompleteTrip,
                trip.ClosedAt);
            if (IsTodayTrip(trip, todayStart, todayEnd))
            {
                todayTrips.Add(row);
            }
            else if (IsUpcomingTrip(trip, todayEnd, upcomingEnd))
            {
                upcomingTrips.Add(row);
            }
        }

        await audit.WriteAsync(
            ReadAction,
            tenantId,
            actorUserId,
            "driver_portal",
            personId,
            $"{todayTrips.Count}/{upcomingTrips.Count}",
            cancellationToken: cancellationToken);

        return new DriverPortalScheduleResponse(
            todayStart,
            todayEnd,
            upcomingEnd,
            todayTrips,
            upcomingTrips,
            now);
    }

    public async Task<TripDetailResponse> StartTripAsync(
        ClaimsPrincipal principal,
        Guid tripId,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteTransitionAsync(
            principal,
            tripId,
            TripDispatchStatuses.InProgress,
            StartAction,
            cancellationToken);
    }

    public async Task<TripDetailResponse> CompleteTripAsync(
        ClaimsPrincipal principal,
        Guid tripId,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteTransitionAsync(
            principal,
            tripId,
            TripDispatchStatuses.Completed,
            CompleteAction,
            cancellationToken);
    }

    public async Task<TripDetailResponse> CloseTripAsync(
        ClaimsPrincipal principal,
        Guid tripId,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireDriverPortalExecute(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();
        var actorPersonId = principal.GetPersonId().ToString();

        var existing = await tripService.GetAsync(tenantId, tripId, cancellationToken);
        EnsureAssignedDriver(existing.AssignedDriverPersonId, actorPersonId);

        if (string.Equals(existing.DispatchStatus, TripDispatchStatuses.Completed, StringComparison.OrdinalIgnoreCase))
        {
            return await tripService.AcknowledgeDriverCloseAsync(
                tenantId,
                actorUserId,
                tripId,
                actorPersonId,
                cancellationToken);
        }

        return await ExecuteTransitionAsync(
            principal,
            tripId,
            TripDispatchStatuses.Completed,
            CloseAction,
            cancellationToken);
    }

    public async Task<TripDetailResponse> DispatchTripAsync(
        ClaimsPrincipal principal,
        Guid tripId,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteTransitionAsync(
            principal,
            tripId,
            TripDispatchStatuses.Dispatched,
            DispatchAction,
            cancellationToken);
    }

    private async Task<TripDetailResponse> ExecuteTransitionAsync(
        ClaimsPrincipal principal,
        Guid tripId,
        string targetStatus,
        string auditAction,
        CancellationToken cancellationToken)
    {
        authorization.RequireDriverPortalExecute(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();
        var actorPersonId = principal.GetPersonId().ToString();

        var existing = await tripService.GetAsync(tenantId, tripId, cancellationToken);
        EnsureAssignedDriver(existing.AssignedDriverPersonId, actorPersonId);

        if (string.Equals(targetStatus, TripDispatchStatuses.InProgress, StringComparison.OrdinalIgnoreCase))
        {
            await captureService.AssertCanStartAsync(tenantId, tripId, existing.DispatchStatus, cancellationToken);
        }
        else if (string.Equals(targetStatus, TripDispatchStatuses.Completed, StringComparison.OrdinalIgnoreCase)
                 && string.Equals(auditAction, CompleteAction, StringComparison.Ordinal))
        {
            await captureService.AssertCanCompleteAsync(tenantId, tripId, existing.DispatchStatus, cancellationToken);
        }

        var canManageAny = authorization.CanViewAllTrips(principal)
            && !string.Equals(
                principal.GetTenantRoleKey(),
                "routarr_driver",
                StringComparison.OrdinalIgnoreCase);

        var updated = await tripService.UpdateDispatchStatusAsync(
            tenantId,
            actorUserId,
            tripId,
            new UpdateTripDispatchStatusRequest(targetStatus),
            canManageAny,
            actorPersonId,
            cancellationToken);

        await audit.WriteAsync(
            auditAction,
            tenantId,
            actorUserId,
            "trip",
            tripId.ToString(),
            targetStatus,
            cancellationToken: cancellationToken);

        return updated;
    }

    private static void EnsureAssignedDriver(string? assignedPersonId, string actorPersonId)
    {
        if (string.IsNullOrWhiteSpace(assignedPersonId)
            || !string.Equals(assignedPersonId.Trim(), actorPersonId, StringComparison.Ordinal))
        {
            throw new StlApiException(
                "driver_portal.not_assigned",
                "You can only execute trips assigned to you.",
                403);
        }
    }

    private static bool IsTodayTrip(Trip trip, DateTimeOffset todayStart, DateTimeOffset todayEnd)
    {
        if (trip.DispatchStatus is TripDispatchStatuses.Dispatched or TripDispatchStatuses.InProgress)
        {
            return true;
        }

        if (string.Equals(trip.DispatchStatus, TripDispatchStatuses.Completed, StringComparison.OrdinalIgnoreCase)
            && !trip.ClosedAt.HasValue)
        {
            if (trip.CompletedAt.HasValue
                && trip.CompletedAt.Value >= todayStart
                && trip.CompletedAt.Value < todayEnd)
            {
                return true;
            }

            if (trip.ScheduledStartAt.HasValue
                && trip.ScheduledStartAt.Value >= todayStart
                && trip.ScheduledStartAt.Value < todayEnd)
            {
                return true;
            }

            return !trip.ScheduledStartAt.HasValue && trip.UpdatedAt >= todayStart;
        }

        if (trip.ScheduledStartAt.HasValue
            && trip.ScheduledStartAt.Value >= todayStart
            && trip.ScheduledStartAt.Value < todayEnd)
        {
            return true;
        }

        return !trip.ScheduledStartAt.HasValue
            && trip.DispatchStatus is TripDispatchStatuses.Assigned
            && trip.UpdatedAt >= todayStart;
    }

    private static bool IsUpcomingTrip(Trip trip, DateTimeOffset todayEnd, DateTimeOffset upcomingEnd) =>
        trip.ScheduledStartAt.HasValue
        && trip.ScheduledStartAt.Value >= todayEnd
        && trip.ScheduledStartAt.Value < upcomingEnd
        && trip.DispatchStatus is TripDispatchStatuses.Planned
            or TripDispatchStatuses.Assigned
            or TripDispatchStatuses.Dispatched;

    private static DriverPortalTripRow MapTripRow(
        Trip trip,
        int proofCount,
        bool hasPreTripDvir,
        bool hasPostTripDvir,
        bool captureStartReady,
        bool captureCompleteReady,
        DateTimeOffset? closedAt)
    {
        var status = trip.DispatchStatus;
        var canStartStatus = string.Equals(status, TripDispatchStatuses.Dispatched, StringComparison.OrdinalIgnoreCase);
        var canCompleteStatus = string.Equals(status, TripDispatchStatuses.InProgress, StringComparison.OrdinalIgnoreCase);
        var isCompleted = string.Equals(status, TripDispatchStatuses.Completed, StringComparison.OrdinalIgnoreCase);
        var pendingDriverClose = isCompleted && !closedAt.HasValue;
        return new DriverPortalTripRow(
            trip.Id,
            trip.TripNumber,
            trip.Title,
            status,
            trip.VehicleRefKey,
            trip.ScheduledStartAt,
            trip.ScheduledEndAt,
            trip.DispatchedAt,
            trip.StartedAt,
            trip.CompletedAt,
            closedAt,
            CanDispatch: string.Equals(status, TripDispatchStatuses.Assigned, StringComparison.OrdinalIgnoreCase),
            CanStart: canStartStatus && captureStartReady,
            CanComplete: canCompleteStatus && captureCompleteReady,
            CanClose: canCompleteStatus || pendingDriverClose,
            proofCount,
            hasPreTripDvir,
            hasPostTripDvir,
            captureStartReady,
            captureCompleteReady);
    }
}
