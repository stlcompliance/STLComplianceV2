using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class TrainingEventEnqueueService(
    TrainArrDbContext db,
    EventProcessingSettingsService settingsService,
    TrainArrTenantSettingsService tenantSettingsService,
    TrainingEventProcessingService processingService)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Guid?> TryEnqueueAsync(
        Guid tenantId,
        string eventKind,
        TrainingDomainEventPayload payload,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (!EventProcessingRules.ShouldProcessForTenant(settings))
        {
            return null;
        }

        var tenantSettings = await tenantSettingsService.LoadPayloadAsync(tenantId, cancellationToken);
        if (!tenantSettings.Enforcement.PublishQualificationEvents
            && IsQualificationEvent(eventKind))
        {
            return null;
        }

        var idempotencyKey = EventProcessingRules.BuildIdempotencyKey(
            eventKind,
            payload.RelatedEntityType,
            payload.RelatedEntityId);

        var duplicate = await db.TrainingDomainEvents.AnyAsync(
            x => x.TenantId == tenantId
                && x.IdempotencyKey == idempotencyKey
                && (x.ProcessingStatus == TrainingDomainEventStatuses.Pending
                    || x.ProcessingStatus == TrainingDomainEventStatuses.Processed),
            cancellationToken);

        if (duplicate)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var domainEvent = new TrainingDomainEvent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EventKind = eventKind,
            IdempotencyKey = idempotencyKey,
            StaffarrPersonId = payload.StaffarrPersonId,
            RelatedEntityType = payload.RelatedEntityType,
            RelatedEntityId = payload.RelatedEntityId,
            PayloadJson = JsonSerializer.Serialize(payload, JsonOptions),
            ProcessingStatus = TrainingDomainEventStatuses.Pending,
            AttemptCount = 0,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.TrainingDomainEvents.Add(domainEvent);
        await db.SaveChangesAsync(cancellationToken);

        await processingService.TryProcessSingleAsync(domainEvent, cancellationToken);
        return domainEvent.Id;
    }

    private static bool IsQualificationEvent(string eventKind) =>
        eventKind is TrainingDomainEventKinds.QualificationIssued
            or TrainingDomainEventKinds.QualificationSuspended
            or TrainingDomainEventKinds.QualificationRevoked
            or TrainingDomainEventKinds.QualificationExpired;
}
