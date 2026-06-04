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
- Tenant entitlement
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
- Product entitlement
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


---


# AssurArr — Nonconformance and Quality Hold Model

## Nonconformance

A Nonconformance is a quality issue where something failed to meet a requirement, expectation, specification, procedure, customer requirement, supplier requirement, internal standard, or compliance obligation.

It can originate from receiving, inventory, maintenance, customer complaints, supplier issues, audits, route/delivery issues, training/process failures, or internal observation.

## Nonconformance shape

```text
Nonconformance
- nonconformanceId
- tenantId
- nonconformanceNumber
- title
- description
- nonconformanceType
  - receiving
  - supplier
  - customer_complaint
  - internal_process
  - maintenance
  - delivery
  - inventory
  - document
  - training
  - audit_finding
  - regulatory
  - safety_quality
  - product_service
  - other
- category
  - defect
  - damage
  - shortage
  - overage
  - wrong_item
  - expired
  - contamination
  - missing_document
  - invalid_document
  - process_failure
  - failed_inspection
  - failed_verification
  - customer_rejection
  - supplier_failure
  - repeat_issue
  - other
- severity
  - low
  - moderate
  - high
  - critical
- status
  - draft
  - open
  - containment
  - investigation
  - disposition_pending
  - corrective_action
  - verification
  - release_pending
  - closed
  - canceled
- sourceProduct
- sourceObjectRef
- discoveredAt
- discoveredByPersonId
- ownerPersonId
- staffarrSiteId
- staffarrLocationId
- affectedObjectRefs
- affectedItemRefs
- affectedAssetRefs
- affectedOrderRefs
- affectedSupplierRefs
- affectedCustomerRefs
- affectedShipmentRefs
- containmentRefs
- holdRefs
- dispositionRefs
- rootCauseRef
- capaRefs
- complianceRefs
- recordRefs
- customerImpact
- supplierImpact
- safetyImpact
- complianceImpact
- financialImpactSnapshot
- recurrenceFlag
- repeatOfNonconformanceRef
- dueAt
- closedAt
- closedByPersonId
- closureSummary
- auditTrail
```

## Nonconformance status definitions

```text
draft
- Nonconformance is being drafted and has not been formally opened.

open
- Nonconformance exists and requires review.

containment
- Immediate containment actions are being performed.

investigation
- Root cause and impact are being investigated.

disposition_pending
- A decision is needed on what to do with affected objects.

corrective_action
- CAPA/action plan is active.

verification
- Corrective/preventive action effectiveness is being verified.

release_pending
- Release or closure review is pending.

closed
- Required actions are complete and the case is closed.

canceled
- Created in error, duplicate, or invalid.
```

## Nonconformance source examples

```text
LoadArr
- Receiving damaged item
- Wrong item received
- Count variance
- Expired stock found
- Pick discrepancy
- Quarantine need

MaintainArr
- Failed repair verification
- Repeat defect
- Unsafe maintenance process
- Asset return-to-service concern
- Wrong part installed

RoutArr
- Damaged freight
- Customer refused delivery
- Delivery documentation issue
- Temperature/control issue if applicable

SupplyArr
- Supplier document expired
- Supplier noncompliance
- Supplier quality failure

CustomArr
- Customer complaint
- Customer rejection
- Customer-specific requirement failure

OrdArr
- Fulfillment quality block
- Order cannot close due to quality issue

StaffArr
- Personnel/process issue requiring quality action

Compliance Core
- Requirement/evidence failure that needs quality workflow
```

## Quality hold

A QualityHold blocks or restricts an object until AssurArr releases it.

## QualityHold shape

```text
QualityHold
- holdId
- tenantId
- holdNumber
- title
- description
- holdType
  - inventory
  - supplier
  - customer_order
  - asset
  - shipment
  - route
  - document
  - training
  - person_process
  - location
  - work_order
  - purchase_order
  - other
- status
  - draft
  - active
  - release_pending
  - released
  - rejected
  - canceled
  - expired
- severity
  - low
  - moderate
  - high
  - critical
- sourceNonconformanceRef
- sourceProduct
- sourceObjectRef
- affectedObjectRefs
- holdReason
- holdScope
  - full
  - partial
  - conditional
  - informational
- quantityHeld
- unitOfMeasure
- lotNumber
- serialNumber
- staffarrSiteId
- staffarrLocationId
- placedAt
- placedByPersonId
- ownerPersonId
- releaseRequirements
- releaseApprovalRefs
- releasedAt
- releasedByPersonId
- releaseReason
- rejectedAt
- rejectedByPersonId
- rejectionReason
- conditionalReleaseTerms
- expiresAt
- recordRefs
- auditTrail
```

