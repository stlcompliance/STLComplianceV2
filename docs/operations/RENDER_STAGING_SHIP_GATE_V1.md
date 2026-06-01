# Render staging ship gate validation (V1)

**Status:** Adopted — 2026-05-28  
**Scope:** M13 operator validation against deployed Render staging URLs  
**Builds on:** W145 (`StlM13ShipGateCatalog`), W350 (`StlRenderBlueprintCatalog`), W351 (cross-product Playwright journeys)

## Purpose

Operators can prove a Render staging deployment meets the suite ship-gate minimum before promoting to production:

1. **Local catalog gates** — Blueprint inventory, M13 ship-gate catalog, OpenAPI parity (no live credentials required).
2. **Live API health** — all seven product APIs return `Healthy` on `/health` and `/health/ready`.
3. **Live auth + platform probes** — NexArr demo login, launch-context entitlement denial, NexArr platform health aggregation.
4. **Optional static sites** — when frontend URLs are configured, verify HTTP 200 and Blueprint security headers.
5. **Optional extended E2E** — map staging URLs into `E2E_*_URL` and run selected live entitlement probes.

Canonical C# definitions: `StlRenderStagingShipGateCatalog`, `StlRenderStagingShipGateSupport`.

## Environment variables

### Required for live API probes

| Variable | Purpose |
|----------|---------|
| `RENDER_STAGING_NEXARR_API_URL` | NexArr staging API base URL |
| `RENDER_STAGING_STAFFARR_API_URL` | StaffArr staging API base URL |
| `RENDER_STAGING_TRAINARR_API_URL` | TrainArr staging API base URL |
| `RENDER_STAGING_MAINTAINARR_API_URL` | MaintainArr staging API base URL |
| `RENDER_STAGING_ROUTARR_API_URL` | RoutArr staging API base URL |
| `RENDER_STAGING_SUPPLYARR_API_URL` | SupplyArr staging API base URL |
| `RENDER_STAGING_COMPLIANCECORE_API_URL` | Compliance Core staging API base URL |
| `SHIP_GATE_RENDER_STAGING_LIVE` | Set to `1` for live xUnit staging ship-gate probes |

Each API URL is the public `https://…onrender.com` base URL from the Render Dashboard (no trailing path).

### Optional credentials (staging demo tenant)

| Variable | Default |
|----------|---------|
| `STL_LOAD_DEMO_EMAIL` | `admin@demo.stl` |
| `STL_LOAD_DEMO_PASSWORD` | `ChangeMe!Demo2026` |
| `STL_LOAD_DEMO_TENANT_ID` | `11111111-1111-1111-1111-111111111101` |

Override when staging seeds use different demo credentials.

### Optional static site probes

Set any of these to enable static-site checks (skipped when unset):

| Variable | Static site |
|----------|-------------|
| `RENDER_STAGING_STLCOMPLIANCESITE_URL` | `stlcompliancesite` |
| `RENDER_STAGING_SUITE_FRONTEND_URL` | `suite-frontend` |
| `RENDER_STAGING_STAFFARR_FRONTEND_URL` | `staffarr-frontend` |
| `RENDER_STAGING_TRAINARR_FRONTEND_URL` | `trainarr-frontend` |
| `RENDER_STAGING_MAINTAINARR_FRONTEND_URL` | `maintainarr-frontend` |
| `RENDER_STAGING_ROUTARR_FRONTEND_URL` | `routarr-frontend` |
| `RENDER_STAGING_SUPPLYARR_FRONTEND_URL` | `supplyarr-frontend` |
| `RENDER_STAGING_COMPLIANCECORE_FRONTEND_URL` | `compliancecore-frontend` |
| `RENDER_STAGING_COMPANION_FRONTEND_URL` | `companion-frontend` |

Static probes assert HTTP success and Blueprint security headers (`X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy`).

## Operator workflow

### 1. Local catalog only (no Render credentials)

```powershell
./scripts/ops/render-staging-ship-gate-validate.ps1 -Phase local-catalog
```

Runs:

- `Category=Ci&Area=RenderStagingShipGate`
- `Category=Ci&Area=RenderBlueprint`
- `Category=E2e&Area=ShipGate`
- `Category=OpenApi&Area=ShipGate`

### 2. Full staging ship gate (recommended after deploy)

