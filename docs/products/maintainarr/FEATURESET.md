# MaintainArr — CMMS Full Feature Set

[Workflow catalog](./WORKFLOWS.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md) · [Rollout](./ROLLOUT.md)

## Document control

| Field | Value |
| --- | --- |
| Product | MaintainArr (CMMS) |
| Category | Computerized Maintenance Management System |
| Repository maturity | Durable |
| Audit basis | Static repository inspection on 2026-06-24 |
| Cataloged capabilities | 73 |
| Cataloged workflows | 14 |

## Product charter

MaintainArr owns physical asset identity and configuration in the maintenance domain, maintenance strategy, inspection and defect truth, work execution, downtime, asset readiness, and return-to-service. It requests parts from LoadArr/SupplyArr, people and qualifications from StaffArr/TrainArr, evidence storage from RecordArr, quality decisions from AssurArr, and compliance meaning from Compliance Core.

> **Implementation reality — Durable:** MaintainArr has extensive persistent models for assets, components, meters, PM, inspections, defects, work orders, labor, parts demand, blockers, permits, evidence, closeout, return-to-service, downtime, availability, history, recalls, enrichment, quality holds, readiness checks, kits, vendor work, catalogs, integrations, notifications, audit, and background workers. The primary completion need is turning this breadth into a consistent, low-friction field experience and closing external/provider and cross-product loops. Asset reservation and motor-pool support now has a partial live slice with request/read, conflict detection, and lifecycle actions on the asset profile surface.

## Source-of-truth boundary

### MaintainArr owns

- Asset classes/types, assets, specifications, custom fields, components, installed-component history, external identifiers/mappings, enrichment, and status/location/assignment history.
- Asset meters and readings, maintenance plans/programs/schedules/occurrences, condition and usage triggers, and due-state computation.
- Inspection templates, categories, checklist items, applicable asset types, inspection runs/answers/pause events, and inspection evidence references.
- Defects, defect evidence, severity, readiness impact, escalation, and links to work orders or quality review.
- Work orders, tasks, labor, technician assignments, evidence, parts demand, blockers, comments, timeline, permits, closeout, and return-to-service.
- Asset downtime, availability snapshots/rollups, maintenance history, readiness states/checks, recall campaigns/cases, and vendor maintenance work.
- Maintenance-specific part metadata and kits while physical inventory balance remains LoadArr and procurement/vendor commercial truth remains SupplyArr.

### MaintainArr does not own

- Physical inventory balance, reservation, issue, return, or stock ledger; LoadArr owns those transactions.
- Supplier identity, sourcing, purchase order, pricing, lead time, or vendor performance; SupplyArr owns those truths.
- Person/employment/permission/location identity; StaffArr owns them.
- Qualification issuance; TrainArr owns it, though MaintainArr may require and check qualifications.
- Quality hold/release or CAPA; AssurArr owns quality decisions.
- Route/trip execution; RoutArr consumes readiness and reports operational defects/exceptions.
- Document binaries; RecordArr owns evidence files and packages.

## Suite access and authorization

- All normal tenant users may launch every product application in the suite; product actions and records remain permission-scoped server-side.
- Fixed-suite access is nonvariable. Deployment health, feature readiness, legal restrictions, external-provider setup, and permissions may still constrain a capability.
- Platform administration remains a NexArr-only responsibility and is not a tenant product surface.
- Compliance Core administrative/authoring UI is restricted, while Compliance Core runtime evaluations, fact requirements, evidence needs, and workflow gates serve every tenant and product.

## Primary users

- Asset administrator
- Maintenance manager
- Planner/scheduler
- Technician
- Operator/requester
- Inspector
- Reliability engineer
- Vendor/contractor
- Fleet coordinator
- Auditor

## Required integrations

- StaffArr
- TrainArr
- LoadArr
- SupplyArr
- RoutArr
- AssurArr
- RecordArr
- Compliance Core
- ReportArr
- Field Companion
- Telematics/reference/recall providers

## Product principles

