# Worker 350 — Render V1 deployment hardening (follow-ups)

Builds on **W89** (initial V1 Blueprint inventory), **W349** (6/6 Arr product frontend CI gates), **W145** (M13 ship-gate catalog pattern), and **`StlIntegrationTokenCatalog`** (cross-product/service-token wiring).

Closes the backlog item to harden Render V1 deployment config: private-network internal API URLs, `sync: false` integration tokens, evidence persistent disks, static-site security headers, missing `shared-worker` job env wiring, Blueprint catalog CI gates, and operator validation scripts.

## Scope

### Blueprint (`render.yaml`)

| Area | Change |
|------|--------|
| `stl-internal-api-urls` | Private-network bases `http://{service}:10000` (replaces public HTTPS server-to-server URLs) |
| `shared-worker` | All job base URLs on private network; `fromGroup: stl-internal-api-urls`; missing `ComplianceCoreRuleChangeMonitor`, `SupplyArrIntegrationEvents`, `StaffArrPersonnelHistoryRollup` env keys; every job `*__ServiceToken` as `sync: false` |
| Product APIs | `Handoff__ServiceToken` + cross-product tokens as `sync: false` per `StlIntegrationTokenCatalog` |
| Evidence storage | 10 GB persistent disks on `trainarr-api` + `maintainarr-api` mounted at evidence root paths |
| Static frontends | Security headers (`X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy`) on all nine static sites |
| `stl-shared` | Documents optional `OTEL_EXPORTER_OTLP_ENDPOINT` placeholder |

### Shared catalog (`packages/shared-dotnet/STLCompliance.Shared/Operations/StlRenderBlueprintCatalog.cs`)

| Type | Purpose |
|------|---------|
| `StlRenderBlueprintCatalog` | Blueprint inventory (APIs, workers, static sites, DBs, env groups, evidence disks, internal URL keys, security headers) |
| `SyncFalseEnvKeysByConsumer()` | Derives Dashboard-secret env keys from `StlIntegrationTokenCatalog` |

### Integration token catalog (`StlIntegrationTokenCatalog.cs`)

Added missing `shared-worker` profiles: `StaffArrPersonExportDelivery`, `StaffArrPersonnelHistoryRollup`, `SupplyArrLeadTimeSnapshot`, `SupplyArrAvailabilitySnapshot`, `SupplyArrProcurementExceptionEscalations`.

### Catalog tests (`tests/STLCompliance.E2E/Catalog/StlRenderBlueprintCatalogTests.cs`)

| Test | Coverage |
|------|----------|
| `Blueprint_catalog_lists_*` | Inventory counts |
| `Internal_api_url_env_keys_map_to_private_network_base_urls` | URL builder conventions |
| `Sync_false_env_keys_by_consumer_cover_integration_token_catalog` | Token catalog parity |
| `Render_yaml_declares_blueprint_inventory_*` | `render.yaml` health checks, disks, headers, private URLs, sync:false tokens |
| `Main_ci_workflow_runs_render_blueprint_catalog_checks` | CI wiring |

Trait: `Category=Ci`, `Area=RenderBlueprint`.

### CI workflow (`.github/workflows/ci.yml`)

| Step | Coverage |
|------|----------|
| `Render blueprint catalog checks` | `Category=Ci&Area=RenderBlueprint` |
| `CI frontend catalog checks` | Scoped to `Area=Frontend` (unchanged behavior) |

### Docs

| File | Change |
|------|--------|
| `docs/deployment/ENV_VARS_V1.md` | Private-network internal URLs, disks, static security headers, expanded worker/API token tables |
| `docs/implementation/worker-slices/00_SLICE_STATE.md` | Worker 350 row + next backlog |
| `docs/implementation/worker-slices/W89_RENDER_V1_DEPLOYMENT_HARDENING.md` | Points to W350 follow-ups |

### Operator scripts

| Script | Purpose |
|--------|---------|
| `scripts/ops/render-blueprint-validate.ps1` | Local Blueprint catalog test gate + optional Render CLI validate |
| `scripts/ops/render-blueprint-validate.sh` | Bash equivalent |

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "Category=Ci&Area=RenderBlueprint"
./scripts/ops/render-blueprint-validate.ps1
# Optional when Render CLI v2.7+ is installed:
# render blueprints validate render.yaml
```

Post-deploy (Render Dashboard):

1. Sync Blueprint from repo root.
2. Confirm auto-provisioned integration tokens (`STL_INTEGRATION_TOKEN_AUTO_PROVISION=true`) or set `sync: false` secrets manually per `ENV_VARS_V1.md`.
3. Hit each API `/health/ready` (public URL) after deploy.
4. Rebuild static sites if public API hostnames change (custom domains).

## Out of scope

- Live Render deploy / MCP apply (operator step)
- Custom domain cutover (update public URL env groups when DNS changes)
- SupplyArr procurement exception post-cancel reopen (API support)

## Remaining milestone gaps (M13 partial)

- Additional M13 Playwright cross-product operator journeys beyond W344 notification set
- Full-suite live Render ship-gate proof (all products deployed + E2E live battery green)
- `FINAL_IMPLEMENTATION_REPORT.md` consolidation after remaining M13 slices

## Next recommended slice

- **W352** — Live Render staging ship-gate validation runbook/scripts (complete — see `W352_RENDER_STAGING_SHIP_GATE_VALIDATION.md`)
- **W353** — `FINAL_IMPLEMENTATION_REPORT.md` consolidation; or additional M13 Playwright cross-product operator journeys
