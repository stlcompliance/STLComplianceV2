# Shared NexArr handoff client deduplication

## Slice name

Shared NexArr handoff client dedup — consolidate duplicated per-product NexArr redeem HTTP clients into `STLCompliance.Shared/Integration`.

## Products touched

- **STLCompliance.Shared** — `StlNexArrHandoffClient`, contracts, DI extension
- **ComplianceCore, MaintainArr, RoutArr, StaffArr, SupplyArr, TrainArr** — remove duplicate clients; register shared client
- **Auth test projects + E2E** — update test host wiring to `StlNexArrHandoffClient`

## Shared additions

| File | Purpose |
|------|---------|
| `Integration/StlNexArrHandoffClient.cs` | POST `/api/launch/handoff/redeem` with service token |
| `Integration/StlNexArrHandoffContracts.cs` | `StlNexArrRedeemHandoffRequest`, `StlNexArrHandoffRedeemedResponse` |
| `Integration/StlNexArrHandoffServiceCollectionExtensions.cs` | `AddStlNexArrHandoffClient` (`NexArr:BaseUrl` + typed `HttpClient`) |

Product `HandoffAuthService` classes retain product-specific entitlement and JWT minting; shared client handles only NexArr redeem transport.

## Removed per product (×6)

- `Services/NexArrHandoffClient.cs`
- `Contracts/NexArrLaunchContracts.cs`

## Boundary compliance

Shared library provides transport contracts and HTTP helper only. Product authority (entitlement checks, product key validation, token issuance) remains in each product API.

## Verification commands

```powershell
dotnet build "STLCompliance.slnx" -c Release
dotnet test "STLCompliance.slnx" -c Release --filter "Category!=Live"
```

Handoff-specific coverage retained via existing `*HandoffApiTests` and `NexArrHandoffFlowTests`.

## Gap analysis update (M13)

| Area | Status after this slice |
|------|-------------------------|
| Handoff client duplication | **Complete** — single shared implementation |
| Load / performance | Still blocked — needs SLO definitions |
| DR / backup restore | Still open |
| OTEL / metrics dashboards | Still open |

## Next slice

Load-test harness once product owners publish SLO targets; DR restore drill script; OTEL smoke checks.
