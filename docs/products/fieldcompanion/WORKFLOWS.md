# Field Companion — MAM Workflow Catalog

[Feature catalog](./FEATURESET.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md) · [Rollout](./ROLLOUT.md)

## Workflow contract

This document defines the end-to-end business state machines for Field Companion. A workflow is not complete because a page or endpoint exists: the trigger, authority, state transitions, validation, handoffs, evidence, exceptions, retries, conflict behavior, notifications, reporting, and final source-of-truth commit must all work together.

## Ownership rule

Provide a secure, unified, mobile-first execution surface for work owned by the STL products: discover assignments, scan context, capture evidence, complete permitted actions, communicate, and synchronize safely under poor connectivity. Field Companion is an application/action layer, not a new source of operational truth and not a full device-management replacement.

- People, assets, work orders, training, transport, inventory, suppliers, customers, orders, finance, quality, documents, analytics, or compliance decisions.
- A parallel mobile database that becomes authoritative while disconnected; owning products validate and commit every domain action.
- Tenant role definitions or domain authorization.
- Full mobile device management such as hardware enrollment, OS patch enforcement, carrier management, or organization-wide device inventory unless supplied by an external MDM provider.
- Unbounded employee tracking, covert location collection, or access to unrelated personal device data.

## Workflow index

| Workflow ID | Workflow | Class | State | Trigger |
| --- | --- | --- | --- | --- |
| FC-WF-001 | Launch Field Companion and establish a secure session | CURRENT · FOUNDATION | Partial | User opens the PWA/web app, follows a product deep link, or taps a notification. |
| FC-WF-002 | Discover and accept mobile work | CURRENT · COMMON | Partial | User opens My Work/Inbox, receives a notification, scans context, or work is assigned/available. |
| FC-WF-003 | Scan a code and resolve context | CURRENT · COMMON | Partial | User opens Scan or a workflow requests a scan step. |
| FC-WF-004 | Capture and submit field evidence | CURRENT · COMMON | Partial | A product workflow requires or permits photo, video, audio, file, signature, note, measurement, or document evidence. |
| FC-WF-005 | Execute an offline-safe action and synchronize | CURRENT · UNDERSERVED | Partial | User downloads eligible work or connectivity drops during an explicitly offline-capable task. |
| FC-WF-006 | Resolve a mobile synchronization conflict | UNDERSERVED · FOUNDATION | Partial | The owning product rejects an offline/queued action because the source record or authorization changed. |
| FC-WF-007 | Open a push notification and complete its action | CURRENT · COMMON | Partial | NexArr/product notification service sends an event to a registered push subscription. |
| FC-WF-008 | Clock in/out or record labor from mobile | CURRENT · COMMON | Partial | User starts/ends shift, break, travel, task labor, or corrects a missed event through the Clock surface. |
| FC-WF-009 | Report an issue, incident, defect, or observation | CURRENT · UNDERSERVED | Partial | User taps Report, scans context, or an active task offers a report action. |
| FC-WF-010 | Complete a mobile form, checklist, or attestation | COMMON · FOUNDATION | Target | Owning product assigns a form/checklist/inspection/assessment/attestation step. |
| FC-WF-011 | Collect a signature or acknowledgement | COMMON · UNDERSERVED | Partial | A product workflow requests a receipt, inspection, delivery, training, policy, quality, customer, supplier, maintenance, or other acknowledgement/signature. |
| FC-WF-012 | Use a one-time external capture link | UNDERSERVED | Target | An authorized product user requests a customer, supplier, applicant, witness, consignee, auditor, or other external party action. |
| FC-WF-013 | Apply app-protection policy and selective wipe | DEMOCRATIZE · FOUNDATION | Partial | User signs in, policy changes, risk signal arrives, membership ends, device is lost, or administrator issues a wipe/revoke command. |
| FC-WF-014 | Operate on a shared or kiosk device | COMMON · DEMOCRATIZE | Partial | A user begins or ends a session on a shared warehouse, shop, vehicle, counter, or kiosk device. |
| FC-WF-015 | Register notification and device capabilities | CURRENT · COMMON | Partial | User opens profile/settings, grants/revokes a browser permission, updates app/browser, or push token changes. |
| FC-WF-016 | Update app schema and remote configuration safely | COMMON · DEMOCRATIZE | Partial | A new app/service-worker/schema/feature version is deployed or an urgent kill switch/config change is needed. |
| FC-WF-017 | Handle emergency or degraded mobile operation | FOUNDATION | Partial | Health/capability checks or an attempted action detects a material outage or failure. |

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

## FC-WF-001 — Launch Field Companion and establish a secure session

| Field | Definition |
| --- | --- |
| Classification | CURRENT · FOUNDATION |
| Implementation state | Partial |
| Purpose | Enter the mobile shell with correct user, tenant, product permissions, policy state, and safe return/deep-link context. |
| Trigger | User opens the PWA/web app, follows a product deep link, or taps a notification. |

