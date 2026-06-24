# Compliance Core — Workflows, Status Logic, Events, and APIs

## Major workflow: product compliance evaluation

```text
1. Product gathers object facts and evidence refs.
2. Product calls Compliance Core evaluation endpoint.
3. Compliance Core resolves applicable rulepacks.
4. Compliance Core evaluates applicability.
5. Compliance Core applies exceptions and exemptions.
6. Compliance Core evaluates evidence requirements.
7. Compliance Core checks acceptable alternatives.
8. Compliance Core returns evaluation, requirement results, missing evidence, invalid evidence, warnings, and explanation.
9. Product stores ComplianceStatusSnapshot.
10. ReportArr consumes evaluation facts if needed.
```

## Major workflow: rulepack authoring

```text
1. NexArr platform-admin validated user creates rulepack.
2. Admin selects governing body, jurisdiction, source, and domain.
3. Admin creates citation references.
4. Admin creates requirements.
5. Admin defines applicability logic.
6. Admin defines evidence requirements.
7. Admin defines exceptions/exemptions.
8. Admin validates rulepack.
9. Reviewer approves.
10. Rulepack becomes active.
```

## Major workflow: evidence mapping

```text
1. RecordArr or product submits evidence record refs.
2. Compliance Core analyzes record metadata, OCR fields, aliases, and context.
3. Compliance Core creates mapping suggestions.
4. User confirms, changes, rejects, or skips suggestions.
5. Confirmed mappings become usable evidence.
6. Compliance evaluations use confirmed mappings.
```

## Major workflow: theoretical situation evaluation

```text
1. User opens TSE.
2. User selects situation type.
3. User answers controlled fields.
4. Compliance Core infers applicable rulepacks.
5. Compliance Core evaluates selected facts and evidence states.
6. Compliance Core returns likely compliant/noncompliant/warning/unknown.
7. User can export scenario report or save package.
```

## Major workflow: rulepack import

```text
1. User uploads import source.
2. Compliance Core parses import rows.
3. Compliance Core validates keys, citations, logic, evidence types, exceptions, and references.
4. Validation issues are shown.
5. Reviewer approves import.
6. Compliance Core creates/updates objects.
7. New rulepack/catalog version is activated when approved.
```

## Major workflow: audit package evaluation

```text
1. User selects audit scope in ReportArr or Compliance Core.
2. Compliance Core resolves applicable rulepacks and requirements.
3. RecordArr supplies evidence package.
4. Products supply operational object facts.
5. Compliance Core evaluates requirement coverage.
6. Missing/invalid evidence is returned.
7. ReportArr renders audit report.
8. RecordArr stores final audit package.
```

## Compliance Core emitted events

```text
compliancecore.governing_body.created
compliancecore.governing_body.updated

compliancecore.catalog.created
compliancecore.catalog.updated
compliancecore.catalog.entry_created
compliancecore.catalog.entry_updated

compliancecore.citation.created
compliancecore.citation.updated
compliancecore.citation.superseded

compliancecore.rulepack.created
compliancecore.rulepack.updated
compliancecore.rulepack.submitted_for_review
compliancecore.rulepack.approved
compliancecore.rulepack.activated
compliancecore.rulepack.superseded
compliancecore.rulepack.deprecated

compliancecore.requirement.created
compliancecore.requirement.updated
compliancecore.requirement.activated
compliancecore.requirement.superseded

compliancecore.applicability_rule.created
compliancecore.compliance_logic.created
compliancecore.exception.created
compliancecore.exemption.created
compliancecore.retention_rule.created

compliancecore.evidence_type.created
compliancecore.evidence_requirement.created
compliancecore.evidence_mapping.suggested
compliancecore.evidence_mapping.confirmed
compliancecore.evidence_mapping.rejected

compliancecore.evaluation.requested
compliancecore.evaluation.completed
compliancecore.evaluation.manual_review_required
compliancecore.evaluation.reviewed
compliancecore.status_snapshot.published

compliancecore.tse.created
compliancecore.tse.completed

compliancecore.import_batch.created
compliancecore.import_batch.parsed
compliancecore.import_batch.validation_failed
compliancecore.import_batch.ready_for_review
compliancecore.import_batch.imported

compliancecore.rulepack_diff.completed
compliancecore.change_impact.completed
```

## Integration APIs Compliance Core should expose

```text
GET /api/v1/catalogs
GET /api/v1/catalogs/{catalogKey}
GET /api/v1/catalogs/{catalogKey}/entries

GET /api/v1/governing-bodies
GET /api/v1/governing-bodies/{governingBodyId}
GET /api/v1/jurisdictions
GET /api/v1/regulation-sources
GET /api/v1/citations
GET /api/v1/citations/{citationId}

GET /api/v1/rulepacks
GET /api/v1/rulepacks/{rulepackId}
GET /api/v1/rulepacks/{rulepackId}/versions
POST /api/v1/rulepacks
POST /api/v1/rulepacks/{rulepackId}/submit-review
POST /api/v1/rulepacks/{rulepackId}/approve
POST /api/v1/rulepacks/{rulepackId}/activate

GET /api/v1/requirements
GET /api/v1/requirements/{requirementId}
POST /api/v1/requirements

GET /api/v1/evidence-types
GET /api/v1/evidence-requirements
POST /api/v1/evidence-mapping/suggest
POST /api/v1/evidence-mapping/confirm
POST /api/v1/evidence-mapping/reject

POST /api/v1/evaluations
GET /api/v1/evaluations/{evaluationId}
POST /api/v1/evaluations/{evaluationId}/review

POST /api/v1/tse/evaluate
GET /api/v1/tse/{tseId}

POST /api/v1/import-batches
GET /api/v1/import-batches/{importBatchId}
POST /api/v1/import-batches/{importBatchId}/validate
POST /api/v1/import-batches/{importBatchId}/approve
POST /api/v1/import-batches/{importBatchId}/import

POST /api/v1/rulepack-diffs
GET /api/v1/rulepack-diffs/{diffId}
POST /api/v1/change-impact-analyses
```

