# RecordArr — DMS Full Feature Set

[Workflow catalog](./WORKFLOWS.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md) · [Rollout](./ROLLOUT.md)

## Document control

| Field | Value |
| --- | --- |
| Product | RecordArr (DMS) |
| Category | Document and Records Management System |
| Repository maturity | Scaffold |
| Audit basis | Static repository inspection on 2026-06-24 |
| Cataloged capabilities | 69 |
| Cataloged workflows | 15 |

## Product charter

RecordArr is the suite document, evidence, and records system of record. It owns record metadata, file objects and renditions, capture/scan/OCR/extraction processing, controlled document versions and distribution, evidence mapping, packages/manifests, access/sharing/redaction/signature, retention/disposition/legal holds, and access audit. Domain products own the business record that a document supports and reference RecordArr IDs rather than storing duplicate files.

> **Implementation reality — Partial durable migration:** RecordArr now persists core record, file-version, file-integrity check, file-malware scan, storage-reconciliation, object-store object index, object-store fixity observation, disaster-recovery restore run, audit-event, audit-seal, record metadata, record link, record comment, upload-session, capture-request, scan-processing, OCR-result, extraction-result, evidence-mapping, package, package-manifest, retention-status, disposal-review, destruction-certificate, legal-hold, retention-scheduler run, retention-scheduler lease, retention-scheduler outbox, controlled-document, controlled-document-version, document-review, document-distribution, document-acknowledgement, access-policy, access-grant, external-share, access-log, access-history seal, redaction, redaction-provider job, signature-record, signature trust-service job, and photo-evidence metadata through EF-backed tables, with filesystem-backed object storage keys for inline uploads, generated scan PDFs, generated package files, controlled-document versions, signatures, photos, and redacted copies. Evidence coverage is derived tenant-safely from durable mappings, package/manifests are tenant-scoped through their parent records, retention/hold/disposal reads and mutations are tenant-scoped through the owning record or hold row, retention-disposition runs can acquire/release durable tenant-scoped scheduler leases, idempotently create pending archive/purge disposal reviews while skipping records blocked by active legal hold, persist pending outbox notification records for the owning RecordArr review task without auto-disposing records, execute already-approved disposal reviews through an explicit `execute_approved_reviews` policy that revalidates tenant scope, active legal holds, and retention eligibility before applying archive/purge outcomes, leave held or ineligible reviews unexecuted with denied access evidence, mark executed reviews completed, remain idempotent on retry, process in-app outbox messages into delivered/failed states, fail explicitly requested external delivery channels truthfully until a provider reference is supplied, retry failed messages when a provider is configured, and escalate unresolved messages with recipient/escalation evidence. Unsupported automatic disposition execution policies now fail as durable scheduler runs, release their scheduler lease, write denied access/audit evidence against eligible records, and create no disposal reviews, outbox messages, destruction certificates, archive actions, or purge actions. Approved purge disposition now creates durable tenant-scoped destruction certificates with file tombstone refs and hash evidence, direct archive/purge disposition is blocked and denial-logged under active legal hold, active legal holds block core record-attached file replacement, status changes, metadata/link/comment mutation, redaction, signature, photo-evidence capture, package lock/archive/create, controlled-document lifecycle mutation, document distribution/acknowledgement/review mutation, access-policy/grant mutation, external-share create/revoke/expire mutation, passive access-grant/external-share expiry or expired-share replay mutation, passive controlled-document periodic-review/acknowledgement-overdue refresh mutation, and passive retention-status legal-hold block/restore refresh mutation with denied or allowed audit evidence, controlled-document lifecycle reads/mutations are tenant-scoped through the owning backing record, and access/share/redaction/signature/photo/fixity-check/object-store-index/malware-scan/storage-reconciliation/disaster-recovery/audit-event/audit-seal/access-history-seal/scheduler-evidence reads and mutations are tenant-scoped through the owning record or tenant-scoped audit/reconciliation/scheduler/restore row. New files remain unavailable until malware scan evidence releases them; a tenant-scoped provider-run contract can process pending files idempotently, quarantine infected/failed files, release clean/skipped files, and dead-letter repeated failed scans with durable access/audit evidence while preserving clean-scan recovery. A disabled-by-default hosted malware worker can now poll configured tenant IDs, consume explicit external scanner verdict manifests, apply only clean/infected/failed verdicts for pending tenant files, and leave files pending when no verdict exists instead of creating fake clean results. Storage reconciliation findings can now be remediated through tenant-scoped workspace/integration actions that resolve restored missing/corrupt, released quarantined, and scanned pending issue refs with durable fixity, malware, access-log, reconciliation evidence, object-store index snapshots, and fixity observation history. Disaster-recovery restore verification now persists tenant-scoped restore runs with recovery point, RPO/RTO targets, restored/blocked record refs, verified/failed file refs, durable fixity observations, access evidence, and truthful failure states for stale recovery points, cross-tenant records, missing objects, corrupt checksums, and RTO misses. Backup verification now requires explicit provider/job/manifest/recovery-point evidence before claiming provider backup coverage and persists tenant-scoped backup verification runs with provider, job, manifest hash, verified/failed file refs, fixity observations, and access evidence. Signature captures now persist locked signature evidence hashes, truthfully distinguish local-only captures from provider-verified envelopes, require provider envelope and certificate fingerprint evidence before claiming provider verification, expose provider evidence through the integration contract, reconcile explicit provider callbacks only when provider/envelope/certificate evidence matches the stored signature, persist durable trust-service job submissions/manifests only when provider envelope and certificate evidence match, and include a disabled-by-default signature trust-service worker that consumes explicit provider manifests for configured tenants without fabricating callbacks. Redactions now require explicit redaction rules, persist locked package hashes, review actor/time, approval reason evidence for the generated redacted copy, explicit rendered-overlay review evidence, durable provider job submission evidence, provider manifest reconciliation only when provider-supplied package hashes match the locked RecordArr redaction package hash, include a disabled-by-default redaction provider worker that consumes explicit provider manifests for configured tenants without fabricating provider approval, and include a disabled-by-default rendered-overlay review worker that consumes explicit rendered-overlay manifests for configured tenants without fabricating overlay evidence. A disabled-by-default hosted object-store reconciliation worker can now poll configured tenant IDs, consume explicit external object-store inventory manifests, reconcile only provider-verified/missing/corrupt file refs, ignore cross-tenant manifest rows, and apply explicit restore/recheck remediation evidence without fabricating a clean pass when provider evidence is absent. Access logs and audit events are hash-chained and expose tenant-scoped integrity verification; audit and access-history seals can detect tampering within sealed ranges. The broader DMS domain remains scaffolded: provider-grade immutable audit storage/notarization beyond hash-chain seals, durable seal checkpoints, audit-governance reports, and explicit audit-anchor evidence, encryption governance, provider-backed object-storage lifecycle/backup automation beyond the durable object index, fixity trail, lifecycle-verification evidence, restore-run and backup-verification evidence, external trust-service webhook ingestion/provider scheduling and provider-managed timestamp/long-term-validation automation beyond explicit signature trust-service jobs/manifests plus the disabled manifest worker, external redaction-provider webhook ingestion/provider scheduling/delivery orchestration beyond explicit provider jobs/manifests plus the disabled manifest worker, and rendered-overlay generation/provider scheduling beyond explicit overlay manifests plus the disabled manifest worker still require owned provider workflows before full production reliance.

