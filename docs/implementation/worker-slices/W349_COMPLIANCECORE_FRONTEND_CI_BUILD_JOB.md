# W349 — compliancecore-frontend CI build job (complete Arr product frontend CI gates)

Builds on **W348** (supplyarr-frontend CI build job + `StlCiFrontendCatalog` pattern), **W347** (maintainarr-frontend CI build job), **W346** (trainarr-frontend CI build job), **W345** (staffarr-frontend CI build job), **W340** (routarr-frontend CI build job), **W101** (Playwright compose e2e profile with all product frontend previews 5174–5180), **W94/W232** (suite-frontend CI job pattern in `.github/workflows/ci.yml`), and **W92/W145** (M13 ship-gate catalog test patterns).

Completes the backlog item to gate **Compliance Core** product frontend builds/tests in main CI: adds a dedicated `compliancecore-frontend` GitHub Actions job and extends `StlCiFrontendCatalog` so all six Arr product frontend gates are explicit and auditable (6/6).

## Scope

### Shared catalog (`packages/shared-dotnet/STLCompliance.Shared/Operations/StlCiFrontendCatalog.cs`)

| Type | Purpose |
|------|---------|
| `StlCiFrontendJob` | Job id, app directory, package-lock path, build/test flags, product gate marker |
| `StlCiFrontendCatalog.ComplianceCoreFrontend` | compliancecore-frontend job entry |
| `StlCiFrontendCatalog.MainCiFrontendJobs` | stlcompliancesite + suite-frontend + routarr + staffarr + trainarr + maintainarr + supplyarr + compliancecore |
| `StlCiFrontendCatalog.GatedProductFrontendJobs` | All six Arr product frontends gated in main CI |

### Catalog tests (`tests/STLCompliance.E2E/Catalog/StlCiFrontendCatalogTests.cs`)

| Test | Coverage |
|------|----------|
| `Main_ci_frontend_jobs_include_suite_routarr_staffarr_trainarr_maintainarr_supplyarr_and_compliancecore` | Catalog lists expected jobs |
| `Gated_product_frontend_jobs_start_with_routarr_staffarr_trainarr_maintainarr_supplyarr_then_compliancecore` | Gated product frontend order (6 jobs) |
| `Every_main_ci_frontend_job_has_package_lock_and_build_test_scripts` | Repo paths + npm scripts exist |
| `Main_ci_workflow_declares_every_catalog_frontend_job` | `.github/workflows/ci.yml` declares each job id + working-directory |
| `Gated_product_frontend_jobs_are_subset_of_main_ci_jobs` | Gate list consistency |

Trait: `Category=Ci`, `Area=Frontend`.

### CI workflow (`.github/workflows/ci.yml`)

| Job / step | Coverage |
|------------|----------|
| `compliancecore-frontend` | Node 22, `npm ci`, `npm run build`, `npm test` in `apps/compliancecore-frontend` |
| `dotnet` → `CI frontend catalog checks` | Existing step validates updated catalog via `Category=Ci` |

Mirrors existing `supplyarr-frontend` job shape (cache on package-lock, parallel ubuntu job).

### compliancecore-frontend CI unblock (`apps/compliancecore-frontend`)

No frontend build/test blockers required fixes — `npm ci`, `npm run build`, and `npm test` (22 files / 45 tests) pass cleanly on Node 22.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "Category=Ci"
cd apps/compliancecore-frontend
npm ci
npm run build
npm test
```

## Out of scope

- SupplyArr procurement exception post-cancel reopen (blocked on API reopen support)
- Additional cross-product gate override notification journeys (W344 completed the event-kind set)

## Remaining milestone gaps (M13 partial)

- Render V1 deployment hardening follow-ups per `00_SLICE_STATE.md`
- Additional M13 Playwright operator journeys beyond the W344 notification event-kind set

## Next recommended slice

- **W350** — Render V1 deployment hardening; or additional M13 Playwright cross-product operator journeys
