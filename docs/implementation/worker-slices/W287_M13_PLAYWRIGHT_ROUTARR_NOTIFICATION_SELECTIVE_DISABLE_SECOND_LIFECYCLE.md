# W287 — M13 Playwright: RoutArr dispatch notification enable-all then selective-disable second lifecycle

Builds on **W127** (dispatch notification outbox), **W279** (settings panel save/reload), **W285** (multi-event API journey), and **W286** (per-event UI negative smoke with only trip-dispatched enabled).

Verifies that **all event toggles saved through the live panel** enqueue completion-path kinds on the first trip lifecycle, and that **post-save selective disable** prevents newly disabled kinds from enqueueing on a **second** trip lifecycle.

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-settings-notification-dispatch-selective-disable-second-lifecycle-smoke.spec.ts` | `/settings` | Suite sign-in → handoff → `notification-settings-panel`: enable notifications + W287 webhook; **all** event toggles ON → Save → reload → first `createAndRunRoutArrDispatchNotificationFullLifecycle` (API) → assert assigned/dispatched/in_progress/completed (no cancelled) → UI selective disable (only trip-dispatched ON) → Save → reload → second lifecycle (API) → assert second trip only `trip_dispatched` → reload **Recent dispatches**: first trip multi-kind rows, second trip dispatched-only → restore original settings |

Webhook delivery is read-only (`https://hooks.example.com/routarr-e2e-w287`); no external webhook sink or process-batch in this spec.

### `e2eApi` helpers

Reuses:

- `createAndRunRoutArrDispatchNotificationFullLifecycle` — full assign → dispatched → in_progress → completed (called twice)
- `assertRoutArrDispatchNotificationDispatchesForTrip` — API assertion for expected/absent event kinds

Added constants:

- `routArrDispatchNotificationUiSecondLifecycleFirstTripExpectedEventKinds`
- `routArrDispatchNotificationUiSecondLifecycleSecondTripEnabledEventKind`
- `routArrDispatchNotificationUiSecondLifecycleSecondTripDisabledEventKinds` (alias of W286 disabled kinds)

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrSettingsNotificationDispatchSelectiveDisableSecondLifecycleSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w287`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-settings-notification-dispatch-selective-disable-second-lifecycle-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin role for notification settings + trip lifecycle (`routarr.dispatch.manage`).

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-settings-notification-dispatch-selective-disable-second-lifecycle-smoke.spec.ts
```

## Out of scope

- Live webhook sink / process-batch worker run
- API-driven settings upsert for the enable-all path (W285 covers API fixture seed)
- UI status transitions from dispatch board
- `trip_cancelled` on completed lifecycle (cancelled toggle enabled but lifecycle does not cancel)

## Next recommended slice

**M13 Playwright — RoutArr dispatch notification journey with internal process-batch verify on all five event kinds** (optional worker batch after UI settings save; builds on W287/W286/W280–W284). **Completed in W288.**
