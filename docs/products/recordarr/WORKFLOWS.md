# RecordArr — DMS Workflow Catalog

[Feature catalog](./FEATURESET.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md) · [Rollout](./ROLLOUT.md)

## Workflow contract

This document defines the end-to-end business state machines for RecordArr. A workflow is not complete because a page or endpoint exists: the trigger, authority, state transitions, validation, handoffs, evidence, exceptions, retries, conflict behavior, notifications, reporting, and final source-of-truth commit must all work together.

## Ownership rule

RecordArr is the suite document, evidence, and records system of record. It owns record metadata, file objects and renditions, capture/scan/OCR/extraction processing, controlled document versions and distribution, evidence mapping, packages/manifests, access/sharing/redaction/signature, retention/disposition/legal holds, and access audit. Domain products own the business record that a document supports and reference RecordArr IDs rather than storing duplicate files.

- The operational meaning or lifecycle of a work order, person, order, supplier, customer, trip, quality case, training assignment, compliance finding, or financial transaction.
- Compliance evidence sufficiency or applicability; Compliance Core decides whether referenced evidence satisfies a requirement.
- Training distribution/qualification; TrainArr owns assignments and completion while RecordArr stores controlled content/evidence.
- Business approvals unrelated to document control; owning products retain their own decisions and reference documents.
- General-purpose reporting or analytics; ReportArr owns reports/read models.

## Workflow index

| Workflow ID | Workflow | Class | State | Trigger |
| --- | --- | --- | --- | --- |
| RE-WF-001 | Create record and upload a file version | CURRENT · COMMON | Partial | Authorized user/product begins upload or creates a record placeholder and queues scan processing. |
| RE-WF-002 | Mobile scan, auto-crop, OCR, classify, and review | CURRENT · UNDERSERVED | Partial | User opens RecordArr capture or a product issues a capture request. |
| RE-WF-003 | Email, portal, integration, and batch intake routing | COMMON · UNDERSERVED | Target | Authorized mailbox, portal, API, scanner, or batch import receives content. |
| RE-WF-004 | Controlled document author, review, approve, publish, and supersede | CURRENT · COMMON | Partial | Document owner creates or revises a controlled document and can route review. |
| RE-WF-005 | Controlled distribution and acknowledgement | CURRENT · COMMON | Partial | A controlled document version becomes effective or distribution population changes, and users can create distribution and acknowledgement records from the workspace. |
| RE-WF-006 | Evidence mapping and requirement coverage | CURRENT · UNDERSERVED | Partial | User/product/Compliance Core requests evidence mapping, and RecordArr can create mappings and show resulting coverage from the record detail workspace. |
| RE-WF-007 | Evidence/audit package assembly and lock | CURRENT · COMMON | Partial | Product, auditor, customer, regulator, or internal owner requests a package, and the package workspace can assemble, lock, inspect, and export it. |
| RE-WF-008 | External share/data room | CURRENT · UNDERSERVED | Partial | Authorized owner creates a share or collaboration room, logs access, and expires or revokes access. |
| RE-WF-009 | Redaction review and release | CURRENT · COMMON | Partial | A package/share/export/request can create a redacted copy while preserving the source record. |
| RE-WF-010 | Electronic signature request and completion | CURRENT · COMMON | Partial | Owning product or document owner requests one/more signatures. |
| RE-WF-011 | Retention classification and event trigger | CURRENT · COMMON | Partial | Record is created/classified or a triggering business event occurs. |
| RE-WF-012 | Disposition review and defensible deletion | CURRENT · COMMON | Partial | Scheduled worker identifies records past retention eligibility. |
| RE-WF-013 | Legal hold create, preserve, notify, and release | CURRENT · COMMON | Partial | Authorized legal/compliance user opens a matter/hold and can create, activate, and release a hold from the workspace. |
| RE-WF-014 | Controlled document periodic review | COMMON | Target | Scheduled review date approaches or a change/event triggers early review. |
| RE-WF-015 | Disaster recovery and integrity verification | FOUNDATION · DEMOCRATIZE | Target | Scheduled fixity/restore test, storage alert, security incident, or disaster occurs. |

## Universal workflow requirements

