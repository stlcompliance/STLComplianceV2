# Browser E2E (Playwright)

Optional smoke tests for **suite-frontend**: NexArr login → unified dashboard → product launch surfaces → handoff redirect to each Arr product frontend (5175–5180).

## Quick start (host Vite previews — recommended for CI)

```powershell
# APIs
./scripts/ops/e2e-stack-up.ps1

# Suite + six product frontends (5174–5180) and Companion (5181)
./scripts/ops/e2e-frontends-preview.ps1

cd tests/e2e-playwright
npm install
npx playwright install chromium
$env:E2E_LIVE = "1"
npm test
```

## Full docker-compose e2e profile

Builds and serves all frontends in containers (slower, self-contained):

```powershell
./scripts/ops/e2e-stack-up.ps1 -BuildFrontends
$env:E2E_LIVE = "1"
cd tests/e2e-playwright; npm test
```

```bash
./scripts/ops/e2e-stack-up.sh --build-frontends
export E2E_LIVE=1
cd tests/e2e-playwright && npm test
```

Compose files: `docker-compose.yml` + `docker-compose.e2e.yml` with profile `e2e`.

## Specs

| Test file | Coverage |
|-----------|----------|
| `suite-login-handoff-smoke.spec.ts` | Login, StaffArr launch surface, StaffArr handoff redirect |
| `product-handoff-smoke.spec.ts` | Handoff redirect for all six product frontends |
| `companion-field-inbox-trainarr-deep-link.spec.ts` | Companion field inbox → TrainArr assignment deep link (W133) |
| `product-trainarr-assignment-deep-link.spec.ts` | TrainArr `/assignments/{id}/evidence` route smoke |

Catalog: `StlE2ePlaywrightSpecCatalog` + `StlE2eFrontendCatalog.CompanionFrontend` in shared .NET (`Category=E2e` tests).

## Skip behavior

- `E2E_LIVE` not `1`/`true` → tests **skipped** (CI-safe)
- Suite (`5174`) or NexArr (`5101`) unreachable → **skipped**
- Individual product frontend unreachable → that product test **skipped**
- Default `npm test` without live stack: all tests skipped, exit 0

## Environment

| Variable | Default |
|----------|---------|
| `E2E_LIVE` | unset — tests skipped |
| `E2E_SUITE_URL` | `http://localhost:5174` |
| `E2E_NEXARR_URL` | `http://localhost:5101` |
| `E2E_STAFFARR_URL` | `http://localhost:5175` (frontend preview) |
| `E2E_TRAINARR_URL` | `http://localhost:5176` |
| `E2E_COMPLIANCECORE_URL` | `http://localhost:5177` |
| `E2E_MAINTAINARR_URL` | `http://localhost:5178` |
| `E2E_SUPPLYARR_URL` | `http://localhost:5179` |
| `E2E_ROUTARR_URL` | `http://localhost:5180` |
| `E2E_COMPANION_URL` | `http://localhost:5181` |
| `E2E_TRAINARR_API_URL` | `http://localhost:5103` |
| `E2E_DEMO_EMAIL` | `admin@demo.stl` |
| `E2E_DEMO_PASSWORD` | `ChangeMe!Demo2026` |
