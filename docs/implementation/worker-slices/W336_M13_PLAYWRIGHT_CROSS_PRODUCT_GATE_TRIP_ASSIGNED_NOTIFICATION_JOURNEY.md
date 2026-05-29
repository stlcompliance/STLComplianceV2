# W336 — M13 Playwright: Cross-product operator journey — Compliance Core workflow gate block → RoutArr unassigned assign override → trip-assigned notification dispatch

Builds on **W335** (unassigned bulk assign gate override Playwright), **W331** (cross-product Compliance Core gate → RoutArr unassigned assign), **W281** (trip-assigned notification dispatch journey), **W127** (dispatch notification hooks), and **W116** (load-test journey seed API).

Closes the W335 follow-up: cross-product operator journey wiring Compliance Core workflow gate block confirmation through RoutArr UI gate override assign into dispatch notification outbox visibility (`trip_assigned` in settings Recent dispatches) with optional internal process-batch verify.

Also fixes pre-existing **routarr-frontend** Vitest fixture drift (`closedAt` on trip summaries, `capturedByPersonId`/`createdAt`/`sizeBytes` on capture attachments) that blocked `npm run build`.

## Scope

### e2eApi fixtures (`tests/e2e-playwright/support/e2eApi.ts`)

| Helper | Purpose |
|--------|---------|
| `ensureComplianceCoreRoutArrDispatchGateTripAssignedNotificationFixture()` | W336 — block gate + unassigned trip + trip_assigned notification settings enabled (trip not pre-assigned) |
| `ComplianceCoreRoutArrDispatchGateTripAssignedNotificationFixture` | Extends gate journey fixture with `expectedEventKind: 'trip_assigned'` |
| *(reused)* `ensureComplianceCoreRoutArrDispatchGateBlockFixture()` | W331 block gate + unassigned trip seed |
| *(reused)* `processRoutArrDispatchNotificationBatch()` | W281 internal process-batch after pending row verify |

Cross-product wiring uses **APIs only** (Compliance Core fact sources + rule pack content + RoutArr trip/gate/driver-ref/notification settings endpoints). No cross-DB FKs.

### Playwright (`tests/e2e-playwright`)

| Spec | Route(s) | Coverage |
|------|----------|----------|
| `compliancecore-routarr-dispatch-gate-trip-assigned-notification-journey-smoke.spec.ts` | Compliance Core `/findings` → suite → RoutArr `/dispatch` unassigned assign → `/settings` | CC gate check `block` → UI override assign → pending `trip_assigned` dispatch row → optional process-batch → sent/failed/skipped |

Reuses Compliance Core test ids from W331: `findings-workflow-gates-panel`, `findings-workflow-gate-*`. Reuses RoutArr notification test ids from W281: `notification-settings-panel`, `notification-dispatch-row-{tripId}`.

### Catalog

- `StlE2ePlaywrightSpecCatalog.ComplianceCoreRoutArrDispatchGateTripAssignedNotificationJourneySmokeSpec` in `OperatorJourneySmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Operator_journey_smoke_specs_include_compliance_core_and_multi_handoff`

### routarr-frontend Vitest fixture fixes

- `DispatchCommandCenterPanel.test.tsx` — add `closedAt: null` on trip column fixtures
- `DriverPortalPanel.test.tsx` — add `closedAt` on portal trip row + execution summary mocks
- `TripsPanel.test.tsx` — add `closedAt: null` on trip list fixture
- `TripProofDvirReadPanel.test.tsx` — rename `uploadedByPersonId`/`uploadedAt` → `capturedByPersonId`/`createdAt`, add `sizeBytes`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/compliancecore-routarr-dispatch-gate-trip-assigned-notification-journey-smoke.spec.ts
```

Requires Compliance Core API (5107) + frontend (5177), RoutArr API (5105) + frontend (5180), and NexArr handoff. RoutArr must have Compliance Core workflow gate integration configured in compose (W87).

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/routarr-frontend
npm run test -- src/components/DispatchCommandCenterPanel.test.tsx src/components/DriverPortalPanel.test.tsx src/components/TripsPanel.test.tsx src/components/TripProofDvirReadPanel.test.tsx
npm run build
cd ../../tests/e2e-playwright
npm run test -- --list
```

## Out of scope

- Materialized dispatch gate decision snapshots on trip assign
- SupplyArr procurement exception post-cancel reopen (blocked on API reopen support)
- Cross-product notification journeys for trip-dispatched/in-progress/completed/cancelled after gate override (future slice)

## Remaining milestone gaps (M13 partial)

- Further RoutArr dispatch/notification cross-product depth (other event kinds after gate override)
- Additional cross-product operator journeys (e.g. gate → command-center assign → notification)
- Render V1 deployment hardening follow-ups per `00_SLICE_STATE.md`

## Next recommended slice

- **W338** — M13 Playwright cross-product: Compliance Core gate → RoutArr bulk dispatch override → `trip_assigned` notification dispatch journey; or routarr-frontend CI build job if product frontends should gate main CI
