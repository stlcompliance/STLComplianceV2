using System.Security.Claims;
using NexArr.Api.Contracts;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class FieldCompanionClockService(FieldCompanionProductClient productClient)
{
    public async Task<FieldCompanionClockStatusResponse> GetStatusAsync(
        ClaimsPrincipal principal,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        EnsureClockAccess(principal);
        var upstream = await productClient.GetStaffArrFieldCompanionClockStatusAsync(accessToken, cancellationToken);
        return new FieldCompanionClockStatusResponse(
            upstream.CurrentState,
            upstream.LatestEvent is null ? null : MapEvent(upstream.LatestEvent),
            upstream.RecentEvents.Select(MapEvent).ToList());
    }

    public async Task<FieldCompanionClockSubmissionResponse> SubmitAsync(
        ClaimsPrincipal principal,
        string accessToken,
        SubmitFieldCompanionClockEventRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureClockAccess(principal);
        ValidateRequest(request);

        var upstream = await productClient.SubmitStaffArrFieldCompanionClockEventAsync(
            accessToken,
            new StaffArrSubmitFieldCompanionClockEventUpstreamRequest(
                request.EventType.Trim(),
                request.EventTimestamp,
                request.CapturedAt,
                request.Timezone.Trim(),
                request.IdempotencyKey.Trim(),
                request.SourceDeviceId?.Trim(),
                request.GeoPoint?.Trim(),
                request.SiteRef?.Trim(),
                request.LocationRef?.Trim(),
                request.Notes?.Trim()),
            cancellationToken);

        return new FieldCompanionClockSubmissionResponse(
            upstream.ClockEventId,
            upstream.Created,
            upstream.ConflictDetected,
            upstream.Status,
            upstream.CurrentState,
            MapEvent(upstream.Event));
    }

    private static void EnsureClockAccess(ClaimsPrincipal principal)
    {
        FieldCompanionFieldInboxService.RequireFieldCompanionAccess(principal);
        if (principal.GetPersonId() == Guid.Empty)
        {
            throw new StlApiException(
                "fieldcompanion.clock.person_required",
                "Field Companion clock actions require a linked worker person record.",
                403);
        }
    }

    private static void ValidateRequest(SubmitFieldCompanionClockEventRequest request)
    {
        var normalizedEventType = request.EventType.Trim().ToLowerInvariant();
        if (normalizedEventType is not "clock_in" and not "clock_out")
        {
            throw new StlApiException(
                "fieldcompanion.clock.invalid_event_type",
                "Field Companion clock currently supports clock_in and clock_out only.",
                400);
        }

        if (string.IsNullOrWhiteSpace(request.Timezone))
        {
            throw new StlApiException("fieldcompanion.clock.timezone_required", "Timezone is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            throw new StlApiException("fieldcompanion.clock.idempotency_required", "Idempotency key is required.", 400);
        }
    }

    private static FieldCompanionClockEventResponse MapEvent(StaffArrFieldCompanionClockEventUpstreamResponse item) =>
        new(
            item.Id,
            item.EventType,
            item.EventTimestamp,
            item.CapturedTimestamp,
            item.Timezone,
            item.SourceDeviceId,
            item.GeoPoint,
            item.SiteRef,
            item.LocationRef,
            item.Notes,
            item.AnomalyFlags);
}
