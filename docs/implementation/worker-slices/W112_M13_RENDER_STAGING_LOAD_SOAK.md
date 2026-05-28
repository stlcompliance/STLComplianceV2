# M13 Render staging load soak against PO SLOs

## Slice name

M13 Render staging load soak — staging API URL catalog, operator scripts, workflow_dispatch CI, live xUnit soak tests, product-owner SLO profile

## Products touched

- **STLCompliance.Shared** — `StlRenderStagingLoadTestCatalog`, `StlRenderStagingLoadTestSupport`, `StlRenderStagingLoadSoakCatalog`
- **Platform ops** — `scripts/ops/render-staging-load-soak.*` (wraps `load-test-run.*`)
- **tests/STLCompliance.Load.Tests** — staging catalog unit tests + optional live soak (`LOAD_RENDER_STAGING_LIVE=1`)
- **CI** — `.github/workflows/load-staging-render.yml` (manual workflow_dispatch; requires GitHub secrets)
- **docs/operations** — `RENDER_STAGING_LOAD_SOAK_V1.md`

## Shared additions

| File | Purpose |
|------|---------|
| `StlRenderStagingLoadTestCatalog.cs` | Seven Render API services + `RENDER_STAGING_*_API_URL` → `STL_*_BASE_URL` mapping |
| `StlRenderStagingLoadTestSupport.cs` | Resolve/apply staging endpoints, health probe, live mode flag |
| `StlRenderStagingLoadSoakCatalog.cs` | Default 5 VU / 30s soak + full PO SLO targets |

## Environment variables

| Variable | Purpose |
|----------|---------|
| `RENDER_STAGING_NEXARR_API_URL` … `RENDER_STAGING_COMPLIANCECORE_API_URL` | Public Render API base URLs |
| `RENDER_STAGING_LOAD_OUTPUT_DIRECTORY` | k6 summary output (optional) |
| `LOAD_RENDER_STAGING_LIVE` | Set to `1` for live xUnit staging soak |
| `STL_LOAD_DEMO_*` / `STL_LOAD_SUBJECT_PERSON_ID` / journey keys | Authenticated scenario fixtures |

## Operator workflow

```powershell
$env:RENDER_STAGING_NEXARR_API_URL = "https://nexarr-api-jdyi.onrender.com"
# ... all seven ...
./scripts/ops/render-staging-load-soak.ps1
```

GitHub: workflow **Load Staging Render** with secrets for all seven API URLs and optional staging credentials.

## Tests

### Backend unit (`STLCompliance.Load.Tests`)

- `Catalog_includes_seven_render_api_entries`
- `Soak_catalog_covers_all_product_owner_scenarios`
- `ResolveEndpointsFromEnvironment_maps_render_urls_to_k6_env`
- `ResolveEndpointsFromEnvironment_throws_when_api_url_missing`
- `ApplyK6Environment_sets_product_owner_profile`
- `K6_scenario_meets_product_owner_staging_soak_slo` × 7 (Live category, skipped unless `LOAD_RENDER_STAGING_LIVE=1`)

## Verification commands

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj -c Release --filter "Category=Load&Category!=Live"
```

Optional live staging soak (requires k6 + all seven staging API URLs):

```powershell
$env:LOAD_RENDER_STAGING_LIVE = "1"
$env:RENDER_STAGING_NEXARR_API_URL = "https://..."
# ... all seven ...
dotnet test tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj -c Release --filter "FullyQualifiedName~RenderStagingLoadSoakLiveTests"
```

## Gap analysis update (M13)

| Area | Status after this slice |
|------|-------------------------|
| Load / performance | **Staging operator soak** — seven PO scenarios against Render URLs with health gate + SLO validation |
| DR / backup restore | Staging snapshot drill unchanged (W103) |

## Next recommended slice

StaffArr org-unit filter on person export UI, or scheduled weekly staging load soak in CI.
