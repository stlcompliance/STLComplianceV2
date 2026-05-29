# W321 — M13 Playwright: MaintainArr settings admin workspace smoke

Builds on **W232** (suite handoff product admin Playwright pattern), **W125** (notification settings panel), **W171/W172/W173/W249** (M12 worker settings panels), **W206** (asset bulk import — outside admin workspace), **W230/W132** (audit export — separate spec), and **W316/W318/W320** (RoutArr/SupplyArr/TrainArr settings admin workspace pattern).

Completes consolidated **MaintainArr product-admin** coverage for the `/settings` workspace: one browser session verifies all five tenant-admin panels load with headings, save controls, and key worker/dispatch history sections (no save mutations).

## Scope

### Frontend (`SettingsSection`)

| Test id | Element |
|---------|---------|
| `maintainarr-settings-admin-workspace` | Wrapper around all five notification/worker settings panels |

Reuses existing panel test ids: `pm-due-scan-settings-panel`, `maintenance-history-rollup-settings-panel`, `asset-status-rollup-settings-panel`, `defect-escalation-settings-panel`, `notification-settings-panel`.

Added save/history test ids aligned with RoutArr/SupplyArr/TrainArr:

- `NotificationSettingsPanel`: `notification-settings-panel`, `notification-settings-save`, `notification-dispatches-empty`, `notification-dispatches-list`
- Worker panels: `*-save`, `*-runs-empty`, `*-runs-list`, pending/events empty/list test ids where applicable

`asset-bulk-import-panel` and `maintainarr-audit-export-panel` remain outside the admin workspace wrapper; audit export depth covered by W230.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `maintainarr-settings-admin-workspace-smoke.spec.ts` | `/settings` | Handoff → admin workspace visible; all five panels with headings + save buttons; notification dispatches + worker pending/run/event sections loaded (empty or list) |

No settings save mutations (depth covered by W125/W171/W172/W173/W249+).

### Vitest

- `SettingsSection.test.tsx` — admin workspace test id + all five panels for authorized admin; audit panel outside wrapper when `canExportAudit`; admin workspace omitted for unauthorized notification roles

### Catalog

- `StlE2ePlaywrightSpecCatalog.MaintainArrSettingsAdminWorkspaceSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w321`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/maintainarr-settings-admin-workspace-smoke.spec.ts
```

Requires MaintainArr API and frontend (5178). Demo admin / platform admin with `canManageNotificationSettings`.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/maintainarr-frontend
npm run test -- --run SettingsSection
cd ../../tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/maintainarr-settings-admin-workspace-smoke.spec.ts
```

## Out of scope

- Per-panel save/reload depth (W125/W171/W172/W173/W249+)
- Audit package export depth (W230)
- Asset bulk import depth (W206)

## Next recommended slice

- **M13 Playwright** — RoutArr dispatch/notification depth, TrainArr reports workspace smoke, or next milestone backlog item per `00_SLICE_STATE.md`
