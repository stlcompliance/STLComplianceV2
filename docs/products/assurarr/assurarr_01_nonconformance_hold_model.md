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
