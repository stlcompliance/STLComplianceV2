# LoadArr — Scope, Ownership, and Boundaries

## Product purpose

LoadArr is the WMS and inventory execution product for the STL Compliance / ARR suite. It owns what stock exists, where it is, what quantity is available, what is reserved, what is held, how it moves, how it is received, how it is put away, how it is picked, how it is issued, and how it is counted.

LoadArr answers:

- What inventory exists?
- Where is it physically located?
- How much is on hand?
- How much is available?
- How much is reserved?
- How much is on hold/quarantine/damaged/inspection?
- What stock movements happened?
- What is expected to arrive?
- What was received?
- What needs putaway?
- What needs picked?
- What was issued to a work order or order?
- What variance exists after a count?
- What discrepancy needs quality/procurement/personnel follow-up?

## LoadArr owns

```text
- Inventory execution item view
- WMS location behavior attached to StaffArr locations
- Inventory balance
- Stock ledger
- Expected receipt
- Receipt
- Receipt lines
- Putaway tasks
- Reservations
- Pick tasks
- Issue confirmation
- Returns to stock
- Internal transfers
- Replenishment signals
- Inventory counts
- Count lines
- Variances
- Inventory adjustments
- Inventory discrepancies
- Quarantine movement behavior
- Hold-aware movement blocking
- Availability checks
- Inventory status snapshots
```

## LoadArr does not own

```text
- Platform login
- Tenant entitlement
- Person master
- Permission assignment truth
- Internal location identity
- Training/certification truth
- Regulatory/rulepack meaning
- Document/file storage truth
- Supplier/vendor master
- Sourcing records
- Purchase requests
- Purchase orders
- Maintenance work orders
- Maintenance parts demand
- Customer master
- Customer order lifecycle
- Route/trip execution
- Quality hold/release decision
- Analytics read models
- Accounting execution
```

## External product dependencies

```text
NexArr
- Product entitlement
- Login/handoff
- Service tokens

StaffArr
- Person references
- Site/location identity
- Permission checks
- Personnel incidents when inventory issue involves person/process behavior

TrainArr
- Qualification checks for forklift/receiving/count/pick/issue tasks where applicable

Compliance Core
- Storage requirements
- Hazmat/regulated item handling requirements
- Receiving evidence requirements
- Retention/evidence requirements
- Compliance evaluations

RecordArr
- BOL documents
- Packing slips
- Receiving photos
- Damage photos
- Count evidence
- Adjustment evidence
- Signature records
- Generated PDFs/OCR

SupplyArr
- Supplier/vendor references
- Purchase order expected receipts
- Supplier item/sourcing context
- Procurement replenishment
- Supplier performance updates

MaintainArr
- Work-order part demand
- Maintenance counter issue
- Technician pickup
- Part usage/installation events consumed from MaintainArr

RoutArr
- Inbound trip/appointment events
- ETA, arrival, departure
- Carrier/driver context
- Transportation exceptions

OrdArr
- Customer/internal order demand
- Fulfillment dependency
- Order blockers/status updates

CustomArr
- Customer location context where fulfillment/customer returns are involved

AssurArr
- Quality holds
- Nonconformance
- Disposition decisions
- Release decisions
- Inventory quality status

ReportArr
- Inventory dashboards
- Receiving KPIs
- Count accuracy
- Pick/putaway/issue metrics

Field Companion
- Mobile receiving
- Mobile putaway
- Mobile pick/issue
- Mobile transfer
- Mobile count
- Barcode/QR scanning
- Photo/document capture
```

## Core source-of-truth rules

```text
1. StaffArr owns canonical internal location identity.
2. LoadArr owns WMS behavior attached to StaffArr locations.
3. LoadArr owns inventory balances.
4. LoadArr owns stock ledger movement truth.
5. SupplyArr owns supplier/vendor/procurement truth.
6. MaintainArr owns work-order demand and installed/used parts.
7. OrdArr owns order lifecycle demand.
8. RoutArr owns trip/transportation events.
9. AssurArr owns hold/release decisions.
10. LoadArr must obey AssurArr holds.
11. RecordArr owns actual files and evidence records.
12. Compliance Core owns regulatory meaning.
13. ReportArr owns reporting read models, not inventory truth.
```