- A work order may close only through explicit closeout; asset return-to-service is a separate decision.
- Inventory and procurement status are consumed through handoffs, never recreated as MaintainArr balances or POs.
- Readiness must be explainable, including stale data, warnings, holds, defects, qualifications, and overrides.
- Field execution favors progressive disclosure, scanning, voice/accessibility, and offline-safe actions over large forms.

## Repository implementation snapshot

| Static indicator | Count |
| --- | --- |
| Persistent DbSet declarations | 98 |
| Discovered server classes | 693 |
| Discovered HTTP route declarations | 377 |
| Frontend source files | 146 |
| Frontend page files | 31 |
| Documentation headings | 87 |

### Evidence used for the current-state classification

- Persistent asset classes/types/assets, specs, custom fields, components, installed components, document refs, compliance states, status/location/assignment history, readiness, external mappings/identifiers, enrichment snapshots/suggestions, and external provider cache/audit.
- Persistent PM schedules/occurrences/programs/program schedules, meters/readings, inspection templates/categories/items/applicability, runs/answers/pause events, defects and evidence.
- Persistent work orders, tasks, labor, technicians, evidence, parts demand/status, blockers, closeouts, permits, return-to-service, comments, timeline, notifications, and audit packages.
- Persistent downtime events, availability snapshots, rollups/history and recurring workers for defect escalation, PM scans, status/history rollups, downtime sync, and platform outbox/inbox.
- Persistent recall campaigns/applicability/cases/audit/aliases, asset quality holds, readiness checks, parts kits/lines, vendor work, catalogs/options/dependencies/fieldsets, imports, and reference caches.
- maintainarr-frontend routes for dashboard/assets/imports/PM/recalls/meters/work orders/scheduling/defects/inspections/templates/parts/kits/history/reports/downtime/settings.

> Counts are static-discovery indicators, not proof that every route or screen is complete, reachable, secure, migrated, or production-ready.

## Feature catalog

### A. Currently implemented or meaningfully represented

These capabilities have repository evidence. Their state follows the product-level maturity and may still require hardening or completion.

| Feature ID | Capability | Class | State | Required behavior / evidence |
| --- | --- | --- | --- | --- |
| MA-CUR-001 | Asset registry and hierarchy | CURRENT | Durable | Classes, types, assets, components, installed-component relationships, specifications, custom fields, identifiers, mappings, and history are durable. |
| MA-CUR-002 | Guided and data-driven asset fields | CURRENT | Durable | Catalog, option dependency, fieldset, pending-value, reference cache, and custom-field models support controlled asset creation rather than hardcoded free text. |
| MA-CUR-003 | Asset status, location, assignment, and readiness history | CURRENT | Durable | History tables and current readiness state support explainable availability decisions. |
| MA-CUR-004 | Meter and usage tracking | CURRENT | Durable | Asset meters/readings can drive maintenance and history. |
| MA-CUR-005 | Preventive maintenance schedules and programs | CURRENT | Durable | Schedules, programs, program schedules, occurrences, due scans, and recurring worker state are persistent. |
| MA-CUR-006 | Inspection template builder | CURRENT | Durable | Templates, categories, checklist items, asset-type applicability, version-like structure, and execution records exist. |
| MA-CUR-007 | Guided inspection execution | CURRENT | Durable | Runs, answers, pause events, evidence, defects, and mobile-oriented flows are represented. |
| MA-CUR-008 | Defect management and escalation | CURRENT | Durable | Defects, evidence, severity/readiness impact, escalation settings/runs/events, and work-order links are persistent. |
| MA-CUR-009 | Work order lifecycle | CURRENT | Durable | Work orders, task lines, technician assignments, labor, comments, evidence, blockers, timeline, closeout, and return-to-service are durable. |
| MA-CUR-010 | Parts demand and status coordination | CURRENT | Durable | Work orders create demand lines and consume status events while LoadArr/SupplyArr remain inventory/procurement owners. |
| MA-CUR-011 | Permits and return-to-service controls | CURRENT | Durable | Permit references, closeout data, readiness checks, and explicit return-to-service records support safe completion. |
| MA-CUR-012 | Downtime and availability analytics source | CURRENT | Durable | Downtime events, asset/fleet availability snapshots, rollups, and sync runs provide operational facts. |
| MA-CUR-013 | Recall campaign and applicability management | CURRENT | Durable | Campaigns, aliases, applicability, asset cases, snapshots, and recall audit are modeled. |
| MA-CUR-014 | Asset enrichment and external provider cache | CURRENT | Durable | Snapshots, suggestions, provider cache, aliases, audit, and external IDs support VIN/catalog/recall-style enrichment. |
| MA-CUR-015 | Maintenance parts, kits, and vendor work | CURRENT | Durable | Maintenance part definitions, kits/lines, vendor maintenance work, and scoped portal access are represented. |
| MA-CUR-016 | Quality hold and compliance mirrors | CURRENT | Durable | Asset quality-hold and regulatory-key mirror models allow cross-product blocking without taking ownership. |
| MA-CUR-017 | Imports, notifications, audit packages, and platform events | CURRENT | Durable | Import batches, notification dispatch, audit generation, outbox/inbox, and worker runs are durable. |

