# W328 — M13 Playwright: Compliance Core operator batch workflow gate check journey smokes

Builds on **W326** (workflow gate + dashboard journey), **W327** (batch evaluate + findings emit), **W116** (load-test journey seed API), **W39** (batch workflow gate checks API + `BatchWorkflowGateCheckPanel`), and **W34** (single gate check).

Extends Compliance Core **operator journey** Playwright coverage with multi-gate batch workflow gate checks on journey-seeded dispatch gates using shared fact inputs and batch summary assertions.

## Scope

### Frontend test ids

| Test id | Component |
|---------|-----------|
| `batch-workflow-gate-check-panel` | `BatchWorkflowGateCheckPanel` wrapper |
| `batch-workflow-gate-gate-{gateKey}` | Per-gate selection checkbox |
| `batch-workflow-gate-emit-findings` | Emit findings when blocked checkbox |
| `batch-workflow-gate-run` | Run batch check button |
| `batch-workflow-gate-latest-result` | Batch summary (`data-allow-count`, `data-block-count`, `data-warn-count`) |

Reuses W326/W327 ids: `findings-workflow-gates-panel`, `findings-workflow-gate-findings-section`.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `compliancecore-operator-batch-workflow-gate-check-journey-smoke.spec.ts` | `/findings` | Journey seed → handoff → select three dispatch gates → shared `driver_license_valid` checked → batch allow summary; same gates with license unchecked + emit → batch block summary + findings list populated |

Uses `POST /api/load-test-journey/seed` (W116) — seeds `dispatch_driver_qualification`, `dispatch_hazmat`, `dispatch_hours_of_service` via `DispatchWorkflowGateSeedService`.

### Vitest

- `BatchWorkflowGateCheckPanel.test.tsx` — batch panel test ids and summary attributes

### Catalog

- `StlE2ePlaywrightSpecCatalog.ComplianceCoreOperatorBatchWorkflowGateCheckJourneySmokeSpec` in `OperatorJourneySmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Operator_journey_smoke_specs_include_compliance_core_and_multi_handoff`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/compliancecore-operator-batch-workflow-gate-check-journey-smoke.spec.ts
```

Requires Compliance Core API (5107) and frontend (5177). Demo platform admin with compliance operator permissions.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/compliancecore-frontend
npm run test -- --run BatchWorkflowGateCheckPanel
```

## Out of scope

- Single-gate check depth (W326/W327)
- Batch rule evaluation UI (W327 `/evaluation`)
- Admin M12 worker settings save (W232) or audit delivery trigger clicks (W242)
- Operator dashboard summary (W326)

## Next recommended slice

- **M13 Playwright** — RoutArr dispatch/notification depth if gaps remain; SupplyArr procurement exception post-cancel reopen only if API gains reopen support; or next milestone backlog item per `00_SLICE_STATE.md`
