# W188 — SupplyArr M8 RFQs + quote comparison

## Scope

Tenant-scoped RFQ workflow with vendor invitations, vendor quotes, side-by-side comparison, award selection, and optional purchase request creation — all owned by SupplyArr (no cross-product DB FKs).

## Persistence

| Table | Purpose |
|-------|---------|
| `supplyarr_rfqs` | RFQ header, status, award/PR linkage |
| `supplyarr_rfq_lines` | Part lines requested |
| `supplyarr_rfq_vendor_invitations` | Invited vendors per RFQ |
| `supplyarr_vendor_quotes` | Vendor quote header (totals, lead time, status) |
| `supplyarr_vendor_quote_lines` | Per-line pricing tied to RFQ lines |

Migration: `SupplyArrRfqQuoteComparison` (timestamped file under `Migrations/`).

### Status model

- **RFQ:** `draft` → `submitted` → `awarded` → `closed` (after PR created) / `cancelled`
- **Quote:** `draft` → `submitted` → `selected` | `rejected` | `withdrawn`

## API (`/api/rfqs`, JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/` | `RequireRfqRead` |
| GET | `/{id}` | `RequireRfqRead` |
| POST | `/` | `RequireRfqManage` |
| PUT | `/{id}` | `RequireRfqManage` |
| POST | `/{id}/lines` | `RequireRfqManage` |
| PUT | `/{id}/lines/{lineId}` | `RequireRfqManage` |
| POST | `/{id}/submit` | `RequireRfqManage` |
| POST | `/{id}/invite-vendors` | `RequireRfqManage` |
| POST | `/{id}/quotes` | `RequireRfqManage` |
| PUT | `/{id}/quotes/{quoteId}/lines` | `RequireRfqManage` |
| POST | `/{id}/quotes/{quoteId}/submit` | `RequireRfqManage` |
| GET | `/{id}/quote-comparison` | `RequireRfqRead` |
| POST | `/{id}/select-quote` | `RequireRfqAward` |
| POST | `/{id}/create-purchase-request` | `RequireRfqManage` |

Auth maps to existing procurement roles: read = PR read; manage = buyer; award = PR approve (manager/admin).

## Outbox (W187)

Events enqueued on: `rfq.submitted`, `rfq.vendors.invited`, `rfq.quote.submitted`, `rfq.awarded`.

## UI

- `RfqPanel` on Purchasing workspace (`PurchasingSection`) — list/detail, invite, quote entry, comparison table, award, create PR.

## Tests

- `SupplyArrRfqTests` — end-to-end compare/award/PR, duplicate key, auth
- `RfqPanel.test.tsx`

## Next slice

Per backlog M8: **supplier onboarding** workflow depth. Alternative: **M10** RoutArr demand intake mirror (or TrainArr/StaffArr demand intake).
