# RoutArr — Scope, Ownership, and Boundaries

## Product purpose

RoutArr is the transportation, dispatch, route, trip, stop, ETA, proof, and route-exception execution product for the STL Compliance / ARR suite.

RoutArr answers:

- What route or trip needs to happen?
- Which driver is assigned?
- Which vehicle/trailer/equipment is assigned?
- Is the driver allowed/qualified?
- Is the equipment ready?
- What stops are planned?
- What is the current trip status?
- When did the driver arrive/depart?
- What proof was captured?
- What route exception occurred?
- What inbound appointment/ETA should LoadArr know about?
- What customer/order/delivery impact exists?

## RoutArr owns

```text
- Dispatch plan
- Dispatch board
- Route
- Trip
- Stop
- Stop sequence
- Driver assignment context
- Equipment assignment context
- ETA
- Arrival/departure events
- Proof of pickup
- Proof of delivery
- Route exception
- Transportation delay
- Dock appointment notification
- Inbound transportation visibility
- Transportation readiness validation result
- Route/trip execution status
- Transportation-origin events
```

## RoutArr does not own

```text
- Platform login
- Platform identity, active tenant membership, and session lifecycle
- Person master
- Driver employee profile
- Product permission assignment truth
- Training/certification truth
- Asset maintenance truth
- Vehicle readiness truth
- Inventory balance
- Stock ledger
- Warehouse receiving
- Dock receiving workflow
- Supplier/vendor master
- Customer master
- Customer order lifecycle
- Document/file storage truth
- Quality hold/release decision
- Regulatory rulepack meaning
- Reporting read models
- Accounting execution
```

## External product dependencies

```text
NexArr
- Product registry, launch context, and operational availability
- Login/handoff
- Service tokens

StaffArr
- Person references
- Driver/dispatcher/supervisor references
- Internal site/location/depot/dock identity
- Permission checks
- Personnel incidents from route/driver issues

TrainArr
- Driver qualification
- Equipment/route/customer/site qualification requirements
- Remediation training after incidents

Compliance Core
- Transportation rulepacks
- Driver/equipment/document compliance checks
- Evidence requirements
- Controlled catalogs

MaintainArr
- Vehicle/equipment asset references
- Asset readiness
- Open defects/out-of-service status
- Breakdown-generated defects/work orders

LoadArr
- Inbound dock appointment coordination
- Receiving readiness/status
- Staged inventory readiness for outbound delivery
- Load/pick/stage status where applicable

SupplyArr
- Supplier pickup/delivery context
- Supplier/carrier references where relevant
- SupplierLocation references for supplier/vendor operational stops

CustomArr
- Customer master
- Customer locations
- Customer contacts
- Customer delivery requirements
- Customer activity updates

OrdArr
- Order delivery demand
- Fulfillment dependencies
- Delivery status updates
- Order blockers

RecordArr
- BOL
- POD
- Signature
- Photos
- Route exception evidence
- Delivery/transport documents

AssurArr
- Shipment/order/asset quality holds
- Freight damage nonconformance
- Delivery quality issues
- Quality release before dispatch/delivery

ReportArr
- Transportation dashboards
- On-time KPIs
- Exception trends
- Proof capture metrics

Field Companion
- Driver mobile trip execution
- Stop actions
- Proof capture
- Exception reporting
- Document upload
```

## Core source-of-truth rules

```text
1. RoutArr owns transportation execution.
2. StaffArr owns driver/person identity and internal location identity.
3. TrainArr owns driver/equipment qualification truth.
4. MaintainArr owns vehicle/equipment readiness truth.
5. LoadArr owns receiving, staged inventory, dock receiving workflow, and stock truth.
6. CustomArr owns customer/location/contact truth.
7. OrdArr owns order lifecycle and fulfillment commitment.
8. RecordArr owns proof/document files.
9. AssurArr owns quality hold/release decisions.
10. Compliance Core owns transportation compliance meaning.
11. ReportArr owns reporting outputs, not trip truth.
12. RoutArr may notify LoadArr of dock appointments but must not perform receiving.
```

## Standard RoutArr object envelope

```text
RoutArrObject
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
- driverPersonId
- vehicleAssetRef
- customerRef
- orderRefs
- recordRefs
- complianceRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- completedAt
- canceledAt
- auditTrail
- eventLog
```

## RoutArr object prefixes

```text
DSP    Dispatch plan
RTE    Route
TRIP   Trip
STOP   Stop
DRV    Driver assignment validation
EQP    Equipment assignment validation
ETA    ETA event
ARR    Arrival/departure event
PROOF  Proof event
EXC    Route exception
DAPT   Dock appointment notification
LOAD   Transportation load visibility
DOC    Transportation document requirement
BLK    Transportation blocker
```

## Standard stop location reference

```text
StopLocationRef
- locationType
  - staffarr_internal_location
  - customer_location
- supplier_location
  - ad_hoc_address
- staffarrLocationId
- customerLocationId
- supplierLocationId
- supplierLocationRef
- addressSnapshot
- displayNameSnapshot
- contactSnapshot
- instructionsSnapshot
- lastResolvedAt
```

## Standard transportation source trace

```text
TransportationSourceRef
- sourceProduct
- sourceObjectType
- sourceObjectId
- sourceObjectNumber
- displayNameSnapshot
- statusSnapshot
- lastResolvedAt
```
