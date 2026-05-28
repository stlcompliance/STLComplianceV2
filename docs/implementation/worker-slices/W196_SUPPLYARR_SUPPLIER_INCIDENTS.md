# Worker 196 — SupplyArr M8 supplier incidents

**Products:** SupplyArr, supplyarr-frontend  
**Milestone:** M8  
**Backlog:** SupplyArr `[M8] supplier incidents` (first incomplete M8 item after W195 vendor restrictions)

## Summary

Tenant-scoped supplier/vendor incident tracking with status workflow, optional links to procurement/receiving records, audit logging, integration outbox events, and optional procurement hold via W195 vendor restrictions.

## Persistence

Migration: `SupplyArrSupplierIncidents`

| Table | Purpose |
|-------|---------|
| `supplyarr_supplier_incidents` | Incident records linked to `supplyarr_external_parties` |

Optional FKs: `PurchaseRequestId`, `PurchaseOrderId`, `ReceivingReceiptId`, `ReceivingExceptionId`, `VendorRestrictionId` (set when hold applied).

### Status workflow

`open` → `investigating` → `resolved` → `closed`  
`open` / `investigating` → `cancelled`

### Incident types

`quality`, `delivery`, `compliance`, `safety`, `other`

### Severity

`low`, `medium`, `high`, `critical` (high/critical show procurement-hold action in UI)

## API (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/supplier-incidents` | parties read |
| GET | `/api/supplier-incidents/{incidentId}` | parties read |
| POST | `/api/supplier-incidents` | parties manage |
| PUT | `/api/supplier-incidents/{incidentId}` | parties manage |
| POST | `/api/supplier-incidents/{incidentId}/start-investigation` | parties manage |
| POST | `/api/supplier-incidents/{incidentId}/resolve` | parties manage |
| POST | `/api/supplier-incidents/{incidentId}/close` | parties manage |
| POST | `/api/supplier-incidents/{incidentId}/cancel` | parties manage |
| POST | `/api/supplier-incidents/{incidentId}/apply-procurement-restriction` | parties manage |
| GET | `/api/parties/{partyId}/supplier-incidents` | parties read |

## Enforcement hook

`apply-procurement-restriction` creates a W195 vendor restriction (`all_procurement` or custom scopes) and links `VendorRestrictionId` on the incident. Existing `VendorProcurementGuardService` enforces blocks on PR/PO/RFQ/receiving.

## Outbox (W187)

- `supplier_incident.created`
- `supplier_incident.updated`
- `supplier_incident.investigating`
- `supplier_incident.resolved`
- `supplier_incident.closed`
- `supplier_incident.cancelled`
- `supplier_incident.restriction_applied`

## Audit

- `supplier_incident.create`, `.update`, `.investigate`, `.resolve`, `.close`, `.cancel`, `.apply_restriction`

## UI

- `SupplierIncidentsPanel` on Parties workspace — open incidents, workflow actions, apply procurement hold for high/critical

## Tests

- `SupplyArrSupplierIncidentTests` — workflow + PR block after hold; outbox on create
- `SupplierIncidentsPanel.test.tsx`

## Next slice

Worker **197** — procurement exceptions (complete). Next: SupplyArr **M10** items per backlog (`approval authority from StaffArr`, `Compliance Core fact publishing`).