## Hold status definitions

```text
draft
- Hold is prepared but not active.

active
- Hold is active and target products must block affected actions.

release_pending
- Release was requested and requires review/approval.

released
- Hold no longer blocks affected objects.

rejected
- Release request was rejected or affected object was rejected.

canceled
- Hold was canceled because it was invalid or no longer needed.

expired
- Time-limited hold expired according to policy.
```

## Hold effect by target product

```text
LoadArr inventory hold
- Blocks pick, issue, transfer, shipping, consumption, or adjustment except allowed disposition moves.
- May allow transfer to quarantine or inspection hold.

MaintainArr asset hold
- Blocks return-to-service.
- May block work order closeout.
- May require inspection, repair, or quality verification.

OrdArr order hold
- Blocks fulfillment or closure.
- Adds order blocker.

RoutArr shipment/route hold
- Blocks dispatch, departure, delivery, or release depending on scope.

SupplyArr supplier hold
- Blocks supplier selection, PO approval, or receipt acceptance depending on scope.

RecordArr document hold
- Blocks document acceptance/use as evidence.

StaffArr person/process hold
- May create restriction, incident, or training review.

TrainArr training hold
- May block qualification use pending review.

CustomArr customer-related hold
- Flags customer requirement failure or complaint workflow.
```

## Containment action

Containment is the immediate action used to stop spread, prevent use, isolate issue, or protect customers/people/assets.

```text
ContainmentAction
- containmentActionId
- tenantId
- nonconformanceId
- title
- description
- actionType
  - isolate
  - quarantine
  - stop_ship
  - stop_use
  - notify_customer
  - notify_supplier
  - inspect_all
  - sort
  - retrain
  - repair
  - rework
  - block_order
  - block_supplier
  - block_asset
  - hold_inventory
  - other
- assignedPersonId
- assignedTeamRef
- sourceProductActionRef
- status
  - open
  - assigned
  - in_progress
  - completed
  - verified
  - canceled
- dueAt
- startedAt
- completedAt
- completedByPersonId
- verificationRequired
- verifiedByPersonId
- verifiedAt
- evidenceRecordRefs
- notes
```

## Disposition

Disposition is the decision for what happens to the affected object.

```text
Disposition
- dispositionId
- tenantId
- nonconformanceId
- dispositionNumber
- dispositionType
  - use_as_is
  - rework
  - repair
  - return_to_supplier
  - scrap
  - sort
  - regrade
  - reject
  - replace
  - conditional_release
  - release_no_action
- status
  - proposed
  - pending_approval
  - approved
  - executed
  - rejected
  - canceled
- affectedObjectRefs
- decisionByPersonId
- decisionAt
- approvedByPersonId
- approvedAt
- rationale
- requiredActions
- executionProduct
  - loadarr
  - maintainarr
  - supplyarr
  - routarr
  - ordarr
  - recordarr
  - staffarr
  - manual
- executionObjectRef
- evidenceRecordRefs
- notes
```

## Root cause

```text
RootCauseAnalysis
- rootCauseId
- tenantId
- nonconformanceId
- method
  - five_whys
  - fishbone
  - fault_tree
  - manual
  - unknown
- status
  - not_started
  - in_progress
  - completed
  - inconclusive
- primaryCauseCategory
  - people
  - process
  - equipment
  - material
  - environment
  - supplier
  - customer
  - documentation
  - training
  - system
  - unknown
- rootCauseSummary
- contributingFactors
- analyzedByPersonId
- completedAt
- evidenceRecordRefs
```

## Nonconformance workflow

```text
1. Source product reports quality issue or user creates nonconformance manually.
2. AssurArr creates Nonconformance.
3. AssurArr classifies type, category, severity, and affected objects.
4. AssurArr creates QualityHold if immediate block is required.
5. AssurArr creates ContainmentActions.
6. Investigation/root cause begins.
7. Disposition is proposed and approved.
8. Execution product performs disposition action.
9. CAPA is created if required.
10. Effectiveness is verified.
11. Holds are released or affected objects are rejected/scrapped.
12. Nonconformance closes.
```

## Hold release workflow

```text
1. Release is requested.
2. AssurArr checks release requirements.
3. Required evidence is verified.
4. Required approvals are completed.
5. AssurArr releases or rejects release request.
6. Target products receive hold released event.
7. Target products unblock affected actions.
```


