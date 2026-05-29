# W288 — M13 Playwright: RoutArr dispatch notification all-five-event-kinds process-batch journey

Builds on **W127** (dispatch notification outbox + internal process-batch), **W279** (settings panel save/reload), **W280–W284** (per-event-kind journey smokes), **W285** (multi-event + process-batch with API fixture seed), **W286/W287** (live UI settings save patterns).

Completes the notification E2E path from **outbox enqueue through worker delivery** for all five RoutArr dispatch notification event kinds, with settings saved through the live panel first.

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-notification-dispatch-all-events-process-batch-journey-smoke.spec.ts` | `/settings` | Suite sign-in → handoff → `notification-settings-panel`: enable notifications + W288 webhook; **all** event toggles ON → Save → reload → completed-path `createAndRunRoutArrDispatchNotificationFullLifecycle` (API) → cancelled-branch `createAndRunRoutArrDispatchNotificationCancelledBranch` (API) → assert pending rows for assigned/dispatched/in_progress/completed on completed trip + `trip_cancelled` on cancelled trip → `processRoutArrDispatchNotificationBatch` → assert API + UI rows move off pending to sent/failed/skipped → restore original settings |

Webhook delivery is read-only (`https://hooks.example.com/routarr-e2e-w288`); no external webhook sink.

### `e2eApi` helpers

Added:

- `routArrDispatchNotificationAllEventsProcessBatchCompletedPathExpectedEventKinds`
- `routArrDispatchNotificationAllEventsProcessBatchCancelledBranchExpectedEventKind`
- `routArrDispatchNotificationAllFiveEventKinds`
- `createAndRunRoutArrDispatchNotificationCancelledBranch` — assign + cancel without changing settings
- `assertRoutArrDispatchNotificationDispatchesProcessedForTrip` — post-batch API assertion

Reuses:

- `createAndRunRoutArrDispatchNotificationFullLifecycle`
- `assertRoutArrDispatchNotificationDispatchesForTrip`
- `issueRoutArrDispatchNotificationWorkerToken`
- `processRoutArrDispatchNotificationBatch`

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrNotificationDispatchAllEventsProcessBatchJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w288`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-notification-dispatch-all-events-process-batch-journey-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin role for notification settings + trip lifecycle (`routarr.dispatch.manage`).

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-notification-dispatch-all-events-process-batch-journey-smoke.spec.ts
```

## Out of scope

- Live webhook sink
- API-driven settings upsert for the enable-all path (W285 covers API fixture seed)
- UI status transitions from dispatch board
- Selective disable second lifecycle (W287)

## Next recommended slice

**M13 Playwright — RoutArr dispatch notification settings panel live save with webhook URL change + reload persistence smoke** (settings-only; builds on W279/W288).
