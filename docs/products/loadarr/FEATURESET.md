# LoadArr — WMS Full Feature Set

[Workflow catalog](./WORKFLOWS.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md)

## Document control

| Field | Value |
| --- | --- |
| Product | LoadArr (WMS) |
| Category | Warehouse Management System |
| Repository maturity | Scaffold |
| Audit basis | Static repository inspection on 2026-06-24 |
| Cataloged capabilities | 71 |
| Cataloged workflows | 15 |

## Product charter

LoadArr owns warehouse execution and physical inventory truth: receiving, inventory status and custody, putaway, reservation/allocation, replenishment, picking, packing/staging, shipping confirmation, transfer, count, adjustment, return, and the immutable stock ledger. It references StaffArr-owned locations and SupplyArr-owned item/supplier/procurement context rather than duplicating them.

> **Implementation reality — Scaffold:** LoadArr has a large frontend route map and API surface covering receiving, putaway, reservations, picking, transfers, holds, staging, shipping, counts, exceptions, returns, backorders, warehouses, stock ledger, devices, labels, integrations, unexplained inventory, and kits. Static inspection found durable EF persistence primarily for tenant settings and setting audit entries; many operational endpoints return generated/static data or non-durable write responses. Treat the current product as a UI/API scaffold that requires a durable inventory ledger and transaction engine before production use.

## Source-of-truth boundary

### LoadArr owns

- Warehouse operational profiles and mappings to StaffArr-owned site/location hierarchy.
- Inventory balances by item, owner, location, status, lot/serial/expiry, handling unit, and custody context.
- Immutable stock ledger and all physical inventory movements, reservations, allocations, issues, returns, adjustments, and transfers.
- Expected receipts/ASNs, receiving execution, discrepancies, staging, putaway tasks, and receipt completion.
- Replenishment, wave/task planning, picking, packing, staging, loadout, shipping confirmation, and warehouse proof.
- Cycle counts, wall-to-wall counts, unexplained inventory investigation, inventory holds, quarantine execution, and warehouse exceptions.
- Warehouse devices, printers/labels, scan rules, task execution, offline validation, and operational settings.

### LoadArr does not own

- Canonical internal locations; StaffArr owns location identity and hierarchy, while LoadArr owns warehouse meaning and movement using those IDs.
- Part/item commercial master, suppliers, purchase orders, pricing, or sourcing; SupplyArr owns them.
- Customer/order lifecycle; CustomArr/OrdArr own customer and order truth.
- Transportation trip/route/ETA; RoutArr owns transport and notifies LoadArr of inbound/outbound movements and appointments.
- Quality disposition/release; AssurArr owns it, though LoadArr enforces physical holds and movement blocks.
- Maintenance parts demand/work order; MaintainArr owns demand context while LoadArr reserves/issues inventory.
- General-ledger inventory valuation; LedgArr owns accounting while LoadArr supplies quantity/custody facts.

## Suite access and authorization

- All normal tenant users may launch every product application in the suite; product actions and records remain permission-scoped server-side.
- Fixed-suite access is nonvariable. Deployment health, feature readiness, legal restrictions, external-provider setup, and permissions may still constrain a capability.
- Platform administration remains a NexArr-only responsibility and is not a tenant product surface.
- Compliance Core administrative/authoring UI is restricted, while Compliance Core runtime evaluations, fact requirements, evidence needs, and workflow gates serve every tenant and product.

## Primary users

- Warehouse administrator
- Receiving worker
- Picker/packer
- Inventory control
- Warehouse supervisor
- Dock/yard coordinator
- Returns worker
- Cycle counter
- Implementation lead
- Auditor

## Required integrations

- StaffArr
- SupplyArr
- OrdArr
- RoutArr
- MaintainArr
- TrainArr
- AssurArr
- RecordArr
- Compliance Core
- ReportArr
- LedgArr
- Field Companion
- Barcode/RFID/printer/automation providers

## Product principles

- No operational release until a durable, atomic, idempotent stock ledger is implemented and tested under concurrency.
- Balances are derived from ledger transactions; no endpoint directly edits an inventory balance.
- StaffArr owns location identity; LoadArr adds warehouse operational meaning and uses those IDs in movements.
- Offline actions remain visibly pending until server validation commits the inventory transaction.

