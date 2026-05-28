# M13 weekly Render staging load soak in CI

## Slice name

M13 weekly staging load soak schedule — GitHub Actions cron, secret-readiness gate, schedule catalog, unit tests

## Products touched

- **STLCompliance.Shared** — `StlRenderStagingLoadSoakScheduleCatalog`
- **CI** — `.github/workflows/load-staging-render.yml` (`schedule` + secret gate job)
- **docs/operations** — `RENDER_STAGING_LOAD_SOAK_V1.md` schedule section

## Shared additions

| File | Purpose |
|------|---------|
| `StlRenderStagingLoadSoakScheduleCatalog.cs` | Weekly cron, required secret env vars, readiness helpers |

## CI changes

- **Schedule:** Sunday 07:00 UTC (`0 7 * * 0`)
- **`evaluate` job:** skips soak when any of seven `RENDER_STAGING_*_API_URL` secrets is unset (no false-red for unconfigured forks)
- **Scheduled defaults:** all seven PO scenarios, 5 VUs, 30s (same as operator soak)
- **Artifact name:** includes `github.run_id` to avoid collisions across weekly runs

## Tests

### Backend unit (`STLCompliance.Load.Tests`)

- `Schedule_catalog_lists_seven_required_staging_api_url_env_vars`
- `AreStagingApiUrlsConfigured_returns_false_when_any_url_missing`
- `AreStagingApiUrlsConfigured_returns_true_when_all_urls_present`

## Verification commands

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj -c Release --filter "Category=Load&Category!=Live"
```

## Required GitHub secrets

All seven `RENDER_STAGING_*_API_URL` values plus optional `RENDER_STAGING_LOAD_*` credential overrides (see `RENDER_STAGING_LOAD_SOAK_V1.md`).

## Next recommended slice

StaffArr export filter presets, or Compliance Core rule-pack staging seeds for journey k6 scenarios.