### B. Common category baseline

These are expected for a credible Computerized Maintenance Management System product. Where they overlap current behavior, the target definition is the acceptance standard rather than a duplicate feature.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| MA-COM-001 | Asset hierarchy and lifecycle | COMMON | Target | Parent/child systems, components, commissioning, warranty, transfer, mothball, disposal, replacement, and full service history. |
| MA-COM-002 | Work request portal | COMMON | Target | Simple request intake with asset/location lookup, photos, urgency, duplicate suggestions, triage, and requester status. |
| MA-COM-003 | Work planning and scheduling | COMMON | Target | Backlog, priority, craft/skill, estimates, permits, parts/tools, dependencies, calendar/board, and capacity-aware scheduling. |
| MA-COM-004 | Preventive and condition-based maintenance | COMMON | Target | Calendar, meter, usage, threshold, seasonal, and inspection-condition triggers with forecast and optimization. |
| MA-COM-005 | Inspections and checklists | COMMON | Target | Versioned templates, conditional questions, measurements, photos, signatures, defect creation, and pass/fail rules. |
| MA-COM-006 | Labor and technician execution | COMMON | Target | Assignments, start/pause/complete, travel/wrench time, notes, evidence, signatures, and qualification checks. |
| MA-COM-007 | Parts and materials | COMMON | Target | BOM, reservations, issues, returns, substitutions, kitting, shortage visibility, procurement handoff, and cost attribution. |
| MA-COM-008 | Vendor/contractor maintenance | COMMON | Target | Quote/authorization, scoped portal, dispatch, check-in, evidence, invoice context, warranty, and performance. |
| MA-COM-009 | Downtime and availability | COMMON | Target | Planned/unplanned downtime, reason, production/service impact, MTBF, MTTR, availability, and backlog risk. |
| MA-COM-010 | Warranty and recall | COMMON | Target | Warranty terms/claims, campaign applicability, notifications, parts/labor recovery, completion evidence, and unresolved-risk tracking. |
| MA-COM-011 | Asset reservation/readiness | COMMON | Partial | Motor-pool style availability, reservations, conflicts, pre/post-use checks, handoff, usage, and damage/charge context. |
| MA-COM-012 | Maintenance reporting | COMMON | Target | Backlog, schedule compliance, PM compliance, failures, downtime, labor, parts, cost, repeat defects, warranty recovery, and readiness. |
| MA-COM-013 | Mobile scanning and offline work | COMMON | Target | QR/barcode/NFC asset lookup, work execution, parts scan, evidence, signature, and safe offline queue. |
| MA-COM-014 | Safety and permit controls | COMMON | Target | LOTO, confined space, hot work, PPE, hazard acknowledgement, permit refs, and qualified-worker gates. |

### C. Commonly requested but widely underserved

