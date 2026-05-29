using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Services;

public sealed class TripAuditTrailService(
    RoutArrDbContext db,
    TripService tripService,
    RoutArrAuthorizationService authorization,
    IRoutArrAuditService audit)
{
    public const string ListAction = "trip_audit_trail.list";

    public async Task<TripAuditTrailResponse> ListAsync(
        ClaimsPrincipal principal,
        Guid tripId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireTripsRead(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();

        var trip = await tripService.GetAsync(tenantId, tripId, cancellationToken);
        authorization.RequireTripAccess(
            principal,
            trip.CreatedByUserId,
            trip.AssignedDriverPersonId);

        var cappedLimit = Math.Clamp(limit, 1, 100);
        var tripIdText = tripId.ToString();

        var proofIds = await db.TripProofRecords
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TripId == tripId)
            .Select(x => x.Id.ToString())
            .ToListAsync(cancellationToken);

        var dvirIds = await db.TripDvirInspections
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TripId == tripId)
            .Select(x => x.Id.ToString())
            .ToListAsync(cancellationToken);

        var entries = await db.AuditEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Where(x =>
                (x.TargetType == "trip" && x.TargetId == tripIdText)
                || (x.TargetType == "trip_capture_readiness" && x.TargetId == tripIdText)
                || (x.TargetType == "trip_proof" && x.TargetId != null && proofIds.Contains(x.TargetId))
                || (x.TargetType == "trip_dvir" && x.TargetId != null && dvirIds.Contains(x.TargetId)))
            .OrderByDescending(x => x.OccurredAt)
            .Take(cappedLimit)
            .Select(x => new TripAuditTrailEntryResponse(
                x.Id,
                x.ActorUserId,
                x.Action,
                x.TargetType,
                x.TargetId,
                x.Result,
                x.ReasonCode,
                x.CorrelationId,
                x.OccurredAt))
            .ToListAsync(cancellationToken);

        await audit.WriteAsync(
            ListAction,
            tenantId,
            actorUserId,
            "trip",
            tripIdText,
            entries.Count.ToString(),
            cancellationToken: cancellationToken);

        return new TripAuditTrailResponse(tripId, entries);
    }
}
