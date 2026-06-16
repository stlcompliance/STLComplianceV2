# SupplyArr — Workflows, Status Logic, Events, and APIs

## Major workflow: supplier onboarding

```text
1. User creates Supplier.
2. Supplier starts as prospect or onboarding.
3. User enters contacts, addresses, supplier type, terms snapshots, and notes.
4. SupplyArr creates onboarding checklist.
5. Compliance requirements are generated from supplier type and Compliance Core.
6. Supplier documents are requested and stored in RecordArr.
7. Compliance Core evaluates evidence where applicable.
8. AssurArr quality status is checked if needed.
9. Approver approves, restricts, suspends, or blocks supplier.
10. Supplier becomes available for sourcing and purchase orders if approved.
```

## Major workflow: maintenance part demand to purchase order

```text
1. MaintainArr creates PartDemand.
2. LoadArr checks inventory and cannot fulfill.
3. LoadArr creates ReplenishmentSignal.
4. SupplyArr creates PurchaseRequest.
5. Buyer reviews demand and needed-by date.
6. SupplyArr selects sourcing record/supplier.
7. Approval workflow runs.
8. PurchaseOrder is created.
9. PO is sent to supplier.
10. SupplyArr publishes ExpectedReceipt to LoadArr.
11. LoadArr receives goods.
12. MaintainArr receives availability/issue status through LoadArr.
```

## Major workflow: order-driven procurement

```text
1. OrdArr creates order demand.
2. LoadArr cannot fulfill from stock or requires direct purchase.
3. SupplyArr creates PurchaseRequest.
4. Buyer selects supplier/source.
5. PurchaseOrder is created.
6. ExpectedReceipt is sent to LoadArr.
7. Receipt and fulfillment status update OrdArr through LoadArr/SupplyArr events.
```

## Major workflow: purchase request approval

```text
1. PurchaseRequest is submitted.
2. SupplyArr evaluates sourcing, supplier eligibility, estimated cost, urgency, and compliance.
3. Approval route is selected.
4. Approver approves or rejects.
5. If approved, PR can convert to PO.
6. If rejected, source product receives rejection/blocker status.
```

## Major workflow: purchase order receiving update

```text
1. SupplyArr sends ExpectedReceipt to LoadArr.
2. LoadArr receives against expected lines.
3. LoadArr sends receipt status updates.
4. SupplyArr updates PO line received quantity snapshots.
5. Discrepancies create supplier issue or AssurArr nonconformance.
6. PO moves to partially_received, received, closed, or exception handling.
```

## Major workflow: supplier document request

```text
1. Compliance requirement is missing/expired/rejected.
2. SupplyArr creates SupplierDocumentRequest.
3. Secure upload session is created through RecordArr/Field Companion if needed.
4. Supplier submits document.
5. RecordArr stores document.
6. Compliance Core evaluates evidence.
7. Reviewer accepts or rejects.
8. Supplier compliance status updates.
```

## Major workflow: supplier quality restriction

```text
1. AssurArr creates supplier quality issue, hold, or SCAR.
2. SupplyArr receives quality status event.
3. SupplierQualityStatusSnapshot updates.
4. Supplier eligibility may become restricted, suspended, or blocked.
5. Open PRs/POs/sourcing records are evaluated.
6. Buyers are warned or blocked when selecting supplier.
```

## SupplyArr emitted events

