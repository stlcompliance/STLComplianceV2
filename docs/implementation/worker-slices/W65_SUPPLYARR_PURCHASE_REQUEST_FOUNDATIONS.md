# Worker 65 — SupplyArr purchase request foundations

## Slice name

M8 procurement spine — purchase request header/lines, draft edit, submit/approve/reject workflow start, `/api/purchase-requests`, supplyarr-frontend panel, tests.

## Products touched

- **SupplyArr API** (`apps/supplyarr-api`): purchase request domain tables, CRUD + workflow endpoints, audit events.
- **SupplyArr Frontend** (`apps/supplyarr-frontend`): `PurchaseRequestPanel` on home workspace with create/submit/approve/reject flows.

## Schema

Migration: `SupplyArrPurchaseRequestFoundations`

Added SupplyArr tables:

- `supplyarr_purchase_requests` — tenant-scoped PR header (`requestKey`, `title`, `notes`, `status`, optional `vendorPartyId`, requester/submit/approve/reject metadata)
- `supplyarr_purchase_request_lines` — line items (`lineNumber`, `partId`, `quantityRequested`, `unitOfMeasure`, `notes`)

Status workflow (this slice):

- `draft` → edit header/lines, submit, cancel (cancel not exposed in UI yet)
- `submitted` → approve or reject
- terminal: `approved`, `rejected`, `cancelled`

## API + auth changes

### SupplyArr API endpoints

- `GET /api/purchase-requests` — list (optional `status` filter)
- `GET /api/purchase-requests/{purchaseRequestId}` — detail with lines
- `POST /api/purchase-requests` — create draft (optional initial lines)
- `PUT /api/purchase-requests/{purchaseRequestId}` — update draft header
- `POST /api/purchase-requests/{purchaseRequestId}/lines` — add line (draft)
- `PUT /api/purchase-requests/{purchaseRequestId}/lines/{lineId}` — update line (draft)
- `DELETE /api/purchase-requests/{purchaseRequestId}/lines/{lineId}` — remove line (draft)
- `POST /api/purchase-requests/{purchaseRequestId}/submit` — submit for approval
- `POST /api/purchase-requests/{purchaseRequestId}/approve` — approve submitted PR
- `POST /api/purchase-requests/{purchaseRequestId}/reject` — reject with reason

### Authorization

`SupplyArrAuthorizationService` enforces:

- read: platform admin, tenant admin, supplyarr admin/manager/buyer/clerk, tenant member (`RequirePurchaseRequestRead`)
- create/submit/line edits: platform admin, tenant admin, supplyarr admin/manager/buyer (`supplyarr.purchaseRequests.create`)
- approve/reject: platform admin, tenant admin, supplyarr admin/manager (`supplyarr.purchaseRequests.approve`)

## Frontend changes

- `PurchaseRequestPanel` — lists PRs, shows lines, create draft with first line, submit/approve/reject actions
- API client: `getPurchaseRequests`, `createPurchaseRequest`, `submitPurchaseRequest`, `approvePurchaseRequest`, `rejectPurchaseRequest`
- Role helpers: `canCreatePurchaseRequests`, `canApprovePurchaseRequests`

## Tests

### Backend integration (`tests/STLCompliance.SupplyArr.Auth.Tests`)

- purchase request submit + approve happy path
- purchase request submit + reject happy path
- create denied for `supplyarr_clerk`
- approve denied for `supplyarr_buyer`

### Frontend unit

- `src/api/client.test.ts` — purchase requests list success parsing
- `src/components/PurchaseRequestPanel.test.tsx` — list + approve action rendering

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

- Purchase orders, receiving, pricing/lead-time snapshots (later M8 slices)
- PR cancel endpoint/UI, multi-line edit UI, idempotency keys on create
- MaintainArr demand intake → PR automation (M10)
- Event publish `supplyarr.purchaseRequest.created` (integration slice)

## Next slice (Worker 66)

Recommended: **SupplyArr purchase order foundations** or **RoutArr trip/dispatch foundations** per M8/M9 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
