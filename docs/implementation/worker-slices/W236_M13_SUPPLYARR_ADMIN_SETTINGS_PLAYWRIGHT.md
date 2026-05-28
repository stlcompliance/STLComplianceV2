# W236 — M13 Playwright: SupplyArr admin/settings smoke

Builds on **W232** (suite handoff product admin Playwright pattern) and SupplyArr **integration event settings** (W187) + **supply readiness dashboard** (M12 reporting).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Routes | Coverage |
|------|--------|----------|
| `supplyarr-settings-integration-events-smoke.spec.ts` | `/settings`, `/readiness` | Suite sign-in → handoff → Settings `integration-event-settings-panel` (enabled toggle, retry interval, Save settings, outbox section); Readiness `supply-readiness-dashboard-panel` with metric labels |

### Catalog

- `StlE2ePlaywrightSpecCatalog.SupplyArrSettingsIntegrationEventsSmokeSpec` in `ProductAdminSmokeSpecs`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_supplyarr_w230_w236`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/supplyarr-settings-integration-events-smoke.spec.ts
```

Requires SupplyArr API (5106) and frontend (5179). Demo admin / platform admin has `canManageNotificationSettings` for Settings workspace.

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
```

## Out of scope

- Reports CSV export flows (vendor/purchasing panels)
- Demand processing worker live batch run

## Next slice

- **Suite M13** — SupplyArr Reports export Playwright or StaffArr audit export panel
- **Compliance Core M12** — audit delivery orchestration UI
- **RoutArr** — dispatch exception queue Playwright (W210)
