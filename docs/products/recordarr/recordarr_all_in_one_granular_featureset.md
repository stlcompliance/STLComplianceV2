# RecordArr — Scope, Ownership, and Boundaries

## Product purpose

RecordArr is the document, evidence, controlled record, retention, OCR, scan-processing, and audit package system for the STL Compliance / ARR suite.

RecordArr is not just file upload. It is the record/evidence authority that gives every product a stable place to store, classify, version, retain, and package evidence.

RecordArr answers:

- What record exists?
- What file versions are attached?
- What source product/object created or uses it?
- What document type is it?
- What classification applies?
- What OCR or extracted metadata exists?
- Is this record active, rejected, superseded, archived, expired, purged, or on hold?
- What retention policy applies?
- What legal hold applies?
- What evidence mappings exist?
- What package contains this record?
- Can this record be shared, exported, or used as evidence?

## RecordArr owns

```text
- Record identity
- File metadata
- File storage reference
- Document versioning
- Document classification
- Document scan processing state
- Image edge/crop metadata
- OCR results
- Extracted fields
- Controlled document lifecycle
- Document approval workflow references
- Evidence mapping records
- Evidence package assembly
- Record package manifest
- Retention policy execution state
- Legal hold
- Record access policy
- Secure upload sessions where file persistence is involved
- Record audit trail
- Record export/package generation
```

## RecordArr does not own

```text
- Platform login
- Tenant entitlement
- Person master
- Product permissions
- Training assignment completion truth
- Certificate issuance truth
- Regulatory/rulepack meaning
- Asset truth
- Work order truth
- Inventory balance
- Stock ledger
- Receiving truth
- Supplier/vendor master
- Purchase order truth
- Route/trip execution
- Customer master
- Order lifecycle
- Quality hold/release decision
- Reporting read models
- Accounting execution
```

## External product dependencies

```text
NexArr
- Product entitlement
- Login/handoff
- Service tokens
- Platform audit context

StaffArr
- Person references
- Owner/reviewer/approver references
- Site/location context
- Permission checks
- Personnel records/person audit packages

TrainArr
- Training evidence
- Certificate records
- Signoff records
- Qualification evidence packages

Compliance Core
- Evidence type definitions
- Evidence requirements
- Retention rule definitions
- Evidence mapping suggestions/confirmations
- Compliance evaluations

MaintainArr
- Asset documents
- Manuals
- Inspection records
- Work order photos
- Repair evidence
- Return-to-service evidence
- Defect evidence

LoadArr
- BOLs
- Packing slips
- Receiving photos
- Count evidence
- Adjustment evidence
- Inventory discrepancy evidence

SupplyArr
- Supplier documents
- Vendor contracts
- Insurance certificates
- PO documents
- Supplier corrective action responses

RoutArr
- Proof of delivery
- Proof of pickup
- BOL/POD photos
- Delivery signatures
- Route exception evidence

CustomArr
- Customer documents
- Customer signatures
- Customer complaint documents
- Customer requirement evidence

OrdArr
- Order documents
- Fulfillment proof
- Customer acceptance packages
- Closure packages

AssurArr
- Nonconformance evidence
- CAPA evidence
- Quality audit records
- Hold/release evidence
- Supplier/customer quality case records

ReportArr
- Generated reports
- Audit exports
- Scheduled report outputs

Field Companion
- Mobile uploads
- Document scans
- Photo capture
- Signature capture
- No-login secure upload flows
```

## Core source-of-truth rules

```text
1. RecordArr owns file/document/record truth.
2. Products own the operational event that caused the record.
3. Compliance Core owns whether a record satisfies a requirement.
4. RecordArr stores evidence mappings but Compliance Core owns requirement meaning.
5. RecordArr owns document versions and controlled document lifecycle.
6. RecordArr owns retention/legal hold execution state.
7. RecordArr must not decide asset readiness, inventory availability, route completion, training completion, quality release, or order closure.
8. RecordArr can package evidence from many products without becoming the source of those products' facts.
9. RecordArr should expose stable RecordRefs to products.
10. Products should not store raw files independently when the record should be controlled/evidence-bearing.
```

## Standard RecordArr object envelope

```text
RecordArrObject
- id
- tenantId
- objectNumber
- objectType
- status
- title
- description
- sourceProduct
- sourceObjectRef
- classification
- ownerPersonId
- recordRefs
- fileRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- archivedAt
- purgedAt
- auditTrail
- eventLog
```

## RecordArr object prefixes