## Repository implementation snapshot

| Static indicator | Count |
| --- | --- |
| Persistent DbSet declarations | 2 |
| Discovered server classes | 147 |
| Discovered HTTP route declarations | 165 |
| Frontend source files | 17 |
| Frontend page files | 1 |
| Documentation headings | 93 |

### Evidence used for the current-state classification

- loadarr-api has durable LoadArrTenantSettings and LoadArrTenantSettingAuditEntries.
- Operational route/API names cover receiving, receipt detail, putaway, reservations, allocation, picking, packing/staging, shipping, transfers, replenishment, counts, adjustments, holds, returns, backorders, exceptions, stock ledger/history, warehouses/areas, devices, labels, integrations, unexplained inventory, and kits.
- loadarr-frontend exposes broad warehouse pages and workflow screens, demonstrating intended navigation and user tasks.
- Static analysis did not find a durable operational DbContext/entity set for inventory balances, ledger entries, receipts, tasks, shipments, or counts; current operational responses should not be represented as production persistence.
- The canonical V2 workflow packs already define SupplyArr PO → LoadArr receiving/putaway and AssurArr hold/release boundaries.

> Counts are static-discovery indicators, not proof that every route or screen is complete, reachable, secure, migrated, or production-ready.

## Mandatory migration or refactor work

- Create durable operational persistence before describing receiving, putaway, inventory, task, wave, count, replenishment, picking, packing, staging, shipping, labor, yard, or automation features as shipped.
- Move canonical inventory balance, stock ledger, and warehouse movement truth out of SupplyArr; preserve source references and idempotent migration/events.

## Feature catalog

### A. Currently implemented or meaningfully represented

These capabilities have repository evidence. Their state follows the product-level maturity and may still require hardening or completion.

| Feature ID | Capability | Class | State | Required behavior / evidence |
| --- | --- | --- | --- | --- |
| LO-CUR-001 | Tenant warehouse settings and audit | CURRENT | Scaffold | Tenant settings are durably persisted and audited. |
| LO-CUR-002 | Warehouse dashboard and operational navigation scaffold | CURRENT | Scaffold | Frontend surfaces expose the intended warehouse operating model and queues. |
| LO-CUR-003 | Receiving and expected-receipt scaffold | CURRENT | Scaffold | Routes and API shapes exist for receipts, check-in, discrepancies, and completion, but durable transaction persistence is incomplete. |
| LO-CUR-004 | Putaway task scaffold | CURRENT | Scaffold | Pages/actions represent directed putaway and staging-to-location movement. |
| LO-CUR-005 | Reservations and allocation scaffold | CURRENT | Scaffold | Surfaces represent demand reservations, allocation, shortage, release, and status. |
| LO-CUR-006 | Picking, packing, staging, and shipping scaffold | CURRENT | Scaffold | Operational routes cover outbound execution through shipping confirmation. |
| LO-CUR-007 | Transfers and replenishment scaffold | CURRENT | Scaffold | Pages/actions model internal location movement and stock replenishment. |
| LO-CUR-008 | Cycle count and adjustment scaffold | CURRENT | Scaffold | Count queues, count entry, discrepancy, approval, and adjustment concepts are present. |
| LO-CUR-009 | Inventory hold and quarantine scaffold | CURRENT | Scaffold | Hold views/actions represent physical movement blocks and AssurArr coordination. |
| LO-CUR-010 | Returns, backorders, and exceptions scaffold | CURRENT | Scaffold | Operational routes model reverse flow and unresolved fulfillment/receiving conditions. |
| LO-CUR-011 | Stock ledger/history UI scaffold | CURRENT | Scaffold | Pages imply immutable movement history but the durable ledger engine must be implemented. |
| LO-CUR-012 | Warehouses, areas, devices, labels, and integrations scaffold | CURRENT | Scaffold | Administration routes cover operational topology and hardware/integration configuration. |
| LO-CUR-013 | Unexplained inventory and kit workflows scaffold | CURRENT | Scaffold | The UI includes investigation and kit-related operational concepts. |

### B. Common category baseline