> **Backup worker reality:** A disabled-by-default backup verification worker now consumes explicit provider backup manifests for configured tenants and calls the durable backup-verification path only for known tenant record scopes. Missing manifests, missing provider evidence, cross-tenant rows, unknown records, and unknown file refs do not create passing backup evidence. Provider backup scheduling, provider job execution, immutable backup retention, and restore orchestration beyond explicit manifests remain incomplete.

## Source-of-truth boundary

### RecordArr owns

- Record containers, file versions, storage objects, checksums, renditions/previews, metadata, classifications, tags, links, comments, and access history.
- Upload sessions, secure capture requests, scan/image processing, OCR, extraction proposals, human review, and source/provenance.
- Controlled documents, versions, review/approval, effective/superseded/obsolete state, distribution, acknowledgements, and controlled-copy evidence.
- Evidence requirements mapping, coverage, packages, manifests, locks, exports/downloads, signatures, redactions, and photo evidence.
- Retention schedules, record classes, disposition eligibility/review/destruction, legal holds, preservation, and defensible deletion evidence.
- Record access policies, grants, external shares, recipient authentication, expiry/revocation, watermark/download controls, and audit logs.

### RecordArr does not own

- The operational meaning or lifecycle of a work order, person, order, supplier, customer, trip, quality case, training assignment, compliance finding, or financial transaction.
- Compliance evidence sufficiency or applicability; Compliance Core decides whether referenced evidence satisfies a requirement.
- Training distribution/qualification; TrainArr owns assignments and completion while RecordArr stores controlled content/evidence.
- Business approvals unrelated to document control; owning products retain their own decisions and reference documents.
- General-purpose reporting or analytics; ReportArr owns reports/read models.

## Suite access and authorization

- All normal tenant users may launch every product application in the suite; product actions and records remain permission-scoped server-side.
- Fixed-suite access is nonvariable. Deployment health, feature readiness, legal restrictions, external-provider setup, and permissions may still constrain a capability.
- Platform administration remains a NexArr-only responsibility and is not a tenant product surface.
- Compliance Core administrative/authoring UI is restricted, while Compliance Core runtime evaluations, fact requirements, evidence needs, and workflow gates serve every tenant and product.

## Primary users