These address recurring user friction, fragmented add-ons, poor transparency, inaccessible setup, or weak SMB usability.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| MA-UND-001 | One-tap field execution | UNDERSERVED | Target | Technicians see only the next useful action, required evidence, parts, hazards, and blockers without enterprise form overload. |
| MA-UND-002 | Voice-guided inspection and work capture | UNDERSERVED | Target | Hands-free readout, spoken answers/notes, pause/resume, confirmation, and accessible fallback with timestamps and review. |
| MA-UND-003 | Explainable asset readiness | UNDERSERVED | Target | Show every blocker, warning, stale input, override, and owning product rather than a mysterious red/green status. |
| MA-UND-004 | Quick-create asset, part, location, and vendor references | UNDERSERVED | Partial | Capture the minimum valid missing reference in context and return to the work order or inspection immediately; asset quick-create is live in defect intake, part quick-create is live in kit authoring, and location/vendor references remain pending. |
| MA-UND-005 | Small-fleet and mixed-asset mode | UNDERSERVED | Target | Vehicles, facilities, tools, production equipment, and IT/utility assets can coexist without separate products or excessive setup. |
| MA-UND-006 | Maintenance-to-procurement custody visibility | UNDERSERVED | Target | A technician can see requested, reserved, ordered, shipped, received, staged, and issued parts without MaintainArr pretending to own inventory. |
| MA-UND-007 | Affordable condition monitoring | UNDERSERVED | Target | Manual readings, inexpensive sensors, telematics imports, confidence, anomaly review, and rules provide value before an enterprise IIoT program. |
| MA-UND-008 | Field evidence quality coaching | UNDERSERVED | Target | Prompt for missing angles, measurements, serials, signatures, and closeout proof before the technician leaves the asset. |
| MA-UND-009 | Guided troubleshooting and knowledge reuse | UNDERSERVED | Target | Surface prior failures, likely causes, approved procedures, known fixes, bulletins, and parts while preserving technician judgment. |
| MA-UND-010 | Transparent schedule and backlog promises | UNDERSERVED | Target | Requesters and managers see planned window, blockers, risk, ownership, and updates rather than only open/closed. |
| MA-UND-011 | Contractor-friendly scoped access | UNDERSERVED | Target | External technicians receive only assigned work, asset context, safety requirements, evidence upload, and completion actions. |
| MA-UND-012 | Corrective feedback from operations | UNDERSERVED | Target | Trip, warehouse, quality, and operator events can create defects with source context and receive readiness outcome without duplicated records. |

### D. Advanced capabilities to democratize

These are commonly found only in enterprise tiers, specialist products, or expensive implementations. STL should make them practical without weakening governance.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| MA-DEM-001 | Predictive maintenance and failure-risk scoring | DEMOCRATIZE | Target | Combine readings, failures, environment, usage, parts, inspections, and known campaigns into explainable risk with human approval. |
| MA-DEM-002 | Maintenance strategy optimization | DEMOCRATIZE | Target | Compare reactive, PM, condition-based, redesign, and replacement strategies using cost, downtime, safety, and risk scenarios. |
| MA-DEM-003 | Digital twin maintenance context | DEMOCRATIZE | Target | Asset configuration, component lineage, documents, live readings, work history, and operational assignments in one navigable model. |
| MA-DEM-004 | Augmented-reality work guidance | DEMOCRATIZE | Target | Overlay approved steps, component identification, measurements, and remote expert annotations without making AR mandatory. |
| MA-DEM-005 | Reliability engineering toolkit | DEMOCRATIZE | Target | Failure codes, FMEA/RCM links, Weibull/trend analysis, bad-actor ranking, root cause, and reliability action tracking. |
| MA-DEM-006 | Multi-site maintenance planning | DEMOCRATIZE | Target | Shared backlog, traveling technicians, parts/tool availability, shutdown coordination, and cross-site prioritization. |
| MA-DEM-007 | Warranty recovery automation | DEMOCRATIZE | Target | Identify warrantable work/parts, assemble claim evidence, submit/track claims, and reconcile recovered value. |
| MA-DEM-008 | Automated recall and service-bulletin intelligence | DEMOCRATIZE | Target | Ingest manufacturer/regulator campaigns, match assets/components, explain confidence, assign work, and prove completion. |
| MA-DEM-009 | Maintenance cost and lifecycle forecasting | DEMOCRATIZE | Target | Forecast labor, parts, downtime, replacement, capital plan, and residual risk by asset/system/site. |
| MA-DEM-010 | Remote expert collaboration | DEMOCRATIZE | Target | Secure live video, annotation, evidence capture, and escalation tied to the work record for small teams without a separate enterprise platform. |

