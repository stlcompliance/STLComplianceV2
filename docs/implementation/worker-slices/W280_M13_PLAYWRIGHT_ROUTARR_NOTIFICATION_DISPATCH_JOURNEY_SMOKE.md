# W280 — M13 Playwright: RoutArr end-to-end notification dispatch journey smoke

Builds on **W127** (`NotificationSettingsPanel`, dispatch outbox + internal process-batch), **W279** (settings panel smoke + `e2eApi` dispatch notification helpers), and journey patterns from **W270–275** (`ensureRoutArr*` fixtures + single-session UI verify).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-notification-dispatch-journey-smoke.spec.ts` | `/settings` | `beforeAll` `ensureRoutArrDispatchNotificationJourneyFixture`: enable notifications + webhook, create trip, assign driver, PATCH status `dispatched` → pending `trip_dispatched` outbox row. Suite sign-in → handoff → `notification-settings-panel` **Recent dispatches** shows fixture row (`notification-dispatch-row-{tripId}`) with `trip_dispatched` + `pending`. Optional read-only internal `processRoutArrDispatchNotificationBatch` (example.com webhook fails/succeeds without live delivery target) → reload verifies row no longer `pending` (`sent`/`failed`/`skipped`). |

Webhook delivery is read-only (fixture URL `https://hooks.example.com/routarr-e2e-w280`); no external webhook sink required.

### Panel UX (`NotificationSettingsPanel`)

- `data-testid={`notification-dispatch-row-${item.tripId}`}` on each recent dispatch list item (W280)

### `e2eApi` helpers

- `listRoutArrDispatchNotificationDispatches`
- `ensureRoutArrDispatchNotificationJourneyFixture` (settings + trip lifecycle + outbox assert)
- Reuses W279: `upsertRoutArrDispatchNotificationSettings`, `processRoutArrDispatchNotificationBatch`, `issueRoutArrDispatchNotificationWorkerToken`

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrNotificationDispatchJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w280`
- `All.Count >= 45`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-notification-dispatch-journey-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin role for notification settings + trip mutations.

## Verification

```powershell
dotnet build STLCompliance.sln -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/routarr-frontend
npm test -- NotificationSettingsPanel
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-notification-dispatch-journey-smoke.spec.ts
```

## Out of scope

- Live webhook sink / capture server
- Trip-assigned-only journey (separate slice)
- Settings toggle save/restore (covered by W279)

## Next recommended slice

**M13 Playwright — RoutArr dispatch notification journey with trip-in-progress event** (status change to in_progress hook only → outbox row; builds on W281/W280/W127).
