# TrainArr — Scope, Ownership, and Boundaries

## Product purpose

TrainArr is the learning management and qualification product for the STL Compliance / ARR suite.

TrainArr is the LMS + qualification management floor for the suite. It owns course creation, delivery, enrollment, progress tracking, assessments, certifications, compliance documentation, and analytics, then extends that baseline with role gating, renewals, remediation, audit packets, and readiness publication.

TrainArr answers:

- What course, program, or learning path is required?
- What content, module, or step must be completed?
- Who is enrolled or assigned?
- What progress has been made?
- What assessment, signoff, or evaluation is required?
- What certificate, qualification, or credential is issued?
- What is due, overdue, expiring, or blocked?
- What evidence is required and where is it stored?
- What remediation or retraining is required?
- What readiness status should StaffArr receive?
- How do other products check whether a person is qualified?

## LMS floor capabilities

TrainArr should cover the baseline LMS expectations before adding compliance-specific behavior:

- course catalog and search
- course creation and versioning
- learning paths and programs
- learner enrollment and assignment
- progress tracking and resume state
- quizzes, exams, and assessments
- instructor-led sessions and attendance
- practical evaluation and signoff
- certificate issuance and renewal
- transcript and completion history
- notifications and due-date management
- analytics and completion reporting

## TrainArr-specific compliance extensions

TrainArr adds the compliance-specific layer on top of the LMS floor:

- role/site/equipment/customer-driven qualification rules
- remediation after failure, incident, or audit finding
- qualification checks for downstream products
- evidence capture and audit packet generation
- renewal and expiration enforcement
- StaffArr readiness publication

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
- Platform identity, active tenant membership, and session lifecycle
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
- Product registry, launch context, and operational availability
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
