# M13 authenticated k6 load-test flows

## Slice name

M13 authenticated k6 load-test flows — NexArr login/me and cross-product handoff bootstrap scenarios with shared auth helpers, engineering-default SLOs, operator script updates, and live k6 tests.

## Products touched

- **STLCompliance.Shared** — `StlLoadTestAuthDefaults`, extended `StlLoadTestSloCatalog`
- **tests/load-k6** — `lib/stl-auth.js`, `lib/stl-config.js`, `nexarr-auth-me.js`, `product-auth-handoff-me.js`
- **scripts/ops** — `load-test-run.ps1`, `load-test-run.sh`
- **tests/STLCompliance.Load.Tests** — catalog/auth unit tests + live `nexarr-auth-me` k6 probe
- **CI** — existing Load unit step; nightly live k6 job unchanged (runs all Live load tests when stack up)

## Shared additions

| File | Purpose |
|------|---------|
| `StlLoadTestAuthDefaults.cs` | Demo credential defaults + env var names for k6 |
| `StlLoadTestSloCatalog.cs` | `nexarr-auth-me`, `product-auth-handoff-me` SLO targets |

## k6 scenarios

| Key | Script | Flow |
|-----|--------|------|
| `nexarr-auth-me` | `tests/load-k6/scenarios/nexarr-auth-me.js` | POST `/api/auth/login` → GET `/api/me` |
| `product-auth-handoff-me` | `tests/load-k6/scenarios/product-auth-handoff-me.js` | Login → handoff → redeem → `/api/me` for all six product APIs |

Shared helpers live in `tests/load-k6/lib/stl-auth.js` and `stl-config.js`.

## Environment variables

| Variable | Default | Purpose |
|----------|---------|---------|
| `STL_LOAD_DEMO_EMAIL` | `admin@demo.stl` | NexArr login email |
| `STL_LOAD_DEMO_PASSWORD` | `ChangeMe!Demo2026` | NexArr login password |
| `STL_LOAD_DEMO_TENANT_ID` | demo tenant GUID | NexArr login tenant |
| `STL_LOAD_VUS` | scenario-specific | Virtual users |
| `STL_LOAD_DURATION` | `30s` | Scenario duration |
| `STL_*_BASE_URL` | localhost ports 5101–5107 | API base URLs |

## Operator workflow

```powershell
docker compose up -d postgres nexarr-api staffarr-api trainarr-api maintainarr-api routarr-api supplyarr-api compliancecore-api
./scripts/ops/load-test-run.ps1 -Scenario nexarr-auth-me -Vus 2 -Duration 10s
./scripts/ops/load-test-run.ps1 -Scenario product-auth-handoff-me -Vus 2 -Duration 15s
./scripts/ops/load-test-run.ps1   # all five scenarios
```

## Verification commands

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test STLCompliance.slnx -c Release --filter "Category!=Live"
dotnet test tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj -c Release --filter "Category=Load&Category!=Live"
```

Optional live k6 (docker-compose + k6 on PATH + seeded demo tenant):

```powershell
docker compose up -d postgres nexarr-api staffarr-api trainarr-api maintainarr-api routarr-api supplyarr-api compliancecore-api
$env:LOAD_LIVE = "1"
dotnet test tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj -c Release --filter "Category=Load&Category=Live"
```

## Gap analysis update (M13)

| Area | Status after this slice |
|------|-------------------------|
| Load / performance | **Authenticated flows ready** — login/me + handoff bootstrap k6 scenarios; engineering-default SLOs until PO publishes targets |
| DR / backup restore | Nightly seven-DB drill (W102) + Render staging drill (W103) |
| Browser E2E | Playwright compose profile (W101) |

## Next slice

**Product-owner SLO adoption** — replace engineering-default thresholds when PO publishes SLO document; extend k6 with cross-product journey scenarios (qualification check, dispatch gate) once SLO targets exist.
