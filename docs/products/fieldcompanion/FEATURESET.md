# Field Companion — MAM Full Feature Set

[Workflow catalog](./WORKFLOWS.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md)

## Document control

| Field | Value |
| --- | --- |
| Product | Field Companion (MAM) |
| Category | Mobile Application and Action Management |
| Repository maturity | Scaffold |
| Audit basis | Static repository inspection on 2026-06-24 |
| Cataloged capabilities | 71 |
| Cataloged workflows | 17 |

## Product charter

Provide a secure, unified, mobile-first execution surface for work owned by the STL products: discover assignments, scan context, capture evidence, complete permitted actions, communicate, and synchronize safely under poor connectivity. Field Companion is an application/action layer, not a new source of operational truth and not a full device-management replacement.

> **Implementation reality — Scaffold:** The repository contains a substantial mobile/PWA frontend shell, service worker, scan/report/clock/profile/notification/offline-queue surfaces, and shared API integration, but no dedicated durable Field Companion backend domain. Durable mobile action state currently belongs in NexArr and the owning products.

## Source-of-truth boundary

### Field Companion owns

- Mobile/PWA shell, navigation, product surface discovery, local presentation state, accessibility, device capability adaptation, and user mobile preferences.
- Offline action queue client behavior, encrypted/local cache policy, synchronization UX, conflict presentation, retry/cancel controls, and storage/network visibility.
- Capture orchestration for camera, file, barcode/QR, audio, signature, location (when justified), and device metadata before submission to an owning product/RecordArr.
- Push notification registration UX, deep-link routing, notification display controls, and mobile task/inbox aggregation.
- App-protection integration points and policy-aware client behavior for MAM/BYOD, shared device, conditional launch, and selective wipe scenarios.

### Field Companion does not own

- People, assets, work orders, training, transport, inventory, suppliers, customers, orders, finance, quality, documents, analytics, or compliance decisions.
- A parallel mobile database that becomes authoritative while disconnected; owning products validate and commit every domain action.
- Tenant role definitions or domain authorization.
- Full mobile device management such as hardware enrollment, OS patch enforcement, carrier management, or organization-wide device inventory unless supplied by an external MDM provider.
- Unbounded employee tracking, covert location collection, or access to unrelated personal device data.

## Suite access and authorization

- All normal tenant users may launch every product application in the suite; product actions and records remain permission-scoped server-side.
- Fixed-suite access is nonvariable. Deployment health, feature readiness, legal restrictions, external-provider setup, and permissions may still constrain a capability.
- Platform administration remains a NexArr-only responsibility and is not a tenant product surface.
- Compliance Core administrative/authoring UI is restricted, while Compliance Core runtime evaluations, fact requirements, evidence needs, and workflow gates serve every tenant and product.

## Primary users

- Field worker/technician
- Driver/operator
- Warehouse worker
- Supervisor/approver
- Customer/supplier/external participant
- Mobile application administrator
- Security/MAM administrator
- Product workflow owner
- Support/operator

## Required integrations

- NexArr
- StaffArr
- All operational products
- RecordArr
- Compliance Core
- ReportArr
- Push notification services
- External MAM/MDM/managed-browser/threat-defense providers
- Device camera/scanner/file/location/browser capabilities

## Product principles

- Field Companion is a mobile execution shell; every business record remains owned, validated, and committed by the corresponding product.
- Offline state is explicit and bounded. Queued, uploaded, accepted, and committed are never presented as the same status.
- BYOD protection minimizes and selectively removes STL business data without claiming control over unrelated personal device content.
- Mobile layouts expose the smallest safe action, not compressed desktop walls of forms or tables.
- Location, device, and telemetry collection must be purpose-limited, visible, proportionate, and never treated as infallible proof.

## Repository implementation snapshot

| Static indicator | Count |
| --- | --- |
| Persistent DbSet declarations | 0 |
| Discovered server classes | 0 |
| Discovered HTTP route declarations | 0 |
| Frontend source files | 74 |
| Frontend page files | 9 |
| Documentation headings | 107 |

### Evidence used for the current-state classification

