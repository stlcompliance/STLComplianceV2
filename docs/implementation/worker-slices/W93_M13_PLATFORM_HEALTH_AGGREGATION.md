# Worker 93 — M13 platform health aggregation

## Slice name

M13 observability hardening — NexArr `GET /api/platform/health` aggregates downstream product `/health/ready` probes.

## Products touched

- **nexarr-api** — `PlatformHealthService`, `PlatformProductUrlsOptions`, `GET /api/platform/health`
- **tests/STLCompliance.NexArr.Auth.Tests** — unit + API tests; admin product-create test fix
- **tests/STLCompliance.OpenApi.Tests/snapshots/nexarr.openapi.json** — new route in snapshot
- **docs/implementation/worker-slices/00_SLICE_STATE.md** — Worker 93 completion

## Design

### Endpoint

`GET /api/platform/health` (anonymous)

- Probes six product APIs in parallel: StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, Compliance Core
- Each probe calls `{BaseUrl}/health/ready` and parses shared `HealthResponse` JSON
- Per-product status: `Healthy`, `Degraded`, `Unhealthy`, `Unreachable`, `NotConfigured`
- Aggregate status: `Healthy` (all configured healthy), `Degraded` (mixed), `Unhealthy` (all configured down)
- HTTP `503` when aggregate is `Unhealthy`; `200` otherwise

### Configuration

Uses existing internal URL env vars (`stl-internal-api-urls` on Render):

| Variable | Product |
|----------|---------|
| `StaffArr__BaseUrl` | staffarr |
| `TrainArr__BaseUrl` | trainarr |
| `MaintainArr__BaseUrl` | maintainarr |
| `RoutArr__BaseUrl` | routarr |
| `SupplyArr__BaseUrl` | supplyarr |
| `ComplianceCore__BaseUrl` | compliancecore |

Local defaults in `appsettings.json` (`5102`–`5107`).

### Admin test fix

`Platform_admin_can_create_product` now creates `audit-portal` instead of `companion` (already seeded by `PlatformSeeder`), eliminating the pre-existing `409 Conflict` failure.

## Verification commands

```powershell
dotnet build "STLCompliance.slnx" -c Release
dotnet test "tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj" -c Release
dotnet test "tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj" -c Release --filter "Category=OpenApi"
```

## Test results (Worker 93)

| Suite | Result |
|-------|--------|
| NexArr auth + platform health (45 tests) | Pass |
| OpenAPI parity (14 tests) | Pass |

## Gap analysis update (M13)

| Area | Status after W93 |
|------|------------------|
| **Observability validation** | **Partial** — platform health aggregation endpoint + tests; no metrics/tracing dashboards yet |
| Browser E2E (Playwright) | Still open — needs stable frontend URLs + docker seed |
| Load / performance | Still open — needs SLO targets |
| Recovery / DR | Still open |
| Tenant isolation soak | Still open |

## Next slice (Worker 94)

Recommended: **Playwright browser smoke** for suite-frontend NexArr handoff (skip when stack down), **load-test harness** once SLOs exist, or **FINAL_IMPLEMENTATION_REPORT.md** if only blocked ship-gate items remain.
