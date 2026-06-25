# CustomArr — CRM Full Feature Set

[Workflow catalog](./WORKFLOWS.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md)

## Document control

| Field | Value |
| --- | --- |
| Product | CustomArr (CRM) |
| Category | Customer Relationship Management |
| Repository maturity | Durable |
| Audit basis | Static repository inspection on 2026-06-24 |
| Cataloged capabilities | 70 |
| Cataloged workflows | 16 |

## Product charter

CustomArr is the tenant customer and CRM system of record. It owns customer accounts and hierarchies, contacts and authorizations, addresses/locations in the customer context, commercial relationship history, requirements/preferences, leads, opportunities, proposals, agreements, customer onboarding, service cases, tasks/activities, health/success, and customer portal access. OrdArr owns order lifecycle; LedgArr owns invoices/payments; RecordArr owns files.

> **Implementation reality — Durable:** CustomArr has persistent customer/account, contacts, addresses, identifiers, billing profiles, requirements, external references, relationships, custom fields, activity, leads, opportunities, proposals, agreements, cases, tasks, portal access/submissions, service profiles, eligibility, onboarding, health, import, dedupe, merge, integrations, and extensive tenant configuration models. Remaining work is chiefly communication integration, stronger sales/service execution, consent/privacy, and cross-product orchestration.

## Source-of-truth boundary

### CustomArr owns

- Customer/account identity, hierarchy, relationships, identifiers, external refs, lifecycle stage, classifications, owners, and dedupe/merge history.
- Customer contacts, roles, authorizations, communication preferences/consent, and customer addresses/service locations in the customer context.
- Customer requirements, preferences, service profiles, eligibility, billing-profile references, custom fields, and contractual/commercial context.
- Leads, opportunities, pipeline, proposals, agreements, activities, tasks, interactions, and customer-facing commercial history.
- Customer onboarding/checklists, portal access, portal submissions, service/support cases, health profiles, renewal/retention context, and success plans.
- CRM imports, match/dedupe/merge, integration references, lifecycle/field/owner/notification rules, and tenant settings/audit.

### CustomArr does not own

- Order/request lifecycle, fulfillment handoffs, or order completion; OrdArr owns them.
- Supplier/vendor truth; SupplyArr owns procurement external parties.
- Invoices, payments, tax, collections accounting, or general ledger; LedgArr owns financial truth while CustomArr may show references/summaries.
- Documents/files; RecordArr owns storage and controlled documents.
- Transportation, warehouse, maintenance, quality, or compliance execution.
- Platform authentication; NexArr owns login/session, though CustomArr may request scoped portal access.

## Suite access and authorization

- All normal tenant users may launch every product application in the suite; product actions and records remain permission-scoped server-side.
- Fixed-suite access is nonvariable. Deployment health, feature readiness, legal restrictions, external-provider setup, and permissions may still constrain a capability.
- Platform administration remains a NexArr-only responsibility and is not a tenant product surface.
- Compliance Core administrative/authoring UI is restricted, while Compliance Core runtime evaluations, fact requirements, evidence needs, and workflow gates serve every tenant and product.

## Primary users

- Sales/business development user
- Account executive
- Account/customer success manager
- Customer service agent
- CRM administrator/data steward
- Customer portal user
- Sales manager
- Implementation/onboarding owner
- Auditor/reviewer

## Required integrations

- OrdArr
- LedgArr
- RoutArr
- LoadArr
- MaintainArr
- AssurArr
- RecordArr
- Compliance Core
- ReportArr
- NexArr
- Field Companion
- Email/calendar/telephony/messaging/form/e-sign providers

## Product principles

- CustomArr is CRM and the customer system of record; it is not a generic custom-app builder.
- Customer requirements are structured, effective-dated, and propagated as references—not buried in notes.
- Communication follows consent, preference, purpose, and channel rules; internal notes are distinct from customer-visible messages.
- Order execution remains OrdArr; finance execution remains LedgArr.

## Repository implementation snapshot

| Static indicator | Count |
| --- | --- |
| Persistent DbSet declarations | 50 |
| Discovered server classes | 134 |
| Discovered HTTP route declarations | 88 |
| Frontend source files | 10 |
| Frontend page files | 1 |
| Documentation headings | 82 |

### Evidence used for the current-state classification

