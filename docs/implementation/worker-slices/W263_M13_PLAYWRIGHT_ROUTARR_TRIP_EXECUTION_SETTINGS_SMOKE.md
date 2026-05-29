# W263 — M13 Playwright: RoutArr trip execution settings panel smoke

Builds on **W257** (`TripExecutionSettingsPanel`, `/api/trip-execution-settings`), **W261** (attachment requirement toggles), **W259** (driver-portal capture depth companion), and **W236/W239** (product settings save Playwright patterns).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-settings-trip-execution-smoke.spec.ts` | `/settings` | Suite sign-in → handoff → `trip-execution-settings-panel`: heading; six core capture policy checkboxes; **Attachment requirements** section with five attachment toggles; toggle **Require post-trip DVIR before complete** → **Save capture policy** → reload verifies persistence → restore original toggle |

Save/restore uses a single non-critical policy flag so shared demo tenant state stays stable for W259 driver-portal smokes.

No `e2eApi` helpers required — UI toggle-back pattern matches W236/W239 settings smokes.

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrSettingsTripExecutionSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w263`
- `All.Count >= 31`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-settings-trip-execution-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo dispatcher/admin role can manage notification/trip execution settings (`canManageNotificationSettings`).

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet build STLCompliance.sln -c Release
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-settings-trip-execution-smoke.spec.ts
```

## Out of scope

- Driver-portal capture readiness / DVIR submit (W259)
- ~~Attachment upload flows (W261 follow-up Playwright)~~ → **W264 complete**
- Trip completion rollup or notification settings panels on same `/settings` page
- Bulk policy presets or audit export

## Next slice

- **M13 Playwright** — RoutArr dispatch proof/DVIR read attachment download smoke (dispatcher download path; builds on W261/W248)