### Actors

- Field user
- NexArr
- Field Companion
- External MAM provider when configured

### State path

`opened → auth_required → policy_check → ready → degraded → denied → revoked → signed_out`

### Required sequence

1. Validate the deep link origin/intent without trusting tenant, record, or return URL parameters.
2. Authenticate through NexArr and require configured MFA/passkey/session policy.
3. Resolve active tenant and StaffArr-backed product action permissions; product availability is fixed-suite access, not a commercial access gate.
4. Check minimum app version, supported browser/capabilities, MAM/conditional-access state, and remote revocation/wipe commands.
5. Open or initialize an encrypted tenant/user local workspace with schema/version/TTL checks.
6. Fetch inbox/surface manifest and notification state using scoped APIs.
7. Route to the permitted target or explain denial/degraded capability with a safe fallback.
8. Record session/app/policy context without collecting unrelated personal device data.

### Exception and recovery paths

- Expired/replayed deep link, wrong tenant, inactive membership, MFA/policy failure, unsupported version, local workspace belongs to another user, device storage unavailable, or offline with no valid cached session.
- A shared device must clear the prior user workspace before new data is shown.

### Cross-product and external handoffs

- Field Companion ↔ NexArr/StaffArr.
- Field Companion ↔ external MAM/MDM policy provider.
- Field Companion → product APIs.

### Evidence and audit record

- Session/tenant/app version.
- Policy/capability decision.
- Deep-link resolution.
- Revocation/logout/cleanup.

### Field Companion / offline behavior

A previously validated offline session may open only explicitly cached low-risk work until its TTL; privileged or hard-gated actions require online revalidation.

### Measures and acceptance signals

- Launch success/time.
- Policy denials.
- Wrong-context prevention.
- Unsupported-device rate.
- Session cleanup success.

## FC-WF-002 — Discover and accept mobile work

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Give a field user one prioritized, permission-aware view of actionable work without changing product ownership. |
| Trigger | User opens My Work/Inbox, receives a notification, scans context, or work is assigned/available. |

### Actors

- Field user
- Field Companion
- Owning products

### State path

`available → assigned → claimed → in_progress → blocked → completed → cancelled → expired`

### Required sequence

1. Request a normalized mobile work envelope from permitted products: owner product, action key, subject, title, priority, due window, location, status, capability requirements, offline policy, and deep link.
2. Filter server-side by tenant/user/role/scope and remove expired or unauthorized work.
3. Group assigned, available/claimable, approvals, escalations, mentions, drafts, and blocked items with product identity.
4. Show prerequisites, expected duration, required device capabilities/evidence, and online/offline availability.
5. Allow claim/accept only through the owning product with idempotency and concurrency validation.
6. Download permitted work package when the user explicitly makes it available offline.
7. Open the owning product micro-surface and retain return context.
8. Refresh from product events and display stale/changed state clearly.

### Exception and recovery paths

- Assignment revoked, another worker claimed work, prerequisite failed, shift/site scope changed, task expired, offline package unavailable, or product is degraded.
- Field Companion does not invent a universal task status; it maps product states into a presentation envelope.

### Cross-product and external handoffs

- Products → Field Companion: mobile work projection.
- Field Companion → product: claim/open/action.
- NexArr: notification/deep link.

### Evidence and audit record

- Work envelope/version.
- Claim outcome.
- Offline package/TTL.
- Open/complete/cancel outcome.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Work-open latency.
- Claim conflicts.
- Overdue work.
- Offline package success.
- Cross-product completion.

## FC-WF-003 — Scan a code and resolve context

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Use a barcode/QR or typed identifier to find the correct permitted record/action quickly and safely. |
| Trigger | User opens Scan or a workflow requests a scan step. |

### Actors

- Field user
- Field Companion
- Owning products

### State path

`ready → scanning → decoded → resolving → matched → ambiguous → not_found → confirmed → cancelled`

### Required sequence

1. Check camera permission/capability and provide hardware-scanner/typed/manual selection fallback.
2. Display target instructions, accepted symbologies/identifiers, and flashlight/zoom/accessibility controls.
3. Capture/decode locally where practical, normalize without exposing the raw value to unrelated products, and prevent rapid duplicate submissions.
4. Send the code plus declared purpose/expected record types to a scoped resolver API.
5. Resolve tenant-permitted candidates and show human-readable identity, owner product, status, location, and allowed actions—never only internal IDs.
6. Require user confirmation when ambiguous or when the scanned item differs from expected context.
7. Open the smallest owning-product action surface or add the verified item to the current workflow.
8. Record scan outcome and discard raw camera frames unless explicitly captured as evidence.

### Exception and recovery paths

