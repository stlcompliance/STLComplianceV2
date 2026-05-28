# W190 — SupplyArr M8 emergency purchase workflow

## Scope

Urgent procurement path on purchase requests with emergency metadata, expedited submit, stricter manager override approval (tenant/SupplyArr admin only), audit trail, integration outbox events, and PO issuance linked to the approved emergency PR.

## Persistence

Emergency fields on `supplyarr_purchase_requests` (no separate table):

| Column | Purpose |
|--------|---------|
| `IsEmergency` | Marks emergency workflow |
| `EmergencyReason` | Required business justification |
| `EmergencyExpeditedAt` / `EmergencyExpeditedByUserId` | Expedited submit audit |
| `ManagerOverrideApproved` | Override approval flag |
| `ManagerOverrideJustification` | Required on override approve |
| `ManagerOverrideApprovedAt` / `ManagerOverrideApprovedByUserId` | Override approver audit |

Migration: `SupplyArrEmergencyPurchase`.

## API (`/api/emergency-purchases`, JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/` | `RequireEmergencyPurchaseRead` |
| GET | `/pending` | `RequireEmergencyPurchaseOverrideApprove` |
| GET | `/{id}` | read |
| POST | `/` | `RequireEmergencyPurchaseCreate` (manager+, not buyer) |
| POST | `/{id}/expedited-submit` | `RequireEmergencyPurchaseExpedite` |
| POST | `/{id}/manager-override-approve` | `RequireEmergencyPurchaseOverrideApprove` (tenant_admin, supplyarr_admin) |
| POST | `/{id}/issue-purchase-order` | `RequireEmergencyPurchaseIssueOrder` |

Standard `PurchaseRequestResponse` includes emergency fields for list/detail elsewhere.

## Outbox (W187)

- `emergency_purchase.created`
- `emergency_purchase.expedited_submitted`
- `emergency_purchase.manager_override_approved`
- `emergency_purchase.purchase_order_issued`

Also emits standard `purchase_request.approved` on manager override for downstream consumers.

## UI

- `EmergencyPurchasePanel` on Purchasing workspace — create, expedited submit, pending queue, override approve, issue PO.

## Tests

- `SupplyArrEmergencyPurchaseTests` — E2E flow + issue without override guard
- `EmergencyPurchasePanel.test.tsx`

## Next slice

Per backlog: cross-milestone **M10 RoutArr demand intake mirror**, or remaining M8 procurement depth (e.g. vendor restrictions automation).
