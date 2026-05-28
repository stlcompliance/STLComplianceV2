# Worker 146 — Companion offline queue + notification Playwright E2E

## Scope

W134 follow-up: real Companion offline acknowledge queue with NexArr sync, push-notification readiness UI, and live Playwright coverage.

## Deliverables

| Area | Change |
|------|--------|
| **NexArr API** | `nexarr_companion_offline_actions`, `POST /api/companion/offline-actions/sync`, `GET /api/companion/offline-actions` |
| **Companion UI** | `offlineQueue.ts`, `OfflineQueuePanel`, field-inbox **Acknowledge** button, push permission readiness in `NotificationSettingsPanel` |
| **Playwright** | `companion-offline-queue-notification.spec.ts` (`E2E_LIVE` skip) |
| **e2eApi** | `listCompanionOfflineActions` |
| **Catalog** | `StlE2ePlaywrightSpecCatalog.CompanionOfflineQueueNotificationSpec` |
| **Tests** | `NexArrCompanionOfflineSyncTests`, `offlineQueue.test.ts`, catalog tests |

## Verification

```powershell
dotnet test tests/STLCompliance.NexArr.Auth.Tests -c Release --filter "CompanionOffline"
cd apps/companion-frontend; npm test -- --run
dotnet test tests/STLCompliance.E2E -c Release --filter "Category=E2e&FullyQualifiedName~Companion"
# Live Playwright (optional):
$env:E2E_LIVE = "1"
cd tests/e2e-playwright; npm test -- companion-offline-queue-notification
```

## Out of scope

- Native mobile push (FCM/APNs) — browser permission readiness only
- Product-owned task state mutation (ack stored in NexArr as companion sync audit)

## Related

- W131 — operational notification webhooks
- W134 — deep-link Playwright harness
