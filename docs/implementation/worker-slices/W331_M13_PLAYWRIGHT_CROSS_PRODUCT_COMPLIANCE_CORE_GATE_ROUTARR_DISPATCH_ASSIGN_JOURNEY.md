# W331 — M13 Playwright: Cross-product operator journey — Compliance Core workflow gate block/warn → RoutArr dispatch assign

Builds on **W326–W328** (Compliance Core operator workflow gate journeys), **W87** (RoutArr ↔ Compliance Core dispatch workflow gate integration), **W245/W258** (RoutArr unassigned queue assign preview/cancel patterns), and **W116** (load-test journey seed API).

Extends M13 **operator journey** Playwright coverage with the first end-to-end cross-product browser flow: Compliance Core operator confirms a dispatch qualification gate outcome, then a dispatcher assigns a driver on RoutArr with matching Compliance Core workflow gate enforcement (block dialog + override, and allow path).

## Scope

### e2eApi fixtures (`tests/e2e-playwright/support/e2eApi.ts`)

| Helper | Purpose |
|--------|---------|
| `ensureComplianceCoreRoutArrDispatchGateBlockFixture()` | Journey seed (Compliance Core) + priority `-10` static `driver_license_valid=false` override + unassigned RoutArr trip + API assert `POST /api/dispatch-workflow-gates/check` returns `block` |
| `ensureComplianceCoreRoutArrDispatchGateAllowFixture()` | Journey seed + priority `-20` static `driver_license_valid=true` override + unassigned trip + API assert gate check returns `allow` |
| `checkRoutArrDispatchWorkflowGates()` | RoutArr dispatch workflow gate check helper for fixture validation |

Cross-product wiring uses **APIs only** (Compliance Core fact sources + RoutArr trip/gate endpoints). No cross-DB FKs.

### Playwright (`tests/e2e-playwright`)

| Spec | Route(s) | Coverage |
|------|----------|----------|
| `compliancecore-routarr-dispatch-gate-assign-journey-smoke.spec.ts` | Compliance Core `/findings` → suite → RoutArr `/dispatch` | Block fixture → gate check `block` in CC UI → unassigned assign shows workflow gate confirm → cancel; same with confirm override → `Driver assigned.`; allow fixture → CC allow + RoutArr assign without gate dialog |

Reuses test ids: `findings-workflow-gates-panel`, `findings-workflow-gate-*`, `unassigned-work-queue-panel`, `unassigned-trip-*`, `unassigned-queue-status`.

### Catalog

- `StlE2ePlaywrightSpecCatalog.ComplianceCoreRoutArrDispatchGateAssignJourneySmokeSpec` in `OperatorJourneySmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Operator_journey_smoke_specs_include_compliance_core_and_multi_handoff`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/compliancecore-routarr-dispatch-gate-assign-journey-smoke.spec.ts
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

- Compliance Core UI-only gate outcomes that are not reflected in RoutArr fact sources (UI manual facts vs internal evaluation)
- Live browser test for RoutArr workflow gate **warn** assign confirm (warn merge covered by `DispatchWorkflowGateRulesTests` + `dispatchAssignment.test.ts`; live warn fixture needs unresolved-fact source control)
- Bulk dispatch UI `ignoreWorkflowGateBlocks` surfacing (W87 gap)
- Drag-and-drop assignment panel journey (unassigned queue per-trip assign only)

## Remaining milestone gaps (M13 partial)

- RoutArr dispatch/notification depth Playwright if gaps remain
- Compliance Core operator journey extensions (reports/admin already covered W329–W330)
- SupplyArr procurement exception post-cancel reopen (blocked on API reopen support)
- Render V1 deployment hardening follow-ups per `00_SLICE_STATE.md`

## Next recommended slice

- **M13 Playwright** — RoutArr dispatch command-center drag-and-drop assignment with workflow gate block/warn depth (**W332**), or next milestone backlog item per `00_SLICE_STATE.md`
