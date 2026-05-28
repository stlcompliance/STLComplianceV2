# Worker 195 — SupplyArr M8 vendor restrictions

**Products:** SupplyArr, supplyarr-frontend  
**Milestone:** M8  
**Backlog:** SupplyArr `[M8] vendor restrictions` (first incomplete M8 item after W181–194)

## Summary

Tenant-scoped vendor/supplier procurement restrictions with scoped enforcement on purchase requests, purchase orders, RFQ invitations, and receiving. Includes CRUD APIs, audit logging, integration outbox events, and Parties workspace UI.

## Persistence

Migration: `SupplyArrVendorRestrictions`

| Table | Purpose |
|-------|---------|
| `supplyarr_vendor_restrictions` | Active/lifted restriction records per external party |

Fields: `RestrictionKey`, `ScopesJson`, `Reason`, `Status`, `EffectiveFrom`/`EffectiveUntil`, lift metadata, FK to `supplyarr_external_parties`.

### Restriction scopes

| Scope | Enforced on |
|-------|-------------|
| `purchase_requests` | PR create/update vendor |
| `purchase_orders` | PO create from PR, PO issue |
| `rfq_invitations` | RFQ vendor invite |
| `receiving` | Receipt create from issued PO |
| `all_procurement` | All of the above |

Party `approval_status` of `restricted` or `inactive` also blocks procurement (implicit all-procurement).

## API (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/vendor-restrictions` | parties read |
| GET | `/api/vendor-restrictions/{restrictionId}` | parties read |
| PUT | `/api/vendor-restrictions/{restrictionId}` | parties manage |
| POST | `/api/vendor-restrictions/{restrictionId}/lift` | parties manage |
| GET | `/api/parties/{partyId}/vendor-restrictions` | parties read |
| POST | `/api/parties/{partyId}/vendor-restrictions` | parties manage |
| GET | `/api/parties/{partyId}/vendor-restrictions/enforcement` | parties read |

## Services

- `VendorRestrictionService` — CRUD, lift, sync party `approval_status` to `restricted` on create (when previously approved), revert to `approved` when no active restrictions remain
- `VendorProcurementGuardService` — `EnsureVendorAllowedForScopeAsync` used by purchase request, purchase order, RFQ, receiving, and emergency purchase flows

## Outbox (W187)

- `vendor_restriction.created`
- `vendor_restriction.updated`
- `vendor_restriction.lifted`

## Audit

- `vendor_restriction.create`, `.update`, `.lift`

## UI

- `VendorRestrictionsPanel` on Parties workspace — party picker, scope checkboxes, create/lift restrictions, enforcement preview

## Tests

- `SupplyArrVendorRestrictionTests` — block PR + lift allows; outbox + approval sync
- `VendorRestrictionsPanel.test.tsx`

## Next slice

Per backlog: next M8 items after vendor restrictions include supplier incidents, procurement exceptions, or deeper PO coordination if extended beyond W177 procurement coordination worker.
