# NexArr — IAM Full Feature Set

[Workflow catalog](./WORKFLOWS.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md)

## Document control

| Field | Value |
| --- | --- |
| Product | NexArr (IAM) |
| Category | Identity and Access Management |
| Repository maturity | Durable |
| Audit basis | Static repository inspection on 2026-06-24 |
| Cataloged capabilities | 69 |
| Cataloged workflows | 14 |

## Product charter

NexArr is the platform identity and service-trust management workspace. It authenticates people and machine clients, manages tenant membership and secure sessions, launches users into the suite, protects platform administration, and provides auditable credentials for cross-product integrations. It does not decide operational authority inside StaffArr, MaintainArr, RoutArr, or any other domain product.

> **Implementation reality — Durable:** The repository contains a broad persistent identity, tenant, session, service-token, integration, platform-event, Smart Import, and AI-proposal domain. The dedicated experience is delivered chiefly through suite-frontend rather than a standalone NexArr frontend. The settled model is a fixed-suite launch model with product-local permissions; any legacy product-access/license references are historical cleanup, not the current access design.

## Source-of-truth boundary

### NexArr owns

- Platform user accounts, credentials, authentication factors, recovery, sessions, and external identity mappings.
- Tenant identity, tenant membership, tenant lifecycle, and tenant-switch context.
- Platform roles and the platform-administrator boundary.
- Service clients, service tokens, integration credentials, launch profiles, handoff codes, and secure callbacks.
- Suite product catalog and launch metadata; all normal tenant products are available as a fixed suite rather than variable product gating.
- Platform audit events, outbox publishing, integration intake health, Smart Import orchestration, and AI action-proposal audit.
- Field Companion push subscriptions, notification dispatch infrastructure, offline-action intake, and secure mobile launch/session context.

### NexArr does not own

- Employment/personnel truth, reporting lines, positions, teams, or internal locations; StaffArr owns those records.
- Domain permissions such as closing a work order or releasing a quality hold; each product enforces its own actions using StaffArr-backed assignments.
- Compliance applicability, legal meaning, or evidence sufficiency; Compliance Core owns those decisions.
- Operational business records, documents, reports, or finance records.
- Fixed-suite product availability for ordinary tenants; product actions remain permission-scoped and platform-admin boundaries remain separate.

## Suite access and authorization

- All normal tenant users may launch every product application in the suite; product actions and records remain permission-scoped server-side.
- Fixed-suite access is nonvariable. Deployment health, feature readiness, legal restrictions, external-provider setup, and permissions may still constrain a capability.
- Platform administration remains a NexArr-only responsibility and is not a tenant product surface.
- Compliance Core administrative/authoring UI is restricted, while Compliance Core runtime evaluations, fact requirements, evidence needs, and workflow gates serve every tenant and product.

## Primary users

- Platform administrator
- Tenant administrator
- Identity/security administrator
- Integration administrator
- Service owner
- End user
- Access reviewer
- External collaborator

## Required integrations

- StaffArr
- All product APIs
- External OIDC/SAML identity providers
- SCIM clients/providers
- Email/SMS delivery providers
- RecordArr
- ReportArr
- Field Companion

## Product principles

- Authentication and platform administration are centralized; domain authorization remains local to the owning product.
- All ordinary tenant products are part of the suite. Product-specific action permission is not a commercial access grant.
- Secrets are write-only after entry, rotated, least-privilege, and never returned to browsers or logs.
- A StaffArr person may exist without a login; a login may be provisioned from StaffArr through NexArr-owned APIs.

## Repository implementation snapshot

| Static indicator | Count |
| --- | --- |
| Persistent DbSet declarations | 67 |
| Discovered server classes | 504 |
| Discovered HTTP route declarations | 307 |
| Frontend source files | 0 |
| Frontend page files | 0 |
| Documentation headings | 102 |

### Evidence used for the current-state classification