- Unreadable/unsupported code, multiple matches, wrong tenant/site, retired/merged record, code copied/spoofed, offline resolver unavailable, or camera denied.
- A scan is context evidence, not sole authorization or proof of physical possession.

### Cross-product and external handoffs

- Field Companion ↔ product resolver APIs/RecordArr where document QR is used.
- NexArr/StaffArr: permission scope.

### Evidence and audit record

- Purpose/expected type.
- Decoded normalized value hash where appropriate.
- Resolution candidates/decision.
- Opened action/outcome.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to match.
- First-scan success.
- Ambiguity/wrong-item rate.
- Manual fallback.
- Unauthorized-match suppression.

## FC-WF-004 — Capture and submit field evidence

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Collect high-quality, attributable evidence and attach it to the correct owning record without duplicate document silos. |
| Trigger | A product workflow requires or permits photo, video, audio, file, signature, note, measurement, or document evidence. |

### Actors

- Field user
- Field Companion
- Owning product
- RecordArr

### State path

`required → capturing → review → queued → uploading → processing → accepted → rejected → cancelled`

### Required sequence

1. Receive capture requirements from the owning product: type, count, purpose, subject, sensitivity, quality, retention, metadata, and whether offline is allowed.
2. Explain permissions and privacy, then invoke camera/file/audio/signature/location only as needed.
3. Capture media/data and run client checks for blur, cutoff, orientation, duplicates, size, duration, required fields, and available storage.
4. Let the user review, annotate/redact where policy permits, retake, describe, and confirm attribution/attestation.
5. Encrypt local payload and create an idempotent queued submission with product action, subject, evidence metadata, hash, and schema version.
6. Upload/resume to RecordArr or approved intake endpoint; malware/format/content checks occur server-side.
7. Owning product validates evidence acceptance and commits only the reference and workflow outcome.
8. Show final source record/link or specific rejection while retaining a safe retry/correction path.

### Exception and recovery paths

- Permission denied, storage full, capture too poor, unsupported/malicious file, upload interrupted, subject changed, duplicate evidence, signature declined, location unavailable, or product rejects after upload.
- Sensitive captures do not remain in the general device photo library unless explicitly required and disclosed.

### Cross-product and external handoffs

- Field Companion → RecordArr: file/evidence.
- Field Companion → owning product: evidence reference/action.
- Compliance Core: evidence mapping where relevant.

### Evidence and audit record

- Requirements/version.
- Capture metadata/hash/consent.
- Queue/upload processing.
- Product acceptance/rejection.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Retake/rejection rate.
- Upload success/time.
- Duplicate reduction.
- Storage/network cost.
- Evidence acceptance.

## FC-WF-005 — Execute an offline-safe action and synchronize

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Partial |
| Purpose | Let field work continue during poor connectivity while preserving authorization, validation, idempotency, and user understanding. |
| Trigger | User downloads eligible work or connectivity drops during an explicitly offline-capable task. |

### Actors

- Field user
- Field Companion sync engine
- NexArr
- Owning product

### State path

`downloaded → offline_active → draft → queued → syncing → accepted → rejected → conflict → expired → cancelled`

### Required sequence

1. Before going offline, obtain a signed/scoped work package with subject snapshot, action schema, allowed operations, local validation, TTL, maximum evidence size, conflict policy, and gate requirements.
2. Encrypt and store only necessary tenant/user/task data with visible size/expiry and wipe rules.
3. Record each local operation as an immutable intent with sequence, client time, app/schema version, idempotency key, dependencies, and evidence hashes.
4. Validate locally for completeness but label server-dependent checks and never claim final completion prematurely.
5. Display queued/pending/blocked/expired state and allow safe edit/cancel before submission.
6. On reconnect, reauthenticate, refresh policy/revocation, submit intents in dependency order, and resume evidence uploads.
7. Owning product revalidates permissions, record version, business rules, Compliance Core gates, and commit-time facts, then returns accepted/rejected/conflict outcomes.
8. Update local state, preserve rejection/conflict evidence, and reconcile product events before declaring synced.

### Exception and recovery paths

- Session revoked, package expired, record closed/changed, permission removed, duplicate accepted elsewhere, hard gate unavailable, schema migrated, evidence missing, queue dependency fails, or device clock is wrong.
- Fail-open/fail-closed behavior is action-specific and server-defined.

### Cross-product and external handoffs

- Field Companion ↔ NexArr offline-action service.
- Field Companion ↔ owning products/Compliance Core/RecordArr.
- Products → Field Companion: reconciliation events.

### Evidence and audit record

- Package/policy/TTL.
- Local intents/sequence/hash.
- Sync attempts/server validation.
- Commit/reject/conflict/reconciliation.

### Field Companion / offline behavior

This is the canonical offline workflow. Only server-declared operations may queue; hard safety, identity, financial, release, or compliance gates are revalidated online.

### Measures and acceptance signals

