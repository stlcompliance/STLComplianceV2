# Workflow Pack — Procure to Receive to Putaway

## Purpose

This workflow defines how procurement context from SupplyArr becomes receiving, inventory movement, and stock ledger truth in LoadArr.

## Trigger

```text
SupplyArr purchase order issued
```

Alternate triggers:

```text
- MaintainArr parts demand creates purchase need
- OrdArr order fulfillment creates procurement need
- manual receiving without PO creates review-required receipt
- external supplier acknowledgement
```

## Participating products

```text
SupplyArr
LoadArr
ReferenceDataCore
RecordArr
AssurArr
Compliance Core
StaffArr
Field Companion
ReportArr
OrdArr
MaintainArr
RoutArr where inbound transportation is visible
```

## Source-of-truth table

| Business truth | Owner |
|---|---|
| supplier/vendor/dealer master | SupplyArr |
| internal SKU and commercial item context | SupplyArr |
| purchase request/order | SupplyArr |
| receiving execution | LoadArr |
| inventory balance and stock ledger | LoadArr |
| canonical internal locations | StaffArr |
| public product identity and UOM/package rules | ReferenceDataCore |
| stored files and receiving package | RecordArr |
| quality holds/nonconformance | AssurArr |
| regulatory/evidence meaning | Compliance Core |
| inbound carrier/trip visibility | RoutArr |

## Main flow

1. SupplyArr issues purchase order.
2. SupplyArr emits `supplyarr.purchase_order.issued`.
3. LoadArr creates expected receipt or receiving queue item.
4. RoutArr may provide inbound ETA/dock appointment when transportation is managed.
5. Receiver checks in shipment.
6. LoadArr scans/enters items and quantities.
7. ReferenceDataCore resolves UPC/GTIN, manufacturer, package, and UOM where available.
8. LoadArr validates received quantity against PO expectation.
9. RecordArr stores packing slips/photos/SDS/evidence files.
10. AssurArr creates hold/nonconformance if damaged, mismatched, suspect, or inspection-required.
11. LoadArr stages accepted goods.
12. LoadArr posts receiving movement and stock ledger transaction.
13. LoadArr creates putaway tasks.
14. Putaway moves inventory to StaffArr-owned internal location refs.
15. SupplyArr receives receipt status.
16. OrdArr/MaintainArr receive availability updates if they requested the goods.
17. ReportArr projects supplier performance, receiving cycle time, discrepancies, and putaway completion.

## Required events

```text
supplyarr.purchase_order.issued
supplyarr.purchase_order.acknowledged
loadarr.receipt.created
loadarr.receipt.checked_in
loadarr.receipt.discrepancy_created
loadarr.inventory_hold.created
loadarr.inventory_movement.posted
loadarr.receipt.completed
loadarr.putaway.completed
recordarr.record.uploaded
assurarr.nonconformance.created
assurarr.hold.created
```

## Required handoffs

```text
supplyarr -> loadarr: expected receipt
loadarr -> assurarr: quality review/hold request
loadarr -> recordarr: receiving package/evidence storage
loadarr -> referencedatacore: item/package lookup
loadarr -> compliancecore: evidence/regulatory requirement check
loadarr -> maintainarr: parts availability update
loadarr -> ordarr: fulfillment availability update
```

## Blockers

Common blockers:

```text
- PO not found
- supplier blocked
- item unknown
- UPC/GTIN maps to multiple candidates
- package conversion unknown
- damaged goods
- quantity discrepancy above tolerance
- required SDS/evidence missing
- receiving location invalid
- quality hold open
```

## Receipt without PO

If receiving occurs without a PO:

1. LoadArr creates receipt in review-required state.
2. SupplyArr receives a procurement context review item.
3. ReferenceDataCore may resolve item identity.
4. LoadArr does not post unrestricted inventory until the owning policy allows.
5. AssurArr may place hold if goods are suspect.

## Field Companion behavior

Field Companion may support dock check-in, item scan, photo capture, discrepancy capture, staging, and putaway task completion.

Offline receiving actions must validate on sync before stock ledger posting.

## Evidence package

RecordArr receiving package should include:

```text
- PO snapshot
- receipt record
- packing slip
- photos
- discrepancy records
- hold/nonconformance records
- putaway movement refs
- stock ledger refs
- external portal acknowledgements where applicable
```

## Closeout

Receiving is complete when:

```text
- accepted quantities are posted
- rejected/held quantities are accounted for
- putaway is complete or intentionally deferred
- discrepancies are resolved or routed
- evidence package is complete or accepted with warnings
```

## Non-goals

SupplyArr does not own physical inventory balance.

LoadArr does not own supplier commercial truth.