- apps/nexarr-api/NexArrDbContext.cs and persistent entities for users, credentials, sessions, tenants, memberships, platform roles, IdP mappings, service clients/tokens, handoffs, and audit.
- Persistent tenant integration connections, encrypted credential metadata, external mappings, sync runs, intake attempts, provider health, and mapping templates.
- Persistent platform outbox, publisher settings/runs, Smart Import batches/files/classifications/proposals/matches/review decisions/commit plans, and AI proposal/audit records.
- suite-frontend routes for login, password reset, account/preferences, imports, integrations, platform administration, reference data, and identity administration.
- Current tables named Entitlements and TenantProductLicenses are legacy compatibility storage from an older model. Live access and launch flows have been realigned around fixed-suite membership, launch-destination status, and product-local permission checks, with retired compatibility endpoints left only for explicit legacy callers.

> Counts are static-discovery indicators, not proof that every route or screen is complete, reachable, secure, migrated, or production-ready.

## Mandatory migration or refactor work

- Migrate or retire the remaining legacy access/license compatibility tables, endpoints, and checks so the fixed-suite access model is the only live control path; retain action permission, platform-admin, integration availability, and feature-readiness controls.

## Feature catalog

### A. Currently implemented or meaningfully represented

These capabilities have repository evidence. Their state follows the product-level maturity and may still require hardening or completion.

| Feature ID | Capability | Class | State | Required behavior / evidence |
| --- | --- | --- | --- | --- |
| NX-CUR-001 | Local account authentication and credential storage | CURRENT | Durable | User credentials, password reset tokens, session issuance, logout/revocation, and account-state checks are modeled persistently. |
| NX-CUR-002 | User and tenant membership administration | CURRENT | Durable | Create and manage platform users, tenants, memberships, active/inactive state, and tenant context. |
| NX-CUR-003 | Platform role assignments | CURRENT | Durable | Platform-level assignments and platform-admin authority are represented separately from product-domain permissions. |
| NX-CUR-004 | External identity provider mappings | CURRENT | Durable | Persist mappings between external identity subjects/providers and local users; complete protocol configuration remains a target. |
| NX-CUR-005 | Session inventory and controls | CURRENT | Durable | Track user sessions and tenant session policy, enabling revocation and cleanup workflows. |
| NX-CUR-006 | Service clients and service tokens | CURRENT | Durable | Issue, validate, rotate, audit, and clean up machine credentials used for product-to-product calls. |
| NX-CUR-007 | Secure product launch and handoff | CURRENT | Durable | Launch profiles, callback allowlists, tenant hints, return URLs, and short-lived handoff codes are modeled. |
| NX-CUR-008 | Tenant integration connection registry | CURRENT | Durable | Connections, credentials, mappings, sync runs, provider health, manual mapping templates, and intake attempts are durable. |
| NX-CUR-009 | Platform event outbox | CURRENT | Durable | Durable event publishing settings and runs support reliable platform integration behavior. |
| NX-CUR-010 | Reference-data ingestion administration | CURRENT | Durable | Datasets, sources, staging records, versions, crosswalks, tenant overlays, mappings, publish events, and audit are durable. |
| NX-CUR-011 | Smart Import review pipeline | CURRENT | Durable | Batches, files, classification, extracted fields, proposed records, match candidates, review decisions, commit plans, and audit are modeled. |
| NX-CUR-012 | AI session and action-proposal audit | CURRENT | Durable | AI conversations, proposals, and audit events support review-before-commit behavior. |
| NX-CUR-013 | Field Companion notification and offline intake infrastructure | CURRENT | Durable | Push subscriptions, dispatch records, tenant notification settings, test dispatch, offline actions, and field submissions are represented. |
| NX-CUR-014 | Platform audit package and maintenance jobs | CURRENT | Durable | Cleanup, reconciliation, tenant lifecycle, and audit-package job settings/runs are modeled. |
| NX-CUR-015 | Fixed-suite launch model cleanup | CURRENT | Durable | Historical access/license references are retained only where needed for migration evidence and backward-compatible redirects; launch decisions use fixed-suite suite membership plus product-local permissions. |

### B. Common category baseline