```powershell
# Export staging API URLs from Render Dashboard
$env:RENDER_STAGING_NEXARR_API_URL = "https://nexarr-api-3zlb.onrender.com"
$env:RENDER_STAGING_STAFFARR_API_URL = "https://staffarr-api-jdyi.onrender.com"
$env:RENDER_STAGING_TRAINARR_API_URL = "https://trainarr-api-jdyi.onrender.com"
$env:RENDER_STAGING_MAINTAINARR_API_URL = "https://maintainarr-api-jdyi.onrender.com"
$env:RENDER_STAGING_ROUTARR_API_URL = "https://routarr-api-jdyi.onrender.com"
$env:RENDER_STAGING_SUPPLYARR_API_URL = "https://supplyarr-api-jdyi.onrender.com"
$env:RENDER_STAGING_COMPLIANCECORE_API_URL = "https://compliancecore-api-jdyi.onrender.com"

# Optional: staging demo credentials
$env:STL_LOAD_DEMO_EMAIL = "admin@demo.stl"
$env:STL_LOAD_DEMO_PASSWORD = "ChangeMe!Demo2026"

# Optional: static frontend URLs
$env:RENDER_STAGING_SUITE_FRONTEND_URL = "https://suite-frontend-jdyi.onrender.com"

# Run local catalog + live API/auth/platform probes
./scripts/ops/render-staging-ship-gate-validate.ps1 -Phase api-health
```

Linux / macOS:

```bash
export RENDER_STAGING_NEXARR_API_URL="https://nexarr-api-3zlb.onrender.com"
# ... repeat for all seven API URLs ...
./scripts/ops/render-staging-ship-gate-validate.sh api-health
```

### 3. Extended optional live E2E

```powershell
./scripts/ops/render-staging-ship-gate-validate.ps1 -Phase optional-live-e2e
```

Adds optional entitlement-denial live probe (`Live_NexArr_launch_context_forbidden_for_unknown_product`) with staging URLs mapped into `E2E_*_URL`.

Product `/api/me` entitlement denial with locally minted JWTs remains a **docker-compose-only** probe unless operators also configure matching JWT signing keys — not required for staging ship gate.

## Post-deploy checklist (Render Dashboard)

1. Sync Blueprint from repo root (`render.yaml`).
2. Confirm migrations ran on all seven PostgreSQL databases.
3. Confirm `sync: false` integration tokens are set (or auto-provision succeeded) per `docs/deployment/ENV_VARS_V1.md`.
4. Run `./scripts/ops/render-blueprint-validate.ps1` (local Blueprint catalog).
5. Export staging URLs and run `./scripts/ops/render-staging-ship-gate-validate.ps1 -Phase api-health`.
6. Optionally run weekly load soak (`./scripts/ops/render-staging-load-soak.ps1`) and DR drill scripts after snapshot fetch.

## CI / GitHub Actions

| Workflow | Trigger | Secret gate |
|----------|---------|-------------|
| **Ship Gate Staging Render** (`.github/workflows/ship-gate-staging-render.yml`) | `workflow_dispatch` | Skips when any `RENDER_STAGING_*_API_URL` secret is missing |
| **CI** (`.github/workflows/ci.yml`) | push / PR | Runs `Category=Ci&Area=RenderStagingShipGate` catalog checks |

Configure repository secrets matching the seven required API URL environment variables. Optional frontend URL secrets enable static-site probes in the workflow.

## Related operator scripts

| Script | Purpose |
|--------|---------|
| `scripts/ops/render-blueprint-validate.ps1` | Local Blueprint catalog + optional Render CLI validate |
| `scripts/ops/render-staging-load-soak.ps1` | k6 load soak against staging APIs |
| `scripts/ops/render-staging-dr-restore-drill.ps1` | Staging database DR restore drill |

## Out of scope

- Applying Blueprint changes to Render (requires Dashboard / Render CLI credentials)
- Full Playwright cross-product journey battery against staging (use docker-compose nightly + optional manual Playwright with staging frontend URLs)
- Production cutover / custom domain DNS

## Canonical test filters

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "Category=Ci&Area=RenderStagingShipGate"
$env:SHIP_GATE_RENDER_STAGING_LIVE = "1"
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "Category=Live&Area=RenderStagingShipGate"
```