```text
REC    Record
FILE   File object
VER    Document version
DOC    Controlled document
SCAN   Document scan processing
OCR    OCR result
EXT    Extraction result
MAP    Evidence mapping
PKG    Record package
RET    Retention policy
DISP   Disposal review
HOLD   Legal hold
ACC    Access policy
UPL    Upload session
EXP    Export job
MAN    Package manifest
```

## Standard RecordRef

Other products should reference RecordArr records using a structured reference.

```text
RecordRef
- recordarrRecordId
- recordNumberSnapshot
- titleSnapshot
- recordTypeSnapshot
- documentTypeSnapshot
- statusSnapshot
- classificationSnapshot
- versionSnapshot
- expiresAtSnapshot
- retentionStatusSnapshot
- lastResolvedAt
```

## Standard source object reference

```text
SourceObjectRef
- productKey
- objectType
- objectId
- humanReadableNumber
- displayNameSnapshot
- statusSnapshot
- lastResolvedAt
```

## Standard record classification

```text
Classification
- public
- internal
- confidential
- restricted
- legal_hold
```

## Standard record lifecycle groups

```text
- draft
- processing
- active
- rejected
- review
- approved
- effective
- superseded
- expired
- archived
- purged
```


---


# RecordArr — Record, File, and Document Model

## Record

A Record is the stable business/evidence object. A record may have one or more files, versions, metadata, OCR results, mappings, retention policies, and source links.

```text
Record
- recordId
- tenantId
- recordNumber
- title
- description
- recordType
  - document
  - photo
  - signature
  - video
  - audio
  - form_submission
  - generated_pdf
  - certificate
  - inspection_record
  - training_record
  - maintenance_record
  - receiving_record
  - delivery_record
  - quality_record
  - audit_evidence
  - evidence_package
  - report_output
  - other
- documentType
  - bol
  - pod
  - packing_slip
  - invoice_reference
  - certificate
  - policy
  - procedure
  - work_instruction
  - form
  - safety_data_sheet
  - inspection_form
  - maintenance_evidence
  - training_evidence
  - quality_evidence
  - customer_document
  - supplier_document
  - contract
  - permit
  - photo_evidence
  - signature_evidence
  - other
- status
  - draft
  - processing
  - active
  - review
  - approved
  - rejected
  - superseded
  - expired
  - archived
  - purged
- classification
  - public
  - internal
  - confidential
  - restricted
  - legal_hold
- sourceProduct
- sourceObjectRef
- sourceObjectRefs
- ownerPersonId
- uploadedByPersonId
- uploadedAt
- effectiveAt
- expiresAt
- archivedAt
- purgedAt
- fileRefs
- currentVersionRef
- versionRefs
- metadata
- ocrResultRefs
- extractionResultRefs
- evidenceMappingRefs
- packageRefs
- retentionPolicyRef
- retentionStatusRef
- legalHoldRefs
- accessPolicyRef
- complianceRefs
- auditTrail
```

## Record status definitions

```text
draft
- Record exists but is incomplete.

processing
- File, scan, OCR, classification, or extraction is being processed.

active
- Record is usable.

review
- Record requires review before use.

approved
- Record was reviewed/approved.

rejected
- Record was rejected and should not satisfy evidence requirements.

superseded
- Record was replaced by a newer version/record.

expired
- Record is past its expiration/effective date.

archived
- Record is retained but not active.

purged
- Record metadata remains only as allowed; file is destroyed or inaccessible according to retention policy.
```

## File object

A FileObject is the stored binary/blob metadata. RecordArr should avoid exposing storage internals directly to products.

```text
FileObject
- fileId
- tenantId
- recordId
- fileNumber
- storageProvider
  - local
  - s3
  - azure_blob
  - gcs
  - minio
  - other
- storageKey
- originalFilename
- normalizedFilename
- extension
- mimeType
- sizeBytes
- checksumSha256
- pageCount
- imageWidth
- imageHeight
- durationSeconds
- uploadedAt
- uploadedByPersonId
- virusScanStatus
  - pending
  - clean
  - infected
  - failed
  - skipped
- processingStatus
  - pending
  - processing
  - completed
  - failed
  - skipped
- encryptionStatus
  - encrypted
  - unencrypted
  - unknown
- deletedAt
- deleteReason
```

## File rendition

A rendition is a derived file, such as thumbnail, preview image, enhanced scan, PDF, compressed video, or text preview.

```text
FileRendition
- renditionId
- fileId
- recordId
- renditionType
  - thumbnail
  - preview
  - enhanced_image
  - generated_pdf
  - text_preview
  - compressed_video
  - redacted_copy
- storageKey
- mimeType
- sizeBytes
- pageCount
- status
  - pending
  - generated
  - failed
- generatedAt
```

## Document version

A DocumentVersion tracks versioned document/file state.

