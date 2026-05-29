# W334 — M13 Playwright: Cross-product operator journey — Compliance Core workflow gate block/warn → RoutArr bulk dispatch panel override

Builds on **W333** (bulk dispatch UI `ignoreWorkflowGateBlocks` surfacing), **W331/W332** (cross-product Compliance Core gate → RoutArr assign/command-center journeys), **W87** (dispatch workflow gate integration), **W80** (`BulkDispatchPanel`), and **W116** (load-test journey seed API).

Closes the W333 follow-up: M13 Playwright coverage for the bulk dispatch panel workflow gate preview block/warn paths with cancel/confirm override (`ignoreWorkflowGateBlocks` on apply).

## Scope

### e2eApi fixtures (`tests/e2e-playwright/support/e2eApi.ts`)

| Helper | Purpose |
|--------|---------|
| `ensureComplianceCoreRoutArrDispatchGateBulkDispatchBlockFixture()` | W334 alias — block gate + unassigned trip for `BulkDispatchPanel` active trips |
| `ensureComplianceCoreRoutArrDispatchGateBulkDispatchWarnFixture()` | W334 alias — warn gate + unassigned trip for bulk dispatch warn apply path |
| *(reused)* `ensureComplianceCoreRoutArrDispatchGateBlockFixture()` / `WarnFixture()` | W331/W332 cross-product gate fixtures |

Cross-product wiring uses **APIs only** (Compliance Core fact sources + rule pack content + RoutArr trip/gate/driver-ref endpoints). No cross-DB FKs.

### Panel test ids (`BulkDispatchPanel.tsx`)

| Test id | Element |
|---------|---------|
| `bulk-dispatch-panel` | Section root |
| `bulk-dispatch-status` | Status / feedback line |
| `bulk-dispatch-preview` | Preview conflicts button |
| `bulk-dispatch-apply` | Apply to selected button |
| `bulk-trip-{tripId}` | Trip checkbox |
| `bulk-preview-{tripId}` | Preview result row |
| `bulk-preview-summary-{tripId}` | Preview summary text |

### Playwright (`tests/e2e-playwright`)

| Spec | Route(s) | Coverage |
|------|----------|----------|
| `compliancecore-routarr-dispatch-gate-bulk-dispatch-journey-smoke.spec.ts` | Compliance Core `/findings` → suite → RoutArr `/dispatch` bulk dispatch panel | Block → CC gate check `block` → preview shows workflow gate conflict → apply cancel; block override confirm → `Applied 1/1`; warn → preview + warn cancel/confirm apply |

Reuses Compliance Core test ids from W331: `findings-workflow-gates-panel`, `findings-workflow-gate-*`.

### Catalog

- `StlE2ePlaywrightSpecCatalog.ComplianceCoreRoutArrDispatchGateBulkDispatchJourneySmokeSpec` in `OperatorJourneySmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Operator_journey_smoke_specs_include_compliance_core_and_multi_handoff`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/compliancecore-routarr-dispatch-gate-bulk-dispatch-journey-smoke.spec.ts
```

Requires Compliance Core API (5107) + frontend (5177), RoutArr API (5105) + frontend (5180), and NexArr handoff. RoutArr must have Compliance Core workflow gate integration configured in compose (W87).

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/routarr-frontend
npm run test -- src/components/BulkDispatchPanel.test.tsx
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

- **M13 Playwright** — RoutArr unassigned queue bulk assign workflow gate override journey (**closed by W335**), further RoutArr dispatch/notification depth, or Render V1 hardening per `00_SLICE_STATE.md`