---


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


---


# AssurArr — Audit, Finding, and Complaint Quality Model

## Quality audit

A QualityAudit is an inspection/review of a process, supplier, product, service, location, document set, or system to determine whether requirements are being met.

```text
QualityAudit
- auditId
- tenantId
- auditNumber
- title
- description
- auditType
  - internal
  - supplier
  - process
  - product
  - service
  - customer
  - compliance
  - document
  - location
  - system
- status
  - draft
  - planned
  - in_progress
  - findings_review
  - corrective_action
  - verification
  - closed
  - canceled
- auditScope
- standardRefs
- complianceRefs
- auditorPersonIds
- leadAuditorPersonId
- auditeeRefs
- staffarrSiteId
- staffarrLocationId
- supplierRef
- customerRef
- plannedStartAt
- plannedEndAt
- actualStartAt
- actualEndAt
- findingRefs
- checklistRefs
- recordRefs
- closedAt
- closedByPersonId
- auditTrail
```

## Audit checklist

```text
QualityAuditChecklist
- checklistId
- auditId
- title
- description
- status
  - draft
  - active
  - completed
  - canceled
- itemRefs
```

## Audit checklist item

```text
QualityAuditChecklistItem
- itemId
- checklistId
- sequence
- prompt
- helpText
- requirementRef
- responseType
  - pass_fail
  - yes_no
  - numeric
  - text
  - select
  - multi_select
  - photo
  - document
- required
- responseValue
- result
  - pass
  - fail
  - observation
  - not_applicable
- findingCreated
- findingRef
- evidenceRecordRefs
- answeredAt
- answeredByPersonId
```

## Audit finding

An AuditFinding is an issue, observation, or improvement opportunity discovered during an audit.

```text
AuditFinding
- findingId
- tenantId
- findingNumber
- auditId
- title
- description
- findingType
  - observation
  - opportunity_for_improvement
  - minor_nonconformance
  - major_nonconformance
  - critical_nonconformance
  - positive_practice
- severity
  - low
  - moderate
  - high
  - critical
- status
  - open
  - accepted
  - disputed
  - nonconformance_created
  - corrective_action
  - verified
  - closed
  - canceled
- sourceRequirementRef
- affectedObjectRefs
- ownerPersonId
- dueAt
- nonconformanceRef
- capaRef
- evidenceRecordRefs
- closureSummary
- closedAt
- closedByPersonId
```

## Customer complaint quality case

CustomArr owns the customer. AssurArr owns the complaint quality workflow when a complaint indicates a quality, service, delivery, product, process, or documentation failure.

```text
CustomerComplaintQualityCase
- complaintCaseId
- tenantId
- complaintNumber
- customerRef
- customerContactSnapshot
- customerLocationRef
- title
- description
- complaintType
  - product_quality
  - service_quality
  - delivery_quality
  - documentation
  - damaged_goods
  - wrong_item
  - late_delivery_quality_impact
  - failed_requirement
  - repeat_issue
  - other
- severity
  - low
  - moderate
  - high
  - critical
- status
  - received
  - triage
  - investigating
  - containment
  - response_pending
  - corrective_action
  - resolved
  - closed
  - canceled
- receivedAt
- receivedByPersonId
- sourceProduct
- sourceObjectRef
- affectedOrderRefs
- affectedShipmentRefs
- affectedItemRefs
- affectedAssetRefs
- nonconformanceRef
- holdRefs
- capaRefs
- customerResponseDueAt
- customerResponseRecordRefs
- recordRefs
- closedAt
- closedByPersonId
```

## Supplier quality issue

SupplyArr owns the supplier. AssurArr owns the supplier quality issue workflow.

```text
SupplierQualityIssue
- supplierQualityIssueId
- tenantId
- issueNumber
- supplierRef
- title
- description
- issueType
  - damaged_received
  - wrong_item
  - late_with_quality_impact
  - missing_document
  - invalid_document
  - failed_specification
  - recurring_defect
  - packaging_failure
  - labeling_failure
  - other
- severity
  - low
  - moderate
  - high
  - critical
- status
  - open
  - supplier_notified
  - response_pending
  - under_review
  - corrective_action
  - resolved
  - closed
  - canceled
- sourceProduct
- sourceObjectRef
- affectedReceiptRefs
- affectedPurchaseOrderRefs
- affectedItemRefs
- nonconformanceRef
- scarRef
- holdRefs
- recordRefs
- openedAt
- closedAt
```