- **Authority:** resolve user/service identity, tenant, action permission, organizational/record scope, delegation, and separation of duties on the server.
- **State:** use explicit human-readable states and legal transitions; never infer final completion solely from a screen closing or an external request being sent.
- **Idempotency:** retries, double-clicks, event replay, import retry, webhooks, and offline sync cannot create duplicate effects.
- **Concurrency:** stale edits receive a conflict with current context and permitted resolution; never silently last-write-wins consequential data.
- **Evidence:** retain actor, source, version, time, reason, input/output, approvals, external calls, attachments by RecordArr reference, and correlation/causation.
- **Handoffs:** the receiving product accepts/rejects explicitly and emits an outcome; the sender does not mark downstream work complete merely because it dispatched a request.
- **Degradation:** state what is saved, what failed, whether retry is safe, and the manual or alternate path. Safety/compliance/financial hard gates never silently fail open.
- **Notifications:** notify only actionable audiences, deduplicate, respect preference/urgency/quiet-hour policy, escalate, and deep-link through a fresh permission check.
- **Mobile/offline:** only server-declared offline-safe actions queue; final authorization, concurrency, references, and hard gates are revalidated by the owning product.
- **Reporting:** emit events/facts to ReportArr with source/effective time and data-quality state; ReportArr never substitutes for the operational record.

## RE-WF-001 — Create record and upload a file version

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Store a file once with trustworthy metadata, checksums, access, and domain references. |
| Trigger | Authorized user/product begins upload or creates a record placeholder. |

### Actors

- Uploader
- Record owner/reviewer
- RecordArr

### State path

`session_created → uploading → scanning → quarantined → processing → review → available → rejected`

### Required sequence

1. Create upload session with tenant, purpose, expected type/size/hash, target record/class, and expiry.
2. Upload directly to protected object storage using a scoped token.
3. Verify size/hash/MIME, scan for malware, and quarantine unsafe content.
4. Create record/file/version metadata with source, owner, confidentiality, retention, and domain refs.
5. Generate preview/renditions asynchronously.
6. Run OCR/classification/extraction when configured.
7. Require metadata/review before publishing to consumers.
8. Emit record/version events and preserve immutable access/audit.

### Exception and recovery paths

- Upload interrupted, hash mismatch, malware, unsupported format, duplicate file, no permission, retention class unknown, or object storage unavailable.
- A placeholder may exist until evidence is supplied.

### Cross-product and external handoffs

- Product/Field Companion → RecordArr.
- RecordArr → owning product: record ref/status.
- RecordArr → ReportArr/Compliance Core: metadata/evidence events.

### Evidence and audit record

- Upload/session/source.
- Hash/scan result.
- Metadata/version.
- Processing/review/publication.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Upload success.
- Malware/quarantine.
- Time to preview.
- Metadata completeness.
- Duplicate prevention.

## RE-WF-002 — Mobile scan, auto-crop, OCR, classify, and review

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Partial |
| Purpose | Turn paper or photographed evidence into a clean, searchable, correctly filed record. |
| Trigger | User opens RecordArr capture or a product issues a capture request. |

### Actors

- Field worker
- Records reviewer
- Field Companion
- RecordArr

### State path

`requested → capturing → queued → processing → review → approved → rejected`

### Required sequence

1. Load capture request with required document class/type/subtype, defaulting from active vocabulary when blank, plus subject refs, page expectations, and evidence guidance.
2. Capture one/more images or import file; detect edges, auto-crop, deskew, dewarp, rotate, enhance, and assess blur/glare/cutoff.
3. Let user reshoot or confirm quality before upload.
4. Upload securely or queue encrypted offline capture.
5. Generate searchable PDF/renditions and OCR text with page coordinates/confidence.
6. Classify/split batch and extract proposed metadata/fields.
7. Reviewer compares image and proposals, corrects, links to domain records, and approves.
8. Publish record and return evidence reference to requesting workflow.

### Exception and recovery paths

- Poor image, missing page, handwritten/unsupported text, duplicate document, offline conflict, wrong subject, sensitive information, or extraction low confidence.
- Multiple documents captured in one batch.

### Cross-product and external handoffs

- Field Companion ↔ NexArr/RecordArr.
- RecordArr → requesting product.
- Compliance Core: evidence candidate.

### Evidence and audit record