```text
DocumentVersion
- versionId
- tenantId
- recordId
- versionNumber
- versionLabel
- status
  - draft
  - review
  - approved
  - effective
  - superseded
  - rejected
  - archived
- fileRef
- createdAt
- createdByPersonId
- submittedForReviewAt
- approvedAt
- approvedByPersonId
- effectiveAt
- supersededAt
- changeSummary
- previousVersionRef
- nextVersionRef
```

## Record metadata

```text
RecordMetadata
- metadataId
- recordId
- key
- value
- valueType
  - string
  - number
  - boolean
  - date
  - datetime
  - enum
  - object_ref
  - json
- source
  - user
  - source_product
  - ocr
  - extraction
  - system
  - import
- confidenceScore
- verified
- verifiedByPersonId
- verifiedAt
```

## Record link

A RecordLink connects one record to another or to a source product object.

```text
RecordLink
- recordLinkId
- recordId
- linkedRecordId
- sourceObjectRef
- linkType
  - source
  - evidence_for
  - supersedes
  - duplicate_of
  - attachment_to
  - package_member
  - generated_from
  - redacted_from
  - related_to
- createdAt
- createdByPersonId
```

## Record comment

```text
RecordComment
- commentId
- recordId
- body
- visibility
  - internal
  - auditor_visible
  - product_visible
  - customer_visible
  - supplier_visible
- createdAt
- createdByPersonId
- editedAt
- editedByPersonId
```

## Record lifecycle workflow

```text
1. Source product or user creates Record.
2. File is uploaded or generated.
3. File is scanned for safety if applicable.
4. Processing runs if required.
5. Record is classified.
6. Metadata/OCR/extraction is stored.
7. Record becomes active or review.
8. Reviewer approves/rejects if required.
9. Products reference RecordRef.
10. Retention and legal hold rules govern long-term state.
```

## Versioning workflow

```text
1. New file/version is uploaded.
2. RecordArr creates DocumentVersion.
3. Version enters draft/review.
4. Approver approves.
5. New version becomes effective.
6. Prior version becomes superseded.
7. Products referencing current record resolve currentVersionRef.
```

## Events

```text
recordarr.record.created
recordarr.record.updated
recordarr.record.status_changed
recordarr.record.classified
recordarr.record.approved
recordarr.record.rejected
recordarr.record.superseded
recordarr.record.expired
recordarr.record.archived
recordarr.record.purged

recordarr.file.uploaded
recordarr.file.virus_scan_completed
recordarr.file.processing_started
recordarr.file.processing_completed
recordarr.file.processing_failed
recordarr.file.rendition_generated

recordarr.document_version.created
recordarr.document_version.submitted_for_review
recordarr.document_version.approved
recordarr.document_version.effective
recordarr.document_version.superseded
recordarr.document_version.rejected
```


---


# RecordArr — Capture, Scan, OCR, and Processing Model

## Upload session

An UploadSession allows a product, user, or secure external link to upload files into RecordArr.

```text
UploadSession
- uploadSessionId
- tenantId
- uploadSessionNumber
- sessionType
  - authenticated
  - secure_no_login
  - service_to_service
  - import
  - generated
- sourceProduct
- sourceObjectRef
- uploadPurpose
  - bol
  - pod
  - packing_slip
  - photo_evidence
  - signature
  - supplier_document
  - customer_document
  - training_evidence
  - maintenance_evidence
  - quality_evidence
  - audit_evidence
  - report_output
  - other
- status
  - created
  - active
  - uploading
  - completed
  - partially_completed
  - expired
  - revoked
  - failed
- tokenHash
- allowedMimeTypes
- maxUploads
- maxFileSizeBytes
- requiresDocumentScan
- requiresOcr
- requiresManualReview
- createdAt
- createdByPersonId
- expiresAt
- completedAt
- revokedAt
- revokedByPersonId
- revokeReason
- uploadedRecordRefs
- auditTrail
```

## Capture request

A CaptureRequest tells a mobile/user flow what evidence to capture.

```text
CaptureRequest
- captureRequestId
- tenantId
- sourceProduct
- sourceObjectRef
- captureType
  - photo
  - document_scan
  - signature
  - video
  - audio
  - file_upload
  - generated_pdf
- title
- instructions
- required
- status
  - open
  - completed
  - skipped
  - expired
  - canceled
- uploadSessionRef
- evidenceRequirementRef
- createdAt
- completedAt
```

## Document scan processing

DocumentScanProcessing turns an uploaded/captured image into a document-like record, commonly PDF plus OCR metadata.

