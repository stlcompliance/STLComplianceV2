# W327 — M13 Playwright: Compliance Core operator batch evaluate + findings-emit-on-block journey smokes

Builds on **W144** (operator rule evaluate smoke), **W326** (workflow gate + dashboard journey), **W116** (load-test journey seed API), **W43** (admin batch evaluate UI), and **W39** (batch workflow gate checks).

Extends Compliance Core **operator journey** Playwright coverage with batch rule evaluation on journey-seeded rule packs and workflow gate check with findings emission when blocked.

## Scope

### Frontend test ids

| Test id | Component |
|---------|-----------|
| `batch-rule-evaluation-panel` | `BatchRuleEvaluationPanel` wrapper |
| `batch-rule-evaluation-pack-{packKey}` | Per-pack selection checkbox |
| `batch-rule-evaluation-emit-findings` | Emit findings on failed evaluation checkbox |
| `batch-rule-evaluation-run` | Run batch evaluation button |
| `batch-rule-evaluation-latest-result` | Batch summary (`data-allow-count`, `data-block-count`, `data-warn-count`) |
| `findings-workflow-gate-emit-findings` | Emit findings when blocked checkbox |
| `findings-workflow-gate-emitted-notice` | Emitted finding count notice after gate check |
| `findings-workflow-gate-findings-section` | Findings list section |

Reuses W326 ids: `findings-workflow-gates-panel`, `findings-workflow-gate-select`, `findings-workflow-gate-check`, `findings-workflow-gate-latest-result`.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `compliancecore-operator-batch-evaluate-findings-emit-journey-smoke.spec.ts` | `/evaluation`, `/findings` | Journey seed → handoff → batch rule evaluation with `driver_license_valid` → `allow`; dispatch gate check with emit enabled and license unchecked → `block` + finding emitted in list |

Uses `POST /api/load-test-journey/seed` (W116) — no UI seed/save mutations required.

### Vitest

- `BatchRuleEvaluationPanel.test.tsx` — batch panel test ids
- `FindingsWorkflowGatesPanel.test.tsx` — emit-findings + findings section test ids

### Catalog

- `StlE2ePlaywrightSpecCatalog.ComplianceCoreOperatorBatchEvaluateFindingsEmitJourneySmokeSpec` in `OperatorJourneySmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Operator_journey_smoke_specs_include_compliance_core_and_multi_handoff`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/compliancecore-operator-batch-evaluate-findings-emit-journey-smoke.spec.ts
```

Requires Compliance Core API (5107) and frontend (5177). Demo platform admin with compliance operator permissions.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/compliancecore-frontend
npm run test -- --run BatchRuleEvaluationPanel FindingsWorkflowGatesPanel
```

## Out of scope

- Batch workflow gate check panel depth (separate slice)
- Rule evaluation single-run UI depth (W144)
- Admin M12 worker settings save (W232) or audit delivery trigger clicks (W242)
- Operator dashboard summary (W326)

## Next recommended slice

- **M13 Playwright** — Compliance Core batch workflow gate check journey; RoutArr dispatch/notification depth if gaps remain; or next milestone backlog item per `00_SLICE_STATE.md`