## Standard LoadArr object envelope

```text
LoadArrObject
- id
- tenantId
- objectNumber
- objectType
- status
- title
- description
- sourceProduct
- sourceObjectRef
- staffarrSiteId
- staffarrLocationId
- itemRef
- quantity
- unitOfMeasure
- recordRefs
- complianceRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- postedAt
- auditTrail
- eventLog
```

## LoadArr object prefixes

```text
ITEM   Inventory item execution view
WLOC   WMS location profile
BAL    Inventory balance
MOV    Stock movement
EXP    Expected receipt
RCV    Receipt
RLN    Receipt line
PUT    Putaway task
RSV    Reservation
PICK   Pick task
ISS    Issue
RET    Return
TRN    Transfer
CNT    Inventory count
CLN    Count line
VAR    Variance
ADJ    Adjustment
DISC   Inventory discrepancy
AVL    Availability check
```

## Standard inventory object reference

```text
InventoryObjectRef
- itemId
- itemNumberSnapshot
- itemNameSnapshot
- lotNumber
- serialNumber
- expirationDate
- unitOfMeasure
- lastResolvedAt
```

## Standard location behavior reference

```text
WmsLocationRef
- staffarrLocationId
- locationNumberSnapshot
- locationNameSnapshot
- locationTypeSnapshot
- siteOrgUnitIdSnapshot
- siteNameSnapshot
- wmsProfileId
- receivable
- pickable
- countable
- quarantine
- statusSnapshot
```


---


# LoadArr — Item, Location Behavior, and Balance Model

## Inventory item execution view

LoadArr may maintain an inventory execution item view. This is not the same as SupplyArr sourcing/procurement truth. LoadArr needs enough item metadata to receive, store, reserve, pick, count, and move inventory safely.

```text
InventoryItem
- itemId
- tenantId
- itemNumber
- name
- description
- itemType
  - part
  - raw_material
  - finished_good
  - consumable
  - tool
  - safety_supply
  - packaging
  - hazmat
  - serialized_asset_candidate
  - maintenance_supply
  - other
- status
  - draft
  - active
  - inactive
  - discontinued
  - blocked
  - archived
- unitOfMeasure
- alternateUnits
- baseUnitOfMeasure
- lotTracked
- serialTracked
- expirationTracked
- conditionTracked
- hazardousFlag
- controlledItemFlag
- temperatureControlled
- storageRequirementRefs
- complianceRefs
- supplyarrSourcingRefs
- recordRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
```

## Item unit conversion

```text
ItemUnitConversion
- conversionId
- itemId
- fromUnit
- toUnit
- factor
- status
  - active
  - inactive
- notes
```

## Item handling rule

```text
ItemHandlingRule
- handlingRuleId
- itemId
- ruleType
  - storage
  - hazmat
  - temperature
  - expiration
  - lot_control
  - serial_control
  - quarantine
  - inspection_required
  - restricted_issue
- complianceRef
- ruleText
- status
  - active
  - inactive
```

## WMS location profile

StaffArr owns the location identity. LoadArr owns whether the location can receive, pick, count, quarantine, stage, store, or move inventory.

```text
WmsLocationProfile
- wmsLocationProfileId
- tenantId
- staffarrLocationId
- locationNumberSnapshot
- locationNameSnapshot
- locationTypeSnapshot
- siteOrgUnitIdSnapshot
- siteNameSnapshot
- pathSnapshot
- status
  - draft
  - active
  - inactive
  - blocked
  - archived
- receivable
- pickable
- countable
- replenishable
- quarantine
- inspectionHold
- staging
- shippingStaging
- receivingStaging
- putawayQueue
- maintenanceHandoff
- technicianPickup
- serviceCounter
- allowsNegativeInventory
- requiresScan
- requiresLot
- requiresSerial
- hazmatAllowed
- temperatureControlled
- capacityRules
- storageRules
- allowedItemTypes
- blockedItemTypes
- allowedMovementTypes
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
```

