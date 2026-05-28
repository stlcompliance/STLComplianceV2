# W245 — M13 Playwright: RoutArr dispatch unassigned work queue smoke

Builds on **W212** (`UnassignedWorkQueuePanel` on Dispatch workspace), **W244** (suite handoff → `/dispatch` Playwright pattern).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-dispatch-unassigned-work-queue-smoke.spec.ts` | `/dispatch` | Suite sign-in → handoff → `unassigned-work-queue-panel` with **Unassigned work queue** heading; `unassigned-trip-*` rows with per-trip **Assign** + bulk bar (`bulk-assign-unassigned`, bulk driver select) when items exist; **No unassigned active trips in this window.** when empty |

Assignment clicks are **out of scope** — read-only visibility smoke only.

Optional `ensureRoutArrFieldInboxFixture` in `beforeAll` (best-effort, same as W235/W244).

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrDispatchUnassignedWorkQueueSmokeSpec` in `ProductAdminSmokeSpecs`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w245`
- `All.Count >= 24`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-dispatch-unassigned-work-queue-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo platform admin typically has `canAssign` (bulk bar renders when queue has items).

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
```

## Out of scope

- Clicking **Assign** or **Assign N selected**
- Exception queue (W243), active trips (W244), command center (W235)

## Next slice (product feature)

RoutArr `/dispatch` M13 smokes are complete for W209–212 panels. Prefer a **product** slice next:

- **SupplyArr M8/M10** — procurement automation / coordination operator UX (W177/W194 depth)
- **RoutArr M9** — driver portal (`/driver-portal`, W213) or proof/DVIR read panel enhancements (W217)
- **MaintainArr** — PM due-scan worker settings observability (W51)