These are expected for a credible Warehouse Management System product. Where they overlap current behavior, the target definition is the acceptance standard rather than a duplicate feature.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| LO-COM-001 | Durable inventory balance and stock ledger | COMMON | Target | Atomic, idempotent movements derive balances by item/location/status/lot/serial/owner with no direct balance edits. |
| LO-COM-002 | Item, lot, serial, and expiry control | COMMON | Target | Scan/validate identity, UOM/package, lot/batch, serial uniqueness, expiration, shelf life, and status. |
| LO-COM-003 | Expected receipt and ASN | COMMON | Target | PO/shipment expectations, appointments, packing structures, labels, and receipt readiness. |
| LO-COM-004 | Receiving | COMMON | Target | Check-in, unload, scan/count, blind receipt, over/short/damage, compliance evidence, hold, and receipt close. |
| LO-COM-005 | Directed putaway | COMMON | Target | Rules using item, hazard, temperature, size, velocity, compatibility, capacity, zone, and travel distance. |
| LO-COM-006 | Inventory status and hold | COMMON | Target | Available, allocated, held, quarantine, damaged, inspection, expired, recalled, and owner/customer-specific status. |
| LO-COM-007 | Reservation and allocation | COMMON | Target | Demand priority, promise, FEFO/FIFO, lot/serial constraints, partial/backorder, substitution, and release. |
| LO-COM-008 | Wave/waveless picking | COMMON | Target | Order/zone/batch/cluster/discrete picking, task interleaving, short pick, skip, substitution, and confirmation. |
| LO-COM-009 | Packing and staging | COMMON | Target | Containerization, weight/dimensions, labels/documents, QA checks, staging lane, load sequencing, and custody. |
| LO-COM-010 | Shipping and loadout | COMMON | Target | Carrier/trip verification, seal, scan-on-load, shipment confirmation, proof, and order/transport events. |
| LO-COM-011 | Replenishment and slotting | COMMON | Target | Min/max, demand-driven, forward-pick replenishment, capacity, velocity, and slot recommendations. |
| LO-COM-012 | Transfers and internal movements | COMMON | Target | Move, consolidate, split, relabel, cross-dock, quarantine, return-to-stock, and inter-warehouse transfer. |
| LO-COM-013 | Cycle counts and adjustments | COMMON | Target | ABC/risk/event counts, blind counts, recount, discrepancy investigation, approval, and reasoned ledger adjustment. |
| LO-COM-014 | Returns and reverse logistics | COMMON | Target | Customer/vendor/internal returns, RMA, inspect, disposition, restock, repair, scrap, and financial/quality handoff. |
| LO-COM-015 | Barcode/RFID and label management | COMMON | Target | GS1-aware scan parsing, configurable symbologies, label templates, printer routing, reprint control, and RFID events. |
| LO-COM-016 | Warehouse labor/task management | COMMON | Target | Queues, skills/equipment, priority, travel, productivity, indirect time, workload balancing, and safe assignment. |

### C. Commonly requested but widely underserved

These address recurring user friction, fragmented add-ons, poor transparency, inaccessible setup, or weak SMB usability.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| LO-UND-001 | Zero-training scan-first worker UX | UNDERSERVED | Target | Each scan identifies context and presents only valid next actions, with large controls, audio/haptic confirmation, and clear recovery. |
| LO-UND-002 | Small warehouse mode | UNDERSERVED | Target | Receive, put away, count, pick, and ship with minimal setup while preserving the same ledger and growth path. |
| LO-UND-003 | Offline handheld execution with conflict transparency | UNDERSERVED | Target | Workers can continue approved tasks offline; queued actions show validation state and conflicts instead of pretending inventory already changed. |
| LO-UND-004 | End-to-end custody explanation | UNDERSERVED | Target | Every unit/lot/serial shows where it came from, who touched it, why it moved, what demand consumed it, and which evidence supports it. |
| LO-UND-005 | Fast unexplained-inventory investigation | UNDERSERVED | Target | Correlate scans, tasks, counts, receipts, picks, cameras/evidence refs, user/device, and neighboring locations to propose likely causes. |
| LO-UND-006 | Affordable slotting and travel optimization | UNDERSERVED | Target | Recommendations use actual velocity, size, compatibility, replenishment effort, congestion, and labor without an enterprise consulting project. |
| LO-UND-007 | Voice and wearable workflows | UNDERSERVED | Target | Hands-free pick/count/putaway with confirmation, accessibility, and safe fallback using commodity devices. |
| LO-UND-008 | Cross-product shortage resolution | UNDERSERVED | Target | Show demand owner, business impact, alternatives, transfer, substitute, purchase, partial fulfillment, and promise effects in one guided decision. |
| LO-UND-009 | Human-readable inventory status | UNDERSERVED | Target | Explain why stock is unavailable—reserved, held, wrong location/UOM/lot, expired, inspection, or data conflict—rather than showing only zero available. |
| LO-UND-010 | Warehouse/customer/supplier exception collaboration | UNDERSERVED | Target | Scoped portals and tasks resolve over/short/damage/label/document questions without email chains or broad system access. |
| LO-UND-011 | Rapid implementation/import | UNDERSERVED | Target | Guided warehouse/location mapping, item/UOM cleanup, opening-balance proof, label setup, and dry-run cutover suitable for small operations. |
| LO-UND-012 | Shared/multi-client warehouse support | UNDERSERVED | Target | Owner-specific inventory, rules, documents, billing facts, and portals without requiring a separate enterprise 3PL suite. |

