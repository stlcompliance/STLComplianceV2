using System.Security.Claims;
using System.Text.Json;
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
    FieldCompanionFieldTaskValidationService validation,
    FieldCompanionClockService clockService)
{
    private static readonly JsonSerializerOptions PayloadJsonOptions = new(JsonSerializerDefaults.Web);

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
                var normalizedAction = ValidateAction(action);

                var existing = await db.FieldCompanionOfflineActions.AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.TenantId == tenantId && x.IdempotencyKey == action.IdempotencyKey,
                        cancellationToken);

                if (existing is not null)
                {
                    EnsureExistingMatches(normalizedAction, existing);
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
                    ActionKind = normalizedAction.ActionKind,
                    TaskKey = normalizedAction.TaskKey,
                    ProductKey = normalizedAction.ProductKey,
                    ClientCreatedAt = action.ClientCreatedAt,
                    SyncedAt = now,
                    PayloadJson = normalizedAction.PayloadJson,
                };

                await ProcessActionAsync(principal, accessToken, normalizedAction, entity, cancellationToken);
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
                if (string.Equals(entity.ActionKind, FieldCompanionOfflineActionKinds.FieldInboxAcknowledge, StringComparison.Ordinal))
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

    private async Task ProcessActionAsync(
        ClaimsPrincipal principal,
        string accessToken,
        NormalizedOfflineAction action,
        FieldCompanionOfflineAction entity,
        CancellationToken cancellationToken)
    {
        if (string.Equals(action.ActionKind, FieldCompanionOfflineActionKinds.FieldInboxAcknowledge, StringComparison.Ordinal))
        {
            await validation.EnsureAllowedAsync(
                principal,
                accessToken,
                action.TaskKey,
                FieldCompanionFieldSubmissionKinds.Acknowledge,
                action.ProductKey,
                cancellationToken);
            return;
        }

        if (string.Equals(action.ActionKind, FieldCompanionOfflineActionKinds.StaffArrClockPunch, StringComparison.Ordinal))
        {
            var payload = DeserializeClockPayload(action.PayloadJson);
            if (!string.Equals(payload.IdempotencyKey.Trim(), action.IdempotencyKey, StringComparison.Ordinal))
            {
                throw new StlApiException(
                    "fieldcompanion.offline_actions.payload_idempotency_mismatch",
                    "Offline clock payload idempotency must match the queued action idempotency key.",
                    400);
            }

            await clockService.SubmitAsync(principal, accessToken, payload, cancellationToken);
            return;
        }
    }

    private static NormalizedOfflineAction ValidateAction(FieldCompanionOfflineActionItem action)
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

        var normalizedIdempotencyKey = action.IdempotencyKey.Trim();
        var kind = action.ActionKind.Trim().ToLowerInvariant();
        var normalizedTaskKey = action.TaskKey.Trim();
        var normalizedProductKey = action.ProductKey.Trim().ToLowerInvariant();
        var payloadJson = action.Payload is null
            ? null
            : JsonSerializer.Serialize(action.Payload, PayloadJsonOptions);

        if (string.Equals(kind, FieldCompanionOfflineActionKinds.FieldInboxAcknowledge, StringComparison.Ordinal))
        {
            return new NormalizedOfflineAction(
                normalizedIdempotencyKey,
                kind,
                normalizedTaskKey,
                normalizedProductKey,
                payloadJson);
        }

        if (string.Equals(kind, FieldCompanionOfflineActionKinds.StaffArrClockPunch, StringComparison.Ordinal))
        {
            if (!string.Equals(normalizedProductKey, "staffarr", StringComparison.Ordinal))
            {
                throw new StlApiException(
                    "fieldcompanion.offline_actions.invalid_product",
                    "Offline clock punches must target StaffArr.",
                    400);
            }

            if (string.IsNullOrWhiteSpace(payloadJson))
            {
                throw new StlApiException(
                    "fieldcompanion.offline_actions.payload_required",
                    "Offline clock punches require a payload.",
                    400);
            }

            _ = DeserializeClockPayload(payloadJson);
            return new NormalizedOfflineAction(
                normalizedIdempotencyKey,
                kind,
                normalizedTaskKey,
                normalizedProductKey,
                payloadJson);
        }

        throw new StlApiException(
            "fieldcompanion.offline_actions.unsupported_kind",
            FieldCompanionDeniedReasonCatalog.ToPlainMessage("fieldcompanion.offline_actions.unsupported_kind"),
            400);
    }

    private static SubmitFieldCompanionClockEventRequest DeserializeClockPayload(string? payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            throw new StlApiException(
                "fieldcompanion.offline_actions.payload_required",
                "Offline clock punches require a payload.",
                400);
        }

        try
        {
            var payload = JsonSerializer.Deserialize<SubmitFieldCompanionClockEventRequest>(
                payloadJson,
                PayloadJsonOptions);
            if (payload is null)
            {
                throw new JsonException("Payload was empty.");
            }

            return payload;
        }
        catch (JsonException)
        {
            throw new StlApiException(
                "fieldcompanion.offline_actions.invalid_payload",
                "Offline clock payload was invalid.",
                400);
        }
    }

    private static void EnsureExistingMatches(
        NormalizedOfflineAction action,
        FieldCompanionOfflineAction existing)
    {
        if (!string.Equals(existing.ActionKind, action.ActionKind, StringComparison.Ordinal)
            || !string.Equals(existing.TaskKey, action.TaskKey, StringComparison.Ordinal)
            || !string.Equals(existing.ProductKey, action.ProductKey, StringComparison.Ordinal)
            || !string.Equals(existing.PayloadJson ?? string.Empty, action.PayloadJson ?? string.Empty, StringComparison.Ordinal))
        {
            throw new StlApiException(
                "fieldcompanion.offline_actions.idempotency_conflict",
                "Offline action idempotency key was already used for a different action.",
                409);
        }
    }

    private static FieldCompanionOfflineActionSyncedItem ToSyncedItem(FieldCompanionOfflineAction entity) =>
        new(entity.IdempotencyKey, entity.ActionKind, entity.TaskKey, entity.ProductKey, entity.SyncedAt);

    private sealed record NormalizedOfflineAction(
        string IdempotencyKey,
        string ActionKind,
        string TaskKey,
        string ProductKey,
        string? PayloadJson);
}