### E. Suite-wide foundation required in MaintainArr

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| MA-FND-001 | Tenant-scoped authorization | FOUNDATION | Target | Every read and write is tenant-scoped server-side; product UI visibility never substitutes for authorization. |
| MA-FND-002 | StaffArr-backed action permissions | FOUNDATION | Target | Use product action keys, role scopes, delegated authority, and explicit denial reasons; ownership does not prevent permissioned delegated workflows. |
| MA-FND-003 | Unified suite shell | FOUNDATION | Target | Consistent navigation, page anatomy, terminology, responsive behavior, keyboard access, and light/dark contrast across products. |
| MA-FND-004 | Record lifecycle and history | FOUNDATION | Target | Human-readable statuses, timeline events, actor/time/source attribution, undo or correction paths, and immutable audit evidence. |
| MA-FND-005 | Quick create for missing references | FOUNDATION · UNDERSERVED | Partial | Create the minimum valid cross-product reference without abandoning the current task; preserve context and allow later backfill. Asset quick-create is wired through MaintainArr defect intake, part quick-create is wired through parts-kit authoring, site quick-create is wired through PM program owning-site selection, and other reference types remain pending. |
| MA-FND-006 | Saved views and personal preferences | COMMON | Target | Per-user filters, columns, density, sort, board/list preference, default landing view, and product-scoped preferences. |
| MA-FND-007 | Bulk operations with preview | COMMON | Target | Selection-aware actions, dry-run/impact preview, partial failure handling, downloadable results, and permission checks per record. |
| MA-FND-008 | Import with mapping and review | COMMON · UNDERSERVED | Target | Product-specific import separate from Smart Import, with templates, mappings, validation, dedupe, dry run, commit plan, and rollback/correction evidence. |
| MA-FND-009 | Export and portability | COMMON · DEMOCRATIZE | Target | CSV/PDF/JSON or package export appropriate to the domain, without trapping customer data behind enterprise tiers. |
| MA-FND-010 | Notifications, tasks, and inbox | COMMON | Target | Actionable notifications with ownership, due dates, escalation, snooze/delegation, deep links, and duplicate suppression. |
| MA-FND-011 | API, webhooks, and event outbox | COMMON · DEMOCRATIZE | Target | Stable versioned APIs, idempotency, service authentication, signed webhooks/events, retries, dead-letter handling, and correlation IDs. |
| MA-FND-012 | RecordArr evidence references | FOUNDATION | Target | Files and evidence are stored once in RecordArr and referenced by owning records; products do not create shadow document vaults. |
| MA-FND-013 | ReportArr projections and metrics | FOUNDATION | Target | Operational products emit facts/events; ReportArr builds read models and reports without becoming the source of operational truth. |
| MA-FND-014 | Compliance Core applicability and gates | FOUNDATION | Target | Products request explainable requirements, missing facts, evidence needs, and gate decisions rather than embedding independent legal logic. |
| MA-FND-015 | Field Companion execution surface | FOUNDATION · UNDERSERVED | Target | Mobile-first task, scan, capture, acknowledgement, and offline execution against the owning product API. |
| MA-FND-016 | Professional print and report view | COMMON | Target | Print output is a purpose-built report document without the app shell, not a screenshot of the current page. |
| MA-FND-017 | Accessible error and degraded states | FOUNDATION | Target | Explain what failed, what remains saved, whether retry is safe, what the user can do, and which administrator or integration owns the issue. |
| MA-FND-018 | Time, locale, and units | FOUNDATION | Target | Store canonical timestamps and units; display tenant/user locale, timezone, currency, and measurement preferences consistently. |
| MA-FND-019 | Retention, privacy, and legal hold awareness | FOUNDATION | Target | Product deletion/archive actions respect RecordArr retention, legal holds, privacy policy, and downstream references. |
| MA-FND-020 | AI-assisted proposals, never silent commits | UNDERSERVED | Target | AI may classify, summarize, suggest, or draft; consequential changes remain reviewable, permissioned, attributable, and reversible. |

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
<summary>Persistent entity sets (98)</summary>

