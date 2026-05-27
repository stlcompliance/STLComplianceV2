# Worker 75 — SupplyArr returns

## Slice name

M8 procurement spine — vendor returns from stock or PO line, RMA records, stock decrement on post, APIs, supplyarr-frontend, tests.

## Products touched

- **SupplyArr API** (`apps/supplyarr-api`): `supplyarr_vendor_returns`, `supplyarr_vendor_return_lines`, `VendorReturnService`, `PartStockService.DecrementOnHandAsync`, CRUD/post/cancel endpoints, audit events.
- **SupplyArr Frontend** (`apps/supplyarr-frontend`): `ReturnsPanel`, API client methods, `HomePage` integration.

## Schema

Migration: `SupplyArrVendorReturns`

Added SupplyArr tables:

- `supplyarr_vendor_returns` — tenant-scoped return header (`status` draft/posted/cancelled, `sourceType` stock | purchase_order_line, vendor party, optional PO link, inventory bin, RMA number, notes, post/cancel metadata)
- `supplyarr_vendor_return_lines` — line items with part, optional PO line link, quantity, notes

Indexes: unique `(tenantId, returnKey)`, `(tenantId, vendorPartyId)`, `(tenantId, purchaseOrderId)`, `(tenantId, status, updatedAt)`, line unique `(tenantId, vendorReturnId, lineNumber)`.

## API + auth changes

### SupplyArr API endpoints

- `GET /api/returns` — list with optional `status`, `vendorPartyId`, `purchaseOrderId`, `partId`
- `GET /api/returns/{returnId}` — detail
- `POST /api/returns/from-stock` — draft return from on-hand stock (multi-line)
- `POST /api/returns/from-purchase-order-line/{purchaseOrderLineId}` — draft return against issued PO line with received qty
- `POST /api/returns/{returnId}/post` — post return and decrement stock per line
- `POST /api/returns/{returnId}/cancel` — cancel draft with reason

### Authorization

- read: `RequireReturnRead` (same roles as purchase order read — includes buyers)
- manage: `RequireReturnManage` (same as receiving perform — clerk/manager/admin)

## Frontend changes

- `ReturnsPanel` — list/filter, stock vs PO line create, RMA number, post/cancel draft actions
- API client: `getVendorReturns`, `createVendorReturnFromStock`, `createVendorReturnFromPurchaseOrderLine`, `postVendorReturn`, `cancelVendorReturn`
- `HomePage` — query, mutations, invalidation on post (stock levels)

## Tests

### Backend integration (`tests/STLCompliance.SupplyArr.Auth.Tests`)

- `Return_from_po_line_post_decrements_stock`
- `Return_from_stock_post_decrements_stock`
- `Return_draft_cancel`

### Frontend unit

- `ReturnsPanel.test.tsx` — draft return with RMA and PR/PO linkage
- `client.test.ts` — vendor returns list parses `rmaNumber`

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

- Credit memo / vendor refund accounting integration
- Partial multi-bin returns on one header
- Event publish `supplyarr.return.posted` (integration slice)
- Auto-adjust PO received quantity on return post
- Barcode scan at return ship

## Next slice (Worker 76)

Recommended: **RoutArr equipment availability panel** or **SupplyArr pricing/lead-time snapshots** per M8/M9 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
