# Worker 73 — SupplyArr backorders

## Slice name

M8 procurement spine — backorder records when PO/receipt short, APIs linked to PR/PO lines, supplyarr-frontend, tests.

## Products touched

- **SupplyArr API** (`apps/supplyarr-api`): `supplyarr_backorders`, `BackorderService`, sync on receiving post, CRUD/fulfill/cancel endpoints, audit events.
- **SupplyArr Frontend** (`apps/supplyarr-frontend`): `BackordersPanel`, API client methods, `HomePage` integration.

## Schema

Migration: `SupplyArrBackorders`

Added SupplyArr table:

- `supplyarr_backorders` — tenant-scoped backorder per open PO shortfall (`status` open/fulfilled/cancelled, `sourceType` receipt_post | purchase_order_line, quantities, PR/PO/PO-line/receipt links, expected date, notes, fulfill/cancel metadata)

Indexes: unique `(tenantId, backorderKey)`, `(tenantId, purchaseOrderLineId, status)`, `(tenantId, partId, status)`.

## API + auth changes

### SupplyArr API endpoints

- `GET /api/backorders` — list with optional `status`, `purchaseOrderId`, `partId`
- `GET /api/backorders/{backorderId}` — detail
- `POST /api/backorders/from-purchase-order-line/{purchaseOrderLineId}` — manual backorder on issued PO line with remaining qty
- `POST /api/backorders/{backorderId}/fulfill` — clerk acknowledgment / manual close
- `POST /api/backorders/{backorderId}/cancel` — cancel with reason

Receiving post (`POST /api/receiving/{id}/post`) auto-syncs open backorders from remaining PO line quantity (creates, updates qty, or fulfills when fully received).

### Authorization

- read: `RequireBackorderRead` (same roles as purchase order read — includes buyers)
- manage: `RequireBackorderManage` (same as receiving perform — clerk/manager/admin)

## Frontend changes

- `BackordersPanel` — list/filter, PR/PO linkage display, manual create from PO line, fulfill/cancel actions
- API client: `getBackorders`, `createBackorderFromPurchaseOrderLine`, `fulfillBackorder`, `cancelBackorder`
- `HomePage` — query, mutations, invalidation on receiving post

## Tests

### Backend integration (`tests/STLCompliance.SupplyArr.Auth.Tests`)

- `Receiving_short_shipment_creates_open_backorder`
- `Backorder_fulfilled_when_po_line_fully_received`
- `Backorder_manual_create_and_cancel`

### Frontend unit

- `BackordersPanel.test.tsx` — open backorder with PR/PO linkage
- `client.test.ts` — backorders list parses `purchaseRequestKey`

## Verification commands

```powershell
dotnet build "apps/supplyarr-api/SupplyArr.Api/SupplyArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj" -c Release
cd apps/supplyarr-frontend
npm install
npm run test
npm run build
```

## Remaining gaps

- Partial shipment / multi-receipt PO closure rules (beyond backorder qty sync)
- Barcode scan at receive
- Event publish `supplyarr.backorder.opened` / `supplyarr.backorder.fulfilled` (integration slice)
- MaintainArr received-part status display (M7 integration)
- Supplier ETA / vendor communication workflow

## Next slice (Worker 74)

Recommended: **RoutArr driver availability panel** or **SupplyArr returns** per M8/M9 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
