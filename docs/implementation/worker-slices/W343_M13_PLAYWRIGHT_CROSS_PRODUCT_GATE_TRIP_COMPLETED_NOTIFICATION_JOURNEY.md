# W343 — M13 Playwright: Cross-product operator journey — Compliance Core workflow gate block → RoutArr unassigned assign override → command-center dispatch → bulk in_progress + completed status changes → trip-completed notification dispatch

Builds on **W342** (unassigned assign override → command-center dispatch → bulk in_progress → trip_in_progress notification Playwright), **W283** (trip-completed notification dispatch journey), **W336** (gate + trip_assigned notification fixtures), **W127** (dispatch notification hooks), and **W116** (load-test journey seed API).

Closes the W342 follow-up for `trip_completed`: cross-product operator journey wiring Compliance Core workflow gate block confirmation through RoutArr UI gate override assign, command-center Dispatch status transition, bulk dispatch in_progress then completed status changes, into dispatch notification outbox visibility (`trip_completed` in settings Recent dispatches) with optional internal process-batch verify.

## Scope

### e2eApi fixtures (`tests/e2e-playwright/support/e2eApi.ts`)

| Helper | Purpose |
|--------|---------|
| `ensureComplianceCoreRoutArrDispatchGateTripCompletedNotificationFixture()` | W343 — block gate + unassigned trip + trip_completed notification settings enabled (trip not pre-assigned) |
| `ComplianceCoreRoutArrDispatchGateTripCompletedNotificationFixture` | Extends gate journey fixture with `expectedEventKind: 'trip_completed'` |
| *(reused)* `ensureComplianceCoreRoutArrDispatchGateBlockFixture()` | W331 block gate + unassigned trip seed |
| *(reused)* `processRoutArrDispatchNotificationBatch()` | W280 internal process-batch after pending row verify |

Cross-product wiring uses **APIs only** (Compliance Core fact sources + rule pack content + RoutArr trip/gate/driver-ref/notification settings endpoints). No cross-DB FKs. Playwright performs assign override + command-center Dispatch + bulk dispatch in_progress/completed in browser; notification settings seeded via API with only `notifyOnTripCompleted` enabled.

### Playwright (`tests/e2e-playwright`)

| Spec | Route(s) | Coverage |
|------|----------|----------|
| `compliancecore-routarr-dispatch-gate-trip-completed-notification-journey-smoke.spec.ts` | Compliance Core `/findings` → suite → RoutArr `/dispatch` unassigned assign + command center + bulk dispatch → `/settings` | CC gate check `block` → UI override assign → command-center Dispatch → bulk dispatch `in_progress` → bulk dispatch `completed` → pending `trip_completed` dispatch row → optional process-batch → sent/failed/skipped |

Reuses Compliance Core test ids from W331: `findings-workflow-gates-panel`, `findings-workflow-gate-*`. Reuses RoutArr dispatch test ids from W235/W337/W341/W342: `dispatch-command-center-panel`, `command-center-trip-*`, `command-center-dispatch-*`, `command-center-status`, `bulk-dispatch-panel`, `bulk-trip-*`, `bulk-dispatch-preview`, `bulk-dispatch-apply`, `bulk-dispatch-status`, `bulk-preview-summary-*`. Reuses RoutArr notification test ids from W280: `notification-settings-panel`, `notification-dispatch-row-{tripId}`.

### Catalog

- `StlE2ePlaywrightSpecCatalog.ComplianceCoreRoutArrDispatchGateTripCompletedNotificationJourneySmokeSpec` in `OperatorJourneySmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Operator_journey_smoke_specs_include_compliance_core_and_multi_handoff`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/compliancecore-routarr-dispatch-gate-trip-completed-notification-journey-smoke.spec.ts
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

- Cross-product notification journeys for trip_cancelled after gate override (delivered in W344)
- Command-center/bulk/unassigned-bulk paths for trip_completed without unassigned assign + dispatch + in_progress prelude (future slices)
- Materialized dispatch gate decision snapshots on trip status change

## Remaining milestone gaps (M13 partial)

- Remaining product frontend CI build jobs (staffarr/trainarr/maintainarr/supplyarr/compliancecore)
- Render V1 deployment hardening follow-ups per `00_SLICE_STATE.md`

## Next recommended slice

- **W344** — M13 Playwright cross-product gate override → `trip_cancelled` notification after assign + dispatch + cancelled status change (Complete); next: staffarr-frontend CI build job or Render V1 hardening