```text
DocumentScanProcessing
- scanProcessingId
- tenantId
- recordId
- originalFileRef
- status
  - uploaded
  - edge_detection_pending
  - edge_detected
  - manual_correction_required
  - manually_corrected
  - enhancement_pending
  - enhanced
  - pdf_generation_pending
  - pdf_generated
  - ocr_pending
  - ocr_completed
  - extraction_pending
  - extraction_completed
  - completed
  - failed
- scanPurpose
  - bol
  - pod
  - packing_slip
  - certificate
  - inspection_form
  - maintenance_record
  - supplier_document
  - customer_document
  - quality_evidence
  - other
- edgeCoordinates
- manualEdgeCoordinates
- correctedByPersonId
- correctedAt
- enhancementSettings
- generatedPdfFileRef
- generatedPdfRecordRef
- ocrResultRef
- extractionResultRef
- confidenceScore
- failureReason
- processedAt
```

## Edge detection result

```text
EdgeDetectionResult
- edgeDetectionResultId
- scanProcessingId
- status
  - detected
  - not_detected
  - low_confidence
  - failed
- confidenceScore
- pageIndex
- corners
  - topLeft
  - topRight
  - bottomRight
  - bottomLeft
- detectedAt
- requiresManualCorrection
```

## Image enhancement settings

```text
ImageEnhancementSettings
- settingsId
- scanProcessingId
- cropApplied
- perspectiveCorrectionApplied
- contrastAdjusted
- brightnessAdjusted
- grayscaleApplied
- noiseReductionApplied
- sharpenApplied
- backgroundCleaned
- outputFormat
  - pdf
  - png
  - jpg
```

## OCR result

```text
OcrResult
- ocrResultId
- tenantId
- recordId
- fileId
- engine
  - tesseract
  - cloud_vision
  - azure_document_intelligence
  - textract
  - other
- status
  - pending
  - processing
  - completed
  - failed
  - skipped
- language
- confidenceScore
- fullText
- pageResults
- blockResults
- extractedAt
- failureReason
```

## OCR page result

```text
OcrPageResult
- pageResultId
- ocrResultId
- pageNumber
- text
- confidenceScore
- width
- height
- blocks
```

## Extraction result

ExtractionResult stores structured fields extracted from OCR, forms, or source product metadata.

```text
ExtractionResult
- extractionResultId
- tenantId
- recordId
- extractionType
  - bol
  - pod
  - packing_slip
  - invoice_reference
  - certificate
  - inspection_form
  - training_record
  - maintenance_record
  - supplier_document
  - customer_document
  - generic
- status
  - pending
  - completed
  - failed
  - manual_review_required
- extractedFields
- confidenceScore
- extractedAt
- reviewedByPersonId
- reviewedAt
- failureReason
```

## Extracted field

```text
ExtractedField
- extractedFieldId
- extractionResultId
- fieldKey
- label
- value
- valueType
  - string
  - number
  - date
  - datetime
  - boolean
  - enum
  - address
  - object_ref
- confidenceScore
- pageNumber
- boundingBox
- reviewStatus
  - unreviewed
  - accepted
  - corrected
  - rejected
- correctedValue
- correctedByPersonId
- correctedAt
```

## Signature record

```text
SignatureRecord
- signatureRecordId
- tenantId
- recordId
- signaturePurpose
  - proof_of_delivery
  - proof_of_pickup
  - training_acknowledgement
  - trainer_signoff
  - evaluator_signoff
  - work_order_closeout
  - inspection_attestation
  - quality_release
  - customer_acceptance
  - policy_acknowledgement
  - other
- signerPersonId
- signerExternalName
- signerTitle
- attestationText
- signatureFileRef
- signedAt
- capturedByPersonId
- sourceProduct
- sourceObjectRef
- geoCoordinates
- deviceSnapshot
```

## Photo evidence record

```text
PhotoEvidence
- photoEvidenceId
- tenantId
- recordId
- photoPurpose
  - defect
  - damage
  - completion
  - before
  - after
  - receipt
  - delivery
  - quality
  - incident
  - audit
  - training
  - other
- sourceProduct
- sourceObjectRef
- capturedAt
- capturedByPersonId
- geoCoordinates
- deviceSnapshot
- notes
```

## Document scan workflow

```text
1. Product creates CaptureRequest or UploadSession.
2. User captures image or uploads file.
3. RecordArr creates Record and FileObject.
4. Virus/file validation runs if applicable.
5. Edge detection runs.
6. If confidence is low, manual correction is requested.
7. Perspective correction/enhancement runs.
8. PDF rendition is generated.
9. OCR runs.
10. Structured extraction runs if template/type is known.
11. Record status becomes active or review.
12. Source product receives RecordRef.
```