## WMS location status definitions

```text
draft
- Location profile is being configured.

active
- Location can be used according to behavior flags.

inactive
- Location profile exists but should not be used for new movement.

blocked
- Location cannot be used because of safety, quality, system, or operational block.

archived
- Location profile is retained for history.
```

## Location capacity rule

```text
LocationCapacityRule
- capacityRuleId
- wmsLocationProfileId
- capacityType
  - quantity
  - volume
  - weight
  - pallet_positions
  - item_count
  - custom
- maxValue
- unitOfMeasure
- warningThreshold
- status
```

## Location storage rule

```text
LocationStorageRule
- storageRuleId
- wmsLocationProfileId
- ruleType
  - allowed_item_type
  - blocked_item_type
  - hazmat_class
  - temperature_range
  - expiration_min_days
  - lot_segregation
  - serial_required
  - quality_status
  - customer_owned
  - supplier_owned
- value
- complianceRef
- status
```

## Inventory balance

InventoryBalance represents the current quantity state for an item at a StaffArr location, with optional lot/serial/expiration/condition dimensions.

```text
InventoryBalance
- balanceId
- tenantId
- itemId
- staffarrLocationId
- wmsLocationProfileId
- lotNumber
- serialNumber
- expirationDate
- condition
  - good
  - damaged
  - expired
  - suspect
  - refurbished
  - used
  - unknown
- ownershipType
  - company_owned
  - customer_owned
  - supplier_owned
  - consigned
  - unknown
- quantityOnHand
- quantityAvailable
- quantityReserved
- quantityAllocated
- quantityPicked
- quantityStaged
- quantityOnHold
- quantityDamaged
- quantityExpired
- quantityInInspection
- quantityInTransit
- quantityQuarantined
- unitOfMeasure
- status
  - available
  - reserved
  - hold
  - damaged
  - expired
  - quarantine
  - inspection
  - blocked
  - zero
- activeHoldRefs
- lastMovementAt
- lastCountAt
- createdAt
- updatedAt
```

## Balance quantity definitions

```text
quantityOnHand
- Physical/system quantity at location.

quantityAvailable
- Quantity available to reserve or use.

quantityReserved
- Quantity reserved for known demand.

quantityAllocated
- Quantity assigned to a demand but not picked/issued.

quantityPicked
- Quantity picked but not issued/shipped/consumed.

quantityStaged
- Quantity staged for shipment, issue, or handoff.

quantityOnHold
- Quantity blocked by hold.

quantityDamaged
- Quantity marked damaged.

quantityExpired
- Quantity expired.

quantityInInspection
- Quantity awaiting inspection.

quantityInTransit
- Quantity moving between locations/sites.

quantityQuarantined
- Quantity isolated pending quality decision.
```

## Inventory status snapshot

```text
InventoryStatusSnapshot
- snapshotId
- tenantId
- itemId
- staffarrLocationId
- status
  - healthy
  - low_stock
  - out_of_stock
  - overstock
  - on_hold
  - blocked
  - unknown
- quantityOnHand
- quantityAvailable
- reorderPoint
- preferredStockLevel
- openDemandQuantity
- openReplenishmentQuantity
- generatedAt
```

## Availability check

Other products ask LoadArr whether inventory is available.

```text
AvailabilityCheck
- availabilityCheckId
- tenantId
- sourceProduct
  - maintainarr
  - ordarr
  - routarr
  - assurarr
  - manual
- sourceObjectRef
- itemId
- requestedQuantity
- unitOfMeasure
- neededBy
- staffarrSiteId
- preferredLocationId
- allowSubstitute
- status
  - available
  - partially_available
  - unavailable
  - blocked
  - unknown
- availableQuantity
- reservedQuantity
- suggestedLocationRefs
- substituteSuggestions
- blockerRefs
- checkedAt
```

## Inventory item lifecycle

