# W340 — routarr-frontend CI build job (gate product frontends in main CI)

Builds on **W339** (cross-product gate → unassigned bulk assign → trip_assigned notification Playwright), **W101** (Playwright compose e2e profile with all product frontend previews 5174–5180), **W94/W232** (suite-frontend CI job pattern in `.github/workflows/ci.yml`), and **W92/W145** (M13 ship-gate catalog test patterns).

Closes the backlog item to gate **RoutArr** product frontend builds/tests in main CI: adds a dedicated `routarr-frontend` GitHub Actions job and an auditable `StlCiFrontendCatalog` so future product frontend gates stay explicit.

## Scope

### Shared catalog (`packages/shared-dotnet/STLCompliance.Shared/Operations/StlCiFrontendCatalog.cs`)

| Type | Purpose |
|------|---------|
| `StlCiFrontendJob` | Job id, app directory, package-lock path, build/test flags, product gate marker |
| `StlCiFrontendCatalog.MainCiFrontendJobs` | stlcompliancesite + suite-frontend + routarr-frontend |
| `StlCiFrontendCatalog.GatedProductFrontendJobs` | Product Arr frontends gated in main CI (starts with routarr) |

### Catalog tests (`tests/STLCompliance.E2E/Catalog/StlCiFrontendCatalogTests.cs`)

| Test | Coverage |
|------|----------|
| `Main_ci_frontend_jobs_include_suite_and_routarr` | Catalog lists expected jobs |
| `Gated_product_frontend_jobs_start_with_routarr` | First gated product frontend is RoutArr |
| `Every_main_ci_frontend_job_has_package_lock_and_build_test_scripts` | Repo paths + npm scripts exist |
| `Main_ci_workflow_declares_every_catalog_frontend_job` | `.github/workflows/ci.yml` declares each job id + working-directory |
| `Gated_product_frontend_jobs_are_subset_of_main_ci_jobs` | Gate list consistency |

Trait: `Category=Ci`, `Area=Frontend`.

### CI workflow (`.github/workflows/ci.yml`)

| Job / step | Coverage |
|------------|----------|
| `routarr-frontend` | Node 22, `npm ci`, `npm run build`, `npm test` in `apps/routarr-frontend` |
| `dotnet` → `CI frontend catalog checks` | `dotnet test ... --filter "Category=Ci"` |

Mirrors existing `suite-frontend` / `stlcompliancesite` job shape (cache on package-lock, parallel ubuntu job).

### routarr-frontend CI unblock (`apps/routarr-frontend`)

| File | Fix |
|------|-----|
| `DispatchCloseoutPanel.tsx` | Stable `openTrips` effect dependency (avoid `?? []` infinite re-render loop that hung `npm test`) |
| `DispatchCloseoutPanel.test.tsx` | Await async checklist/audit sections after query resolution |

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "Category=Ci"
cd apps/routarr-frontend
npm ci
npm run build
npm test
```

## Out of scope

- CI jobs for remaining product frontends (staffarr, trainarr, maintainarr, supplyarr, compliancecore) — follow-on slices can extend `GatedProductFrontendJobs`
- SupplyArr procurement exception post-cancel reopen (blocked on API reopen support)
- Cross-product gate override → trip-dispatched/in-progress/completed/cancelled notification journeys (future M13 slice)

## Remaining milestone gaps (M13 partial)

- Extend main CI product frontend gates to remaining five Arr frontends
- Cross-product gate override → other notification event kinds after assign/status change
- Render V1 deployment hardening follow-ups per `00_SLICE_STATE.md`

## Next recommended slice

- **W341** — M13 Playwright cross-product gate override → `trip_dispatched` notification after assign + status change; or extend main CI to next product frontend (e.g. staffarr-frontend); or Render V1 hardening
