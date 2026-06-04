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
