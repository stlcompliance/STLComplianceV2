# LoadArr — Receiving and Putaway Model

## Expected receipt

An ExpectedReceipt is LoadArr’s receiving expectation. It can come from SupplyArr purchase orders, RoutArr inbound appointments, internal transfers, returns, customer returns, or blind receiving.

```text
ExpectedReceipt
- expectedReceiptId
- tenantId
- expectedReceiptNumber
- sourceType
  - purchase_order
  - transfer
  - return
  - blind
  - route_inbound
  - customer_return
  - supplier_return_replacement
  - maintenance_return
- sourceProduct
  - supplyarr
  - routarr
  - loadarr
  - ordarr
  - maintainarr
  - customarr
  - manual
- sourceObjectRef
- supplierRef
- customerRef
- carrierRef
- routarrTripRef
- dockAppointmentRef
- expectedAt
- appointmentWindowStart
- appointmentWindowEnd
- staffarrSiteId
- staffarrDockLocationId
- staffarrReceivingLocationId
- status
  - draft
  - expected
  - appointment_requested
  - appointment_scheduled
  - in_transit
  - arrived
  - receiving
  - partially_received
  - received
  - discrepancy
  - canceled
  - closed
- expectedLines
- receivedLines
- documentRefs
- notes
- createdAt
- updatedAt
```

## Expected receipt line

```text
ExpectedReceiptLine
- expectedReceiptLineId
- expectedReceiptId
- sourceLineRef
- itemId
- itemDescriptionSnapshot
- expectedQuantity
- unitOfMeasure
- expectedLotNumber
- expectedSerialNumbers
- expectedExpirationDate
- inspectionRequired
- complianceRefs
- notes
```

## Receipt

A Receipt is the actual receiving execution record.

```text
Receipt
- receiptId
- tenantId
- receiptNumber
- expectedReceiptRef
- receiptType
  - planned
  - blind
  - return
  - transfer
  - customer_return
  - maintenance_return
- status
  - draft
  - in_progress
  - received
  - partially_received
  - discrepancy
  - inspection_required
  - putaway_pending
  - closed
  - canceled
- receivedAt
- receivedByPersonId
- staffarrSiteId
- staffarrReceivingLocationId
- staffarrDockLocationId
- supplierRef
- customerRef
- carrierRef
- routarrTripRef
- bolRecordRef
- packingSlipRecordRef
- photoRecordRefs
- receiptLineRefs
- discrepancyRefs
- qualityHoldRefs
- putawayTaskRefs
- complianceEvaluationRef
- notes
- auditTrail
```

## Receipt status definitions

```text
draft
- Receipt exists but receiving has not started.

in_progress
- Receiving is actively being performed.

received
- All expected lines are received without unresolved discrepancy.

partially_received
- Some lines are received, others remain open.

discrepancy
- Difference exists in quantity, item, condition, documents, lot/serial, or other expected detail.

inspection_required
- Received items must be inspected before available stock.

putaway_pending
- Receipt is complete but putaway remains.

closed
- Receipt is complete, discrepancies handled, and putaway/disposition actions created.

canceled
- Receipt was canceled.
```

## Receipt line

```text
ReceiptLine
- receiptLineId
- tenantId
- receiptId
- expectedReceiptLineRef
- itemId
- itemDescriptionSnapshot
- expectedQuantity
- receivedQuantity
- acceptedQuantity
- rejectedQuantity
- damagedQuantity
- shortQuantity
- overQuantity
- unitOfMeasure
- lotNumber
- serialNumbers
- expirationDate
- condition
  - good
  - damaged
  - unknown
  - requires_inspection
  - rejected
- status
  - pending
  - received
  - discrepant
  - on_hold
  - inspection_required
  - putaway_pending
  - putaway_complete
  - rejected
  - closed
- staffarrReceivingLocationId
- suggestedPutawayLocationId
- evidenceRecordRefs
- discrepancyRefs
- qualityHoldRefs
```

## Receiving discrepancy

