using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class TripProofDvirService(
    RoutArrDbContext db,
    TripService tripService,
    RoutArrAuthorizationService authorization,
    IRoutArrAuditService audit)
{
    public const string ProofCreateAction = "trip_proof.create";
    public const string ProofListAction = "trip_proof.list";
    public const string DvirSubmitAction = "trip_dvir.submit";
    public const string DvirListAction = "trip_dvir.list";
    public const string ExecutionReadAction = "trip_execution.summary.read";

    public async Task<TripProofRecordResponse> CreateProofAsync(
        ClaimsPrincipal principal,
        Guid tripId,
        CreateTripProofRequest request,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireTripProofWrite(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();
        var actorPersonId = principal.GetPersonId().ToString();

        var trip = await tripService.GetAsync(tenantId, tripId, cancellationToken);
        EnsureTripProofAccess(principal, trip.AssignedDriverPersonId, write: true);

        var proofType = NormalizeProofType(request.ProofType);
        var now = DateTimeOffset.UtcNow;
        var entity = new TripProofRecord
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TripId = tripId,
            ProofType = proofType,
            CapturedByPersonId = actorPersonId,
            VehicleRefKey = string.IsNullOrWhiteSpace(request.VehicleRefKey)
                ? trip.VehicleRefKey
                : request.VehicleRefKey.Trim(),
            ReferenceKey = request.ReferenceKey?.Trim() ?? string.Empty,
            Notes = request.Notes?.Trim() ?? string.Empty,
            CapturedAt = request.CapturedAt ?? now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.TripProofRecords.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            ProofCreateAction,
            tenantId,
            actorUserId,
            "trip_proof",
            entity.Id.ToString(),
            proofType,
            cancellationToken: cancellationToken);

        return MapProof(entity);
    }

    public async Task<TripProofListResponse> ListProofsAsync(
        ClaimsPrincipal principal,
        Guid tripId,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireTripProofRead(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();

        var trip = await tripService.GetAsync(tenantId, tripId, cancellationToken);
        EnsureTripProofAccess(principal, trip.AssignedDriverPersonId, write: false);

        var items = await db.TripProofRecords
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TripId == tripId)
            .OrderByDescending(x => x.CapturedAt)
            .ToListAsync(cancellationToken);

        await audit.WriteAsync(
            ProofListAction,
            tenantId,
            actorUserId,
            "trip",
            tripId.ToString(),
            items.Count.ToString(),
            cancellationToken: cancellationToken);

        return new TripProofListResponse(tripId, items.Select(MapProof).ToList());
    }

    public async Task<TripDvirInspectionResponse> SubmitDvirAsync(
        ClaimsPrincipal principal,
        Guid tripId,
        SubmitTripDvirRequest request,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireDvirPerform(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();
        var actorPersonId = principal.GetPersonId().ToString();

        var trip = await tripService.GetAsync(tenantId, tripId, cancellationToken);
        EnsureTripProofAccess(principal, trip.AssignedDriverPersonId, write: true);

        var phase = NormalizeDvirPhase(request.Phase);
        var result = NormalizeDvirResult(request.Result);
        var vehicleRef = string.IsNullOrWhiteSpace(request.VehicleRefKey)
            ? trip.VehicleRefKey ?? string.Empty
            : request.VehicleRefKey.Trim();
        var now = DateTimeOffset.UtcNow;

        var existing = await db.TripDvirInspections
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.TripId == tripId && x.Phase == phase,
                cancellationToken);

        if (existing is null)
        {
            existing = new TripDvirInspection
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TripId = tripId,
                Phase = phase,
                CreatedAt = now,
            };
            db.TripDvirInspections.Add(existing);
        }

        existing.VehicleRefKey = vehicleRef;
        existing.Result = result;
        existing.OdometerReading = request.OdometerReading;
        existing.DefectNotes = request.DefectNotes?.Trim() ?? string.Empty;
        existing.SubmittedByPersonId = actorPersonId;
        existing.SubmittedAt = now;
        existing.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            DvirSubmitAction,
            tenantId,
            actorUserId,
            "trip_dvir",
            existing.Id.ToString(),
            $"{phase}:{result}",
            cancellationToken: cancellationToken);

        return MapDvir(existing);
    }

    public async Task<TripDvirListResponse> ListDvirAsync(
        ClaimsPrincipal principal,
        Guid tripId,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireTripProofRead(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();

        var trip = await tripService.GetAsync(tenantId, tripId, cancellationToken);
        EnsureTripProofAccess(principal, trip.AssignedDriverPersonId, write: false);

        var items = await db.TripDvirInspections
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TripId == tripId)
            .OrderBy(x => x.Phase)
            .ToListAsync(cancellationToken);

        await audit.WriteAsync(
            DvirListAction,
            tenantId,
            actorUserId,
            "trip",
            tripId.ToString(),
            items.Count.ToString(),
            cancellationToken: cancellationToken);

        return new TripDvirListResponse(tripId, items.Select(MapDvir).ToList());
    }

    public async Task<TripExecutionSummaryResponse> GetExecutionSummaryAsync(
        ClaimsPrincipal principal,
        Guid tripId,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireTripProofRead(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();

        var trip = await tripService.GetAsync(tenantId, tripId, cancellationToken);
        EnsureTripProofAccess(principal, trip.AssignedDriverPersonId, write: false);

        var proofs = await db.TripProofRecords
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TripId == tripId)
            .OrderByDescending(x => x.CapturedAt)
            .ToListAsync(cancellationToken);

        var dvirs = await db.TripDvirInspections
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TripId == tripId)
            .OrderBy(x => x.Phase)
            .ToListAsync(cancellationToken);

        await audit.WriteAsync(
            ExecutionReadAction,
            tenantId,
            actorUserId,
            "trip_execution",
            tripId.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return new TripExecutionSummaryResponse(
            tripId,
            trip.TripNumber,
            trip.AssignedDriverPersonId,
            proofs.Select(MapProof).ToList(),
            dvirs.Select(MapDvir).ToList(),
            dvirs.Any(x => string.Equals(x.Phase, DvirInspectionPhases.PreTrip, StringComparison.OrdinalIgnoreCase)),
            dvirs.Any(x => string.Equals(x.Phase, DvirInspectionPhases.PostTrip, StringComparison.OrdinalIgnoreCase)));
    }

    private void EnsureTripProofAccess(ClaimsPrincipal principal, string? assignedPersonId, bool write)
    {
        if (!write && authorization.CanViewAllTrips(principal))
        {
            return;
        }

        var actorPersonId = principal.GetPersonId().ToString();
        if (!string.IsNullOrWhiteSpace(assignedPersonId)
            && string.Equals(assignedPersonId.Trim(), actorPersonId, StringComparison.Ordinal))
        {
            return;
        }

        throw new StlApiException(
            write ? "trip_proof.not_assigned" : "auth.forbidden",
            write
                ? "Only the assigned driver can capture proof or DVIR for this trip."
                : "You can only read proof and DVIR for trips assigned to you.",
            403);
    }

    private static string NormalizeProofType(string proofType)
    {
        var normalized = proofType?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!TripProofTypes.All.Contains(normalized))
        {
            throw new StlApiException(
                "trip_proof.invalid_type",
                "Proof type must be pickup or delivery.",
                400);
        }

        return normalized;
    }

    private static string NormalizeDvirPhase(string phase)
    {
        var normalized = phase?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!DvirInspectionPhases.All.Contains(normalized))
        {
            throw new StlApiException(
                "trip_dvir.invalid_phase",
                "DVIR phase must be pre_trip or post_trip.",
                400);
        }

        return normalized;
    }

    private static string NormalizeDvirResult(string result)
    {
        var normalized = result?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!DvirInspectionResults.All.Contains(normalized))
        {
            throw new StlApiException(
                "trip_dvir.invalid_result",
                "DVIR result must be pass, fail, or conditional.",
                400);
        }

        return normalized;
    }

    private static TripProofRecordResponse MapProof(TripProofRecord entity) =>
        new(
            entity.Id,
            entity.TripId,
            entity.ProofType,
            entity.CapturedByPersonId,
            entity.VehicleRefKey,
            entity.ReferenceKey,
            entity.Notes,
            entity.CapturedAt,
            entity.CreatedAt);

    private static TripDvirInspectionResponse MapDvir(TripDvirInspection entity) =>
        new(
            entity.Id,
            entity.TripId,
            entity.Phase,
            entity.VehicleRefKey,
            entity.Result,
            entity.OdometerReading,
            entity.DefectNotes,
            entity.SubmittedByPersonId,
            entity.SubmittedAt);
}
