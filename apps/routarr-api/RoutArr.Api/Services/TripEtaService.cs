using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Services;

public sealed class TripEtaService(
    RoutArrDbContext db,
    IRoutArrAuditService audit)
{
    public const string ReadAction = "trip_eta.read";
    public const string UpdateAction = "routarr.eta.updated";

    public async Task<TripEtaResponse> GetAsync(
        Guid tenantId,
        Guid tripId,
        CancellationToken cancellationToken = default)
    {
        var trip = await db.Trips
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == tripId, cancellationToken)
            ?? throw new StlApiException("trip.not_found", "Trip was not found.", 404);

        var latest = await db.AuditEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.Action == UpdateAction
                && x.TargetType == "trip"
                && x.TargetId == tripId.ToString())
            .OrderByDescending(x => x.OccurredAt)
            .FirstOrDefaultAsync(cancellationToken);

        var payload = TryParsePayload(latest?.Result);

        return new TripEtaResponse(
            tripId,
            payload?.Eta ?? trip.ScheduledEndAt ?? trip.ScheduledStartAt,
            payload?.EtaSource,
            payload?.Confidence,
            payload?.Reason,
            latest?.OccurredAt,
            latest?.ActorUserId);
    }

    public async Task<TripEtaResponse> UpdateAsync(
        ClaimsPrincipal principal,
        UpdateTripEtaRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();
        var tripId = request.TripId;

        var trip = await db.Trips
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == tripId, cancellationToken)
            ?? throw new StlApiException("trip.not_found", "Trip was not found.", 404);

        var eta = request.Eta ?? trip.ScheduledEndAt ?? trip.ScheduledStartAt;
        var payload = new TripEtaPayload(
            eta,
            request.EtaSource?.Trim(),
            request.Confidence?.Trim(),
            request.Reason?.Trim());

        await audit.WriteAsync(
            UpdateAction,
            tenantId,
            actorUserId,
            "trip",
            tripId.ToString(),
            JsonSerializer.Serialize(payload),
            "updated",
            cancellationToken);

        return new TripEtaResponse(
            tripId,
            eta,
            request.EtaSource,
            request.Confidence,
            request.Reason,
            DateTimeOffset.UtcNow,
            principal.GetPersonId());
    }

    private static TripEtaPayload? TryParsePayload(string? result)
    {
        if (string.IsNullOrWhiteSpace(result))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<TripEtaPayload>(result);
        }
        catch
        {
            return null;
        }
    }

    private sealed record TripEtaPayload(
        DateTimeOffset? Eta,
        string? EtaSource,
        string? Confidence,
        string? Reason);
}
