# Worker 352 — Live Render staging ship-gate validation runbook/scripts

Builds on **W350** (Render Blueprint hardening + `render-blueprint-validate`), **W145** (`StlM13ShipGateCatalog`), **W351** (M13 Playwright cross-product notification journey), and existing staging operator scripts (`render-staging-load-soak`, DR drill).

Closes the backlog item for operator-facing validation against deployed Render staging URLs: health/ready probes, M13 ship-gate catalog gates, NexArr auth/platform probes, optional static-site checks, and optional extended live E2E.

## Scope

### Shared catalog (`packages/shared-dotnet/STLCompliance.Shared/Operations/`)

| Type | Purpose |
|------|---------|
| `StlRenderStagingShipGateCatalog` | Staging API/static-site env keys, CI filter tokens, runbook/script paths, GitHub workflow name |
| `StlRenderStagingShipGateSupport` | Resolve staging targets, health/ready probes, static-site header checks, demo credential resolution, map staging URLs to `E2E_*_URL` |

### Live probes (`tests/STLCompliance.E2E/Live/RenderStagingShipGateLiveTests.cs`)

| Test | Coverage |
|------|----------|
| `Staging_all_product_apis_report_liveness` | `/health` on all seven APIs |
| `Staging_all_product_apis_report_ready` | `/health/ready` (Blueprint health check path) |
| `Staging_nexarr_demo_login_succeeds` | Demo tenant login against staging NexArr |
| `Staging_nexarr_launch_context_denied_for_unknown_product` | M13 entitlement denial via real JWT |
| `Staging_nexarr_platform_health_aggregation_is_healthy` | NexArr `/api/platform/health` downstream probes |
| `Staging_optional_static_sites_respond_when_configured` | Optional static sites + security headers |

Trait: `Category=Live`, `Area=RenderStagingShipGate`. Requires `SHIP_GATE_RENDER_STAGING_LIVE=1` and all `RENDER_STAGING_*_API_URL` values.

### Catalog tests (`tests/STLCompliance.E2E/Catalog/StlRenderStagingShipGateCatalogTests.cs`)

| Test | Coverage |
|------|----------|
| `Api_probes_cover_all_render_staging_load_test_products` | Parity with `StlRenderStagingLoadTestCatalog` |
| `Api_probes_align_with_m13_openapi_product_keys` | Cross-check with `StlM13ShipGateCatalog` |
| `Operator_runbook_and_validate_scripts_exist` | Runbook + ps1/sh scripts present |
| `Main_ci_workflow_runs_render_staging_ship_gate_catalog_checks` | CI wiring |
| `Ship_gate_staging_render_workflow_exists` | GitHub workflow dispatch wiring |

Trait: `Category=Ci`, `Area=RenderStagingShipGate`.

### Operator scripts

| Script | Purpose |
|--------|---------|
| `scripts/ops/render-staging-ship-gate-validate.ps1` | Phased validation: local catalog → live API/auth/platform → optional E2E |
| `scripts/ops/render-staging-ship-gate-validate.sh` | Bash equivalent |

Phases: `local-catalog`, `api-health`, `optional-live-e2e`, `all` (default skips live when URLs unset).

### GitHub Actions

| Workflow | Purpose |
|----------|---------|
| `.github/workflows/ship-gate-staging-render.yml` | Manual staging ship-gate run with secret gate |
| `.github/workflows/ci.yml` | Adds `Render staging ship gate catalog checks` step |

### Docs

| File | Change |
|------|--------|
| `docs/operations/RENDER_STAGING_SHIP_GATE_V1.md` | Operator runbook |
| `docs/deployment/ENV_VARS_V1.md` | Ship gate validation section |
| `docs/implementation/worker-slices/00_SLICE_STATE.md` | Worker 352 row + next backlog |
| `docs/implementation/worker-slices/W351_M13_PLAYWRIGHT_CROSS_PRODUCT_GATE_MULTI_EVENT_NOTIFICATION_JOURNEY.md` | Points to W352 completion |

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "Category=Ci&Area=RenderStagingShipGate"
./scripts/ops/render-staging-ship-gate-validate.ps1 -Phase local-catalog
```

Live (operator — requires Render staging URLs):

```powershell
$env:RENDER_STAGING_NEXARR_API_URL = "https://nexarr-api-jdyi.onrender.com"
# ... all seven API URLs ...
./scripts/ops/render-staging-ship-gate-validate.ps1 -Phase api-health
```

## Out of scope

- Render Dashboard Blueprint apply / deploy (operator credentials required)
- Full Playwright staging journey battery
- Production DNS cutover

## Remaining milestone gaps (M13 partial)

- `FINAL_IMPLEMENTATION_REPORT.md` consolidation after remaining M13 slices
- Additional cross-product operator journeys (e.g. TrainArr qualification + RoutArr driver eligibility gate)
- Full-suite Playwright + load soak green against staging in one automated pipeline (operators can compose existing scripts)

## Next recommended slice

- **W353** — `FINAL_IMPLEMENTATION_REPORT.md` consolidation; or additional M13 cross-product operator journeys
