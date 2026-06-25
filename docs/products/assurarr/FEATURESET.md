# AssurArr — QMS Full Feature Set

[Workflow catalog](./WORKFLOWS.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md)

## Document control

| Field | Value |
| --- | --- |
| Product | AssurArr (QMS) |
| Category | Quality Management System |
| Repository maturity | Durable |
| Audit basis | Static repository inspection on 2026-06-24 |
| Cataloged capabilities | 68 |
| Cataloged workflows | 14 |

## Product charter

AssurArr owns quality meaning and decisions: nonconformance, quality hold/release, containment, disposition, root cause, CAPA, corrective actions, verification/effectiveness, quality audits/findings, quality reviews, supplier quality issues/SCAR, customer complaint quality cases, quality risk and scorecards. It blocks or releases product-owned execution through explicit events but does not take ownership of inventory, assets, orders, suppliers, customers, documents, or training.

> **Implementation reality — Durable:** AssurArr has persistent nonconformance, quality hold, CAPA, actions/blockers, verification plans, effectiveness verification, audits/checklists/items/findings, root cause analyses, status snapshots, scorecards/metrics/risk profiles, reviews/releases, containment, disposition, supplier quality/SCAR, customer complaints, and timeline events. The major targets are formal change/deviation control, risk/FMEA/SPC/calibration integration, richer controlled-document/training impact through RecordArr/TrainArr, and polished mobile/external collaboration.

## Source-of-truth boundary

### AssurArr owns

- Quality nonconformance, affected-object scope, severity, classification, investigation, root cause, and quality timeline.
- Quality holds/releases, containment, disposition, release authority, residual conditions, and downstream unblock events.
- CAPA, actions, blockers, verification plans, effectiveness verification, recurrence, and closure/reopen decisions.
- Quality audits, checklists/items, findings, responses, corrective actions, review, and closure.
- Supplier quality issues and SCAR technical workflow; SupplyArr owns supplier commercial status and procurement consequences.
- Customer complaint quality case and investigation; CustomArr owns customer communication/case relationship.
- Quality status snapshots, scorecards, metrics, risk profiles, reviews, and quality release decisions.

### AssurArr does not own

- Physical inventory balance/movement; LoadArr enforces quality status/hold on inventory.
- Asset readiness/work order; MaintainArr enforces asset quality blockers and executes corrective maintenance.
- Supplier/customer/order/transport truth; SupplyArr, CustomArr, OrdArr, and RoutArr own those records.
- Controlled-document storage/version or training assignment; RecordArr and TrainArr own them, with quality-impact handoffs.
- Regulatory interpretation/applicability; Compliance Core owns it.

## Suite access and authorization

- All normal tenant users may launch every product application in the suite; product actions and records remain permission-scoped server-side.
- Fixed-suite access is nonvariable. Deployment health, feature readiness, legal restrictions, external-provider setup, and permissions may still constrain a capability.
- Platform administration remains a NexArr-only responsibility and is not a tenant product surface.
- Compliance Core administrative/authoring UI is restricted, while Compliance Core runtime evaluations, fact requirements, evidence needs, and workflow gates serve every tenant and product.

## Primary users

- Quality manager
- Quality engineer/investigator
- Auditor
- CAPA/action owner
- Operations/process owner
- Supplier quality engineer
- Customer complaint reviewer
- Quality approver/releaser
- External supplier/customer reviewer

## Required integrations

- LoadArr
- MaintainArr
- SupplyArr
- CustomArr
- OrdArr
- RoutArr
- StaffArr
- TrainArr
- RecordArr
- Compliance Core
- ReportArr
- Field Companion
- NexArr

## Product principles

- AssurArr owns quality decisions; affected products own and enforce their local operational blockers and movements.
- A hold/release is versioned, scoped, acknowledged, and never inferred from a closed nonconformance alone.
- CAPA closure requires implementation verification and effectiveness evidence, not merely completed tasks.
- Documents and training impacts are handled through RecordArr and TrainArr rather than duplicate QMS submodules.

## Repository implementation snapshot