- Records administrator
- Document author/owner
- Reviewer/approver
- Scanner/intake operator
- Evidence reviewer
- Legal hold administrator
- Auditor
- External customer/supplier/auditor recipient
- Field worker

## Required integrations

- All products
- NexArr
- Compliance Core
- ReportArr
- Field Companion
- Object storage/KMS/antimalware
- Email/scanner/OCR/e-sign/eDiscovery providers

## Product principles

- A file is stored once and referenced; products do not maintain shadow attachment stores.
- The current implementation is a partial durable migration over a broad prototype domain. Durable object-storage provider control-plane operationalization beyond explicit lifecycle evidence, remaining metadata persistence, provider-grade immutable audit storage/notarization beyond explicit audit-anchor evidence, retention depth, and provider-backed disaster-recovery job orchestration beyond explicit restore and backup-verification evidence are release blockers.
- The disabled backup verification worker reduces the disaster-recovery orchestration gap for explicit provider manifests, but provider-side backup scheduling, execution, immutable backup retention, and restore automation remain release blockers.
- Controlled-document acknowledgement is distinct from TrainArr competence/qualification.
- Disposition is an approval workflow with holds and source-record checks, never a simple delete button.

## Repository implementation snapshot

| Static indicator | Count |
| --- | --- |
| Persistent DbSet declarations | 43 |
| Discovered server type declarations | 213 |
| Discovered HTTP route declarations | 270 |
| Frontend source files | 10 |
| Frontend page files | 1 |
| Documentation headings | 90 |

### Evidence used for the current-state classification

- recordarr-api contains a RecordArrStore covering records/files/renditions, upload/capture/scan/OCR/extraction, evidence mappings/coverage/packages, metadata/links/comments, retention/disposal/legal holds, controlled docs/versions/reviews/distributions/acknowledgements, access/grants/shares/redactions/signatures/photo evidence/access logs.
- The RecordArr EF context exposes durable core record, file, file-integrity check, file-malware scan, storage-reconciliation, object-store object index, object-store fixity observation, disaster-recovery restore run, audit-event, audit-seal, metadata, link, comment, upload-session, capture-request, scan-processing, OCR-result, extraction-result, evidence-mapping, package, package-manifest, retention-status, disposal-review, destruction-certificate, legal-hold, retention-scheduler run, retention-scheduler lease, retention-scheduler outbox, controlled-document, controlled-document-version, document-review, document-distribution, document-acknowledgement, access-policy, access-grant, external-share, access-log, access-history seal, redaction, redaction-provider job, signature-record, signature trust-service job, and photo-evidence entities. Audit-event hashes, audit seals, access-history hashes, access-history seals, destruction-certificate evidence, scheduler-created disposal-review runs, scheduler leases/outbox evidence, storage-reconciliation remediation outcomes, durable object-store index/fixity observations, disaster-recovery restore evidence, backup-verification evidence, signature trust-service job evidence, configured external object-store inventory manifest worker outcomes, and configured signature trust-service manifest worker outcomes can be verified or retrieved through tenant-scoped workspace/integration routes. Provider-grade immutable audit storage/notarization beyond hash-chain seals, durable seal checkpoints, audit-governance reports, and explicit audit-anchor evidence and provider-backed backup/restore job orchestration remain incomplete.
- Configured backup verification manifest worker outcomes are also verifiable through tenant-scoped disaster-recovery run evidence.
- recordarr-frontend routes cover records, capture, controlled docs, reviews, distributions, acknowledgements, packages, retention/disposal/legal holds, access/shares/redactions/logs/settings.
- Canonical product docs define record/file/document, capture/scan/OCR, evidence/package/retention, controlled-document/access, and workflow/API contracts.

> Counts are static-discovery indicators, not proof that every route or screen is complete, reachable, secure, migrated, or production-ready.

## Mandatory migration or refactor work

- Continue replacing the process-local RecordArrStore domain with durable metadata plus approved object storage; add versioning, hashing, retention, hold, access, provider-grade immutable audit storage/notarization beyond explicit audit-anchor evidence, and provider-backed object-storage/backup operational controls beyond the durable object index, fixity trail, lifecycle-verification evidence, restore-run and backup-verification evidence.
- Continue provider-backed object-storage/backup operational controls beyond explicit backup manifests and the disabled backup verification worker.

## Feature catalog

### A. Currently implemented or meaningfully represented

These capabilities have repository evidence. Their state follows the product-level maturity and may still require hardening or completion.

