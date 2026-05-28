# W247 — M13 Playwright: RoutArr driver portal smoke

Builds on **W213** (`DriverPortalPanel` at `/driver-portal`), **W245** (suite handoff → RoutArr Playwright pattern).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-driver-portal-smoke.spec.ts` | `/driver-portal` | Suite sign-in → handoff → `driver-portal-panel` with **Driver portal** heading; **Today** (`driver-portal-today`) and **Upcoming** (`driver-portal-upcoming`) sections; `driver-portal-trip-*` rows or schedule empty states |

Trip execution clicks are **out of scope** — no **Dispatch**, **Start trip**, **Complete**, or **Close** button presses (read-only schedule smoke).

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrDriverPortalSmokeSpec` in `ProductAdminSmokeSpecs`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w247`
- `All.Count >= 25`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-driver-portal-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Schedule is person-scoped; demo admin may see empty Today/Upcoming lists.

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
```

## Out of scope

- Dispatch / start / complete / close mutations
- Proof/DVIR capture (W217)
- Dispatch workspace panels (W235–245)

## Next slice

- **RoutArr** — proof/DVIR read panel Playwright on Dispatch (W248, complete)
- **MaintainArr** — PM due-scan worker observability (W51)
- **SupplyArr M8** — procurement exception resolution depth (W197)
