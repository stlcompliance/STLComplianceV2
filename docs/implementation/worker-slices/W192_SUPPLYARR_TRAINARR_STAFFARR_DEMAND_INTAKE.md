# Worker 192 — SupplyArr demand intake from TrainArr and StaffArr

## Slice name

M10 cross-product workflow — training assignment material demand (TrainArr) and incident supply demand (StaffArr), service-token publish to SupplyArr integration ingest, local demand mirrors, optional PR draft creation, JWT demand-ref read APIs, cross-product tests.

## Products touched

- **TrainArr API**: `trainarr_training_assignment_material_demand_lines`, `/api/training-assignments/{id}/material-demand` + `/publish`, `SupplyArrDemandClient`, migration `TrainArrAssignmentMaterialDemand`.
- **StaffArr API**: `staffarr_incident_supply_demand_lines`, `/api/incidents/{id}/supply-demand` + `/publish`, `SupplyArrDemandClient`, migration `StaffArrIncidentSupplyDemand`.
- **SupplyArr API**: `supplyarr_trainarr_demand_refs`, `supplyarr_staffarr_demand_refs` (+ lines), `/api/integrations/trainarr-demand`, `/api/integrations/staffarr-demand`, `/api/trainarr-demand-refs`, `/api/staffarr-demand-refs`, inbox `trainarr.demand.ingest` / `staffarr.demand.ingest`, outbox received events, migration `SupplyArrTrainStaffDemandIntake`.
- **Shared**: `trainarr-supplyarr`, `staffarr-supplyarr` token profiles in `StlIntegrationTokenCatalog`.
- **Tests**: `TrainArrSupplyArrMaterialDemandTests`, `StaffArrSupplyArrSupplyDemandTests`.

## Schema

No cross-DB FKs. Opaque source IDs and idempotent `*PublicationId` keys per source product.

## API summary

| Source | Publisher routes | SupplyArr integration | SupplyArr JWT refs |
|--------|------------------|----------------------|-------------------|
| TrainArr | `GET/POST /api/training-assignments/{id}/material-demand`, `POST .../publish` | `POST /api/integrations/trainarr-demand` | `GET /api/trainarr-demand-refs` |
| StaffArr | `GET/POST /api/incidents/{id}/supply-demand`, `POST .../publish` | `POST /api/integrations/staffarr-demand` | `GET /api/staffarr-demand-refs` |

Service token scope (both): `supplyarr.demand_intake.write` with source `trainarr` or `staffarr`.

## Tests

- TrainArr: publish mirror, idempotent ingest, missing token → 401
- StaffArr: publish mirror, idempotent ingest, missing token → 401

## Verification

```powershell
dotnet build "apps/supplyarr-api/SupplyArr.Api/SupplyArr.Api.csproj" -c Release
dotnet build "apps/trainarr-api/TrainArr.Api/TrainArr.Api.csproj" -c Release
dotnet build "apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" --filter "FullyQualifiedName~SupplyArr" -c Release
```

## Next slice

Per backlog: **SupplyArr demand status callbacks** to TrainArr/StaffArr (mirror W85), **DemandProcessingWorker** RoutArr/TrainArr/StaffArr auto-PR, or **M8** procurement automation depth.