| Static indicator | Count |
| --- | --- |
| Persistent DbSet declarations | 24 |
| Discovered server classes | 89 |
| Discovered HTTP route declarations | 115 |
| Frontend source files | 9 |
| Frontend page files | 1 |
| Documentation headings | 80 |

### Evidence used for the current-state classification

- Persistent Nonconformances, QualityHolds, Capas, CapaActions, CapaActionBlockers, VerificationPlans, EffectivenessVerifications, RootCauseAnalyses, ContainmentActions, Dispositions, QualityReleases, and TimelineEvents.
- Persistent QualityAudits, QualityAuditChecklists, QualityAuditChecklistItems, AuditFindings, QualityReviews, QualityStatusSnapshots, QualityScorecards, QualityMetrics, and QualityRiskProfiles.
- Persistent SupplierQualityIssues, SupplierCorrectiveActionRequests, and CustomerComplaintQualityCases.
- assurarr-frontend routes for nonconformances, holds, CAPA/actions/blockers, verification/effectiveness, audits/checklists/findings, reviews/releases, containment/disposition, supplier quality/SCAR, complaints, status/risk/scorecards/history/settings.
- Cross-product workflow pack defines quality hold/release behavior for LoadArr, OrdArr, MaintainArr, RoutArr, CustomArr, SupplyArr, and RecordArr.

> Counts are static-discovery indicators, not proof that every route or screen is complete, reachable, secure, migrated, or production-ready.

## Feature catalog

### A. Currently implemented or meaningfully represented

These capabilities have repository evidence. Their state follows the product-level maturity and may still require hardening or completion.

| Feature ID | Capability | Class | State | Required behavior / evidence |
| --- | --- | --- | --- | --- |
| AS-CUR-001 | Nonconformance management | CURRENT | Durable | Durable nonconformance records with affected context, status, severity, and timeline. |
| AS-CUR-002 | Quality hold and release | CURRENT | Durable | Hold scope, lifecycle, release decision, and downstream blocking model are persistent. |
| AS-CUR-003 | Containment and disposition | CURRENT | Durable | Immediate containment and final disposition records are represented. |
| AS-CUR-004 | Root cause analysis | CURRENT | Durable | Structured root-cause records support investigation and CAPA context. |
| AS-CUR-005 | CAPA and action tracking | CURRENT | Durable | CAPA, actions, blockers, owners, due dates, status, and closure are durable. |
| AS-CUR-006 | Verification plans and effectiveness checks | CURRENT | Durable | Verification and effectiveness records allow evidence-based closure/reopen. |
| AS-CUR-007 | Quality audits and checklists | CURRENT | Durable | Audits, checklists/items, findings, and related workflows are persistent. |
| AS-CUR-008 | Quality findings and review | CURRENT | Durable | Findings, reviews, release decisions, and quality status changes are represented. |
| AS-CUR-009 | Supplier quality and SCAR | CURRENT | Durable | Supplier issue and corrective-action request models support external quality collaboration. |
| AS-CUR-010 | Customer complaint quality case | CURRENT | Durable | Quality-side complaint cases can link to CustomArr customer communication. |
| AS-CUR-011 | Quality metrics, scorecards, snapshots, and risk profiles | CURRENT | Durable | Performance and risk models support management review and trend analysis. |
| AS-CUR-012 | Timeline/history UI | CURRENT | Durable | Cross-record history and dedicated route support explainability. |

### B. Common category baseline

