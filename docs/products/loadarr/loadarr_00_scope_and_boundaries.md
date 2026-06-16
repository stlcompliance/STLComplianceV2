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
5. SupplyArr owns supplier/vendor/procurement and tenant commercial item/part/material/SKU truth.
6. ReferenceDataCore owns shared public identifiers, public taxonomies, UOM normalization, manufacturer identity, and crosswalks.
7. MaintainArr owns work-order demand and installed/used parts.
8. OrdArr owns order lifecycle demand.
9. RoutArr owns trip/transportation events.
10. AssurArr owns hold/release decisions.
11. LoadArr must obey AssurArr holds.
12. RecordArr owns actual files and evidence records.
13. Compliance Core owns regulatory meaning.
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