These are expected for a credible Identity and Access Management product. Where they overlap current behavior, the target definition is the acceptance standard rather than a duplicate feature.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| NX-COM-001 | OIDC and SAML single sign-on | COMMON | Target | Standards-based workforce SSO with tested metadata exchange, signed requests, certificate rotation, domain discovery, and safe fallback. |
| NX-COM-002 | Multi-factor authentication | COMMON | Target | TOTP, WebAuthn/passkeys, recovery codes, step-up authentication, remembered devices, and administrator reset with audit. |
| NX-COM-003 | Passwordless authentication | COMMON | Target | Passkeys and secure magic-link or device-bound login where policy permits, without weakening recovery controls. |
| NX-COM-004 | SCIM user and group provisioning | COMMON | Target | Standards-based joiner/mover/leaver provisioning with reconciliation, conflict handling, deprovisioning safety, and dry-run visibility. |
| NX-COM-005 | Account lock, suspend, reactivate, and terminate | COMMON | Target | Explicit lifecycle states that block new sessions, revoke active sessions, and preserve audit history. |
| NX-COM-006 | Delegated tenant administration | COMMON | Target | Tenant administrators manage permitted identity operations without platform-admin access or cross-tenant visibility. |
| NX-COM-007 | Session and device management | COMMON | Target | Users and administrators can view, name, revoke, and investigate sessions/devices with location and risk context where legally appropriate. |
| NX-COM-008 | Authentication policy | COMMON | Target | Configurable password, MFA, session lifetime, idle timeout, IP/network, and step-up policies with safe defaults. |
| NX-COM-009 | Application/service registration | COMMON | Target | Register trusted services, redirect URIs, scopes, secrets/certificates, and ownership contacts. |
| NX-COM-010 | Identity audit and sign-in reporting | COMMON | Target | Searchable login, failure, factor, session, admin, token, and IdP events with export and retention. |
| NX-COM-011 | Break-glass administration | COMMON | Target | Tightly controlled emergency accounts with offline recovery material, forced review, and immediate notifications. |
| NX-COM-012 | Self-service account recovery | COMMON | Target | Verified recovery paths that do not expose whether an account exists and include administrator override controls. |
| NX-COM-013 | Tenant switcher and membership context | COMMON | Target | Clear current-tenant identity, safe switching, recent tenants, and no silent cross-tenant carryover. |
| NX-COM-014 | Machine identity lifecycle | COMMON | Target | Client owner, purpose, scopes, expiration, rotation, usage history, last-used data, and automatic disablement of abandoned credentials. |

### C. Commonly requested but widely underserved

These address recurring user friction, fragmented add-ons, poor transparency, inaccessible setup, or weak SMB usability.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| NX-UND-001 | No-SSO-tax security baseline | UNDERSERVED | Target | Provide SSO, MFA, SCIM, and core audit capabilities without forcing small organizations into an enterprise contract solely to use standard security. |
| NX-UND-002 | Human-readable authorization explanations | UNDERSERVED | Target | Explain which role, scope, policy, or missing assignment allowed or denied an action, without exposing exploitable internals. |
| NX-UND-003 | Permission simulator | UNDERSERVED | Target | Preview effective access for a user, role, location, and product action before saving changes. |
| NX-UND-004 | Temporary and just-in-time access | UNDERSERVED | Target | Time-boxed, reasoned, approval-backed access that automatically expires and appears in reviews. |
| NX-UND-005 | Unified human and machine identity inventory | UNDERSERVED | Target | One searchable graph for users, linked StaffArr people, external identities, service clients, owners, scopes, dependencies, and stale-risk indicators. |
| NX-UND-006 | Safe tenant offboarding and export | UNDERSERVED | Target | Preview dependencies, export identity/audit data, revoke integrations, archive tenant state, and prove completion without destructive surprises. |
| NX-UND-007 | Identity-linked quick create from StaffArr | UNDERSERVED | Target | Permissioned StaffArr users can provision or update a login through NexArr-backed actions while StaffArr remains the working HR surface. |
| NX-UND-008 | Transparent degraded authentication | UNDERSERVED | Target | Clearly distinguish invalid credentials, IdP outage, tenant suspension, policy block, and service degradation while keeping messages secure. |
| NX-UND-009 | Low-friction external collaborator identity | UNDERSERVED | Target | Scoped, expiring portal identities for suppliers, customers, auditors, and contractors without consuming full internal-user administration overhead. |
| NX-UND-010 | Cross-product access review inbox | UNDERSERVED | Target | Review actual high-risk product actions and scopes, not only coarse application assignment. |

### D. Advanced capabilities to democratize

