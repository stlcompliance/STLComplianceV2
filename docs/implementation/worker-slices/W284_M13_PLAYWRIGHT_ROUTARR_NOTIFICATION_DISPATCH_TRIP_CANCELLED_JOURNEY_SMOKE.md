# W284 — M13 Playwright: RoutArr dispatch notification journey with trip-cancelled event

Builds on **W127** (`NotificationSettingsPanel`, dispatch outbox + internal process-batch), **W283** (trip-completed notification journey smoke + fixture pattern), **W282** (trip-in-progress notification journey smoke), **W281** (trip-assigned notification journey smoke), **W280** (end-to-end notification dispatch journey smoke), and **W279** (settings panel smoke + `e2eApi` dispatch notification helpers).

Completes the **W127 dispatch notification event-kind Playwright coverage set** (`trip_assigned`, `trip_dispatched`, `trip_in_progress`, `trip_completed`, `trip_cancelled`).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-notification-dispatch-trip-cancelled-journey-smoke.spec.ts` | `/settings` | `beforeAll` `ensureRoutArrDispatchNotificationTripCancelledJourneyFixture`: enable notifications + webhook with **trip-cancelled only**, create trip, assign driver, PATCH status `cancelled` → pending `trip_cancelled` outbox row. Suite sign-in → handoff → `notification-settings-panel` **Recent dispatches** shows fixture row (`notification-dispatch-row-{tripId}`) with `trip_cancelled` + `pending`. Optional read-only internal `processRoutArrDispatchNotificationBatch` (example.com webhook fails/succeeds without live delivery target) → reload verifies row no longer `pending` (`sent`/`failed`/`skipped`). |

Webhook delivery is read-only (fixture URL `https://hooks.example.com/routarr-e2e-w284`); no external webhook sink required.

### `e2eApi` helpers

- `ensureRoutArrDispatchNotificationTripCancelledJourneyFixture` (settings + create trip + assign-driver + cancelled status + outbox assert)
- Reuses W279/W280/W281/W282/W283: `upsertRoutArrDispatchNotificationSettings`, `listRoutArrDispatchNotificationDispatches`, `processRoutArrDispatchNotificationBatch`, `issueRoutArrDispatchNotificationWorkerToken`

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrNotificationDispatchTripCancelledJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w284`
- `All.Count >= 49`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-notification-dispatch-trip-cancelled-journey-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin role for notification settings + trip cancellation (`routarr.dispatch.manage`).

## Verification

```powershell
dotnet build STLCompliance.sln -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/routarr-frontend
npm test -- NotificationSettingsPanel
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-notification-dispatch-trip-cancelled-journey-smoke.spec.ts
```

## Out of scope

- Live webhook sink / capture server
- Multi-event notification journey combining all event kinds in one spec (**delivered in W285**)
- Settings toggle save/restore (covered by W279)
- UI status transitions from dispatch board (API-only fixture seed)

## Next recommended slice

**M13 Playwright — RoutArr dispatch notification settings panel toggle save/reload per-event-kind negative smoke** (verify disabled toggles do not enqueue on live UI save; builds on W285/W279/W127).
