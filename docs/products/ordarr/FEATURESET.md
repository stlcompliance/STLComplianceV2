# OrdArr — OMS Full Feature Set

[Workflow catalog](./WORKFLOWS.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md) · [Rollout](./ROLLOUT.md)

## Document control

| Field | Value |
| --- | --- |
| Product | OrdArr (OMS) |
| Category | Order Management System |
| Repository maturity | Scaffold |
| Audit basis | Static repository inspection on 2026-06-24 |
| Cataloged capabilities | 66 |
| Cataloged workflows | 15 |

## Product charter

OrdArr owns the lifecycle of customer or internal orders/requests and coordinates execution handoffs. It captures demand, validates readiness, manages order status and holds, decomposes an order into product-owned execution requests, tracks handoff acceptance/block/completion, manages changes/cancellation/returns at the order level, and produces completion and finance-ready packet references. It does not execute warehouse, transport, maintenance, quality, procurement, customer, or finance work.

> **Implementation reality — Scaffold:** OrdArr has a substantial API/UX contract and an in-memory OrdArrStore for orders, lines, holds, timeline, returns, handoffs, completion/invoice-ready/bill-ready packets, readiness, and idempotency. Its EF context does not expose a durable operational model. The current product is therefore a functional prototype/scaffold; durable persistence, concurrency, event delivery, and end-to-end handoff reconciliation are prerequisites for production.

## Source-of-truth boundary

### OrdArr owns

- Order/request header and lines, source channel, customer/requester references, order type, requested dates, priorities, terms/requirements snapshot refs, and external identifiers.
- Order validation, lifecycle status, holds, readiness, changes, cancellation, timeline, and customer-safe status.
- Execution decomposition and handoffs to LoadArr, RoutArr, MaintainArr, SupplyArr, AssurArr, RecordArr, and other owning products.
- Handoff acceptance/rejection/block/completion status, dependencies, allocation of order lines to execution records, and recovery coordination.
- Order-level returns/RMA coordination and completion/invoice-ready/bill-ready packet references.
- Idempotency, order API/webhook contracts, and order reporting source events.

### OrdArr does not own

- Customer master, contacts, requirements, or agreements; CustomArr owns them.
- Inventory availability/balance, allocation, pick, ship; LoadArr owns warehouse execution.
- Transportation planning/trip/proof; RoutArr owns it.
- Service/maintenance work and asset readiness; MaintainArr owns them.
- Supplier/procurement records; SupplyArr owns them.
- Quality hold/release; AssurArr owns quality decisions.
- Invoices, bills, payments, tax, or GL; LedgArr owns financial execution.
- Files/evidence packages; RecordArr owns them.

## Suite access and authorization

- All normal tenant users may launch every product application in the suite; product actions and records remain permission-scoped server-side.
- Fixed-suite access is nonvariable. Deployment health, feature readiness, legal restrictions, external-provider setup, and permissions may still constrain a capability.
- Platform administration remains a NexArr-only responsibility and is not a tenant product surface.
- Compliance Core administrative/authoring UI is restricted, while Compliance Core runtime evaluations, fact requirements, evidence needs, and workflow gates serve every tenant and product.

## Primary users

- Order entry user
- Order coordinator
- Operations/customer service manager
- Customer portal user
- Fulfillment planner
- Returns coordinator
- Finance readiness reviewer
- Integration administrator
- Auditor

## Required integrations

- CustomArr
- LoadArr
- RoutArr
- MaintainArr
- SupplyArr
- AssurArr
- RecordArr
- Compliance Core
- ReportArr
- LedgArr
- NexArr
- Field Companion
- External order/EDI/marketplace channels

## Product principles

- OrdArr coordinates execution; it never becomes a shadow WMS, TMS, CMMS, SRM, QMS, CRM, DMS, or ERP.
- Every handoff has an owner, idempotency key, source/version, acceptance state, blocker explanation, and reconciliation path.
- The current in-memory store is not production-safe; durable persistence and atomic outbox are release blockers.
- Customer promises are versioned commitments with source assumptions and explicit renegotiation when conditions change.

## Repository implementation snapshot

| Static indicator | Count |
| --- | --- |
| Persistent DbSet declarations | 0 |
| Discovered server classes | 29 |
| Discovered HTTP route declarations | 28 |
| Frontend source files | 9 |
| Frontend page files | 1 |
| Documentation headings | 61 |

### Evidence used for the current-state classification