## OCR review workflow

```text
1. OCR/extraction completes.
2. Low-confidence fields are flagged.
3. Reviewer accepts/corrects/rejects fields.
4. Corrected extraction metadata is retained.
5. Compliance Core may use confirmed fields for evidence mapping.
```

## Events

```text
recordarr.upload_session.created
recordarr.upload_session.completed
recordarr.upload_session.expired
recordarr.upload_session.revoked

recordarr.capture_request.created
recordarr.capture_request.completed

recordarr.scan.uploaded
recordarr.scan.edge_detected
recordarr.scan.manual_correction_required
recordarr.scan.manually_corrected
recordarr.scan.enhanced
recordarr.scan.pdf_generated
recordarr.scan.completed
recordarr.scan.failed

recordarr.ocr.started
recordarr.ocr.completed
recordarr.ocr.failed

recordarr.extraction.started
recordarr.extraction.completed
recordarr.extraction.manual_review_required
recordarr.extraction.field_corrected

recordarr.signature.captured
recordarr.photo_evidence.captured
```


---


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


---


# RecordArr — Controlled Document, Review, Access, and Sharing Model

## Controlled document

A ControlledDocument is a formal document with lifecycle control, approval, effective date, versioning, review intervals, and distribution/acknowledgement requirements.

```text
ControlledDocument
- controlledDocumentId
- tenantId
- documentNumber
- recordId
- title
- description
- controlledDocumentType
  - policy
  - procedure
  - work_instruction
  - form
  - safety_data_sheet
  - training_material
  - specification
  - contract
  - permit
  - certificate
  - manual
  - other
- status
  - draft
  - review
  - approved
  - effective
  - superseded
  - obsolete
  - archived
- ownerPersonId
- departmentOrgUnitId
- staffarrSiteId
- currentVersionId
- approvalWorkflowRef
- reviewIntervalDays
- nextReviewAt
- effectiveAt
- expiresAt
- supersedesDocumentRef
- supersededByDocumentRef
- distributionRefs
- acknowledgementRequired
- complianceRefs
- auditTrail
```

## Document review

```text
DocumentReview
- documentReviewId
- controlledDocumentId
- versionId
- reviewType
  - approval
  - periodic_review
  - change_review
  - compliance_review
  - quality_review
  - legal_review
- status
  - pending
  - in_review
  - approved
  - rejected
  - changes_requested
  - canceled
- requestedByPersonId
- reviewerPersonId
- requestedAt
- dueAt
- reviewedAt
- decisionReason
- comments
```

## Document distribution

```text
DocumentDistribution
- distributionId
- controlledDocumentId
- versionId
- distributionType
  - person
  - role
  - department
  - site
  - team
  - product
  - external_link
- targetRef
- status
  - pending
  - distributed
  - acknowledged
  - expired
  - revoked
- distributedAt
- acknowledgedAt
- acknowledgementRef
```

## Document acknowledgement

```text
DocumentAcknowledgement
- acknowledgementId
- controlledDocumentId
- versionId
- personId
- status
  - pending
  - acknowledged
  - overdue
  - waived
- acknowledgedAt
- signatureRecordRef
- attestationText
- dueAt
```

## Access policy

RecordArr access policy should control who can read, download, modify, approve, share, export, or purge a record.

```text
RecordAccessPolicy
- accessPolicyId
- tenantId
- recordId
- policyType
  - default
  - restricted
  - legal_hold
  - product_scoped
  - public_link
  - external_share
- readRules
- writeRules
- downloadRules
- shareRules
- exportRules
- purgeRules
- status
  - active
  - inactive
  - superseded
```

## Access grant

```text
RecordAccessGrant
- accessGrantId
- tenantId
- recordId
- granteeType
  - person
  - role
  - product
  - service_client
  - external_link
- granteeRef
- permission
  - read
  - download
  - upload_new_version
  - approve
  - classify
  - map_evidence
  - export
  - share
  - archive
  - purge
- status
  - active
  - expired
  - revoked
- grantedByPersonId
- grantedAt
- expiresAt
- revokedAt
- revokeReason
```

## External share

External sharing should be narrow, expiring, and auditable.

```text
ExternalShare
- externalShareId
- tenantId
- recordId
- shareNumber
- sharePurpose
  - customer_view
  - supplier_response
  - auditor_access
  - legal_review
  - public_download
  - temporary_upload
- status
  - created
  - active
  - expired
  - revoked
  - completed
- tokenHash
- recipientName
- recipientEmail
- allowedActions
  - view
  - download
  - upload
  - sign
- createdAt
- createdByPersonId
- expiresAt
- revokedAt
- revokedByPersonId
- lastAccessedAt
- accessCount
```