```text
1. Item execution view is created manually, imported, or from SupplyArr.
2. Tracking rules are defined.
3. WMS storage/handling rules are attached.
4. Item becomes active.
5. Item can be received, stored, reserved, picked, issued, counted, and transferred.
6. Item may become blocked/discontinued/inactive.
7. Historical movement remains available after archive.
```

## Location profile workflow

```text
1. StaffArr creates internal location.
2. LoadArr imports/resolves StaffArr location.
3. LoadArr creates WmsLocationProfile.
4. User sets receivable/pickable/countable/hold/staging behavior.
5. LoadArr validates storage and capacity rules.
6. Location becomes active for WMS use.
7. Inventory can move through the location according to behavior flags.
```

## Balance recalculation workflow

```text
1. StockMovement is posted.
2. LoadArr recalculates affected balances.
3. Holds/reservations/allocations are applied.
4. Availability is recalculated.
5. Balance changed event is emitted.
```

## Events

```text
loadarr.item.created
loadarr.item.updated
loadarr.item.status_changed
loadarr.location_profile.created
loadarr.location_profile.updated
loadarr.location_profile.status_changed
loadarr.balance.created
loadarr.balance.changed
loadarr.balance.zeroed
loadarr.availability_check.completed
loadarr.inventory_status.changed
```


---


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


---


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


---


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


---


# LoadArr — Workflows, Status Logic, Events, and APIs

## Major workflow: receiving from purchase order

```text
1. SupplyArr creates PurchaseOrder.
2. SupplyArr sends ExpectedReceipt to LoadArr.
3. RoutArr may send inbound appointment/ETA if transportation is visible.
4. LoadArr creates Receipt.
5. Receiver captures BOL/packing slip through RecordArr/Field Companion.
6. Receiver confirms quantities, condition, lot, serial, and expiration.
7. LoadArr creates discrepancies for mismatches.
8. AssurArr receives quality issue if needed.
9. LoadArr posts receipt movement.
10. LoadArr creates PutawayTasks.
11. Putaway posts stock movement.
12. SupplyArr receives receipt status update.
```

## Major workflow: receiving from RoutArr inbound appointment

```text
1. RoutArr sends dock appointment notification.
2. LoadArr validates StaffArr dock/location identity.
3. LoadArr confirms appointment or returns conflict.
4. RoutArr sends ETA/arrival/departure updates.
5. LoadArr starts receipt when carrier arrives.
6. Receiving proceeds normally.
```

## Major workflow: MaintainArr work-order part demand

```text
1. MaintainArr creates PartDemand.
2. MaintainArr sends demand to LoadArr.
3. LoadArr runs AvailabilityCheck.
4. If available, LoadArr creates Reservation.
5. LoadArr creates PickTask.
6. Worker picks/stages item.
7. LoadArr issues item to work order.
8. MaintainArr receives issue event.
9. Technician records PartUsage in MaintainArr.
```

## Major workflow: part unavailable

```text
1. LoadArr receives demand.
2. LoadArr cannot reserve required quantity.
3. LoadArr marks reservation partially_reserved or backordered.
4. LoadArr creates ReplenishmentSignal.
5. SupplyArr creates PurchaseRequest if procurement is needed.
6. LoadArr updates source product with shortage/backorder status.
7. When stock arrives, LoadArr fulfills reservation.
```

## Major workflow: OrdArr fulfillment demand

```text
1. OrdArr sends order demand.
2. LoadArr reserves stock.
3. LoadArr creates pick tasks.
4. Worker picks and stages.
5. LoadArr issues to order/shipment.
6. OrdArr receives fulfillment status.
7. RoutArr may receive shipment readiness.
```

## Major workflow: inventory hold from AssurArr

```text
1. AssurArr places QualityHold on inventory/object.
2. LoadArr creates InventoryHoldState.
3. Affected quantity becomes unavailable.
4. LoadArr blocks pick/issue/transfer except allowed disposition movement.
5. AssurArr releases or rejects.
6. LoadArr updates hold state.
7. Released stock becomes available or disposition movement occurs.
```

## Major workflow: cycle count

