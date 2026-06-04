# LoadArr — Reservation, Pick, Issue, Return, and Transfer Model

## Reservation

A Reservation protects stock for known demand. Demand may come from MaintainArr work orders, OrdArr customer/internal orders, manual requests, quality dispositions, or other source products.

```text
Reservation
- reservationId
- tenantId
- reservationNumber
- sourceProduct
  - maintainarr
  - ordarr
  - routarr
  - assurarr
  - loadarr
  - manual
- sourceObjectRef
- sourceLineRef
- itemId
- itemDescriptionSnapshot
- requestedQuantity
- reservedQuantity
- unitOfMeasure
- requiredBy
- priority
  - low
  - normal
  - high
  - urgent
  - emergency
- staffarrSiteId
- preferredLocationId
- status
  - requested
  - reserved
  - partially_reserved
  - backordered
  - substituted
  - released
  - pick_created
  - picked
  - issued
  - canceled
- reservationLineRefs
- substituteRefs
- createdAt
- updatedAt
- expiresAt
```

## Reservation line

```text
ReservationLine
- reservationLineId
- reservationId
- itemId
- staffarrLocationId
- lotNumber
- serialNumber
- expirationDate
- reservedQuantity
- unitOfMeasure
- status
  - active
  - picked
  - issued
  - released
  - canceled
```

## Pick task

A PickTask instructs a worker to remove inventory from a location for issue, staging, transfer, order fulfillment, or maintenance handoff.

```text
PickTask
- pickTaskId
- tenantId
- pickNumber
- reservationRef
- sourceProduct
- sourceObjectRef
- pickType
  - work_order_issue
  - order_fulfillment
  - transfer
  - replenishment
  - quality_disposition
  - manual
- itemId
- itemDescriptionSnapshot
- quantityToPick
- quantityPicked
- unitOfMeasure
- fromLocationId
- stagingLocationId
- assignedPersonId
- status
  - open
  - assigned
  - in_progress
  - short_pick
  - picked
  - staged
  - issued
  - canceled
- priority
- scanRequired
- pickedAt
- stagedAt
- issuedAt
- exceptionReason
- evidenceRecordRefs
```

## Pick line

```text
PickLine
- pickLineId
- pickTaskId
- itemId
- fromLocationId
- lotNumber
- serialNumber
- expirationDate
- quantityToPick
- quantityPicked
- unitOfMeasure
- status
  - open
  - picked
  - short
  - substituted
  - canceled
```

## Issue

An Issue is the final stock movement out of inventory control to a consuming object such as a work order or order.

```text
Issue
- issueId
- tenantId
- issueNumber
- issueType
  - issue_to_work_order
  - issue_to_order
  - issue_to_person
  - issue_to_asset
  - issue_to_location
  - issue_to_scrap
  - issue_to_vendor
  - other
- sourceProduct
- sourceObjectRef
- pickTaskRef
- reservationRef
- status
  - draft
  - posted
  - reversed
  - canceled
- issuedByPersonId
- issuedToPersonId
- issuedAt
- issueLineRefs
- recordRefs
- notes
```

## Issue line

```text
IssueLine
- issueLineId
- issueId
- itemId
- fromLocationId
- lotNumber
- serialNumber
- expirationDate
- quantityIssued
- unitOfMeasure
- stockMovementRef
```

## Return to stock

```text
InventoryReturn
- returnId
- tenantId
- returnNumber
- returnType
  - work_order_unused
  - customer_return
  - pick_cancel_return
  - transfer_return
  - vendor_return_cancel
  - found_stock
- sourceProduct
- sourceObjectRef
- itemId
- quantity
- unitOfMeasure
- fromPersonId
- fromLocationId
- toLocationId
- condition
  - good
  - damaged
  - suspect
  - requires_inspection
- status
  - requested
  - received
  - inspection_required
  - returned_to_stock
  - held
  - rejected
  - canceled
- receivedByPersonId
- receivedAt
- recordRefs
- stockMovementRef
```

## Transfer

A Transfer moves inventory from one location to another.