## Audit package API surface

```text
GET /api/v1/compliancecore/audit-packages/manifest
GET /api/v1/compliancecore/audit-packages/export
GET /api/v1/compliancecore/audit-packages/export/stream
POST /api/v1/compliancecore/audit-packages/jobs
GET /api/v1/compliancecore/audit-packages/jobs/{jobId}
GET /api/v1/compliancecore/audit-packages/jobs/{jobId}/download

GET /api/v1/compliancecore/audit-packages/events
POST /api/v1/compliancecore/audit-packages
GET /api/v1/compliancecore/audit-packages
GET /api/v1/compliancecore/audit-packages/{id}
GET /api/v1/compliancecore/audit-packages/{id}/download
GET /api/v1/compliancecore/events
```

## APIs Compliance Core should consume

```text
NexArr
- POST /api/v1/platform/handoff/redeem
- POST /api/v1/platform/service-tokens/introspect
- GET /api/v1/platform/session/context

StaffArr
- GET /persons/{personId}
- GET /locations/{locationId}
- GET /sites/{siteOrgUnitId}
- POST /permission-checks

TrainArr
- GET /qualification-definitions
- GET /persons/{personId}/qualifications

MaintainArr
- GET /assets/{assetId}
- GET /work-orders/{workOrderId}
- GET /inspections/{inspectionId}

LoadArr
- GET /inventory-items/{itemId}
- GET /receipts/{receiptId}
- GET /stock-movements/{movementId}

SupplyArr
- GET /suppliers/{supplierId}
- GET /purchase-orders/{purchaseOrderId}

RoutArr
- GET /trips/{tripId}
- GET /routes/{routeId}

CustomArr
- GET /customers/{customerId}
- GET /customer-locations/{locationId}

OrdArr
- GET /orders/{orderId}

RecordArr
- GET /records/{recordId}
- GET /record-packages/{packageId}

AssurArr
- GET /nonconformances/{nonconformanceId}
- GET /capas/{capaId}
- GET /holds/{holdId}

ReportArr
- POST /events
```

## Permission examples

```text
compliancecore.catalogs.read
compliancecore.catalogs.manage

compliancecore.governing_bodies.read
compliancecore.governing_bodies.manage

compliancecore.citations.read
compliancecore.citations.manage

compliancecore.rulepacks.read
compliancecore.rulepacks.create
compliancecore.rulepacks.update
compliancecore.rulepacks.review
compliancecore.rulepacks.approve
compliancecore.rulepacks.activate
compliancecore.rulepacks.deprecate

compliancecore.requirements.read
compliancecore.requirements.create
compliancecore.requirements.update
compliancecore.requirements.review

compliancecore.evidence_types.read
compliancecore.evidence_types.manage
compliancecore.evidence_mapping.suggest
compliancecore.evidence_mapping.confirm

compliancecore.evaluations.run
compliancecore.evaluations.read
compliancecore.evaluations.review

compliancecore.tse.run
compliancecore.tse.read

compliancecore.imports.create
compliancecore.imports.validate
compliancecore.imports.approve
compliancecore.imports.execute

compliancecore.admin
```

## Default role examples

```text
Compliance Viewer
- Read catalogs, rulepacks, requirements, citations, and evaluations.

Compliance Evaluator
- Run evaluations.
- Run TSE.
- Read evidence mapping suggestions.

Evidence Mapping Reviewer
- Confirm/reject evidence mappings.
- Review low-confidence evidence.

Rulepack Author
- Create and edit draft rulepacks, requirements, logic, and evidence rules.

Rulepack Reviewer
- Review rulepacks and request changes.

Compliance Approver
- Approve and activate rulepacks.

Compliance Admin
- Manage catalogs, governing bodies, citations, imports, rulepacks, and settings.

Auditor
- Read evaluations, evidence coverage, rulepack versions, and audit scope results.
```

## Compliance Core UI surfaces

```text
/app/compliancecore
- dashboard
- governing bodies
- jurisdictions
- regulation sources
- citations
- rulepacks
- rulepack detail
- requirements
- requirement detail
- applicability logic builder
- evidence types
- evidence requirements
- exceptions
- exemptions
- retention rules
- evidence mapping wizard
- evaluations
- theoretical situation evaluation
- imports
- rulepack diff
- change impact
- settings
```

## Rulepack detail UI

```text
RulepackDetailPage
- Header
  - rulepack key
  - title
  - version
  - status
  - governing bodies
  - effective date
- Citations
- Requirements
- Applicability rules
- Evidence requirements
- Exceptions/exemptions
- Retention rules
- Validation issues
- Version history
- Activation controls
```

## Requirement detail UI

```text
RequirementDetailPage
- Header
  - requirement key
  - title
  - type
  - severity
  - status
- Plain-language summary
- Citations
- Applicability logic
- Compliance logic
- Evidence requirements
- Acceptable alternatives
- Exceptions
- Exemptions
- Retention
- Related requirements
- Test evaluation panel
```

## TSE UI

```text
TheoreticalSituationEvaluationPage
- Situation type selector
- Controlled facts
- Evidence exists/does-not-exist/valid/invalid fields
- Optional class/category selectors
- Evaluation result
- Missing evidence
- Acceptable alternatives
- Exceptions/exemptions
- Edge cases
- Export/save action
```