- FieldCompanion.Web includes a PWA/service-worker shell and dedicated Home, Clock, Launch, Notifications, Offline Queue, Profile, Report, Scan, and Surfaces pages.
- The frontend includes API/session/storage tests, shared app-shell integration, notification handling, capture labels, and local/offline UI concepts.
- No Field Companion DbContext, persistent entities, or dedicated backend endpoints were found in the static inventory; durable submissions must flow to NexArr/shared services and owning product APIs.
- NexArr includes durable offline-action, mobile-submission, push-subscription, notification-setting, launch, and service-context models that can support the cross-product mobile plane.

> Counts are static-discovery indicators, not proof that every route or screen is complete, reachable, secure, migrated, or production-ready.

## Mandatory migration or refactor work

- Do not create a competing mobile system of record. Persist cross-product offline intents/submissions in NexArr or a narrowly scoped mobile plane and commit domain records only through owning product APIs.

## Feature catalog

### A. Currently implemented or meaningfully represented

These capabilities have repository evidence. Their state follows the product-level maturity and may still require hardening or completion.

| Feature ID | Capability | Class | State | Required behavior / evidence |
| --- | --- | --- | --- | --- |
| FC-CUR-001 | Mobile/PWA application shell | CURRENT | Scaffold | Responsive web-app structure, service worker, launch/session handling, and unified suite navigation are present. |
| FC-CUR-002 | My work and inbox-oriented home | CURRENT | Scaffold | The frontend presents mobile-oriented work discovery and cross-product surface concepts. |
| FC-CUR-003 | Product surface launcher | CURRENT | Scaffold | Users can discover and open mobile-capable product experiences from a shared surface page. |
| FC-CUR-004 | Clock workflow surface | CURRENT | Scaffold | A dedicated mobile time-clock page exists for cross-product time/labor interaction. |
| FC-CUR-005 | Scan workflow surface | CURRENT | Scaffold | A dedicated scan page establishes barcode/QR-driven record/task lookup intent. |
| FC-CUR-006 | Capture and report workflow surface | CURRENT | Scaffold | Report/capture UI concepts allow field issue or evidence submission. |
| FC-CUR-007 | Offline queue visibility | CURRENT | Scaffold | A dedicated page exposes locally queued actions rather than hiding disconnected work. |
| FC-CUR-008 | Notifications and deep-link context | CURRENT | Scaffold | Notification settings/display surfaces support actionable mobile routing. |
| FC-CUR-009 | Profile, language, browser, and storage context | CURRENT | Scaffold | The mobile profile exposes environment/preferences and device/browser storage information. |
| FC-CUR-010 | Shared API/session client | CURRENT | Scaffold | The frontend integrates with the platform session and cross-product API patterns rather than owning identity. |

### B. Common category baseline

