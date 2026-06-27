# ReportArr — BI Full Feature Set

[Workflow catalog](./WORKFLOWS.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md) · [Rollout](./ROLLOUT.md)

## Document control

| Field | Value |
| --- | --- |
| Product | ReportArr (BI) |
| Category | Business Intelligence and Reporting |
| Repository maturity | Scaffold |
| Audit basis | Static repository inspection on 2026-06-24 |
| Cataloged capabilities | 68 |
| Cataloged workflows | 15 |

## Product charter

ReportArr is the suite reporting and BI plane. It consumes product events/APIs and approved external sources to build governed read models, datasets, metrics, dashboards, operational reports, schedules, alerts, and audit/management packages. It never becomes the source of operational truth and does not write directly into product databases. Every displayed number must have a definition, source, refresh time, lineage, and access policy.

> **Implementation reality — Scaffold:** ReportArr has a broad in-memory domain and frontend/API contract for source connectors, ingestion cursor/events, datasets/fields, read models/lineage, dashboards/access/filters/drilldowns/widgets/visual settings, report definitions/access/parameters/sections/runs/schedules/recipients/exports, metrics/values/snapshots/trends, exceptions, KPIs/alerts, audit scopes/packages, and refresh jobs. The EF context is not a durable analytical store. Production requires persisted semantic definitions, event/read-model ingestion, row/column security, refresh orchestration, provenance, export storage, and scale/testing.

## Source-of-truth boundary

### ReportArr owns

- Source connector configuration, ingestion cursor/status, normalized analytical events, read-model build state, and data-lineage metadata.
- Datasets, fields, semantic measures/metrics, dimensions, filters, calculations, freshness, certification, and access policy.
- Dashboards, pages/widgets, visual settings, drilldown paths, saved views, subscriptions, and sharing/export policy.
- Operational/paginated report definitions, parameters, sections, runs, schedules, recipients, exports, delivery results, and print-ready rendering.
- KPI definitions, values, snapshots, trends, thresholds, exceptions, alerts, acknowledgements, and response links.
- Cross-product audit scopes, reporting packages, refresh/rebuild jobs, provenance, and BI administration.

### ReportArr does not own

- Operational records, approvals, actions, or corrections; users drill into and act through the owning product.
- Compliance evaluation; Compliance Core supplies findings/requirements while ReportArr visualizes them.
- File/document storage; RecordArr owns report exports/packages when retained as records.
- Authentication/role assignment; NexArr/StaffArr supply identity and permissions while ReportArr enforces dataset/report access.
- Unreviewed AI-generated metric definitions or hidden calculations.

## Suite access and authorization

- All normal tenant users may launch every product application in the suite; product actions and records remain permission-scoped server-side.
- Fixed-suite access is nonvariable. Deployment health, feature readiness, legal restrictions, external-provider setup, and permissions may still constrain a capability.
- Platform administration remains a NexArr-only responsibility and is not a tenant product surface.
- Compliance Core administrative/authoring UI is restricted, while Compliance Core runtime evaluations, fact requirements, evidence needs, and workflow gates serve every tenant and product.

## Primary users

- Executive/manager viewer
- Operational analyst
- Report/dashboard author
- Data modeler
- Metric owner
- BI administrator/operator
- Auditor
- External customer/supplier/partner viewer
- Integration administrator

## Required integrations

- All products
- NexArr
- StaffArr
- RecordArr
- Compliance Core
- External databases/APIs/files/BI embeds
- Notification/delivery providers

## Product principles

- ReportArr never becomes the operational source of truth and never repairs data by directly editing source-derived rows.
- The current in-memory implementation is a prototype; durable models, ingestion, security, lineage, and reproducible runs are release blockers.
- Every metric and report displays definition, source, refresh/as-of time, filters, owner, and data-quality state.
- Professional printable reports are purpose-built layouts, not app-shell screenshots.

## Repository implementation snapshot

| Static indicator | Count |
| --- | --- |
| Persistent DbSet declarations | 0 |
| Discovered server classes | 66 |
| Discovered HTTP route declarations | 95 |
| Frontend source files | 10 |
| Frontend page files | 1 |
| Documentation headings | 113 |

### Evidence used for the current-state classification

- reportarr-api contains an in-memory ReportArrStore for source connectors, ingestion events/cursors, datasets/fields, read models/lineage, dashboards/widgets/access/filter/drilldown, reports/runs/schedules/recipients/exports, metrics/KPIs/trends/exceptions/alerts, audit scopes/packages, and refresh jobs.
- The ReportArr EF context lacks a durable analytical model; in-memory state cannot provide production refresh, security, lineage, or reproducibility.
- reportarr-frontend routes include datasets, read models, refresh jobs, dashboards, report builder, schedules, exports, runs, KPIs, metrics, alerts, audit, integrations, source connectors, history, ingestion/status, and settings.
- Canonical docs define datasets/read models, dashboards/widgets, report runs/schedules, KPI/metrics, and workflow/API contracts.

