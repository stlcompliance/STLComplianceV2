# Worker 134 — M13 Playwright deep-link E2E harness (companion + TrainArr)

## Scope

Extends the Playwright compose E2E profile (W90/W101/W133) with **field-inbox deep-link smokes**:

- **Companion → TrainArr** — suite login, companion handoff, field inbox “Open in TrainArr” navigates to `/assignments/{id}` workspace
- **TrainArr product** — handoff then direct navigation to `/assignments/{id}/evidence` (W133 routes)
- **Catalog** — `StlE2ePlaywrightSpecCatalog`, companion port `5181` on `StlE2eFrontendCatalog`
- **Fixture API** — `tests/e2e-playwright/support/e2eApi.ts` seeds journey definition via `POST /api/load-test-journey/seed`, then creates an **active** `manual` assignment (journey seed alone completes the assignment and does not appear in field inbox)
- **Preview scripts** — `companion-frontend` on `5181` with `VITE_*_FRONTEND_BASE` build args for deep links
- **TrainArr API** — `TrainArr__FrontendBaseUrl` in docker-compose for API-composed `deepLinkUrl` in field inbox

## Specs (skip unless `E2E_LIVE=1`)

| File | Coverage |
|------|----------|
| `companion-field-inbox-trainarr-deep-link.spec.ts` | Companion inbox → TrainArr assignment deep link |
| `product-trainarr-assignment-deep-link.spec.ts` | TrainArr `/assignments/{id}/evidence` route after handoff |

All existing handoff smokes retain prior skip semantics (suite/NexArr down, per-frontend unreachable).

## Verification

```powershell
./scripts/ops/e2e-stack-up.ps1
./scripts/ops/e2e-frontends-preview.ps1
$env:E2E_LIVE = "1"
cd tests/e2e-playwright
npm ci
npx playwright install chromium
npm test
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "Category=E2e"
```

Without `E2E_LIVE`: all Playwright specs skipped (exit 0).

## Out of scope

- Full Playwright coverage for MaintainArr/RoutArr/SupplyArr product deep-link routes (no product SPA routes yet)
- Companion offline queue / push notifications