- Persistent Customers, contacts, addresses, identifiers, billing profiles, requirements, external refs, relationships, custom fields, and activity.
- Persistent Leads, Opportunities, Proposals, Agreements, CustomerCases, CustomerTasks, PortalAccessRecords, service profiles, eligibility checks, onboarding/checklist items, and health profiles.
- Persistent import batches, dedupe candidates, merge records, integration references, portal submissions, and idempotency records.
- Persistent tenant settings for customer numbering, lifecycle stages/transitions, classifications, required fields, contact roles, address types, owner rules, onboarding templates, portal, document requirements, duplicate detection, integrations, external IDs, notifications, and custom fields/options plus audit.
- customarr-frontend routes for dashboard, accounts/customers/hierarchy/requirements/contacts, pipeline/commercial, support, operations, health, imports, integrations, and settings.

> Counts are static-discovery indicators, not proof that every route or screen is complete, reachable, secure, migrated, or production-ready.

## Feature catalog

### A. Currently implemented or meaningfully represented

These capabilities have repository evidence. Their state follows the product-level maturity and may still require hardening or completion.

| Feature ID | Capability | Class | State | Required behavior / evidence |
| --- | --- | --- | --- | --- |
| CU-CUR-001 | Customer account and hierarchy | CURRENT | Durable | Durable account records, relationships, hierarchy views, identifiers, classifications, ownership, and lifecycle. |
| CU-CUR-002 | Contacts, roles, and addresses | CURRENT | Durable | Contacts, contact roles, address types, and authorization-relevant customer context are persistent. |
| CU-CUR-003 | Customer requirements and service profiles | CURRENT | Durable | Requirements, preferences, service profiles, eligibility, and custom fields support operational handoffs. |
| CU-CUR-004 | Customer billing-profile references | CURRENT | Durable | Billing context is represented without taking ownership of finance execution. |
| CU-CUR-005 | Customer activity and tasks | CURRENT | Durable | Activity and task records support relationship history and follow-up. |
| CU-CUR-006 | Lead and opportunity pipeline | CURRENT | Durable | Leads, opportunities, pipeline/commercial routes, and lifecycle transitions are durable. |
| CU-CUR-007 | Proposals and agreements | CURRENT | Durable | Proposal/agreement records support commercial progression and handoff. |
| CU-CUR-008 | Customer cases/support | CURRENT | Durable | Customer cases and support routes support issue intake, ownership, status, and escalation. |
| CU-CUR-009 | Customer onboarding and checklist templates | CURRENT | Durable | Onboarding records/items and configurable templates support repeatable activation. |
| CU-CUR-010 | Customer health profiles | CURRENT | Durable | Health/success routes and persistent profiles support retention/service review. |
| CU-CUR-011 | Portal access and submissions | CURRENT | Durable | Scoped portal access records, tenant portal settings, and submission records are durable. |
| CU-CUR-012 | Imports, duplicate detection, and merge | CURRENT | Durable | Import batches, dedupe candidates/rules, merge records, external ID sources, and integration refs support data stewardship. |
| CU-CUR-013 | Configurable lifecycle, fields, ownership, and notifications | CURRENT | Durable | Tenant configuration models support no-code CRM adaptation within a governed customer model. |
| CU-CUR-014 | Tenant settings audit | CURRENT | Durable | Configuration changes are auditable. |

### B. Common category baseline

These are expected for a credible Customer Relationship Management product. Where they overlap current behavior, the target definition is the acceptance standard rather than a duplicate feature.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| CU-COM-001 | Account and contact management | COMMON | Target | Organizations/households/individuals, hierarchy, contacts, roles, relationships, addresses, consent, owner/team, and timeline. |
| CU-COM-002 | Lead capture and qualification | COMMON | Target | Web/import/manual/referral/partner leads, source attribution, scoring, routing, qualification, nurture, conversion, and disqualification reasons. |
| CU-COM-003 | Opportunity and pipeline management | COMMON | Target | Stages, value, probability, products/services, close date, competitors, stakeholders, activities, next step, and forecast categories. |
| CU-COM-004 | Activities and communications | COMMON | Target | Email, call, meeting, SMS/chat, notes, tasks, reminders, templates, sequences, and shared timeline with channel consent. |
| CU-COM-005 | Proposal, quote, and agreement context | COMMON | Target | Versions, products/services, pricing references, approvals, e-sign, acceptance, expiration, and order handoff. |
| CU-COM-006 | Customer onboarding | COMMON | Target | Requirements, contacts, locations, documents, eligibility, implementation tasks, training, portal, and activation criteria. |
| CU-COM-007 | Case and service management | COMMON | Target | Intake, categorization, priority, SLA, assignment, communications, knowledge, escalation, resolution, and satisfaction. |
| CU-COM-008 | Customer portal | COMMON | Target | Profile, contacts, requirements, order/status refs, cases, documents, messages, approvals, and submissions with scoped access. |
| CU-COM-009 | Customer success and renewal | COMMON | Target | Health, adoption/service use, issues, goals, milestones, risks, success plans, renewal, expansion, and churn reason. |
| CU-COM-010 | Territory, ownership, and assignment | COMMON | Target | Owner/team, region, segment, product/service, capacity, round-robin, named account, and reassignment history. |
| CU-COM-011 | Data quality and dedupe | COMMON | Target | Matching, merge, survivorship, external identifiers, validation, enrichment review, and change history. |
| CU-COM-012 | Forecasting and reporting | COMMON | Target | Pipeline, conversion, velocity, activity, win/loss, forecast, SLA, retention, health, revenue references, and customer concentration. |
| CU-COM-013 | Integrations and webhooks | COMMON | Target | Email/calendar/telephony/forms/marketing/e-sign/order/billing/support/data sync with mapping and conflict handling. |
| CU-COM-014 | Privacy and preference management | COMMON | Target | Lawful-purpose/consent basis, opt-in/out, preferred channel/time, subject request context, retention, and restricted fields. |

