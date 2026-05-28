using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class CompanionOfflineSyncService(NexArrDbContext db)
{
    public async Task<SyncCompanionOfflineActionsResponse> SyncAsync(
        ClaimsPrincipal principal,
        SyncCompanionOfflineActionsRequest request,
        CancellationToken cancellationToken = default)
    {
        CompanionFieldInboxService.RequireCompanionAccess(principal);
        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();

        if (request.Actions.Count == 0)
        {
            return new SyncCompanionOfflineActionsResponse(0, 0, []);
        }

        if (request.Actions.Count > 50)
        {
            throw new StlApiException(
                "companion.offline_actions.batch_too_large",
                "At most 50 offline actions may be synced per request.",
                400);
        }

        var accepted = 0;
        var duplicates = 0;
        var synced = new List<CompanionOfflineActionSyncedItem>();

        foreach (var action in request.Actions)
        {
            ValidateAction(action);

            var existing = await db.CompanionOfflineActions.AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.TenantId == tenantId && x.IdempotencyKey == action.IdempotencyKey,
                    cancellationToken);

            if (existing is not null)
            {
                duplicates++;
                synced.Add(ToSyncedItem(existing));
                continue;
            }

            var now = DateTimeOffset.UtcNow;
            var entity = new CompanionOfflineAction
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = userId,
                IdempotencyKey = action.IdempotencyKey.Trim(),
                ActionKind = action.ActionKind.Trim().ToLowerInvariant(),
                TaskKey = action.TaskKey.Trim(),
                ProductKey = action.ProductKey.Trim().ToLowerInvariant(),
                ClientCreatedAt = action.ClientCreatedAt,
                SyncedAt = now,
            };

            db.CompanionOfflineActions.Add(entity);
            accepted++;
            synced.Add(ToSyncedItem(entity));
        }

        if (accepted > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return new SyncCompanionOfflineActionsResponse(accepted, duplicates, synced);
    }

    public async Task<CompanionOfflineActionsListResponse> ListRecentAsync(
        ClaimsPrincipal principal,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        CompanionFieldInboxService.RequireCompanionAccess(principal);
        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();
        var take = Math.Clamp(limit ?? 20, 1, 100);

        var items = await db.CompanionOfflineActions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId)
            .OrderByDescending(x => x.SyncedAt)
            .Take(take)
            .Select(x => new CompanionOfflineActionSyncedItem(
                x.IdempotencyKey,
                x.ActionKind,
                x.TaskKey,
                x.ProductKey,
                x.SyncedAt))
            .ToListAsync(cancellationToken);

        return new CompanionOfflineActionsListResponse(items);
    }

    private static void ValidateAction(CompanionOfflineActionItem action)
    {
        if (string.IsNullOrWhiteSpace(action.IdempotencyKey))
        {
            throw new StlApiException("companion.offline_actions.idempotency_required", "Idempotency key is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(action.TaskKey) || string.IsNullOrWhiteSpace(action.ProductKey))
        {
            throw new StlApiException("companion.offline_actions.task_required", "Task key and product key are required.", 400);
        }

        var kind = action.ActionKind.Trim().ToLowerInvariant();
        if (!string.Equals(kind, CompanionOfflineActionKinds.FieldInboxAcknowledge, StringComparison.Ordinal))
        {
            throw new StlApiException("companion.offline_actions.unsupported_kind", "Unsupported offline action kind.", 400);
        }
    }

    private static CompanionOfflineActionSyncedItem ToSyncedItem(CompanionOfflineAction entity) =>
        new(entity.IdempotencyKey, entity.ActionKind, entity.TaskKey, entity.ProductKey, entity.SyncedAt);
}