| Feature ID | Capability | Class | State | Required behavior / evidence |
| --- | --- | --- | --- | --- |
| RE-CUR-001 | Record and file prototype | CURRENT | Partial | Core created record, file-version metadata, file safety status, and durable malware scan/quarantine decisions persist through EF tables; new files remain scan-pending and unavailable until an explicit scan result, tenant-scoped provider run, or configured hosted worker consuming external verdicts releases or quarantines them; files without external verdicts remain pending, infected/failed files remain quarantined until a later clean/skipped scan releases them, while rendition structures and the capture workflow still demonstrate intended behavior through scaffolded state, including defaulting capture taxonomy from active vocabulary terms. |
| RE-CUR-002 | Upload-session and capture-request prototype | CURRENT | Partial | Upload sessions and capture requests are tenant-scoped and durable through EF tables; tokening, external capture, upload processing, retry, audit trail, and provider-backed file workflow depth remain incomplete. |
| RE-CUR-003 | Scan, OCR, and extraction prototype | CURRENT | Partial | Scan processing, generated PDF file metadata, OCR results, extraction results, manual correction, and extraction review state persist through EF tables; provider queues, bounding-box review depth, correction reuse, and production OCR worker orchestration remain incomplete. |
| RE-CUR-004 | Metadata, links, and comments prototype | CURRENT | Partial | Created record metadata, source links, and comments persist through EF tables; richer verification, cross-product link semantics, audit, and workflow enforcement remain incomplete. |
| RE-CUR-005 | Controlled document/version prototype | CURRENT | Partial | Controlled documents, document versions, review state, publication/effective status, supersession references, audit trail snapshots, and generated version file metadata persist through EF tables with tenant-scoped workspace/integration reads and mutations; immutable audit depth, policy enforcement, and deeper review automation remain incomplete. |
| RE-CUR-006 | Distribution and acknowledgement prototype | CURRENT | Partial | Controlled distributions and recipient acknowledgements persist through EF tables with tenant-scoped create/revoke/expire/acknowledge paths; signature-record enforcement, training/qualification impact, delegated recipient scopes, and audit-grade access history remain incomplete. |
| RE-CUR-007 | Evidence mapping and coverage prototype | CURRENT | Partial | Evidence-to-requirement mappings persist through EF tables and coverage is rebuilt tenant-safely from durable mappings; Compliance Core satisfaction logic, stale evidence evaluation, and package-grade audit history remain incomplete. |
| RE-CUR-008 | Package and manifest prototype | CURRENT | Partial | Packages and manifests persist through EF tables with tenant-scoped list/get/lock/archive access, restart-safe checksums, and generated package PDF/ZIP file metadata; deeper package-grade audit history, immutable export sealing, retention linkage, and external delivery controls remain incomplete. |
| RE-CUR-009 | Retention, disposal, and legal-hold prototype | CURRENT | Partial | Retention statuses, disposal reviews, destruction certificates, legal holds, retention-scheduler runs, retention-scheduler leases, and retention-scheduler outbox messages persist through EF tables with tenant-scoped list/get/create/activate/release/complete paths; retention-disposition runs acquire and release durable tenant scheduler leases, idempotently create pending archive/purge disposal reviews from eligible retention statuses, skip legal-held records, persist pending outbox notification records for each new review, process in-app outbox messages into delivered/failed states, fail explicitly requested external delivery channels truthfully until a provider reference is supplied, retry failed messages when a provider is configured, escalate unresolved outbox messages with recipient/escalation evidence, and support `create_pending_reviews_only` plus `execute_approved_reviews`; approved-review execution revalidates tenant scope, active legal holds, and current retention eligibility before archive/purge side effects, leaves held or ineligible reviews unexecuted with denied access evidence, marks executed reviews completed, and remains idempotent on retry; unsupported execution policies fail as durable scheduler runs with released leases, denied access/audit evidence, and no review/outbox/certificate/archive/purge side effects; approved purge reviews create durable destruction certificates with deleted-file refs, tombstone evidence, certificate hash, and access/audit evidence; active legal holds block disposal-review outcomes, direct archive/purge actions, core record-attached file/status/metadata/link/comment/redaction/signature/photo mutations, package create/lock/archive, controlled-document lifecycle/review/distribution/acknowledgement mutations, access-policy/grant mutations, external-share create/revoke/expire mutations, passive access-grant/external-share expiry or expired-share replay mutations, passive controlled-document periodic-review/acknowledgement-overdue refresh mutations, and passive retention-status legal-hold block/restore refresh mutations, preserve files and held workflow/access state, and write denied or restoration access/audit evidence. Immutable audit depth remains incomplete. |
| RE-CUR-010 | Access policy, grant, and external-share prototype | CURRENT | Partial | Access policies, access grants, external shares, access logs, and access-history seals persist through EF tables with tenant-scoped list/mutation paths, narrow external-share allowed-action enforcement, restart-safe access/share log history, hash-chain access-history integrity verification, and sealed access-history range verification for the migrated workflows; deeper policy evaluation, watermark/download controls, recipient authentication, and external portal depth remain incomplete. |
| RE-CUR-011 | Redaction and signature prototype | CURRENT | Partial | Redactions, redaction-provider jobs, signature records, and signature trust-service jobs persist through EF tables with tenant-scoped create/read paths, durable generated files, redacted-copy records, copied access controls, restart-safe metadata, locked signature evidence hashes, local-vs-provider verification status, provider envelope/certificate evidence when supplied, explicit provider callback reconciliation, durable signature trust-service job submission/manifest reconciliation, a disabled-by-default signature trust-service manifest worker for configured tenants, explicit redaction rules, locked redaction package hashes, durable redaction-provider job submission/manifest reconciliation, a disabled-by-default redaction provider manifest worker for configured tenants, explicit redaction provider reconciliation, rendered-overlay review evidence, a disabled-by-default rendered-overlay review manifest worker for configured tenants, and review/approval evidence; external trust-service webhook ingestion/provider scheduling, provider-managed timestamp/long-term-validation automation beyond explicit jobs/manifests and the disabled manifest worker, rendered-overlay generation/provider scheduling beyond explicit overlay manifests and the disabled manifest worker, external redaction-provider webhook ingestion/provider scheduling/delivery orchestration beyond explicit jobs/manifests and the disabled manifest worker, and provider-grade immutable audit storage/notarization beyond explicit audit-anchor evidence remain incomplete. |
| RE-CUR-012 | Photo evidence prototype | CURRENT | Partial | Photo evidence records persist through EF tables with tenant-scoped create/read paths, durable generated image file metadata, source refs, location/device snapshots, and notes; offline encrypted capture, media integrity checks, provider-backed scanning depth, and immutable chain-of-custody remain incomplete. |
| RE-CUR-013 | Comprehensive DMS navigation scaffold | CURRENT | Partial | Frontend routes cover the expected document and records operating model, including capture, packages, holds, access, redaction, and retention workspaces. |