These are expected for a credible Mobile Application and Action Management product. Where they overlap current behavior, the target definition is the acceptance standard rather than a duplicate feature.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| FC-COM-001 | Secure mobile authentication and session | COMMON | Target | Use NexArr login/handoff, passkeys/MFA where configured, short-lived tokens, refresh controls, session visibility, logout/revocation, and safe deep links. |
| FC-COM-002 | Task and inbox aggregation | COMMON | Target | Show assigned/available/mentioned/approval/escalated work across products with due time, priority, location, status, offline capability, and owning-product identity. |
| FC-COM-003 | Context scanning | COMMON | Target | Support camera barcode/QR, hardware scanners where browser-compatible, typed lookup fallback, duplicate handling, symbology feedback, and permission-scoped results. |
| FC-COM-004 | Rich field capture | COMMON | Target | Photo, video, audio, file, annotation, OCR proposal, signature, measurement, and structured form capture with quality/size/privacy guidance. |
| FC-COM-005 | Offline-first execution | COMMON | Target | Download permitted work/context, queue explicit operations, expose sync state, preserve idempotency, handle expiration, and prevent unsafe commits. |
| FC-COM-006 | Conflict resolution | COMMON | Target | Explain server/local changes, show field-level differences and policy, offer permitted reapply/discard/merge paths, and preserve the original attempt. |
| FC-COM-007 | Push notifications | COMMON | Target | Register devices/browsers, route tenant/product context, honor quiet hours/preferences, dedupe, expire stale tokens, and deep-link safely. |
| FC-COM-008 | Location-aware work with privacy | COMMON | Target | Use coarse/precise location only for declared operational purposes, with visible permission, accuracy, retention, spoof/error handling, and non-location alternatives. |
| FC-COM-009 | Mobile forms and checklists | COMMON | Target | Render product-defined, versioned forms with conditional steps, required evidence, validation, drafts, resumability, and signature/attestation. |
| FC-COM-010 | Attachment optimization | COMMON | Target | Compress/transcode, generate thumbnails, preserve originals where required, resume uploads, enforce malware/type/size policy, and show network/storage cost. |
| FC-COM-011 | Accessibility and field ergonomics | COMMON | Target | Large targets, keyboard/switch/screen-reader support, contrast, sunlight mode, reduced motion, glove-friendly controls, audio prompts, and one-handed layouts. |
| FC-COM-012 | Multilingual and plain-language UI | COMMON | Target | Localized labels, dates/units, translated instructions/content where approved, and source-language retention for evidence. |
| FC-COM-013 | Shared/kiosk device mode | COMMON | Target | Fast secure user switch, badge/QR sign-in where permitted, no data leakage between sessions, short inactivity lock, and selective local cleanup. |
| FC-COM-014 | Device capability and health checks | COMMON | Target | Detect camera/storage/network/push/location/permissions/browser support and present actionable degraded alternatives. |
| FC-COM-015 | Remote configuration and release safety | COMMON | Target | Minimum supported version, staged flags, kill switches, schema compatibility, migration, rollback, and clear update-required behavior. |
| FC-COM-016 | Mobile audit and telemetry | COMMON | Target | Record actor/session/device-class/app version/network/action/sync/outcome without collecting unnecessary personal device data. |

### C. Commonly requested but widely underserved

These address recurring user friction, fragmented add-ons, poor transparency, inaccessible setup, or weak SMB usability.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| FC-UND-001 | Useful offline behavior that is honest | UNDERSERVED | Target | Show exactly what is available offline, what is queued, what can expire, what needs online revalidation, and what failed—rather than a generic offline banner. |
| FC-UND-002 | Cross-product field work without app hopping | UNDERSERVED | Target | One inbox, scan, capture, and session can hand off between products while every action still uses the owning product model and permission. |
| FC-UND-003 | BYOD protection without mandatory full-device enrollment | UNDERSERVED | Target | Protect STL business data and selectively wipe the app context without demanding control of the worker’s personal phone. |
| FC-UND-004 | Low-end device and poor-network support | UNDERSERVED | Target | Adaptive image quality, resumable uploads, compact payloads, storage budgets, background-safe retry, and readable fallback on inexpensive devices. |
| FC-UND-005 | Voice- and glove-friendly workflows | UNDERSERVED | Target | Read instructions, capture dictation, confirm critical values, pause/resume, and support large controls for mechanics, drivers, warehouse, and field personnel. |
| FC-UND-006 | Capture quality coaching | UNDERSERVED | Target | Detect blur, glare, cutoff, missing document corners, unreadable codes, wrong orientation, duplicate media, and insufficient evidence before upload. |
| FC-UND-007 | Privacy-safe location and time proof | UNDERSERVED | Target | Explain why/when location is requested, avoid continuous surveillance, separate accuracy from compliance, and support exceptions/manual review. |
| FC-UND-008 | No-login secure external capture | UNDERSERVED | Target | Use one-time, scoped, expiring links for a customer/supplier/witness/applicant to provide a specific document, signature, photo, response, or acknowledgement. |
| FC-UND-009 | Human-readable sync conflicts | UNDERSERVED | Target | Translate technical conflicts into “the work order closed while you were offline” and preserve a safe resubmission or supervisor-review path. |
| FC-UND-010 | User-controlled storage and network behavior | UNDERSERVED | Target | Display downloaded size, queued upload size, retention, Wi-Fi-only preference, clear-cache effects, and what will be lost before deletion. |
| FC-UND-011 | Micro-surfaces by role and context | UNDERSERVED | Target | Open directly to the smallest safe task—inspect, count, sign, scan, photograph, approve, acknowledge—without loading a desktop form. |
| FC-UND-012 | Accessible mobile evidence collection | UNDERSERVED | Target | Support captions/transcripts, alternative input, document review before signature, and assistance without invalidating attribution. |
| FC-UND-013 | Transparent shared-device safety | UNDERSERVED | Target | Show current signed-in worker, tenant, pending work, last sync, and cleanup state so users do not inherit another person’s tasks/data. |