## Quality review

QualityReview is a formal review step for accepting/rejecting evidence, release, disposition, or closure.

```text
QualityReview
- reviewId
- tenantId
- reviewNumber
- reviewType
  - nonconformance_review
  - hold_release
  - disposition_review
  - capa_review
  - audit_finding_review
  - supplier_response_review
  - customer_response_review
  - document_quality_review
- status
  - pending
  - in_review
  - approved
  - rejected
  - changes_requested
  - canceled
- sourceProduct
- sourceObjectRef
- reviewerPersonId
- requestedAt
- dueAt
- decisionAt
- decisionReason
- requiredEvidenceRefs
- submittedEvidenceRefs
- notes
```

## Quality release

A QualityRelease is the explicit release decision that allows a held object to continue through its source workflow.

```text
QualityRelease
- releaseId
- tenantId
- releaseNumber
- holdRef
- releaseType
  - full
  - partial
  - conditional
  - use_as_is
  - release_after_rework
  - release_after_sort
- status
  - requested
  - pending_review
  - approved
  - rejected
  - executed
  - canceled
- affectedObjectRefs
- requestedByPersonId
- requestedAt
- approvedByPersonId
- approvedAt
- executedAt
- conditions
- expirationAt
- evidenceRecordRefs
- notes
```

## Audit workflow

```text
1. User creates QualityAudit.
2. Scope, requirements, auditors, and auditees are defined.
3. Audit checklist is prepared.
4. Audit is executed.
5. Findings are created.
6. Findings may create Nonconformance and/or CAPA.
7. Corrective actions are completed.
8. Verification occurs.
9. Audit closes.
```

## Customer complaint workflow

```text
1. CustomArr or user creates customer complaint quality case.
2. AssurArr triages severity and affected objects.
3. AssurArr creates Nonconformance if required.
4. AssurArr places holds if needed.
5. Investigation/root cause begins.
6. Customer response is prepared.
7. CAPA is created if systemic issue exists.
8. Customer response/evidence is stored in RecordArr.
9. Case is resolved and closed.
10. CustomArr receives customer activity/status update.
```

## Supplier quality workflow

```text
1. LoadArr/SupplyArr/user reports supplier issue.
2. AssurArr creates SupplierQualityIssue.
3. AssurArr creates Nonconformance if needed.
4. Inventory hold may be placed.
5. Supplier is notified through SupplyArr context.
6. SCAR is created if required.
7. Supplier response is reviewed.
8. Supplier performance/status is updated through SupplyArr.
9. Issue is closed after disposition/CAPA.
```

## Events

```text
assurarr.audit.created
assurarr.audit.started
assurarr.audit.finding_created
assurarr.audit.closed

assurarr.finding.created
assurarr.finding.accepted
assurarr.finding.nonconformance_created
assurarr.finding.closed

assurarr.customer_complaint.created
assurarr.customer_complaint.status_changed
assurarr.customer_complaint.response_sent
assurarr.customer_complaint.closed

assurarr.supplier_quality_issue.created
assurarr.supplier_quality_issue.status_changed
assurarr.supplier_quality_issue.closed

assurarr.quality_review.requested
assurarr.quality_review.approved
assurarr.quality_review.rejected

assurarr.quality_release.requested
assurarr.quality_release.approved
assurarr.quality_release.rejected
assurarr.quality_release.executed
```


---


# AssurArr — Quality Status, Scorecard, and Metrics Model

## Quality status snapshot

QualityStatusSnapshot allows AssurArr to publish current quality state for an object owned elsewhere.

Examples:

- Supplier quality status for SupplyArr
- Inventory quality status for LoadArr
- Asset quality status for MaintainArr
- Order quality status for OrdArr
- Customer complaint status for CustomArr
- Document quality status for RecordArr

```text
QualityStatusSnapshot
- qualityStatusSnapshotId
- tenantId
- targetProduct
- targetObjectRef
- qualityStatus
  - acceptable
  - warning
  - on_hold
  - rejected
  - conditional_release
  - under_review
  - unknown
- severity
  - none
  - low
  - moderate
  - high
  - critical
- activeHoldRefs
- openNonconformanceRefs
- openCapaRefs
- openFindingRefs
- lastReviewedAt
- reviewedByPersonId
- expiresAt
- notes
```

## Quality scorecard

QualityScorecard summarizes quality performance for a supplier, customer, process, site, department, asset class, or other target.