### C. Commonly requested but widely underserved

These address recurring user friction, fragmented add-ons, poor transparency, inaccessible setup, or weak SMB usability.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| CU-UND-001 | Simple CRM without a pricing cliff | UNDERSERVED | Target | Contacts, pipeline, workflows, custom fields, portal, API/webhooks, and security remain useful for small teams without enterprise-tier traps. |
| CU-UND-002 | True B2B account hierarchy and relationship graph | UNDERSERVED | Target | Parents, subsidiaries, sites, buying centers, service locations, partners, competitors, and person roles are navigable and permission-aware. |
| CU-UND-003 | Unified communication inbox with consent | UNDERSERVED | Target | Email, SMS, supported messaging, portal, calls, and internal notes share one customer timeline without violating channel permissions. |
| CU-UND-004 | Operational requirement handoff | UNDERSERVED | Target | Customer requirements become structured, versioned inputs to OrdArr, RoutArr, LoadArr, MaintainArr, AssurArr, and Compliance Core—not notes someone must remember. |
| CU-UND-005 | Customer-controlled data and preferences | UNDERSERVED | Target | Portal users can propose corrections, contacts, locations, preferences, document updates, and communication choices with review status. |
| CU-UND-006 | No-code custom fields and processes without arbitrary custom objects | UNDERSERVED | Target | Extend the governed customer model safely with field types, rules, layouts, and workflow templates instead of creating unowned shadow domains. |
| CU-UND-007 | Mutual action plans | UNDERSERVED | Target | Customer and team share milestones, owners, due dates, blockers, evidence, and outcomes during onboarding, implementations, renewals, or recovery. |
| CU-UND-008 | Relationship continuity when staff changes | UNDERSERVED | Target | Timeline, stakeholders, commitments, next steps, and account plans prevent customer knowledge from leaving with one salesperson. |
| CU-UND-009 | Transparent lead and account routing | UNDERSERVED | Target | Explain why ownership changed, capacity/rule used, response SLA, and correction path. |
| CU-UND-010 | Offline field CRM | UNDERSERVED | Target | Field staff can access scoped customer/visit context, take notes, scan, capture consent/signature, and queue follow-up without broad data download. |
| CU-UND-011 | Compliance-aware commercial workflow | UNDERSERVED | Target | Ask plain-language applicability/eligibility questions and explain blocked/warning requirements before promising work. |
| CU-UND-012 | Customer issue to quality/operations loop | UNDERSERVED | Target | A complaint can create AssurArr quality context, OrdArr/transport/maintenance follow-up, and return a coherent outcome to the customer case. |

### D. Advanced capabilities to democratize