## Redaction

```text
Redaction
- redactionId
- tenantId
- sourceRecordId
- redactedRecordId
- redactionReason
  - privacy
  - legal
  - customer
  - supplier
  - internal
  - security
- redactedByPersonId
- redactedAt
- redactionRules
- status
  - draft
  - completed
  - rejected
```

## Record access log

```text
RecordAccessLog
- accessLogId
- tenantId
- recordId
- actorPersonId
- actorServiceClientId
- externalShareId
- action
  - view
  - download
  - upload
  - approve
  - reject
  - share
  - export
  - archive
  - purge
- result
  - allowed
  - denied
  - failed
- occurredAt
- sourceIp
- userAgent
- reasonCode
```

## Controlled document lifecycle workflow

```text
1. User creates controlled document.
2. Draft version is uploaded.
3. Review workflow is started.
4. Reviewers approve, reject, or request changes.
5. Approved version becomes effective.
6. Document is distributed if required.
7. Acknowledgements are collected if required.
8. Periodic review is scheduled.
9. New version supersedes prior version when updated.
10. Obsolete documents are archived according to retention policy.
```

## Access workflow

```text
1. Product/user requests record.
2. RecordArr checks tenant, product, person/service, classification, policy, and legal hold.
3. Access is allowed/denied.
4. Access log is written.
5. Download/view/share/export proceeds if allowed.
```

## External share workflow

```text
1. Authorized user creates ExternalShare.
2. Recipient gets narrow scoped link.
3. Recipient views/downloads/uploads/signs only permitted record/action.
4. Access is logged.
5. Share expires or is revoked.
```

## Redaction workflow

```text
1. User requests redacted copy.
2. Redaction rules are applied.
3. Redacted rendition/record is created.
4. Original remains preserved.
5. Redacted record can be shared/exported.
```

## Events

```text
recordarr.controlled_document.created
recordarr.controlled_document.submitted_for_review
recordarr.controlled_document.approved
recordarr.controlled_document.effective
recordarr.controlled_document.superseded
recordarr.controlled_document.obsolete
recordarr.controlled_document.archived

recordarr.document_review.requested
recordarr.document_review.approved
recordarr.document_review.rejected
recordarr.document_review.changes_requested

recordarr.document_distribution.created
recordarr.document_distribution.acknowledged
recordarr.document_acknowledgement.completed
recordarr.document_acknowledgement.overdue

recordarr.access_policy.created
recordarr.access_grant.created
recordarr.access_grant.revoked
recordarr.external_share.created
recordarr.external_share.accessed
recordarr.external_share.revoked
recordarr.external_share.expired
recordarr.redaction.created
recordarr.redaction.completed
recordarr.access.logged
```


---


# RecordArr — Workflows, Status Logic, Events, and APIs

## Major workflow: product evidence upload

```text
1. Source product requests upload session.
2. RecordArr creates UploadSession.
3. User or service uploads file.
4. RecordArr creates Record and FileObject.
5. File validation/virus scan runs.
6. Processing/OCR/extraction runs if required.
7. Record is classified.
8. Record becomes active or review.
9. Source product receives RecordRef.
10. Compliance Core may suggest evidence mappings.
```

## Major workflow: Field Companion document scan

```text
1. Field Companion captures document image.
2. RecordArr creates Record and FileObject.
3. Edge detection runs.
4. Manual crop correction is requested if confidence is low.
5. Image enhancement runs.
6. PDF rendition is generated.
7. OCR runs.
8. Structured extraction runs.
9. Record becomes active/review.
10. Source product receives final RecordRef.
```

## Major workflow: BOL capture for LoadArr/RoutArr

```text
1. LoadArr or RoutArr requests BOL upload session.
2. Driver/receiver uploads image through Field Companion secure link.
3. RecordArr processes scan and OCR.
4. RecordArr creates BOL Record.
5. LoadArr links RecordRef to Receipt.
6. RoutArr links RecordRef to Trip/Stop if relevant.
7. Compliance Core may evaluate evidence requirement.
```

## Major workflow: proof of delivery

```text
1. RoutArr requests signature/photo/POD capture.
2. Field Companion captures signature/photos/document.
3. RecordArr stores SignatureRecord and related files.
4. RoutArr receives proof RecordRefs.
5. OrdArr receives fulfillment proof through RoutArr/RecordArr references.
6. CustomArr receives customer activity reference if relevant.
```

## Major workflow: controlled document approval

```text
1. User creates ControlledDocument.
2. Draft file/version is uploaded.
3. Review workflow starts.
4. Reviewers approve or request changes.
5. Approved version becomes effective.
6. Distribution and acknowledgements are triggered if required.
7. Prior version becomes superseded.
8. Retention policy is applied.
```

