# W326 — M13 Playwright: Compliance Core operator workflow gate + dashboard journey smokes

Builds on **W144** (operator rule evaluate smoke), **W116** (load-test journey seed API), **W242** (audit delivery orchestration smoke), and **W232** (operator journey catalog pattern).

Extends Compliance Core **operator journey** Playwright coverage beyond rule evaluation: workflow gate check on journey-seeded dispatch gates and operator dashboard summary visibility.

## Scope

### Frontend test ids

| Test id | Component |
|---------|-----------|
| `findings-workflow-gates-panel` | `FindingsWorkflowGatesPanel` wrapper |
| `findings-workflow-gate-seed` | Seed sample workflow gate button |
| `findings-workflow-gate-select` | Gate selector |
| `findings-workflow-gate-check` | Run gate check button |
| `findings-workflow-gate-latest-result` | Last gate check outcome (`data-outcome`) |
| `compliancecore-operator-dashboard-panel` | `OperatorDashboardPanel` wrapper |

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `compliancecore-operator-workflow-gate-journey-smoke.spec.ts` | `/findings`, `/operator` | Journey seed → handoff → dispatch gate check with `driver_license_valid` → `allow`; operator dashboard summary cards (findings, evaluations, gates) load without spinner |

Uses `POST /api/load-test-journey/seed` (W116) — no UI seed/save mutations required for gate journey.

### Vitest

- `FindingsWorkflowGatesPanel.test.tsx` — panel + gate control test ids
- `OperatorDashboardPanel.test.tsx` — dashboard panel test id

### Catalog

- `StlE2ePlaywrightSpecCatalog.ComplianceCoreOperatorWorkflowGateJourneySmokeSpec` in `OperatorJourneySmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Operator_journey_smoke_specs_include_compliance_core_and_multi_handoff`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/compliancecore-operator-workflow-gate-journey-smoke.spec.ts
```

Requires Compliance Core API (5107) and frontend (5177). Demo platform admin with compliance operator permissions.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/compliancecore-frontend
npm run test -- --run FindingsWorkflowGatesPanel OperatorDashboardPanel
```

## Out of scope

- Rule evaluation UI depth (W144 `compliancecore-operator-rule-evaluate-smoke.spec.ts`)
- Batch workflow gate check / batch rule evaluate journeys
- Admin M12 worker settings save (W232) or audit delivery trigger clicks (W242)
- Findings emit-on-block journey (separate slice)

## Next recommended slice

- **M13 Playwright** — Compliance Core batch workflow gate check journey (completed by W327 for batch evaluate + findings emit); RoutArr dispatch/notification depth if gaps remain; or next milestone backlog item per `00_SLICE_STATE.md`