These are expected for a credible Quality Management System product. Where they overlap current behavior, the target definition is the acceptance standard rather than a duplicate feature.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| AS-COM-001 | Nonconformance and deviation | COMMON | Target | Detection, classification, affected scope, containment, investigation, disposition, approvals, and closure. |
| AS-COM-002 | Quality hold and release | COMMON | Target | Physical/system block, labels/status, scope expansion, review, release/reject, downstream acknowledgement, and residual conditions. |
| AS-COM-003 | CAPA | COMMON | Target | Problem statement, root cause, corrections, corrective/preventive actions, owners, due dates, change/training/document impact, verification, effectiveness, and closure. |
| AS-COM-004 | Audit management | COMMON | Target | Program/schedule, plan, checklist, evidence, findings, responses, actions, follow-up, closure, and audit package. |
| AS-COM-005 | Change control | COMMON | Target | Proposed change, risk/impact, affected products/processes/docs/training/validation, approvals, implementation, verification, and effective date. |
| AS-COM-006 | Document and training integration | COMMON | Target | Controlled procedures/specs/forms from RecordArr and impacted assignments/qualifications through TrainArr. |
| AS-COM-007 | Supplier quality | COMMON | Target | Incoming quality, scorecard, supplier issue, SCAR, containment, source change, qualification, and improvement plan. |
| AS-COM-008 | Customer complaints | COMMON | Target | Intake, severity, reportability screening, investigation, affected product/service, response, trend, CAPA, and closure. |
| AS-COM-009 | Risk management | COMMON | Target | Quality risk register, FMEA or equivalent analyses, controls, residual risk, acceptance, monitoring, and linkage to changes/CAPA. |
| AS-COM-010 | Inspection and test result integration | COMMON | Target | Receive inspection/test/measurement facts from operational systems and preserve method/equipment/evidence refs. |
| AS-COM-011 | Calibration and equipment-quality context | COMMON | Target | Calibration status/records through MaintainArr or specialized integration, out-of-tolerance impact, and affected-result review. |
| AS-COM-012 | Statistical quality and trend | COMMON | Target | Defect/nonconformance rates, control charts, Pareto, capability, sampling, recurrence, and alerting with governed definitions. |
| AS-COM-013 | Management review | COMMON | Target | Objectives, metrics, complaints, audit, CAPA, supplier, risk, resources, actions, and signed review package. |
| AS-COM-014 | Electronic signatures and audit trail | COMMON | Target | Identity, meaning, intent, timestamp, record/version, reason, and non-repudiation appropriate to configured regulation. |

### C. Commonly requested but widely underserved

These address recurring user friction, fragmented add-ons, poor transparency, inaccessible setup, or weak SMB usability.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| AS-UND-001 | One connected quality story | UNDERSERVED | Target | Nonconformance, hold, complaint, supplier, audit, CAPA, change, documents, training, inventory, maintenance, orders, and evidence link without duplicate modules. |
| AS-UND-002 | Frontline mobile quality capture | UNDERSERVED | Target | A worker can report, photograph, scan lot/asset/order, contain, and receive next steps in seconds, including offline queue. |
| AS-UND-003 | Explainable hold propagation | UNDERSERVED | Target | Show exactly which inventory, orders, assets, trips, suppliers, customers, and tasks are blocked and whether each product acknowledged it. |
| AS-UND-004 | Small-team guided QMS | UNDERSERVED | Target | Progressive workflow and templates provide rigor without requiring a full-time quality systems administrator. |
| AS-UND-005 | Supplier/customer collaboration without paid seats | UNDERSERVED | Target | Scoped portals collect containment, evidence, responses, approvals, and acknowledgements without exposing internal data. |
| AS-UND-006 | Quality action workload and escalation | UNDERSERVED | Target | One queue prioritizes overdue/high-risk actions, blockers, dependencies, and next steps rather than sending repetitive email. |
| AS-UND-007 | Evidence completeness coaching | UNDERSERVED | Target | Before closure, explain missing tests, approvals, photos, lots/serials, affected scope, training, document, or effectiveness evidence. |
| AS-UND-008 | Cross-product affected-object traceability | UNDERSERVED | Target | Search from a problem to all related units, assets, work, orders, shipments, suppliers, customers, people, and documents with source confidence. |
| AS-UND-009 | Root cause assistance that respects uncertainty | UNDERSERVED | Target | Suggest hypotheses and evidence gaps, cite similar events, and distinguish contributing factors from verified root cause. |
| AS-UND-010 | Practical no-code quality workflows | UNDERSERVED | Target | Configure approved types, severity, routing, forms, decisions, approvals, and due dates with versioning and simulation. |
| AS-UND-011 | Quality and compliance separation with integration | UNDERSERVED | Target | Quality decisions remain AssurArr while Compliance Core explains legal/evidence applicability; users see one coherent result. |
| AS-UND-012 | Offline controlled procedures and forms | UNDERSERVED | Target | Workers access the current approved version and queue evidence while the server validates version before accepting completion. |