## Major workflow: evidence package assembly

```text
1. Product, ReportArr, Compliance Core, or user requests package.
2. RecordArr resolves source object references.
3. RecordArr gathers relevant records.
4. Compliance Core may provide requirement/evidence matrix.
5. RecordArr generates manifest.
6. RecordArr generates PDF/ZIP if requested.
7. Package becomes complete.
8. Package can be locked and archived.
```

## Major workflow: retention and disposal

```text
1. Record reaches retention review/archive/purge eligibility.
2. RecordArr checks legal hold.
3. If legal hold exists, disposal is blocked.
4. If review required, DisposalReview is created.
5. Authorized reviewer approves/rejects disposal.
6. Record is archived, purged, anonymized, or retained.
7. Audit entry is created.
```

## Major workflow: legal hold

```text
1. Authorized user creates LegalHold.
2. Scope rules identify matching records.
3. Matching records become disposal-blocked.
4. Users may be restricted from deleting/purging records.
5. Hold remains until released.
6. Release resumes normal retention handling.
```

## RecordArr emitted events

```text
recordarr.record.created
recordarr.record.updated
recordarr.record.status_changed
recordarr.record.classified
recordarr.record.approved
recordarr.record.rejected
recordarr.record.superseded
recordarr.record.expired
recordarr.record.archived
recordarr.record.purged

recordarr.file.uploaded
recordarr.file.virus_scan_completed
recordarr.file.processing_started
recordarr.file.processing_completed
recordarr.file.processing_failed
recordarr.file.rendition_generated

recordarr.upload_session.created
recordarr.upload_session.completed
recordarr.upload_session.expired
recordarr.upload_session.revoked

recordarr.scan.uploaded
recordarr.scan.edge_detected
recordarr.scan.manual_correction_required
recordarr.scan.manually_corrected
recordarr.scan.enhanced
recordarr.scan.pdf_generated
recordarr.scan.completed
recordarr.scan.failed

recordarr.ocr.started
recordarr.ocr.completed
recordarr.ocr.failed
recordarr.extraction.completed
recordarr.extraction.manual_review_required

recordarr.signature.captured
recordarr.photo_evidence.captured

recordarr.evidence_mapping.created
recordarr.evidence_mapping.confirmed
recordarr.evidence_mapping.rejected

recordarr.package.created
recordarr.package.assembling
recordarr.package.completed
recordarr.package.locked
recordarr.package.archived
recordarr.package.failed

recordarr.retention_status.changed
recordarr.disposal_review.created
recordarr.disposal_review.approved
recordarr.disposal_review.completed

recordarr.legal_hold.created
recordarr.legal_hold.activated
recordarr.legal_hold.released

recordarr.controlled_document.created
recordarr.controlled_document.approved
recordarr.controlled_document.effective
recordarr.controlled_document.superseded

recordarr.external_share.created
recordarr.external_share.accessed
recordarr.external_share.revoked
```

## Integration APIs RecordArr should expose

```text
GET /api/v1/integrations/records
GET /api/v1/integrations/records/{recordId}
POST /api/v1/integrations/records
PATCH /api/v1/integrations/records/{recordId}

POST /api/v1/integrations/upload-sessions
GET /api/v1/integrations/upload-sessions/{uploadSessionId}
POST /api/v1/integrations/upload-sessions/{uploadSessionId}/complete
POST /api/v1/integrations/upload-sessions/{uploadSessionId}/revoke

POST /api/v1/integrations/files
GET /api/v1/integrations/files/{fileId}
GET /api/v1/integrations/files/{fileId}/download

POST /api/v1/integrations/document-scans
GET /api/v1/integrations/document-scans/{scanProcessingId}
POST /api/v1/integrations/document-scans/{scanProcessingId}/manual-correction

GET /api/v1/integrations/ocr-results/{ocrResultId}
GET /api/v1/integrations/extraction-results/{extractionResultId}
POST /api/v1/integrations/extraction-results/{extractionResultId}/review

POST /api/v1/integrations/signatures
POST /api/v1/integrations/photo-evidence

POST /api/v1/integrations/evidence-mappings
GET /api/v1/integrations/evidence-mappings
POST /api/v1/integrations/evidence-mappings/{mappingId}/confirm
POST /api/v1/integrations/evidence-mappings/{mappingId}/reject

POST /api/v1/integrations/record-packages
GET /api/v1/integrations/record-packages/{packageId}
POST /api/v1/integrations/record-packages/{packageId}/lock
GET /api/v1/integrations/record-packages/{packageId}/download

GET /api/v1/integrations/retention-policies
GET /api/v1/integrations/records/{recordId}/retention-status
POST /api/v1/integrations/legal-holds
POST /api/v1/integrations/legal-holds/{legalHoldId}/release

POST /api/v1/integrations/controlled-documents
GET /api/v1/integrations/controlled-documents/{controlledDocumentId}
POST /api/v1/integrations/controlled-documents/{controlledDocumentId}/versions
POST /api/v1/integrations/controlled-documents/{controlledDocumentId}/reviews

POST /api/v1/integrations/external-shares
POST /api/v1/integrations/external-shares/{externalShareId}/revoke
POST /api/v1/integrations/redactions
```

