# Playwright shell tenant chrome after handoff

## Slice name

M13 Playwright E2E — assert tenant display name and slug in suite and product workspace shells after NexArr handoff redeem

## Products touched

- **NexArr API** — `TenantDisplayName` on `HandoffRedeemedResponse` and Companion handoff session
- **STLCompliance.Shared** — `StlNexArrHandoffRedeemedResponse` includes tenant display name
- **StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, Compliance Core APIs** — `HandoffSessionResponse.TenantDisplayName`
- **@stl/shared-ui** — `WorkspaceUserChrome`, `tenantSlug` on `ProductWorkspaceSession` / `ProductAppShell`
- **All product frontends + suite-frontend** — persist and render tenant name + slug in shell chrome
- **tests/e2e-playwright** — suite login + six-product handoff tenant chrome specs

## Schema

None.

## API changes

| Area | Change |
|------|--------|
| `POST /api/launch/handoff/redeem` (NexArr) | Response adds `tenantDisplayName` |
| `POST /api/auth/handoff/redeem` (products) | Response adds `tenantDisplayName` |
| `POST /api/companion/auth/handoff/redeem` | Response adds `tenantDisplayName` |

## Playwright specs

| File | Coverage |
|------|----------|
| `suite-login-handoff-smoke.spec.ts` | Suite shell `suite-tenant-display-name`, `suite-tenant-slug` after login |
| `product-handoff-tenant-chrome.spec.ts` | Product shell `workspace-tenant-*` after handoff for all six ARR frontends |

Requires `E2E_LIVE=1` and docker-compose e2e stack (`scripts/ops/e2e-stack-up.ps1`, `e2e-frontends-preview.ps1`).

Demo assertions use `demoTenant` in `liveProbe.ts` (defaults: STL Demo Tenant / demo-stl / Demo Platform Admin).

## Tests (CI / local)

- `TrainingNotificationRulesTests` — N/A
- `NexArrLaunchApiTests` — handoff redeem returns tenant display name
- `ProductWorkspaceFrame.test.tsx`, `ProductWorkspaceLayout.test.tsx`
- Playwright — skipped unless live stack up

## Verification

```powershell
cd packages/shared-ui; npm run test
cd apps/staffarr-frontend; npm run test
dotnet test tests/STLCompliance.NexArr.Auth.Tests --filter NexArrLaunchApiTests
# Live browser (optional):
# $env:E2E_LIVE='1'; cd tests/e2e-playwright; npx playwright test
```

## Next recommended slice

**MaintainArr notification settings foundations** (M12 matrix) or **StaffArr audit package export** per backlog.
