# SupplyArr — Purchase Request and Purchase Order Model

## Purchase request

A PurchaseRequest is the request to buy something. It can come from a person, MaintainArr work-order demand, LoadArr replenishment, OrdArr order demand, AssurArr quality replacement, or manual procurement need.

```text
PurchaseRequest
- purchaseRequestId
- tenantId
- purchaseRequestNumber
- title
- description
- requestSource
  - manual
  - maintainarr_part_demand
  - loadarr_replenishment
  - ordarr_order_demand
  - assurarr_quality_replacement
  - routarr_transport_need
  - staffarr_admin_need
  - compliance_requirement
- sourceProduct
- sourceObjectRef
- status
  - draft
  - submitted
  - review
  - sourcing
  - pending_approval
  - approved
  - rejected
  - converted_to_po
  - partially_converted
  - canceled
  - closed
- priority
  - low
  - normal
  - high
  - urgent
  - emergency
- requestedByPersonId
- ownerBuyerPersonId
- requestedAt
- neededBy
- staffarrSiteId
- shipToStaffarrLocationId
- departmentOrgUnitId
- costCenterRef
- lineRefs
- approvalRefs
- blockerRefs
- justification
- rejectionReason
- recordRefs
- createdAt
- updatedAt
```

## Purchase request status definitions

```text
draft
- Request is being prepared.

submitted
- Request has been submitted.

review
- Buyer/supervisor is reviewing request.

sourcing
- Supplier/source selection is in progress.

pending_approval
- Approval is required.

approved
- Request is approved for PO creation.

rejected
- Request was denied.

converted_to_po
- Entire request was converted to purchase order.

partially_converted
- Some lines were converted to purchase order.

canceled
- Request was canceled.

closed
- Request is complete and no further action remains.
```

## Purchase request line

```text
PurchaseRequestLine
- purchaseRequestLineId
- tenantId
- purchaseRequestId
- lineNumber
- lineType
  - item
  - service
  - repair
  - rental
  - freight
  - other
- itemRef
- itemDescription
- requestedQuantity
- unitOfMeasure
- neededBy
- preferredSupplierId
- preferredSourcingRecordRef
- selectedSourcingRecordRef
- acceptableSubstituteRefs
- estimatedUnitCost
- estimatedExtendedCost
- currency
- sourceDemandRef
- complianceRequirementRefs
- status
  - open
  - sourcing
  - approved
  - rejected
  - converted_to_po
  - canceled
- notes
```

## Procurement approval

```text
ProcurementApproval
- approvalId
- tenantId
- sourceObjectType
  - purchase_request
  - purchase_request_line
  - purchase_order
  - purchase_order_line
  - emergency_purchase
  - supplier_approval
  - substitute_approval
- sourceObjectRef
- approvalType
  - supervisor
  - buyer
  - cost
  - compliance
  - quality
  - emergency
  - supplier
  - substitute
- status
  - pending
  - approved
  - rejected
  - canceled
  - expired
- requestedByPersonId
- requestedAt
- approverPersonId
- decisionAt
- decisionReason
- approvalLimitSnapshot
- evidenceRecordRefs
```

## Purchase order

A PurchaseOrder is SupplyArr’s procurement commitment/ordering document. It may be exported to an external financial/accounting system, but SupplyArr does not own bills/payments/GL.

```text
PurchaseOrder
- purchaseOrderId
- tenantId
- purchaseOrderNumber
- supplierId
- supplierSnapshot
- status
  - draft
  - pending_approval
  - approved
  - sent
  - acknowledged
  - partially_received
  - received
  - closed
  - canceled
  - rejected
- orderDate
- requestedByPersonId
- buyerPersonId
- approvedByPersonId
- approvedAt
- sentAt
- acknowledgedAt
- expectedReceiptDate
- shipToStaffarrSiteId
- shipToStaffarrLocationId
- billToRef
- paymentTermsSnapshot
- shippingTermsSnapshot
- freightTermsSnapshot
- lineRefs
- approvalRefs
- documentRefs
- quoteRefs
- loadarrExpectedReceiptRef
- routarrInboundTripRef
- externalAccountingRef
- notes
- createdAt
- updatedAt
- closedAt
- canceledAt
- cancelReason
```

## Purchase order status definitions

