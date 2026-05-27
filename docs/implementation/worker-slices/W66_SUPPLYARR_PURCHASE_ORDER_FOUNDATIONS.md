# Worker 66 — SupplyArr purchase order foundations

## Slice name

M8 procurement spine — PO from approved PR, vendor link, line items, draft/approve/issue status workflow, `/api/purchase-orders`, supplyarr-frontend panel, tests.

## Products touched

- **SupplyArr API** (`apps/supplyarr-api`): purchase order domain tables, CRUD/workflow endpoints, audit events.
- **SupplyArr Frontend** (`apps/supplyarr-frontend`): `PurchaseOrderPanel` on home workspace with create-from-PR, approve, and issue flows.

## Schema

Migration: `SupplyArrPurchaseOrderFoundations`

Added SupplyArr tables:

- `supplyarr_purchase_orders` — tenant-scoped PO header (`orderKey`, `title`, `notes`, `status`, required `purchaseRequestId`, required `vendorPartyId`, approve/issue/cancel metadata)
- `supplyarr_purchase_order_lines` — line items (`lineNumber`, `partId`, `quantityOrdered`, `unitOfMeasure`, `notes`, optional `purchaseRequestLineId`)

Status workflow (this slice):

- `draft` → edit header/lines, approve, cancel
- `approved` → issue to vendor, cancel
- terminal: `issued`, `cancelled`

Creation rule: one open PO (`draft` or `approved`) per purchase request; source PR must be `approved` with vendor and lines.

## API + auth changes

### SupplyArr API endpoints

- `GET /api/purchase-orders` — list (optional `status` filter)
- `GET /api/purchase-orders/{purchaseOrderId}` — detail with lines
- `POST /api/purchase-orders/from-purchase-request/{purchaseRequestId}` — create draft PO from approved PR
- `PUT /api/purchase-orders/{purchaseOrderId}` — update draft header
- `POST /api/purchase-orders/{purchaseOrderId}/lines` — add line (draft)
- `PUT /api/purchase-orders/{purchaseOrderId}/lines/{lineId}` — update line (draft)
- `DELETE /api/purchase-orders/{purchaseOrderId}/lines/{lineId}` — remove line (draft)
- `POST /api/purchase-orders/{purchaseOrderId}/approve` — approve draft PO
- `POST /api/purchase-orders/{purchaseOrderId}/issue` — issue approved PO
- `POST /api/purchase-orders/{purchaseOrderId}/cancel` — cancel draft/approved PO with reason

### Authorization

`SupplyArrAuthorizationService` delegates to purchase-request scopes:

- read: platform admin, tenant admin, supplyarr admin/manager/buyer/clerk, tenant member (`RequirePurchaseOrderRead`)
- create/issue/line edits/cancel: platform admin, tenant admin, supplyarr admin/manager/buyer (`supplyarr.purchaseOrders.create`)
- approve: platform admin, tenant admin, supplyarr admin/manager (`supplyarr.purchaseOrders.approve`)

## Frontend changes

- `PurchaseOrderPanel` — lists POs, create from approved PR, approve draft, issue approved
- API client: `getPurchaseOrders`, `createPurchaseOrderFromPurchaseRequest`, `approvePurchaseOrder`, `issuePurchaseOrder`
- Role helpers: `canCreatePurchaseOrders`, `canApprovePurchaseOrders`

## Tests

### Backend integration (`tests/STLCompliance.SupplyArr.Auth.Tests`)

- purchase order from approved PR + approve + issue happy path
- create denied for `supplyarr_clerk`
- approve denied for `supplyarr_buyer`

### Frontend unit

- `src/api/client.test.ts` — purchase orders list success parsing
- `src/components/PurchaseOrderPanel.test.tsx` — list + approve action rendering

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

- Receiving, pricing/lead-time snapshots (later M8 slices)
- PO cancel UI, multi-line edit UI, duplicate-PR guard messaging in UI
- Event publish `supplyarr.purchaseOrder.issued` (integration slice)
- MaintainArr demand intake → PR → PO automation (M10)

## Next slice (Worker 68)

Recommended: **SupplyArr receiving exceptions** or **RoutArr trip/dispatch foundations** per M8/M9 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
