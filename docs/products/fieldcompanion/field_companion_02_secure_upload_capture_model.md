# Field Companion — Secure Upload and Capture Model

## Secure upload session

A SecureUploadSession allows a narrow, temporary, scoped upload or signature flow. It is useful for drivers, vendors, customers, suppliers, visitors, or other people who should not receive full product access.

The session must not grant broad tenant access.

```text
SecureUploadSession
- uploadSessionId
- tenantId
- uploadSessionNumber
- sourceProduct
  - loadarr
  - routarr
  - recordarr
  - supplyarr
  - customarr
  - ordarr
  - assurarr
  - maintainarr
  - staffarr
- sourceObjectRef
- uploadPurpose
  - bol
  - pod
  - packing_slip
  - invoice_reference
  - supplier_document
  - customer_document
  - delivery_photo
  - damage_photo
  - signature
  - incident_photo
  - training_evidence
  - maintenance_evidence
  - quality_evidence
  - audit_evidence
  - other_document
- status
  - created
  - active
  - used
  - partially_used
  - expired
  - revoked
  - failed
- tokenHash
- publicLabel
- noLoginUserLabel
- allowedMimeTypes
- maxUploads
- maxFileSizeBytes
- requiresSignature
- requiresPhoto
- requiresDocumentScan
- allowRetake
- allowManualCrop
- recordarrUploadTargetRef
- uploadedRecordRefs
- createdAt
- createdByPersonId
- activatedAt
- expiresAt
- revokedAt
- revokedByPersonId
- revokeReason
- completedAt
- sourceIp
- userAgent
- auditTrail
```

## Secure upload access rules

```text
1. Token must be random, unguessable, scoped, and expiring.
2. Token must map to one purpose and one source context.
3. Token must not expose product navigation.
4. Token must not expose unrelated tenant data.
5. Upload limits must be enforced.
6. Session should support revoke.
7. Completed uploads become RecordArr records.
8. Source product receives only record refs and metadata.
```

## Capture artifact

CaptureArtifact is Field Companion’s local/mobile representation of evidence before or during RecordArr persistence.

```text
CaptureArtifact
- captureArtifactId
- tenantId
- mobileSessionId
- uploadSessionId
- sourceProduct
- sourceObjectRef
- captureType
  - photo
  - document_scan
  - signature
  - video
  - audio
  - barcode
  - qr_code
  - geolocation
  - form_response
- status
  - captured
  - processing
  - uploaded
  - accepted
  - rejected
  - failed
- localUri
- recordarrRecordId
- filename
- mimeType
- sizeBytes
- checksum
- capturedAt
- capturedByPersonId
- noLoginUserLabel
- metadata
- rejectionReason
```

## Document scan

Document scanning is the workflow where an image is corrected into a document-like PDF/evidence record.

```text
DocumentScan
- documentScanId
- tenantId
- captureArtifactId
- uploadSessionId
- sourceProduct
- sourceObjectRef
- scanPurpose
  - bol
  - pod
  - packing_slip
  - certificate
  - inspection_form
  - maintenance_record
  - supplier_document
  - customer_document
  - other
- status
  - image_captured
  - edge_detection_pending
  - edge_detected
  - manual_crop_required
  - manually_corrected
  - enhanced
  - pdf_generation_pending
  - pdf_generated
  - ocr_pending
  - ocr_completed
  - failed
- originalImageRef
- correctedImageRef
- generatedPdfRecordRef
- edgeCoordinates
- manualEdgeCoordinates
- enhancementSettings
- ocrSummary
- confidenceScore
- processedAt
- correctedByPersonId
```

## Signature capture

```text
SignatureCapture
- signatureCaptureId
- tenantId
- sourceProduct
- sourceObjectRef
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
  - other
- signerName
- signerPersonId
- signerExternalLabel
- signerTitle
- signatureImageRecordRef
- signedAt
- capturedByPersonId
- geoCoordinates
- deviceSnapshot
- attestationText
```

## Photo capture

```text
PhotoCapture
- photoCaptureId
- tenantId
- sourceProduct
- sourceObjectRef
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
- recordarrRecordId
- capturedAt
- capturedByPersonId
- geoCoordinates
- notes
- required
- acceptedBySourceProduct
```

## Barcode/QR scan

```text
ScanEvent
- scanEventId
- tenantId
- mobileSessionId
- sourceProduct
- sourceObjectRef
- scanType
  - barcode
  - qr_code
  - data_matrix
  - text
- scannedValue
- parsedValue
- scanPurpose
  - identify_asset
  - identify_location
  - identify_item
  - identify_order
  - identify_trip
  - secure_upload
  - login_handoff
  - count
  - pick
  - putaway
  - issue
  - receiving
- validationStatus
  - valid
  - invalid
  - unknown
  - duplicate
- validatedByProduct
- validationMessage
- scannedAt
- scannedByPersonId
```

## Voice note capture

```text
VoiceNoteCapture
- voiceNoteId
- tenantId
- sourceProduct
- sourceObjectRef
- purpose
  - work_note
  - defect_note
  - incident_note
  - route_exception
  - quality_note
  - training_note
- audioRecordRef
- transcriptRecordRef
- transcriptionStatus
  - pending
  - completed
  - failed
  - skipped
- capturedAt
- capturedByPersonId
```

## BOL upload workflow

```text
1. LoadArr or RoutArr creates secure upload session for BOL.
2. Field Companion displays QR code or link.
3. Driver opens link without login.
4. Driver captures or uploads image.
5. Field Companion starts document scan.
6. Edge detection runs.
7. Driver/receiver can manually crop if needed.
8. Image is enhanced and sent to RecordArr.
9. RecordArr creates PDF/OCR record.
10. Source product receives RecordRef.
11. Upload session is marked used/completed.
```

## Proof of delivery workflow

```text
1. Driver opens route stop in Field Companion.
2. RoutArr requires proof fields.
3. Driver captures signature, photos, notes, and optional document scan.
4. Field Companion submits proof action to RoutArr.
5. RecordArr stores evidence artifacts.
6. RoutArr marks stop completed.
7. OrdArr receives fulfillment update.
```

## Damage photo workflow

```text
1. User reports damage in receiving, delivery, maintenance, or quality flow.
2. Field Companion captures required photos.
3. Photo artifacts upload to RecordArr.
4. Source product receives RecordRefs.
5. AssurArr may create nonconformance/hold if quality impact exists.
```

## Capture events

```text
FieldCompanion.secure_upload.created
FieldCompanion.secure_upload.opened
FieldCompanion.secure_upload.completed
FieldCompanion.secure_upload.expired
FieldCompanion.secure_upload.revoked
FieldCompanion.capture.photo_captured
FieldCompanion.capture.signature_captured
FieldCompanion.capture.document_scanned
FieldCompanion.capture.voice_note_captured
FieldCompanion.scan.completed
FieldCompanion.capture.uploaded_to_recordarr
FieldCompanion.capture.rejected_by_source
```
