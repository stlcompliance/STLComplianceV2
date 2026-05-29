# W285 — M13 Playwright: RoutArr dispatch notification end-to-end multi-event smoke

Builds on **W127** (`NotificationSettingsPanel`, dispatch outbox + internal process-batch), **W284** (trip-cancelled notification journey smoke — completes per-event-kind coverage set), **W283** (trip-completed), **W282** (trip-in-progress), **W281** (trip-assigned), **W280** (dispatched journey smoke), and **W279** (settings panel smoke + `e2eApi` dispatch notification helpers).

Consolidates the W280–W284 single-event journeys into one multi-branch smoke with **per-toggle verification** that only enabled event kinds enqueue outbox rows.

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-notification-dispatch-multi-event-journey-smoke.spec.ts` | `/settings` | `beforeAll` `ensureRoutArrDispatchNotificationMultiEventJourneyFixture`: **Completed path** — toggles assigned/dispatched/in_progress/completed ON, cancelled OFF; create trip → assign → dispatched → in_progress → completed; API asserts four enabled kinds + no cancelled. **Cancelled branch** — all toggles OFF except cancelled; create trip → assign → cancelled; API asserts cancelled only (no assigned). Suite sign-in → handoff → `notification-settings-panel` **Recent dispatches** shows pending rows per trip/event kind (list `li` filters by trip id + event kind). Optional read-only internal `processRoutArrDispatchNotificationBatch(batchSize=10)` → reload verifies rows no longer `pending`. |

Webhook delivery is read-only (fixture URLs `https://hooks.example.com/routarr-e2e-w285-*`); no external webhook sink required.

### `e2eApi` helpers

- `ensureRoutArrDispatchNotificationMultiEventJourneyFixture` (two-trip multi-branch seed + per-toggle API assertions)
- `RoutArrDispatchNotificationMultiEventJourneyFixture` type
- Internal helpers: `createRoutArrNotificationE2eTrip`, `assignRoutArrNotificationE2eTripDriver`, `setRoutArrNotificationE2eTripStatus`, `assertRoutArrDispatchNotificationEventKinds`
- Reuses W279–W284: `upsertRoutArrDispatchNotificationSettings`, `listRoutArrDispatchNotificationDispatches`, `processRoutArrDispatchNotificationBatch`, `issueRoutArrDispatchNotificationWorkerToken`

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrNotificationDispatchMultiEventJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w285`
- `All.Count >= 50`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-notification-dispatch-multi-event-journey-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin role for notification settings + trip lifecycle (`routarr.dispatch.manage`).

## Verification

```powershell
dotnet build STLCompliance.sln -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/routarr-frontend
npm test -- NotificationSettingsPanel
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-notification-dispatch-multi-event-journey-smoke.spec.ts
```

## Out of scope

- Live webhook sink / capture server
- UI-driven toggle save during journey (API fixture seed only; W279 covers settings save/reload)
- UI status transitions from dispatch board (API-only fixture seed)
- Changing `notification-dispatch-row-{tripId}` test id to include event kind (existing W280–W284 pattern uses trip-scoped id with list `li` filters)

## Next recommended slice

**M13 Playwright — RoutArr dispatch notification settings panel toggle save/reload per-event-kind negative smoke** (verify disabled toggles do not enqueue on live UI save; builds on W285/W279/W127).
