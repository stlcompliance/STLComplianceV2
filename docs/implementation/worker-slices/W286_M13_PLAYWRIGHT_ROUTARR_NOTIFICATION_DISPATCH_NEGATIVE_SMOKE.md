# W286 — M13 Playwright: RoutArr dispatch notification settings per-event-kind UI negative smoke

Builds on **W127** (`NotificationSettingsPanel`, dispatch outbox), **W279** (settings panel save/reload via live UI), **W285** (multi-event journey with per-toggle API assertions), and **W284–W280** (single-event journey smokes).

Verifies that **disabled event toggles saved through the live settings panel** do not enqueue outbox rows when a full trip lifecycle runs afterward.

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-settings-notification-dispatch-negative-smoke.spec.ts` | `/settings` | Suite sign-in → handoff → `notification-settings-panel`: enable notifications + W286 webhook URL; set **only** trip-dispatched ON (assigned/in_progress/completed/cancelled OFF) → **Save** → reload verifies toggle persistence → `createAndRunRoutArrDispatchNotificationFullLifecycle` (API) → reload **Recent dispatches**: pending `trip_dispatched` row for trip; no rows for disabled kinds → restore original enable/webhook/toggles via UI save |

Webhook delivery is read-only (`https://hooks.example.com/routarr-e2e-w286`); no external webhook sink or process-batch in this spec.

### Frontend test ids (`NotificationSettingsPanel`)

- Existing: `notification-trip-assigned`
- Added: `notification-trip-dispatched`, `notification-trip-in-progress`, `notification-trip-completed`, `notification-trip-cancelled`

### `e2eApi` helpers

- `createAndRunRoutArrDispatchNotificationFullLifecycle` — create trip + assign → dispatched → in_progress → completed (no settings mutation)
- `assertRoutArrDispatchNotificationDispatchesForTrip` — API assertion for expected/absent event kinds
- `routArrDispatchNotificationUiNegativeSmokeEnabledEventKind` / `routArrDispatchNotificationUiNegativeSmokeDisabledEventKinds` constants

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrSettingsNotificationDispatchNegativeSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w286`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-settings-notification-dispatch-negative-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin role for notification settings + trip lifecycle (`routarr.dispatch.manage`).

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/routarr-frontend
npm test -- NotificationSettingsPanel
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-settings-notification-dispatch-negative-smoke.spec.ts
```

## Out of scope

- Live webhook sink / process-batch worker run
- API-driven settings upsert for the negative path (W285/W280–W284 cover API fixture seed)
- UI status transitions from dispatch board

## Next recommended slice

**M13 Playwright — RoutArr dispatch notification settings panel UI save with all event kinds enabled then selective disable + second lifecycle negative smoke** (second trip after toggling off additional kinds post-save; builds on W286/W285/W279).