- ordarr-api contains an in-memory OrdArrStore supporting orders, lines, holds, timeline, returns, handoffs, readiness, completion packets, invoice-ready/bill-ready packet data, and idempotency.
- The OrdArr EF context is effectively empty for the operational domain; in-memory state is lost on restart and cannot provide production concurrency/audit guarantees.
- ordarr-frontend routes include dashboard, order list/detail/create, handoffs, completion, reports, and settings.
- Canonical docs define order request, lifecycle/status, handoff coordination, completion/financial packet, workflow events/APIs, and order fulfillment events.
- Cross-product workflow packs define Order → Fulfillment and Vendor Order Completion → Dispatch coordination.

> Counts are static-discovery indicators, not proof that every route or screen is complete, reachable, secure, migrated, or production-ready.

## Mandatory migration or refactor work

- Replace the process-local OrdArrStore with tenant-scoped durable persistence, concurrency, migrations, outbox, audit history, and restart-safe workflows.

## Feature catalog

### A. Currently implemented or meaningfully represented

These capabilities have repository evidence. Their state follows the product-level maturity and may still require hardening or completion.

| Feature ID | Capability | Class | State | Required behavior / evidence |
| --- | --- | --- | --- | --- |
| OR-CUR-001 | Order create/read/update prototype | CURRENT | Partial | Frontend/API routes and tests exercise core order-entry, detail, and line-update behavior. |
| OR-CUR-002 | Order line prototype | CURRENT | Partial | Order lines can represent requested goods/services and execution needs, including routed execution targets. |
| OR-CUR-003 | Lifecycle and readiness prototype | CURRENT | Scaffold | Statuses, readiness checks, and timeline concepts are present. |
| OR-CUR-004 | Order hold prototype | CURRENT | Partial | Hold records/actions support blocked order progression. |
| OR-CUR-005 | Execution handoff prototype | CURRENT | Partial | Handoff records track requests to product-owned execution domains. |
| OR-CUR-006 | Handoff status and completion prototype | CURRENT | Scaffold | Acceptance/block/completion can be reflected in order status. |
| OR-CUR-007 | Order return prototype | CURRENT | Scaffold | Return/RMA-like records exist in the store. |
| OR-CUR-008 | Completion and finance-ready packet prototype | CURRENT | Partial | Approved order closeout can reference and advance completion, invoice-ready, and bill-ready packet structures from the order detail workspace. |
| OR-CUR-009 | Idempotency prototype | CURRENT | Scaffold | The store models duplicate-safe request handling. |
| OR-CUR-010 | Dashboard, order, handoff, completion, report, and settings UI scaffold | CURRENT | Partial | The frontend establishes the intended navigation and primary user tasks. |

### B. Common category baseline

These are expected for a credible Order Management System product. Where they overlap current behavior, the target definition is the acceptance standard rather than a duplicate feature.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| OR-COM-001 | Omnichannel order capture | COMMON | Target | Internal UI, customer portal, API, EDI, import, marketplace/site, recurring, quote/agreement, and integration sources with idempotency. |
| OR-COM-002 | Order types and configurable lifecycle | COMMON | Target | Sales, service, internal, transfer, return, replacement, sample, subscription/repeat, and other governed types with state rules. |
| OR-COM-003 | Order validation and enrichment | COMMON | Target | Customer eligibility, addresses, contacts, terms, requirements, item/service, quantity/UOM, dates, pricing refs, tax/finance refs, and compliance facts. |
| OR-COM-004 | Order promising | COMMON | Target | Available-to-promise/capable-to-promise using inventory, procurement, capacity, transport, service, calendars, and constraints with explainable dates. |
| OR-COM-005 | Order orchestration and decomposition | COMMON | Target | Split lines/quantities into warehouse, transport, maintenance/service, procurement, quality, document, and external handoffs. |
| OR-COM-006 | Allocation, split, partial, backorder, and substitution coordination | COMMON | Target | Order-level decisions and customer approval while LoadArr/SupplyArr own physical/procurement execution. |
| OR-COM-007 | Order holds and approvals | COMMON | Target | Credit/eligibility, compliance, quality, inventory, fraud, document, customer, price, capacity, and manual holds with owners and release rules. |
| OR-COM-008 | Order changes and cancellation | COMMON | Target | Versioned amendments, impact preview, approval, downstream compensation, fees/financial refs, and customer communication. |
| OR-COM-009 | Fulfillment status and exception coordination | COMMON | Target | Track handoffs, milestones, blockers, promises, partials, exceptions, and recovery across products. |
| OR-COM-010 | Returns and exchanges | COMMON | Target | RMA eligibility, authorization, receipt/inspection, disposition, replacement, refund/credit refs, and closure. |
| OR-COM-011 | Order completion and financial readiness | COMMON | Target | Confirm required handoffs/evidence, generate customer completion and finance packet refs, and preserve correction/reopen workflow. |
| OR-COM-012 | Customer/order portal status | COMMON | Target | Customer-safe status, actions, changes, documents, appointments, proof, returns, and messages. |
| OR-COM-013 | Order analytics | COMMON | Target | Cycle time, fill/complete, promise accuracy, hold aging, cancellation, return, handoff performance, backlog, and exception causes. |
| OR-COM-014 | API/events and integration | COMMON | Target | Stable create/change/status/hold/handoff/return contracts, outbox, webhooks, retries, dedupe, and reconciliation. |

