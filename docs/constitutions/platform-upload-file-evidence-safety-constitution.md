# STL Compliance Upload, File, and Evidence Safety Constitution

## 1. Audit drivers

SEC-007 found unbounded base64 buffering without type validation, quarantine, or scanning. RecordArr is an evidence system and therefore requires a stronger boundary than ordinary attachment handling.

## 2. Prime directive

No uploaded content becomes trusted, downloadable evidence until size, type, integrity, tenant, authorization, and scanning controls have completed.

## 3. Intake

Use streaming multipart or controlled direct-object upload. Enforce limits at proxy, application, route, tenant policy, and storage layers. Base64-in-JSON is prohibited for normal files.

## 4. Validation and quarantine

Validate declared MIME, extension, and file signature independently. New content enters quarantine with an immutable hash. Malware/content scanning records scanner/version/result and only then transitions to accepted or rejected.

## 5. Storage and metadata

File bytes live in controlled object storage; RecordArr stores tenant-scoped metadata, object key, version, hash, scan status, source, actor, retention, legal hold, access policy, and provenance.

## 6. Access and delivery

Downloads use permission-checked short-lived delivery, safe content disposition, and access logging. Unscanned, rejected, purged, held, or superseded content follows explicit rules.

## 7. OCR and extraction

OCR/extraction output is a proposal with source-page provenance and confidence. It does not silently replace record metadata or Compliance Core facts. Human review is required where consequences are material.

## 8. Retention and legal hold

Purge is impossible while an effective legal hold, retention prohibition, evidence dependency, or controlled-document requirement applies. Archive, supersession, and purge are transactional and audited.

## 9. UI

Show upload progress, quarantine, scanning, accepted, rejected, processing, review-needed, and failed states. Do not show a file as complete while it is only buffered locally or awaiting scan.