- Capture request/version.
- Original images/transformations.
- OCR/classification/extraction.
- Reviewer corrections/link.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- First-capture pass.
- OCR confidence.
- Review time.
- Misclassification.
- Reshoot rate.

## RE-WF-003 — Email, portal, integration, and batch intake routing

| Field | Definition |
| --- | --- |
| Classification | COMMON · UNDERSERVED |
| Implementation state | Target |
| Purpose | Route incoming documents to the right record/product with review instead of unmanaged shared inboxes. |
| Trigger | Authorized mailbox, portal, API, scanner, or batch import receives content. |

### Actors

- Records intake worker
- Reviewer
- RecordArr
- NexArr Smart Import

### State path

`received → security_scan → classification → matching → review → committed → unmatched → rejected`

### Required sequence

1. Authenticate source and create intake envelope with sender/channel/message metadata.
2. Extract attachments and safe body content; reject unsupported/malicious items.
3. Deduplicate by hash/message/external ID and group related files.
4. Classify document type, likely product, subject record, sensitivity, and required metadata with confidence.
5. Search for candidate customer/supplier/person/order/work/etc. through allowed APIs.
6. Present create/link/update/ignore proposals to reviewer.
7. Commit approved record and product references idempotently.
8. Track unmatched/failed items in an owned queue with escalation.

### Exception and recovery paths

- Spoofed sender, encrypted/password file, malicious content, ambiguous product, multiple candidate records, duplicate, or target product unavailable.
- Email body itself may be the record.

### Cross-product and external handoffs

- Mailbox/portal/integration → RecordArr/NexArr.
- RecordArr ↔ product match/commit APIs.
- ReportArr: intake metrics.

### Evidence and audit record

- Source/message/hash.
- Security/classification/match.
- Review decision.
- Record/product commit.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Straight-through routing.
- Unmatched aging.
- Duplicate suppression.
- Security rejection.
- Reviewer override.

## RE-WF-004 — Controlled document author, review, approve, publish, and supersede

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Maintain one effective controlled version with complete review and distribution history. |
| Trigger | Document owner creates or revises a controlled document. |

### Actors

- Author
- Document owner
- Reviewers/approvers
- RecordArr

### State path

`draft → review → changes_requested → approved → scheduled → effective → superseded → obsolete`

### Required sequence

1. Create draft from template or prior effective version with owner, type, purpose, scope, review cycle, and change reason.
2. Edit/upload working version and compare to prior.
3. Route parallel/sequential technical, quality, compliance, legal, and owner review.
4. Resolve comments and create a final candidate version.
5. Obtain required signatures/approvals and set effective/transition date.
6. Publish as effective; freeze prior effective version and mark superseded/obsolete per policy.
7. Trigger distribution, acknowledgement, training/change-impact, and point-of-use updates.
8. Schedule periodic review and retain all prior versions.

### Exception and recovery paths

- Conflicting reviewers, required approver unavailable, effective date before training, broken links, unsigned changes, or emergency temporary revision.
- Printed controlled copies require issue/recall tracking.

### Cross-product and external handoffs

- RecordArr → TrainArr/AssurArr/Compliance Core/products: change impact/publication.
- RecordArr ↔ e-sign provider.
- ReportArr: document control metrics.

### Evidence and audit record

- Draft/version diff.
- Comments/resolutions.
- Approvals/signatures.
- Effective/supersession/distribution.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Review cycle.
- Overdue reviews.
- Unacknowledged distribution.
- Obsolete use attempts.
- Emergency revision rate.

## RE-WF-005 — Controlled distribution and acknowledgement

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Deliver the effective document to the right population and prove receipt/understanding requirement. |
| Trigger | A controlled document version becomes effective or distribution population changes. |

### Actors

- Document owner
- Recipients
- Managers
- RecordArr
- StaffArr/TrainArr

### State path

`planned → distributed → viewed → acknowledged → overdue → superseded → closed`

### Required sequence

1. Resolve distribution population by person/role/location/team/customer/supplier/process with effective snapshot.
2. Create recipient records and determine read, acknowledge, attest, quiz, or training requirement.
3. Notify with deep link to the exact effective version.
4. Record view, acknowledgement, signature, comments/questions, and offline validation.
5. Remind/escalate overdue recipients while respecting leave/inactive state.
6. Handle reassignment, new hires/transfers, and superseding version.
7. Block point-of-use actions only when owning product policy requires.
8. Close distribution with coverage and exception report.