- Offline completion rate.
- Sync success/latency.
- Conflict/expiry rate.
- Duplicate prevention.
- Data loss incidents.

## FC-WF-006 — Resolve a mobile synchronization conflict

| Field | Definition |
| --- | --- |
| Classification | UNDERSERVED · FOUNDATION |
| Implementation state | Partial |
| Purpose | Turn a technical version conflict into an understandable, auditable user or supervisor decision. |
| Trigger | The owning product rejects an offline/queued action because the source record or authorization changed. |

### Actors

- Field user
- Supervisor/record owner
- Field Companion
- Owning product

### State path

`conflict → reviewing → auto_resolvable → user_action → escalated → resubmitted → resolved → discarded`

### Required sequence

1. Preserve the original local intent, source snapshot version, evidence, timestamps, and server rejection reason.
2. Fetch the latest permitted server record and product-provided conflict schema/allowed resolutions.
3. Explain the real-world difference in plain language and highlight values/status/actions changed since the offline snapshot.
4. Classify auto-resolvable nonoverlapping changes, revalidation-needed actions, supervisor-review cases, and permanently invalid actions.
5. Offer only product-defined choices: reapply to current version, edit/resubmit, keep server state, attach evidence as note, escalate, or cancel.
6. For merge/reapply, create a new intent with lineage to the rejected attempt and rerun validation/gates.
7. Record the resolution actor/reason and preserve both versions where audit requires.
8. Clear local conflict data only after reconciliation or explicit discard confirmation.

### Exception and recovery paths

- Server record deleted/merged, sensitive fields no longer visible, multiple queued intents depend on the conflict, product has no safe merge, or user lacks resolution authority.
- Field Companion never performs a generic last-write-wins merge.

### Cross-product and external handoffs

- Field Companion ↔ owning product/NexArr/StaffArr.
- RecordArr: retained evidence if action cannot be applied.

### Evidence and audit record

- Rejected intent/snapshot.
- Server delta/reason.
- Options/decision.
- New intent or discard/reconciliation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to resolve.
- Auto-resolution rate.
- Escalation rate.
- Repeated conflicts.
- Lost-work incidents.

## FC-WF-007 — Open a push notification and complete its action

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Deliver actionable, scoped, nonduplicative mobile notifications without leaking sensitive content or trusting client routing. |
| Trigger | NexArr/product notification service sends an event to a registered push subscription. |

### Actors

- User
- NexArr notification service
- Field Companion
- Owning product

### State path

`created → queued → sent → delivered → opened → actioned → dismissed → expired → failed`

### Required sequence

1. Owning product creates a notification with tenant/user/audience, template, sensitivity, action/deep-link token, urgency, expiry, dedupe/correlation, and preference policy.
2. NexArr resolves active subscriptions, quiet hours, channels, escalation, and sensitive-preview rules.
3. Push payload contains minimum safe preview and an opaque signed reference rather than trusted tenant/record data.
4. Field Companion displays the notification according to OS/browser permission and app policy.
5. On tap, establish/refresh session and resolve the signed action server-side.
6. Recheck tenant, permission, record state, expiry, and MAM policy before showing details.
7. Open the owning-product micro-surface and record delivered/opened/actioned/dismissed outcomes.
8. Expire/revoke stale subscriptions and suppress duplicate or already-completed notifications.

### Exception and recovery paths

- Push permission denied, token expired, wrong user now on shared device, notification expired, record already resolved, deep link replayed, offline with uncached target, or OS suppresses delivery.
- Sensitive details remain hidden on lock screen unless policy/user explicitly permits.

### Cross-product and external handoffs

- Product → NexArr notification service → Field Companion.
- Field Companion → product deep link/action.

### Evidence and audit record

- Template/audience/policy.
- Subscription/send outcome.
- Resolution/security checks.
- Open/action/dismiss.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Delivery/open/action rate.
- Duplicate suppression.
- Stale token rate.
- Deep-link denial.
- Time to action.

## FC-WF-008 — Clock in/out or record labor from mobile

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Capture attributable time events and labor context while StaffArr/LedgArr and owning products retain time/payroll/work ownership. |
| Trigger | User starts/ends shift, break, travel, task labor, or corrects a missed event through the Clock surface. |

### Actors

- Worker
- Supervisor/time approver
- Field Companion
- StaffArr
- Owning product
- LedgArr

### State path

`ready → active → break → submitted → pending_approval → approved → adjusted → rejected → exported`

### Required sequence

1. Load permitted time actions, employment/assignment context, policy, active clock state, work-order/trip/task references, offline policy, and required reason/location rules.
2. Show current tenant/user/timezone and active timer clearly before action.
3. Capture event type, server/client time, optional product work reference, cost center/project, declared location evidence when justified, and user attestation.
4. Submit idempotently to the authoritative timekeeping workflow; do not calculate final payroll in Field Companion.
5. Handle overlapping clocks, missed breaks, prior open events, shift boundary, timezone/DST, and policy warnings.
6. Allow correction request with reason and original event lineage rather than silent edits.
7. Route supervisor approval/exception and send approved labor allocations to owning products/LedgArr as configured.
8. Display authoritative status: recorded, pending approval, adjusted, rejected, or exported.

