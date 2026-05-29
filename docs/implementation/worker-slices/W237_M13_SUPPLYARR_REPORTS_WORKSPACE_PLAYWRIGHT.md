# W237 — M13 Playwright: SupplyArr Reports workspace smoke

Builds on **W232** / **W236** (suite handoff pattern) and SupplyArr M12 reporting panels (**W181** vendor reports, **W183** purchasing reports).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `supplyarr-reports-workspace-smoke.spec.ts` | `/reports` | Suite sign-in → handoff → `vendor-reports-panel` (approval filter, active-only checkbox, Export CSV visible/enabled, summary chips or empty state); `purchasing-reports-panel` (open-documents filter, Export CSV, PR totals or empty state) |

Download click is optional — smoke asserts export controls only.

### Catalog

- `StlE2ePlaywrightSpecCatalog.SupplyArrReportsWorkspaceSmokeSpec` in `ProductAdminSmokeSpecs`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_supplyarr_w230_w237`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/supplyarr-reports-workspace-smoke.spec.ts
```

Requires SupplyArr API (5106) and frontend (5179).

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
```

## Out of scope

- CSV download assertion (export button presence only)
- ~~Parts inventory / compliance report panels~~ (covered by W319)

## Next slice

- **M13 Playwright** — SupplyArr reports workspace consolidation (W319)
- **Compliance Core M12** — audit delivery orchestration UI
- **RoutArr** — dispatch exception queue Playwright (W210)