### D. Advanced capabilities to democratize

These are commonly found only in enterprise tiers, specialist products, or expensive implementations. STL should make them practical without weakening governance.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| AS-DEM-001 | Advanced product/process risk and FMEA | DEMOCRATIZE | Target | Reusable failure modes, effects, causes, controls, scores, actions, change impact, and live operational linkage. |
| AS-DEM-002 | Statistical process control | DEMOCRATIZE | Target | Data collection, control charts, rules, capability, alarms, reaction plans, and evidence connected to actual process/order/item context. |
| AS-DEM-003 | Electronic batch/device/history record review | DEMOCRATIZE | Target | Assemble and review process/production/service evidence, exceptions, signatures, and release without a separate enterprise MES/QMS stack. |
| AS-DEM-004 | Validation and qualification management | DEMOCRATIZE | Target | Protocols, requirements, test scripts, execution evidence, deviations, approvals, traceability, and periodic review. |
| AS-DEM-005 | AI-assisted complaint and nonconformance triage | DEMOCRATIZE | Target | Classify, dedupe, identify affected refs, summarize evidence, and propose severity/routing with human approval and citations. |
| AS-DEM-006 | Computer-vision defect assistance | DEMOCRATIZE | Target | Propose defect type, location, count, severity, and comparison to standards from images while retaining human inspection authority. |
| AS-DEM-007 | Predictive quality and recurrence risk | DEMOCRATIZE | Target | Use supplier, process, lot, asset, environment, operator qualification, inspection, and historical data to prioritize prevention. |
| AS-DEM-008 | Digital quality passport | DEMOCRATIZE | Target | Portable, permissioned package of origin, inspections, certificates, holds/releases, changes, and evidence by product/lot/asset/order. |
| AS-DEM-009 | Regulated e-signature controls | DEMOCRATIZE | Target | Configurable signature meaning, reauthentication, sequence, reason, record locking, audit, and validation evidence suitable for regulated users. |
| AS-DEM-010 | Enterprise supplier quality network for SMB | DEMOCRATIZE | Target | Multi-tier issue collaboration, shared evidence, source-change approval, and cross-supplier trend without enterprise network fees. |