These are commonly found only in enterprise tiers, specialist products, or expensive implementations. STL should make them practical without weakening governance.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| NX-DEM-001 | Identity governance and access certification | DEMOCRATIZE | Target | Campaigns, reviewers, evidence, recommendations, revocation, exceptions, and closure reporting suitable for small teams. |
| NX-DEM-002 | Lifecycle automation | DEMOCRATIZE | Target | Joiner/mover/leaver rules triggered by StaffArr status, position, location, or contract dates, with preview and approval. |
| NX-DEM-003 | Privileged access management lite | DEMOCRATIZE | Target | Vaulted emergency credentials, checkout/approval, short-lived elevation, command/action context, and post-use review for platform operations. |
| NX-DEM-004 | Adaptive authentication | DEMOCRATIZE | Target | Risk-informed step-up using session, device, location, impossible-travel, and behavior signals with transparent policy and appeal/recovery. |
| NX-DEM-005 | Identity threat detection | DEMOCRATIZE | Target | Detect impossible travel, token replay, anomalous service-client use, MFA fatigue, dormant privileged accounts, and unusual tenant switching. |
| NX-DEM-006 | Fine-grained attribute-based access | DEMOCRATIZE | Target | Policy conditions using role, location, assignment, qualification, record state, time, and risk while preserving explainability. |
| NX-DEM-007 | Bring-your-own identity provider | DEMOCRATIZE | Target | Self-service setup, validation, certificate rollover, staged cutover, test users, and rollback without professional-services dependency. |
| NX-DEM-008 | Credentialless workload identity | DEMOCRATIZE | Target | Short-lived workload credentials and federation for cloud/runtime services rather than long-lived shared secrets. |
| NX-DEM-009 | Policy-as-code export and validation | DEMOCRATIZE | Target | Version, diff, test, approve, and export identity policies in a portable representation. |
| NX-DEM-010 | Cross-tenant managed service delegation | DEMOCRATIZE | Target | Explicit customer-approved support delegation with per-tenant scope, reason, duration, recording, and revocation. |

### E. Suite-wide foundation required in NexArr

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| NX-FND-001 | Tenant-scoped authorization | FOUNDATION | Target | Every read and write is tenant-scoped server-side; product UI visibility never substitutes for authorization. |
| NX-FND-002 | StaffArr-backed action permissions | FOUNDATION | Target | Use product action keys, role scopes, delegated authority, and explicit denial reasons; ownership does not prevent permissioned delegated workflows. |
| NX-FND-003 | Unified suite shell | FOUNDATION | Target | Consistent navigation, page anatomy, terminology, responsive behavior, keyboard access, and light/dark contrast across products. |
| NX-FND-004 | Record lifecycle and history | FOUNDATION | Target | Human-readable statuses, timeline events, actor/time/source attribution, undo or correction paths, and immutable audit evidence. |
| NX-FND-005 | Quick create for missing references | FOUNDATION · UNDERSERVED | Target | Create the minimum valid cross-product reference without abandoning the current task; preserve context and allow later backfill. |
| NX-FND-006 | Saved views and personal preferences | COMMON | Target | Per-user filters, columns, density, sort, board/list preference, default landing view, and product-scoped preferences. |
| NX-FND-007 | Bulk operations with preview | COMMON | Target | Selection-aware actions, dry-run/impact preview, partial failure handling, downloadable results, and permission checks per record. |
| NX-FND-008 | Import with mapping and review | COMMON · UNDERSERVED | Target | Product-specific import separate from Smart Import, with templates, mappings, validation, dedupe, dry run, commit plan, and rollback/correction evidence. |
| NX-FND-009 | Export and portability | COMMON · DEMOCRATIZE | Target | CSV/PDF/JSON or package export appropriate to the domain, without trapping customer data behind enterprise tiers. |
| NX-FND-010 | Notifications, tasks, and inbox | COMMON | Target | Actionable notifications with ownership, due dates, escalation, snooze/delegation, deep links, and duplicate suppression. |
| NX-FND-011 | API, webhooks, and event outbox | COMMON · DEMOCRATIZE | Target | Stable versioned APIs, idempotency, service authentication, signed webhooks/events, retries, dead-letter handling, and correlation IDs. |
| NX-FND-012 | RecordArr evidence references | FOUNDATION | Target | Files and evidence are stored once in RecordArr and referenced by owning records; products do not create shadow document vaults. |
| NX-FND-013 | ReportArr projections and metrics | FOUNDATION | Target | Operational products emit facts/events; ReportArr builds read models and reports without becoming the source of operational truth. |
| NX-FND-014 | Compliance Core applicability and gates | FOUNDATION | Target | Products request explainable requirements, missing facts, evidence needs, and gate decisions rather than embedding independent legal logic. |
| NX-FND-015 | Field Companion execution surface | FOUNDATION · UNDERSERVED | Target | Mobile-first task, scan, capture, acknowledgement, and offline execution against the owning product API. |
| NX-FND-016 | Professional print and report view | COMMON | Target | Print output is a purpose-built report document without the app shell, not a screenshot of the current page. |
| NX-FND-017 | Accessible error and degraded states | FOUNDATION | Target | Explain what failed, what remains saved, whether retry is safe, what the user can do, and which administrator or integration owns the issue. |
| NX-FND-018 | Time, locale, and units | FOUNDATION | Target | Store canonical timestamps and units; display tenant/user locale, timezone, currency, and measurement preferences consistently. |
| NX-FND-019 | Retention, privacy, and legal hold awareness | FOUNDATION | Target | Product deletion/archive actions respect RecordArr retention, legal holds, privacy policy, and downstream references. |
| NX-FND-020 | AI-assisted proposals, never silent commits | UNDERSERVED | Target | AI may classify, summarize, suggest, or draft; consequential changes remain reviewable, permissioned, attributable, and reversible. |

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
<summary>Persistent entity sets (67)</summary>