> Counts are static-discovery indicators, not proof that every route or screen is complete, reachable, secure, migrated, or production-ready.

## Mandatory migration or refactor work

- Replace the process-local ReportArrStore with durable source connectors, raw/staging/read models, semantic definitions, runs/exports/schedules, lineage, security, and replay/rebuild.

## Feature catalog

### A. Currently implemented or meaningfully represented

These capabilities have repository evidence. Their state follows the product-level maturity and may still require hardening or completion.

| Feature ID | Capability | Class | State | Required behavior / evidence |
| --- | --- | --- | --- | --- |
| RP-CUR-001 | Source connector and ingestion prototype | CURRENT | Scaffold | In-memory connectors, cursors, events, status, and frontend routes demonstrate intended ingestion administration. |
| RP-CUR-002 | Dataset and field prototype | CURRENT | Scaffold | Dataset/field definitions support reportable domain models. |
| RP-CUR-003 | Read model and lineage prototype | CURRENT | Scaffold | Read-model and lineage concepts are represented. |
| RP-CUR-004 | Dashboard and widget prototype | CURRENT | Scaffold | Dashboards, filters, drilldown, widgets, visual settings, and access are modeled. |
| RP-CUR-005 | Report definition and builder prototype | CURRENT | Scaffold | Reports, parameters, sections, access, and builder routes are represented. |
| RP-CUR-006 | Report run and export prototype | CURRENT | Scaffold | Runs, export artifacts/status, and history concepts are present. |
| RP-CUR-007 | Schedule and recipient prototype | CURRENT | Scaffold | Schedules, recipients, and delivery concepts are modeled. |
| RP-CUR-008 | Metric, KPI, snapshot, and trend prototype | CURRENT | Scaffold | Metric values, snapshots, trends, and KPI definitions support performance views. |
| RP-CUR-009 | Alert and exception prototype | CURRENT | Scaffold | Threshold/exception alert concepts and frontend routes are represented. |
| RP-CUR-010 | Audit scope/package prototype | CURRENT | Scaffold | Cross-product report/audit package concepts are present. |
| RP-CUR-011 | Refresh/rebuild job prototype | CURRENT | Scaffold | Job/status routes show intended analytical maintenance. |
| RP-CUR-012 | Comprehensive BI navigation scaffold | CURRENT | Scaffold | Frontend covers expected analyst, manager, and administrator tasks. |

### B. Common category baseline

These are expected for a credible Business Intelligence and Reporting product. Where they overlap current behavior, the target definition is the acceptance standard rather than a duplicate feature.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| RP-COM-001 | Durable source ingestion and read models | COMMON | Target | Event/API/connector ingestion with cursors, idempotency, schema contracts, replay, quarantine, backfill, and rebuild. |
| RP-COM-002 | Semantic dataset layer | COMMON | Target | Governed dimensions, measures, calculations, time intelligence, units/currency, joins, grain, descriptions, ownership, and certification. |
| RP-COM-003 | Dashboards and interactive analysis | COMMON | Target | Responsive widgets, filters, cross-filter, drilldown/drillthrough, details, saved views, bookmarks, annotations, and sharing. |
| RP-COM-004 | Operational and paginated reports | COMMON | Target | Parameter-driven, print/PDF/Excel/CSV-ready layouts with headers, footers, grouping, totals, page breaks, and accessibility. |
| RP-COM-005 | Scheduling and subscriptions | COMMON | Target | Time/event-driven runs, recipient lists, tenant/user timezone, conditional delivery, retry, secure links/attachments, and history. |
| RP-COM-006 | Row- and column-level security | COMMON | Target | Dataset policies by tenant, role, org/location, ownership, customer/supplier scope, and sensitive field classification. |
| RP-COM-007 | KPIs and metrics | COMMON | Target | Definition, target, owner, grain, frequency, source, formula, thresholds, trend, commentary, and action links. |
| RP-COM-008 | Alerts and anomaly detection | COMMON | Target | Threshold, change, missing data, SLA, risk, anomaly, suppression, dedupe, acknowledgement, escalation, and resolution. |
| RP-COM-009 | Data lineage and provenance | COMMON | Target | Source product/event/field, transformation, refresh time, model version, data quality, and drill-to-source. |
| RP-COM-010 | Exports and embedding | COMMON | Target | CSV/XLSX/PDF/image/data API, secure external sharing, embedded dashboards/reports, and usage auditing. |
| RP-COM-011 | Data quality and observability | COMMON | Target | Freshness, completeness, duplicates, schema drift, failed joins, reconciliation, outliers, and owner workflows. |
| RP-COM-012 | Performance and scale | COMMON | Target | Incremental refresh, partitions, caching, query limits, asynchronous exports, concurrency, and cost/resource visibility. |
| RP-COM-013 | Accessibility and localization | COMMON | Target | Keyboard/screen reader, text alternatives, contrast, table view, localized dates/numbers/currency, timezone, and printable output. |
| RP-COM-014 | BI administration and lifecycle | COMMON | Target | Dev/test/publish, versions, dependencies, impact analysis, access review, usage, deprecation, backup/export, and audit. |

