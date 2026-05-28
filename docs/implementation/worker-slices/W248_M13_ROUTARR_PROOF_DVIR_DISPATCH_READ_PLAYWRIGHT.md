# W248 — M13 Playwright: RoutArr proof/DVIR dispatch read smoke

Builds on **W217** (`TripProofDvirReadPanel` on `/dispatch` for assign-capable roles), **W247** (suite handoff → RoutArr Playwright pattern).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-dispatch-proof-dvir-read-smoke.spec.ts` | `/dispatch` | Suite sign-in → handoff → `trip-proof-dvir-read-panel` with **Trip proof & DVIR** heading; trip GUID input and **Load execution** (disabled when empty); optional `ensureRoutArrFieldInboxFixture` trip ID → execution summary (`pre DVIR` line), `proof-row-*` / `dvir-row-*` or **No proof captured.** / **No DVIR submitted.** |

Proof/DVIR **capture and submit** are **out of scope** — read-only dispatcher lookup (`GET` execution summary) only.

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrDispatchProofDvirReadSmokeSpec` in `ProductAdminSmokeSpecs`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w248`
- `All.Count >= 26`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-dispatch-proof-dvir-read-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Panel renders only when `canAssignDrivers` is true (dispatcher/admin handoff). Fixture seed uses `POST /api/load-test-journey/seed` + assign + `assigned` status.

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
```

## Out of scope

- Driver portal proof/DVIR capture (W217 driver write path)
- `ProofDvirReportsPanel` on `/reports`
- Dispatch closeout, drag-assign, live exception triage

## Next slice

- **MaintainArr** — PM due-scan worker observability (W51)
- **SupplyArr M8** — procurement exception resolution depth (W197)
- **RoutArr** — dispatch closeout / drag-assign depth (W78/W82) or further M13 smokes