| Entity type | DbSet/property | Source |
| --- | --- | --- |
| PlatformUser | Users | NexArr.Api/Data/NexArrDbContext.cs |
| UserCredential | UserCredentials | NexArr.Api/Data/NexArrDbContext.cs |
| UserSession | UserSessions | NexArr.Api/Data/NexArrDbContext.cs |
| Tenant | Tenants | NexArr.Api/Data/NexArrDbContext.cs |
| TenantMembership | TenantMemberships | NexArr.Api/Data/NexArrDbContext.cs |
| PlatformRoleAssignment | PlatformRoleAssignments | NexArr.Api/Data/NexArrDbContext.cs |
| ExternalIdentityProviderMapping | ExternalIdentityProviderMappings | NexArr.Api/Data/NexArrDbContext.cs |
| ProductCatalogItem | ProductCatalog | NexArr.Api/Data/NexArrDbContext.cs |
| TenantProductEntitlement (legacy launch-destination compatibility record) | Entitlements | NexArr.Api/Data/NexArrDbContext.cs |
| PlatformAuditEvent | AuditEvents | NexArr.Api/Data/NexArrDbContext.cs |
| ServiceClient | ServiceClients | NexArr.Api/Data/NexArrDbContext.cs |
| ServiceTokenRecord | ServiceTokens | NexArr.Api/Data/NexArrDbContext.cs |
| ProductLaunchProfile | LaunchProfiles | NexArr.Api/Data/NexArrDbContext.cs |
| HandoffCodeRecord | HandoffCodes | NexArr.Api/Data/NexArrDbContext.cs |
| ReferenceDataset | ReferenceDatasets | NexArr.Api/Data/NexArrDbContext.cs |
| ReferenceSource | ReferenceSources | NexArr.Api/Data/NexArrDbContext.cs |
| IngestionJob | IngestionJobs | NexArr.Api/Data/NexArrDbContext.cs |
| StagingRecord | StagingRecords | NexArr.Api/Data/NexArrDbContext.cs |
| ReferenceEntity | ReferenceEntities | NexArr.Api/Data/NexArrDbContext.cs |
| ReferenceEntityVersion | ReferenceEntityVersions | NexArr.Api/Data/NexArrDbContext.cs |
| ReferenceCrosswalk | ReferenceCrosswalks | NexArr.Api/Data/NexArrDbContext.cs |
| TenantReferenceOverlay | TenantReferenceOverlays | NexArr.Api/Data/NexArrDbContext.cs |
| ProductMapping | ProductMappings | NexArr.Api/Data/NexArrDbContext.cs |
| ReferencePublishEvent | ReferencePublishEvents | NexArr.Api/Data/NexArrDbContext.cs |
| ReferenceAuditEvent | ReferenceAuditEvents | NexArr.Api/Data/NexArrDbContext.cs |
| PasswordResetToken | PasswordResetTokens | NexArr.Api/Data/NexArrDbContext.cs |
| ProductCallbackAllowlistEntry | CallbackAllowlist | NexArr.Api/Data/NexArrDbContext.cs |
| TenantFieldCompanionNotificationSettings | TenantFieldCompanionNotificationSettings | NexArr.Api/Data/NexArrDbContext.cs |
| FieldCompanionNotificationDispatch | FieldCompanionNotificationDispatches | NexArr.Api/Data/NexArrDbContext.cs |
| FieldCompanionPushSubscription | FieldCompanionPushSubscriptions | NexArr.Api/Data/NexArrDbContext.cs |
| PlatformAuditPackageGenerationJob | PlatformAuditPackageGenerationJobs | NexArr.Api/Data/NexArrDbContext.cs |
| PlatformServiceTokenCleanupSettings | PlatformServiceTokenCleanupSettings | NexArr.Api/Data/NexArrDbContext.cs |
| PlatformSessionSettings | PlatformSessionSettings | NexArr.Api/Data/NexArrDbContext.cs |
| ServiceTokenCleanupRun | ServiceTokenCleanupRuns | NexArr.Api/Data/NexArrDbContext.cs |
| TenantProductLicense | TenantProductLicenses | NexArr.Api/Data/NexArrDbContext.cs |
| PlatformLaunchDestinationReconciliationSettings | PlatformLaunchDestinationReconciliationSettings | NexArr.Api/Data/NexArrDbContext.cs |
| LaunchDestinationReconciliationRun | LaunchDestinationReconciliationRuns | NexArr.Api/Data/NexArrDbContext.cs |
| PlatformTenantLifecycleSettings | PlatformTenantLifecycleSettings | NexArr.Api/Data/NexArrDbContext.cs |
| TenantLifecycleRun | TenantLifecycleRuns | NexArr.Api/Data/NexArrDbContext.cs |
| FieldCompanionOfflineAction | FieldCompanionOfflineActions | NexArr.Api/Data/NexArrDbContext.cs |
| FieldCompanionFieldSubmission | FieldCompanionFieldSubmissions | NexArr.Api/Data/NexArrDbContext.cs |
| TenantProductDataPlaneProfile | DataPlaneProfiles | NexArr.Api/Data/NexArrDbContext.cs |
| TenantIntegrationConnection | TenantIntegrationConnections | NexArr.Api/Data/NexArrDbContext.cs |
| TenantIntegrationCredential | TenantIntegrationCredentials | NexArr.Api/Data/NexArrDbContext.cs |
| TenantIntegrationExternalMapping | TenantIntegrationExternalMappings | NexArr.Api/Data/NexArrDbContext.cs |
| TenantIntegrationSyncRun | TenantIntegrationSyncRuns | NexArr.Api/Data/NexArrDbContext.cs |
| TenantIntegrationIntakeAttempt | TenantIntegrationIntakeAttempts | NexArr.Api/Data/NexArrDbContext.cs |
| TenantIntegrationProviderHealth | TenantIntegrationProviderHealth | NexArr.Api/Data/NexArrDbContext.cs |
| TenantIntegrationManualMappingTemplate | TenantIntegrationManualMappingTemplates | NexArr.Api/Data/NexArrDbContext.cs |
| PlatformOutboxEvent | PlatformOutboxEvents | NexArr.Api/Data/NexArrDbContext.cs |
| PlatformOutboxPublisherSettings | PlatformOutboxPublisherSettings | NexArr.Api/Data/NexArrDbContext.cs |
| PlatformOutboxPublisherRun | PlatformOutboxPublisherRuns | NexArr.Api/Data/NexArrDbContext.cs |
| AiSession | AiSessions | NexArr.Api/Data/NexArrDbContext.cs |
| AiMessage | AiMessages | NexArr.Api/Data/NexArrDbContext.cs |
| AiActionProposal | AiActionProposals | NexArr.Api/Data/NexArrDbContext.cs |
| AiAuditEvent | AiAuditEvents | NexArr.Api/Data/NexArrDbContext.cs |
| ImportBatch | ImportBatches | NexArr.Api/Data/NexArrDbContext.cs |
| ImportFile | ImportFiles | NexArr.Api/Data/NexArrDbContext.cs |
| ImportClassification | ImportClassifications | NexArr.Api/Data/NexArrDbContext.cs |
| ImportExtractedField | ImportExtractedFields | NexArr.Api/Data/NexArrDbContext.cs |
| ImportProposedRecord | ImportProposedRecords | NexArr.Api/Data/NexArrDbContext.cs |
| ImportMatchCandidate | ImportMatchCandidates | NexArr.Api/Data/NexArrDbContext.cs |
| ImportReviewDecision | ImportReviewDecisions | NexArr.Api/Data/NexArrDbContext.cs |
| ImportCommitPlan | ImportCommitPlans | NexArr.Api/Data/NexArrDbContext.cs |
| ImportCommitStep | ImportCommitSteps | NexArr.Api/Data/NexArrDbContext.cs |
| ImportAuditEvent | ImportAuditEvents | NexArr.Api/Data/NexArrDbContext.cs |
| ImportMappingTemplate | ImportMappingTemplates | NexArr.Api/Data/NexArrDbContext.cs |

