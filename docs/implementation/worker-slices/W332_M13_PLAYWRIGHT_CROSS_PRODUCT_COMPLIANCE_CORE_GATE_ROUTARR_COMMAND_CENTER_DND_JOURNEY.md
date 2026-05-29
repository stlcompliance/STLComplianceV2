# W332 — M13 Playwright: Cross-product operator journey — Compliance Core workflow gate block/warn → RoutArr command-center drag-and-drop assign

Builds on **W331** (cross-product Compliance Core gate → RoutArr unassigned assign), **W78** (drag-and-drop assignment preview/confirm), **W252** (command center assignment depth), **W235** (command center panel smoke), and **W116** (load-test journey seed API).

Extends M13 **operator journey** Playwright coverage with command-center **drag-and-drop** driver assignment and full workflow gate **block / warn / allow** depth using shared cross-product gate fixtures from W331.

## Scope

### e2eApi fixtures (`tests/e2e-playwright/support/e2eApi.ts`)

| Helper | Purpose |
|--------|---------|
| `ensureRoutArrJourneyDriverPersonRef()` | Upserts journey subject driver into RoutArr `driver-refs` for command-center chips |
| `ensureComplianceCoreDispatchGateUnresolvedWarnRulePack()` | Idempotently adds `e2e_w332_dispatch_unresolved_fact` rule to journey rule pack |
| `ensureComplianceCoreDispatchGateUnresolvedFactResolvedOverride()` | Resolves W332 warn fact for block/allow fixtures after warn rule exists |
| `ensureComplianceCoreRoutArrDispatchGateWarnFixture()` | Journey seed + unresolved fact rule + license allow override + unassigned trip + API assert gate check returns `warn` |
| *(updated)* `ensureComplianceCoreRoutArrDispatchGateBlockFixture()` / `AllowFixture()` | Also ensure driver ref + W332 rule pack compatibility |

Cross-product wiring uses **APIs only** (Compliance Core fact sources + rule pack content + RoutArr trip/gate/driver-ref endpoints). No cross-DB FKs.

### Playwright (`tests/e2e-playwright`)

| Spec | Route(s) | Coverage |
|------|----------|----------|
| `compliancecore-routarr-dispatch-gate-command-center-dnd-journey-smoke.spec.ts` | Compliance Core `/findings` → suite → RoutArr `/dispatch` command center | Block → CC gate check `block` → drag driver chip onto trip → cancel; override confirm → `Driver assigned.`; allow → no dialog; warn → CC gate check `warn` → dismiss/confirm workflow gate warning dialog |

Reuses test ids: `findings-workflow-gates-panel`, `findings-workflow-gate-*`, `dispatch-command-center-panel`, `command-center-trip-*`, `command-center-driver-chip-*`, `command-center-status`.

### Catalog

- `StlE2ePlaywrightSpecCatalog.ComplianceCoreRoutArrDispatchGateCommandCenterDndJourneySmokeSpec` in `OperatorJourneySmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Operator_journey_smoke_specs_include_compliance_core_and_multi_handoff`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/compliancecore-routarr-dispatch-gate-command-center-dnd-journey-smoke.spec.ts
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

- Command-center vehicle drag-and-drop assign (driver-only in status columns per W252)
- Bulk dispatch UI `ignoreWorkflowGateBlocks` surfacing (W87 gap) — **closed by W333**
- Compliance Core UI-only gate outcomes not reflected in RoutArr fact sources

## Remaining milestone gaps (M13 partial)

- RoutArr dispatch/notification depth Playwright if gaps remain
- SupplyArr procurement exception post-cancel reopen (blocked on API reopen support)
- Render V1 deployment hardening follow-ups per `00_SLICE_STATE.md`

## Next recommended slice

- **M13 Playwright** — next milestone backlog item per `00_SLICE_STATE.md` (e.g. further RoutArr dispatch depth, cross-product journeys, or Render V1 hardening)
