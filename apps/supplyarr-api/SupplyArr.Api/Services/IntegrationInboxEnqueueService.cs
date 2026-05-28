using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class IntegrationInboxEnqueueService(
    SupplyArrDbContext db,
    IntegrationEventSettingsService settingsService)
{
    public async Task<EnqueueIntegrationInboxResponse> TryEnqueueAsync(
        EnqueueIntegrationInboxRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadSnapshotAsync(request.TenantId, cancellationToken);
        if (!IntegrationEventRules.ShouldProcessForTenant(settings))
        {
            return new EnqueueIntegrationInboxResponse(null, false);
        }

        var idempotencyKey = IntegrationEventRules.BuildInboxIdempotencyKey(
            request.SourceProduct,
            request.EventKind,
            request.IdempotencyKey);

        var duplicate = await db.IntegrationInboxEvents.AnyAsync(
            x => x.TenantId == request.TenantId
                && x.IdempotencyKey == idempotencyKey
                && (x.ProcessingStatus == IntegrationEventStatuses.Pending
                    || x.ProcessingStatus == IntegrationEventStatuses.Processed),
            cancellationToken);

        if (duplicate)
        {
            return new EnqueueIntegrationInboxResponse(null, true);
        }

        var now = DateTimeOffset.UtcNow;
        var inboxEvent = new IntegrationInboxEvent
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            SourceProduct = request.SourceProduct.Trim().ToLowerInvariant(),
            EventKind = request.EventKind.Trim().ToLowerInvariant(),
            IdempotencyKey = idempotencyKey,
            RelatedEntityType = request.RelatedEntityType.Trim().ToLowerInvariant(),
            RelatedEntityId = request.RelatedEntityId?.Trim(),
            PayloadJson = request.PayloadJson,
            ProcessingStatus = IntegrationEventStatuses.Pending,
            AttemptCount = 0,
            CorrelationId = request.CorrelationId ?? Guid.NewGuid(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.IntegrationInboxEvents.Add(inboxEvent);
        await db.SaveChangesAsync(cancellationToken);
        return new EnqueueIntegrationInboxResponse(inboxEvent.Id, false);
    }
}
