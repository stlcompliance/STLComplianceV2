# Render staging load soak (V1)

**Status:** Adopted — 2026-05-27  
**Scope:** M13 k6 load-test harness against Render staging web services  
**SLO profile:** `product-owner` (full thresholds from `PRODUCT_OWNER_LOAD_SLO_V1.md`)

## Purpose

Operators and CI can run the same seven product-owner k6 scenarios used locally against Render staging API URLs, with pre-run health gates and SLO validation.

## Environment variables

| Variable | Purpose |
|----------|---------|
| `RENDER_STAGING_NEXARR_API_URL` | NexArr staging API base URL |
| `RENDER_STAGING_STAFFARR_API_URL` | StaffArr staging API base URL |
| `RENDER_STAGING_TRAINARR_API_URL` | TrainArr staging API base URL |
| `RENDER_STAGING_MAINTAINARR_API_URL` | MaintainArr staging API base URL |
| `RENDER_STAGING_ROUTARR_API_URL` | RoutArr staging API base URL |
| `RENDER_STAGING_SUPPLYARR_API_URL` | SupplyArr staging API base URL |
| `RENDER_STAGING_COMPLIANCECORE_API_URL` | Compliance Core staging API base URL |
| `RENDER_STAGING_LOAD_OUTPUT_DIRECTORY` | k6 summary output directory (optional) |
| `LOAD_RENDER_STAGING_LIVE` | Set to `1` for live xUnit staging soak |
| `STL_LOAD_DEMO_EMAIL` / `STL_LOAD_DEMO_PASSWORD` / `STL_LOAD_DEMO_TENANT_ID` | Staging tenant credentials for authenticated scenarios |
| `STL_LOAD_SUBJECT_PERSON_ID` / `STL_LOAD_QUALIFICATION_KEY` / `STL_LOAD_RULE_PACK_KEY` | Journey scenario fixtures (override when staging seeds differ) |

Each API URL is the public `https://…onrender.com` base URL from the Render Dashboard (no trailing path).

## Operator workflow

```powershell
# 1. Export staging API URLs (one per Render web service)
$env:RENDER_STAGING_NEXARR_API_URL = "https://nexarr-api-jdyi.onrender.com"
# ... repeat for all seven ...

# 2. Optional: staging demo credentials (defaults match docker-compose demo tenant)
$env:STL_LOAD_DEMO_EMAIL = "admin@demo.stl"
$env:STL_LOAD_DEMO_PASSWORD = "ChangeMe!Demo2026"

# 3. Full soak (5 VUs / 30s, all seven scenarios, product-owner SLOs)
./scripts/ops/render-staging-load-soak.ps1

# Single scenario smoke
./scripts/ops/render-staging-load-soak.ps1 -Scenario nexarr-auth-me -Vus 2 -Duration 10s
```

Linux / GitHub Actions:

```bash
export RENDER_STAGING_NEXARR_API_URL="https://nexarr-api-jdyi.onrender.com"
./scripts/ops/render-staging-load-soak.sh all
```

GitHub: run workflow **Load Staging Render** manually, or rely on the **weekly schedule** (Sunday 07:00 UTC) after configuring repository secrets matching the environment variables above. Scheduled runs skip cleanly when staging API URL secrets are not configured and **seed Compliance Core journey fixtures** before the soak when NexArr + Compliance Core URLs are available.

### Journey seed (before staging soak)

```powershell
./scripts/ops/compliancecore-staging-journey-seed.ps1
```

Seeds `driver_qualification` rule pack content, `driver_license_valid` fact source, and dispatch workflow gates required by `trainarr-qualification-check` and `routarr-dispatch-workflow-gate` k6 scenarios. See `StlLoadTestJourneySeedCatalog`.

## CI schedule

| Setting | Value |
|---------|-------|
| Workflow | `Load Staging Render` (`.github/workflows/load-staging-render.yml`) |
| Cron (UTC) | `0 7 * * 0` (Sunday 07:00 — after docker-compose nightly at 06:00) |
| Trigger | `schedule` + `workflow_dispatch` |
| Secret gate | Skips soak when any `RENDER_STAGING_*_API_URL` secret is missing |

Canonical C# definitions: `StlRenderStagingLoadSoakScheduleCatalog`.

## Soak defaults

| Setting | Value |
|---------|-------|
| Virtual users | 5 |
| Duration | 30s |
| Scenarios | All seven PO keys |
| SLO profile | `product-owner` (full min-request counts) |

Canonical C# definitions: `StlRenderStagingLoadSoakCatalog`, `StlRenderStagingLoadTestCatalog`.

## Related docs

- [Product-owner load SLO targets](./PRODUCT_OWNER_LOAD_SLO_V1.md)
- [Render staging snapshot DR drill](../implementation/worker-slices/W103_M13_RENDER_STAGING_SNAPSHOT_DRILL.md)
