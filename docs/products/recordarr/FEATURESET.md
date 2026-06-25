# RecordArr — DMS Full Feature Set

[Workflow catalog](./WORKFLOWS.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md)

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

> **Implementation reality — Scaffold:** RecordArr has an extensive in-memory domain and UI/API contract covering records, files, renditions, upload sessions, capture requests, scans, OCR, extraction, evidence mapping/coverage, packages, metadata, links, comments, retention, disposal, legal holds, controlled documents/versions/reviews/distributions/acknowledgements, access policies/grants, external shares, redactions, signatures, photo evidence, and access logs. The EF context does not expose a durable records domain. Durable object storage, database persistence, immutable audit, content scanning, encryption, retention enforcement, and disaster recovery are production prerequisites.

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
- The current in-memory implementation is a prototype. Durable object storage, metadata persistence, audit, retention, and disaster recovery are release blockers.
- Controlled-document acknowledgement is distinct from TrainArr competence/qualification.
- Disposition is an approval workflow with holds and source-record checks, never a simple delete button.

## Repository implementation snapshot

| Static indicator | Count |
| --- | --- |
| Persistent DbSet declarations | 0 |
| Discovered server classes | 101 |
| Discovered HTTP route declarations | 186 |
| Frontend source files | 10 |
| Frontend page files | 1 |
| Documentation headings | 90 |

### Evidence used for the current-state classification

- recordarr-api contains an in-memory RecordArrStore covering records/files/renditions, upload/capture/scan/OCR/extraction, evidence mappings/coverage/packages, metadata/links/comments, retention/disposal/legal holds, controlled docs/versions/reviews/distributions/acknowledgements, access/grants/shares/redactions/signatures/photo evidence/access logs.
- The RecordArr EF context does not expose durable operational entities; state is not production-durable across restart.
- recordarr-frontend routes cover records, capture, controlled docs, reviews, distributions, acknowledgements, packages, retention/disposal/legal holds, access/shares/redactions/logs/settings.
- Canonical product docs define record/file/document, capture/scan/OCR, evidence/package/retention, controlled-document/access, and workflow/API contracts.

> Counts are static-discovery indicators, not proof that every route or screen is complete, reachable, secure, migrated, or production-ready.

## Mandatory migration or refactor work

- Replace the process-local RecordArrStore with durable metadata plus approved object storage; add versioning, hashing, retention, hold, access, malware processing, and immutable audit.

## Feature catalog

### A. Currently implemented or meaningfully represented

These capabilities have repository evidence. Their state follows the product-level maturity and may still require hardening or completion.

| Feature ID | Capability | Class | State | Required behavior / evidence |
| --- | --- | --- | --- | --- |
| RE-CUR-001 | Record and file prototype | CURRENT | Scaffold | In-memory record/file/version/rendition structures demonstrate intended behavior. |
| RE-CUR-002 | Upload-session and capture-request prototype | CURRENT | Scaffold | Contracts support secure upload and requested capture flows. |
| RE-CUR-003 | Scan, OCR, and extraction prototype | CURRENT | Scaffold | Scan/image/OCR/extraction review concepts and routes are present. |
| RE-CUR-004 | Metadata, links, and comments prototype | CURRENT | Scaffold | Records can be classified and related to domain records. |
| RE-CUR-005 | Controlled document/version prototype | CURRENT | Scaffold | Draft/review/approval/publication/supersession concepts are represented. |
| RE-CUR-006 | Distribution and acknowledgement prototype | CURRENT | Scaffold | Controlled distribution and recipient acknowledgement are modeled. |
| RE-CUR-007 | Evidence mapping and coverage prototype | CURRENT | Scaffold | Evidence-to-requirement linkage and coverage concepts are present. |
| RE-CUR-008 | Package and manifest prototype | CURRENT | Scaffold | Packages can group records/evidence for audit or handoff. |
| RE-CUR-009 | Retention, disposal, and legal-hold prototype | CURRENT | Scaffold | Retention/disposition/hold concepts exist in the in-memory domain. |
| RE-CUR-010 | Access policy, grant, and external-share prototype | CURRENT | Scaffold | Scoped access, expiring shares, and access logging are represented. |
| RE-CUR-011 | Redaction and signature prototype | CURRENT | Scaffold | Redaction and electronic-signature concepts are present. |
| RE-CUR-012 | Photo evidence prototype | CURRENT | Scaffold | Field/mobile evidence capture has a modeled destination. |
| RE-CUR-013 | Comprehensive DMS navigation scaffold | CURRENT | Scaffold | Frontend routes cover the expected document and records operating model. |

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
<summary>Persistent entity sets (0)</summary>

_No persistent product DbSet declarations were found in the static inventory._

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
| RecordArrIntegrationEndpoints.cs | 94 |
| WorkspaceEndpoints.cs | 82 |
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
