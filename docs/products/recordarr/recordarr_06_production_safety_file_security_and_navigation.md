# RecordArr — Production Safety, File Security, and Navigation

## Audit mandate

Replace the global singleton fixture store with tenant-scoped durable metadata and controlled object storage. Access-policy absence defaults to deny. Actor identity comes from the principal.

## Durable model

Persist record identity, file/version metadata, upload/quarantine/scan state, OCR/extraction jobs, metadata assertions, links/comments, evidence mappings, packages, controlled-document versions, approvals, access grants, external shares, retention schedules, legal holds, access logs, and purge events.

## Upload pipeline

Use streamed or direct-object upload with limits, signature validation, hash, quarantine, scanning, safe delivery, and status UI. OCR/extraction is reviewable proposal data with page provenance.

## Retention and access

Archive, supersession, legal hold, retention, external sharing, and purge are transactional and permissioned. Purge is impossible while any effective hold or prohibition applies. Every access/download is tenant checked and logged.

## Navigation

Use direct destinations where a group has one child. Core groups: Records, Capture, Controlled Documents, Packages, Retention & Holds, Access & Sharing, Administration. Document navigation supports class → type → subtype filters plus search, saved views, and source/owner context.

## Pages

Primary record detail includes Overview, Files/Versions, Metadata, Evidence, Related Records, Access, Retention/Holds, and History. Raw JSON is advanced-only. StaffArr sites and all foreign references use live owner pickers.
