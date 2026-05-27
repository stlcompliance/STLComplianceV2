# Worker 67 — SupplyArr receiving foundations

## Slice name

M8 procurement spine — receive against issued PO, post stock to bin, `/api/receiving`, supplyarr-frontend panel, tests.

## Products touched

- **SupplyArr API** (`apps/supplyarr-api`): receiving receipt tables, draft/post workflow, stock increment on post, audit events.
- **SupplyArr Frontend** (`apps/supplyarr-frontend`): `ReceivingPanel` on home workspace with create-from-issued-PO and post flows.

## Schema

Migration: `SupplyArrReceivingFoundations`

Added SupplyArr tables:

- `supplyarr_receiving_receipts` — tenant-scoped receipt header (`receiptKey`, `purchaseOrderId`, `inventoryBinId`, `status`, `notes`, post metadata)
- `supplyarr_receiving_receipt_lines` — line items linked to PO lines (`quantityReceived`, `partId`)

Extended:

- `supplyarr_purchase_order_lines.QuantityReceived` — cumulative received quantity per PO line

Status workflow (this slice):

- `draft` → edit line quantities, post to inventory
- terminal: `posted`

Creation rule: one draft receipt per issued PO; source PO must be `issued` with remaining quantity on at least one line.

## API + auth changes

### SupplyArr API endpoints

- `GET /api/receiving` — list (optional `status`, `purchaseOrderId` filters)
- `GET /api/receiving/{receivingReceiptId}` — detail with lines
- `POST /api/receiving/from-purchase-order/{purchaseOrderId}` — create draft receipt prefilled with remaining PO quantities
- `PUT /api/receiving/{receivingReceiptId}/lines/{lineId}` — update draft line quantity
- `POST /api/receiving/{receivingReceiptId}/post` — post receipt, increment PO line received qty, increment bin stock

### Authorization

`SupplyArrAuthorizationService`:

- read: delegates to inventory read (`RequireReceivingRead`)
- perform: platform admin, tenant admin, supplyarr admin/manager/clerk (`supplyarr.receiving.perform` via `RequireReceivingPerform`)

## Frontend changes

- `ReceivingPanel` — lists receipts, create from issued PO into selected bin, post draft receipts
- API client: `getReceivingReceipts`, `createReceivingReceiptFromPurchaseOrder`, `postReceivingReceipt`
- Role helper: `canPerformReceiving`
- `PurchaseOrderLineResponse` extended with `quantityReceived` / `quantityRemaining`

## Tests

### Backend integration (`tests/STLCompliance.SupplyArr.Auth.Tests`)

- receiving against issued PO + post updates stock and PO line received qty
- create denied for `supplyarr_buyer`

### Frontend unit

- `src/api/client.test.ts` — receiving list success parsing
- `src/components/ReceivingPanel.test.tsx` — list + post action rendering

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

- Partial shipment workflows, barcode scan (later M8 slices)
- Multi-location bin picker without selecting inventory location first in UI
- Event publish `supplyarr.receiving.posted` (integration slice)
- MaintainArr received-part status display (M7 integration)

## Next slice (Worker 68+)

Receiving exceptions delivered in Worker 68 — see `W68_SUPPLYARR_RECEIVING_EXCEPTIONS.md`. Next: **RoutArr trip/dispatch foundations** or **SupplyArr backorders** per M8/M9 priority — see `00_SLICE_STATE.md`.
