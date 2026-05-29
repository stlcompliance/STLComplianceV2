using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Services;

public sealed class TripExecutionCaptureService(
    RoutArrDbContext db,
    TripService tripService,
    TripCaptureAttachmentService attachmentService,
    RoutArrAuthorizationService authorization,
    IRoutArrAuditService audit)
{
    public const string SettingsReadAction = "trip_execution_settings.read";
    public const string SettingsUpdateAction = "trip_execution_settings.update";
    public const string CaptureReadinessReadAction = "trip_capture_readiness.read";

    public async Task<TripExecutionSettingsResponse> GetSettingsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantTripExecutionSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return entity is null
            ? MapSettingsResponse(TripExecutionSettingsSnapshot.Defaults, null)
            : MapSettingsResponse(TripExecutionCaptureRules.ResolveSettings(entity), entity.UpdatedAt);
    }

    public async Task<TripExecutionSettingsResponse> UpsertSettingsAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertTripExecutionSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantTripExecutionSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantTripExecutionSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantTripExecutionSettings.Add(entity);
        }

        entity.RequirePreTripDvirBeforeStart = request.RequirePreTripDvirBeforeStart;
        entity.RequirePostTripDvirBeforeComplete = request.RequirePostTripDvirBeforeComplete;
        entity.RequireDeliveryProofBeforeComplete = request.RequireDeliveryProofBeforeComplete;
        entity.RequirePickupProofBeforeStart = request.RequirePickupProofBeforeStart;
        entity.BlockTripStartOnDvirFail = request.BlockTripStartOnDvirFail;
        entity.BlockTripCompleteOnDvirFail = request.BlockTripCompleteOnDvirFail;
        entity.RequirePickupProofPhotoBeforeStart = request.RequirePickupProofPhotoBeforeStart;
        entity.RequireDeliveryProofPhotoBeforeComplete = request.RequireDeliveryProofPhotoBeforeComplete;
        entity.RequireDeliverySignatureBeforeComplete = request.RequireDeliverySignatureBeforeComplete;
        entity.RequirePreTripDvirPhotoBeforeStart = request.RequirePreTripDvirPhotoBeforeStart;
        entity.RequirePostTripDvirPhotoBeforeComplete = request.RequirePostTripDvirPhotoBeforeComplete;
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            SettingsUpdateAction,
            tenantId,
            actorUserId,
            "tenant_trip_execution_settings",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapSettingsResponse(TripExecutionCaptureRules.ResolveSettings(entity), entity.UpdatedAt);
    }

    public async Task<TripCaptureReadinessResponse> GetCaptureReadinessAsync(
        ClaimsPrincipal principal,
        Guid tripId,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireTripProofRead(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();

        var trip = await tripService.GetAsync(tenantId, tripId, cancellationToken);
        EnsureTripReadAccess(principal, trip.AssignedDriverPersonId);

        var readiness = await BuildReadinessAsync(tenantId, tripId, trip.DispatchStatus, cancellationToken);

        await audit.WriteAsync(
            CaptureReadinessReadAction,
            tenantId,
            actorUserId,
            "trip_capture_readiness",
            tripId.ToString(),
            readiness.CanStartTrip ? "start_ready" : "start_blocked",
            cancellationToken: cancellationToken);

        return readiness;
    }

    public async Task<TripCaptureReadinessResponse> BuildReadinessAsync(
        Guid tenantId,
        Guid tripId,
        string dispatchStatus,
        CancellationToken cancellationToken = default)
    {
        var settingsEntity = await db.TenantTripExecutionSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        var settings = TripExecutionCaptureRules.ResolveSettings(settingsEntity);

        var proofTypes = await db.TripProofRecords
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TripId == tripId)
            .Select(x => x.ProofType)
            .ToListAsync(cancellationToken);

        var dvirRows = await db.TripDvirInspections
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TripId == tripId)
            .Select(x => new { x.Phase, x.Result })
            .ToListAsync(cancellationToken);

        var hasPickup = proofTypes.Any(x =>
            string.Equals(x, TripProofTypes.Pickup, StringComparison.OrdinalIgnoreCase));
        var hasDelivery = proofTypes.Any(x =>
            string.Equals(x, TripProofTypes.Delivery, StringComparison.OrdinalIgnoreCase));

        var preTrip = dvirRows.FirstOrDefault(x =>
            string.Equals(x.Phase, DvirInspectionPhases.PreTrip, StringComparison.OrdinalIgnoreCase));
        var postTrip = dvirRows.FirstOrDefault(x =>
            string.Equals(x.Phase, DvirInspectionPhases.PostTrip, StringComparison.OrdinalIgnoreCase));

        var attachmentState = await attachmentService.BuildAttachmentStateAsync(tenantId, tripId, cancellationToken);

        return TripExecutionCaptureRules.BuildReadiness(
            tripId,
            dispatchStatus,
            settings,
            hasPickup,
            hasDelivery,
            preTrip is not null,
            postTrip is not null,
            preTrip?.Result,
            postTrip?.Result,
            attachmentState);
    }

    public async Task AssertCanStartAsync(
        Guid tenantId,
        Guid tripId,
        string dispatchStatus,
        CancellationToken cancellationToken = default)
    {
        var readiness = await BuildReadinessAsync(tenantId, tripId, dispatchStatus, cancellationToken);
        TripExecutionCaptureRules.EnsureCanStart(readiness);
    }

    public async Task AssertCanCompleteAsync(
        Guid tenantId,
        Guid tripId,
        string dispatchStatus,
        CancellationToken cancellationToken = default)
    {
        var readiness = await BuildReadinessAsync(tenantId, tripId, dispatchStatus, cancellationToken);
        TripExecutionCaptureRules.EnsureCanComplete(readiness);
    }

    private void EnsureTripReadAccess(ClaimsPrincipal principal, string? assignedPersonId)
    {
        if (authorization.CanViewAllTrips(principal))
        {
            return;
        }

        var actorPersonId = principal.GetPersonId().ToString();
        if (!string.IsNullOrWhiteSpace(assignedPersonId)
            && string.Equals(assignedPersonId.Trim(), actorPersonId, StringComparison.Ordinal))
        {
            return;
        }

        throw new STLCompliance.Shared.Contracts.StlApiException(
            "auth.forbidden",
            "You can only read capture readiness for trips assigned to you.",
            403);
    }

    private static TripExecutionSettingsResponse MapSettingsResponse(
        TripExecutionSettingsSnapshot settings,
        DateTimeOffset? updatedAt) =>
        new(
            settings.RequirePreTripDvirBeforeStart,
            settings.RequirePostTripDvirBeforeComplete,
            settings.RequireDeliveryProofBeforeComplete,
            settings.RequirePickupProofBeforeStart,
            settings.BlockTripStartOnDvirFail,
            settings.BlockTripCompleteOnDvirFail,
            settings.RequirePickupProofPhotoBeforeStart,
            settings.RequireDeliveryProofPhotoBeforeComplete,
            settings.RequireDeliverySignatureBeforeComplete,
            settings.RequirePreTripDvirPhotoBeforeStart,
            settings.RequirePostTripDvirPhotoBeforeComplete,
            updatedAt);
}
