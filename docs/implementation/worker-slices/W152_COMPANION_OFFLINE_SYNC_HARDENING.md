# W152 — Companion offline sync hardening

**Milestone:** M11 (Companion)  
**Products:** NexArr, companion-frontend  
**Status:** Complete

## Summary

Hardens companion offline acknowledgment sync so mixed batches return per-item outcomes instead of failing the whole request. The companion UI keeps retryable failures in the local queue, drops permanent rejections, and caps the pending queue at 50 items.

## Backend (NexArr)

- `SyncCompanionOfflineActionsResponse` adds `rejected` count and `rejectedItems` (`idempotencyKey`, `reasonCode`, `reasonMessage`)
- `CompanionOfflineSyncService` catches validation errors per action; valid actions still persist
- Plain messages from `CompanionDeniedReasonCatalog` for offline validation errors

## Frontend (companion)

- `offlineSyncOutcome.ts` — retryable vs permanent rejection partitioning, sync summary text
- `offlineQueue.ts` — `markSyncPartial`, max queue size (`MAX_OFFLINE_QUEUE_SIZE = 50`)
- `useOfflineQueue` — partial sync handling, capacity error toast

## Tests

- `NexArrCompanionOfflineSyncTests.Sync_accepts_valid_actions_and_rejects_invalid_in_same_batch`
- Updated per-item rejection expectations in offline + validation tests
- `offlineSyncOutcome.test.ts`, extended `offlineQueue.test.ts`

## Permissions

- Unchanged: companion JWT + entitlement (`RequireCompanionAccess`)

## DB

- None (uses existing `nexarr_companion_offline_actions`)

## APIs

- `POST /api/companion/offline-actions/sync` — response shape extended (backward-compatible additive fields)

## Next gaps

- Web Push subscription storage and delivery (beyond browser permission readiness)
- Additional offline action kinds (evidence queue)