### Exception and recovery paths

- Offline clock event exceeds TTL, device clock wrong, user already clocked elsewhere, assignment invalid, location unavailable, meal/break rule issue, or payroll period locked.
- Geolocation is one policy input, not conclusive proof of work.

### Cross-product and external handoffs

- Field Companion ↔ StaffArr time workflow.
- StaffArr → owning product/LedgArr.
- Compliance Core: labor-rule guidance/gates where configured.

### Evidence and audit record

- Event/context/attestation.
- Server time/version.
- Exception/correction/approval.
- Allocation/export.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Punch success.
- Missed/duplicate events.
- Correction rate.
- Approval time.
- Offline clock reconciliation.

## FC-WF-009 — Report an issue, incident, defect, or observation

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Partial |
| Purpose | Let a field user quickly capture a concern and route it to the correct owning workflow without knowing the suite architecture. |
| Trigger | User taps Report, scans context, or an active task offers a report action. |

### Actors

- Reporter
- Field Companion
- Routing service
- StaffArr/MaintainArr/AssurArr/RoutArr/LoadArr/other owner

### State path

`draft → queued → submitted → routed → acknowledged → triage → closed_elsewhere`

### Required sequence

1. Choose or infer report context from current task/scan/location while showing the user the proposed destination and purpose.
2. Collect a concise description, category/severity/safety urgency, affected person/asset/location/material/order/shipment references, and optional media/witness/contact preference.
3. For emergencies, display tenant-defined immediate actions/contact instructions before normal submission; do not represent the app as emergency dispatch.
4. Allow anonymous/confidential reporting only where tenant policy and law permit, with clear limits.
5. Save draft/queue offline when the selected report type is safe and configured for it.
6. Submit to a routing contract that selects the owning product and creates the authoritative incident/defect/nonconformance/claim/task record.
7. Return the human-readable reference, ownership, expected next step, and permitted follow-up visibility.
8. Prevent duplicates through correlation/context suggestions without discouraging reporting.

### Exception and recovery paths

- Wrong category/owner, immediate danger, reporter loses connection, evidence contains sensitive data, duplicate report, subject cannot be identified, or anonymous follow-up is impossible.
- Field Companion does not close or investigate the resulting source record.

### Cross-product and external handoffs

- Field Companion → owning product/StaffArr incident hub.
- RecordArr: evidence.
- Compliance Core/ReportArr: applicable evaluation/metrics.

### Evidence and audit record

- Reporter/context/privacy choice.
- Description/category/evidence.
- Routing/source record.
- Acknowledgement/follow-up.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to submit/acknowledge.
- Routing accuracy.
- Duplicate rate.
- Offline success.
- Reporter follow-up visibility.

## FC-WF-010 — Complete a mobile form, checklist, or attestation

| Field | Definition |
| --- | --- |
| Classification | COMMON · FOUNDATION |
| Implementation state | Partial |
| Purpose | Render any product-owned structured workflow as a safe, resumable, accessible mobile micro-surface. |
| Trigger | Owning product assigns a form/checklist/inspection/assessment/attestation step. |

### Actors

- Field user
- Owning product
- Field Companion
- Compliance Core where gated

### State path

`assigned → draft → blocked → ready → queued → submitted → accepted → rejected → superseded`

### Required sequence

1. Fetch a signed/versioned form schema, response context, validation, conditional logic, evidence requirements, offline policy, and action permissions from the owning product.
2. Render field-friendly sections with progress, plain-language guidance, hints toggle, units, accessibility, and minimal on-screen density.
3. Save encrypted local drafts with schema/version and show unsynced state.
4. Apply local validation/branching while distinguishing server-derived references, uniqueness, permission, and compliance checks.
5. Capture required scans/evidence/signature and let the user review the complete attestation before submission.
6. Submit an idempotent response to the owning product, which reruns authoritative validation and gates.
7. Handle field-level rejection/conflict without erasing accepted responses or evidence.
8. Show authoritative completion, next task, and immutable response/evidence link.

### Exception and recovery paths

- Schema updated mid-draft, option/reference retired, user loses permission, conditional branch changes, required evidence fails, signature revoked, offline package expires, or form is recalled.
- Field Companion does not persist the business response as the system of record.

### Cross-product and external handoffs

- Owning product ↔ Field Companion.
- Field Companion → RecordArr for evidence.
- Compliance Core: dynamic requirements/gates.

### Evidence and audit record

- Schema/version/context.
- Draft changes.
- Evidence/signature.
- Server validation/outcome.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Completion time.
- Drop-off.
- Validation/rejection rate.
- Offline sync.
- Accessibility issues.