### B. Common category baseline

These are expected for a credible Document and Records Management System product. Where they overlap current behavior, the target definition is the acceptance standard rather than a duplicate feature.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| RE-COM-001 | Durable content and metadata storage | COMMON | Target | Encrypted object storage, durable metadata DB, checksums, versioning, virus/malware scan, backups, replication, and restore testing. |
| RE-COM-002 | Document check-in/version control | COMMON | Target | Immutable versions, draft/current/effective state, edit lock or collaborative model, comparison, restore, and lineage. |
| RE-COM-003 | Metadata and classification | COMMON | Target | Record class, document type/subtype, owner, dates, source, subject refs, confidentiality, retention, tags, custom fields, and controlled vocabulary. |
| RE-COM-004 | Full-text and metadata search | COMMON | Target | OCR text, filters, facets, saved searches, permissions trimming, highlighting, related records, and result explainability. |
| RE-COM-005 | Capture and imaging | COMMON | Target | Camera/file/email/scanner ingestion, auto-crop/deskew/rotate/dewarp, quality checks, barcode/QR separation, OCR, and batch capture. |
| RE-COM-006 | OCR and data extraction | COMMON | Target | Languages, tables/forms, confidence, source bounding boxes, validation, human review, and export/commit proposals. |
| RE-COM-007 | Controlled document management | COMMON | Target | Author, review, approve, sign, effective date, publish, controlled copy, periodic review, revise, supersede, obsolete, and distribution. |
| RE-COM-008 | Records management and retention | COMMON | Target | Classification, event/time-based retention, freeze/hold, eligibility, review, disposition, destruction certificate, and audit. |
| RE-COM-009 | Legal hold and preservation | COMMON | Target | Matters, custodians/systems/records, notices, acknowledgement, collection, preservation, release, and defensible chain. |
| RE-COM-010 | Access and sharing | COMMON | Target | Role/attribute/record policies, case/project/customer/vendor scopes, external recipients, expiry, revocation, watermark, download, and audit. |
| RE-COM-011 | Redaction and privacy | COMMON | Target | Permanent/non-destructive redaction, reason, reviewer, overlay validation, original protection, and export-safe output. |
| RE-COM-012 | Electronic signatures | COMMON | Target | Signature request, signer authentication, intent/meaning, sequence, completion certificate, tamper evidence, and external provider integration. |
| RE-COM-013 | Evidence packages and audit export | COMMON | Target | Manifest, required items, source refs, hashes, validation, redaction, lock, share, export, and supplemental versions. |
| RE-COM-014 | APIs and connectors | COMMON | Target | Office/email/scanner/cloud storage/e-sign/product APIs with idempotent upload, search, metadata, version, retention, and event contracts. |

### C. Commonly requested but widely underserved