### Exception and recovery paths

- Recipient inactive/on leave, no login, accessibility/language issue, disputes content, document superseded during campaign, or external recipient access expires.
- Acknowledgement is not equivalent to competency unless TrainArr assessment is required.

### Cross-product and external handoffs

- StaffArr → RecordArr: population.
- RecordArr → TrainArr/products: requirement/status.
- NexArr: notifications/access.

### Evidence and audit record

- Population snapshot.
- Version delivered.
- Views/ack/signatures.
- Reminders/exceptions.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Coverage.
- Time to acknowledge.
- Overdue rate.
- Version mismatch.
- Accessibility issues.

## RE-WF-006 — Evidence mapping and requirement coverage

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Partial |
| Purpose | Map records to requirements and show coverage without RecordArr deciding compliance. |
| Trigger | User/product/Compliance Core requests evidence mapping. |

### Actors

- Evidence reviewer
- Compliance analyst
- RecordArr
- Compliance Core

### State path

`requested → candidates → mapped → evaluation → covered → partial → missing → stale`

### Required sequence

1. Receive requirement, subject, period, evidence type, minimum attributes, and access scope.
2. Search authorized records by metadata/content/relationships and suggest candidates with reasons.
3. Reviewer links one/more records and specifies what each proves, coverage period, limitations, and confidence.
4. Compliance Core evaluates sufficiency/currentness/combination rules.
5. Show covered, partial, missing, stale, conflicting, or review-required status.
6. Create evidence request/capture task for gaps.
7. Track replaced/superseded/disposed evidence and re-evaluate.
8. Preserve mapping and evaluation history.

### Exception and recovery paths

- Record inaccessible, version obsolete, period mismatch, evidence supports only part, duplicate/conflicting records, retention disposal scheduled, or legal hold restricts sharing.
- Same record may evidence multiple requirements with separate mappings.

### Cross-product and external handoffs

- Compliance Core ↔ RecordArr.
- RecordArr → owning products/Field Companion: evidence requests.
- ReportArr: coverage metrics.

### Evidence and audit record

- Requirement/source.
- Candidate rationale.
- Mapping/limitations.
- Evaluation result/history.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Coverage rate.
- Stale evidence.
- Reviewer time.
- False candidate rate.
- Gap closure.

## RE-WF-007 — Evidence/audit package assembly and lock

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Assemble a complete, immutable, verifiable package from cross-product records. |
| Trigger | Product, auditor, customer, regulator, or internal owner requests a package. |

### Actors

- Package owner
- Reviewer
- External recipient
- RecordArr

### State path

`requested → collecting → validation → gap_review → assembled → locked → shared → superseded`

### Required sequence

1. Define purpose, scope, recipient, date/time snapshot, record categories, requirements, redaction, and expiry.
2. Collect direct records and product-provided snapshots/refs; do not copy live operational truth without provenance.
3. Validate required items, versions, signatures, hashes, retention/hold, access, and missing evidence.
4. Resolve gaps or record accepted exceptions.
5. Generate manifest with source product/record/version/time/hash and package version.
6. Render/download files, professional index/report, and machine-readable manifest.
7. Lock/finalize package, share through scoped access, and log downloads.
8. Create supplemental package versions without mutating the original.

### Exception and recovery paths

- Missing file, inaccessible source, document superseded after snapshot, sensitive data, package too large, external share not allowed, or recipient identity fails.
- Package may contain links rather than copies for internal dynamic review.

### Cross-product and external handoffs

- Products → RecordArr: snapshots/refs.
- RecordArr ↔ NexArr: recipient access.
- ReportArr: package analytics.

### Evidence and audit record

- Scope/snapshot.
- Manifest/source/hash.
- Validation/exceptions/redactions.
- Lock/share/access/supplements.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Generation time.
- Missing item.
- Verification success.
- Unauthorized access.
- Supplement rate.

## RE-WF-008 — External share/data room

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Scaffold |
| Purpose | Share selected documents securely with customers, suppliers, auditors, or partners. |
| Trigger | Authorized owner creates a share or collaboration room. |