## FC-WF-011 — Collect a signature or acknowledgement

| Field | Definition |
| --- | --- |
| Classification | COMMON · UNDERSERVED |
| Implementation state | Partial |
| Purpose | Capture a deliberate, reviewable acknowledgement with appropriate identity, document/content version, intent, and legal context. |
| Trigger | A product workflow requests a receipt, inspection, delivery, training, policy, quality, customer, supplier, maintenance, or other acknowledgement/signature. |

### Actors

- Signer
- Witness/approver where required
- Field Companion
- Owning product
- RecordArr

### State path

`presented → review → authenticated → signed → refused → submitted → accepted → invalidated`

### Required sequence

1. Load the exact content/document/transaction version, signer role, purpose, disclosure, required authentication, witness/order, and refusal path.
2. Make the content reviewable and accessible before signing; record scroll/read prompts only as UX, not proof of comprehension.
3. Reauthenticate or apply step-up verification when risk/policy requires.
4. Capture explicit intent, signer identity/role, timestamp, signature method, optional drawn mark, and permitted device/session context.
5. Hash/bind signature evidence to the exact content/version and submit to the owning product/RecordArr.
6. Allow refusal with reason and route the operational exception instead of coercing completion.
7. Return signed copy/receipt and preserve verification information.
8. Invalidate or obtain a new signature when content materially changes; never move an old signature to a new version.

### Exception and recovery paths

- Signer identity unclear, content changed while open, offline signature not permitted/expired, witness sequence invalid, user requests accommodation, device shared, or legal requirements exceed available method.
- A drawn mark alone is not the complete signature evidence.

### Cross-product and external handoffs

- Field Companion ↔ NexArr/owning product/RecordArr.
- Compliance Core: signature/attestation requirement.

### Evidence and audit record

- Content/version/hash.
- Disclosure/authentication/intent.
- Signature/session/time.
- Receipt/refusal/invalidation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Completion/refusal rate.
- Step-up success.
- Invalidation rate.
- Disputes.
- Accessible review success.

## FC-WF-012 — Use a one-time external capture link

| Field | Definition |
| --- | --- |
| Classification | UNDERSERVED |
| Implementation state | Target |
| Purpose | Collect a narrowly scoped response or evidence from an external person without granting a full tenant account. |
| Trigger | An authorized product user requests a customer, supplier, applicant, witness, consignee, auditor, or other external party action. |

### Actors

- External participant
- Requesting product user
- NexArr
- Field Companion public capture surface
- RecordArr

### State path

`issued → delivered → opened → verified → submitted → completed → expired → revoked → failed`

### Required sequence

1. Owning product defines exact action, subject, permitted fields/evidence, recipient/verification method, expiration, use count, locale, confidentiality, and return behavior.
2. NexArr issues an opaque, signed, one-time/scoped token; URLs contain no trusted tenant/record data.
3. External user opens a minimal branded surface, reviews purpose/privacy/contact and verifies identity as configured.
4. Capture only the requested response/file/photo/signature/acknowledgement with quality and accessibility checks.
5. Submit idempotently; server validates token scope, expiry, action status, malware/file policy, and duplicate/replay.
6. Store evidence in RecordArr and let the owning product commit the response/status.
7. Show receipt without exposing internal records and notify the requester.
8. Revoke/expire the link and audit attempts, delivery, verification, and completion.

### Exception and recovery paths

- Link forwarded, recipient identity mismatch, expired/replayed token, request already completed/cancelled, upload unsafe, external user needs accommodation, or sensitive data exceeds allowed channel.
- Public capture does not create broad portal or tenant access.

### Cross-product and external handoffs

- Owning product ↔ NexArr/Field Companion public surface/RecordArr.
- Notification provider.

### Evidence and audit record

- Request/scope/recipient.
- Token issue/delivery/verification.
- Submission/evidence.
- Commit/receipt/expiry.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Completion time/rate.
- Verification failures.
- Expired/reissued links.
- Support requests.
- Replay prevention.

## FC-WF-013 — Apply app-protection policy and selective wipe

| Field | Definition |
| --- | --- |
| Classification | DEMOCRATIZE · FOUNDATION |
| Implementation state | Target |
| Purpose | Protect STL business data on BYOD or managed devices while avoiding unnecessary control of personal data. |
| Trigger | User signs in, policy changes, risk signal arrives, membership ends, device is lost, or administrator issues a wipe/revoke command. |

### Actors

- User
- NexArr administrator
- External MAM/MDM provider
- Field Companion

### State path

`compliant → grace → remediation_required → restricted → revoked → wipe_pending → wiped → verification_failed`

### Required sequence

