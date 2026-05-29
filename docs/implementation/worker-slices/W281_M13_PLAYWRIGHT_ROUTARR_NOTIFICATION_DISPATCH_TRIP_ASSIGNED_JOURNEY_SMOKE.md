# W281 — M13 Playwright: RoutArr dispatch notification journey with trip-assigned event

Builds on **W127** (`NotificationSettingsPanel`, dispatch outbox + internal process-batch), **W280** (end-to-end notification dispatch journey smoke + `ensureRoutArrDispatchNotificationJourneyFixture` pattern), and **W279** (settings panel smoke + `e2eApi` dispatch notification helpers).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-notification-dispatch-trip-assigned-journey-smoke.spec.ts` | `/settings` | `beforeAll` `ensureRoutArrDispatchNotificationTripAssignedJourneyFixture`: enable notifications + webhook with **trip-assigned only**, create trip, assign driver (no status PATCH) → pending `trip_assigned` outbox row. Suite sign-in → handoff → `notification-settings-panel` **Recent dispatches** shows fixture row (`notification-dispatch-row-{tripId}`) with `trip_assigned` + `pending`. Optional read-only internal `processRoutArrDispatchNotificationBatch` (example.com webhook fails/succeeds without live delivery target) → reload verifies row no longer `pending` (`sent`/`failed`/`skipped`). |

Webhook delivery is read-only (fixture URL `https://hooks.example.com/routarr-e2e-w281`); no external webhook sink required.

### `e2eApi` helpers

- `ensureRoutArrDispatchNotificationTripAssignedJourneyFixture` (settings + create trip + assign-driver only + outbox assert)
- Reuses W279/W280: `upsertRoutArrDispatchNotificationSettings`, `listRoutArrDispatchNotificationDispatches`, `processRoutArrDispatchNotificationBatch`, `issueRoutArrDispatchNotificationWorkerToken`

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrNotificationDispatchTripAssignedJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w281`
- `All.Count >= 46`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-notification-dispatch-trip-assigned-journey-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin role for notification settings + trip assign-driver.

## Verification

```powershell
dotnet build STLCompliance.sln -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/routarr-frontend
npm test -- NotificationSettingsPanel
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-notification-dispatch-trip-assigned-journey-smoke.spec.ts
```

## Out of scope

- Live webhook sink / capture server
- Trip status change to dispatched/in-progress (covered by W280 or future slices)
- Settings toggle save/restore (covered by W279)
- UI assign-driver from dispatch board (API-only fixture seed)

## Next recommended slice

**M13 Playwright — RoutArr dispatch notification journey with trip-in-progress event** (status change to in_progress hook only → outbox row; builds on W281/W280/W127).
