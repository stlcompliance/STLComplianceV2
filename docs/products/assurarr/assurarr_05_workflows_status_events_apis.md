# AssurArr — Workflows, Status Logic, Events, and APIs

## Major workflow: receiving discrepancy to nonconformance

```text
1. LoadArr detects receiving discrepancy.
2. LoadArr sends discrepancy event to AssurArr.
3. AssurArr creates Nonconformance.
4. AssurArr classifies severity and affected inventory.
5. AssurArr places QualityHold if needed.
6. LoadArr blocks affected inventory movement.
7. AssurArr assigns containment action.
8. AssurArr determines disposition.
9. LoadArr executes disposition movement if inventory action is required.
10. SupplyArr receives supplier quality impact.
11. CAPA/SCAR is created if systemic or supplier-responsible.
12. Nonconformance closes after verification.
```

## Major workflow: asset quality hold

```text
1. MaintainArr reports failed repair verification or quality concern.
2. AssurArr creates Nonconformance.
3. AssurArr places asset QualityHold.
4. MaintainArr blocks return-to-service.
5. AssurArr defines containment/disposition/CAPA.
6. MaintainArr performs repair/rework if required.
7. AssurArr verifies evidence.
8. AssurArr releases hold.
9. MaintainArr resumes return-to-service workflow.
```

## Major workflow: order quality hold

```text
1. Quality issue affects order fulfillment.
2. AssurArr places order QualityHold.
3. OrdArr creates order blocker.
4. LoadArr/RoutArr/other execution products stop affected fulfillment.
5. AssurArr investigates and determines release/disposition.
6. AssurArr releases or rejects.
7. OrdArr resolves blocker and continues or cancels/revises order.
```

## Major workflow: supplier corrective action

```text
1. Supplier quality issue is opened.
2. AssurArr creates Nonconformance.
3. AssurArr creates SCAR if supplier response is required.
4. SupplyArr supplier context is referenced.
5. Supplier response is received as RecordArr evidence.
6. AssurArr reviews response.
7. AssurArr accepts, rejects, or requests revision.
8. Supplier quality score/status updates.
9. SupplyArr consumes supplier quality status.
```

## Major workflow: customer complaint quality case

```text
1. CustomArr or user reports customer complaint.
2. AssurArr creates CustomerComplaintQualityCase.
3. AssurArr triages severity and affected objects.
4. AssurArr creates Nonconformance if required.
5. AssurArr creates holds if required.
6. Investigation/root cause occurs.
7. Customer response is prepared and stored in RecordArr.
8. CAPA is created if systemic issue exists.
9. CustomArr receives customer activity/status update.
10. Complaint case closes.
```

## Major workflow: CAPA

```text
1. Nonconformance, finding, complaint, supplier issue, or trend requires CAPA.
2. AssurArr creates CAPA.
3. Root cause analysis is completed.
4. Action plan is defined.
5. Actions are routed to people/products.
6. Evidence is collected in RecordArr.
7. Verification plan runs.
8. Effectiveness is verified.
9. CAPA closes if effective.
10. CAPA reopens or creates follow-up if ineffective.
```

## Major workflow: quality audit

```text
1. User creates QualityAudit.
2. Scope, requirements, auditors, and checklist are defined.
3. Audit is executed.
4. Findings are created.
5. Findings create Nonconformance/CAPA when needed.
6. Corrective actions are completed.
7. Findings are verified.
8. Audit closes.
```

## AssurArr emitted events

```text
assurarr.nonconformance.created
assurarr.nonconformance.status_changed
assurarr.nonconformance.closed

assurarr.hold.placed
assurarr.hold.status_changed
assurarr.hold.release_requested
assurarr.hold.released
assurarr.hold.rejected
assurarr.hold.canceled

assurarr.containment.created
assurarr.containment.assigned
assurarr.containment.completed
assurarr.containment.verified

assurarr.disposition.proposed
assurarr.disposition.approved
assurarr.disposition.executed
assurarr.disposition.rejected

assurarr.root_cause.started
assurarr.root_cause.completed

assurarr.capa.created
assurarr.capa.status_changed
assurarr.capa.action_assigned
assurarr.capa.action_completed
assurarr.capa.verified_effective
assurarr.capa.verified_ineffective
assurarr.capa.closed
assurarr.capa.reopened

assurarr.scar.created
assurarr.scar.sent
assurarr.scar.response_received
assurarr.scar.accepted
assurarr.scar.rejected
assurarr.scar.closed

assurarr.audit.created
assurarr.audit.started
assurarr.audit.finding_created
assurarr.audit.closed

assurarr.customer_complaint.created
assurarr.customer_complaint.status_changed
assurarr.customer_complaint.closed

assurarr.supplier_quality_issue.created
assurarr.supplier_quality_issue.status_changed
assurarr.supplier_quality_issue.closed

assurarr.quality_status.changed
assurarr.quality_status.published
assurarr.scorecard.generated
```

## Integration APIs AssurArr should expose