```text
ReceivingDiscrepancy
- discrepancyId
- tenantId
- discrepancyNumber
- receiptId
- receiptLineId
- discrepancyType
  - shortage
  - overage
  - wrong_item
  - damaged
  - missing_document
  - invalid_document
  - lot_mismatch
  - serial_mismatch
  - expiration_issue
  - quality_issue
  - unknown_item
- severity
  - low
  - moderate
  - high
  - critical
- status
  - open
  - investigating
  - resolved
  - escalated_to_assurarr
  - accepted
  - rejected
  - closed
- expectedValue
- actualValue
- quantityAffected
- evidenceRecordRefs
- assurarrNonconformanceRef
- supplyarrSupplierIssueRef
- notes
```

## Putaway task

A PutawayTask moves received goods from receiving/staging/inspection to a storage location.

```text
PutawayTask
- putawayTaskId
- tenantId
- putawayNumber
- receiptRef
- receiptLineRef
- itemId
- itemDescriptionSnapshot
- lotNumber
- serialNumbers
- expirationDate
- quantity
- unitOfMeasure
- fromLocationId
- suggestedToLocationId
- actualToLocationId
- assignedPersonId
- status
  - open
  - assigned
  - in_progress
  - blocked
  - completed
  - canceled
- priority
  - low
  - normal
  - high
  - urgent
- createdAt
- assignedAt
- startedAt
- completedAt
- completedByPersonId
- exceptionReason
- scanRequired
- scanResultRefs
- evidenceRecordRefs
```

## Putaway status definitions

```text
open
- Putaway is needed but not assigned.

assigned
- Putaway is assigned to a person.

in_progress
- Person started putaway.

blocked
- Putaway cannot proceed due to location, hold, scan, quality, or capacity issue.

completed
- Stock moved to destination location and ledger posted.

canceled
- Putaway was canceled.
```

## Receiving document capture

```text
ReceivingDocumentCapture
- captureId
- tenantId
- receiptId
- documentType
  - bol
  - packing_slip
  - certificate
  - invoice_reference
  - photo
  - other
- recordarrRecordId
- captureSource
  - receiver_upload
  - driver_secure_link
  - routarr_upload
  - supplier_upload
  - import
- status
  - requested
  - uploaded
  - accepted
  - rejected
- uploadedAt
- acceptedByPersonId
- acceptedAt
```

## Receiving workflow

```text
1. SupplyArr, RoutArr, transfer, return, or manual source creates ExpectedReceipt.
2. Receiver starts Receipt.
3. BOL/packing slip is captured through RecordArr/Field Companion if required.
4. Receiver scans/identifies items.
5. Receiver enters quantities, condition, lot, serial, expiration.
6. LoadArr compares actual vs expected.
7. Discrepancies are created if needed.
8. AssurArr nonconformance/hold is created for quality issues.
9. Accepted quantity posts receipt movement.
10. Putaway tasks are created.
11. SupplyArr receives PO receipt status if applicable.
12. Receipt closes after required actions.
```

## Putaway workflow

```text
1. Receipt line creates putaway task.
2. LoadArr suggests destination based on WMS location rules.
3. Worker scans item and destination.
4. LoadArr validates location behavior and capacity.
5. Worker confirms putaway.
6. StockMovement posts.
7. InventoryBalance updates.
8. Putaway task completes.
```

## Blind receiving workflow

```text
1. Receiver starts blind receipt.
2. Receiver identifies supplier/carrier if known.
3. Receiver scans/enters item and quantity.
4. LoadArr creates receipt without expected line match.
5. Discrepancy or unmatched receipt review is created.
6. SupplyArr/procurement may reconcile to PO later.
7. AssurArr may review quality/document issues.
```

## Events

```text
loadarr.expected_receipt.created
loadarr.expected_receipt.updated
loadarr.expected_receipt.arrived
loadarr.expected_receipt.canceled

loadarr.receipt.created
loadarr.receipt.started
loadarr.receipt.line_received
loadarr.receipt.discrepancy_found
loadarr.receipt.partially_received
loadarr.receipt.completed
loadarr.receipt.closed
loadarr.receipt.canceled

loadarr.receiving_document.requested
loadarr.receiving_document.uploaded
loadarr.receiving_document.accepted
loadarr.receiving_document.rejected

loadarr.putaway.created
loadarr.putaway.assigned
loadarr.putaway.started
loadarr.putaway.blocked
loadarr.putaway.completed
loadarr.putaway.canceled
```
