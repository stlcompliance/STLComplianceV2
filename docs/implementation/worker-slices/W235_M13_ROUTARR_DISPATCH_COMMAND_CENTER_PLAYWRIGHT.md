# W235 — M13 Playwright: RoutArr dispatch command center smoke

Builds on **W209** (dispatch command center API + panel), **W232** / **W234** (suite handoff Playwright pattern).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-dispatch-command-center-smoke.spec.ts` | RoutArr `/dispatch` | Suite sign-in → handoff → dispatch workspace; `dispatch-command-center-panel`; daily/weekly scope toggle; at least one `trip-column-*` with trip card or `No trips` empty state |

### E2E API (`support/e2eApi.ts`)

Reuses `ensureRoutArrFieldInboxFixture` in `beforeAll` (best-effort) so live stacks with RoutArr API seeded often show trip cards; spec still passes on empty columns.

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrDispatchCommandCenterSmokeSpec` in `ProductAdminSmokeSpecs`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_compliance_core_trainarr_and_routarr_w230_w235`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-dispatch-command-center-smoke.spec.ts
```

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
```

## Out of scope

- Exception queue, active trips map, unassigned work queue panels (separate slices W210–W212)
- Driver portal Playwright

## Next slice

- **Suite M13** — SupplyArr admin/settings Playwright parity
- **Compliance Core M12** — audit delivery orchestration UI
- **RoutArr** — dispatch exception queue or active trips Playwright depth
