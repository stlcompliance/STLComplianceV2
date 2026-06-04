# RecordArr — Evidence Mapping, Package, Retention, and Legal Hold Model

## Evidence mapping

EvidenceMapping links a RecordArr record to a Compliance Core requirement, source object, or evidence type.

Compliance Core owns the meaning of the requirement. RecordArr owns the mapping record and evidence file reference.

```text
EvidenceMapping
- evidenceMappingId
- tenantId
- recordId
- sourceProduct
- sourceObjectRef
- complianceCoreRequirementRef
- complianceCoreEvidenceTypeRef
- evidenceTypeKey
- mappingSource
  - compliancecore_suggestion
  - user_confirmed
  - product_asserted
  - import
  - system
- status
  - suggested
  - confirmed
  - rejected
  - superseded
  - expired
- confidenceScore
- confirmedByPersonId
- confirmedAt
- rejectedByPersonId
- rejectedAt
- rejectionReason
- notes
```

## Evidence coverage

```text
EvidenceCoverage
- evidenceCoverageId
- tenantId
- sourceProduct
- sourceObjectRef
- complianceCoreRequirementRef
- status
  - satisfied
  - missing
  - invalid
  - expired
  - warning
  - not_applicable
  - unknown
- recordRefs
- missingEvidenceTypes
- invalidRecordRefs
- evaluatedAt
- evaluationRef
```

## Record package

A RecordPackage is an assembled package of records and manifests. Examples include audit package, work order closeout package, training completion package, receiving package, CAPA package, and customer order package.

```text
RecordPackage
- packageId
- tenantId
- packageNumber
- title
- description
- packageType
  - audit
  - work_order_closeout
  - training_completion
  - receiving
  - delivery
  - quality
  - capa
  - customer
  - supplier
  - compliance
  - incident
  - person_audit
  - report_output
  - custom
- status
  - draft
  - assembling
  - complete
  - locked
  - archived
  - failed
  - canceled
- sourceProduct
- sourceObjectRefs
- recordRefs
- manifestRef
- generatedPdfRecordRef
- generatedZipFileRef
- requestedByPersonId
- createdAt
- completedAt
- lockedAt
- archivedAt
- expiresAt
- auditTrail
```

## Package manifest

```text
PackageManifest
- manifestId
- packageId
- manifestVersion
- generatedAt
- recordEntries
- sourceObjectEntries
- requirementEntries
- checksum
- generatedByPersonId
```

## Package manifest entry

```text
PackageManifestEntry
- manifestEntryId
- manifestId
- entryType
  - record
  - source_object
  - requirement
  - evaluation
  - note
- displayName
- sourceProduct
- sourceObjectRef
- recordRef
- complianceRequirementRef
- statusSnapshot
- checksum
```

## Retention policy

A RetentionPolicy defines how long records should be retained and what should happen when retention expires.

```text
RetentionPolicy
- retentionPolicyId
- tenantId
- policyKey
- title
- description
- recordTypeApplicability
- documentTypeApplicability
- sourceProductApplicability
- complianceCoreRetentionRuleRefs
- retainFor
- retentionUnit
  - days
  - months
  - years
  - indefinite
- retentionStartTrigger
  - created_at
  - uploaded_at
  - effective_at
  - expiration_at
  - closure_at
  - termination_at
  - superseded_at
  - incident_date
- disposalAction
  - review
  - archive
  - purge
  - anonymize
- legalHoldOverrides
- status
  - draft
  - active
  - inactive
  - archived
- createdAt
- updatedAt
```

## Retention status

```text
RetentionStatus
- retentionStatusId
- recordId
- retentionPolicyRef
- status
  - active
  - due_for_review
  - eligible_for_archive
  - archived
  - eligible_for_purge
  - purged
  - blocked_by_legal_hold
  - indefinite
- retentionStartAt
- retentionExpiresAt
- nextReviewAt
- lastReviewedAt
- reviewedByPersonId
- disposalReviewRef
```

## Disposal review

```text
DisposalReview
- disposalReviewId
- tenantId
- recordId
- retentionStatusRef
- proposedAction
  - archive
  - purge
  - anonymize
  - retain
- status
  - pending
  - approved
  - rejected
  - completed
  - canceled
- requestedAt
- requestedByPersonId
- reviewedByPersonId
- reviewedAt
- decisionReason
- completedAt
```

## Legal hold

LegalHold prevents disposal/purge and may restrict changes.

```text
LegalHold
- legalHoldId
- tenantId
- holdNumber
- title
- description
- status
  - draft
  - active
  - released
  - canceled
- holdType
  - legal
  - regulatory
  - audit
  - investigation
  - customer_dispute
  - supplier_dispute
  - internal_review
- scopeRules
- recordRefs
- sourceProduct
- sourceObjectRef
- createdAt
- createdByPersonId
- activatedAt
- releasedAt
- releasedByPersonId
- releaseReason
- auditTrail
```

## Legal hold scope rule

```text
LegalHoldScopeRule
- scopeRuleId
- legalHoldId
- scopeType
  - record
  - record_type
  - document_type
  - source_product
  - source_object
  - person
  - asset
  - customer
  - supplier
  - date_range
  - search_query
- value
- status
```

## Evidence package workflow

```text
1. Product, ReportArr, Compliance Core, or user requests package.
2. RecordArr resolves source object references.
3. RecordArr gathers linked records.
4. Compliance Core may provide requirement/evidence matrix.
5. RecordArr generates manifest.
6. RecordArr optionally generates PDF/ZIP package.
7. Package becomes complete.
8. Package can be locked for audit.
```

## Retention workflow

```text
1. Record becomes active/effective/closed/etc.
2. RecordArr determines applicable RetentionPolicy.
3. RetentionStatus is created.
4. Scheduler checks review/archive/purge eligibility.
5. Legal holds are checked before disposal.
6. Disposal review is completed if required.
7. Record is archived/purged/anonymized/retained.
```

## Legal hold workflow

```text
1. Authorized user creates LegalHold.
2. Scope rules are defined.
3. RecordArr identifies matching records.
4. LegalHold becomes active.
5. Matching records become blocked_by_legal_hold for disposal.
6. Hold is released when issue ends.
7. Retention scheduler resumes normal handling.
```

## Events

```text
recordarr.evidence_mapping.created
recordarr.evidence_mapping.suggested
recordarr.evidence_mapping.confirmed
recordarr.evidence_mapping.rejected
recordarr.evidence_coverage.updated

recordarr.package.created
recordarr.package.assembling
recordarr.package.completed
recordarr.package.locked
recordarr.package.archived
recordarr.package.failed

recordarr.retention_policy.created
recordarr.retention_policy.updated
recordarr.retention_status.created
recordarr.retention_status.changed
recordarr.disposal_review.created
recordarr.disposal_review.approved
recordarr.disposal_review.completed

recordarr.legal_hold.created
recordarr.legal_hold.activated
recordarr.legal_hold.released
recordarr.legal_hold.canceled
```
