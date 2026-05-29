# W320 — M13 Playwright: TrainArr settings admin workspace smoke

Builds on **W232** (suite handoff product admin Playwright pattern), **W123/W161/W229** (notification + reminder/escalation settings), **W157–W164** (M12 worker settings panels), **W166** (integration settings), **W316/W318** (RoutArr/SupplyArr settings admin workspace pattern), and **W239** (audit export depth smoke — separate spec).

Completes consolidated **TrainArr product-admin** coverage for the `/settings` workspace: one browser session verifies all ten tenant-admin panels load with headings, save controls, and key worker/dispatch history sections (no save mutations).

## Scope

### Frontend (`SettingsSection`)

| Test id | Element |
|---------|---------|
| `trainarr-settings-admin-workspace` | Wrapper around all ten notification/worker settings panels |

Reuses existing panel test ids: `integration-settings-panel`, `notification-settings-panel`, `assignment-reminder-escalation-settings-panel`, `recertification-settings-panel`, `qualification-recalculation-settings-panel`, `rule-pack-impact-settings-panel`, `evidence-retention-settings-panel`, `orphan-reference-settings-panel`, `staffarr-publication-settings-panel`, `event-processing-settings-panel`.

Added to `NotificationSettingsPanel`: `notification-settings-panel`, `notification-settings-save`, `notification-dispatches-empty`, `notification-dispatches-list` (aligned with RoutArr/SupplyArr).

Audit package export panel (`trainarr-audit-package-export-panel`) remains outside the admin workspace wrapper; depth covered by W239.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `trainarr-settings-admin-workspace-smoke.spec.ts` | `/settings` | Handoff → admin workspace visible; all ten panels with headings + save buttons; integration probes + notification dispatches + worker run/history sections loaded (empty or list) |

No settings save mutations (depth covered by W123/W161/W229/W157–W166+).

### Vitest

- `SettingsSection.test.tsx` — admin workspace test id + all ten panels for authorized admin; audit panel outside wrapper when `canReadAudit`; permission message for unauthorized roles

### Catalog

- `StlE2ePlaywrightSpecCatalog.TrainArrSettingsAdminWorkspaceSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w320`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/trainarr-settings-admin-workspace-smoke.spec.ts
```

Requires TrainArr API and frontend (5176). Demo admin / platform admin with `canManageNotificationSettings`.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/trainarr-frontend
npm run test -- --run SettingsSection
cd ../../tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/trainarr-settings-admin-workspace-smoke.spec.ts
```

## Out of scope

- Per-panel save/reload depth (W123/W161/W229/W157–W166+)
- Audit package export depth (W239)
- MaintainArr settings admin workspace smoke (W321)

## Next recommended slice

- **M13 Playwright** — MaintainArr settings admin workspace smoke (`maintainarr-settings-admin-workspace` wrapper + all product-admin panels), RoutArr dispatch/notification depth, or next milestone backlog item per `00_SLICE_STATE.md`
