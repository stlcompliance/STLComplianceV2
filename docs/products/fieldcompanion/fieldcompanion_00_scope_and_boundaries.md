# Field Companion — Scope, Ownership, and Boundaries

## Product purpose

Field Companion is the mobile human execution layer for the STL Compliance / ARR suite. It gives workers, drivers, technicians, receivers, trainers, supervisors, vendors, customers, and temporary external users a simple way to perform permitted actions against the correct product APIs.

Field Companion answers:

- What do I need to do?
- What am I allowed to do?
- What is blocked?
- Why is it blocked?
- What can I complete from my phone?
- What evidence do I need to capture?
- What product owns this action?
- Can this be done offline?
- Did my action sync successfully?

Field Companion is intentionally not a source-of-truth business product. It is a controlled mobile interface that routes actions to source products.

## Field Companion owns

```text
- Mobile task inbox presentation
- Product switcher presentation
- My work view
- Mobile session context
- Device profile
- Secure upload session presentation
- QR/barcode scan UX
- Photo capture UX
- Signature capture UX
- Voice note capture UX
- Document scan UX
- Offline action queue
- Offline sync status
- Conflict presentation
- Mobile action schema rendering
- Field-friendly error messages
- Push notification presentation
- Worker-first task grouping
```

## Field Companion does not own

```text
- Platform login
- Platform identity, active tenant membership, and session lifecycle
- Person master
- Permission assignment truth
- Training assignment truth
- Qualification truth
- Asset truth
- Work order truth
- Defect truth
- Inspection truth
- Inventory balance
- Stock ledger
- Receiving truth
- Procurement truth
- Route/trip truth
- Customer truth
- Order lifecycle truth
- Document/file storage truth
- Quality hold/release truth
- Regulatory meaning
- Reporting read models
- Accounting execution
```

## External product dependencies

```text
NexArr
- Login/handoff
- Active tenant membership and session context
- Product registry and operational-state context
- Session security

StaffArr
- Person context
- Permission context
- Readiness
- Incident reporting
- Person/location references

TrainArr
- Training assignments
- Training steps
- Trainee acknowledgement
- Trainer signoff
- Evaluator signoff
- Qualification status

Compliance Core
- Field-facing compliance prompts
- Evidence requirements
- Controlled situation/evidence fields
- Compliance warnings when products expose them

MaintainArr
- Work orders
- Work order tasks
- Inspections
- Defects
- Meter readings
- Labor entries
- Part requests
- Part usage
- Asset status

LoadArr
- Receiving tasks
- Putaway tasks
- Pick tasks
- Issue tasks
- Transfer tasks
- Count tasks
- Barcode scan validation
- Discrepancy reporting

SupplyArr
- Supplier document upload
- Purchase receiving context where delegated
- Supplier-facing upload links if allowed

RoutArr
- Trips
- Stops
- Arrive/depart actions
- Proof of pickup
- Proof of delivery
- Route exceptions
- BOL/POD capture

CustomArr
- Customer contact/location context
- Customer-facing secure upload or signature flows where allowed
- Customer issue intake where allowed

OrdArr
- Order task context
- Fulfillment task presentation
- Order completion evidence capture

RecordArr
- Actual file/document/photo/signature storage
- Secure upload sessions
- OCR/scanning/PDF processing
- Evidence package references

AssurArr
- Nonconformance evidence capture
- Containment task completion
- CAPA action completion
- Quality audit checklist execution
- Hold/release evidence capture

ReportArr
- Mobile activity reporting facts
- Operational task completion metrics
```

## Core source-of-truth rules

```text
1. Field Companion owns mobile UX, not business truth.
2. Every business action must route to the owning product API.
3. Offline actions are pending until the owning product accepts them.
4. Field Companion may cache task/action schemas but cannot become authoritative.
5. Field Companion may show status snapshots but must refresh from source products.
6. No-login secure links must be narrow, scoped, expiring, and auditable.
7. RecordArr owns uploaded file records.
8. StaffArr owns person identity and permissions.
9. NexArr owns login, tenant membership, sessions, product registry, and launch context.
10. Product APIs own validation and final acceptance of actions.
```

## Standard Field Companion object envelope

```text
FieldCompanionObject
- id
- tenantId
- objectNumber
- objectType
- status
- title
- summary
- personId
- deviceId
- sourceProduct
- sourceObjectRef
- actionType
- createdAt
- createdByPersonId
- updatedAt
- expiresAt
- syncedAt
- auditTrail
- eventLog
```

## Object prefixes

```text
MTASK  Mobile task
MSESS  Mobile session
DEV    Device profile
ACT    Mobile action
OFF    Offline action
SYNC   Sync batch
UPL    Secure upload session
CAP    Capture artifact
SCAN   Scan event
PUSH   Push notification
CONF   Conflict
FORM   Mobile form schema
VIEW   Mobile view definition
```

## Standard source action reference

```text
SourceActionRef
- sourceProduct
- sourceObjectType
- sourceObjectId
- sourceObjectNumber
- actionKey
- actionLabel
- statusSnapshot
- versionSnapshot
- lastResolvedAt
```

## Standard mobile display principle

Field Companion should show:

```text
- what the user needs to do
- what object it is for
- why it matters
- what evidence is required
- what is blocking it
- what happens after submission
- whether it is synced
```

Field Companion should avoid:

```text
- raw JSON
- admin-heavy screens
- unclear product boundaries
- hidden sync failures
- freetyped compliance answers where controlled options are possible
- broad no-login access
```