These are commonly found only in enterprise tiers, specialist products, or expensive implementations. STL should make them practical without weakening governance.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| CU-DEM-001 | Revenue intelligence and relationship signals | DEMOCRATIZE | Target | Summarize engagement, stakeholders, risks, commitments, inactivity, next actions, and forecast evidence with citations and user control. |
| CU-DEM-002 | AI-assisted account research and call preparation | DEMOCRATIZE | Target | Combine approved internal history and public sources into a cited brief without silently changing records or exposing tenant data. |
| CU-DEM-003 | Conversation intelligence | DEMOCRATIZE | Target | Transcribe with consent, extract action/commitment/risk proposals, coach quality, and preserve source snippets/evidence. |
| CU-DEM-004 | Customer data platform lite | DEMOCRATIZE | Target | Resolve customer identity and events across approved sources into governed profiles and segments without a separate enterprise CDP. |
| CU-DEM-005 | Advanced forecasting and scenario analysis | DEMOCRATIZE | Target | Explain forecast changes, stage risk, historical accuracy, capacity, product/service constraints, and best/worst/commit scenarios. |
| CU-DEM-006 | Partner/channel relationship management | DEMOCRATIZE | Target | Partner onboarding, referrals, deal registration, shared accounts/opportunities, conflict rules, commissions refs, and scoped portals. |
| CU-DEM-007 | Configure-price-quote contribution | DEMOCRATIZE | Target | Guided configuration, eligibility, pricing rules, approvals, proposal generation, and OrdArr handoff while LedgArr remains finance owner. |
| CU-DEM-008 | Customer journey orchestration | DEMOCRATIZE | Target | Coordinate lifecycle milestones, communications, tasks, service events, and handoffs across products with consent and fatigue controls. |
| CU-DEM-009 | Customer risk and profitability view | DEMOCRATIZE | Target | Combine service cost, quality, fulfillment, payment/collection refs, growth, risk, and contractual obligations with governed definitions. |
| CU-DEM-010 | Secure customer data rooms | DEMOCRATIZE | Target | Scoped, expiring document exchange, requests, acknowledgements, approvals, and audit through RecordArr/portal without separate enterprise deal-room software. |

### E. Suite-wide foundation required in CustomArr

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| CU-FND-001 | Tenant-scoped authorization | FOUNDATION | Target | Every read and write is tenant-scoped server-side; product UI visibility never substitutes for authorization. |
| CU-FND-002 | StaffArr-backed action permissions | FOUNDATION | Target | Use product action keys, role scopes, delegated authority, and explicit denial reasons; ownership does not prevent permissioned delegated workflows. |
| CU-FND-003 | Unified suite shell | FOUNDATION | Target | Consistent navigation, page anatomy, terminology, responsive behavior, keyboard access, and light/dark contrast across products. |
| CU-FND-004 | Record lifecycle and history | FOUNDATION | Target | Human-readable statuses, timeline events, actor/time/source attribution, undo or correction paths, and immutable audit evidence. |
| CU-FND-005 | Quick create for missing references | FOUNDATION · UNDERSERVED | Target | Create the minimum valid cross-product reference without abandoning the current task; preserve context and allow later backfill. |
| CU-FND-006 | Saved views and personal preferences | COMMON | Target | Per-user filters, columns, density, sort, board/list preference, default landing view, and product-scoped preferences. |
| CU-FND-007 | Bulk operations with preview | COMMON | Target | Selection-aware actions, dry-run/impact preview, partial failure handling, downloadable results, and permission checks per record. |
| CU-FND-008 | Import with mapping and review | COMMON · UNDERSERVED | Target | Product-specific import separate from Smart Import, with templates, mappings, validation, dedupe, dry run, commit plan, and rollback/correction evidence. |
| CU-FND-009 | Export and portability | COMMON · DEMOCRATIZE | Target | CSV/PDF/JSON or package export appropriate to the domain, without trapping customer data behind enterprise tiers. |
| CU-FND-010 | Notifications, tasks, and inbox | COMMON | Target | Actionable notifications with ownership, due dates, escalation, snooze/delegation, deep links, and duplicate suppression. |
| CU-FND-011 | API, webhooks, and event outbox | COMMON · DEMOCRATIZE | Target | Stable versioned APIs, idempotency, service authentication, signed webhooks/events, retries, dead-letter handling, and correlation IDs. |
| CU-FND-012 | RecordArr evidence references | FOUNDATION | Target | Files and evidence are stored once in RecordArr and referenced by owning records; products do not create shadow document vaults. |
| CU-FND-013 | ReportArr projections and metrics | FOUNDATION | Target | Operational products emit facts/events; ReportArr builds read models and reports without becoming the source of operational truth. |
| CU-FND-014 | Compliance Core applicability and gates | FOUNDATION | Target | Products request explainable requirements, missing facts, evidence needs, and gate decisions rather than embedding independent legal logic. |
| CU-FND-015 | Field Companion execution surface | FOUNDATION · UNDERSERVED | Target | Mobile-first task, scan, capture, acknowledgement, and offline execution against the owning product API. |
| CU-FND-016 | Professional print and report view | COMMON | Target | Print output is a purpose-built report document without the app shell, not a screenshot of the current page. |
| CU-FND-017 | Accessible error and degraded states | FOUNDATION | Target | Explain what failed, what remains saved, whether retry is safe, what the user can do, and which administrator or integration owns the issue. |
| CU-FND-018 | Time, locale, and units | FOUNDATION | Target | Store canonical timestamps and units; display tenant/user locale, timezone, currency, and measurement preferences consistently. |
| CU-FND-019 | Retention, privacy, and legal hold awareness | FOUNDATION | Target | Product deletion/archive actions respect RecordArr retention, legal holds, privacy policy, and downstream references. |
| CU-FND-020 | AI-assisted proposals, never silent commits | UNDERSERVED | Target | AI may classify, summarize, suggest, or draft; consequential changes remain reviewable, permissioned, attributable, and reversible. |

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
<summary>Persistent entity sets (50)</summary>

