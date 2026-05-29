# W278 — M13 Playwright: RoutArr settings trip completion rollup panel smoke

Builds on **W176** (`TripCompletionRollupSettingsPanel`, `/api/trip-completion-rollup-settings`, rollup runs API), **W263** (RoutArr `/settings` save/reload Playwright pattern), and **W277** (settings worker panel runs empty/list section + `e2eApi` batch helper conventions).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-settings-trip-completion-rollup-smoke.spec.ts` | `/settings` | Suite sign-in → handoff → `trip-completion-rollup-settings-panel`: heading; enable checkbox + staleness hours input; toggle enable + change hours → **Save settings** → reload verifies persistence → **Recent worker runs** section shows empty state or run list → restore original enable/hours |

Save/restore keeps shared demo tenant stable. No live trip completion rollup batch in this smoke (read-only worker path; avoids mutating demo rollup materialization).

### Panel UX (`TripCompletionRollupSettingsPanel`)

- Always-visible **Recent worker runs** heading
- `trip-completion-rollup-runs-empty` when no runs recorded
- `trip-completion-rollup-runs-list` when runs exist (matches W277 attachment retention pattern)

### `e2eApi` helpers (optional for future fixture smokes)

- `upsertRoutArrTripCompletionRollupSettings`
- `processRoutArrTripCompletionRollupBatch`
- `issueRoutArrTripCompletionRollupWorkerToken`

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrSettingsTripCompletionRollupSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w278`
- `All.Count >= 43`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-settings-trip-completion-rollup-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo dispatcher/admin role can manage rollup settings (`RequireTripCompletionRollupSettingsManage`).

## Verification

```powershell
dotnet build STLCompliance.sln -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/routarr-frontend
npm test -- TripCompletionRollupSettingsPanel
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-settings-trip-completion-rollup-smoke.spec.ts
```

## Out of scope

- Live shared-worker trip completion rollup batch (would refresh demo trip rollup state)
- Pending rollups panel interactions when enabled (read-only visibility only via save toggle in smoke)
- Trip execution, attachment retention, or notification settings panels on same `/settings` page

## Next recommended slice

**M13 Playwright — RoutArr settings notification hooks panel smoke** (handoff → `/settings` notification settings panel, enable toggles + webhook URL save/reload; builds on W127/W263/W278).