| Entity type | DbSet/property | Source |
| --- | --- | --- |
| AssetClass | AssetClasses | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetType | AssetTypes | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| Asset | Assets | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| MaintainArrAuditEvent | AuditEvents | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| MaintainArrTenantSettings | MaintainArrTenantSettings | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| MaintainArrTenantSettingsAudit | MaintainArrTenantSettingsAudit | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| PmSchedule | PmSchedules | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| PmOccurrence | PmOccurrences | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| PmProgram | PmPrograms | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| PmProgramSchedule | PmProgramSchedules | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| InspectionTemplate | InspectionTemplates | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| InspectionTemplateCategory | InspectionTemplateCategories | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| InspectionChecklistItem | InspectionChecklistItems | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| InspectionTemplateAssetType | InspectionTemplateAssetTypes | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| InspectionRun | InspectionRuns | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| InspectionRunAnswer | InspectionRunAnswers | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| InspectionRunPauseEvent | InspectionRunPauseEvents | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| Defect | Defects | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| DefectEvidence | DefectEvidence | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| InspectionRunEvidence | InspectionRunEvidence | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetMeter | AssetMeters | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| MeterReading | MeterReadings | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| WorkOrder | WorkOrders | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| WorkOrderTaskLine | WorkOrderTaskLines | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| WorkOrderLaborEntry | WorkOrderLaborEntries | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| WorkOrderTechnicianAssignment | WorkOrderTechnicianAssignments | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| WorkOrderEvidence | WorkOrderEvidence | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| MaintenancePart | MaintenanceParts | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| WorkOrderPartsDemandLine | WorkOrderPartsDemandLines | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| WorkOrderPartsDemandStatusEvent | WorkOrderPartsDemandStatusEvents | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| WorkOrderBlocker | WorkOrderBlockers | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| WorkOrderCloseout | WorkOrderCloseouts | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| MaintenancePermitRef | MaintenancePermitRefs | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| ReturnToService | ReturnToServices | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| WorkOrderComment | WorkOrderComments | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| WorkOrderTimelineEvent | WorkOrderTimelineEvents | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| TenantMaintenanceNotificationSettings | TenantMaintenanceNotificationSettings | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| MaintenanceNotificationDispatch | MaintenanceNotificationDispatches | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AuditPackageGenerationJob | AuditPackageGenerationJobs | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| TenantDefectEscalationSettings | TenantDefectEscalationSettings | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| DefectEscalationRun | DefectEscalationRuns | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| DefectEscalationEvent | DefectEscalationEvents | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| TenantAssetStatusRollupSettings | TenantAssetStatusRollupSettings | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetStatusRollup | AssetStatusRollups | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetStatusScopeRollup | AssetStatusScopeRollups | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetStatusRollupRun | AssetStatusRollupRuns | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| TenantMaintenanceHistoryRollupSettings | TenantMaintenanceHistoryRollupSettings | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| MaintenanceHistoryRollup | MaintenanceHistoryRollups | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| MaintenanceHistoryEvent | MaintenanceHistoryEvents | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| MaintenanceHistoryRollupRun | MaintenanceHistoryRollupRuns | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| TenantPmDueScanSettings | TenantPmDueScanSettings | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| PmDueScanRun | PmDueScanRuns | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| ComplianceRegulatoryKeyMirror | ComplianceRegulatoryKeyMirrors | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| MaintainArrImportBatch | MaintainArrImportBatches | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| MaintainArrStaffPersonRef | StaffPersonRefs | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| TenantDowntimeTrackingSettings | TenantDowntimeTrackingSettings | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetDowntimeEvent | AssetDowntimeEvents | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetAvailabilitySnapshot | AssetAvailabilitySnapshots | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| FleetAvailabilitySnapshot | FleetAvailabilitySnapshots | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetDowntimeSyncRun | AssetDowntimeSyncRuns | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| TenantMaintenancePlatformEventSettings | TenantMaintenancePlatformEventSettings | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| MaintenancePlatformOutboxEvent | MaintenancePlatformOutboxEvents | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| MaintenancePlatformEventProcessingRun | MaintenancePlatformEventProcessingRuns | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| MaintenanceInboundPlatformEvent | MaintenanceInboundPlatformEvents | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| CatalogDefinition | CatalogDefinitions | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| CatalogOption | CatalogOptions | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| CatalogOptionDependency | CatalogOptionDependencies | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| FieldsetDefinition | FieldsetDefinitions | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| FieldsetField | FieldsetFields | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| PendingCatalogValue | PendingCatalogValues | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| ReferenceCacheEntry | ReferenceCacheEntries | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetCustomFieldValue | AssetCustomFieldValues | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetSpec | AssetSpecs | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetComponent | AssetComponents | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetInstalledComponent | AssetInstalledComponents | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetDocumentRef | AssetDocumentRefs | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetComplianceState | AssetComplianceStates | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetStatusHistory | AssetStatusHistory | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetLocationHistory | AssetLocationHistory | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetAssignmentHistory | AssetAssignmentHistory | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetReadinessState | AssetReadinessStates | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetExternalMapping | AssetExternalMappings | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetExternalIdentifier | AssetExternalIdentifiers | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetEnrichmentSnapshot | AssetEnrichmentSnapshots | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetEnrichmentSuggestion | AssetEnrichmentSuggestions | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetRecallSnapshot | AssetRecallSnapshots | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| RecallCampaign | RecallCampaigns | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| RecallCampaignApplicability | RecallCampaignApplicabilities | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetRecallCase | AssetRecallCases | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| RecallAuditLogEntry | RecallAuditLogEntries | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| RecallMakeModelAlias | RecallMakeModelAliases | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| ExternalProviderCacheEntry | ExternalProviderCacheEntries | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| ExternalProviderAuditLogEntry | ExternalProviderAuditLogEntries | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetQualityHold | AssetQualityHolds | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| AssetReadinessCheck | AssetReadinessChecks | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| MaintenancePartsKit | MaintenancePartsKits | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| MaintenancePartsKitLine | MaintenancePartsKitLines | MaintainArr.Api/Data/MaintainArrDbContext.cs |
| MaintenanceVendorWork | MaintenanceVendorWorks | MaintainArr.Api/Data/MaintainArrDbContext.cs |

