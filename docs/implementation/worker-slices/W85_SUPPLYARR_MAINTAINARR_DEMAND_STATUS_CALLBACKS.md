# Worker 85 — SupplyArr demand status callbacks to MaintainArr

## Slice name

M10 cross-product workflow — when PR/PO/receiving progresses in SupplyArr, notify MaintainArr work-order parts demand mirror via service-token integration API; procurement status on demand lines, status event audit, frontends, cross-product tests.

## Products touched

- **MaintainArr API** (`apps/maintainarr-api`): extended `maintainarr_work_order_parts_demand_lines` with procurement fields, new `maintainarr_work_order_parts_demand_status_events`, `WorkOrderPartsDemandStatusIngestionService`, `POST /api/integrations/supplyarr-demand-status`, EF migration `MaintainArrWorkOrderPartsDemandStatus`.
- **SupplyArr API** (`apps/supplyarr-api`): extended `supplyarr_maintainarr_demand_refs` with procurement tracking, `MaintainArrDemandStatusCallbackService`, `MaintainArrDemandStatusClient`, hooks on PR/PO/receiving lifecycle, EF migration `SupplyArrMaintainArrDemandStatusCallbacks`.
- **MaintainArr Frontend** (`apps/maintainarr-frontend`): procurement status badges on `WorkOrderPartsDemandPanel`.
- **SupplyArr Frontend** (`apps/supplyarr-frontend`): procurement status column on `DemandRefsPanel`.
- **Tests**: extended `MaintainArrSupplyArrPartsDemandTests`, frontend unit tests.

## Schema

### MaintainArr migration: `MaintainArrWorkOrderPartsDemandStatus`

- Extended `maintainarr_work_order_parts_demand_lines` — `procurementStatus`, `supplyarrPurchaseRequestId`, `supplyarrPurchaseOrderId`, `quantityReceived`, `procurementStatusMessage`, `lastProcurementStatusAt`
- `maintainarr_work_order_parts_demand_status_events` — idempotent callback audit keyed by `supplyarrCallbackPublicationId` (unique per tenant)

### SupplyArr migration: `SupplyArrMaintainArrDemandStatusCallbacks`

- Extended `supplyarr_maintainarr_demand_refs` — `procurementStatus`, `purchaseOrderId`, `lastStatusCallbackAt`

## API + auth changes

### MaintainArr integration API (service token)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/integrations/supplyarr-demand-status` | Service token: source `supplyarr`, target `maintainarr`, scope `maintainarr.demand_status.write` |

Idempotent on `(tenantId, supplyarrCallbackPublicationId)`. Updates all demand lines sharing `maintainarrPublicationId`.

### SupplyArr lifecycle hooks (internal)

Callbacks fire after successful:

- PR draft from demand ref (`pr_drafted`)
- PR submit / approve / reject
- PO create from PR (`po_created`)
- PO issue (`po_issued`)
- Receiving receipt post (`receiving_posted` or `receiving_complete` when PO fully received)

### Configuration

`apps/supplyarr-api/SupplyArr.Api/appsettings.json`:

```json
"MaintainArr": {
  "BaseUrl": "http://localhost:5104",
  "ServiceToken": ""
}
```

## Frontend changes

- `WorkOrderPartsDemandPanel` — procurement status badge, received qty, status message on published lines
- `DemandRefsPanel` — procurement status column alongside intake status

## Tests

### Cross-product integration (`STLCompliance.MaintainArr.Auth.Tests`)

- `Pr_submit_updates_maintainarr_procurement_status`
- `Supplyarr_demand_status_callback_is_idempotent`
- `Supplyarr_demand_status_callback_rejects_missing_service_token`
- Existing W83 tests updated for bidirectional service-token wiring

### Frontend unit

- `WorkOrderPartsDemandPanel.test.tsx`
- `DemandRefsPanel.test.tsx` — updated mock shape

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

- RoutArr asset dispatchability checks (MaintainArr readiness gate)
- Stock availability check before PR draft
- Event bus publish for demand status (integration slice)
- Compliance Core purchase evidence evaluation

## Next slice (Worker 86)

Recommended: **RoutArr asset dispatchability checks** or **Compliance Core dispatch workflow gates** per M10 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