</details>

<details>
<summary>Frontend page files (0)</summary>

_No dedicated frontend page files were found in the static inventory._

</details>

<details>
<summary>Endpoint source families (45)</summary>

| Endpoint source file | Discovered route declarations |
| --- | --- |
| PlatformAdminEndpoints.cs | 34 |
| ReferenceDataEndpoints.cs | 34 |
| AuthEndpoints.cs | 22 |
| ServiceTokenEndpoints.cs | 22 |
| TenantIntegrationEndpoints.cs | 22 |
| LaunchEndpoints.cs | 20 |
| TenantEndpoints.cs | 18 |
| SmartImportEndpoints.cs | 13 |
| ProductEndpoints.cs | 10 |
| RetiredEntitlementCompatibilityEndpoints.cs | 9 |
| PlatformAuditPackageEndpoints.cs | 8 |
| AiAssistanceEndpoints.cs | 6 |
| PlatformLaunchDestinationReconciliationEndpoints.cs | 6 |
| AuditEndpoints.cs | 5 |
| HybridDataPlaneEndpoints.cs | 5 |
| InternalPlatformIdentityEndpoints.cs | 5 |
| PlatformOutboxPublisherEndpoints.cs | 5 |
| PlatformWorkerHealthOrchestrationEndpoints.cs | 5 |
| PlatformTenantLifecycleEndpoints.cs | 4 |
| FieldCompanionEndpoints.cs | 3 |
| FieldCompanionFieldInspectionEndpoints.cs | 3 |
| FieldCompanionFieldReceivingEndpoints.cs | 3 |
| FieldCompanionFieldWorkOrderEndpoints.cs | 3 |
| FieldCompanionNotificationEndpoints.cs | 3 |
| FieldCompanionPushEndpoints.cs | 3 |
| InternalPlatformOutboxPublisherEndpoints.cs | 3 |
| PlatformHealthEndpoints.cs | 3 |
| PlatformServiceTokenCleanupEndpoints.cs | 3 |
| ServiceTokenDiscoveryEndpoints.cs | 3 |
| FieldCompanionClockEndpoints.cs | 2 |
| FieldCompanionFieldSubmissionEndpoints.cs | 2 |
| FieldCompanionOfflineEndpoints.cs | 2 |
| InternalLaunchDestinationReconciliationEndpoints.cs | 2 |
| InternalFieldCompanionNotificationEndpoints.cs | 2 |
| InternalPlatformAuditPackageGenerationEndpoints.cs | 2 |
| InternalServiceTokenCleanupEndpoints.cs | 2 |
| InternalTenantLifecycleEndpoints.cs | 2 |
| FieldCompanionFieldDvirEndpoints.cs | 1 |
| FieldCompanionFieldEvidenceEndpoints.cs | 1 |
| FieldCompanionScanEndpoints.cs | 1 |
| InternalIntegrationTokenEndpoints.cs | 1 |
| InternalPersonLoginDisableEndpoints.cs | 1 |
| InternalPersonLoginEnableEndpoints.cs | 1 |
| PlatformLifecycleOverviewEndpoints.cs | 1 |
| SettingsEndpoints.cs | 1 |

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

Retire or refactor any remaining legacy access/license references so launch reflects the fixed-suite launch model; retain permission, feature-readiness, integration-health, and platform-admin boundaries.

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