</details>

<details>
<summary>Frontend page files (31)</summary>

| Page |
| --- |
| src/components/AssetDetailsPage.tsx |
| src/lib/createWorkspacePage.tsx |
| src/pages/LaunchPage.tsx |
| src/pages/WorkOrderWorkspacePage.tsx |
| src/workspace/MaintainArrWorkspacePage.tsx |
| src/pages/assets/AssetCreatePage.tsx |
| src/pages/assets/AssetProfilePage.tsx |
| src/pages/assets/AssetsPage.tsx |
| src/pages/defects/DefectCreatePage.tsx |
| src/pages/defects/DefectsPage.tsx |
| src/pages/downtime/DowntimePage.tsx |
| src/pages/history/HistoryPage.tsx |
| src/pages/imports/ImportsPage.tsx |
| src/pages/inspection-templates/InspectionTemplateCreatePage.tsx |
| src/pages/inspection-templates/InspectionTemplatesPage.tsx |
| src/pages/inspections/InspectionsPage.tsx |
| src/pages/meters/MetersPage.tsx |
| src/pages/overview/OverviewPage.tsx |
| src/pages/parts-kits/PartsKitCreatePage.tsx |
| src/pages/parts-kits/PartsKitsPage.tsx |
| src/pages/parts/PartCreatePage.tsx |
| src/pages/parts/PartDetailPage.tsx |
| src/pages/parts/PartsPage.tsx |
| src/pages/pm-programs/PmProgramCreatePage.tsx |
| src/pages/pm-programs/PmProgramsPage.tsx |
| src/pages/recalls/RecallsPage.tsx |
| src/pages/reports/ReportsPage.tsx |
| src/pages/scheduling/WorkSchedulingPage.tsx |
| src/pages/settings/SettingsPage.tsx |
| src/pages/work-orders/WorkOrderCreatePage.tsx |
| src/pages/work-orders/WorkOrdersPage.tsx |

