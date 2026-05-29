using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;

namespace NexArr.Api.Services;

public sealed class PlatformOutboxEnqueueService(NexArrDbContext db)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Guid?> TryEnqueueAsync(
        string eventType,
        string targetType,
        string targetId,
        string changeToken,
        PlatformOutboxPayload payload,
        Guid? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var idempotencyKey = PlatformOutboxRules.BuildIdempotencyKey(
            eventType,
            targetType,
            targetId,
            changeToken);

        var duplicate = await db.PlatformOutboxEvents.AnyAsync(
            x => x.IdempotencyKey == idempotencyKey
                && (x.ProcessingStatus == PlatformOutboxEventStatuses.Pending
                    || x.ProcessingStatus == PlatformOutboxEventStatuses.Published),
            cancellationToken);

        if (duplicate)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var domainEvent = new PlatformOutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            IdempotencyKey = idempotencyKey,
            SchemaVersion = payload.SchemaVersion,
            PayloadJson = JsonSerializer.Serialize(payload, JsonOptions),
            TenantId = payload.TenantId,
            ActorPersonId = payload.ActorPersonId,
            ProductCode = ResolveProductCode(payload.Metadata),
            CorrelationId = correlationId ?? Guid.NewGuid(),
            ProcessingStatus = PlatformOutboxEventStatuses.Pending,
            AttemptCount = 0,
            OccurredAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.PlatformOutboxEvents.Add(domainEvent);
        await db.SaveChangesAsync(cancellationToken);
        return domainEvent.Id;
    }

    private static string? ResolveProductCode(IReadOnlyDictionary<string, string>? metadata)
    {
        if (metadata is null)
        {
            return null;
        }

        return metadata.TryGetValue("productCode", out var productCode) ? productCode : null;
    }
}
