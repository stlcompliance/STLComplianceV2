# W283 — M13 Playwright: RoutArr dispatch notification journey with trip-completed event

Builds on **W127** (`NotificationSettingsPanel`, dispatch outbox + internal process-batch), **W282** (trip-in-progress notification journey smoke + fixture pattern), **W281** (trip-assigned notification journey smoke), **W280** (end-to-end notification dispatch journey smoke), and **W279** (settings panel smoke + `e2eApi` dispatch notification helpers).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-notification-dispatch-trip-completed-journey-smoke.spec.ts` | `/settings` | `beforeAll` `ensureRoutArrDispatchNotificationTripCompletedJourneyFixture`: enable notifications + webhook with **trip-completed only**, create trip, assign driver, transition to dispatched, in_progress, PATCH status `completed` → pending `trip_completed` outbox row. Suite sign-in → handoff → `notification-settings-panel` **Recent dispatches** shows fixture row (`notification-dispatch-row-{tripId}`) with `trip_completed` + `pending`. Optional read-only internal `processRoutArrDispatchNotificationBatch` (example.com webhook fails/succeeds without live delivery target) → reload verifies row no longer `pending` (`sent`/`failed`/`skipped`). |

Webhook delivery is read-only (fixture URL `https://hooks.example.com/routarr-e2e-w283`); no external webhook sink required.

### `e2eApi` helpers

- `ensureRoutArrDispatchNotificationTripCompletedJourneyFixture` (settings + create trip + assign-driver + dispatched + in_progress + completed status + outbox assert)
- Reuses W279/W280/W281/W282: `upsertRoutArrDispatchNotificationSettings`, `listRoutArrDispatchNotificationDispatches`, `processRoutArrDispatchNotificationBatch`, `issueRoutArrDispatchNotificationWorkerToken`

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrNotificationDispatchTripCompletedJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w283`
- `All.Count >= 48`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-notification-dispatch-trip-completed-journey-smoke.spec.ts
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
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-notification-dispatch-trip-completed-journey-smoke.spec.ts
```

## Out of scope

- Live webhook sink / capture server
- Trip status change to cancelled (future slice)
- Settings toggle save/restore (covered by W279)
- UI status transitions from dispatch board (API-only fixture seed)

## Next recommended slice

**M13 Playwright — RoutArr dispatch notification journey with trip-cancelled event** (status change to cancelled hook only → outbox row; builds on W283/W282/W281/W280/W127).