### D. Advanced capabilities to democratize

These are commonly found only in enterprise tiers, specialist products, or expensive implementations. STL should make them practical without weakening governance.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| FC-DEM-001 | MAM app-protection policy integration | DEMOCRATIZE | Target | Support managed/open-in boundaries, copy/paste/save restrictions, encrypted app data, approved storage, conditional launch, and policy status on enrolled or unenrolled devices. |
| FC-DEM-002 | Selective wipe and session revocation | DEMOCRATIZE | Target | Remove STL cached data, tokens, keys, and queued content after user removal, device loss, policy failure, or remote command without erasing personal data. |
| FC-DEM-003 | Per-app VPN and certificate integration | DEMOCRATIZE | Target | Use external MAM/MDM providers for app-scoped network, client certificates, device/app attestation, and private APIs where required. |
| FC-DEM-004 | Mobile threat-defense integration | DEMOCRATIZE | Target | Consume risk signals for compromised device, malicious app/network, outdated OS/browser, or unsafe runtime and apply explainable conditional access. |
| FC-DEM-005 | Computer-vision-assisted capture | DEMOCRATIZE | Target | Propose crop, classification, field extraction, damage/defect detection, count, label/SDS matching, and quality checks with human confirmation. |
| FC-DEM-006 | Remote expert and guided assistance | DEMOCRATIZE | Target | Live video/audio/chat, annotations, pointer steps, screen/co-browse where safe, session consent, and automatic evidence packaging. |
| FC-DEM-007 | Digital credential wallet | DEMOCRATIZE | Target | Present/verify training, authorization, permit, inspection, identity, or customer credentials with expiry, selective disclosure, and offline-verifiable signatures where appropriate. |
| FC-DEM-008 | Advanced offline policy engine | DEMOCRATIZE | Target | Package data/actions by role/site/task, encrypt per tenant/session, enforce TTL/revocation, prioritize sync, and simulate migrations/conflicts before release. |
| FC-DEM-009 | Geofenced and proximity-aware actions | DEMOCRATIZE | Target | Use transparent, narrowly scoped geofence/Bluetooth/NFC proximity signals as one input—not sole proof—for arrival, asset/site context, or safety prompts. |
| FC-DEM-010 | Augmented-reality work guidance | DEMOCRATIZE | Target | Overlay approved steps, asset/component identification, measurements, hazards, and remote annotations while retaining a non-AR accessible workflow. |
| FC-DEM-011 | Enterprise shared-device orchestration | DEMOCRATIZE | Target | Integrate identity, kiosk/session pinning, shift handoff, badge login, managed home screen, app policy, and rapid secure cleanup without enterprise-only STL licensing. |
| FC-DEM-012 | Mobile observability and fleet compatibility matrix | DEMOCRATIZE | Target | Track app/browser/OS/device-class/capability performance, sync failures, crash-free sessions, storage pressure, and staged rollout health without invasive personal telemetry. |