### D. Advanced capabilities to democratize

These are commonly found only in enterprise tiers, specialist products, or expensive implementations. STL should make them practical without weakening governance.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| LO-DEM-001 | Warehouse execution system and automation orchestration | DEMOCRATIZE | Target | Vendor-neutral task exchange, equipment capability/status, exception fallback, and human/robot work coordination. |
| LO-DEM-002 | Computer-vision receiving and counting | DEMOCRATIZE | Target | Propose counts, labels, damage, dimensions, and pallet/carton identity from images/video with confidence and human verification. |
| LO-DEM-003 | Dynamic slotting and digital twin | DEMOCRATIZE | Target | Continuously model capacity, congestion, velocity, compatibility, automation, and labor impact before applying slot changes. |
| LO-DEM-004 | Advanced labor planning | DEMOCRATIZE | Target | Forecast workload and staffing by inbound/outbound demand, skills, equipment, service windows, and historical performance without punitive surveillance. |
| LO-DEM-005 | Waveless orchestration and task interleaving | DEMOCRATIZE | Target | Continuously prioritize work across receive/putaway/replenish/pick/count/load using SLA and travel impact. |
| LO-DEM-006 | Robotics and autonomous-mobile-robot integration | DEMOCRATIZE | Target | Standard task/mission contracts, traffic zones, exception handoff, safety state, and proof independent of one vendor. |
| LO-DEM-007 | Inventory risk prediction | DEMOCRATIZE | Target | Identify likely stockout, expiry, mis-slot, shrink, congestion, or count error with evidence and proposed actions. |
| LO-DEM-008 | Yard-to-warehouse control tower | DEMOCRATIZE | Target | One operational view of inbound ETA, yard/trailer, dock door, labor, receiving, staging, outbound readiness, and exceptions. |
| LO-DEM-009 | Cold-chain and regulated warehouse controls | DEMOCRATIZE | Target | Temperature excursions, chain of custody, quarantine, calibration references, FEFO, attestations, and evidence packages accessible to smaller operators. |
| LO-DEM-010 | 3PL billing contribution | DEMOCRATIZE | Target | Generate auditable storage, handling, value-added service, labor, accessorial, and transportation facts for LedgArr without embedding final invoicing logic. |