```text
GET /api/v1/integrations/nonconformances
GET /api/v1/integrations/nonconformances/{nonconformanceId}
POST /api/v1/integrations/nonconformances
POST /api/v1/integrations/nonconformances/{nonconformanceId}/status-updates

GET /api/v1/integrations/holds
GET /api/v1/integrations/holds/{holdId}
POST /api/v1/integrations/holds
POST /api/v1/integrations/holds/{holdId}/release-requests
POST /api/v1/integrations/holds/{holdId}/release
POST /api/v1/integrations/holds/{holdId}/reject

POST /api/v1/integrations/containment-actions
POST /api/v1/integrations/dispositions
POST /api/v1/integrations/root-cause-analyses

GET /api/v1/integrations/capas
GET /api/v1/integrations/capas/{capaId}
POST /api/v1/integrations/capas
POST /api/v1/integrations/capas/{capaId}/actions
POST /api/v1/integrations/capas/{capaId}/verification

POST /api/v1/integrations/supplier-quality-issues
POST /api/v1/integrations/customer-complaint-quality-cases
POST /api/v1/integrations/audits
POST /api/v1/integrations/findings

GET /api/v1/integrations/quality-status
GET /api/v1/integrations/quality-status/{targetProduct}/{targetObjectId}
POST /api/v1/integrations/quality-status-checks
GET /api/v1/integrations/scorecards
```

## APIs AssurArr should consume

```text
NexArr
- POST /api/v1/platform/handoff/redeem
- POST /api/v1/platform/service-tokens/introspect
- GET /api/v1/platform/session/context

StaffArr
- GET /persons/{personId}
- GET /persons/{personId}/permissions
- GET /locations/{locationId}
- POST /incidents
- POST /restrictions

TrainArr
- POST /remediation-requests
- POST /qualification-checks

Compliance Core
- GET /catalogs/governing-bodies
- GET /rulepacks
- POST /evaluations
- POST /evidence-mapping/suggest

RecordArr
- POST /records
- GET /records/{recordId}
- POST /upload-sessions
- POST /record-packages

MaintainArr
- GET /assets/{assetId}
- POST /quality-holds
- POST /quality-hold-releases
- POST /work-orders

LoadArr
- GET /balances
- POST /holds
- POST /hold-releases
- POST /disposition-movements

SupplyArr
- GET /suppliers/{supplierId}
- POST /supplier-quality-events
- POST /supplier-status-updates

RoutArr
- POST /shipment-holds
- POST /route-exception-quality-events

CustomArr
- GET /customers/{customerId}
- POST /customer-activities
- POST /customer-issues

OrdArr
- POST /orders/{orderId}/blockers
- POST /orders/{orderId}/blockers/{blockerId}/resolve

ReportArr
- POST /events
```

## Permission examples

```text
assurarr.nonconformances.read
assurarr.nonconformances.create
assurarr.nonconformances.triage
assurarr.nonconformances.investigate
assurarr.nonconformances.close

assurarr.holds.read
assurarr.holds.place
assurarr.holds.release_request
assurarr.holds.release
assurarr.holds.reject

assurarr.containment.assign
assurarr.containment.complete
assurarr.containment.verify

assurarr.dispositions.propose
assurarr.dispositions.approve
assurarr.dispositions.execute

assurarr.capa.read
assurarr.capa.create
assurarr.capa.plan
assurarr.capa.assign_actions
assurarr.capa.verify
assurarr.capa.close

assurarr.audits.read
assurarr.audits.create
assurarr.audits.execute
assurarr.audits.close

assurarr.findings.read
assurarr.findings.create
assurarr.findings.close

assurarr.supplier_quality.read
assurarr.supplier_quality.manage
assurarr.customer_complaints.read
assurarr.customer_complaints.manage

assurarr.scorecards.read
assurarr.settings.manage
assurarr.admin
```

## Default role examples

```text
Quality Viewer
- Read nonconformances, holds, CAPAs, audits, scorecards.

Quality Technician
- Create nonconformances.
- Complete containment actions.
- Upload evidence.
- Execute assigned audit checklist items.

Quality Reviewer
- Triage nonconformances.
- Review evidence.
- Propose dispositions.
- Request hold releases.

Quality Manager
- Place/release holds.
- Approve dispositions.
- Open/close CAPAs.
- Approve verification.
- Close nonconformances.

Supplier Quality Manager
- Manage supplier quality issues.
- Send/review SCARs.
- Update supplier quality status.

Customer Quality Manager
- Manage customer complaint quality cases.
- Prepare response records.
- Coordinate customer-facing closure with CustomArr.

Quality Auditor
- Create/execute audits.
- Create findings.
- Verify finding closure.

AssurArr Admin
- Manage settings, templates, catalogs, and role configuration.
```

## AssurArr UI surfaces

```text
/app/assurarr
- dashboard
- nonconformances
- nonconformance detail
- holds
- hold detail
- containment actions
- dispositions
- CAPA
- CAPA detail
- audits
- audit detail
- findings
- supplier quality
- customer complaints
- quality releases
- scorecards
- settings
```

## Nonconformance detail UI

```text
NonconformanceDetailPage
- Header
  - nonconformanceNumber
  - title
  - status
  - severity
  - owner
  - due date
- Source context
  - source product
  - source object
  - affected objects
- Classification
  - type
  - category
  - impact flags
- Holds
  - active holds
  - release requests
- Containment
  - action list
- Investigation
  - root cause
  - contributing factors
- Disposition
  - proposed/approved/executed disposition
- CAPA
  - related CAPAs/actions
- Evidence
  - RecordArr records
- Timeline
  - audit history
```

## Hold detail UI

```text
HoldDetailPage
- Hold header
- Affected object list
- Blocking product impact
- Release requirements
- Release evidence
- Release approvals
- Timeline
```

## CAPA detail UI

```text
CapaDetailPage
- CAPA header
- Source references
- Root cause
- Action plan
- Assigned actions
- Blockers
- Verification plan
- Effectiveness results
- Evidence
- Timeline
```