```text
1. LoadArr creates InventoryCount.
2. Worker counts in Field Companion.
3. LoadArr compares expected and counted quantity.
4. Variance is created if mismatch exists.
5. Recount/approval occurs.
6. Adjustment posts if approved.
7. Balance updates.
8. Serious discrepancy may escalate to AssurArr or StaffArr.
```

## Major workflow: service truck replenishment

```text
1. Service truck is modeled as StaffArr location if it carries stock.
2. LoadArr has WMS profile for service truck.
3. Replenishment need is created.
4. Transfer from parts room to service truck is requested.
5. Worker picks parts room stock.
6. Worker receives into service truck location.
7. Balances update at both locations.
```

## LoadArr emitted events

```text
loadarr.item.created
loadarr.item.updated
loadarr.item.status_changed

loadarr.location_profile.created
loadarr.location_profile.updated
loadarr.location_profile.status_changed

loadarr.balance.created
loadarr.balance.changed
loadarr.balance.zeroed

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

loadarr.putaway.created
loadarr.putaway.assigned
loadarr.putaway.started
loadarr.putaway.blocked
loadarr.putaway.completed
loadarr.putaway.canceled

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

loadarr.count.created
loadarr.count.started
loadarr.count.variance_found
loadarr.count.approved
loadarr.count.posted

loadarr.adjustment.created
loadarr.adjustment.approved
loadarr.adjustment.posted

loadarr.discrepancy.created
loadarr.discrepancy.escalated
loadarr.discrepancy.closed

loadarr.replenishment_signal.created
loadarr.replenishment_signal.sent_to_supplyarr
```

## Integration APIs LoadArr should expose

```text
GET /api/v1/integrations/items
GET /api/v1/integrations/items/{itemId}
POST /api/v1/integrations/items

GET /api/v1/integrations/location-profiles
GET /api/v1/integrations/location-profiles/{wmsLocationProfileId}
POST /api/v1/integrations/location-profiles

GET /api/v1/integrations/balances
GET /api/v1/integrations/balances/{balanceId}
POST /api/v1/integrations/availability-checks

POST /api/v1/integrations/expected-receipts
GET /api/v1/integrations/expected-receipts/{expectedReceiptId}
POST /api/v1/integrations/expected-receipts/{expectedReceiptId}/status-updates

POST /api/v1/integrations/receipts
GET /api/v1/integrations/receipts/{receiptId}
POST /api/v1/integrations/receipts/{receiptId}/lines
POST /api/v1/integrations/receipts/{receiptId}/close

POST /api/v1/integrations/putaway-tasks
POST /api/v1/integrations/putaway-tasks/{putawayTaskId}/complete

POST /api/v1/integrations/reservations
GET /api/v1/integrations/reservations/{reservationId}
POST /api/v1/integrations/reservations/{reservationId}/release

POST /api/v1/integrations/work-order-demands
POST /api/v1/integrations/order-demands

POST /api/v1/integrations/pick-tasks
POST /api/v1/integrations/pick-tasks/{pickTaskId}/complete
POST /api/v1/integrations/issues
POST /api/v1/integrations/returns
POST /api/v1/integrations/transfers

POST /api/v1/integrations/counts
GET /api/v1/integrations/counts/{countId}
POST /api/v1/integrations/counts/{countId}/lines
POST /api/v1/integrations/counts/{countId}/post

POST /api/v1/integrations/adjustments
POST /api/v1/integrations/discrepancies

POST /api/v1/integrations/holds
POST /api/v1/integrations/hold-releases
POST /api/v1/integrations/disposition-movements

GET /api/v1/integrations/stock-movements
GET /api/v1/integrations/stock-movements/{movementId}
```

## APIs LoadArr should consume

