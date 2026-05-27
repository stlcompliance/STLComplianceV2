# M13 multi-tenant isolation E2E battery + nightly live CI

## Slice name

M13 tenant isolation soak — cross-tenant JWT and service-token denial battery across product APIs; live-stack probe; scheduled E2E nightly workflow.

## Products touched

- **tests/STLCompliance.E2E** — integration + live tenant isolation tests
- **tests/STLCompliance.NexArr.Auth.Tests** — companion inbox assertion aligned with upstream_401 when product APIs are not wired
- **tests/STLCompliance.OpenApi.Tests** — NexArr snapshot refresh for `/api/internal/integration-tokens`
- **.github/workflows/e2e-nightly.yml** — scheduled live API E2E + Playwright smoke
- **docs/implementation/worker-slices/00_SLICE_STATE.md**, **docs/implementation-status.md**, **tests/STLCompliance.E2E/README.md**

## Integration battery (7 tests)

| Test | Assertion |
|------|-----------|
| StaffArr cross-tenant GET person | 404 |
| StaffArr list scoping | Tenant B people excluded from Tenant A list |
| StaffArr service token tenant mismatch | 403 on TrainArr→StaffArr blocker ingest |
| MaintainArr cross-tenant GET asset | 404 |
| RoutArr cross-tenant GET trip | 404 |
| TrainArr cross-tenant GET assignment | 404 |
| Compliance Core vocabulary list scoping | Tenant A terms excluded from Tenant B list |

Traits: `Category=Integration`, `Area=TenantIsolation`

## Live probe (1 test)

`TenantIsolationLiveTests.Live_StaffArr_cross_tenant_person_get_returns_not_found` — docker-compose StaffArr + NexArr; mints Tenant B JWT with dev signing key (`E2E_DEV_SIGNING_KEY` override).

Trait: `Category=Live`, `Area=TenantIsolation`

## Nightly workflow

`.github/workflows/e2e-nightly.yml`:

- **live-api-e2e** — docker-compose all APIs, `E2E_LIVE=1`, `Category=Live` dotnet tests
- **playwright-smoke** — suite + staffarr preview with `VITE_NEXARR_API_URL`, Playwright handoff smoke

Schedule: daily 06:00 UTC; also `workflow_dispatch`.

## Verification commands

```powershell
dotnet build "STLCompliance.slnx" -c Release
dotnet test "STLCompliance.slnx" -c Release --filter "Category!=Live"
dotnet test "tests/STLCompliance.E2E/STLCompliance.E2E.csproj" -c Release --filter "Area=TenantIsolation&Category=Integration"
```

## Gap analysis update (M13)

| Area | Status after this slice |
|------|-------------------------|
| Tenant isolation soak | **Complete** — integration battery + live StaffArr probe |
| Nightly live E2E CI | **Complete** — workflow added |
| Load / performance | Still blocked — needs SLO definitions |
| DR / backup restore | Still open |
| OTEL / metrics dashboards | Still open |

## Next slice

Load-test harness once product owners publish SLO targets; DR restore drill script; SupplyArr tenant isolation parity in E2E battery.
