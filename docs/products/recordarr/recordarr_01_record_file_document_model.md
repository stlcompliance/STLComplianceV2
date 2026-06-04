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
