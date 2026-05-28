# Worker 232 — M13 Playwright product admin smokes (MaintainArr audit export + Compliance Core M12 worker)

Builds on **W230** (MaintainArr audit export UX), **W231** (Compliance Core M12 analytics worker settings), and **W138** (platform-admin audit export Playwright pattern).

## Slice choice

SupplyArr **M8/M10 integration** backlog items are worker-sliced through **W199** (`00_SLICE_STATE.md`). This slice implements the recommended **Suite M13 Playwright** coverage for the latest Compliance Core / MaintainArr admin surfaces.

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `maintainarr-settings-audit-export-smoke.spec.ts` | MaintainArr `/settings` | Manifest, summary, timeline, filters, CSV/ZIP/JSON preview, background ZIP job + internal `process-batch` helper |
| `compliancecore-m12-worker-settings-smoke.spec.ts` | Compliance Core `/admin` | M12 worker settings panel: enable, scope, interval, forecast + audit delivery toggles, save, last-run section |

### E2E API helpers (`support/e2eApi.ts`)

- `issueSharedWorkerServiceToken` — generic NexArr service token issuance (refactors NexArr-only helper)
- `processMaintainArrAuditPackageGenerationBatch` — `maintainarr.audit_packages.generate`
- `processComplianceCoreM12AnalyticsBatch` — `compliancecore.m12_analytics.process_batch` (available for future live batch smoke)

### Catalog

- `StlE2ePlaywrightSpecCatalog.ProductAdminSmokeSpecs`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_and_compliance_core_w230_w231`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/maintainarr-settings-audit-export-smoke.spec.ts
npx playwright test tests/compliancecore-m12-worker-settings-smoke.spec.ts
```

Without `E2E_LIVE`, specs skip (exit 0).

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
```

## Out of scope

- Full M12 analytics batch browser run (API integration covered in W231)
- SupplyArr integration (no remaining unsliced M8/M10 gaps in slice state)
- TrainArr demand callback Playwright depth
