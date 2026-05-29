# W318 — M13 Playwright: SupplyArr settings admin workspace smoke

Builds on **W232** (suite handoff product admin Playwright pattern), **W236** (integration event settings + readiness smoke), **W279/W297** (notification + escalation settings panel smokes), and **W316** (RoutArr settings admin workspace pattern).

Completes consolidated **SupplyArr product-admin** coverage for the `/settings` workspace: one browser session verifies all nine tenant-admin panels load with headings, save controls, and key worker/dispatch history sections (no save mutations).

## Scope

### Frontend (`SettingsSection`)

| Test id | Element |
|---------|---------|
| `supplyarr-settings-admin-workspace` | Wrapper around all nine settings panels |

Reuses existing panel test ids: `notification-settings-panel`, `price-snapshot-settings-panel`, `lead-time-snapshot-settings-panel`, `availability-snapshot-settings-panel`, `procurement-coordination-settings-panel`, `approval-reminder-settings-panel`, `procurement-exception-escalation-settings-panel`, `demand-processing-settings-panel`, `integration-event-settings-panel`.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `supplyarr-settings-admin-workspace-smoke.spec.ts` | `/settings` | Handoff → admin workspace visible; all nine panels with headings + save buttons; notification dispatches + escalation runs + integration outbox/inbox sections loaded (empty or list) |

No settings save mutations (depth covered by W236/W279/W297+).

### Vitest

- `SettingsSection.test.tsx` — admin workspace test id + all nine panels for authorized admin; permission message for unauthorized roles

### Catalog

- `StlE2ePlaywrightSpecCatalog.SupplyArrSettingsAdminWorkspaceSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w318`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/supplyarr-settings-admin-workspace-smoke.spec.ts
```

Requires SupplyArr API and frontend (5179). Demo admin / platform admin with `canManageNotificationSettings`.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/supplyarr-frontend
npm run test -- --run SettingsSection
cd ../../tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/supplyarr-settings-admin-workspace-smoke.spec.ts
```

## Out of scope

- Per-panel save/reload depth (W236/W279/W297+)
- SupplyArr reports workspace smoke (W237)
- Procurement exception post-cancel reopen (no API)

## Next recommended slice

- **M13 Playwright** — SupplyArr reports workspace consolidation (`supplyarr-reports-workspace` wrapper + all five M12 report panels), TrainArr/MaintainArr settings admin workspace smokes, or RoutArr dispatch/notification depth per `00_SLICE_STATE.md`