### E. Suite-wide foundation required in Field Companion

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| FC-FND-001 | Tenant-scoped authorization | FOUNDATION | Target | Every read and write is tenant-scoped server-side; product UI visibility never substitutes for authorization. |
| FC-FND-002 | StaffArr-backed action permissions | FOUNDATION | Target | Use product action keys, role scopes, delegated authority, and explicit denial reasons; ownership does not prevent permissioned delegated workflows. |
| FC-FND-003 | Unified suite shell | FOUNDATION | Target | Consistent navigation, page anatomy, terminology, responsive behavior, keyboard access, and light/dark contrast across products. |
| FC-FND-004 | Record lifecycle and history | FOUNDATION | Target | Human-readable statuses, timeline events, actor/time/source attribution, undo or correction paths, and immutable audit evidence. |
| FC-FND-005 | Quick create for missing references | FOUNDATION · UNDERSERVED | Target | Create the minimum valid cross-product reference without abandoning the current task; preserve context and allow later backfill. |
| FC-FND-006 | Saved views and personal preferences | COMMON | Target | Per-user filters, columns, density, sort, board/list preference, default landing view, and product-scoped preferences. |
| FC-FND-007 | Bulk operations with preview | COMMON | Target | Selection-aware actions, dry-run/impact preview, partial failure handling, downloadable results, and permission checks per record. |
| FC-FND-008 | Import with mapping and review | COMMON · UNDERSERVED | Target | Product-specific import separate from Smart Import, with templates, mappings, validation, dedupe, dry run, commit plan, and rollback/correction evidence. |
| FC-FND-009 | Export and portability | COMMON · DEMOCRATIZE | Target | CSV/PDF/JSON or package export appropriate to the domain, without trapping customer data behind enterprise tiers. |
| FC-FND-010 | Notifications, tasks, and inbox | COMMON | Target | Actionable notifications with ownership, due dates, escalation, snooze/delegation, deep links, and duplicate suppression. |
| FC-FND-011 | API, webhooks, and event outbox | COMMON · DEMOCRATIZE | Target | Stable versioned APIs, idempotency, service authentication, signed webhooks/events, retries, dead-letter handling, and correlation IDs. |
| FC-FND-012 | RecordArr evidence references | FOUNDATION | Target | Files and evidence are stored once in RecordArr and referenced by owning records; products do not create shadow document vaults. |
| FC-FND-013 | ReportArr projections and metrics | FOUNDATION | Target | Operational products emit facts/events; ReportArr builds read models and reports without becoming the source of operational truth. |
| FC-FND-014 | Compliance Core applicability and gates | FOUNDATION | Target | Products request explainable requirements, missing facts, evidence needs, and gate decisions rather than embedding independent legal logic. |
| FC-FND-015 | Field Companion execution surface | FOUNDATION · UNDERSERVED | Target | Mobile-first task, scan, capture, acknowledgement, and offline execution against the owning product API. |
| FC-FND-016 | Professional print and report view | COMMON | Target | Print output is a purpose-built report document without the app shell, not a screenshot of the current page. |
| FC-FND-017 | Accessible error and degraded states | FOUNDATION | Target | Explain what failed, what remains saved, whether retry is safe, what the user can do, and which administrator or integration owns the issue. |
| FC-FND-018 | Time, locale, and units | FOUNDATION | Target | Store canonical timestamps and units; display tenant/user locale, timezone, currency, and measurement preferences consistently. |
| FC-FND-019 | Retention, privacy, and legal hold awareness | FOUNDATION | Target | Product deletion/archive actions respect RecordArr retention, legal holds, privacy policy, and downstream references. |
| FC-FND-020 | AI-assisted proposals, never silent commits | UNDERSERVED | Target | AI may classify, summarize, suggest, or draft; consequential changes remain reviewable, permissioned, attributable, and reversible. |

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
<summary>Frontend page files (9)</summary>

| Page |
| --- |
| src/pages/ClockPage.tsx |
| src/pages/HomePage.tsx |
| src/pages/LaunchPage.tsx |
| src/pages/NotificationsPage.tsx |
| src/pages/OfflineQueuePage.tsx |
| src/pages/ProfilePage.tsx |
| src/pages/ReportPage.tsx |
| src/pages/ScanPage.tsx |
| src/pages/SurfacesPage.tsx |

</details>

<details>
<summary>Endpoint source families (0)</summary>

_No dedicated server endpoint route declarations were found in the static inventory._

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

Formalize mobile work/action contracts, encrypted offline queue and sync, capture-to-RecordArr, product validation, MAM integrations, and honest degraded-state behavior.

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