## APIs RecordArr should consume

```text
NexArr
- POST /handoff/redeem
- POST /service-tokens/introspect
- GET /entitlements/{productKey}

StaffArr
- GET /persons/{personId}
- GET /persons/{personId}/permissions
- GET /locations/{locationId}
- POST /permission-checks

Compliance Core
- GET /evidence-types
- GET /evidence-requirements
- POST /evidence-mapping/suggest
- POST /evidence-mapping/confirm
- POST /evaluations

ReportArr
- POST /events

Source product read APIs as needed for package assembly:
- MaintainArr assets/work-orders/inspections
- LoadArr receipts/counts/movements
- TrainArr assignments/certificates
- RoutArr trips/stops/proofs
- SupplyArr suppliers/POs
- CustomArr customers
- OrdArr orders
- AssurArr nonconformances/CAPAs/holds
```

## Permission examples

```text
recordarr.records.read
recordarr.records.upload
recordarr.records.update
recordarr.records.classify
recordarr.records.approve
recordarr.records.reject
recordarr.records.archive
recordarr.records.purge

recordarr.files.download
recordarr.files.upload
recordarr.files.delete

recordarr.scans.create
recordarr.scans.correct
recordarr.ocr.read
recordarr.extraction.review

recordarr.evidence_mappings.read
recordarr.evidence_mappings.create
recordarr.evidence_mappings.confirm
recordarr.evidence_mappings.reject

recordarr.packages.read
recordarr.packages.create
recordarr.packages.lock
recordarr.packages.export

recordarr.retention.read
recordarr.retention.manage
recordarr.disposal.review
recordarr.legal_holds.manage

recordarr.controlled_documents.read
recordarr.controlled_documents.create
recordarr.controlled_documents.review
recordarr.controlled_documents.approve
recordarr.controlled_documents.supersede
recordarr.controlled_documents.archive

recordarr.external_shares.create
recordarr.external_shares.revoke
recordarr.redactions.create
recordarr.access_logs.read
recordarr.admin
```

## Default role examples

```text
Record Viewer
- Read permitted records and packages.

Record Contributor
- Upload records and evidence for assigned workflows.

Record Reviewer
- Review records, OCR/extraction fields, and evidence mappings.

Document Controller
- Manage controlled documents, versions, reviews, distributions, and acknowledgements.

Evidence Manager
- Create evidence packages.
- Confirm/reject evidence mappings.
- Coordinate audit evidence.

Retention Manager
- Manage retention policies, disposal reviews, archives, legal holds.

External Share Manager
- Create/revoke external shares and redacted copies.

RecordArr Admin
- Manage settings, access policies, retention, controlled document configuration, and integrations.
```

## RecordArr UI surfaces

```text
/app/recordarr
- dashboard
- records
- record detail
- uploads
- scan processing
- OCR/extraction review
- evidence mappings
- controlled documents
- document reviews
- distributions/acknowledgements
- record packages
- retention
- disposal reviews
- legal holds
- external shares
- redactions
- access logs
- settings
```

## Record detail UI

```text
RecordDetailPage
- Header
  - recordNumber
  - title
  - type
  - status
  - classification
  - source product/object
- Files
  - current file
  - renditions
  - download/preview
- Versions
  - version history
- Metadata
  - source metadata
  - OCR/extraction fields
- Evidence
  - mappings
  - compliance requirements
- Packages
  - package memberships
- Retention
  - policy
  - status
  - legal holds
- Access
  - access policy
  - grants
  - share links
- Timeline
  - audit/history
```

## Package detail UI

```text
PackageDetailPage
- Package header
- Source scope
- Record list
- Requirement/evidence matrix
- Manifest
- Generated output
- Lock/archive controls
- Timeline
```

## Controlled document detail UI

```text
ControlledDocumentDetailPage
- Header
- Current effective version
- Draft/review versions
- Review workflow
- Distribution
- Acknowledgements
- Related records
- Retention
- History
```
