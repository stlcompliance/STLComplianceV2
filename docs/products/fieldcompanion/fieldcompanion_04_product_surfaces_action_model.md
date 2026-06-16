# Field Companion — Product Surfaces and Action Model

## Product surface rule

Field Companion should not simply mirror every product admin screen. It should expose field-appropriate actions.

The default mobile layout should be:

```text
- My Work
- Scan
- Report
- Product surfaces
- Offline queue
- Profile/readiness
```

## MaintainArr mobile surface

```text
MaintainArrSurface
- assigned work orders
- assigned work order tasks
- asset lookup by scan/search
- inspection execution
- defect reporting
- meter reading entry
- labor start/stop
- part request
- part usage/install confirmation
- photo evidence
- return-to-service checklist where permitted
```

### MaintainArr mobile actions

```text
maintainarr.work_order.start
maintainarr.work_order.pause
maintainarr.work_order.resume
maintainarr.work_order.complete_task
maintainarr.work_order.add_note
maintainarr.work_order.record_labor
maintainarr.work_order.request_part
maintainarr.work_order.record_part_usage
maintainarr.work_order.upload_evidence
maintainarr.inspection.start
maintainarr.inspection.answer_item
maintainarr.inspection.pause
maintainarr.inspection.complete
maintainarr.defect.report
maintainarr.meter_reading.record
maintainarr.asset.scan_lookup
```

## TrainArr mobile surface

```text
TrainArrSurface
- my training assignments
- training steps
- trainee acknowledgement
- trainer signoff
- evaluator signoff
- practical evaluation checklist
- evidence upload
- remediation tasks
```

### TrainArr mobile actions

```text
trainarr.assignment.start
trainarr.step.complete
trainarr.step.upload_evidence
trainarr.trainee.acknowledge
trainarr.trainer.signoff
trainarr.evaluator.signoff
trainarr.evaluation.pass
trainarr.evaluation.fail
```

## RoutArr mobile surface

```text
RoutArrSurface
- assigned trips
- route stops
- stop instructions
- arrive/depart
- pickup proof
- delivery proof
- route exceptions
- BOL/POD upload
- customer signature
- damage photos
```

### RoutArr mobile actions

```text
routarr.trip.start
routarr.trip.complete
routarr.stop.arrive
routarr.stop.depart
routarr.proof.pickup_capture
routarr.proof.delivery_capture
routarr.exception.report
routarr.document.upload_bol
routarr.document.upload_pod
```

## LoadArr mobile surface

```text
LoadArrSurface
- receiving tasks
- putaway tasks
- pick tasks
- issue tasks
- transfer tasks
- cycle counts
- item/location scans
- discrepancy reporting
- quarantine/hold visibility
```

### LoadArr mobile actions

```text
loadarr.receiving.start
loadarr.receiving.scan_item
loadarr.receiving.confirm_line
loadarr.receiving.report_discrepancy
loadarr.putaway.start
loadarr.putaway.confirm
loadarr.pick.start
loadarr.pick.confirm
loadarr.issue.confirm
loadarr.transfer.confirm
loadarr.count.record
loadarr.location.scan
loadarr.item.scan
```

## StaffArr mobile surface

```text
StaffArrSurface
- my profile
- my readiness
- my permissions snapshot
- my team/direct reports if supervisor
- incident reporting
- approval tasks
- restriction/blocker visibility
```

### StaffArr mobile actions

```text
staffarr.incident.report
staffarr.approval.decide
staffarr.readiness.view
staffarr.person.scan_lookup
staffarr.location.scan_lookup
```

## AssurArr mobile surface

```text
AssurArrSurface
- assigned containment actions
- assigned CAPA actions
- quality audit checklist
- nonconformance evidence capture
- hold/release evidence
- quality photos
```

### AssurArr mobile actions

```text
assurarr.nonconformance.create
assurarr.containment.start
assurarr.containment.complete
assurarr.capa_action.complete
assurarr.audit.answer_item
assurarr.quality_evidence.upload
assurarr.hold_release.request
```

## RecordArr mobile surface

```text
RecordArrSurface
- document upload
- document scan
- photo capture
- signature capture
- evidence package contribution
- upload status
```

### RecordArr mobile actions

```text
recordarr.record.upload
recordarr.document.scan
recordarr.photo.capture
recordarr.signature.capture
recordarr.evidence.submit
```

## SupplyArr mobile surface

```text
SupplyArrSurface
- supplier document upload
- supplier receiving evidence where allowed
- purchase request evidence
- vendor work evidence
```

### SupplyArr mobile actions

```text
supplyarr.supplier_document.upload
supplyarr.purchase_request.evidence_upload
supplyarr.vendor_contact.capture
```

## OrdArr mobile surface

```text
OrdArrSurface
- assigned order tasks
- fulfillment evidence
- customer acceptance where permitted
- order blocker visibility
```

### OrdArr mobile actions

```text
ordarr.order_task.complete
ordarr.fulfillment_evidence.capture
ordarr.customer_acceptance.capture
ordarr.blocker.view
```

## CustomArr mobile surface

```text
CustomArrSurface
- customer location details
- customer contact details
- customer issue capture
- customer signature/upload where permitted
```

### CustomArr mobile actions

```text
customarr.customer_location.view
customarr.customer_issue.create
customarr.customer_signature.capture
customarr.customer_document.upload
```

## Compliance Core mobile surface

Compliance Core should not become a general legal research UI in Field Companion. It may expose controlled prompts where needed.

```text
ComplianceCoreSurface
- controlled compliance prompts
- evidence requirement prompts
- field-safe TSE input where permitted
- requirement explanation snippets
```

### Compliance Core mobile actions

```text
compliancecore.evidence_requirement.view
compliancecore.controlled_prompt.answer
compliancecore.tse.mobile_evaluate_limited
```

## Mobile action submission

```text
MobileActionSubmission
- submissionId
- tenantId
- mobileTaskId
- mobileSessionId
- personId
- deviceId
- sourceProduct
- sourceObjectRef
- actionKey
- payload
- evidenceArtifactRefs
- status
  - draft
  - submitted
  - accepted
  - rejected
  - conflict
  - failed
- submittedAt
- acceptedAt
- rejectedAt
- rejectionReason
- sourceProductResponse
- idempotencyKey
```

## Mobile validation result

```text
MobileValidationResult
- validationResultId
- submissionId
- sourceProduct
- status
  - valid
  - warning
  - invalid
  - blocked
- messages
- fieldErrors
- blockers
- requiredActions
```

## Human-friendly blocker

```text
MobileBlockerDisplay
- blockerId
- sourceProduct
- sourceObjectRef
- blockerType
  - permission
  - qualification
  - assignment
  - safety
  - quality
  - inventory
  - compliance
  - document
  - sync
  - expired
  - unknown
- title
- plainLanguageMessage
- canResolveHere
- resolutionAction
- escalationTarget
```

## Mobile UX rules

```text
1. Default to task-based UI, not product admin UI.
2. A worker should not need to know which product owns the backend object.
3. Product ownership should remain clear in metadata and audit.
4. Blockers must be plain language.
5. Critical actions need confirmation.
6. Evidence requirements should be shown before submission.
7. Offline eligibility should be obvious.
8. Failed sync should be impossible to miss.
9. No-login users should see only the requested upload/signature action.
```