```text
NexArr
- POST /handoff/redeem
- POST /service-tokens/introspect
- GET /entitlements/{productKey}

StaffArr
- GET /persons/{personId}
- GET /persons/{personId}/permissions
- GET /locations
- GET /locations/{locationId}
- GET /sites
- POST /incidents

TrainArr
- POST /qualification-checks

Compliance Core
- GET /catalogs/governing-bodies
- GET /rulepacks
- POST /evaluations

RecordArr
- POST /records
- GET /records/{recordId}
- POST /upload-sessions

SupplyArr
- GET /suppliers/{supplierId}
- GET /purchase-orders/{purchaseOrderId}
- GET /sourcing-records
- POST /purchase-requests
- POST /receipt-status-updates
- POST /supplier-quality-events

MaintainArr
- GET /work-orders/{workOrderId}
- POST /part-demand-status-updates
- POST /part-issue-events

RoutArr
- POST /dock-appointment-status
- GET /trips/{tripId}

OrdArr
- POST /orders/{orderId}/fulfillment-records
- POST /orders/{orderId}/blockers

AssurArr
- GET /holds
- POST /nonconformances
- POST /quality-events

ReportArr
- POST /events
```

## Permission examples

```text
loadarr.items.read
loadarr.items.create
loadarr.items.update

loadarr.location_profiles.read
loadarr.location_profiles.manage

loadarr.inventory.read
loadarr.inventory.availability_check

loadarr.receiving.read
loadarr.receiving.execute
loadarr.receiving.close

loadarr.putaway.read
loadarr.putaway.execute

loadarr.reservations.read
loadarr.reservations.create
loadarr.reservations.release

loadarr.pick.read
loadarr.pick.execute
loadarr.issue.execute

loadarr.returns.execute
loadarr.transfers.create
loadarr.transfers.approve
loadarr.transfers.execute

loadarr.counts.read
loadarr.counts.create
loadarr.counts.execute
loadarr.counts.approve
loadarr.counts.post

loadarr.adjustments.create
loadarr.adjustments.approve
loadarr.adjustments.post

loadarr.discrepancies.read
loadarr.discrepancies.manage

loadarr.stock_movements.read
loadarr.admin
```

## Default role examples

```text
Warehouse Viewer
- Read inventory, balances, locations, receipts, picks, counts.

Receiver
- Execute receiving.
- Capture receiving documents.
- Report discrepancies.

Putaway Operator
- Execute putaway tasks.
- Scan locations/items.

Picker
- Execute pick tasks.
- Stage picked goods.

Parts Counter
- Issue parts to work orders.
- Receive returns.
- View reservations.

Inventory Counter
- Execute assigned counts.
- Record count evidence.

Inventory Supervisor
- Approve counts.
- Approve adjustments.
- Resolve variances.
- Manage discrepancies.

Warehouse Manager
- Manage WMS location profiles.
- Manage inventory workflows.
- Approve transfers/adjustments.
- Review dashboard.

LoadArr Admin
- Manage settings, item execution views, WMS profiles, and permissions.
```

## LoadArr UI surfaces

```text
/app/loadarr
- dashboard
- inventory
- item detail
- locations
- location detail
- balances
- stock ledger
- expected receipts
- receiving
- putaway
- reservations
- picks
- issues
- returns
- transfers
- counts
- adjustments
- discrepancies
- holds/quarantine
- replenishment
- settings
```

## Inventory item detail UI

```text
ItemDetailPage
- Item header
- Status
- Tracking rules
- Storage/handling rules
- Balances by location
- Lots/serials
- Open reservations
- Open replenishment
- Recent movements
- Receiving history
- Issue history
- Count history
- Holds/discrepancies
- Documents/evidence
```

## Location detail UI

```text
LocationDetailPage
- StaffArr location snapshot
- WMS behavior flags
- Capacity/storage rules
- Current balances
- Open tasks
- Holds/quarantine status
- Recent movements
- Count history
```

## Receipt detail UI

```text
ReceiptDetailPage
- Receipt header
- Expected receipt context
- Supplier/carrier/source
- Dock/receiving location
- Document capture
- Receipt lines
- Discrepancies
- Holds
- Putaway tasks
- Timeline
```

## Count detail UI

```text
CountDetailPage
- Count header
- Scope
- Assigned counters
- Count lines
- Variances
- Recount status
- Approval
- Adjustment posting
- Evidence
- Timeline
```
