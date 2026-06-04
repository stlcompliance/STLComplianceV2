# MaintainArr — Scope, Ownership, and Boundaries

## Product purpose

MaintainArr is the maintenance execution system. It owns assets, components, preventive maintenance, inspections, defects, work orders, repair execution, maintenance readiness, downtime, labor context, parts demand, and maintenance closeout.

MaintainArr answers:

- What assets exist?
- What condition are they in?
- Are they ready, limited, down, unsafe, or retired?
- What inspections are required?
- What preventive maintenance is due?
- What defects exist?
- What work is needed?
- Who is assigned to the work?
- What parts are needed for the work?
- Was the work completed correctly?
- Can the asset return to service?
- What maintenance evidence exists?

## MaintainArr owns

```text
- Asset registry
- Asset components
- Asset hierarchy
- Asset readiness
- Asset operating status
- Asset compliance status snapshot
- Meter readings used for maintenance
- Preventive maintenance plans
- PM occurrences
- Inspection templates
- Inspection executions
- Inspection answers
- Defects
- Defect severity
- Work orders
- Work order tasks
- Work order labor entries
- Work order parts demand
- Work order part usage/installation
- Maintenance downtime
- Maintenance vendor work coordination
- Maintenance closeout
- Maintenance audit trail
- Maintenance-origin events
```

## MaintainArr does not own

```text
- Platform login
- Tenant entitlement
- Person master
- Permission assignment truth
- Internal location identity
- Training/certification truth
- Regulatory/rulepack meaning
- Document/file storage truth
- Inventory balance
- Stock ledger
- Receiving
- Putaway
- Pick/issue movement truth
- Supplier/vendor master
- Purchase requests
- Purchase orders
- Route/trip execution
- Customer master
- Customer order lifecycle
- Quality hold/release decision
- Reporting read models
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
- Technician/supervisor references
- Site/location references
- Product permission assignments
- Personnel incidents

TrainArr
- Qualification checks
- Required training/certification status
- Remediation assignment after incidents

Compliance Core
- Governing body catalogs
- Rulepacks
- Inspection/maintenance regulatory requirements
- Evidence requirement definitions
- Compliance evaluations

RecordArr
- Photos
- PDFs
- Manuals
- Inspection records
- Work order evidence
- Return-to-service evidence
- Vendor invoices/documents where stored as records

LoadArr
- Inventory availability
- Reservation
- Pick
- Issue
- Return
- Stock ledger
- Parts location behavior

SupplyArr
- Supplier/vendor sourcing
- Purchase requests
- Purchase orders
- Supplier status

RoutArr
- Route exceptions
- Asset breakdown events during trip execution
- Transportation impact

AssurArr
- Quality holds
- Nonconformance
- CAPA
- Asset/part quality release

ReportArr
- Maintenance dashboards
- KPIs
- Cross-product reporting

Field Companion
- Mobile execution of inspections, work orders, photos, signatures, meter readings, and task updates
```

## Core source-of-truth rules

```text
1. MaintainArr owns asset readiness.
2. MaintainArr owns work-order lifecycle.
3. MaintainArr owns maintenance demand for parts.
4. LoadArr owns whether parts physically exist and where they move.
5. SupplyArr owns how unavailable parts are purchased.
6. StaffArr owns internal location identity.
7. MaintainArr references StaffArr locations but does not create canonical locations.
8. StaffArr owns person identity and permissions.
9. TrainArr owns whether a person is qualified.
10. MaintainArr can block work if qualifications are missing.
11. Compliance Core owns regulatory meaning.
12. RecordArr owns the actual document/file/evidence object.
13. AssurArr owns quality hold/release decisions.
14. ReportArr owns reporting views, not maintenance truth.
```

## Standard MaintainArr object envelope

Every major MaintainArr object should include:

```text
MaintainArrObject
- id
- tenantId
- objectNumber
- objectType
- status
- title
- description
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- sourceProduct
- sourceObjectRef
- staffarrSiteId
- staffarrLocationId
- assetRef
- recordRefs
- complianceRefs
- auditTrail
- eventLog
```

## Standard structured reference

```text
SuiteRef
- productKey
- objectType
- objectId
- humanReadableNumber
- displayNameSnapshot
- statusSnapshot
- versionSnapshot
- lastResolvedAt
```

## MaintainArr object prefixes

```text
AST    Asset
CMP    Asset component
MTR    Meter reading
DEF    Defect
WO     Work order
WOT    Work order task
LAB    Labor entry
PDEM   Part demand
PUSE   Part usage
INSP   Inspection
ITPL   Inspection template
PM     Preventive maintenance plan
PMO    PM occurrence
DT     Downtime
MWV    Maintenance vendor work
```
