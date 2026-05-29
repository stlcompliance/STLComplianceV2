# W339 — M13 Playwright: Cross-product operator journey — Compliance Core workflow gate block → RoutArr unassigned bulk assign override → trip-assigned notification dispatch

Builds on **W338** (bulk dispatch gate override → trip_assigned notification Playwright), **W335** (cross-product Compliance Core gate → RoutArr unassigned queue bulk assign override), **W336** (unassigned assign gate override → trip_assigned notification Playwright), **W281** (trip-assigned notification dispatch journey), **W127** (dispatch notification hooks), and **W116** (load-test journey seed API).

Closes the W335 follow-up for the unassigned bulk assign path: completes the `trip_assigned` notification quartet (single unassigned assign W336, command-center W337, bulk dispatch W338, unassigned bulk assign W339) by wiring Compliance Core workflow gate block confirmation through RoutArr unassigned work queue bulk assign gate override into dispatch notification outbox visibility (`trip_assigned` in settings Recent dispatches) with optional internal process-batch verify.

## Scope

### e2eApi fixtures (`tests/e2e-playwright/support/e2eApi.ts`)

| Helper | Purpose |
|--------|---------|
| `ensureComplianceCoreRoutArrDispatchGateUnassignedBulkAssignTripAssignedNotificationFixture()` | W339 — block gate + unassigned trip + driver ref + trip_assigned notification settings enabled (trip not pre-assigned) |
| `ComplianceCoreRoutArrDispatchGateUnassignedBulkAssignTripAssignedNotificationFixture` | Extends gate journey fixture with `expectedEventKind: 'trip_assigned'` |
| *(reused)* `ensureComplianceCoreRoutArrDispatchGateBlockFixture()` | W331/W335 block gate + unassigned trip + driver ref seed |
| *(reused)* `processRoutArrDispatchNotificationBatch()` | W281 internal process-batch after pending row verify |

Cross-product wiring uses **APIs only** (Compliance Core fact sources + rule pack content + RoutArr trip/gate/driver-ref/notification settings endpoints). No cross-DB FKs.

### Playwright (`tests/e2e-playwright`)

| Spec | Route(s) | Coverage |
|------|----------|----------|
| `compliancecore-routarr-dispatch-gate-unassigned-bulk-assign-trip-assigned-notification-journey-smoke.spec.ts` | Compliance Core `/findings` → suite → RoutArr `/dispatch` unassigned bulk assign → `/settings` | CC gate check `block` → unassigned bulk assign override → pending `trip_assigned` dispatch row → optional process-batch → sent/failed/skipped |

Reuses Compliance Core test ids from W331: `findings-workflow-gates-panel`, `findings-workflow-gate-*`. Reuses RoutArr unassigned bulk assign test ids from W335: `unassigned-work-queue-panel`, `unassigned-trip-*`, `bulk-assign-unassigned`, `unassigned-queue-status`. Reuses RoutArr notification test ids from W281: `notification-settings-panel`, `notification-dispatch-row-{tripId}`.

### Catalog

- `StlE2ePlaywrightSpecCatalog.ComplianceCoreRoutArrDispatchGateUnassignedBulkAssignTripAssignedNotificationJourneySmokeSpec` in `OperatorJourneySmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Operator_journey_smoke_specs_include_compliance_core_and_multi_handoff`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/compliancecore-routarr-dispatch-gate-unassigned-bulk-assign-trip-assigned-notification-journey-smoke.spec.ts
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

- Materialized dispatch gate decision snapshots on unassigned bulk assign apply
- SupplyArr procurement exception post-cancel reopen (blocked on API reopen support)
- Cross-product notification journeys for trip-dispatched/in-progress/completed/cancelled after unassigned bulk assign gate override (future slice)

## Remaining milestone gaps (M13 partial)

- Further RoutArr dispatch/notification cross-product depth (other event kinds after gate override via unassigned, command-center, bulk, or unassigned-bulk paths)
- routarr-frontend CI build job if product frontends should gate main CI
- Render V1 deployment hardening follow-ups per `00_SLICE_STATE.md`

## Next recommended slice

- **W340** — routarr-frontend CI build job if product frontends should gate main CI; or M13 Playwright cross-product gate override → other notification event kinds (trip-dispatched/in-progress/completed/cancelled); or Render V1 hardening
