# W244 — M13 Playwright: RoutArr dispatch active trips smoke

Builds on **W211** (`ActiveTripsPanel` on Dispatch workspace), **W243** (suite handoff → `/dispatch` Playwright pattern).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-dispatch-active-trips-smoke.spec.ts` | `/dispatch` | Suite sign-in → handoff → `active-trips-panel` with **Active trips** heading; **list** / **map** toggle (active state styling); list view shows `active-trip-row-*` or **No dispatched or in-progress trips in this window.**; map view shows `active-trips-map` with `active-trip-map-*` blocks or **No active trips in window**; returns to list view |

Optional `ensureRoutArrFieldInboxFixture` in `beforeAll` (best-effort) — same as W235; spec passes on empty active-trip window.

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrDispatchActiveTripsSmokeSpec` in `ProductAdminSmokeSpecs`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w244`
- `All.Count >= 23`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-dispatch-active-trips-smoke.spec.ts
```

Requires RoutArr API and frontend (5180).

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
```

## Out of scope

- Dispatching or starting trips (driver portal / execution flows)
- Exception queue (W243), command center scope toggle (W235)
- Geo map coordinates (W211 schedule-based timeline strip only)

## Next slice

- **RoutArr** — unassigned work queue Playwright (W212) → **W245 complete**
- **SupplyArr M8/M10** — procurement automation / coordination UX depth
- **MaintainArr** — preventive maintenance schedule worker settings Playwright when panels ship
- **NexArr** — platform-admin service token / worker health orchestration UI (if scoped)
