# STLCompliance.E2E

M13 end-to-end verification harness for the STL Compliance / Arr suite.

## Modes

| Mode | When | Command |
|------|------|---------|
| **Integration (default)** | CI and local dev without docker-compose | `dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "Category=Integration"` |
| **Live stack** | docker-compose APIs running on host ports | `E2E_LIVE=1 dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "Category=Live"` |
| **All** | Both integration + live (live skips if stack down) | `dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release` |

Integration tests spin up in-memory `WebApplicationFactory` hosts wired together (NexArr + product APIs). They do **not** require Postgres, Redis, or docker-compose.

Live tests probe real `/health` endpoints and optional NexArr demo login. They **skip** (do not fail) when `E2E_LIVE` is unset or services are unreachable.

## Cross-product flows covered

1. **NexArrHandoffFlowTests** — login, `/api/me`, handoff redeem into StaffArr/RoutArr
2. **StaffArrReadinessFlowTests** — baseline certification blockers → ready
3. **StaffArrWorkforceOnboardingFlowTests** — docs/23 new employee → qualified worker with TrainArr history (W16)
4. **TrainArrAssignmentCompleteFlowTests** — incident route → assignment → complete → StaffArr certification/unblock
5. **MaintainArrWorkOrderFlowTests** — handoff → work order create → in_progress → completed
6. **MaintainArrInspectionToWorkOrderFlowTests** — failed inspection → defect → work order → readiness blocked (docs/23)
7. **MaintainArrSupplyArrPartsDemandFlowTests** — work order parts demand → SupplyArr mirror (docs/23)
8. **RoutArrAssetDispatchReadyFlowTests** — MaintainArr ready asset → RoutArr dispatch assign (docs/23)
9. **StaffArrMaintainArrTechnicianSyncFlowTests** — StaffArr person sync → MaintainArr technician ref mirror (docs/23)
10. **RoutArrDispatchAssignFlowTests** — trip → workflow gate block → preview → override assign
11. **TenantIsolationFlowTests** — multi-tenant JWT/service-token denial across NexArr, StaffArr, MaintainArr, RoutArr, TrainArr, Compliance Core, SupplyArr (`Area=TenantIsolation`)
12. **EntitlementDenialFlowTests** — JWT without product entitlement denied on `/api/me`; NexArr launch context denied for unknown product (`Area=EntitlementDenial`)
13. **StlM13ShipGateCatalogTests** / **StlDocs23CrossProductFlowCatalogTests** — ship-gate minimums aligned with shared catalogs (`Area=ShipGate`)
14. **StlE2eFrontendCatalogTests** / **StlE2ePlaywrightSpecCatalogTests** — canonical Vite preview ports (5174–5185) and Playwright spec filenames including platform-admin audit export (`Category=E2e`)

## Playwright browser smokes (`tests/e2e-playwright`)

| Spec | Coverage |
|------|----------|
| `suite-login-handoff-smoke.spec.ts` | Suite login → StaffArr handoff |
| `product-handoff-smoke.spec.ts` | Handoff to all seven product frontends |
| `product-handoff-tenant-chrome.spec.ts` | Tenant name/slug in product shell after handoff |
| `FieldCompanion-field-inbox-trainarr-deep-link.spec.ts` | Field Companion field inbox → TrainArr assignment deep link |
| `product-trainarr-assignment-deep-link.spec.ts` | TrainArr `/assignments/{id}/evidence` route |

Requires `E2E_LIVE=1` and `scripts/ops/e2e-frontends-preview` (suite 5174, products 5175–5180 plus 5182, 5183, and 5185, Field Companion 5181). See `tests/e2e-playwright/README.md` and `docs/implementation/worker-slices/W134_M13_PLAYWRIGHT_DEEP_LINK_E2E.md`.

## Live URL configuration

Defaults match `docker-compose.yml` host port mappings:

| Product | Default URL | Override env |
|---------|-------------|--------------|
| NexArr | `http://localhost:5101` | `E2E_NEXARR_URL` |
| StaffArr | `http://localhost:5102` | `E2E_STAFFARR_URL` |
| TrainArr | `http://localhost:5103` | `E2E_TRAINARR_URL` |
| MaintainArr | `http://localhost:5104` | `E2E_MAINTAINARR_URL` |
| RoutArr | `http://localhost:5105` | `E2E_ROUTARR_URL` |
| SupplyArr | `http://localhost:5106` | `E2E_SUPPLYARR_URL` |
| Compliance Core | `http://localhost:5107` | `E2E_COMPLIANCECORE_URL` |

Enable live mode: `E2E_LIVE=1` (or `true`).

## Local verification

```powershell
# Integration flows only (CI-safe)
dotnet test "tests/STLCompliance.E2E/STLCompliance.E2E.csproj" -c Release --filter "Category=Integration"

# With docker-compose stack up
docker compose up -d postgres nexarr-api staffarr-api trainarr-api maintainarr-api routarr-api supplyarr-api compliancecore-api
$env:E2E_LIVE = "1"
dotnet test "tests/STLCompliance.E2E/STLCompliance.E2E.csproj" -c Release --filter "Category=Live"
```

## Project layout

```
Support/          Shared NexArr host, HTTP helpers, live probes, tenant constants
Flows/            In-memory cross-product journey tests + tenant isolation battery
Live/             Optional docker-compose smoke + tenant isolation live probe
```

See also: `docs/implementation/worker-slices/W91_M13_E2E_VERIFICATION_HARNESS.md`.