### C. Commonly requested but widely underserved

These address recurring user friction, fragmented add-ons, poor transparency, inaccessible setup, or weak SMB usability.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| OR-UND-001 | SMB omnichannel orchestration without enterprise complexity | UNDERSERVED | Target | A small team can coordinate goods, services, pickup/delivery, procurement, and field work from one order without a huge implementation. |
| OR-UND-002 | Explainable promise date | UNDERSERVED | Target | Show which inventory, supplier, capacity, transport, customer, calendar, or compliance fact determines the date and confidence. |
| OR-UND-003 | Goods-plus-service orders | UNDERSERVED | Target | One order can coordinate parts, warehouse fulfillment, installation/maintenance, transportation, training, documents, and acceptance. |
| OR-UND-004 | Customer self-service changes with impact preview | UNDERSERVED | Target | Customers can request date, quantity, address, contact, pickup/delivery, cancellation, or substitution changes within policy. |
| OR-UND-005 | Exception-first operations queue | UNDERSERVED | Target | Orders needing human action are grouped by owner, reason, impact, next action, and aging rather than buried in status lists. |
| OR-UND-006 | Visible handoff ownership | UNDERSERVED | Target | Every order segment shows which product/team owns the next action and why OrdArr cannot complete it. |
| OR-UND-007 | Configurable orchestration without custom code | UNDERSERVED | Target | Tenant admins can compose approved order types, validation, handoff templates, approvals, and status mappings with tests/versioning. |
| OR-UND-008 | Low-cost EDI/API/portal intake | UNDERSERVED | Target | Support partners and customers at their technical maturity without high transaction fees or duplicate entry. |
| OR-UND-009 | Partial fulfillment decision support | UNDERSERVED | Target | Compare wait, split, substitute, source elsewhere, expedite, pickup, or cancel with cost/service/customer impact. |
| OR-UND-010 | Complete audit trail across corrections | UNDERSERVED | Target | Never silently rewrite the order; show requested, accepted, changed, executed, canceled, returned, and financially prepared versions. |
| OR-UND-011 | Customer promise protection during disruption | UNDERSERVED | Target | Propagate warehouse, supplier, route, asset, quality, and compliance risk early and coordinate customer choices. |
| OR-UND-012 | Order template and repeat-order usability | UNDERSERVED | Target | Save customer/location/requirement-aware templates without copying stale terms or invalid references. |

### D. Advanced capabilities to democratize

These are commonly found only in enterprise tiers, specialist products, or expensive implementations. STL should make them practical without weakening governance.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| OR-DEM-001 | Distributed order management | DEMOCRATIZE | Target | Orchestrate multiple sites, warehouses, suppliers, service teams, carriers, and channels using cost/service/capacity/risk rules. |
| OR-DEM-002 | Intelligent order promising | DEMOCRATIZE | Target | Scenario-based ATP/CTP with confidence, constraints, what-if choices, and human-controlled overrides. |
| OR-DEM-003 | Dynamic fulfillment optimization | DEMOCRATIZE | Target | Re-source/replan open orders as inventory, supplier, capacity, route, quality, or customer conditions change. |
| OR-DEM-004 | Fraud and anomaly review | DEMOCRATIZE | Target | Risk signals, duplicate/address/payment/order-pattern checks, explainable review, and appeal without opaque auto-rejection. |
| OR-DEM-005 | Subscription and recurring order management | DEMOCRATIZE | Target | Schedules, quantity/date adjustments, pauses, skip, renewal, agreement/contract refs, and exception coordination. |
| OR-DEM-006 | Marketplace and partner order hub | DEMOCRATIZE | Target | Normalize orders/status across channels/partners with mapping, acknowledgements, rate limits, and reconciliation. |
| OR-DEM-007 | Order control tower | DEMOCRATIZE | Target | Cross-product backlog, promise risk, handoff health, customer impact, financial readiness, and response actions. |
| OR-DEM-008 | Automated document and compliance packet generation | DEMOCRATIZE | Target | Create order-specific terms, labels, manifests, certificates, proof, and completion packages from source records. |
| OR-DEM-009 | Mass order change simulation | DEMOCRATIZE | Target | Preview weather, supplier, recall, shutdown, capacity, or policy changes across open orders before applying actions. |
| OR-DEM-010 | Customer-specific orchestration policies | DEMOCRATIZE | Target | Honor negotiated service, split/partial, substitution, carrier, packaging, proof, appointment, and communication requirements at scale. |

