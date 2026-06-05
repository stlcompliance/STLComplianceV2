using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class FieldCompanionOfflineSyncService(
    NexArrDbContext db,
    FieldCompanionFieldSubmissionService submissions,
    FieldCompanionFieldTaskValidationService validation)
{
    public async Task<SyncFieldCompanionOfflineActionsResponse> SyncAsync(
        ClaimsPrincipal principal,
        string accessToken,
        SyncFieldCompanionOfflineActionsRequest request,
        CancellationToken cancellationToken = default)
    {
        FieldCompanionFieldInboxService.RequireFieldCompanionAccess(principal);
        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();

        if (request.Actions.Count == 0)
        {
            return new SyncFieldCompanionOfflineActionsResponse(0, 0, 0, [], []);
        }

        if (request.Actions.Count > 50)
        {
            throw new StlApiException(
                "fieldcompanion.offline_actions.batch_too_large",
                "At most 50 offline actions may be synced per request.",
                400);
        }

        var accepted = 0;
        var duplicates = 0;
        var rejected = 0;
        var synced = new List<FieldCompanionOfflineActionSyncedItem>();
        var rejectedItems = new List<FieldCompanionOfflineActionRejectedItem>();
        var newlyAccepted = new List<FieldCompanionOfflineAction>();

        foreach (var action in request.Actions)
        {
            try
            {
                ValidateAction(action);
                await validation.EnsureAllowedAsync(
                    principal,
                    accessToken,
                    action.TaskKey,
                    FieldCompanionFieldSubmissionKinds.Acknowledge,
                    action.ProductKey,
                    cancellationToken);

                var existing = await db.FieldCompanionOfflineActions.AsNoTracking()
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
                var entity = new FieldCompanionOfflineAction
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

                db.FieldCompanionOfflineActions.Add(entity);
                accepted++;
                newlyAccepted.Add(entity);
                synced.Add(ToSyncedItem(entity));
            }
            catch (StlApiException ex)
            {
                rejected++;
                rejectedItems.Add(new FieldCompanionOfflineActionRejectedItem(
                    action.IdempotencyKey.Trim(),
                    ex.Code,
                    ex.Message));
            }
        }

        if (accepted > 0)
        {
            await db.SaveChangesAsync(cancellationToken);

            foreach (var entity in newlyAccepted)
            {
                await submissions.RecordAsync(
                    tenantId,
                    userId,
                    entity.TaskKey,
                    entity.ProductKey,
                    FieldCompanionFieldSubmissionKinds.Acknowledge,
                    FieldCompanionFieldSubmissionStatuses.Synced,
                    "Field acknowledgment synced to NexArr.",
                    entity.ClientCreatedAt,
                    cancellationToken);
            }
        }

        return new SyncFieldCompanionOfflineActionsResponse(
            accepted,
            duplicates,
            rejected,
            synced,
            rejectedItems);
    }

    public async Task<FieldCompanionOfflineActionsListResponse> ListRecentAsync(
        ClaimsPrincipal principal,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        FieldCompanionFieldInboxService.RequireFieldCompanionAccess(principal);
        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();
        var take = Math.Clamp(limit ?? 20, 1, 100);

        var items = await db.FieldCompanionOfflineActions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId)
            .OrderByDescending(x => x.SyncedAt)
            .Take(take)
            .Select(x => new FieldCompanionOfflineActionSyncedItem(
                x.IdempotencyKey,
                x.ActionKind,
                x.TaskKey,
                x.ProductKey,
                x.SyncedAt))
            .ToListAsync(cancellationToken);

        return new FieldCompanionOfflineActionsListResponse(items);
    }

    private static void ValidateAction(FieldCompanionOfflineActionItem action)
    {
        if (string.IsNullOrWhiteSpace(action.IdempotencyKey))
        {
            throw new StlApiException(
                "fieldcompanion.offline_actions.idempotency_required",
                FieldCompanionDeniedReasonCatalog.ToPlainMessage("fieldcompanion.offline_actions.idempotency_required"),
                400);
        }

        if (string.IsNullOrWhiteSpace(action.TaskKey) || string.IsNullOrWhiteSpace(action.ProductKey))
        {
            throw new StlApiException(
                "fieldcompanion.offline_actions.task_required",
                FieldCompanionDeniedReasonCatalog.ToPlainMessage("fieldcompanion.offline_actions.task_required"),
                400);
        }

        var kind = action.ActionKind.Trim().ToLowerInvariant();
        if (!string.Equals(kind, FieldCompanionOfflineActionKinds.FieldInboxAcknowledge, StringComparison.Ordinal))
        {
            throw new StlApiException(
                "fieldcompanion.offline_actions.unsupported_kind",
                FieldCompanionDeniedReasonCatalog.ToPlainMessage("fieldcompanion.offline_actions.unsupported_kind"),
                400);
        }
    }

    private static FieldCompanionOfflineActionSyncedItem ToSyncedItem(FieldCompanionOfflineAction entity) =>
        new(entity.IdempotencyKey, entity.ActionKind, entity.TaskKey, entity.ProductKey, entity.SyncedAt);
}