These address recurring user friction, fragmented add-ons, poor transparency, inaccessible setup, or weak SMB usability.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| RE-UND-001 | Metadata-first DMS that remains simple | UNDERSERVED | Target | Users file documents by what they are and what they relate to, without navigating brittle folder trees or enterprise taxonomy projects. |
| RE-UND-002 | Affordable scan/OCR/approval stack | UNDERSERVED | Target | Small organizations get capture, OCR, metadata review, approvals, retention, and integration without buying separate tools. |
| RE-UND-003 | One file, many references | UNDERSERVED | Target | Store a file once and safely reference it from multiple domain records/requirements while each context retains its own meaning and access. |
| RE-UND-004 | Source-cited semantic search and Q&A | UNDERSERVED | Target | Answer only from authorized documents, cite exact pages/sections, distinguish versions, and say when evidence is missing. |
| RE-UND-005 | Universal intake with ownership routing | UNDERSERVED | Target | Email/upload/scan/mobile/integration is classified and proposed to the correct product/record with human review, not dumped into an inbox forever. |
| RE-UND-006 | Transparent OCR/extraction review | UNDERSERVED | Target | Show image and extracted field side by side with confidence/bounding source, keyboard flow, and correction reuse. |
| RE-UND-007 | Controlled procedure at point of work | UNDERSERVED | Target | Operational products and Field Companion display the effective version and record which version was viewed/acknowledged/executed. |
| RE-UND-008 | Portable archive and exit | UNDERSERVED | Target | Export content, versions, metadata, relationships, retention/holds, audit, and manifests in documented formats without vendor lock-in. |
| RE-UND-009 | Human-readable retention | UNDERSERVED | Target | Explain why a record is retained, event/date used, earliest disposition, hold, owner, and review steps. |
| RE-UND-010 | Secure customer/supplier/auditor data rooms | UNDERSERVED | Target | Scoped requests, uploads, reviews, questions, acknowledgements, and revocation without a separate enterprise deal-room product. |
| RE-UND-011 | Offline encrypted field capture | UNDERSERVED | Target | Capture evidence offline, show pending upload, prevent unauthorized local sharing, and verify server acceptance before deleting local copy. |
| RE-UND-012 | Duplicate and near-duplicate detection | UNDERSERVED | Target | Detect exact hashes, scans of the same source, revised documents, and conflicting versions while preserving intentional copies. |

### D. Advanced capabilities to democratize

These are commonly found only in enterprise tiers, specialist products, or expensive implementations. STL should make them practical without weakening governance.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| RE-DEM-001 | Intelligent document processing | DEMOCRATIZE | Target | Classify, split, OCR, extract, validate, match, and propose product actions from documents with source citations and human approval. |
| RE-DEM-002 | Enterprise records and information governance | DEMOCRATIZE | Target | Event-based retention, legal holds, disposition approvals, defensible deletion, privacy requests, and audit across all products. |
| RE-DEM-003 | Knowledge graph and semantic retrieval | DEMOCRATIZE | Target | Connect documents, versions, entities, events, requirements, controls, products, and evidence while enforcing record-level permissions. |
| RE-DEM-004 | Digital signatures and trust services | DEMOCRATIZE | Target | Multiple signature providers, certificates, timestamping, seals, validation, and long-term signature preservation. |
| RE-DEM-005 | eDiscovery and investigation collections | DEMOCRATIZE | Target | Matter scoping, custodians, preservation, collection, search, review, tagging, export, and chain-of-custody for smaller organizations. |
| RE-DEM-006 | Automated document change impact | DEMOCRATIZE | Target | Compare versions, identify changed obligations/procedures/forms, and propose affected training, rules, workflows, and acknowledgements. |
| RE-DEM-007 | Content authenticity and provenance | DEMOCRATIZE | Target | Capture source, device, signer, hash, transformation, AI processing, watermark, and chain of custody for high-value evidence. |
| RE-DEM-008 | Secure external collaboration rooms | DEMOCRATIZE | Target | Granular rooms, request lists, Q&A, versions, reviewer roles, activity, watermark, and expiration without broad guest accounts. |
| RE-DEM-009 | Advanced forms and data capture | DEMOCRATIZE | Target | Template recognition, handwriting/signature/checkbox/table extraction, validation rules, and direct reviewed proposals into product records. |
| RE-DEM-010 | Long-term preservation | DEMOCRATIZE | Target | Format migration, fixity checks, metadata preservation, storage tiers, integrity alerts, and retrieval tests for statutory or permanent records. |