### C. Commonly requested but widely underserved

These address recurring user friction, fragmented add-ons, poor transparency, inaccessible setup, or weak SMB usability.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| RP-UND-001 | Affordable embedded BI with real row/column security | UNDERSERVED | Target | Non-enterprise organizations can safely expose customer, supplier, manager, site, or external portal analytics without buying a separate premium platform. |
| RP-UND-002 | One governed metric definition | UNDERSERVED | Target | Every KPI explains formula, grain, inclusions/exclusions, owner, source, refresh, and changes so teams stop arguing over spreadsheets. |
| RP-UND-003 | Cross-product lineage to action | UNDERSERVED | Target | Click from a chart to the exact owning record and permitted action, not a copied warehouse row with no operational context. |
| RP-UND-004 | Print-ready operational reports | UNDERSERVED | Target | Generate professional report documents with clear tables, pagination, signatures/filters/as-of time, not browser screenshots. |
| RP-UND-005 | Small-company connector pack | UNDERSERVED | Target | Practical connectors and mapping for common accounting, payroll, banking, CRM, forms, spreadsheets, and email sources with transparent limits. |
| RP-UND-006 | Natural-language analysis with citations | UNDERSERVED | Target | Questions produce governed metric queries, source/definition citations, filters, and uncertainty; no invented numbers or silent model changes. |
| RP-UND-007 | Actionable alert fatigue controls | UNDERSERVED | Target | Owners, quiet hours, dedupe, hysteresis, escalation, suppression, maintenance windows, and closure stop dashboards from becoming noise. |
| RP-UND-008 | Snapshot and “as-of” reporting | UNDERSERVED | Target | Reproduce what a report showed at a prior close/audit date, including model/filters/source versions and late-arriving corrections. |
| RP-UND-009 | Data quality visible to end users | UNDERSERVED | Target | Clearly mark stale, partial, estimated, backfilled, or source-unavailable data and explain whether decisions are safe. |
| RP-UND-010 | User-created reports without security bypass | UNDERSERVED | Target | Business users can compose approved fields/measures while inherited dataset policies, sensitivity, and export restrictions remain enforced. |
| RP-UND-011 | External portal analytics | UNDERSERVED | Target | Customers/suppliers/auditors see only their scoped data with readable metrics and downloadable evidence, without full BI licenses. |
| RP-UND-012 | Writeback through owned actions | UNDERSERVED | Target | Dashboards may launch tasks/comments/acknowledgements or product actions through APIs; ReportArr never edits analytical rows as a substitute for source workflow. |

### D. Advanced capabilities to democratize

These are commonly found only in enterprise tiers, specialist products, or expensive implementations. STL should make them practical without weakening governance.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| RP-DEM-001 | Enterprise semantic layer and metric store | DEMOCRATIZE | Target | Central governed measures, reusable models, dimensions, time logic, ownership, certification, versioning, and APIs. |
| RP-DEM-002 | Data lakehouse/warehouse orchestration | DEMOCRATIZE | Target | Incremental ingestion, historical models, slowly changing dimensions, event facts, partitions, retention, and reproducible rebuilds. |
| RP-DEM-003 | Advanced anomaly and forecasting | DEMOCRATIZE | Target | Seasonality, trend, uncertainty, drivers, change points, and business context with explainable alerts and human validation. |
| RP-DEM-004 | Embedded analytics SDK | DEMOCRATIZE | Target | Secure tenant/user context, themes, filters, exports, events, and action callbacks for all product/portal surfaces. |
| RP-DEM-005 | What-if and scenario analysis | DEMOCRATIZE | Target | Parameter-driven simulations for workforce, maintenance, supply, transport, inventory, orders, quality, compliance, and finance without changing live records. |
| RP-DEM-006 | Narrative reporting and board packs | DEMOCRATIZE | Target | Generate cited commentary, variance explanations, charts, statements, risk/actions, and professional recurring packages with review. |
| RP-DEM-007 | Data catalog and impact graph | DEMOCRATIZE | Target | Search datasets, fields, metrics, reports, owners, usage, lineage, sensitivity, dependencies, and change impact. |
| RP-DEM-008 | Privacy-preserving analytics | DEMOCRATIZE | Target | Aggregation thresholds, masking, purpose policies, differential/privacy techniques where appropriate, and safe people/customer reporting. |
| RP-DEM-009 | Streaming and operational intelligence | DEMOCRATIZE | Target | Near-real-time event models, windows, stateful alerts, control-tower views, and replay for operational response. |
| RP-DEM-010 | Model governance and testing | DEMOCRATIZE | Target | Unit/data/contract/security/performance tests, golden reports, reconciliation, approval, rollback, and usage-based deprecation for BI assets. |

