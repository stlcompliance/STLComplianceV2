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