### E. Suite-wide foundation required in RecordArr

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| RE-FND-001 | Tenant-scoped authorization | FOUNDATION | Target | Every read and write is tenant-scoped server-side; product UI visibility never substitutes for authorization. |
| RE-FND-002 | StaffArr-backed action permissions | FOUNDATION | Target | Use product action keys, role scopes, delegated authority, and explicit denial reasons; ownership does not prevent permissioned delegated workflows. |
| RE-FND-003 | Unified suite shell | FOUNDATION | Target | Consistent navigation, page anatomy, terminology, responsive behavior, keyboard access, and light/dark contrast across products. |
| RE-FND-004 | Record lifecycle and history | FOUNDATION | Target | Human-readable statuses, timeline events, actor/time/source attribution, undo or correction paths, and immutable audit evidence. |
| RE-FND-005 | Quick create for missing references | FOUNDATION · UNDERSERVED | Target | Create the minimum valid cross-product reference without abandoning the current task; preserve context and allow later backfill. |
| RE-FND-006 | Saved views and personal preferences | COMMON | Target | Per-user filters, columns, density, sort, board/list preference, default landing view, and product-scoped preferences. |
| RE-FND-007 | Bulk operations with preview | COMMON | Target | Selection-aware actions, dry-run/impact preview, partial failure handling, downloadable results, and permission checks per record. |
| RE-FND-008 | Import with mapping and review | COMMON · UNDERSERVED | Target | Product-specific import separate from Smart Import, with templates, mappings, validation, dedupe, dry run, commit plan, and rollback/correction evidence. |
| RE-FND-009 | Export and portability | COMMON · DEMOCRATIZE | Target | CSV/PDF/JSON or package export appropriate to the domain, without trapping customer data behind enterprise tiers. |
| RE-FND-010 | Notifications, tasks, and inbox | COMMON | Target | Actionable notifications with ownership, due dates, escalation, snooze/delegation, deep links, and duplicate suppression. |
| RE-FND-011 | API, webhooks, and event outbox | COMMON · DEMOCRATIZE | Target | Stable versioned APIs, idempotency, service authentication, signed webhooks/events, retries, dead-letter handling, and correlation IDs. |
| RE-FND-012 | RecordArr evidence references | FOUNDATION | Target | Files and evidence are stored once in RecordArr and referenced by owning records; products do not create shadow document vaults. |
| RE-FND-013 | ReportArr projections and metrics | FOUNDATION | Target | Operational products emit facts/events; ReportArr builds read models and reports without becoming the source of operational truth. |
| RE-FND-014 | Compliance Core applicability and gates | FOUNDATION | Target | Products request explainable requirements, missing facts, evidence needs, and gate decisions rather than embedding independent legal logic. |
| RE-FND-015 | Field Companion execution surface | FOUNDATION · UNDERSERVED | Target | Mobile-first task, scan, capture, acknowledgement, and offline execution against the owning product API. |
| RE-FND-016 | Professional print and report view | COMMON | Target | Print output is a purpose-built report document without the app shell, not a screenshot of the current page. |
| RE-FND-017 | Accessible error and degraded states | FOUNDATION | Target | Explain what failed, what remains saved, whether retry is safe, what the user can do, and which administrator or integration owns the issue. |
| RE-FND-018 | Time, locale, and units | FOUNDATION | Target | Store canonical timestamps and units; display tenant/user locale, timezone, currency, and measurement preferences consistently. |
| RE-FND-019 | Retention, privacy, and legal hold awareness | FOUNDATION | Target | Product deletion/archive actions respect RecordArr retention, legal holds, privacy policy, and downstream references. |
| RE-FND-020 | AI-assisted proposals, never silent commits | UNDERSERVED | Target | AI may classify, summarize, suggest, or draft; consequential changes remain reviewable, permissioned, attributable, and reversible. |

## Cross-cutting nonfunctional requirements

| Area | Acceptance requirement |
| --- | --- |
| Security and tenancy | Every server operation validates tenant, identity/service principal, action permission, subject scope, and object tenant. Client-provided tenant, role, status, amount, or decision data is never trusted. |
| Auditability | Record actor/service, source, before/after or immutable event, reason, effective time, correlation/causation, version, approvals, overrides, and external calls. Audit logs are searchable but not user-editable. |
| Idempotency and concurrency | Commands support idempotency and optimistic concurrency or explicit conflict behavior. Retries, imports, events, and offline sync cannot create duplicate business effects. |
| Availability and degradation | Each dependency has timeout, retry/circuit behavior, health visibility, saved-state guarantees, and a user-readable degraded path. Safety/compliance/financial hard gates never silently fail open. |
| Privacy and data minimization | Collect only domain-required data, classify sensitive fields, restrict exports/logs/notifications, support retention and lawful correction/deletion, and avoid covert employee or device tracking. |
| Accessibility and responsive design | Meet keyboard, screen-reader, contrast, zoom/reflow, focus, error-identification, target-size, reduced-motion, and mobile requirements in both light and dark modes. |
| Performance | Use pagination/virtualization, asynchronous long jobs, bounded queries, indexes, backpressure, caching with invalidation, and measurable latency/error budgets. |
| Observability | Emit structured logs, metrics, traces, job/event status, dead-letter/quarantine state, dependency health, and correlation IDs without secrets or excessive personal data. |
| Configuration governance | Tenant configuration is versioned, validated, permissioned, explainable, testable, exportable, and recoverable. Product behavior is not hidden in hard-coded UI-only rules. |
| Integration contracts | APIs/events are versioned, documented, idempotent, tenant-scoped, effective-time aware, and backward-compatible within policy; no cross-product database foreign keys. |
| Data portability and professional output | Users can obtain useful structured exports and report-quality printable artifacts without the application shell or enterprise-only licensing. |
| AI safety and provenance | AI output is a proposal with source/context/confidence and human review. AI cannot reveal secrets, bypass permissions, invent records, or silently commit consequential changes. |

