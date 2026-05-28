# Worker 197 — SupplyArr M8 procurement exceptions

**Products:** SupplyArr, supplyarr-frontend  
**Milestone:** M8  
**Backlog:** SupplyArr `[M8] procurement exceptions` (distinct from W68 receiving exceptions and W196 supplier incidents)

## Summary

Structured procurement exceptions attached to purchase requests, purchase orders, or RFQs with investigation, resolution, waive-with-approval workflow, audit logging, and integration outbox events.

## Persistence

Migration: `SupplyArrProcurementExceptions`

| Table | Purpose |
|-------|---------|
| `supplyarr_procurement_exceptions` | Exception records keyed by tenant + `exception_key`, linked to PR/PO/RFQ subject |

### Status workflow

`open` → `investigating` → `resolved` → `closed`  
`investigating` → `waive_pending` → `waived` → `closed`  
`waive_pending` → `investigating` (reject waive)  
`open` / `investigating` → `cancelled`

### Categories

`approval_delay`, `vendor_issue`, `budget_override`, `policy_violation`, `pricing_variance`, `other`

### Subject types

`purchase_request`, `purchase_order`, `rfq`

## API (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/procurement-exceptions` | PR read |
| GET | `/api/procurement-exceptions/{exceptionId}` | PR read |
| PUT | `/api/procurement-exceptions/{exceptionId}` | PR create |
| POST | `/api/procurement-exceptions/{exceptionId}/start-investigation` | PR create |
| POST | `/api/procurement-exceptions/{exceptionId}/resolve` | PR create |
| POST | `/api/procurement-exceptions/{exceptionId}/request-waive` | PR create |
| POST | `/api/procurement-exceptions/{exceptionId}/approve-waive` | PR **approve** |
| POST | `/api/procurement-exceptions/{exceptionId}/reject-waive` | PR **approve** |
| POST | `/api/procurement-exceptions/{exceptionId}/close` | PR create |
| POST | `/api/procurement-exceptions/{exceptionId}/cancel` | PR create |
| POST/GET | `/api/purchase-requests/{id}/procurement-exceptions` | create / list |
| POST/GET | `/api/purchase-orders/{id}/procurement-exceptions` | create / list |
| POST/GET | `/api/rfqs/{id}/procurement-exceptions` | create / list |

## Outbox (W187)

- `procurement_exception.created`
- `procurement_exception.updated`
- `procurement_exception.investigating`
- `procurement_exception.resolved`
- `procurement_exception.waive_requested`
- `procurement_exception.waived`
- `procurement_exception.waive_rejected`
- `procurement_exception.closed`
- `procurement_exception.cancelled`

## Audit

- `procurement_exception.create`, `.update`, `.investigate`, `.resolve`, `.waive_requested`, `.waive_approved`, `.waive_rejected`, `.close`, `.cancel`

## UI

- `ProcurementExceptionsPanel` on Purchasing workspace — open exceptions on PR/PO/RFQ, investigate, resolve, request/approve/reject waive, close, cancel

## Tests

- `SupplyArrProcurementExceptionTests` — waive approval workflow, resolve/cancel paths, outbox on create
- `ProcurementExceptionsPanel.test.tsx`

## Boundary notes

- **Not** receiving exceptions (W68) — those remain on receipts/lines.
- **Not** supplier incidents (W196) — party-level quality/delivery tracking with optional vendor restrictions.

## Next slice

After M8 cluster completion, see `02_PRODUCT_IMPLEMENTATION_BACKLOG.md` for first incomplete item outside SupplyArr M8 (typically M10 cross-product demand or next product milestone).