### E. Suite-wide foundation required in OrdArr

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| OR-FND-001 | Tenant-scoped authorization | FOUNDATION | Target | Every read and write is tenant-scoped server-side; product UI visibility never substitutes for authorization. |
| OR-FND-002 | StaffArr-backed action permissions | FOUNDATION | Target | Use product action keys, role scopes, delegated authority, and explicit denial reasons; ownership does not prevent permissioned delegated workflows. |
| OR-FND-003 | Unified suite shell | FOUNDATION | Target | Consistent navigation, page anatomy, terminology, responsive behavior, keyboard access, and light/dark contrast across products. |
| OR-FND-004 | Record lifecycle and history | FOUNDATION | Target | Human-readable statuses, timeline events, actor/time/source attribution, undo or correction paths, and immutable audit evidence. |
| OR-FND-005 | Quick create for missing references | FOUNDATION · UNDERSERVED | Target | Create the minimum valid cross-product reference without abandoning the current task; preserve context and allow later backfill. |
| OR-FND-006 | Saved views and personal preferences | COMMON | Target | Per-user filters, columns, density, sort, board/list preference, default landing view, and product-scoped preferences. |
| OR-FND-007 | Bulk operations with preview | COMMON | Target | Selection-aware actions, dry-run/impact preview, partial failure handling, downloadable results, and permission checks per record. |
| OR-FND-008 | Import with mapping and review | COMMON · UNDERSERVED | Target | Product-specific import separate from Smart Import, with templates, mappings, validation, dedupe, dry run, commit plan, and rollback/correction evidence. |
| OR-FND-009 | Export and portability | COMMON · DEMOCRATIZE | Target | CSV/PDF/JSON or package export appropriate to the domain, without trapping customer data behind enterprise tiers. |
| OR-FND-010 | Notifications, tasks, and inbox | COMMON | Target | Actionable notifications with ownership, due dates, escalation, snooze/delegation, deep links, and duplicate suppression. |
| OR-FND-011 | API, webhooks, and event outbox | COMMON · DEMOCRATIZE | Target | Stable versioned APIs, idempotency, service authentication, signed webhooks/events, retries, dead-letter handling, and correlation IDs. |
| OR-FND-012 | RecordArr evidence references | FOUNDATION | Target | Files and evidence are stored once in RecordArr and referenced by owning records; products do not create shadow document vaults. |
| OR-FND-013 | ReportArr projections and metrics | FOUNDATION | Target | Operational products emit facts/events; ReportArr builds read models and reports without becoming the source of operational truth. |
| OR-FND-014 | Compliance Core applicability and gates | FOUNDATION | Target | Products request explainable requirements, missing facts, evidence needs, and gate decisions rather than embedding independent legal logic. |
| OR-FND-015 | Field Companion execution surface | FOUNDATION · UNDERSERVED | Target | Mobile-first task, scan, capture, acknowledgement, and offline execution against the owning product API. |
| OR-FND-016 | Professional print and report view | COMMON | Target | Print output is a purpose-built report document without the app shell, not a screenshot of the current page. |
| OR-FND-017 | Accessible error and degraded states | FOUNDATION | Target | Explain what failed, what remains saved, whether retry is safe, what the user can do, and which administrator or integration owns the issue. |
| OR-FND-018 | Time, locale, and units | FOUNDATION | Target | Store canonical timestamps and units; display tenant/user locale, timezone, currency, and measurement preferences consistently. |
| OR-FND-019 | Retention, privacy, and legal hold awareness | FOUNDATION | Target | Product deletion/archive actions respect RecordArr retention, legal holds, privacy policy, and downstream references. |
| OR-FND-020 | AI-assisted proposals, never silent commits | UNDERSERVED | Target | AI may classify, summarize, suggest, or draft; consequential changes remain reviewable, permissioned, attributable, and reversible. |

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
<summary>Endpoint source families (2)</summary>

| Endpoint source file | Discovered route declarations |
| --- | --- |
| WorkspaceEndpoints.cs | 22 |
| AuthEndpoints.cs | 6 |

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

Replace the in-memory store with durable OMS entities, APIs, outbox, inventory promise/allocation integration, returns, and exception recovery.

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