### Actors

- Internal owner
- External recipient
- RecordArr
- NexArr

### State path

`draft → invited → active → expired → revoked → closed`

### Required sequence

1. Select records/versions and verify share authority, confidentiality, purpose, and recipient relationship.
2. Set recipient identities/domains, actions, watermark, download/print, expiry, passcode/MFA, access count, and revocation policy.
3. Issue scoped invitation through NexArr or secure token flow.
4. Recipient verifies identity, accepts terms, and sees only selected content.
5. Track views/downloads/questions/uploads/acknowledgements.
6. Review or ingest external uploads through controlled intake.
7. Revoke/expire and propagate access immediately.
8. Retain full share/access audit and export.

### Exception and recovery paths

- Forwarded invitation, wrong recipient, expired link, download prohibited, document superseded, recipient account compromised, or legal hold/privacy change.
- Public anonymous links are disallowed for sensitive records.

### Cross-product and external handoffs

- RecordArr ↔ NexArr/notification provider.
- RecordArr ↔ owning product portal/case.
- ReportArr: access metrics.

### Evidence and audit record

- Authority/selection/version.
- Share policy/invite.
- Recipient actions/uploads.
- Revocation/expiry.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Activation.
- Unauthorized attempts.
- Share aging.
- Revocation latency.
- External completion.

## RE-WF-009 — Redaction review and release

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Create a safe disclosed rendition while protecting the original. |
| Trigger | A package/share/export/request requires information removal. |

### Actors

- Redactor
- Reviewer/approver
- RecordArr

### State path

`draft → review → approved → released → superseded → rejected`

### Required sequence

1. Select exact file version and disclosure purpose/authority.
2. Create non-destructive redaction annotations with reason/category and page coordinates.
3. Search for recurring sensitive text/patterns and propose additional redactions.
4. Generate flattened redacted rendition that removes underlying text/images/metadata as required.
5. Reviewer compares original and rendition, checks missed/over-redaction, and approves.
6. Lock redaction set/rendition with hash and approval.
7. Use only approved rendition in package/share/export.
8. Preserve original under stricter access and audit every view.

### Exception and recovery paths

- OCR inaccurate, hidden layers/attachments/comments, spreadsheet cells, digital signature invalidated, multimedia, over-redaction, or source changes.
- Legal privilege review may require separate access group.

### Cross-product and external handoffs

- RecordArr ↔ package/share/export workflows.
- Compliance Core/privacy owner may provide policy refs.

### Evidence and audit record

- Purpose/authority.
- Redaction annotations/reasons.
- Rendered hash/validation.
- Review/approval/use.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Redaction cycle.
- Miss/over-redaction findings.
- Rendition validation.
- Original access.

## RE-WF-010 — Electronic signature request and completion

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Obtain attributable signatures on a fixed document version and preserve evidence. |
| Trigger | Owning product or document owner requests one/more signatures. |

### Actors

- Requester
- Signers
- RecordArr
- E-sign provider/NexArr

### State path

`draft → sent → viewed → signed → declined → expired → voided → completed`

### Required sequence

1. Freeze the document/version and calculate hash.
2. Define signers, order, role, signature meaning, fields, due date, authentication, and reminders.
3. Validate signer authority/contact and prevent requester/self-approval conflicts where policy requires.
4. Send through approved provider or native controlled flow.
5. Signer authenticates, reviews exact version, consents, and signs/declines.
6. Capture provider certificate, timestamps, IP/device as permitted, and tamper evidence.
7. Finalize signed rendition/package and notify owning product.
8. Handle expiration, decline, correction, void, or new version through new request.

### Exception and recovery paths

- Signer unauthorized/unavailable, email forwarded, document changes, provider outage, identity verification failure, signature field error, or regulatory requirement unsupported.
- Wet signature scan is captured as evidence but distinguished from digital signature.

### Cross-product and external handoffs

- RecordArr ↔ e-sign provider/NexArr.
- RecordArr → owning product: status/completed record.

### Evidence and audit record

- Document/version/hash.
- Signer/authority/authentication.
- Events/certificate.
- Final signed artifact/status.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Completion time.
- Decline/expiry.
- Authentication failures.
- Version mismatch prevented.

