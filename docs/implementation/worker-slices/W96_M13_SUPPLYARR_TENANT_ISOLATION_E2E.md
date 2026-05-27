# M13 SupplyArr tenant isolation E2E battery

## Slice name

M13 SupplyArr tenant isolation — cross-tenant JWT and MaintainArr→SupplyArr service-token denial in the E2E battery; live-stack SupplyArr probe.

## Products touched

- **tests/STLCompliance.E2E** — SupplyArr integration + live tenant isolation tests
- **docs/implementation/worker-slices/00_SLICE_STATE.md**, **docs/implementation-status.md**, **tests/STLCompliance.E2E/README.md**

## Integration battery additions (3 tests)

| Test | Assertion |
|------|-----------|
| SupplyArr cross-tenant GET vendor | 404 |
| SupplyArr list scoping | Tenant B vendors excluded from Tenant A list |
| SupplyArr service token tenant mismatch | 403 on MaintainArr→SupplyArr demand ingest |

Traits: `Category=Integration`, `Area=TenantIsolation`

Total tenant isolation integration tests: **10** (7 prior + 3 SupplyArr).

## Live probe addition (1 test)

`TenantIsolationLiveTests.Live_SupplyArr_cross_tenant_vendor_get_returns_not_found` — docker-compose SupplyArr + NexArr; mints Tenant B JWT with dev signing key.

Trait: `Category=Live`, `Area=TenantIsolation`

## Support changes

- `E2EAccessTokenHelper.SupplyArr` — mint SupplyArr JWTs via `SupplyArrTokenService`
- `STLCompliance.E2E.csproj` — project reference to `SupplyArr.Api`
- Tenant B entitlements extended with `supplyarr` in isolation fixture setup

## Verification commands

```powershell
dotnet build "STLCompliance.slnx" -c Release
dotnet test "STLCompliance.slnx" -c Release --filter "Category!=Live"
dotnet test "tests/STLCompliance.E2E/STLCompliance.E2E.csproj" -c Release --filter "Area=TenantIsolation&Category=Integration"
```

## Gap analysis update (M13)

| Area | Status after this slice |
|------|-------------------------|
| Tenant isolation soak | **Complete** — all 7 product APIs covered in integration battery |
| SupplyArr isolation parity | **Complete** |
| Load / performance | Still blocked — needs SLO definitions |
| DR / backup restore | Still open |
| OTEL / metrics dashboards | Still open |

## Next slice

Load-test harness once product owners publish SLO targets; DR restore drill script; OTEL smoke checks.