### E. Suite-wide foundation required in ReportArr

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| RP-FND-001 | Tenant-scoped authorization | FOUNDATION | Target | Every read and write is tenant-scoped server-side; product UI visibility never substitutes for authorization. |
| RP-FND-002 | StaffArr-backed action permissions | FOUNDATION | Target | Use product action keys, role scopes, delegated authority, and explicit denial reasons; ownership does not prevent permissioned delegated workflows. |
| RP-FND-003 | Unified suite shell | FOUNDATION | Target | Consistent navigation, page anatomy, terminology, responsive behavior, keyboard access, and light/dark contrast across products. |
| RP-FND-004 | Record lifecycle and history | FOUNDATION | Target | Human-readable statuses, timeline events, actor/time/source attribution, undo or correction paths, and immutable audit evidence. |
| RP-FND-005 | Quick create for missing references | FOUNDATION · UNDERSERVED | Target | Create the minimum valid cross-product reference without abandoning the current task; preserve context and allow later backfill. |
| RP-FND-006 | Saved views and personal preferences | COMMON | Target | Per-user filters, columns, density, sort, board/list preference, default landing view, and product-scoped preferences. |
| RP-FND-007 | Bulk operations with preview | COMMON | Target | Selection-aware actions, dry-run/impact preview, partial failure handling, downloadable results, and permission checks per record. |
| RP-FND-008 | Import with mapping and review | COMMON · UNDERSERVED | Target | Product-specific import separate from Smart Import, with templates, mappings, validation, dedupe, dry run, commit plan, and rollback/correction evidence. |
| RP-FND-009 | Export and portability | COMMON · DEMOCRATIZE | Target | CSV/PDF/JSON or package export appropriate to the domain, without trapping customer data behind enterprise tiers. |
| RP-FND-010 | Notifications, tasks, and inbox | COMMON | Target | Actionable notifications with ownership, due dates, escalation, snooze/delegation, deep links, and duplicate suppression. |
| RP-FND-011 | API, webhooks, and event outbox | COMMON · DEMOCRATIZE | Target | Stable versioned APIs, idempotency, service authentication, signed webhooks/events, retries, dead-letter handling, and correlation IDs. |
| RP-FND-012 | RecordArr evidence references | FOUNDATION | Target | Files and evidence are stored once in RecordArr and referenced by owning records; products do not create shadow document vaults. |
| RP-FND-013 | ReportArr projections and metrics | FOUNDATION | Target | Operational products emit facts/events; ReportArr builds read models and reports without becoming the source of operational truth. |
| RP-FND-014 | Compliance Core applicability and gates | FOUNDATION | Target | Products request explainable requirements, missing facts, evidence needs, and gate decisions rather than embedding independent legal logic. |
| RP-FND-015 | Field Companion execution surface | FOUNDATION · UNDERSERVED | Target | Mobile-first task, scan, capture, acknowledgement, and offline execution against the owning product API. |
| RP-FND-016 | Professional print and report view | COMMON | Target | Print output is a purpose-built report document without the app shell, not a screenshot of the current page. |
| RP-FND-017 | Accessible error and degraded states | FOUNDATION | Target | Explain what failed, what remains saved, whether retry is safe, what the user can do, and which administrator or integration owns the issue. |
| RP-FND-018 | Time, locale, and units | FOUNDATION | Target | Store canonical timestamps and units; display tenant/user locale, timezone, currency, and measurement preferences consistently. |
| RP-FND-019 | Retention, privacy, and legal hold awareness | FOUNDATION | Target | Product deletion/archive actions respect RecordArr retention, legal holds, privacy policy, and downstream references. |
| RP-FND-020 | AI-assisted proposals, never silent commits | UNDERSERVED | Target | AI may classify, summarize, suggest, or draft; consequential changes remain reviewable, permissioned, attributable, and reversible. |

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
<summary>Endpoint source families (3)</summary>

| Endpoint source file | Discovered route declarations |
| --- | --- |
| IntegrationEndpoints.cs | 59 |
| WorkspaceEndpoints.cs | 28 |
| AuthEndpoints.cs | 8 |

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

Replace the in-memory store with durable ingestion/read models, semantic metrics, lineage, row/column security, reproducible runs, and professional reports.

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
