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
| `platform-admin-audit-export-smoke.spec.ts` | Suite platform-admin audit export manifest/timeline/sync ZIP + background job (W138) |
| `maintainarr-settings-audit-export-smoke.spec.ts` | MaintainArr Settings audit export panel (W230): manifest, filters, ZIP/JSON, background job (W232) |
| `compliancecore-m12-worker-settings-smoke.spec.ts` | Compliance Core Admin M12 analytics worker settings save (W231/W232) |
| `compliancecore-audit-delivery-orchestration-smoke.spec.ts` | Compliance Core Admin audit delivery orchestration panel: status sections + trigger controls visible (W240/W242; no live triggers) |
| `trainarr-assignment-material-demand-smoke.spec.ts` | TrainArr handoff → assignment workspace material demand panel: lines, optional publish, procurement badge/timeline (W233/W234) |
| `routarr-dispatch-command-center-smoke.spec.ts` | RoutArr handoff → `/dispatch` command center panel: daily/weekly scope toggle, status columns or empty state (W209/W235) |
| `routarr-dispatch-exception-queue-smoke.spec.ts` | RoutArr handoff → `/dispatch` exception queue panel: heading, create form when triage allowed, open rows or empty state (W210/W243) |
| `routarr-dispatch-active-trips-smoke.spec.ts` | RoutArr handoff → `/dispatch` active trips panel: list/map toggle, summary tiles, trip rows or empty state (W211/W244) |
| `routarr-dispatch-unassigned-work-queue-smoke.spec.ts` | RoutArr handoff → `/dispatch` unassigned work queue: heading, per-trip assign controls, bulk assign when rows present, or empty state (W212/W245) |
| `routarr-driver-portal-smoke.spec.ts` | RoutArr handoff → `/driver-portal` schedule panel: Today/Upcoming sections, trip cards or empty state (W213/W247; no dispatch/start clicks) |
| `routarr-dispatch-proof-dvir-read-smoke.spec.ts` | RoutArr handoff → `/dispatch` `trip-proof-dvir-read-panel`: trip ID lookup, load execution summary, proof/DVIR rows or empty lists (W217/W248; read-only, no proof/DVIR capture) |
| `supplyarr-settings-integration-events-smoke.spec.ts` | SupplyArr handoff → Settings integration event outbox/inbox save + Readiness dashboard metrics (W236) |
| `supplyarr-reports-workspace-smoke.spec.ts` | SupplyArr handoff → Reports vendor + purchasing panels: filters, summary/empty state, Export CSV present (W237) |
| `staffarr-admin-audit-export-smoke.spec.ts` | StaffArr handoff → Admin audit package panel: manifest, summary, filters, sync ZIP/JSON/CSV, background job (W228/W238) |
| `trainarr-settings-audit-export-smoke.spec.ts` | TrainArr handoff → Settings training audit package panel: manifest, date filters, JSON summary counts, sync + background ZIP (W165/W167/W239) |
| `routarr-reports-audit-export-smoke.spec.ts` | RoutArr handoff → Reports audit package panel: manifest, summary, filters, sync ZIP/JSON/CSV, background job (W227/W241) |
| `companion-field-inbox-operations-deep-links.spec.ts` | Companion → MaintainArr / RoutArr / SupplyArr field inbox deep links (W140) |
| `companion-offline-queue-notification.spec.ts` | Offline acknowledge queue sync + notification/push readiness surfaces (W146) |
| `companion-field-task-evidence.spec.ts` | TrainArr field-inbox photo evidence upload via companion API (W147) |
| `companion-field-scan.spec.ts` | Companion manual scan resolve → field inbox task highlight (M11) |
| `compliancecore-operator-rule-evaluate-smoke.spec.ts` | Compliance Core handoff → rule pack seed + evaluate (operator path) |
| `suite-multi-product-handoff-journey.spec.ts` | Suite session chains StaffArr → TrainArr → Compliance Core handoffs |

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
| `E2E_STAFFARR_API_URL` | `http://localhost:5102` |
| `E2E_COMPLIANCECORE_API_URL` | `http://localhost:5107` |
| `E2E_DEMO_EMAIL` | `admin@demo.stl` |
| `E2E_DEMO_PASSWORD` | `ChangeMe!Demo2026` |
