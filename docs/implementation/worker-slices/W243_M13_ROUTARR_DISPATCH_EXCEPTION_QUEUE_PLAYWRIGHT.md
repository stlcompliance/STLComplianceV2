# W243 — M13 Playwright: RoutArr dispatch exception queue smoke

Builds on **W210** (`DispatchExceptionQueuePanel` on Dispatch workspace), **W235** (suite handoff → `/dispatch` Playwright pattern).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-dispatch-exception-queue-smoke.spec.ts` | `/dispatch` | Suite sign-in → handoff → `dispatch-exception-queue-panel` with **Exception queue** heading; when caller can triage (`canAssign`), create form placeholders and **Log exception** button visible; list shows `exception-row-*` rows or **No open exceptions** empty state |

Triage mutations (create, assign, resolve, link trip) are **out of scope** — read-only visibility smoke only.

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrDispatchExceptionQueueSmokeSpec` in `ProductAdminSmokeSpecs`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w243`
- `All.Count >= 22`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-dispatch-exception-queue-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo platform admin typically has dispatch assign/triage (`canTriage` = `canAssign` in `DispatchSection`).

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
```

## Out of scope

- Command center scope toggle (W235)
- Active trips map/list, unassigned work queue panels (W211–W212)
- Logging or resolving exceptions in live tenant

## Next slice

- **RoutArr** — active trips Playwright (W211) → **W244 complete**; unassigned work queue (W212)
- **SupplyArr M8/M10** — procurement automation / coordination UX depth
- **MaintainArr** — preventive maintenance schedule worker settings Playwright when panels ship
- **NexArr** — platform-admin service token / worker health orchestration UI (if scoped)