### E. Suite-wide foundation required in AssurArr

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| AS-FND-001 | Tenant-scoped authorization | FOUNDATION | Target | Every read and write is tenant-scoped server-side; product UI visibility never substitutes for authorization. |
| AS-FND-002 | StaffArr-backed action permissions | FOUNDATION | Target | Use product action keys, role scopes, delegated authority, and explicit denial reasons; ownership does not prevent permissioned delegated workflows. |
| AS-FND-003 | Unified suite shell | FOUNDATION | Target | Consistent navigation, page anatomy, terminology, responsive behavior, keyboard access, and light/dark contrast across products. |
| AS-FND-004 | Record lifecycle and history | FOUNDATION | Target | Human-readable statuses, timeline events, actor/time/source attribution, undo or correction paths, and immutable audit evidence. |
| AS-FND-005 | Quick create for missing references | FOUNDATION · UNDERSERVED | Target | Create the minimum valid cross-product reference without abandoning the current task; preserve context and allow later backfill. |
| AS-FND-006 | Saved views and personal preferences | COMMON | Target | Per-user filters, columns, density, sort, board/list preference, default landing view, and product-scoped preferences. |
| AS-FND-007 | Bulk operations with preview | COMMON | Target | Selection-aware actions, dry-run/impact preview, partial failure handling, downloadable results, and permission checks per record. |
| AS-FND-008 | Import with mapping and review | COMMON · UNDERSERVED | Target | Product-specific import separate from Smart Import, with templates, mappings, validation, dedupe, dry run, commit plan, and rollback/correction evidence. |
| AS-FND-009 | Export and portability | COMMON · DEMOCRATIZE | Target | CSV/PDF/JSON or package export appropriate to the domain, without trapping customer data behind enterprise tiers. |
| AS-FND-010 | Notifications, tasks, and inbox | COMMON | Target | Actionable notifications with ownership, due dates, escalation, snooze/delegation, deep links, and duplicate suppression. |
| AS-FND-011 | API, webhooks, and event outbox | COMMON · DEMOCRATIZE | Target | Stable versioned APIs, idempotency, service authentication, signed webhooks/events, retries, dead-letter handling, and correlation IDs. |
| AS-FND-012 | RecordArr evidence references | FOUNDATION | Target | Files and evidence are stored once in RecordArr and referenced by owning records; products do not create shadow document vaults. |
| AS-FND-013 | ReportArr projections and metrics | FOUNDATION | Target | Operational products emit facts/events; ReportArr builds read models and reports without becoming the source of operational truth. |
| AS-FND-014 | Compliance Core applicability and gates | FOUNDATION | Target | Products request explainable requirements, missing facts, evidence needs, and gate decisions rather than embedding independent legal logic. |
| AS-FND-015 | Field Companion execution surface | FOUNDATION · UNDERSERVED | Target | Mobile-first task, scan, capture, acknowledgement, and offline execution against the owning product API. |
| AS-FND-016 | Professional print and report view | COMMON | Target | Print output is a purpose-built report document without the app shell, not a screenshot of the current page. |
| AS-FND-017 | Accessible error and degraded states | FOUNDATION | Target | Explain what failed, what remains saved, whether retry is safe, what the user can do, and which administrator or integration owns the issue. |
| AS-FND-018 | Time, locale, and units | FOUNDATION | Target | Store canonical timestamps and units; display tenant/user locale, timezone, currency, and measurement preferences consistently. |
| AS-FND-019 | Retention, privacy, and legal hold awareness | FOUNDATION | Target | Product deletion/archive actions respect RecordArr retention, legal holds, privacy policy, and downstream references. |
| AS-FND-020 | AI-assisted proposals, never silent commits | UNDERSERVED | Target | AI may classify, summarize, suggest, or draft; consequential changes remain reviewable, permissioned, attributable, and reversible. |

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
<summary>Persistent entity sets (24)</summary>

| Entity type | DbSet/property | Source |
| --- | --- | --- |
| AssurArrNonconformance | Nonconformances | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrQualityHold | QualityHolds | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrCapa | Capas | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrCapaAction | CapaActions | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrCapaActionBlocker | CapaActionBlockers | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrVerificationPlan | VerificationPlans | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrEffectivenessVerification | EffectivenessVerifications | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrQualityAudit | QualityAudits | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrQualityAuditChecklist | QualityAuditChecklists | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrQualityAuditChecklistItem | QualityAuditChecklistItems | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrAuditFinding | AuditFindings | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrRootCauseAnalysis | RootCauseAnalyses | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrQualityStatusSnapshot | QualityStatusSnapshots | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrQualityScorecard | QualityScorecards | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrQualityMetric | QualityMetrics | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrQualityRiskProfile | QualityRiskProfiles | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrQualityReview | QualityReviews | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrQualityRelease | QualityReleases | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrContainmentAction | ContainmentActions | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrDisposition | Dispositions | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrSupplierQualityIssue | SupplierQualityIssues | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrSupplierCorrectiveActionRequest | SupplierCorrectiveActionRequests | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrCustomerComplaintQualityCase | CustomerComplaintQualityCases | AssurArr.Api/Data/AssurArrDbContext.cs |
| AssurArrTimelineEvent | TimelineEvents | AssurArr.Api/Data/AssurArrDbContext.cs |

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
| AssurArrEndpoints.cs | 109 |
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

Complete QMS workflow orchestration, sampling/SPC, supplier/customer quality, controlled signatures, and closed-loop CAPA effectiveness across product handoffs.

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
