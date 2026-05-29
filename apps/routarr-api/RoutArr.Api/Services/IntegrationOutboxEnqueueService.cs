using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public sealed record RoutArrIntegrationOutboxPayload(
    Guid TenantId,
    string Summary,
    Guid TripId,
    string? TripNumber = null,
    string? DriverPersonId = null,
    string? VehicleRefKey = null,
    string? DispatchStatus = null,
    Guid? ExceptionId = null,
    string? ExceptionKey = null,
    string? ExceptionCategory = null);

public sealed class IntegrationOutboxEnqueueService(
    RoutArrDbContext db,
    IntegrationEventSettingsService settingsService)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

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

    public Task<Guid?> TryEnqueueExceptionCreatedAsync(
        Trip trip,
        DispatchException exception,
        CancellationToken cancellationToken = default) =>
        TryEnqueueAsync(
            trip.TenantId,
            RoutArrIntegrationOutboxEventKinds.ExceptionCreated,
            "dispatch_exception",
            exception.Id,
            BuildTripPayload(trip, "Transportation exception created")
                with
                {
                    ExceptionId = exception.Id,
                    ExceptionKey = exception.ExceptionKey,
                    ExceptionCategory = exception.Category,
                },
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
}
