# W316 — M13 Playwright: RoutArr settings admin workspace smoke

Builds on **W232** (suite handoff product admin Playwright pattern), **W127** (notification settings panel), **W176** (trip completion rollup settings), **W257/W261** (trip execution settings), and individual settings panel smokes **W263/W277/W278/W279**.

Completes consolidated **RoutArr product-admin** coverage for the `/settings` workspace: one browser session verifies all four tenant-admin panels load with headings, save controls, and worker/dispatch history sections (no save mutations).

## Scope

### Frontend (`SettingsSection`)

| Test id | Element |
|---------|---------|
| `routarr-settings-admin-workspace` | Wrapper around all four settings panels |

Reuses existing panel test ids: `notification-settings-panel`, `trip-execution-settings-panel`, `trip-completion-rollup-settings-panel`, `attachment-retention-settings-panel`.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-settings-admin-workspace-smoke.spec.ts` | `/settings` | Handoff → admin workspace visible; all four panels with headings + save buttons; recent dispatches / worker runs / retention runs sections loaded (empty or list) |

No settings save mutations (depth covered by W263/W277/W278/W279+).

### Vitest

- `SettingsSection.test.tsx` — admin workspace test id + all four panels for `routarr_admin`; empty render for unauthorized roles

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrSettingsAdminWorkspaceSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w316`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-settings-admin-workspace-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin / platform admin with `canManageNotificationSettings`.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/routarr-frontend
npm run test -- --run SettingsSection
cd ../../tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-settings-admin-workspace-smoke.spec.ts
```

## Out of scope

- Per-panel save/reload depth (W263/W277/W278/W279+)
- Reports workspace smoke (W317 — dispatch/route/proof-DVIR/data exports panels; W241 covers audit export only)
- Post-cancel reopen (no API)

## Next recommended slice

- **M13 Playwright** — RoutArr reports workspace smoke (dispatch/route/proof-DVIR/data exports panels on `/reports`), SupplyArr procurement exception post-cancel reopen only if API gains reopen support, or next RoutArr dispatch/notification depth slice per milestone plan
