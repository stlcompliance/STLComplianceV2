# W282 — M13 Playwright: RoutArr dispatch notification journey with trip-in-progress event

Builds on **W127** (`NotificationSettingsPanel`, dispatch outbox + internal process-batch), **W281** (trip-assigned notification journey smoke + fixture pattern), **W280** (end-to-end notification dispatch journey smoke), and **W279** (settings panel smoke + `e2eApi` dispatch notification helpers).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-notification-dispatch-trip-in-progress-journey-smoke.spec.ts` | `/settings` | `beforeAll` `ensureRoutArrDispatchNotificationTripInProgressJourneyFixture`: enable notifications + webhook with **trip-in-progress only**, create trip, assign driver, transition to dispatched, PATCH status `in_progress` → pending `trip_in_progress` outbox row. Suite sign-in → handoff → `notification-settings-panel` **Recent dispatches** shows fixture row (`notification-dispatch-row-{tripId}`) with `trip_in_progress` + `pending`. Optional read-only internal `processRoutArrDispatchNotificationBatch` (example.com webhook fails/succeeds without live delivery target) → reload verifies row no longer `pending` (`sent`/`failed`/`skipped`). |

Webhook delivery is read-only (fixture URL `https://hooks.example.com/routarr-e2e-w282`); no external webhook sink required.

### `e2eApi` helpers

- `ensureRoutArrDispatchNotificationTripInProgressJourneyFixture` (settings + create trip + assign-driver + dispatched + in_progress status + outbox assert)
- Reuses W279/W280/W281: `upsertRoutArrDispatchNotificationSettings`, `listRoutArrDispatchNotificationDispatches`, `processRoutArrDispatchNotificationBatch`, `issueRoutArrDispatchNotificationWorkerToken`

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrNotificationDispatchTripInProgressJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w282`
- `All.Count >= 47`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-notification-dispatch-trip-in-progress-journey-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin role for notification settings + trip status transitions.

## Verification

```powershell
dotnet build STLCompliance.sln -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/routarr-frontend
npm test -- NotificationSettingsPanel
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-notification-dispatch-trip-in-progress-journey-smoke.spec.ts
```

## Out of scope

- Live webhook sink / capture server
- Trip status change to completed/cancelled (future slices)
- Settings toggle save/restore (covered by W279)
- UI status transitions from dispatch board (API-only fixture seed)

## Next recommended slice

**M13 Playwright — RoutArr dispatch notification journey with trip-completed event** (status change to completed hook only → outbox row; builds on W282/W281/W280/W127).
