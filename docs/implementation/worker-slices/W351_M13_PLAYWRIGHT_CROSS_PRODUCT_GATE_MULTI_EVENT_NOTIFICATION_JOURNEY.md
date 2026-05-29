# W351 — M13 Playwright: Cross-product operator journey — Compliance Core workflow gate block → RoutArr full lifecycle → multi-event notification dispatch

Builds on **W344** (gate override → trip_cancelled notification), **W343** (gate override → trip_completed notification), **W285** (RoutArr-only multi-event notification completed path), **W336–W341** (gate + single-event notification journeys), **W127** (dispatch notification hooks), and **W116** (load-test journey seed API).

Closes the backlog item for cross-product operator journeys beyond the W341–W344 single-event notification set: one Playwright journey wires Compliance Core workflow gate block confirmation through RoutArr UI gate override assign, command-center Dispatch, bulk in_progress, and bulk completed, then verifies all four completion-path notification event kinds enqueue pending rows in settings Recent dispatches (with `trip_cancelled` absent when disabled).

## Scope

### e2eApi fixtures (`tests/e2e-playwright/support/e2eApi.ts`)

| Helper | Purpose |
|--------|---------|
| `ensureComplianceCoreRoutArrDispatchGateMultiEventNotificationFixture()` | W351 — block gate + unassigned trip + all completion-path notification toggles enabled (cancelled off) |
| `ComplianceCoreRoutArrDispatchGateMultiEventNotificationFixture` | Extends gate journey fixture with `expectedEventKinds` + `absentEventKinds` |
| *(reused)* `ensureComplianceCoreRoutArrDispatchGateBlockFixture()` | W331 block gate + unassigned trip seed |
| *(reused)* `processRoutArrDispatchNotificationBatch()` | W127 internal process-batch after pending row verify |

Cross-product wiring uses **APIs only** (Compliance Core fact sources + rule pack content + RoutArr trip/gate/driver-ref/notification settings endpoints). No cross-DB FKs. Playwright performs assign override + command-center Dispatch + bulk dispatch status changes in browser.

### Playwright (`tests/e2e-playwright`)

| Spec | Route(s) | Coverage |
|------|----------|----------|
| `compliancecore-routarr-dispatch-gate-multi-event-notification-journey-smoke.spec.ts` | Compliance Core `/findings` → suite → RoutArr `/dispatch` + `/settings` | CC gate check `block` → UI override assign → command-center Dispatch → bulk `in_progress` → bulk `completed` → Recent dispatches pending rows for `trip_assigned`, `trip_dispatched`, `trip_in_progress`, `trip_completed` (no `trip_cancelled`) → optional process-batch → sent/failed/skipped |

Reuses Compliance Core test ids from W331 and RoutArr dispatch/notification test ids from W341–W343 and W285 (`notification-dispatches-list` + per-event `li` filters).

### Catalog

- `StlE2ePlaywrightSpecCatalog.ComplianceCoreRoutArrDispatchGateMultiEventNotificationJourneySmokeSpec` in `OperatorJourneySmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Operator_journey_smoke_specs_include_compliance_core_and_multi_handoff`
- `StlE2ePlaywrightSpecCatalog.All.Count >= 52`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/compliancecore-routarr-dispatch-gate-multi-event-notification-journey-smoke.spec.ts
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

- SupplyArr procurement exception post-cancel reopen (blocked until API supports reopen)
- Live Render staging ship-gate proof (operator deploy step)
- Command-center/bulk/unassigned-bulk paths for multi-event without unassigned assign + dispatch prelude
- Live webhook sink / capture server

## Remaining milestone gaps (M13 partial)

- `FINAL_IMPLEMENTATION_REPORT.md` consolidation after remaining M13 slices
- Additional cross-product operator journeys (e.g. TrainArr qualification + RoutArr driver eligibility gate)

## Next recommended slice

- **W353** — `FINAL_IMPLEMENTATION_REPORT.md` consolidation; or additional M13 cross-product operator journeys