## RE-WF-011 — Retention classification and event trigger

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Assign and calculate retention based on the correct record class and business event. |
| Trigger | Record is created/classified or a triggering business event occurs. |

### Actors

- Records manager
- Owning product
- RecordArr

### State path

`unclassified → classified → trigger_pending → retained → eligible → held`

### Required sequence

1. Determine record class from document type, owner, subject, jurisdiction, contract/process, and policy.
2. Apply approved retention schedule/version and explain rationale.
3. Resolve trigger as creation, closure, termination, expiration, supersession, last activity, or product-provided event.
4. Calculate earliest eligibility and review/hold requirements.
5. Detect conflicts among multiple schedules and choose stricter/authorized rule.
6. Recalculate when trigger or classification changes, preserving prior calculation.
7. Notify owner of missing trigger or approaching eligibility.
8. Prevent disposition under active legal hold or unresolved relationship.

### Exception and recovery paths

- Record unclassified, trigger never received, multiple jurisdictions, legal hold, permanent record, changed schedule, or linked records have different retention.
- AI may propose class but records manager approves high-impact classification.

### Cross-product and external handoffs

- Owning products → RecordArr: lifecycle events.
- Compliance Core → RecordArr: policy refs.
- ReportArr: retention metrics.

### Evidence and audit record

- Class/schedule/version.
- Trigger/source.
- Calculations/conflicts.
- Changes/notifications.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Unclassified rate.
- Missing triggers.
- Recalculation.
- Over-retention/early-disposal prevention.

## RE-WF-012 — Disposition review and defensible deletion

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Dispose eligible records only after legal, business, and ownership checks. |
| Trigger | Scheduled worker identifies records past retention eligibility. |

### Actors

- Records manager
- Record owner
- Legal/hold reviewer
- Approver
- RecordArr

### State path

`eligible → review → approved → extended → destroying → disposed → exception`

### Required sequence

1. Create disposition batch by schedule/class/owner/period with snapshot and counts.
2. Revalidate retention, triggers, legal holds, investigations, audits, active operational refs, privacy restrictions, and transfer needs.
3. Notify owners/reviewers and collect approve/extend/transfer/exception decisions.
4. Require separation of duties for destruction approval.
5. Generate destruction plan and verify backup/object lifecycle implications.
6. Delete or anonymize according to policy; preserve minimal tombstone and certificate without recoverable content.
7. Update references with disposed status and emit events.
8. Finalize destruction certificate/package and audit.

### Exception and recovery paths

- Active hold, source product still needs record, owner unavailable, conflicting schedule, immutable backup constraints, external share, or deletion failure.
- Some records transfer to archive rather than destroy.

### Cross-product and external handoffs

- RecordArr ↔ owning products/Compliance Core/NexArr/object storage.
- RecordArr → ReportArr: disposition metrics.

### Evidence and audit record

- Batch/snapshot.
- Revalidation/decisions.
- Destruction execution/results.
- Certificate/tombstones.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Disposition cycle.
- Held/extended rate.
- Deletion failures.
- Early-disposal prevention.
- Overdue eligible volume.

## RE-WF-013 — Legal hold create, preserve, notify, and release

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Preserve relevant records and suspend disposition for a legal/investigation matter. |
| Trigger | Authorized legal/compliance user opens a matter/hold. |

### Actors

- Legal/hold administrator
- Custodians/owners
- Records manager
- RecordArr

### State path

`draft → active → notices_open → collecting → monitoring → released → closed`

### Required sequence

1. Create matter/hold with authority, scope, dates, custodians, systems/products, subjects, keywords/categories, and confidentiality.
2. Identify candidate records across metadata/relationships/search and snapshot collection criteria.
3. Apply hold to records and future matching content; suspend disposition/object deletion.
4. Issue notices/acknowledgements to custodians and track reminders/escalation.
5. Collect/preserve exports or immutable copies where policy requires with chain of custody.
6. Monitor scope changes, departures, new records, and preservation health.
7. Approve release/narrowing after matter closure and recalculate retention eligibility.
8. Retain hold history, notices, collection, release, and audit package.

### Exception and recovery paths

- Overbroad/ambiguous scope, custodian inactive, product unavailable, missing records, privacy conflict, object deleted before hold, or hold overlaps another matter.
- Legal privilege restricts access to hold details.

