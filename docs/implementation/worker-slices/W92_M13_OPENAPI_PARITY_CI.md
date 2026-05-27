# Worker 92 — M13 OpenAPI parity CI gate

## Slice name

M13 OpenAPI parity CI — checked-in OpenAPI snapshots for all seven product APIs with automated drift detection in CI.

## Products touched

- **packages/shared-dotnet/STLCompliance.Shared** — expose `/openapi/v1.json` in Testing environment (in addition to Development)
- **tests/STLCompliance.OpenApi.Tests** — new test project with snapshot parity + route coverage checks
- **tests/STLCompliance.OpenApi.Tests/snapshots/** — baseline OpenAPI documents (7 files)
- **STLCompliance.slnx** — project registration
- **.github/workflows/ci.yml** — explicit OpenAPI parity step
- **docs/implementation/worker-slices/00_SLICE_STATE.md** — Worker 92 completion

## Design

### Snapshot parity

Each API boots via `WebApplicationFactory` in the **Testing** environment (no Postgres). Tests fetch `/openapi/v1.json`, normalize JSON (strip `info.version`, sort paths), and compare to `snapshots/{productKey}.openapi.json`.

Regenerate after intentional API changes:

```powershell
$env:OPENAPI_UPDATE_SNAPSHOTS = "1"
dotnet test "tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj" -c Release --filter "Category=OpenApi"
```

### Why Testing environment

`StlApiHost` previously mapped OpenAPI only in Development. Parity tests need OpenAPI without triggering EF migrations or requiring a database connection. Enabling `MapOpenApi()` for `Testing` keeps Production unchanged.

### CI integration

- Dedicated workflow step: `OpenAPI parity`
- Also included in the main `dotnet test` run (`Category!=Live` filter)

## Verification commands

```powershell
dotnet build "STLCompliance.slnx" -c Release
dotnet test "tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj" -c Release --filter "Category=OpenApi"
dotnet test "STLCompliance.slnx" -c Release --filter "Category!=Live"
```

## Test results (Worker 92)

| Suite | Result |
|-------|--------|
| OpenAPI parity (14 tests) | Pass |
| Full CI filter `Category!=Live` | Pass except 1 pre-existing NexArr admin test failure (`Platform_admin_can_create_product` — Conflict vs Created; unrelated to this slice) |

## Gap analysis update (M13)

| Area | Status after W92 |
|------|------------------|
| **OpenAPI parity** | **Addressed** — snapshot gate in CI for all 7 APIs |
| Load / performance | Still open — needs SLO targets |
| Browser E2E (Playwright) | Still open — needs stable frontend URLs + docker seed |
| Observability validation | Still open |
| Recovery / DR | Still open |
| Tenant isolation soak | Still open |

## Next slice (Worker 93)

Recommended: **Playwright browser smoke** for suite-frontend NexArr handoff (if docker-compose seed stabilized), **observability health aggregation endpoint**, or **load-test harness** once SLOs are defined — per W91 gap analysis.