```text
draft
- PO is being prepared.

pending_approval
- PO requires approval.

approved
- PO is approved but not sent.

sent
- PO was sent to supplier.

acknowledged
- Supplier acknowledged PO.

partially_received
- Some lines have been received by LoadArr.

received
- All expected lines received or closed.

closed
- PO is administratively closed.

canceled
- PO was canceled.

rejected
- PO approval or supplier acknowledgement was rejected.
```

## Purchase order line

```text
PurchaseOrderLine
- purchaseOrderLineId
- tenantId
- purchaseOrderId
- lineNumber
- lineType
  - item
  - service
  - repair
  - rental
  - freight
  - other
- itemRef
- sourcingRecordRef
- supplierItemNumber
- manufacturerPartNumber
- vendorPartNumber
- description
- orderedQuantity
- receivedQuantitySnapshot
- canceledQuantity
- remainingQuantity
- unitOfMeasure
- unitCost
- extendedCost
- currency
- expectedAt
- status
  - open
  - partially_received
  - received
  - canceled
  - backordered
  - closed
- sourcePurchaseRequestLineRef
- sourceDemandRef
- complianceRequirementRefs
- notes
```

## Purchase order change

```text
PurchaseOrderChange
- poChangeId
- tenantId
- purchaseOrderId
- changeType
  - quantity
  - price
  - supplier
  - expected_date
  - cancellation
  - line_add
  - line_remove
  - terms
  - ship_to
- status
  - draft
  - pending_approval
  - approved
  - rejected
  - applied
  - canceled
- requestedByPersonId
- requestedAt
- approvedByPersonId
- approvedAt
- beforeSnapshot
- afterSnapshot
- reason
```

## External financial reference

SupplyArr stores references to external financial systems but does not own accounting execution.

```text
ExternalFinancialReference
- externalFinancialReferenceId
- tenantId
- sourceObjectType
  - supplier
  - purchase_order
  - purchase_order_line
  - purchase_request
- sourceObjectRef
- externalSystem
  - quickbooks
  - netsuite
  - dynamics
  - sap
  - other
- externalObjectType
  - vendor
  - purchase_order
  - bill
  - invoice
  - payment
  - account
  - class
  - cost_center
- externalObjectId
- externalObjectNumber
- statusSnapshot
- lastSyncedAt
```

## Purchase request workflow

```text
1. Demand is created manually or by another product.
2. SupplyArr creates PurchaseRequest.
3. Buyer reviews request.
4. Sourcing selection occurs.
5. Approval workflow runs if required.
6. Request is approved, rejected, canceled, or converted to PO.
7. Source product receives status update.
```

## Purchase order workflow

```text
1. Buyer converts approved PR lines or creates PO manually.
2. Supplier, ship-to location, lines, costs, and terms are selected.
3. Approval workflow runs if required.
4. PO is approved.
5. PO is sent to supplier.
6. Supplier acknowledges or rejects/changes.
7. SupplyArr sends ExpectedReceipt to LoadArr.
8. LoadArr receives goods/services evidence as applicable.
9. SupplyArr updates PO line received snapshots.
10. PO closes when complete.
```

## PO receiving update workflow

```text
1. LoadArr receives against ExpectedReceipt.
2. LoadArr sends receipt status update to SupplyArr.
3. SupplyArr updates PO lines with received quantity snapshot.
4. Discrepancies may create AssurArr supplier quality issue.
5. Supplier performance updates.
6. PO becomes partially_received, received, or discrepancy/exception state.
```

## Emergency purchase workflow

```text
1. User creates urgent/emergency PurchaseRequest.
2. Justification is required.
3. Emergency approval route is used.
4. Buyer may select restricted but allowed emergency source if policy permits.
5. PO is created/sent quickly.
6. Post-purchase review is required.
7. Audit trail is retained.
```

## Events

```text
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

supplyarr.procurement_approval.requested
supplyarr.procurement_approval.approved
supplyarr.procurement_approval.rejected
supplyarr.procurement_approval.expired

supplyarr.purchase_order.created
supplyarr.purchase_order.approval_requested
supplyarr.purchase_order.approved
supplyarr.purchase_order.sent
supplyarr.purchase_order.acknowledged
supplyarr.purchase_order.partially_received
supplyarr.purchase_order.received
supplyarr.purchase_order.closed
supplyarr.purchase_order.canceled
supplyarr.purchase_order.change_requested
supplyarr.purchase_order.change_applied

supplyarr.external_financial_ref.created
supplyarr.external_financial_ref.synced
```