```text
InventoryTransfer
- transferId
- tenantId
- transferNumber
- transferType
  - bin_to_bin
  - site_to_site
  - warehouse_to_shop
  - parts_room_to_service_truck
  - service_truck_to_parts_room
  - quarantine_transfer
  - inspection_transfer
  - staging_transfer
  - maintenance_handoff
  - replenishment
- sourceProduct
- sourceObjectRef
- status
  - draft
  - requested
  - approved
  - picked
  - in_transit
  - received
  - posted
  - canceled
- fromLocationId
- toLocationId
- requestedByPersonId
- approvedByPersonId
- pickedByPersonId
- receivedByPersonId
- requestedAt
- approvedAt
- pickedAt
- receivedAt
- postedAt
- lineRefs
- recordRefs
```

## Transfer line

```text
InventoryTransferLine
- transferLineId
- transferId
- itemId
- lotNumber
- serialNumber
- expirationDate
- requestedQuantity
- transferredQuantity
- receivedQuantity
- unitOfMeasure
- status
  - open
  - picked
  - in_transit
  - received
  - short
  - canceled
```

## Replenishment signal

```text
ReplenishmentSignal
- replenishmentSignalId
- tenantId
- signalNumber
- itemId
- staffarrSiteId
- preferredLocationId
- signalType
  - below_reorder_point
  - demand_shortage
  - work_order_demand
  - order_demand
  - min_max
  - manual
- sourceProduct
- sourceObjectRef
- requestedQuantity
- unitOfMeasure
- priority
- status
  - open
  - sent_to_supplyarr
  - purchase_request_created
  - fulfilled
  - canceled
- supplyarrPurchaseRequestRef
- createdAt
```

## Reservation workflow

```text
1. Source product sends demand.
2. LoadArr checks available balance.
3. LoadArr creates Reservation.
4. Quantity is reserved against balance.
5. Pick task is created when ready.
6. If stock is unavailable, replenishment signal is created.
7. Source product receives reservation status.
```

## Pick and issue workflow

```text
1. Reservation creates PickTask.
2. Worker opens task in Field Companion.
3. Worker scans location/item/lot/serial if required.
4. Worker confirms picked quantity.
5. Short pick creates exception if needed.
6. Picked stock moves to staging or issue state.
7. Issue is posted to source object.
8. StockMovement updates balance.
9. Source product receives issue event.
```

## Maintenance issue workflow

```text
1. MaintainArr sends PartDemand.
2. LoadArr reserves stock.
3. LoadArr creates PickTask.
4. Parts worker picks item.
5. Item is staged at maintenance handoff/service counter/technician pickup.
6. Item is issued to work order.
7. MaintainArr receives issue event.
8. Technician records part usage/installation in MaintainArr.
```

## Order fulfillment workflow

```text
1. OrdArr sends order demand.
2. LoadArr reserves stock.
3. LoadArr creates pick task.
4. Stock is picked and staged.
5. RoutArr may receive pickup/delivery readiness.
6. Stock is issued to order/shipment.
7. OrdArr receives fulfillment status.
```

## Transfer workflow

```text
1. User or system requests transfer.
2. LoadArr validates from/to locations and item rules.
3. Approval occurs if required.
4. Stock is picked from source location.
5. Stock is marked in transit if site-to-site or staged movement.
6. Destination receives stock.
7. StockMovement posts transfer.
8. Balances update.
```

## Events

```text
loadarr.reservation.created
loadarr.reservation.reserved
loadarr.reservation.partially_reserved
loadarr.reservation.backordered
loadarr.reservation.released
loadarr.reservation.canceled

loadarr.pick.created
loadarr.pick.assigned
loadarr.pick.started
loadarr.pick.short
loadarr.pick.completed
loadarr.pick.staged
loadarr.pick.canceled

loadarr.issue.created
loadarr.issue.posted
loadarr.issue.reversed
loadarr.issue.canceled

loadarr.return.created
loadarr.return.received
loadarr.return.posted
loadarr.return.held

loadarr.transfer.created
loadarr.transfer.approved
loadarr.transfer.picked
loadarr.transfer.in_transit
loadarr.transfer.received
loadarr.transfer.posted
loadarr.transfer.canceled

loadarr.replenishment_signal.created
loadarr.replenishment_signal.sent_to_supplyarr
loadarr.replenishment_signal.fulfilled
```
