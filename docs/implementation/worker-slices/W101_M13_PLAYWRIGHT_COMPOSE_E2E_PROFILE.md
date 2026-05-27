# Worker 101 — M13 Playwright compose e2e profile (all product frontends)

## Slice name

M13 Playwright compose e2e profile — docker-compose `e2e` profile for seven Vite previews, host preview scripts, six-product handoff smoke specs, frontend catalog, nightly CI wiring.

## Products touched

- **STLCompliance.Shared** — `StlE2eFrontendCatalog`
- **docker** — `Dockerfile.frontend-e2e`, `docker-compose.e2e.yml`
- **scripts/ops** — `e2e-stack-up.*`, `e2e-frontends-preview.*`
- **apps/*-frontend** — `preview.proxy` on all seven Vite configs
- **apps/nexarr-api** — TrainArr launch `BaseUrl` → `http://localhost:5176`
- **tests/e2e-playwright** — `product-handoff-smoke.spec.ts`, shared `signInFromSuite`, per-product skip probes
- **tests/STLCompliance.E2E** — `StlE2eFrontendCatalogTests` (`Category=E2e`)
- **CI** — `.github/workflows/e2e-nightly.yml` playwright job uses full frontend preview script

## Shared additions

| File | Purpose |
|------|---------|
| `Operations/StlE2eFrontendCatalog.cs` | Canonical suite + six handoff frontend ports/URLs |

## Operator workflow

```powershell
# APIs only (fast) + host previews (CI default)
./scripts/ops/e2e-stack-up.ps1
./scripts/ops/e2e-frontends-preview.ps1
$env:E2E_LIVE = "1"
cd tests/e2e-playwright; npm test

# Full containerized frontends (local self-contained)
./scripts/ops/e2e-stack-up.ps1 -BuildFrontends
$env:E2E_LIVE = "1"
cd tests/e2e-playwright; npm test
```

## Verification commands

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test STLCompliance.slnx -c Release --filter "Category!=Live"
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "Category=E2e"

cd tests/e2e-playwright
npm ci
npx playwright install chromium
npm test                    # skipped without E2E_LIVE
```

## Gap analysis update (M13)

| Area | Status after this slice |
|------|-------------------------|
| **Browser E2E** | **Expanded** — compose e2e profile + all six product handoff smokes; per-frontend skip when preview down |
| **Playwright full pass** | Requires live stack (`E2E_LIVE=1`); nightly runs all previews |
| Load / performance | Still blocked — PO SLO document |
| DR verification | Scripted drill (W99); seven-DB nightly drill still open |

## Next slice (Worker 102)

- **Full seven-database DR nightly drill** (extend `DrRestoreDrillLiveTests` + nightly job), or
- **Product-owner SLO adoption** once SLO document is published (replace engineering defaults in W100 harness).