</details>

<details>
<summary>Endpoint source families (61)</summary>

| Endpoint source file | Discovered route declarations |
| --- | --- |
| IntegrationEndpoints.cs | 38 |
| ExternalIntelligenceEndpoints.cs | 18 |
| InspectionTemplateEndpoints.cs | 17 |
| RecallEndpoints.cs | 17 |
| MaintenancePartsKitEndpoints.cs | 14 |
| WorkOrderEndpoints.cs | 14 |
| DefectEndpoints.cs | 11 |
| ReferenceEndpoints.cs | 11 |
| WorkOrderLaborEvidenceEndpoints.cs | 11 |
| AssetEndpoints.cs | 10 |
| CatalogFieldsetEndpoints.cs | 10 |
| InspectionEndpoints.cs | 9 |
| MeterEndpoints.cs | 9 |
| PmProgramEndpoints.cs | 9 |
| SchedulingEndpoints.cs | 9 |
| AuditPackageEndpoints.cs | 8 |
| AuthEndpoints.cs | 8 |
| PreventiveMaintenanceEndpoints.cs | 8 |
| AssetDowntimeEndpoints.cs | 7 |
| AssetStatusRollupEndpoints.cs | 7 |
| MaintenanceReportEndpoints.cs | 7 |
| AssetImportEndpoints.cs | 6 |
| AssetReadinessEndpoints.cs | 6 |
| WorkOrderPartsDemandEndpoints.cs | 6 |
| DefectEscalationSettingsEndpoints.cs | 5 |
| EntityExportEndpoints.cs | 5 |
| MaintenancePartEndpoints.cs | 5 |
| PmDueScanSettingsEndpoints.cs | 5 |
| ReferenceIntegrationEndpoints.cs | 5 |
| AssetClassEndpoints.cs | 4 |
| AssetComponentEndpoints.cs | 4 |
| AssetStatusRollupSettingsEndpoints.cs | 4 |
| AssetTypeEndpoints.cs | 4 |
| ComplianceReportEndpoints.cs | 4 |
| DefectEvidenceEndpoints.cs | 4 |
| DowntimeTrackingSettingsEndpoints.cs | 4 |
| MaintainArrTenantSettingsEndpoints.cs | 4 |
| MaintenanceHistoryRollupSettingsEndpoints.cs | 4 |
| MaintenancePlatformEventSettingsEndpoints.cs | 4 |
| DocumentEndpoints.cs | 3 |
| NotificationSettingsEndpoints.cs | 3 |
| WorkOrderDiscussionEndpoints.cs | 3 |
| EventAndAuditEndpoints.cs | 2 |
| ExecutiveReportEndpoints.cs | 2 |
| InternalAssetDowntimeSyncEndpoints.cs | 2 |
| InternalAssetStatusRollupEndpoints.cs | 2 |
| InternalAuditPackageGenerationEndpoints.cs | 2 |
| InternalDefectEscalationEndpoints.cs | 2 |
| InternalMaintenanceHistoryRollupEndpoints.cs | 2 |
| InternalMaintenanceNotificationEndpoints.cs | 2 |
| InternalMaintenancePlatformEventEndpoints.cs | 2 |
| InternalPmDueScanEndpoints.cs | 2 |
| InternalTechnicianRefSyncEndpoints.cs | 2 |
| MaintenanceHistoryEndpoints.cs | 2 |
| MaintenanceVendorWorkEndpoints.cs | 2 |
| TechnicianRefEndpoints.cs | 2 |
| DashboardEndpoints.cs | 1 |
| FieldInboxEndpoints.cs | 1 |
| ReportIndexEndpoints.cs | 1 |
| SettingsEndpoints.cs | 1 |
| WorkOrderSupplyReadinessEndpoints.cs | 1 |

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

Stabilize deep workflows, complete inventory/procurement handoffs, preserve technician usability, and prevent configuration depth from becoming form overload.

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
