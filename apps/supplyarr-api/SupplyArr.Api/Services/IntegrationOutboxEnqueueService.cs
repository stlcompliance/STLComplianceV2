using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed record IntegrationOutboxPayload(
    Guid TenantId,
    string Summary,
    Guid? VendorPartyId = null);

public sealed class IntegrationOutboxEnqueueService(
    SupplyArrDbContext db,
    IntegrationEventSettingsService settingsService)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Guid?> TryEnqueueAsync(
        Guid tenantId,
        string eventKind,
        string relatedEntityType,
        Guid relatedEntityId,
        IntegrationOutboxPayload payload,
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
            relatedEntityId);

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
}
