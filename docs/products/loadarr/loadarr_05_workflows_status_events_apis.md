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
