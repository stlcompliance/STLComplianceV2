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
