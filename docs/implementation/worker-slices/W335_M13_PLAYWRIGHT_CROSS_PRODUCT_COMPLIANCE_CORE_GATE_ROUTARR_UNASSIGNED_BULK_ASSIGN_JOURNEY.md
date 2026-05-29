# W335 — M13 Playwright: Cross-product operator journey — Compliance Core workflow gate block/warn → RoutArr unassigned queue bulk assign override

Builds on **W334** (bulk dispatch panel gate override Playwright), **W333** (`confirmBulkDispatchPreview` in `UnassignedWorkQueuePanel`), **W331/W332** (cross-product Compliance Core gate → RoutArr assign/command-center journeys), **W87** (dispatch workflow gate integration), **W212** (unassigned work queue bulk assign), and **W116** (load-test journey seed API).

Closes the W334 follow-up: M13 Playwright coverage for the unassigned work queue bulk assign path with workflow gate block/warn cancel/confirm override (`ignoreWorkflowGateBlocks` on apply via `confirmBulkDispatchPreview`).

## Scope

### e2eApi fixtures (`tests/e2e-playwright/support/e2eApi.ts`)

| Helper | Purpose |
|--------|---------|
| `ensureComplianceCoreRoutArrDispatchGateUnassignedBulkBlockFixture()` | W335 alias — block gate + unassigned trip for `UnassignedWorkQueuePanel` bulk assign |
| `ensureComplianceCoreRoutArrDispatchGateUnassignedBulkWarnFixture()` | W335 alias — warn gate + unassigned trip for bulk assign warn path |
| *(reused)* `ensureComplianceCoreRoutArrDispatchGateBlockFixture()` / `WarnFixture()` | W331/W332 cross-product gate fixtures |

Cross-product wiring uses **APIs only** (Compliance Core fact sources + rule pack content + RoutArr trip/gate/driver-ref endpoints). No cross-DB FKs.

### Panel test ids (`UnassignedWorkQueuePanel.tsx`)

| Test id | Element |
|---------|---------|
| `unassigned-work-queue-panel` | Section root |
| `unassigned-queue-status` | Status / feedback line |
| `unassigned-trip-{tripId}` | Trip row |
| `bulk-assign-unassigned` | Bulk assign selected button |
| `unassigned-attention-filter` | Urgent-only filter checkbox |

### Playwright (`tests/e2e-playwright`)

| Spec | Route(s) | Coverage |
|------|----------|----------|
| `compliancecore-routarr-dispatch-gate-unassigned-bulk-assign-journey-smoke.spec.ts` | Compliance Core `/findings` → suite → RoutArr `/dispatch` unassigned work queue | Block → CC gate check `block` → fixture trip visible + bulk assign enabled; block cancel/override via `confirmBulkDispatchPreview`; warn cancel/confirm |

Reuses Compliance Core test ids from W331: `findings-workflow-gates-panel`, `findings-workflow-gate-*`.

### Catalog

- `StlE2ePlaywrightSpecCatalog.ComplianceCoreRoutArrDispatchGateUnassignedBulkAssignJourneySmokeSpec` in `OperatorJourneySmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Operator_journey_smoke_specs_include_compliance_core_and_multi_handoff`

### Frontend unit test

- `UnassignedWorkQueuePanel.test.tsx` — bulk assign passes `ignoreWorkflowGateBlocks: true` when user confirms workflow gate block override

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/compliancecore-routarr-dispatch-gate-unassigned-bulk-assign-journey-smoke.spec.ts
```

Requires Compliance Core API (5107) + frontend (5177), RoutArr API (5105) + frontend (5180), and NexArr handoff. RoutArr must have Compliance Core workflow gate integration configured in compose (W87).

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/routarr-frontend
npm run test -- src/components/UnassignedWorkQueuePanel.test.tsx
npm run build
cd ../../tests/e2e-playwright
npm run test -- --list
```

## Out of scope

- Materialized dispatch gate decision snapshots on trip assign
- SupplyArr procurement exception post-cancel reopen (blocked on API reopen support)

## Remaining milestone gaps (M13 partial)

- Further RoutArr dispatch/notification depth Playwright if gaps remain
- Render V1 deployment hardening follow-ups per `00_SLICE_STATE.md`

## Next recommended slice

- Further RoutArr dispatch/notification depth Playwright, additional cross-product operator journeys, or Render V1 hardening per `00_SLICE_STATE.md`