1. Resolve user/tenant/app identity and request policy/risk state from the configured provider or NexArr policy bridge.
2. Evaluate minimum OS/browser/app version, encryption/storage, device integrity, screen capture, copy/paste/open-in/save-as, approved apps/storage, network/VPN, PIN/biometric, and offline grace requirements.
3. Explain required remediation and which STL data/actions are unavailable; do not expose hidden provider risk detail unnecessarily.
4. Apply client controls where web/PWA platform permits and server controls for data minimization, export, token lifetime, download, and action gating.
5. On revoke/wipe, invalidate server sessions/tokens/keys/subscriptions first, then delete tenant/user cached data, work packages, queue payloads, thumbnails, and cryptographic material.
6. Preserve only server-side audit and already committed source records; report any unsynced work that cannot be recovered according to policy.
7. Verify cleanup and prevent old service-worker/cache versions from restoring data.
8. Allow personal apps/data to remain untouched and document limitations of browser-only enforcement.

### Exception and recovery paths

- Device offline beyond grace, browser prevents reliable selective wipe, queued evidence has no server copy, provider unavailable, shared device has multiple tenant workspaces, or threat signal is disputed.
- Some MAM controls require a native wrapper or external managed browser; the product must state those limits.

### Cross-product and external handoffs

- Field Companion ↔ NexArr/external MAM provider.
- NexArr → all product APIs: token/session revocation.
- RecordArr: already committed evidence only.

### Evidence and audit record

- Policy/version/provider state.
- Decision/remediation.
- Revoke/wipe command.
- Cleanup verification/unsynced impact.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Policy check latency.
- Remediation success.
- Wipe completion.
- Residual cache tests.
- Unsynced work loss.

## FC-WF-014 — Operate on a shared or kiosk device

| Field | Definition |
| --- | --- |
| Classification | COMMON · DEMOCRATIZE |
| Implementation state | Partial |
| Purpose | Allow multiple workers to use a managed/shared device without cross-user data leakage or attribution errors. |
| Trigger | A user begins or ends a session on a shared warehouse, shop, vehicle, counter, or kiosk device. |

### Actors

- Worker
- Supervisor/device custodian
- NexArr
- Field Companion
- External MDM provider when configured

### State path

`available → user_active → locked → handoff_required → cleanup → ready_next_user → quarantined`

### Required sequence

1. Display current tenant/user/session prominently and require sign-in or approved badge/QR plus secondary factor where policy requires.
2. Create a user-isolated encrypted workspace and fetch only role/site/shift/task-scoped data.
3. Use short inactivity lock and require reauthentication for consequential actions/signatures.
4. Prevent personal password manager/autofill, notification preview, downloads, clipboard, and file leakage where platform policy allows.
5. On user switch/sign-out, block until queued work is synced, transferred through an authorized handoff, explicitly discarded with warning, or preserved under encrypted user isolation.
6. Clear caches/media/tokens/keys/notifications and verify no prior user data remains visible.
7. Record custody/health issues and permit a supervisor to quarantine the device.
8. Support managed-home/kiosk integration without requiring it for ordinary BYOD users.

### Exception and recovery paths

- Prior user left unsynced work, user cannot authenticate, badge shared, cleanup fails, device storage full, OS account leaks files, or kiosk loses network.
- No automatic reassignment of prior user work occurs during switch.

### Cross-product and external handoffs

- Field Companion ↔ NexArr/products/external MDM.
- StaffArr: user/shift/site scope.

### Evidence and audit record

- Device/session/user.
- Queued-work decision.
- Cleanup verification.
- Quarantine/custody.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Switch time.
- Cleanup failures.
- Cross-user leakage tests.
- Abandoned queue rate.
- Authentication incidents.

## FC-WF-015 — Register notification and device capabilities

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Keep push subscriptions, permissions, capabilities, app version, and privacy choices accurate without creating invasive device inventory. |
| Trigger | User opens profile/settings, grants/revokes a browser permission, updates app/browser, or push token changes. |

### Actors

- User
- Field Companion
- NexArr notification service

### State path

`detected → permission_needed → registered → healthy → degraded → revoked → stale`

### Required sequence

1. Detect browser/app version, install mode, supported camera/scan/file/audio/location/push/storage/background capabilities, and current permission state.
2. Explain each optional permission and request it only when needed or user explicitly enables it.
3. Register/rotate push subscription with tenant/user/app context, public key/token, expiration, and device alias chosen by the user.
4. Store user notification preferences, quiet hours, sensitive preview, channels, and product/category controls through NexArr.
5. Report coarse capability/health needed for workflow eligibility without collecting unrelated personal identifiers.
6. Update on permission/token/version change and remove stale subscriptions on logout/wipe/inactivity.
7. Offer a test notification and capability diagnostics with remediation steps.
8. Audit preference and subscription changes.

### Exception and recovery paths

- Push unsupported/blocked, browser clears storage, duplicate subscription, permission permanently denied, shared device ambiguity, token belongs to old user, or app version incompatible.
- Capability data is not used as covert employee performance tracking.

