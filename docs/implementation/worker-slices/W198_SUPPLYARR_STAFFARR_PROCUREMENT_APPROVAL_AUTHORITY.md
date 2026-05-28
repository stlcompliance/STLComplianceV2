# Worker 198 — SupplyArr M10 approval authority from StaffArr

**Products:** StaffArr, SupplyArr, supplyarr-frontend  
**Milestone:** M10  
**Backlog:** SupplyArr `[M10] approval authority from StaffArr`

## Summary

Purchase request submit/approve and purchase order issue in SupplyArr are gated by effective StaffArr permission projections (limits, scopes, personId). SupplyArr maintains a rebuildable local mirror refreshed on demand from a StaffArr service-token integration API — no cross-product database FKs.

## StaffArr permission keys

| Key | Action |
|-----|--------|
| `supplyarr.procurement.purchase_requests.submit` | Submit PR |
| `supplyarr.procurement.purchase_requests.approve` | Approve PR |
| `supplyarr.procurement.purchase_orders.issue` | Issue PO |

### Scope types

- `tenant` — unlimited monetary authority for that permission
- `monetary_limit` — `scope_value` is max amount (decimal string)
- `org_unit` — `scope_value` is org unit id (mirrored for display; full org matching deferred)

Authority is derived from materialized or computed effective permission projection (W49).

## StaffArr integration API

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/integrations/procurement-approval-authority` | Service token: source `supplyarr`, target `staffarr`, scope `staffarr.procurement_approval_authority.read` |

Query: `tenantId` + (`personId` or `externalUserId`).

## SupplyArr persistence

Migration: `SupplyArrStaffarrProcurementApprovalAuthority`

| Table | Purpose |
|-------|---------|
| `supplyarr_staffarr_procurement_approval_authority_mirrors` | Per-person authority snapshot (flags, limits, grants JSON, source timestamps) |

## SupplyArr enforcement

When `StaffArr:EnforceProcurementApprovalAuthority` is `true` (default in `appsettings.json`):

- `POST /api/purchase-requests/{id}/submit` — requires submit permission (+ optional submit monetary limit vs estimated PR total from pricing snapshots/catalog)
- `POST /api/purchase-requests/{id}/approve` — requires approve permission (+ approve limit)
- `POST /api/purchase-orders/{id}/issue` — requires issue permission (+ issue limit)

Existing SupplyArr role checks (`RequirePurchaseRequestCreate` / `Approve` / PO create) remain; StaffArr authority is an additional layer.

Mirror refresh: on demand when stale (>1h) or missing; resolves person by `personId` or `externalUserId` (NexArr user id).

Denials return `403` with codes such as `procurement_approval_authority.submit_denied` and details including `authoritySource` and `staffarrPersonId`.

## SupplyArr API (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/me/procurement-approval-authority` | PR read |

## Configuration

```json
"StaffArr": {
  "BaseUrl": "http://localhost:5102",
  "ServiceToken": "",
  "EnforceProcurementApprovalAuthority": true
}
```

Integration token catalog: `supplyarr-staffarr` scope extended with `staffarr.procurement_approval_authority.read`.

## UI

- `ProcurementApprovalAuthorityBanner` on Purchasing workspace — shows StaffArr source, submit/approve/issue allowance, and monetary limits

## Tests

- `StaffArrProcurementApprovalAuthorityTests` — integration endpoint + token scope
- `SupplyArrStaffarrProcurementApprovalAuthorityTests` — cross-product submit/approve + denial without permissions (enforcement enabled in test host)

## Next slice

Worker **199** — Compliance Core fact publishing (complete). Next: **SupplyArr M8 supply readiness dashboard** per backlog.
