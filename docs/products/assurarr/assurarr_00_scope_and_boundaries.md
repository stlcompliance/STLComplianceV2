# AssurArr — Scope, Ownership, and Boundaries

## Product purpose

AssurArr is the quality assurance, nonconformance, hold, containment, disposition, CAPA, audit finding, supplier quality, customer complaint, and quality release system for the STL Compliance / ARR suite.

AssurArr answers:

- What quality issue exists?
- What is affected?
- Should the affected object be held?
- What containment action is required?
- What disposition is allowed?
- What corrective/preventive action is required?
- Was the corrective action effective?
- Can the held object be released?
- Which products must block work because of quality status?
- What evidence proves the quality decision?

## AssurArr owns

```text
- Quality nonconformance
- Quality issue classification
- Quality severity
- Quality hold placement
- Quality hold release decision
- Containment actions
- Disposition decisions
- Corrective action
- Preventive action
- CAPA action plan
- CAPA verification of effectiveness
- Quality audit
- Quality audit finding
- Supplier quality issue
- Customer complaint quality workflow
- Quality review
- Quality release
- Quality score/status snapshots
- Quality-origin events
```

## AssurArr does not own

```text
- Platform login
- Platform identity, active tenant membership, and session lifecycle
- Person master
- Permission assignment truth
- Training/certification truth
- Regulatory/rulepack meaning
- Document/file storage truth
- Asset repair execution
- Work order execution
- Inventory balance
- Stock ledger
- Receiving execution
- Procurement/purchase order truth
- Supplier/vendor master
- Route/trip execution
- Customer master
- Order lifecycle
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
- Owner/assignee references
- Site/location references
- Permission checks
- Personnel incidents if quality issue involves people/process behavior

TrainArr
- Remediation training when issue is caused by competence/training gap
- Qualification status where quality approval requires qualified reviewers

Compliance Core
- Rulepack requirements
- Evidence requirements
- Regulatory implications
- Controlled catalogs for quality/compliance classification

RecordArr
- Photos
- PDFs
- Audit evidence
- Nonconformance evidence
- CAPA evidence
- Supplier documents
- Customer complaint documents
- Release evidence

MaintainArr
- Asset holds
- Maintenance-related nonconformance
- Repair quality failures
- Work order corrective actions
- Asset return-to-service blockers

LoadArr
- Inventory holds
- Receiving discrepancies
- Quarantine inventory
- Inventory disposition execution
- Stock movement after disposition

SupplyArr
- Supplier master references
- Supplier quality events
- Supplier corrective action requests
- Supplier score/status impact

RoutArr
- Shipment/trip holds
- Delivery quality incidents
- Freight damage events

CustomArr
- Customer complaint context
- Customer relationship history
- Customer quality requirements

OrdArr
- Order holds
- Fulfillment blockers
- Customer/order quality release dependencies

ReportArr
- Quality dashboards
- CAPA aging
- Nonconformance trends
- Supplier quality metrics
- Customer complaint metrics

Field Companion
- Mobile quality evidence capture
- Containment task execution
- CAPA action completion
- Audit checklist execution
```

## Core source-of-truth rules

```text
1. AssurArr owns nonconformance truth.
2. AssurArr owns quality hold and release decisions.
3. AssurArr owns CAPA truth.
4. AssurArr owns quality audit finding truth.
5. LoadArr obeys inventory holds but owns stock ledger movement.
6. MaintainArr obeys asset holds but owns maintenance work execution.
7. OrdArr obeys order holds but owns order lifecycle.
8. RoutArr obeys shipment/trip holds but owns transportation execution.
9. SupplyArr owns supplier master; AssurArr owns supplier quality issue and quality status.
10. CustomArr owns customer master; AssurArr owns customer complaint quality workflow.
11. StaffArr owns person records; AssurArr can trigger personnel incident or retraining workflows.
12. TrainArr owns remediation training.
13. RecordArr owns evidence files.
14. Compliance Core owns regulatory meaning.
15. ReportArr owns analytics outputs, not quality decisions.
```

## Standard AssurArr object envelope

Every major AssurArr object should include:

```text
AssurArrObject
- id
- tenantId
- objectNumber
- objectType
- status
- title
- description
- severity
- sourceProduct
- sourceObjectRef
- affectedObjectRefs
- ownerPersonId
- staffarrSiteId
- staffarrLocationId
- recordRefs
- complianceRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- closedAt
- closedByPersonId
- auditTrail
- eventLog
```

## AssurArr object prefixes

```text
NCR    Nonconformance
HOLD   Quality hold
CONT   Containment action
DISP   Disposition
CAPA   Corrective/preventive action
ACT    CAPA action
VER    Effectiveness verification
AUD    Quality audit
FIND   Audit finding
COMP   Customer complaint quality case
SQA    Supplier quality action
QREV   Quality review
QREL   Quality release
QS     Quality status snapshot
SCORE  Quality scorecard
```

## Standard affected object reference

```text
AffectedObjectRef
- productKey
- objectType
- objectId
- humanReadableNumber
- displayNameSnapshot
- statusSnapshot
- affectedQuantity
- unitOfMeasure
- lotNumber
- serialNumber
- locationSnapshot
- impactType
  - held
  - suspect
  - damaged
  - rejected
  - blocked
  - requires_review
  - informational
- lastResolvedAt
```

## Quality severity model

```text
low
- Minor issue with limited impact and no immediate safety/compliance concern.

moderate
- Meaningful issue requiring correction, containment, review, or trend monitoring.

high
- Serious issue affecting customer, compliance, supplier performance, inventory usability, asset readiness, or process reliability.

critical
- Severe issue requiring immediate containment, hold, escalation, or regulatory/customer leadership attention.
```