```text
supplyarr.supplier.created
supplyarr.supplier.updated
supplyarr.supplier.onboarding_started
supplyarr.supplier_eligibility.changed
supplyarr.supplier.inactivated
supplyarr.supplier.archived

supplyarr.supplier_contact.created
supplyarr.supplier_contact.updated
supplyarr.supplier_address.created
supplyarr.supplier_address.updated

supplyarr.sourcing_record.created
supplyarr.sourcing_record.updated
supplyarr.sourcing_record.activated
supplyarr.sourcing_record.blocked
supplyarr.sourcing_record.discontinued
supplyarr.sourcing_selection.completed

supplyarr.quote.requested
supplyarr.quote.received
supplyarr.quote.accepted
supplyarr.quote.rejected
supplyarr.quote.expired

supplyarr.purchase_request.created
supplyarr.purchase_request.submitted
supplyarr.purchase_request.review_started
supplyarr.purchase_request.sourcing_started
supplyarr.purchase_request.approval_requested
supplyarr.purchase_request.approved
supplyarr.purchase_request.rejected
supplyarr.purchase_request.converted_to_po
supplyarr.purchase_request.canceled
supplyarr.purchase_request.closed

supplyarr.purchase_order.created
supplyarr.purchase_order.approval_requested
supplyarr.purchase_order.approved
supplyarr.purchase_order.sent
supplyarr.purchase_order.acknowledged
supplyarr.purchase_order.partially_received
supplyarr.purchase_order.received
supplyarr.purchase_order.closed
supplyarr.purchase_order.canceled

supplyarr.supplier_compliance_status.changed
supplyarr.supplier_quality_status.changed
supplyarr.supplier_performance.calculated
supplyarr.supplier_issue.created
supplyarr.supplier_issue.escalated_to_assurarr
supplyarr.supplier_issue.closed
```

## Integration APIs SupplyArr should expose

```text
GET /api/v1/integrations/suppliers
GET /api/v1/integrations/suppliers/{supplierId}
POST /api/v1/integrations/suppliers
PATCH /api/v1/integrations/suppliers/{supplierId}
POST /api/v1/integrations/suppliers/{supplierId}/status-changes

GET /api/v1/integrations/suppliers/{supplierId}/contacts
POST /api/v1/integrations/suppliers/{supplierId}/contacts
GET /api/v1/integrations/suppliers/{supplierId}/addresses
POST /api/v1/integrations/suppliers/{supplierId}/addresses

GET /api/v1/integrations/sourcing-records
GET /api/v1/integrations/sourcing-records/{sourcingRecordId}
POST /api/v1/integrations/sourcing-records
POST /api/v1/integrations/sourcing-selection

GET /api/v1/integrations/supplier-items
POST /api/v1/integrations/supplier-items
GET /api/v1/integrations/substitutes
POST /api/v1/integrations/substitutes

POST /api/v1/integrations/quotes
GET /api/v1/integrations/quotes/{quoteId}

GET /api/v1/integrations/purchase-requests
GET /api/v1/integrations/purchase-requests/{purchaseRequestId}
POST /api/v1/integrations/purchase-requests
POST /api/v1/integrations/purchase-requests/{purchaseRequestId}/submit
POST /api/v1/integrations/purchase-requests/{purchaseRequestId}/approve
POST /api/v1/integrations/purchase-requests/{purchaseRequestId}/reject
POST /api/v1/integrations/purchase-requests/{purchaseRequestId}/convert-to-po

GET /api/v1/integrations/purchase-orders
GET /api/v1/integrations/purchase-orders/{purchaseOrderId}
POST /api/v1/integrations/purchase-orders
POST /api/v1/integrations/purchase-orders/{purchaseOrderId}/approve
POST /api/v1/integrations/purchase-orders/{purchaseOrderId}/send
POST /api/v1/integrations/purchase-orders/{purchaseOrderId}/acknowledge
POST /api/v1/integrations/purchase-orders/{purchaseOrderId}/cancel
POST /api/v1/integrations/purchase-orders/{purchaseOrderId}/close

POST /api/v1/integrations/expected-receipts/publish
POST /api/v1/integrations/receipt-status-updates

GET /api/v1/integrations/supplier-compliance-requirements
POST /api/v1/integrations/supplier-compliance-requirements
POST /api/v1/integrations/supplier-document-requests
POST /api/v1/integrations/supplier-document-requests/{documentRequestId}/review

POST /api/v1/integrations/supplier-quality-events
GET /api/v1/integrations/supplier-performance/{supplierId}
POST /api/v1/integrations/supplier-performance/calculate
```

## APIs SupplyArr should consume

