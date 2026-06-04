# LoadArr — Counts, Adjustments, Discrepancy, and Stock Ledger Model

## Stock movement

StockMovement is the immutable inventory ledger event. All balance changes should be explainable by stock movements.

```text
StockMovement
- movementId
- tenantId
- movementNumber
- movementType
  - receipt
  - putaway
  - transfer
  - reservation
  - reservation_release
  - pick
  - stage
  - issue_to_work_order
  - issue_to_order
  - issue_to_person
  - return_from_work_order
  - return_from_customer
  - return_to_stock
  - adjustment
  - count_adjustment
  - quarantine
  - release_from_hold
  - scrap
  - reject
  - reverse
- status
  - pending
  - posted
  - reversed
  - canceled
- itemId
- fromLocationId
- toLocationId
- lotNumber
- serialNumber
- expirationDate
- quantity
- unitOfMeasure
- sourceProduct
- sourceObjectRef
- performedByPersonId
- performedAt
- postedAt
- postedByPersonId
- reasonCode
- recordRefs
- reversalOfMovementRef
- auditTrail
```

## Stock movement rule

```text
1. Posted movements are not edited.
2. Corrections are reversal/adjustment movements.
3. Balance is derived from movements plus reservation/hold state.
4. Every movement must have a reason/source.
5. High-risk movements require approval/evidence.
```

## Inventory count

```text
InventoryCount
- countId
- tenantId
- countNumber
- countType
  - cycle
  - full
  - spot
  - variance_recount
  - location_audit
  - item_audit
- scope
  - location
  - item
  - zone
  - site
  - custom
- status
  - draft
  - planned
  - open
  - in_progress
  - variance_review
  - recount_required
  - approved
  - posted
  - canceled
- staffarrSiteId
- locationRefs
- itemRefs
- assignedPersonId
- plannedAt
- startedAt
- completedAt
- approvedByPersonId
- approvedAt
- postedAt
- countLineRefs
- varianceRefs
- adjustmentRefs
- recordRefs
- notes
```

## Count line

```text
CountLine
- countLineId
- countId
- itemId
- staffarrLocationId
- lotNumber
- serialNumber
- expirationDate
- expectedQuantitySnapshot
- countedQuantity
- varianceQuantity
- unitOfMeasure
- countedByPersonId
- countedAt
- recountRequired
- status
  - pending
  - counted
  - variance
  - recounted
  - approved
  - rejected
- evidenceRecordRefs
- notes
```

## Inventory variance

```text
InventoryVariance
- varianceId
- tenantId
- varianceNumber
- countId
- countLineId
- itemId
- staffarrLocationId
- expectedQuantity
- countedQuantity
- varianceQuantity
- varianceValueSnapshot
- severity
  - low
  - moderate
  - high
  - critical
- status
  - open
  - recount_required
  - investigating
  - approved_adjustment
  - rejected
  - posted
  - escalated_to_assurarr
  - escalated_to_staffarr
  - closed
- reasonCode
- explanation
- approvedByPersonId
- approvedAt
- adjustmentRef
- discrepancyRef
- evidenceRecordRefs
```

## Inventory adjustment

```text
InventoryAdjustment
- adjustmentId
- tenantId
- adjustmentNumber
- adjustmentType
  - count_adjustment
  - damage
  - expiration
  - found_stock
  - lost_stock
  - correction
  - scrap
  - quality_disposition
  - system_correction
- status
  - draft
  - pending_approval
  - approved
  - posted
  - rejected
  - canceled
- itemId
- staffarrLocationId
- lotNumber
- serialNumber
- expirationDate
- quantityChange
- unitOfMeasure
- reasonCode
- requestedByPersonId
- approvedByPersonId
- postedByPersonId
- requestedAt
- approvedAt
- postedAt
- stockMovementRef
- evidenceRecordRefs
- notes
```

## Inventory discrepancy