### Cross-product and external handoffs

- Field Companion ↔ NexArr notification/settings service.
- Products consume capability eligibility, not raw personal device details.

### Evidence and audit record

- Capabilities/app version.
- Permission decisions.
- Subscription/preferences.
- Rotation/removal/test.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Registration success.
- Stale subscription rate.
- Permission opt-in/denial.
- Notification test success.
- Capability-related task failures.

## FC-WF-016 — Update app schema and remote configuration safely

| Field | Definition |
| --- | --- |
| Classification | COMMON · DEMOCRATIZE |
| Implementation state | Partial |
| Purpose | Roll out Field Companion changes without corrupting offline work, breaking forms, or stranding old clients. |
| Trigger | A new app/service-worker/schema/feature version is deployed or an urgent kill switch/config change is needed. |

### Actors

- Release operator
- Product owners
- Field Companion
- NexArr

### State path

`planned → testing → staged → rolling_out → paused → rolled_back → required → retired`

### Required sequence

1. Define compatibility matrix across app, service worker, local schema, offline work packages, product API/schema, and minimum supported version.
2. Run migration, downgrade/rollback, queued-intent replay, storage-limit, offline, shared-device, accessibility, and MAM-policy tests.
3. Stage release by tenant/user/device class or percentage using server-side flags that do not bypass authorization.
4. Notify active users before a disruptive update and show which drafts/queues must sync or can migrate.
5. Install/migrate atomically where platform permits and retain recovery copy until verification.
6. Use kill switch to disable only affected action/capture/offline capability while preserving safe access/status/exports.
7. Monitor launch, crash, sync, migration, API errors, storage, and task completion; pause/rollback on thresholds.
8. Retire incompatible clients with a clear update path and preserve/recover unsynced work where technically possible.

### Exception and recovery paths

- Service worker serves mixed versions, local migration fails, old queue schema cannot replay, app store/managed browser delays update, offline user misses deadline, or rollback cannot read new local state.
- Critical security revocation may intentionally sacrifice uncommitted local work after documented risk decision.

### Cross-product and external handoffs

- Field Companion ↔ NexArr/config service/all product APIs.
- External MAM/managed-browser provider where applicable.

### Evidence and audit record

- Compatibility/test evidence.
- Flag/release cohort.
- Migration/telemetry.
- Pause/rollback/retirement.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Crash-free/launch success.
- Migration failures.
- Sync regression.
- Rollback rate.
- Users stranded on old version.

## FC-WF-017 — Handle emergency or degraded mobile operation

| Field | Definition |
| --- | --- |
| Classification | FOUNDATION |
| Implementation state | Target |
| Purpose | Provide safe, explicit fallback when the app, network, product, identity, capture capability, or device is unavailable. |
| Trigger | Health/capability checks or an attempted action detects a material outage or failure. |

### Actors

- Field user
- Supervisor
- Platform/product operator
- Field Companion

### State path

`detected → contained → offline_or_fallback → repair → reconnect → reconcile → restored → follow_up`

### Required sequence

1. Identify affected function, tenant/product, connectivity, device capability, session/policy, and whether cached work remains valid.
2. Preserve drafts/queue and tell the user what is saved, what is not, what can continue offline, and what requires online/server confirmation.
3. For safety/emergency contexts, show tenant-approved human contact or local procedure; do not imply the web app is emergency service.
4. Use product-defined degraded policy: read-only cached instructions, manual reference number, paper fallback, alternate station/device, or block.
5. Generate correlation/diagnostic package without secrets or unrelated device data.
6. Notify operators and suppress repeated user submissions/duplicate incidents.
7. After recovery, reauthenticate, replay idempotently, reconcile authoritative records, and route conflicts.
8. Communicate rejected/accepted actions and retain incident evidence.

### Exception and recovery paths

- Identity outage, Compliance Core hard gate unavailable, local data expired, device lost/damaged, no alternate procedure, manual action occurred outside system, or recovery produces duplicate/conflicting records.
- The UI never labels queued/manual work as committed until the owning product confirms it.

### Cross-product and external handoffs

- Field Companion ↔ NexArr/all products/RecordArr/ReportArr.
- Tenant emergency/support channels.

### Evidence and audit record

- Failure/scope/correlation.
- Saved/unsaved state.
- Fallback/manual reference.
- Recovery/reconciliation/outcome.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Detection/recovery.
- Draft loss.
- Duplicate attempts.
- Fallback usage.
- Reconciliation exceptions.



## Workflow definition of done

A workflow is releasable only when its happy path, every listed exception, dependency outage, duplicate/retry, stale/concurrent update, permission denial, cancellation, correction/void, archival/retention, import/API/event path, notification, audit trail, reporting projection, mobile behavior, and professional printable output have automated and human-usable acceptance coverage.