## Repository object inventory

<details>
<summary>Persistent entity sets (43)</summary>

| DbSet |
| --- |
| `RecordArrRecords` |
| `RecordArrFiles` |
| `RecordArrFileIntegrityChecks` |
| `RecordArrFileMalwareScans` |
| `RecordArrStorageReconciliations` |
| `RecordArrObjectStoreObjects` |
| `RecordArrObjectStoreFixityObservations` |
| `RecordArrDisasterRecoveryRuns` |
| `RecordArrRecordMetadata` |
| `RecordArrRecordLinks` |
| `RecordArrRecordComments` |
| `RecordArrUploadSessions` |
| `RecordArrCaptureRequests` |
| `RecordArrScanProcessing` |
| `RecordArrOcrResults` |
| `RecordArrExtractionResults` |
| `RecordArrEvidenceMappings` |
| `RecordArrPackages` |
| `RecordArrPackageManifests` |
| `RecordArrRetentionStatuses` |
| `RecordArrDisposalReviews` |
| `RecordArrDestructionCertificates` |
| `RecordArrRetentionSchedulerRuns` |
| `RecordArrRetentionSchedulerLeases` |
| `RecordArrRetentionSchedulerOutboxMessages` |
| `RecordArrLegalHolds` |
| `RecordArrControlledDocuments` |
| `RecordArrControlledDocumentVersions` |
| `RecordArrDocumentReviews` |
| `RecordArrDocumentDistributions` |
| `RecordArrDocumentAcknowledgements` |
| `RecordArrAccessPolicies` |
| `RecordArrAccessGrants` |
| `RecordArrExternalShares` |
| `RecordArrRedactions` |
| `RecordArrRedactionProviderJobs` |
| `RecordArrSignatureRecords` |
| `RecordArrSignatureTrustServiceJobs` |
| `RecordArrPhotoEvidence` |
| `RecordArrAccessLogs` |
| `RecordArrAccessHistorySeals` |
| `RecordArrAuditEvents` |
| `RecordArrAuditSeals` |

</details>

<details>
<summary>Frontend page files (1)</summary>

| Page |
| --- |
| src/LaunchPage.tsx |

</details>

<details>
<summary>Endpoint source families (4)</summary>

| Endpoint source file | Discovered route declarations |
| --- | --- |
| RecordArrIntegrationEndpoints.cs | 136 |
| WorkspaceEndpoints.cs | 124 |
| AuthEndpoints.cs | 6 |
| ReferenceIntegrationEndpoints.cs | 4 |

</details>

## Implementation order

| Phase | Exit objective |
| --- | --- |
| 0 — Boundary and durability | Remove shadow ownership, in-memory/static production paths, legacy access conflicts, cross-DB assumptions, and unaudited writes. Establish tenant-safe persistence and event/API contracts. |
| 1 — Current-path hardening | Make every currently implemented workflow complete, permissioned, observable, recoverable, accessible, and consistent in light/dark/mobile/print states. |
| 2 — Common baseline | Deliver the category-standard capabilities in the `COMMON` catalog with migrations, APIs, workflows, UI, reporting, imports/exports, and tests. |
| 3 — Underserved differentiation | Prioritize high-frequency friction, SMB affordability, transparent limits, quick create, evidence reuse, offline/mobile execution, and owner-respecting integration. |
| 4 — Enterprise democratization | Add advanced analytics, automation, optimization, collaboration, governance, and ecosystem functions without commercial feature withholding or opaque AI. |

### Immediate product priority

Replace the in-memory store with durable object storage/metadata/versioning, retention/legal hold, OCR/capture, controlled-document, sharing, and evidence APIs.

## Definition of done for every feature

- The owning domain, actor permissions, tenant boundary, state model, effective dates, concurrency, idempotency, and source references are explicit.
- Create, read, update/correct, archive/void/close, details, history, search/list, import/export, bulk action, notification, print/report, and API/event behavior exist where the domain permits them.
- The UI includes empty, loading, success, validation, permission-denied, conflict, dependency-down, partial-failure, and retry states in light/dark and responsive layouts.
- Quick create is available for missing permitted reference entities without abandoning the current operation.
- Cross-product reads and writes use authenticated APIs/events; no cross-product database foreign keys or UI-only write shortcuts are introduced.
- Audit, metrics, logs, traces, outbox/retry, data retention, accessibility, security, privacy, and automated tests meet the nonfunctional requirements above.
- AI, automation, optimization, and recommendation features expose inputs, assumptions, confidence, alternatives, and approval; they never silently commit consequential records.

## Related workflow specification

The operational state machines, triggers, actors, steps, exceptions, evidence, handoffs, mobile behavior, and measures are defined in [WORKFLOWS.md](./WORKFLOWS.md).
