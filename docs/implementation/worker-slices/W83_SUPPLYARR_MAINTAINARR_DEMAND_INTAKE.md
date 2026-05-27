# Worker 83 — SupplyArr demand intake from MaintainArr work orders

## Slice name

M10 cross-product workflow — work order parts demand lines in MaintainArr, service-token publish to SupplyArr integration ingest, local demand mirror, optional PR draft creation, JWT demand-ref read APIs, frontends, cross-product tests.

## Products touched

- **MaintainArr API** (`apps/maintainarr-api`): `maintainarr_work_order_parts_demand_lines`, nested `/api/work-orders/{id}/parts-demand` + `/publish`, `SupplyArrDemandClient`, EF migration `MaintainArrWorkOrderPartsDemand`.
- **SupplyArr API** (`apps/supplyarr-api`): `supplyarr_maintainarr_demand_refs`, `supplyarr_maintainarr_demand_ref_lines`, `/api/integrations/maintainarr-demand`, `/api/demand-refs`, `MaintainArrDemandIntakeService`, EF migration `SupplyArrMaintainArrDemandIntake`.
- **MaintainArr Frontend** (`apps/maintainarr-frontend`): `WorkOrderPartsDemandPanel` embedded in work order detail.
- **SupplyArr Frontend** (`apps/supplyarr-frontend`): `DemandRefsPanel` on home workspace.
- **Tests**: `MaintainArrSupplyArrPartsDemandTests`, frontend unit tests.

## Schema

### MaintainArr migration: `MaintainArrWorkOrderPartsDemand`

- `maintainarr_work_order_parts_demand_lines` — tenant-scoped demand lines on work orders (`lineNumber`, optional opaque `supplyarrPartId`, `partNumber`, `quantityRequested`, `status` pending/published/cancelled, publication/demand-ref metadata)

### SupplyArr migration: `SupplyArrMaintainArrDemandIntake`

- `supplyarr_maintainarr_demand_refs` — local mirror keyed by opaque `maintainarrPublicationId` (unique per tenant), opaque `maintainarrWorkOrderId`, WO number/asset snapshots, optional `purchaseRequestId`
- `supplyarr_maintainarr_demand_ref_lines` — mirrored line items with opaque `maintainarrDemandLineId`, optional local `partId`

No cross-DB FKs — MaintainArr owns work orders; SupplyArr owns procurement demand truth.

## API + auth changes

### MaintainArr user API (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/work-orders/{workOrderId}/parts-demand` | Work orders read + work order access |
| POST | `/api/work-orders/{workOrderId}/parts-demand` | Work orders perform + work order access |
| POST | `/api/work-orders/{workOrderId}/parts-demand/publish` | Work orders perform + work order access |

Publish calls SupplyArr via service token (`maintainarr` → `supplyarr`, scope `supplyarr.demand_intake.write`).

### SupplyArr integration API (service token)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/integrations/maintainarr-demand` | Service token: source `maintainarr`, target `supplyarr`, scope `supplyarr.demand_intake.write` |

Idempotent on `(tenantId, maintainarrPublicationId)`. Optional `createPurchaseRequestDraft` creates draft PR when catalog-linked parts exist.

### SupplyArr user API (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/demand-refs` | Purchase request read scope |
| GET | `/api/demand-refs/{demandRefId}` | Purchase request read scope |
| POST | `/api/demand-refs/{demandRefId}/create-purchase-request` | Purchase request create scope |

## Frontend changes

- `WorkOrderPartsDemandPanel` — add demand lines, publish to SupplyArr, optional PR draft checkbox
- `DemandRefsPanel` — list MaintainArr demand mirrors, manual PR draft from selected ref
- API clients extended on both frontends

## Tests

### Cross-product integration (`STLCompliance.MaintainArr.Auth.Tests`)

- `Work_order_parts_demand_publish_creates_supplyarr_mirror`
- `Maintainarr_demand_ingest_is_idempotent`
- `Publish_with_pr_draft_creates_purchase_request`
- `Maintainarr_demand_ingest_rejects_missing_service_token`

### Frontend unit

- `WorkOrderPartsDemandPanel.test.tsx`
- `DemandRefsPanel.test.tsx`

## Verification commands

```powershell
dotnet build "apps/maintainarr-api/MaintainArr.Api/MaintainArr.Api.csproj" -c Release
dotnet build "apps/supplyarr-api/SupplyArr.Api/SupplyArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~PartsDemand"
cd apps/maintainarr-frontend
npm run test
npm run build
cd ../supplyarr-frontend
npm run test
npm run build
```

## Remaining gaps

- Stock availability check before PR draft
- RoutArr demand intake (M10)
- Event bus publish for demand intake (integration slice)

## Next slice (Worker 84)

Recommended: **RoutArr driver eligibility** or **SupplyArr demand status callbacks to MaintainArr** per M9/M10 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
