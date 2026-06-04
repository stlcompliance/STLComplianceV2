# AssurArr — CAPA and Action Model

## CAPA

CAPA means corrective action and preventive action. It is used when a quality issue requires systemic correction, not just one-time containment.

A CAPA can originate from a nonconformance, audit finding, supplier issue, customer complaint, repeated defect, compliance issue, or management review.

## CAPA shape

```text
Capa
- capaId
- tenantId
- capaNumber
- title
- description
- capaType
  - corrective
  - preventive
  - corrective_and_preventive
- sourceType
  - nonconformance
  - audit_finding
  - customer_complaint
  - supplier_issue
  - repeated_defect
  - compliance_gap
  - management_review
  - trend
  - manual
- sourceRefs
- severity
  - low
  - moderate
  - high
  - critical
- status
  - draft
  - open
  - root_cause
  - action_plan
  - implementation
  - verification
  - effective
  - ineffective
  - closed
  - canceled
- ownerPersonId
- sponsorPersonId
- staffarrSiteId
- staffarrLocationId
- rootCauseRef
- actionPlanRefs
- verificationPlanRef
- effectivenessVerificationRefs
- relatedNonconformanceRefs
- relatedAuditFindingRefs
- relatedCustomerComplaintRefs
- relatedSupplierIssueRefs
- complianceRefs
- recordRefs
- dueAt
- openedAt
- closedAt
- closedByPersonId
- closureSummary
- auditTrail
```

## CAPA status definitions

```text
draft
- CAPA is being drafted.

open
- CAPA has been opened and assigned.

root_cause
- Root cause analysis is required or in progress.

action_plan
- Corrective/preventive action plan is being defined or approved.

implementation
- Actions are being executed.

verification
- Effectiveness verification is active.

effective
- CAPA was verified effective.

ineffective
- CAPA failed effectiveness verification and needs revision/reopen.

closed
- CAPA is administratively closed.

canceled
- CAPA was canceled or deemed unnecessary.
```

## CAPA action

A CAPA action is a specific work item required to correct or prevent the issue.

```text
CapaAction
- capaActionId
- tenantId
- capaId
- actionNumber
- title
- description
- actionType
  - update_procedure
  - retrain_personnel
  - repair_asset
  - change_supplier
  - update_inspection
  - update_pm
  - update_work_instruction
  - update_document
  - quarantine_inventory
  - rework_inventory
  - customer_notification
  - supplier_corrective_action
  - system_change
  - process_change
  - audit_followup
  - other
- status
  - open
  - assigned
  - in_progress
  - blocked
  - completed
  - verified
  - rejected
  - canceled
- assignedPersonId
- assignedTeamRef
- sourceProductActionRef
- targetProduct
  - staffarr
  - trainarr
  - maintainarr
  - loadarr
  - supplyarr
  - routarr
  - customarr
  - ordarr
  - recordarr
  - compliancecore
  - manual
- targetObjectRef
- dueAt
- startedAt
- completedAt
- completedByPersonId
- verificationRequired
- verifiedAt
- verifiedByPersonId
- evidenceRecordRefs
- blockerRefs
- notes
```

## CAPA action blocker

```text
CapaActionBlocker
- blockerId
- capaActionId
- blockerType
  - missing_approval
  - missing_evidence
  - waiting_training
  - waiting_maintenance
  - waiting_supplier
  - waiting_customer
  - waiting_inventory
  - waiting_document
  - system
  - other
- sourceProduct
- sourceObjectRef
- title
- description
- status
  - active
  - resolved
  - overridden
- createdAt
- resolvedAt
- resolvedByPersonId
```

## Verification plan

```text
VerificationPlan
- verificationPlanId
- capaId
- title
- description
- verificationType
  - observation
  - audit
  - inspection
  - trend_review
  - sample_review
  - customer_confirmation
  - supplier_confirmation
  - document_review
  - training_completion_review
- successCriteria
- sampleSize
- observationPeriodDays
- requiredEvidenceTypes
- responsiblePersonId
- plannedVerificationAt
- status
  - draft
  - approved
  - active
  - completed
  - canceled
```

## Effectiveness verification

```text
EffectivenessVerification
- verificationId
- capaId
- verificationPlanId
- status
  - scheduled
  - in_progress
  - effective
  - ineffective
  - inconclusive
  - canceled
- performedByPersonId
- performedAt
- resultSummary
- evidenceRecordRefs
- metricResults
- recurrenceFound
- followUpRequired
- reopenedCapaRef
```

## Supplier corrective action request

A supplier corrective action request is a CAPA-style request sent to or tracked against a supplier. SupplyArr owns the supplier master; AssurArr owns the quality action.

```text
SupplierCorrectiveActionRequest
- scarId
- tenantId
- scarNumber
- supplierRef
- sourceNonconformanceRef
- sourceCapaRef
- title
- description
- severity
  - low
  - moderate
  - high
  - critical
- status
  - draft
  - sent
  - acknowledged
  - supplier_response_pending
  - response_received
  - under_review
  - accepted
  - rejected
  - closed
  - canceled
- requestedAt
- requestedByPersonId
- supplierDueAt
- supplierResponseRecordRefs
- reviewPersonId
- reviewedAt
- reviewDecision
- followUpCapaRef
- recordRefs
```

## CAPA workflow

```text
1. AssurArr determines CAPA is required.
2. CAPA is opened and assigned.
3. Root cause analysis is completed.
4. Action plan is created.
5. Action plan is approved if required.
6. CAPA actions are assigned to people/products.
7. Actions are completed with evidence.
8. Verification plan begins.
9. Effectiveness is verified.
10. If effective, CAPA closes.
11. If ineffective, CAPA is revised or reopened.
```

## CAPA action routing examples

```text
Training issue
- AssurArr CAPA action targets TrainArr remediation assignment.
- TrainArr owns training execution.

Maintenance process issue
- AssurArr CAPA action targets MaintainArr PM/inspection/work-order process update.
- MaintainArr owns maintenance implementation.

Inventory handling issue
- AssurArr CAPA action targets LoadArr receiving/putaway/count workflow.
- LoadArr owns WMS execution.

Supplier issue
- AssurArr creates SCAR.
- SupplyArr owns supplier master/status impact.
- AssurArr owns supplier quality action and response review.

Document/procedure issue
- AssurArr action targets RecordArr controlled document update.
- RecordArr owns document lifecycle.

Personnel behavior issue
- AssurArr action targets StaffArr personnel incident/restriction.
- StaffArr owns people incident.
```

## CAPA events

```text
assurarr.capa.created
assurarr.capa.status_changed
assurarr.capa.root_cause_completed
assurarr.capa.action_plan_created
assurarr.capa.action_assigned
assurarr.capa.action_completed
assurarr.capa.action_verified
assurarr.capa.verification_started
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
```
