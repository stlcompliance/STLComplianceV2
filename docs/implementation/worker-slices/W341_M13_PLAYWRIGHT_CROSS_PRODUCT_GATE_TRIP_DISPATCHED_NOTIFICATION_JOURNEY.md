# W341 — M13 Playwright: Cross-product operator journey — Compliance Core workflow gate block → RoutArr unassigned assign override → command-center dispatch → trip-dispatched notification dispatch

Builds on **W339** (unassigned bulk assign gate override → trip_assigned notification Playwright), **W336** (unassigned assign gate override → trip_assigned notification Playwright), **W280** (trip-dispatched notification dispatch journey), **W127** (dispatch notification hooks), and **W116** (load-test journey seed API).

Closes the W339/W340 follow-up for `trip_dispatched`: cross-product operator journey wiring Compliance Core workflow gate block confirmation through RoutArr UI gate override assign, command-center Dispatch status transition, into dispatch notification outbox visibility (`trip_dispatched` in settings Recent dispatches) with optional internal process-batch verify.

## Scope

### e2eApi fixtures (`tests/e2e-playwright/support/e2eApi.ts`)

| Helper | Purpose |
|--------|---------|
| `ensureComplianceCoreRoutArrDispatchGateTripDispatchedNotificationFixture()` | W341 — block gate + unassigned trip + trip_dispatched notification settings enabled (trip not pre-assigned) |
| `ComplianceCoreRoutArrDispatchGateTripDispatchedNotificationFixture` | Extends gate journey fixture with `expectedEventKind: 'trip_dispatched'` |
| *(reused)* `ensureComplianceCoreRoutArrDispatchGateBlockFixture()` | W331 block gate + unassigned trip seed |
| *(reused)* `processRoutArrDispatchNotificationBatch()` | W280 internal process-batch after pending row verify |

Cross-product wiring uses **APIs only** (Compliance Core fact sources + rule pack content + RoutArr trip/gate/driver-ref/notification settings endpoints). No cross-DB FKs. Playwright performs assign override + command-center Dispatch in browser; notification settings seeded via API with only `notifyOnTripDispatched` enabled.

### Playwright (`tests/e2e-playwright`)

| Spec | Route(s) | Coverage |
|------|----------|----------|
| `compliancecore-routarr-dispatch-gate-trip-dispatched-notification-journey-smoke.spec.ts` | Compliance Core `/findings` → suite → RoutArr `/dispatch` unassigned assign + command center → `/settings` | CC gate check `block` → UI override assign → command-center Dispatch → pending `trip_dispatched` dispatch row → optional process-batch → sent/failed/skipped |

Reuses Compliance Core test ids from W331: `findings-workflow-gates-panel`, `findings-workflow-gate-*`. Reuses RoutArr dispatch test ids from W235/W337: `dispatch-command-center-panel`, `command-center-trip-*`, `command-center-dispatch-*`, `command-center-status`. Reuses RoutArr notification test ids from W280: `notification-settings-panel`, `notification-dispatch-row-{tripId}`.

### Catalog

- `StlE2ePlaywrightSpecCatalog.ComplianceCoreRoutArrDispatchGateTripDispatchedNotificationJourneySmokeSpec` in `OperatorJourneySmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Operator_journey_smoke_specs_include_compliance_core_and_multi_handoff`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/compliancecore-routarr-dispatch-gate-trip-dispatched-notification-journey-smoke.spec.ts
```

Requires Compliance Core API (5107) + frontend (5177), RoutArr API (5105) + frontend (5180), and NexArr handoff. RoutArr must have Compliance Core workflow gate integration configured in compose (W87).

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd tests/e2e-playwright
npm run test -- --list
```

## Out of scope

- Cross-product notification journeys for trip-in-progress/completed/cancelled after gate override (future slices)
- Command-center/bulk/unassigned-bulk paths for trip_dispatched (future slices after unassigned assign path)
- Materialized dispatch gate decision snapshots on trip status change

## Remaining milestone gaps (M13 partial)

- Cross-product gate override → trip_in_progress / trip_completed / trip_cancelled notification journeys
- Remaining product frontend CI build jobs (staffarr/trainarr/maintainarr/supplyarr/compliancecore)
- Render V1 deployment hardening follow-ups per `00_SLICE_STATE.md`

## Next recommended slice

- **W343** — M13 Playwright cross-product gate override → `trip_completed` notification after assign + dispatch + in_progress + completed status changes; or staffarr-frontend CI build job if product frontends should gate main CI; or Render V1 hardening
