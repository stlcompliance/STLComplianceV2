# W347 — maintainarr-frontend CI build job (gate product frontends in main CI)

Builds on **W346** (trainarr-frontend CI build job + `StlCiFrontendCatalog` pattern), **W345** (staffarr-frontend CI build job), **W340** (routarr-frontend CI build job), **W101** (Playwright compose e2e profile with all product frontend previews 5174–5180), **W94/W232** (suite-frontend CI job pattern in `.github/workflows/ci.yml`), and **W92/W145** (M13 ship-gate catalog test patterns).

Closes the backlog item to gate **MaintainArr** product frontend builds/tests in main CI: adds a dedicated `maintainarr-frontend` GitHub Actions job and extends `StlCiFrontendCatalog` so product frontend gates stay explicit and auditable.

## Scope

### Shared catalog (`packages/shared-dotnet/STLCompliance.Shared/Operations/StlCiFrontendCatalog.cs`)

| Type | Purpose |
|------|---------|
| `StlCiFrontendJob` | Job id, app directory, package-lock path, build/test flags, product gate marker |
| `StlCiFrontendCatalog.MainCiFrontendJobs` | stlcompliancesite + suite-frontend + routarr + staffarr + trainarr + maintainarr |
| `StlCiFrontendCatalog.GatedProductFrontendJobs` | Product Arr frontends gated in main CI (routarr, staffarr, trainarr, maintainarr) |

### Catalog tests (`tests/STLCompliance.E2E/Catalog/StlCiFrontendCatalogTests.cs`)

| Test | Coverage |
|------|----------|
| `Main_ci_frontend_jobs_include_suite_routarr_staffarr_trainarr_and_maintainarr` | Catalog lists expected jobs |
| `Gated_product_frontend_jobs_start_with_routarr_staffarr_trainarr_then_maintainarr` | Gated product frontend order |
| `Every_main_ci_frontend_job_has_package_lock_and_build_test_scripts` | Repo paths + npm scripts exist |
| `Main_ci_workflow_declares_every_catalog_frontend_job` | `.github/workflows/ci.yml` declares each job id + working-directory |
| `Gated_product_frontend_jobs_are_subset_of_main_ci_jobs` | Gate list consistency |

Trait: `Category=Ci`, `Area=Frontend`.

### CI workflow (`.github/workflows/ci.yml`)

| Job / step | Coverage |
|------------|----------|
| `maintainarr-frontend` | Node 22, `npm ci`, `npm run build`, `npm test` in `apps/maintainarr-frontend` |
| `dotnet` → `CI frontend catalog checks` | Existing step validates updated catalog via `Category=Ci` |

Mirrors existing `trainarr-frontend` / `staffarr-frontend` job shape (cache on package-lock, parallel ubuntu job).

### maintainarr-frontend CI unblock (`apps/maintainarr-frontend`)

| File | Fix |
|------|-----|
| `SettingsSection.test.tsx` | Cast partial workspace mock through `unknown` so `tsc -b` passes during CI build (mirrors W345 `ReportsSection.test.tsx` pattern) |

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "Category=Ci"
cd apps/maintainarr-frontend
npm ci
npm run build
npm test
```

## Out of scope

- CI jobs for remaining product frontends (supplyarr, compliancecore) — follow-on slices can extend `GatedProductFrontendJobs`
- SupplyArr procurement exception post-cancel reopen (blocked on API reopen support)
- Additional cross-product gate override notification journeys (W344 completed the event-kind set)

## Remaining milestone gaps (M13 partial)

- Extend main CI product frontend gates to remaining two Arr frontends (supplyarr, compliancecore)
- Render V1 deployment hardening follow-ups per `00_SLICE_STATE.md`

## Next recommended slice

- **W348** — supplyarr-frontend CI build job if product frontends should gate main CI; or compliancecore-frontend CI build job; or Render V1 hardening
