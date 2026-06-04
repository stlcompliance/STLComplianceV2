# TrainArr — Scope, Ownership, and Boundaries

## Product purpose

TrainArr is the training, assignment, evaluation, signoff, remediation, qualification, certificate, expiration, and renewal product for the STL Compliance / ARR suite.

TrainArr is the qualification engine. It owns whether a person completed required training and whether that completion produces an active qualification/certificate.

TrainArr answers:

- What training program is required?
- What module or step must be completed?
- Who is assigned?
- What is due or overdue?
- What evidence is required?
- Who trained, evaluated, or signed off?
- Did the person pass?
- Is remediation required?
- What qualification is issued?
- When does it expire?
- Should StaffArr readiness be updated?

## TrainArr owns

```text
- Training program
- Training program version
- Training module
- Training step
- Training content references
- Training requirement profile
- Training assignment
- Module progress
- Step progress
- Trainee acknowledgement
- Trainer signoff
- Evaluator signoff
- Written evaluation result
- Practical evaluation result
- Observation result
- Remediation assignment
- Qualification definition
- Person qualification
- Certificate issuance
- Qualification expiration
- Qualification renewal
- Qualification suspension/revocation
- Training evidence references
- Training-origin events
```

## TrainArr does not own

```text
- Platform login
- Tenant entitlement
- Person master
- Person employment status
- Org structure
- Site/location identity
- Product permission assignment truth
- Regulatory/rulepack meaning
- Governing body catalog truth
- Document/file storage truth
- Asset truth
- Work order truth
- Inventory truth
- Procurement truth
- Route/trip truth
- Customer/order truth
- Quality hold/release truth
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
- Position, department, team, site, location context
- Permission checks
- Person readiness display
- Incident/remediation requests
- Person history updates

Compliance Core
- Rulepacks
- Training requirement regulatory meaning
- Citation references
- Evidence requirements
- Applicability logic
- Retention requirements

RecordArr
- Training evidence files
- Signed acknowledgements
- Certificates as files
- Evaluation evidence
- Training completion packages

MaintainArr
- Asset/equipment training applicability
- Maintenance skill requirements
- Incident-driven retraining requests

LoadArr
- Warehouse equipment/process training requirements
- Forklift/PIT/receiving/count/pick/issue task qualification checks

SupplyArr
- Procurement/supplier compliance training requirements where applicable

RoutArr
- Driver/route/equipment/customer/site training requirements
- Driver qualification checks before dispatch

CustomArr
- Customer-specific training requirements
- Customer site access/training constraints

OrdArr
- Order/customer/service requirements that depend on qualified workers

AssurArr
- Remediation after quality issue, audit finding, nonconformance, or CAPA
- CAPA actions requiring training completion

ReportArr
- Training dashboards
- Qualification expiration reports
- Completion metrics
- Remediation KPIs

Field Companion
- Mobile training steps
- Trainer/evaluator signoff
- Practical evaluation execution
- Training evidence capture
```

## Core source-of-truth rules

```text
1. TrainArr owns training program definitions.
2. TrainArr owns training assignment execution state.
3. TrainArr owns signoff/evaluation state.
4. TrainArr owns qualification/certificate issuance truth.
5. StaffArr owns the person and readiness/person history view.
6. TrainArr publishes qualification status to StaffArr.
7. Compliance Core owns regulatory meaning and citations.
8. RecordArr owns actual files and evidence records.
9. Origin products own incidents or operational events that trigger retraining.
10. Products may block work based on TrainArr qualification checks.
11. TrainArr should not create product-local permissions.
12. TrainArr should not own StaffArr org/position/site identity.
```

## Standard TrainArr object envelope

```text
TrainArrObject
- id
- tenantId
- objectNumber
- objectType
- status
- title
- description
- version
- sourceProduct
- sourceObjectRef
- personId
- staffarrSiteId
- staffarrLocationId
- complianceRefs
- recordRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- completedAt
- closedAt
- auditTrail
- eventLog
```

## TrainArr object prefixes

```text
TPROG  Training program
TPV    Training program version
TMOD   Training module
TSTEP  Training step
TREQ   Training requirement profile
TASN   Training assignment
MPROG  Module progress
SPROG  Step progress
SIGN   Signoff
EVAL   Evaluation
OBS    Observation
REM    Remediation assignment
QDEF   Qualification definition
QUAL   Person qualification
CERT   Certificate
REN    Renewal event
SUSP   Qualification suspension
REV    Qualification revocation
```

## Standard person training reference

```text
TrainingPersonRef
- personId
- personNumberSnapshot
- displayNameSnapshot
- primaryPositionSnapshot
- primaryDepartmentSnapshot
- primarySiteSnapshot
- statusSnapshot
- lastResolvedAt
```

## Standard qualification status

```text
QualificationStatus
- pending
- active
- expiring_soon
- expired
- suspended
- revoked
- superseded
- not_required
```
