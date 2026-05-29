# W279 — M13 Playwright: RoutArr settings notification hooks panel smoke

Builds on **W127** (`NotificationSettingsPanel`, `/api/notification-settings`, dispatch outbox + internal process-batch), **W263** (RoutArr `/settings` save/reload Playwright pattern), and **W278** (settings worker panel dispatches empty/list section + `e2eApi` batch helper conventions).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-settings-notification-hooks-smoke.spec.ts` | `/settings` | Suite sign-in → handoff → `notification-settings-panel`: heading; enable checkbox + webhook URL + trip-assigned event toggle; toggle enable + change webhook + flip trip-assigned → **Save notification settings** → reload verifies persistence → **Recent dispatches** section shows empty state or dispatch list → restore original enable/webhook/toggles |

Save/restore keeps shared demo tenant stable. No live dispatch notification batch in this smoke (read-only worker path; avoids mutating demo outbox delivery state).

### Panel UX (`NotificationSettingsPanel`)

- `data-testid="notification-settings-panel"` on section
- `notification-settings-enabled`, `notification-settings-webhook`, `notification-trip-assigned`, `notification-settings-save`
- `notification-dispatches-empty` when no dispatches recorded
- `notification-dispatches-list` when dispatches exist (matches W277/W278 runs pattern)

### `e2eApi` helpers (optional for future fixture smokes)

- `upsertRoutArrDispatchNotificationSettings`
- `processRoutArrDispatchNotificationBatch`
- `issueRoutArrDispatchNotificationWorkerToken`

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrSettingsNotificationHooksSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w279`
- `All.Count >= 44`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-settings-notification-hooks-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo dispatcher/admin role can manage notification settings (`RequireNotificationSettingsManage`).

## Verification

```powershell
dotnet build STLCompliance.sln -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/routarr-frontend
npm test -- NotificationSettingsPanel
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-settings-notification-hooks-smoke.spec.ts
```

## Out of scope

- Live shared-worker dispatch notification batch (would deliver demo webhook outbox rows)
- Trip assign/status mutations to seed outbox rows
- Trip execution, attachment retention, or trip completion rollup panels on same `/settings` page

## Next recommended slice

**M13 Playwright — RoutArr end-to-end notification dispatch journey smoke** (fixture trip status change → outbox row visible in Recent dispatches; optional internal process-batch read-only verify; builds on W127/W279).