An InventoryDiscrepancy is a broader issue that may come from receiving, count, pick, return, transfer, damage, expiration, or system mismatch.

```text
InventoryDiscrepancy
- discrepancyId
- tenantId
- discrepancyNumber
- sourceType
  - receiving
  - count
  - pick
  - return
  - transfer
  - damage
  - expiration
  - system
  - unknown
- sourceObjectRef
- itemId
- staffarrLocationId
- lotNumber
- serialNumber
- quantityDifference
- unitOfMeasure
- discrepancyType
  - shortage
  - overage
  - damaged
  - expired
  - wrong_location
  - wrong_item
  - serial_mismatch
  - lot_mismatch
  - duplicate
  - missing_document
  - unknown
- severity
  - low
  - moderate
  - high
  - critical
- status
  - open
  - investigating
  - resolved
  - adjusted
  - escalated_to_assurarr
  - escalated_to_staffarr
  - closed
  - canceled
- reasonCode
- rootCauseSummary
- evidenceRecordRefs
- assurarrNonconformanceRef
- staffarrIncidentRef
- adjustmentRefs
- closedAt
- closedByPersonId
```

## Hold-aware inventory state

LoadArr does not decide quality release, but it must enforce holds.

```text
InventoryHoldState
- holdStateId
- tenantId
- itemId
- staffarrLocationId
- lotNumber
- serialNumber
- quantityHeld
- unitOfMeasure
- assurarrHoldRef
- holdStatusSnapshot
  - active
  - release_pending
  - released
  - rejected
- allowedMovementTypes
  - quarantine_transfer
  - inspection_transfer
  - disposition_movement
  - none
- createdAt
- releasedAt
```

## Count workflow

```text
1. User creates InventoryCount.
2. LoadArr selects scope: location, item, zone, or site.
3. Count is assigned.
4. Worker counts via Field Companion.
5. CountLine records counted quantity.
6. LoadArr compares expected vs counted.
7. Variance is created if mismatch exists.
8. Recount may be required.
9. Supervisor approves adjustment if appropriate.
10. Adjustment posts stock movement.
11. Balance updates.
```

## Adjustment workflow

```text
1. Adjustment is requested manually or from variance/discrepancy.
2. LoadArr checks approval requirement.
3. Evidence is attached if required.
4. Supervisor approves or rejects.
5. Approved adjustment posts StockMovement.
6. Balance updates.
7. ReportArr receives adjustment fact.
```

## Discrepancy escalation workflow

```text
1. LoadArr creates InventoryDiscrepancy.
2. LoadArr classifies type/severity.
3. If quality issue, create/notify AssurArr nonconformance.
4. If personnel/process issue, create StaffArr incident.
5. If supplier issue, notify SupplyArr.
6. If order/work impact, notify OrdArr/MaintainArr.
7. Discrepancy closes after correction or escalation.
```

## Stock ledger audit workflow

```text
1. User opens item/location/lot/serial ledger.
2. LoadArr shows movement history.
3. Each balance change traces to movement.
4. Each movement traces to source object and actor.
5. Reversals/adjustments show reason and approval.
```

## Events

```text
loadarr.stock_movement.created
loadarr.stock_movement.posted
loadarr.stock_movement.reversed
loadarr.stock_movement.canceled

loadarr.count.created
loadarr.count.started
loadarr.count.line_counted
loadarr.count.variance_found
loadarr.count.recount_required
loadarr.count.approved
loadarr.count.posted
loadarr.count.canceled

loadarr.variance.created
loadarr.variance.approved
loadarr.variance.posted
loadarr.variance.escalated

loadarr.adjustment.created
loadarr.adjustment.approved
loadarr.adjustment.posted
loadarr.adjustment.rejected

loadarr.discrepancy.created
loadarr.discrepancy.escalated_to_assurarr
loadarr.discrepancy.escalated_to_staffarr
loadarr.discrepancy.closed

loadarr.inventory_hold_state.created
loadarr.inventory_hold_state.released
```