```text
QualityScorecard
- scorecardId
- tenantId
- scorecardNumber
- targetType
  - supplier
  - customer
  - site
  - department
  - process
  - asset_class
  - inventory_item
  - product_service
  - route_lane
  - other
- targetRef
- periodStart
- periodEnd
- status
  - draft
  - active
  - finalized
  - archived
- overallScore
- qualityStatus
  - excellent
  - acceptable
  - warning
  - poor
  - blocked
  - unknown
- metricRefs
- trend
  - improving
  - stable
  - worsening
  - unknown
- generatedAt
- generatedBy
  - system
  - person
- reviewedByPersonId
- reviewedAt
```

## Quality metric

```text
QualityMetric
- metricId
- scorecardId
- metricKey
- title
- description
- category
  - nonconformance
  - hold
  - capa
  - audit
  - supplier
  - customer
  - delivery
  - inventory
  - maintenance
  - documentation
- value
- numerator
- denominator
- unit
- targetValue
- warningThreshold
- criticalThreshold
- status
  - good
  - warning
  - critical
  - unknown
- sourceProductRefs
```

## Common quality metrics

```text
Nonconformance
- open nonconformance count
- nonconformance aging
- repeat nonconformance count
- critical nonconformance count
- time to containment
- time to disposition
- time to closure

Hold
- active hold count
- hold aging
- inventory quantity on hold
- order hold count
- asset hold count
- release cycle time

CAPA
- open CAPA count
- overdue CAPA count
- CAPA aging
- ineffective CAPA count
- CAPA recurrence rate
- action completion rate

Supplier
- supplier quality issue count
- damaged receipt rate
- wrong item receipt rate
- supplier response time
- SCAR acceptance rate
- supplier repeat issue rate

Customer
- complaint count
- complaint response time
- complaint closure time
- repeat complaint rate
- customer rejection rate

Audit
- findings count
- major findings count
- repeat findings
- audit closure time
- finding closure time

Maintenance quality
- repeat repair count
- failed return-to-service count
- maintenance rework rate
- asset quality hold count

Inventory quality
- quarantine quantity
- expired stock count
- count-related quality issue count
- receiving discrepancy quality rate
```

## Quality risk profile

```text
QualityRiskProfile
- riskProfileId
- tenantId
- targetType
  - supplier
  - customer
  - process
  - site
  - asset
  - inventory_item
  - order
  - route
- targetRef
- riskLevel
  - low
  - moderate
  - high
  - critical
  - unknown
- riskFactors
- openIssueCount
- repeatIssueCount
- criticalIssueCount
- lastIncidentAt
- mitigationActions
- reviewedAt
- reviewedByPersonId
```

## Quality dashboard cards

```text
QualityDashboard
- Open nonconformances
- Critical nonconformances
- Active holds
- Hold aging
- Open CAPAs
- Overdue CAPAs
- CAPA effectiveness
- Supplier quality issues
- Customer complaint cases
- Audit findings
- Repeat issues
- Recently released holds
- Quality risk by site
- Quality risk by supplier
- Quality risk by process
```

## Quality status publishing workflow

```text
1. AssurArr creates or updates quality issue.
2. AssurArr recalculates target quality status.
3. AssurArr publishes QualityStatusSnapshot.
4. Target product consumes snapshot/event.
5. Target product blocks, warns, or allows workflow based on quality status and local rules.
6. ReportArr consumes quality facts for analytics.
```

## Supplier scorecard workflow

```text
1. AssurArr collects supplier quality issues, SCARs, holds, and nonconformances.
2. SupplyArr provides supplier context.
3. LoadArr provides receipt/discrepancy facts.
4. AssurArr calculates supplier quality scorecard.
5. SupplyArr consumes quality status/score for supplier decision support.
6. ReportArr displays supplier quality trends.
```

## Customer quality score workflow

```text
1. AssurArr collects customer complaint quality cases.
2. CustomArr provides customer context.
3. OrdArr/RoutArr/LoadArr provide fulfillment/delivery context.
4. AssurArr calculates customer quality metrics.
5. CustomArr receives quality activity/status.
6. ReportArr displays customer quality trends.
```

## Events

```text
assurarr.quality_status.changed
assurarr.quality_status.published
assurarr.scorecard.generated
assurarr.scorecard.reviewed
assurarr.risk_profile.updated
assurarr.metric.calculated
```


---


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
- POST /handoff/redeem
- POST /service-tokens/introspect
- GET /entitlements/{productKey}

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