| Entity type | DbSet/property | Source |
| --- | --- | --- |
| CustomArrCustomer | Customers | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerContact | CustomerContacts | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerAddress | CustomerAddresses | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerIdentifier | CustomerIdentifiers | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerBillingProfile | CustomerBillingProfiles | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerRequirement | CustomerRequirements | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerExternalRef | CustomerExternalRefs | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerRelationship | CustomerRelationships | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerCustomFieldValue | CustomerCustomFieldValues | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerActivity | CustomerActivity | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrLead | Leads | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrOpportunity | Opportunities | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrProposal | Proposals | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrAgreement | Agreements | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerCase | CustomerCases | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrTask | CustomerTasks | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrPortalAccessRecord | PortalAccessRecords | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerServiceProfile | CustomerServiceProfiles | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrEligibilityCheck | EligibilityChecks | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerOnboarding | CustomerOnboarding | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerOnboardingChecklistItem | CustomerOnboardingChecklistItems | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerHealthProfile | CustomerHealthProfiles | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrImportBatch | ImportBatches | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrDedupeCandidate | DedupeCandidates | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrMergeRecord | MergeRecords | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrIntegrationReference | IntegrationReferences | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrPortalSubmission | PortalSubmissions | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrIdempotencyRecord | IdempotencyRecords | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrTenantSettings | TenantSettings | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerNumberingSettings | CustomerNumberingSettings | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerLifecycleStage | CustomerLifecycleStages | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerLifecycleTransitionRule | CustomerLifecycleTransitionRules | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerClassificationCatalog | CustomerClassificationCatalogs | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerRequiredFieldRule | CustomerRequiredFieldRules | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerContactRole | CustomerContactRoles | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerAddressType | CustomerAddressTypes | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerOwnerRule | CustomerOwnerRules | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerOnboardingTemplate | CustomerOnboardingTemplates | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerOnboardingChecklistItemTemplate | CustomerOnboardingChecklistItemTemplates | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerPortalTenantSettings | CustomerPortalTenantSettings | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerDocumentRequirement | CustomerDocumentRequirements | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerDuplicateDetectionRule | CustomerDuplicateDetectionRules | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerIntegrationSettings | CustomerIntegrationSettings | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerExternalIdSource | CustomerExternalIdSources | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerNotificationRule | CustomerNotificationRules | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerCustomFieldDefinition | CustomerCustomFieldDefinitions | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrCustomerCustomFieldOption | CustomerCustomFieldOptions | CustomArr.Api/Data/CustomArrDbContext.cs |
| CustomArrTenantSettingsAuditEvent | TenantSettingsAuditEvents | CustomArr.Api/Data/CustomArrDbContext.cs |
| TEntity | set | CustomArr.Api/Services/CustomArrTenantSettingsService.cs |
| TEntity | set | CustomArr.Api/Services/CustomArrTenantSettingsService.cs |

</details>

<details>
<summary>Frontend page files (1)</summary>

| Page |
| --- |
| src/LaunchPage.tsx |

</details>

<details>
<summary>Endpoint source families (5)</summary>

| Endpoint source file | Discovered route declarations |
| --- | --- |
| CrmWorkspaceEndpoints.cs | 36 |
| TenantSettingsEndpoints.cs | 34 |
| WorkspaceEndpoints.cs | 7 |
| AuthEndpoints.cs | 6 |
| ReferenceIntegrationEndpoints.cs | 5 |

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

Complete durable CRM sales/service/portal workflows, flexible fields and automation, while leaving orders, finance, documents, and supplier records to their owners.

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
