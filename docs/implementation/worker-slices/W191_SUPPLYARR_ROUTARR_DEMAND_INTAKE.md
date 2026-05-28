# Worker 191 — SupplyArr demand intake from RoutArr trips

## Slice name

M10 cross-product workflow — trip parts demand lines in RoutArr, service-token publish to SupplyArr integration ingest, local demand mirror, optional PR draft creation, JWT demand-ref read APIs, cross-product tests.

## Products touched

- **RoutArr API** (`apps/routarr-api`): `routarr_trip_parts_demand_lines`, nested `/api/trips/{id}/parts-demand` + `/publish`, `SupplyArrDemandClient`, EF migration `RoutArrTripPartsDemand`.
- **SupplyArr API** (`apps/supplyarr-api`): `supplyarr_routarr_demand_refs`, `supplyarr_routarr_demand_ref_lines`, `/api/integrations/routarr-demand`, `/api/routarr-demand-refs`, `RoutArrDemandIntakeService`, inbox handler `routarr.demand.ingest`, outbox `routarr.demand.received`, EF migration `SupplyArrRoutArrDemandIntake`.
- **Shared** (`packages/shared-dotnet`): `routarr-supplyarr` service token profile in `StlIntegrationTokenCatalog`.
- **Tests**: `RoutArrSupplyArrPartsDemandTests`.

## Schema

### RoutArr migration: `RoutArrTripPartsDemand`

- `routarr_trip_parts_demand_lines` — tenant-scoped demand lines on trips (`lineNumber`, optional opaque `supplyarrPartId`, `partNumber`, `quantityRequested`, `status` pending/published, publication/demand-ref metadata)

### SupplyArr migration: `SupplyArrRoutArrDemandIntake`

- `supplyarr_routarr_demand_refs` — local mirror keyed by opaque `routarrPublicationId` (unique per tenant), opaque `routarrTripId`, trip number/vehicle ref snapshots, optional `purchaseRequestId`
- `supplyarr_routarr_demand_ref_lines` — mirrored line items with opaque `routarrDemandLineId`, optional local `partId`

No cross-DB FKs — RoutArr owns trips; SupplyArr owns procurement demand truth.

## API + auth changes

### RoutArr user API (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/trips/{tripId}/parts-demand` | Trips read + trip access |
| POST | `/api/trips/{tripId}/parts-demand` | Trips perform + trip access |
| POST | `/api/trips/{tripId}/parts-demand/publish` | Trips perform + trip access |

Publish calls SupplyArr via service token (`routarr` → `supplyarr`, scope `supplyarr.demand_intake.write`).

### SupplyArr integration API (service token)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/integrations/routarr-demand` | Service token: source `routarr`, target `supplyarr`, scope `supplyarr.demand_intake.write` |

Idempotent on `(tenantId, routarrPublicationId)`. Optional `createPurchaseRequestDraft` creates draft PR when catalog-linked parts exist.

Internal inbox enqueue accepts `routarr` source with the same scope; processing handles `routarr.demand.ingest`.

### SupplyArr user API (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/routarr-demand-refs` | Purchase request read scope |
| GET | `/api/routarr-demand-refs/{demandRefId}` | Purchase request read scope |
| POST | `/api/routarr-demand-refs/{demandRefId}/create-purchase-request` | Purchase request create scope |

## Tests

### Cross-product integration (`STLCompliance.RoutArr.Auth.Tests`)

- `Trip_parts_demand_publish_creates_supplyarr_mirror`
- `Routarr_demand_ingest_is_idempotent`
- `Publish_with_pr_draft_creates_purchase_request`
- `Routarr_demand_ingest_rejects_missing_service_token`

## Verification commands

```powershell
dotnet build "apps/routarr-api/RoutArr.Api/RoutArr.Api.csproj" -c Release
dotnet build "apps/supplyarr-api/SupplyArr.Api/SupplyArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj" --filter "FullyQualifiedName~RoutArrSupplyArrPartsDemandTests" --no-build
```

## Out of scope (follow-up)

- SupplyArr demand status callbacks to RoutArr (mirror W85 MaintainArr callbacks)
- SupplyArr `DemandProcessingWorker` auto-PR for RoutArr refs (worker still MaintainArr-only)
- SupplyArr frontend `DemandRefsPanel` RoutArr tab (optional UI slice)
- Event bus-only publish path (inbox enqueue already supported)

## Next slice

Per backlog M10: **TrainArr/StaffArr demand intake** mirror, or remaining M8 procurement automation — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
