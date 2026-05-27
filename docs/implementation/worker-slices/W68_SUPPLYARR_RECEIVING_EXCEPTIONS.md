# Worker 68 — SupplyArr receiving exceptions

## Slice name

M8 procurement spine — over/short/damage on receipt, exception records, post validation, APIs, supplyarr-frontend, tests.

## Products touched

- **SupplyArr API** (`apps/supplyarr-api`): `supplyarr_receiving_exceptions`, `QuantityExpected` on receipt lines, `ReceivingExceptionService`, extended post validation, audit events.
- **SupplyArr Frontend** (`apps/supplyarr-frontend`): `ReceivingPanel` line adjustments, exception capture/resolve, API client methods.

## Schema

Migration: `SupplyArrReceivingExceptions`

Added SupplyArr table:

- `supplyarr_receiving_exceptions` — tenant-scoped exception per receipt line (`exceptionType` = `short` | `over` | `damage`, `quantity`, `notes`, `status` open/resolved, resolve metadata)

Extended:

- `supplyarr_receiving_receipt_lines.QuantityExpected` — snapshot of expected qty when draft receipt is created (backfilled from `QuantityReceived` for existing rows)

Exception workflow (this slice):

- Record exceptions on **draft** receipts only
- **Post** validates variance coverage: short when good+damage below expected; over when good qty exceeds PO remaining
- Only **good** `QuantityReceived` increments bin stock and PO `QuantityReceived`
- Resolve marks exception `resolved` (optional clerk acknowledgment before post)

## API + auth changes

### SupplyArr API endpoints

- `GET /api/receiving/{receivingReceiptId}/exceptions` — list exceptions for a receipt
- `POST /api/receiving/{receivingReceiptId}/lines/{lineId}/exceptions` — create exception on draft receipt line
- `POST /api/receiving/exceptions/{receivingExceptionId}/resolve` — resolve open exception on draft receipt
- Existing `PUT /api/receiving/{id}/lines/{lineId}` — no longer blocks over-receive at update time (validated at post with exceptions)
- Receipt/line responses include nested `exceptions` and `quantityExpected`

### Authorization

Same as receiving foundations:

- read: `RequireReceivingRead`
- perform (create/resolve/update line/post): `RequireReceivingPerform`

## Frontend changes

- `ReceivingPanel` — per-line good qty edit, record short/over/damage exceptions, resolve open exceptions, exception summary on receipt
- API client: `updateReceivingReceiptLine`, `createReceivingException`, `resolveReceivingException`
- `HomePage` — mutations and state for line qty + exception forms

## Tests

### Backend integration (`tests/STLCompliance.SupplyArr.Auth.Tests`)

- `Receiving_short_shipment_with_exception_posts_partial_stock`
- `Receiving_over_receive_requires_exception_before_post`
- `Receiving_damage_exception_posts_good_quantity_only`
- Existing W67 receiving tests unchanged

### Frontend unit

- `ReceivingPanel.test.tsx` — exception record button on draft receipt
- `client.test.ts` — receiving list parses `exceptions` array

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

- Partial shipment / multi-receipt PO closure rules
- Barcode scan at receive
- Event publish `supplyarr.receiving.posted` / exception notifications (integration slice)
- MaintainArr received-part status display (M7 integration)
- Supplier incident linkage for damage exceptions

## Next slice (Worker 69)

Recommended: **RoutArr trip/dispatch foundations** or **SupplyArr backorders** per M8/M9 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