```text
NexArr
- POST /api/v1/platform/handoff/redeem
- POST /api/v1/platform/service-tokens/introspect
- GET /api/v1/platform/tenants/{tenantId}/entitlements/{productKey}

StaffArr
- GET /persons/{personId}
- GET /persons/{personId}/permissions
- GET /locations/{locationId}
- GET /sites
- POST /permission-checks
- POST /incidents

TrainArr
- POST /qualification-checks

Compliance Core
- GET /catalogs/governing-bodies
- GET /evidence-types
- GET /rulepacks
- POST /evaluations

RecordArr
- POST /records
- GET /records/{recordId}
- POST /upload-sessions
- POST /record-packages

LoadArr
- POST /expected-receipts
- GET /receipts/{receiptId}
- GET /discrepancies
- GET /replenishment-signals

MaintainArr
- GET /work-orders/{workOrderId}
- POST /part-demand-status-updates

OrdArr
- GET /orders/{orderId}
- POST /orders/{orderId}/blockers
- POST /orders/{orderId}/status-updates

RoutArr
- POST /trips
- GET /trips/{tripId}
- POST /dock-appointments

AssurArr
- GET /holds
- GET /supplier-quality-issues
- GET /nonconformances
- GET /scar/{scarId}
- POST /quality-events

ReportArr
- POST /events
```

## Permission examples

```text
supplyarr.suppliers.read
supplyarr.suppliers.create
supplyarr.suppliers.update
supplyarr.suppliers.approve
supplyarr.suppliers.restrict
supplyarr.suppliers.suspend
supplyarr.suppliers.block
supplyarr.suppliers.archive

supplyarr.sourcing.read
supplyarr.sourcing.create
supplyarr.sourcing.update
supplyarr.sourcing.approve
supplyarr.sourcing.block

supplyarr.quotes.read
supplyarr.quotes.request
supplyarr.quotes.review

supplyarr.purchase_requests.read
supplyarr.purchase_requests.create
supplyarr.purchase_requests.review
supplyarr.purchase_requests.approve
supplyarr.purchase_requests.reject
supplyarr.purchase_requests.convert_to_po

supplyarr.purchase_orders.read
supplyarr.purchase_orders.create
supplyarr.purchase_orders.approve
supplyarr.purchase_orders.send
supplyarr.purchase_orders.cancel
supplyarr.purchase_orders.close

supplyarr.compliance.read
supplyarr.compliance.review
supplyarr.supplier_documents.request
supplyarr.supplier_documents.review

supplyarr.performance.read
supplyarr.performance.review
supplyarr.admin
```

## Default role examples

```text
SupplyArr Viewer
- Read suppliers, sourcing records, PRs, POs, and supplier lifecycle/eligibility status.

Requester
- Create purchase requests.
- View own request status.

Buyer
- Review PRs.
- Select suppliers/sourcing records.
- Create POs.
- Send POs where allowed.

Procurement Approver
- Approve/reject purchase requests and purchase orders within assigned limits.

Supplier Manager
- Create/update suppliers.
- Manage contacts, addresses, onboarding, restrictions.

Supplier Compliance Reviewer
- Review supplier compliance requirements and documents.

Supplier Quality Coordinator
- View supplier quality status.
- Coordinate with AssurArr on supplier issues/SCARs.

SupplyArr Admin
- Manage procurement settings, sourcing rules, approval routes, and product configuration.
```

## SupplyArr UI surfaces

```text
/app/supplyarr
- dashboard
- suppliers
- supplier detail
- supplier onboarding
- supplier compliance
- supplier performance
- sourcing records
- supplier item catalog
- substitutes
- quotes
- purchase requests
- purchase request detail
- purchase orders
- purchase order detail
- approvals
- supplier issues
- document requests
- settings
```

## Supplier detail UI

```text
SupplierDetailPage
- Header
  - supplierNumber
  - displayName
  - status
  - complianceStatus
  - qualityStatus
  - riskStatus
- Profile
- Contacts
- Addresses
- Compliance requirements
- Documents
- Sourcing records
- Purchase history
- Open PRs/POs
- Quality issues
- Performance
- Notes/communications
- Timeline
```

## Purchase request detail UI

```text
PurchaseRequestDetailPage
- Header
- Source demand
- Lines
- Sourcing selection
- Estimated cost
- Justification
- Approvals
- Blockers
- Conversion to PO
- Timeline
```

## Purchase order detail UI

```text
PurchaseOrderDetailPage
- Header
- Supplier
- Ship-to
- Lines
- Terms snapshots
- Documents/quotes
- Approvals
- Sent/acknowledged state
- Expected receipt
- Receipt status
- Discrepancies
- External financial refs
- Timeline
```
