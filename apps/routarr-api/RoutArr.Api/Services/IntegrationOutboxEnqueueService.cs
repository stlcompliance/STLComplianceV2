using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public sealed record RoutArrIntegrationOutboxPayload(
    Guid TenantId,
    string Summary,
    Guid? TripId,
    string? TripNumber = null,
    string? DriverPersonId = null,
    string? VehicleRefKey = null,
    string? DispatchStatus = null,
    Guid? RouteId = null,
    Guid? StopId = null,
    string? StopKey = null,
    string? StopStatus = null,
    Guid? ProofId = null,
    string? ProofType = null,
    Guid? DvirId = null,
    string? DvirPhase = null,
    string? DvirResult = null,
    string? DefectNotes = null,
    Guid? ExceptionId = null,
    string? ExceptionKey = null,
    string? ExceptionCategory = null,
    string? IncidentType = null,
    string? IncidentSeverity = null,
    string? IncidentReviewStatus = null,
    string? IncidentRoutedProduct = null,
    string? OverrideTargetType = null,
    IReadOnlyList<string>? OverrideKinds = null,
    string? RouteNumber = null,
    string? RouteStatus = null);

public sealed class IntegrationOutboxEnqueueService(
    RoutArrDbContext db,
    IntegrationEventSettingsService settingsService)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task<Guid?> TryEnqueueRouteCreatedAsync(
        DispatchRoute route,
        CancellationToken cancellationToken = default) =>
        TryEnqueueAsync(
            route.TenantId,
            RoutArrIntegrationOutboxEventKinds.RouteCreated,
            "route",
            route.Id,
            BuildRoutePayload(route, "Route created"),
            cancellationToken: cancellationToken);

    public Task<Guid?> TryEnqueueRouteUpdatedAsync(
        DispatchRoute route,
        string summary,
        CancellationToken cancellationToken = default)
    {
        var suffix = route.UpdatedAt.ToUnixTimeMilliseconds().ToString();
        return TryEnqueueAsync(
            route.TenantId,
            RoutArrIntegrationOutboxEventKinds.RouteUpdated,
            "route",
            route.Id,
            BuildRoutePayload(route, summary),
            idempotencySuffix: suffix,
            cancellationToken: cancellationToken);
    }

    public Task<Guid?> TryEnqueueTripCreatedAsync(
        Trip trip,
        CancellationToken cancellationToken = default) =>
        TryEnqueueAsync(
            trip.TenantId,
            RoutArrIntegrationOutboxEventKinds.TripCreated,
            "trip",
            trip.Id,
            BuildTripPayload(trip, "Trip created"),
            cancellationToken: cancellationToken);

    public Task<Guid?> TryEnqueueTripReleasedAsync(
        Trip trip,
        CancellationToken cancellationToken = default) =>
        TryEnqueueAsync(
            trip.TenantId,
            RoutArrIntegrationOutboxEventKinds.TripReleased,
            "trip",
            trip.Id,
            BuildTripPayload(trip, "Trip released for dispatch"),
            cancellationToken: cancellationToken);

    public Task<Guid?> TryEnqueueTripDispatchedAsync(
        Trip trip,
        CancellationToken cancellationToken = default) =>
        TryEnqueueAsync(
            trip.TenantId,
            RoutArrIntegrationOutboxEventKinds.TripDispatched,
            "trip",
            trip.Id,
            BuildTripPayload(trip, "Trip dispatched"),
            cancellationToken: cancellationToken);

    public Task<Guid?> TryEnqueueTripAcceptedAsync(
        Trip trip,
        CancellationToken cancellationToken = default) =>
        TryEnqueueAsync(
            trip.TenantId,
            RoutArrIntegrationOutboxEventKinds.TripAccepted,
            "trip",
            trip.Id,
            BuildTripPayload(trip, "Trip accepted"),
            idempotencySuffix: trip.AcceptedAt?.ToUnixTimeMilliseconds().ToString(),
            cancellationToken: cancellationToken);

    public Task<Guid?> TryEnqueueTripStartedAsync(
        Trip trip,
        CancellationToken cancellationToken = default) =>
        TryEnqueueAsync(
            trip.TenantId,
            RoutArrIntegrationOutboxEventKinds.TripStarted,
            "trip",
            trip.Id,
            BuildTripPayload(trip, "Trip started"),
            cancellationToken: cancellationToken);

    public Task<Guid?> TryEnqueueTripCompletedAsync(
        Trip trip,
        CancellationToken cancellationToken = default) =>
        TryEnqueueAsync(
            trip.TenantId,
            RoutArrIntegrationOutboxEventKinds.TripCompleted,
            "trip",
            trip.Id,
            BuildTripPayload(trip, "Trip completed"),
            cancellationToken: cancellationToken);

    public Task<Guid?> TryEnqueueTripCancelledAsync(
        Trip trip,
        CancellationToken cancellationToken = default) =>
        TryEnqueueAsync(
            trip.TenantId,
            RoutArrIntegrationOutboxEventKinds.TripCancelled,
            "trip",
            trip.Id,
            BuildTripPayload(trip, "Trip cancelled"),
            cancellationToken: cancellationToken);

    public Task<Guid?> TryEnqueueDriverAssignmentChangedAsync(
        Trip trip,
        string driverPersonId,
        CancellationToken cancellationToken = default)
    {
        var suffix = Guid.NewGuid().ToString("N");
        return TryEnqueueAsync(
            trip.TenantId,
            RoutArrIntegrationOutboxEventKinds.DriverAssignmentChanged,
            "trip",
            trip.Id,
            BuildTripPayload(trip, "Driver assignment changed", driverPersonId),
            idempotencySuffix: suffix,
            cancellationToken: cancellationToken);
    }

    public Task<Guid?> TryEnqueueEquipmentAssignmentChangedAsync(
        Trip trip,
        string? vehicleRefKey,
        CancellationToken cancellationToken = default)
    {
        var suffix = Guid.NewGuid().ToString("N");
        return TryEnqueueAsync(
            trip.TenantId,
            RoutArrIntegrationOutboxEventKinds.EquipmentAssignmentChanged,
            "trip",
            trip.Id,
            BuildTripPayload(trip, "Equipment assignment changed") with { VehicleRefKey = vehicleRefKey },
            idempotencySuffix: suffix,
            cancellationToken: cancellationToken);
    }

    public Task<Guid?> TryEnqueueComplianceOverridePerformedAsync(
        Trip trip,
        string targetType,
        string? targetKey,
        IReadOnlyList<string> overrideKinds,
        CancellationToken cancellationToken = default)
    {
        if (overrideKinds.Count == 0)
        {
            return Task.FromResult<Guid?>(null);
        }

        var suffix = Guid.NewGuid().ToString("N");
        var payload = BuildTripPayload(trip, "Compliance override performed") with
        {
            DriverPersonId = string.Equals(targetType, "driver", StringComparison.OrdinalIgnoreCase)
                ? targetKey
                : trip.AssignedDriverPersonId,
            VehicleRefKey = string.Equals(targetType, "equipment", StringComparison.OrdinalIgnoreCase)
                ? targetKey
                : trip.VehicleRefKey,
            OverrideTargetType = targetType,
            OverrideKinds = overrideKinds,
        };

        return TryEnqueueAsync(
            trip.TenantId,
            RoutArrIntegrationOutboxEventKinds.ComplianceOverridePerformed,
            "trip",
            trip.Id,
            payload,
            idempotencySuffix: suffix,
            cancellationToken: cancellationToken);
    }

    public Task<Guid?> TryEnqueueDispatchOverridePerformedAsync(
        Trip trip,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var suffix = Guid.NewGuid().ToString("N");
        var payload = BuildTripPayload(trip, $"Dispatch override performed: {reason}") with
        {
            OverrideTargetType = "vendor_readiness",
            OverrideKinds = [reason],
        };

        return TryEnqueueAsync(
            trip.TenantId,
            RoutArrIntegrationOutboxEventKinds.DispatchOverridePerformed,
            "trip",
            trip.Id,
            payload,
            idempotencySuffix: suffix,
            cancellationToken: cancellationToken);
    }

    public Task<Guid?> TryEnqueueStopCompletedAsync(
        RouteStop stop,
        CancellationToken cancellationToken = default) =>
        TryEnqueueStopAsync(
            stop,
            RoutArrIntegrationOutboxEventKinds.StopCompleted,
            "Stop completed",
            cancellationToken);

    public Task<Guid?> TryEnqueueStopArrivedAsync(
        RouteStop stop,
        CancellationToken cancellationToken = default) =>
        TryEnqueueStopAsync(
            stop,
            RoutArrIntegrationOutboxEventKinds.StopArrived,
            "Stop arrived",
            cancellationToken);

    public Task<Guid?> TryEnqueueStopEnRouteAsync(
        RouteStop stop,
        CancellationToken cancellationToken = default) =>
        TryEnqueueStopAsync(
            stop,
            RoutArrIntegrationOutboxEventKinds.StopEnRoute,
            "Stop en route",
            cancellationToken);

    public Task<Guid?> TryEnqueueStopMissedAsync(
        RouteStop stop,
        CancellationToken cancellationToken = default) =>
        TryEnqueueStopAsync(
            stop,
            RoutArrIntegrationOutboxEventKinds.StopMissed,
            "Stop missed",
            cancellationToken);

    public Task<Guid?> TryEnqueueProofCapturedAsync(
        TripProofRecord proof,
        TripDetailResponse trip,
        CancellationToken cancellationToken = default) =>
        TryEnqueueProofAsync(
            proof,
            trip,
            RoutArrIntegrationOutboxEventKinds.ProofCaptured,
            "Proof captured",
            cancellationToken);

    public Task<Guid?> TryEnqueueProofCreatedAsync(
        TripProofRecord proof,
        TripDetailResponse trip,
        CancellationToken cancellationToken = default) =>
        TryEnqueueProofAsync(
            proof,
            trip,
            RoutArrIntegrationOutboxEventKinds.ProofCreated,
            "Proof created",
            cancellationToken);

    private Task<Guid?> TryEnqueueProofAsync(
        TripProofRecord proof,
        TripDetailResponse trip,
        string eventKind,
        string summary,
        CancellationToken cancellationToken) =>
        TryEnqueueAsync(
            proof.TenantId,
            eventKind,
            "trip_proof",
            proof.Id,
            new RoutArrIntegrationOutboxPayload(
                proof.TenantId,
                summary,
                proof.TripId,
                trip.TripNumber,
                trip.AssignedDriverPersonId,
                proof.VehicleRefKey,
                trip.DispatchStatus,
                ProofId: proof.Id,
                ProofType: proof.ProofType),
            cancellationToken: cancellationToken);

    public Task<Guid?> TryEnqueueDriverReportedDefectAsync(
        TripDvirInspection dvir,
        TripDetailResponse trip,
        CancellationToken cancellationToken = default) =>
        TryEnqueueAsync(
            dvir.TenantId,
            RoutArrIntegrationOutboxEventKinds.DriverReportedDefect,
            "trip_dvir",
            dvir.Id,
            new RoutArrIntegrationOutboxPayload(
                dvir.TenantId,
                "Driver-reported defect",
                dvir.TripId,
                trip.TripNumber,
                trip.AssignedDriverPersonId,
                dvir.VehicleRefKey,
                trip.DispatchStatus,
                DvirId: dvir.Id,
                DvirPhase: dvir.Phase,
                DvirResult: dvir.Result,
                DefectNotes: dvir.DefectNotes),
            idempotencySuffix: dvir.UpdatedAt.ToUnixTimeMilliseconds().ToString(),
            cancellationToken: cancellationToken);

    public Task<Guid?> TryEnqueueExceptionCreatedAsync(
        Trip trip,
        DispatchException exception,
        CancellationToken cancellationToken = default) =>
        TryEnqueueExceptionCreatedAsync(exception, trip, cancellationToken);

    public Task<Guid?> TryEnqueueExceptionCreatedAsync(
        DispatchException exception,
        Trip? trip,
        CancellationToken cancellationToken = default) =>
        TryEnqueueAsync(
            exception.TenantId,
            RoutArrIntegrationOutboxEventKinds.ExceptionCreated,
            "dispatch_exception",
            exception.Id,
            BuildExceptionPayload(exception, trip, "Transportation exception created"),
            cancellationToken: cancellationToken);

    public Task<Guid?> TryEnqueueIncidentCreatedAsync(
        DispatchException exception,
        Trip? trip,
        CancellationToken cancellationToken = default) =>
        TryEnqueueAsync(
            exception.TenantId,
            RoutArrIntegrationOutboxEventKinds.IncidentCreated,
            "dispatch_exception",
            exception.Id,
            BuildExceptionPayload(exception, trip, "Transportation incident created"),
            cancellationToken: cancellationToken);

    public Task<Guid?> TryEnqueueComplianceHoldCreatedAsync(
        DispatchException exception,
        Trip? trip,
        CancellationToken cancellationToken = default) =>
        IsComplianceException(exception)
            ? TryEnqueueAsync(
                exception.TenantId,
                RoutArrIntegrationOutboxEventKinds.ComplianceHoldCreated,
                "dispatch_exception",
                exception.Id,
                BuildExceptionPayload(exception, trip, "Compliance hold created"),
                cancellationToken: cancellationToken)
            : Task.FromResult<Guid?>(null);

    public Task<Guid?> TryEnqueueComplianceHoldReleasedAsync(
        DispatchException exception,
        Trip? trip,
        CancellationToken cancellationToken = default) =>
        IsComplianceException(exception)
            ? TryEnqueueAsync(
                exception.TenantId,
                RoutArrIntegrationOutboxEventKinds.ComplianceHoldReleased,
                "dispatch_exception",
                exception.Id,
                BuildExceptionPayload(exception, trip, "Compliance hold released"),
                idempotencySuffix: exception.UpdatedAt.ToUnixTimeMilliseconds().ToString(),
                cancellationToken: cancellationToken)
            : Task.FromResult<Guid?>(null);

    public Task<Guid?> TryEnqueueExceptionResolvedAsync(
        DispatchException exception,
        Trip? trip,
        CancellationToken cancellationToken = default) =>
        TryEnqueueAsync(
            exception.TenantId,
            RoutArrIntegrationOutboxEventKinds.ExceptionResolved,
            "dispatch_exception",
            exception.Id,
            BuildExceptionPayload(exception, trip, "Transportation exception resolved"),
            idempotencySuffix: exception.UpdatedAt.ToUnixTimeMilliseconds().ToString(),
            cancellationToken: cancellationToken);

    public async Task<Guid?> TryEnqueueAsync(
        Guid tenantId,
        string eventKind,
        string relatedEntityType,
        Guid relatedEntityId,
        RoutArrIntegrationOutboxPayload payload,
        string? idempotencySuffix = null,
        Guid? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (!IntegrationEventRules.ShouldProcessForTenant(settings))
        {
            return null;
        }

        var idempotencyKey = IntegrationEventRules.BuildOutboxIdempotencyKey(
            eventKind,
            relatedEntityType,
            relatedEntityId,
            idempotencySuffix);

        var duplicate = await db.IntegrationOutboxEvents.AnyAsync(
            x => x.TenantId == tenantId
                && x.IdempotencyKey == idempotencyKey
                && (x.ProcessingStatus == IntegrationEventStatuses.Pending
                    || x.ProcessingStatus == IntegrationEventStatuses.Processed),
            cancellationToken);

        if (duplicate)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var domainEvent = new IntegrationOutboxEvent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EventKind = eventKind,
            IdempotencyKey = idempotencyKey,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            PayloadJson = JsonSerializer.Serialize(payload, JsonOptions),
            ProcessingStatus = IntegrationEventStatuses.Pending,
            AttemptCount = 0,
            CorrelationId = correlationId ?? Guid.NewGuid(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.IntegrationOutboxEvents.Add(domainEvent);
        await db.SaveChangesAsync(cancellationToken);
        return domainEvent.Id;
    }

    private static RoutArrIntegrationOutboxPayload BuildTripPayload(
        Trip trip,
        string summary,
        string? driverPersonId = null) =>
        new(
            trip.TenantId,
            summary,
            trip.Id,
            trip.TripNumber,
            driverPersonId ?? trip.AssignedDriverPersonId,
            trip.VehicleRefKey,
            trip.DispatchStatus);

    private static RoutArrIntegrationOutboxPayload BuildRoutePayload(
        DispatchRoute route,
        string summary) =>
        new(
            route.TenantId,
            summary,
            route.TripId,
            RouteId: route.Id,
            RouteNumber: route.RouteNumber,
            RouteStatus: route.RouteStatus);

    private static bool IsComplianceException(DispatchException exception) =>
        string.Equals(exception.Category, DispatchExceptionCategories.Compliance, StringComparison.OrdinalIgnoreCase);

    private static RoutArrIntegrationOutboxPayload BuildExceptionPayload(
        DispatchException exception,
        Trip? trip,
        string summary) =>
        new(
            exception.TenantId,
            summary,
            exception.TripId,
            trip?.TripNumber,
            trip?.AssignedDriverPersonId,
            trip?.VehicleRefKey,
            trip?.DispatchStatus,
            ExceptionId: exception.Id,
            ExceptionKey: exception.ExceptionKey,
            ExceptionCategory: exception.Category,
            IncidentType: exception.IncidentType,
            IncidentSeverity: exception.IncidentSeverity,
            IncidentReviewStatus: exception.IncidentReviewStatus,
            IncidentRoutedProduct: exception.IncidentRoutedProduct);

    private Task<Guid?> TryEnqueueStopAsync(
        RouteStop stop,
        string eventKind,
        string summary,
        CancellationToken cancellationToken)
    {
        var trip = stop.Route?.Trip;
        var route = stop.Route;
        if (route?.TripId is not { } tripId)
        {
            return Task.FromResult<Guid?>(null);
        }

        return TryEnqueueAsync(
            stop.TenantId,
            eventKind,
            "route_stop",
            stop.Id,
            new RoutArrIntegrationOutboxPayload(
                stop.TenantId,
                summary,
                tripId,
                trip?.TripNumber,
                trip?.AssignedDriverPersonId,
                trip?.VehicleRefKey,
                trip?.DispatchStatus,
                route.Id,
                stop.Id,
                stop.StopKey,
                stop.StopStatus),
            cancellationToken: cancellationToken);
    }
}