### E. Suite-wide foundation required in LoadArr

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| LO-FND-001 | Tenant-scoped authorization | FOUNDATION | Target | Every read and write is tenant-scoped server-side; product UI visibility never substitutes for authorization. |
| LO-FND-002 | StaffArr-backed action permissions | FOUNDATION | Target | Use product action keys, role scopes, delegated authority, and explicit denial reasons; ownership does not prevent permissioned delegated workflows. |
| LO-FND-003 | Unified suite shell | FOUNDATION | Target | Consistent navigation, page anatomy, terminology, responsive behavior, keyboard access, and light/dark contrast across products. |
| LO-FND-004 | Record lifecycle and history | FOUNDATION | Target | Human-readable statuses, timeline events, actor/time/source attribution, undo or correction paths, and immutable audit evidence. |
| LO-FND-005 | Quick create for missing references | FOUNDATION · UNDERSERVED | Target | Create the minimum valid cross-product reference without abandoning the current task; preserve context and allow later backfill. |
| LO-FND-006 | Saved views and personal preferences | COMMON | Target | Per-user filters, columns, density, sort, board/list preference, default landing view, and product-scoped preferences. |
| LO-FND-007 | Bulk operations with preview | COMMON | Target | Selection-aware actions, dry-run/impact preview, partial failure handling, downloadable results, and permission checks per record. |
| LO-FND-008 | Import with mapping and review | COMMON · UNDERSERVED | Target | Product-specific import separate from Smart Import, with templates, mappings, validation, dedupe, dry run, commit plan, and rollback/correction evidence. |
| LO-FND-009 | Export and portability | COMMON · DEMOCRATIZE | Target | CSV/PDF/JSON or package export appropriate to the domain, without trapping customer data behind enterprise tiers. |
| LO-FND-010 | Notifications, tasks, and inbox | COMMON | Target | Actionable notifications with ownership, due dates, escalation, snooze/delegation, deep links, and duplicate suppression. |
| LO-FND-011 | API, webhooks, and event outbox | COMMON · DEMOCRATIZE | Target | Stable versioned APIs, idempotency, service authentication, signed webhooks/events, retries, dead-letter handling, and correlation IDs. |
| LO-FND-012 | RecordArr evidence references | FOUNDATION | Target | Files and evidence are stored once in RecordArr and referenced by owning records; products do not create shadow document vaults. |
| LO-FND-013 | ReportArr projections and metrics | FOUNDATION | Target | Operational products emit facts/events; ReportArr builds read models and reports without becoming the source of operational truth. |
| LO-FND-014 | Compliance Core applicability and gates | FOUNDATION | Target | Products request explainable requirements, missing facts, evidence needs, and gate decisions rather than embedding independent legal logic. |
| LO-FND-015 | Field Companion execution surface | FOUNDATION · UNDERSERVED | Target | Mobile-first task, scan, capture, acknowledgement, and offline execution against the owning product API. |
| LO-FND-016 | Professional print and report view | COMMON | Target | Print output is a purpose-built report document without the app shell, not a screenshot of the current page. |
| LO-FND-017 | Accessible error and degraded states | FOUNDATION | Target | Explain what failed, what remains saved, whether retry is safe, what the user can do, and which administrator or integration owns the issue. |
| LO-FND-018 | Time, locale, and units | FOUNDATION | Target | Store canonical timestamps and units; display tenant/user locale, timezone, currency, and measurement preferences consistently. |
| LO-FND-019 | Retention, privacy, and legal hold awareness | FOUNDATION | Target | Product deletion/archive actions respect RecordArr retention, legal holds, privacy policy, and downstream references. |
| LO-FND-020 | AI-assisted proposals, never silent commits | UNDERSERVED | Target | AI may classify, summarize, suggest, or draft; consequential changes remain reviewable, permissioned, attributable, and reversible. |

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
<summary>Persistent entity sets (2)</summary>

| Entity type | DbSet/property | Source |
| --- | --- | --- |
| LoadArrTenantSettings | LoadArrTenantSettings | LoadArr.Api/Data/LoadArrDbContext.cs |
| LoadArrTenantSettingAuditEntry | LoadArrTenantSettingAuditEntries | LoadArr.Api/Data/LoadArrDbContext.cs |

</details>

<details>
<summary>Frontend page files (1)</summary>

| Page |
| --- |
| src/LaunchPage.tsx |

</details>

<details>
<summary>Endpoint source families (8)</summary>

| Endpoint source file | Discovered route declarations |
| --- | --- |
| LoadArrRouteSurfaceEndpoints.cs | 49 |
| LoadArrIntegrationEndpoints.cs | 39 |
| LoadArrWorkspaceEndpoints.cs | 33 |
| LoadArrInventoryManagementEndpoints.cs | 27 |
| LoadArrTenantSettingsEndpoints.cs | 9 |
| AuthEndpoints.cs | 6 |
| FieldInboxEndpoints.cs | 1 |
| LoadArrAdminEndpoints.cs | 1 |

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

Replace static/scaffold operational surfaces with a durable WMS domain; migrate inventory balances, stock ledger, receiving, putaway, and movement truth from SupplyArr.

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
