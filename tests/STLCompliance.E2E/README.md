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
3. **TrainArrAssignmentCompleteFlowTests** — incident route → assignment → complete → StaffArr certification/unblock
4. **MaintainArrWorkOrderFlowTests** — handoff → work order create → in_progress → completed
5. **RoutArrDispatchAssignFlowTests** — trip → workflow gate block → preview → override assign
6. **TenantIsolationFlowTests** — multi-tenant JWT/service-token denial across StaffArr, MaintainArr, RoutArr, TrainArr, Compliance Core, SupplyArr (`Area=TenantIsolation`)

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