### Cross-product and external handoffs

- RecordArr ↔ all products/StaffArr/NexArr.
- ReportArr: hold coverage.
- External eDiscovery provider if configured.

### Evidence and audit record

- Authority/scope/version.
- Matched records/holds.
- Notices/acks.
- Collections/integrity.
- Release/recalculation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to apply.
- Notice acknowledgement.
- Preservation gaps.
- Released-record recalculation.
- Unauthorized access.

## RE-WF-014 — Controlled document periodic review

| Field | Definition |
| --- | --- |
| Classification | COMMON |
| Implementation state | Target |
| Purpose | Confirm an effective document remains accurate and appropriate before its review due date. |
| Trigger | Scheduled review date approaches or a change/event triggers early review. |

### Actors

- Document owner
- Subject reviewer
- Quality/compliance reviewer
- RecordArr

### State path

`due → in_review → reaffirmed → revision_open → obsolete → overdue`

### Required sequence

1. Notify owner with current version, prior changes, linked incidents/CAPA/rules/training/workflow feedback, and usage.
2. Confirm scope, accuracy, references, contacts, formatting, accessibility, and continuing need.
3. Choose reaffirm with reason, revise, supersede/merge, or obsolete.
4. Obtain required review/approval/signature.
5. If revised, launch controlled document workflow and impact analysis.
6. If reaffirmed, record evidence and next review date without creating a fake content revision.
7. If obsolete, ensure replacement/point-of-use/distribution handling.
8. Close review and report overdue risk.

### Exception and recovery paths

- Owner left, linked regulation changed, replacement not ready, active training uses version, open CAPA/change, or document no longer accessible.
- Emergency extension requires approval and reason.

### Cross-product and external handoffs

- RecordArr ↔ AssurArr/TrainArr/Compliance Core/products.
- NexArr: tasks/reminders.
- ReportArr: review metrics.

### Evidence and audit record

- Review inputs.
- Decision/reason/approvals.
- Impact/workflow refs.
- Next review/closure.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- On-time review.
- Reaffirm/revise/obsolete mix.
- Overdue aging.
- Change-triggered reviews.

## RE-WF-015 — Disaster recovery and integrity verification

| Field | Definition |
| --- | --- |
| Classification | FOUNDATION · DEMOCRATIZE |
| Implementation state | Target |
| Purpose | Prove records can be restored intact and audit evidence survives failures. |
| Trigger | Scheduled fixity/restore test, storage alert, security incident, or disaster occurs. |

### Actors

- Records/platform administrator
- Security/operations
- RecordArr

### State path

`healthy → alert → degraded → recovery → reconciliation → restored → remediation`

### Required sequence

1. Continuously verify object checksums, metadata/object consistency, replica/backup status, encryption keys, and orphaned objects.
2. Select representative restore test scope including versions, renditions, metadata, links, audit, holds, and packages.
3. Restore into isolated environment and verify hashes, permissions, search, retention/hold, and package validity.
4. For incident, contain affected storage/services and preserve logs.
5. Fail over according to documented RPO/RTO and communicate degraded functionality.
6. Reconcile writes/uploads queued during outage.
7. Correct integrity issues with traceable recovery rather than silent replacement.
8. Document test/incident results and remediation.

### Exception and recovery paths

- Corrupt backup, lost key, partial object/metadata recovery, ransomware, regional outage, queued upload duplicate, or retention action ran during failure.
- Legal hold data has heightened recovery priority.

### Cross-product and external handoffs

- RecordArr ↔ storage/backup/KMS/security/NexArr/ReportArr.
- Products → RecordArr: queued references.

### Evidence and audit record

- Integrity/fixity logs.
- Restore test selection/results.
- Incident/timeline.
- Reconciliation/corrections.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Fixity success.
- Restore test success.
- RPO/RTO.
- Orphan/corrupt count.
- Post-recovery mismatch.



## Workflow definition of done

A workflow is releasable only when its happy path, every listed exception, dependency outage, duplicate/retry, stale/concurrent update, permission denial, cancellation, correction/void, archival/retention, import/API/event path, notification, audit trail, reporting projection, mobile behavior, and professional printable output have automated and human-usable acceptance coverage.
