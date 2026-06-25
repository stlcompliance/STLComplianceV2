# RoutArr — TMS Full Feature Set

[Workflow catalog](./WORKFLOWS.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md)

## Document control

| Field | Value |
| --- | --- |
| Product | RoutArr (TMS) |
| Category | Transportation Management System |
| Repository maturity | Durable |
| Audit basis | Static repository inspection on 2026-06-24 |
| Cataloged capabilities | 73 |
| Cataloged workflows | 15 |

## Product charter

RoutArr owns transportation demand, planning, routing, carrier/tender decisions, dispatch, routes, trips, stops, movement execution, transportation visibility, proof, transportation exceptions, yard/gate events, claims context, detention/accessorial context, and finance contribution packets. It consumes orders, shipments, inventory readiness, asset readiness, drivers, qualifications, customer/supplier context, and documents from the products that own those truths.

> **Implementation reality — Durable:** RoutArr contains persistent dispatch, trip, route, stop, availability, time, proof, exception, DVIR, attachment, completion-rollup, and broad advanced TMS models: transportation demand, routing guides, tenders, ratings/accessorials, visibility/tracking, planning scenarios/suggestions, capacity snapshots, yard events, portal collaboration, claims, document packets, appointment clocks, multimodal requirements, and finance contributions. Some advanced capabilities are domain-complete models awaiting deeper execution, carrier connectivity, optimization, and settlement integrations.

## Source-of-truth boundary

### RoutArr owns

- Transportation demand and source references, requirements, service levels, mode-specific requirements, and planning state.
- Routing guides, carrier tenders, freight ratings, accessorials, planning scenarios/suggestions, and capacity snapshots.
- Dispatch plans, routes, trips, loads, stops, driver/equipment availability snapshots, dispatch blockers, releases, and messages.
- Trip execution, check-in/out, proof, DVIR context, capture attachments, transportation visibility events, tracking snapshots, and exceptions.
- Yard, gate, trailer, drop/hook events; dock appointment and detention clocks; transportation claims and document packet requests.
- Transportation finance packet contributions and completion rollups, not final accounting truth.
- Transportation portals and tenant TMS settings/audit.

### RoutArr does not own

- Order lifecycle and customer promise; OrdArr owns order coordination while CustomArr owns customer truth.
- Warehouse inventory, pick/pack/staging/shipping truth; LoadArr owns warehouse execution.
- Asset master, defects, maintenance, or readiness; MaintainArr owns them.
- People, employment, locations, permissions, or qualifications; StaffArr/TrainArr own them.
- Supplier/vendor commercial truth and purchase orders; SupplyArr owns them.
- Invoices, payments, GL, or final settlement; LedgArr or external finance owns them.
- File binaries; RecordArr owns proof and document files.

## Suite access and authorization

- All normal tenant users may launch every product application in the suite; product actions and records remain permission-scoped server-side.
- Fixed-suite access is nonvariable. Deployment health, feature readiness, legal restrictions, external-provider setup, and permissions may still constrain a capability.
- Platform administration remains a NexArr-only responsibility and is not a tenant product surface.
- Compliance Core administrative/authoring UI is restricted, while Compliance Core runtime evaluations, fact requirements, evidence needs, and workflow gates serve every tenant and product.

## Primary users

- Transportation planner
- Dispatcher
- Driver
- Carrier dispatcher
- Yard/gate coordinator
- Dock coordinator
- Customer service user
- Claims coordinator
- Transportation finance reviewer
- Auditor

## Required integrations

- OrdArr
- CustomArr
- SupplyArr
- LoadArr
- MaintainArr
- StaffArr
- TrainArr
- AssurArr
- RecordArr
- Compliance Core
- ReportArr
- LedgArr
- Field Companion
- Carrier/rating/telematics/map/weather providers

## Product principles

- Transportation demand exists before and independently of a route or trip.
- Optimization produces explainable proposals; dispatchers retain accountable approval for operational changes.
- Visibility events preserve source, event time, received time, confidence, and correction history.
- RoutArr contributes financial facts but does not own invoices, payments, or the general ledger.

## Repository implementation snapshot

| Static indicator | Count |
| --- | --- |
| Persistent DbSet declarations | 62 |
| Discovered server classes | 543 |
| Discovered HTTP route declarations | 266 |
| Frontend source files | 138 |
| Frontend page files | 23 |
| Documentation headings | 135 |

### Evidence used for the current-state classification

- Persistent trips, trip loads, routes, dispatch plans, route stops, driver/equipment availability, driver time, audit, notifications, trip-execution settings, completion rollups/events/runs, and dispatch board state.
- Persistent exceptions, messages, release snapshots, blocks, vendor-order event receipts, proof records, DVIR inspections, capture attachments, retention jobs, audit packages, and integration outbox.
- Persistent TransportationDemands/lines/requirements/source refs, CarrierTenders, RoutingGuideSteps, FreightRatings, FreightAccessorials, visibility events/tracking snapshots, planning scenarios/suggestions, capacity snapshots, yard events, portal collaboration, claims, document packets, appointment clocks, mode-specific refs, and finance packet contributions.
- Persistent tenant settings/values/lists/overrides/audit entries for RoutArr.
- routarr-frontend routes for dashboard, dispatch board/plans, demands, route planner, driver/customer portals, trips/routes/stops/exceptions/reports, proof review, dock appointments, load visibility, blockers, availability, calendar, and settings.

> Counts are static-discovery indicators, not proof that every route or screen is complete, reachable, secure, migrated, or production-ready.

## Feature catalog

### A. Currently implemented or meaningfully represented

These capabilities have repository evidence. Their state follows the product-level maturity and may still require hardening or completion.

| Feature ID | Capability | Class | State | Required behavior / evidence |
| --- | --- | --- | --- | --- |
| RO-CUR-001 | Transportation demand registry | CURRENT | Durable | Durable demands, lines, requirements, and source refs separate transportation need from an already-created trip. |
| RO-CUR-002 | Dispatch plans, routes, trips, loads, and stops | CURRENT | Durable | Core planning/execution records and relationships are persistent. |
| RO-CUR-003 | Driver and equipment availability | CURRENT | Durable | Availability records, capacity snapshots, StaffArr person refs, and MaintainArr vehicle refs support planning. |
| RO-CUR-004 | Dispatch board and release controls | CURRENT | Durable | Board state, release snapshots, blocks, exceptions, messages, and notifications support execution control. |
| RO-CUR-005 | Trip execution settings and timeline | CURRENT | Durable | Tenant policies, trip events, proof, DVIR, attachments, and completion rollups are durable. |
| RO-CUR-006 | Routing guides and carrier tenders | CURRENT | Durable | Guide steps and tender records support contracted routing and carrier response workflows. |
| RO-CUR-007 | Freight rating and accessorials | CURRENT | Durable | Rating and accessorial records can preserve quoted/planned/actual cost context and reasons. |
| RO-CUR-008 | Visibility events and tracking snapshots | CURRENT | Durable | Normalized event and snapshot models support telematics, carrier, portal, and manual status sources. |
| RO-CUR-009 | Planning scenarios and suggestions | CURRENT | Durable | Scenario/suggestion entities support human-reviewed optimization rather than silent plan replacement. |
| RO-CUR-010 | Yard, gate, trailer, and drop/hook events | CURRENT | Durable | Yard event models support arrival/departure, custody, trailer status, and gate execution. |
| RO-CUR-011 | Dock appointment and detention clocks | CURRENT | Durable | Appointment-clock records support SLA, wait, detention, and accessorial evidence. |
| RO-CUR-012 | Customer/driver portal collaboration | CURRENT | Durable | Portal submission records and dedicated routes support scoped external collaboration. |
| RO-CUR-013 | Transportation claims and document packets | CURRENT | Durable | Claims and packet-request models support damage/loss and transport-document assembly. |
| RO-CUR-014 | Multimodal requirement references | CURRENT | Durable | Mode-specific requirements can be attached without forcing all transport into truckload assumptions. |
| RO-CUR-015 | Transportation finance contributions | CURRENT | Durable | RoutArr can produce rated/actual/accessorial context for OrdArr/LedgArr without owning the ledger. |
| RO-CUR-016 | SupplyArr vendor-order/shipment intent coordination | CURRENT | Durable | Shipment intents and vendor-order event receipts support procurement-to-transport handoff. |
| RO-CUR-017 | Integration events, retention, notifications, audit, and reports | CURRENT | Durable | Outbox, attachment retention, notification dispatch, audit packages, and frontend report routes are represented. |

### B. Common category baseline

These are expected for a credible Transportation Management System product. Where they overlap current behavior, the target definition is the acceptance standard rather than a duplicate feature.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| RO-COM-001 | Demand consolidation and load building | COMMON | Target | Combine compatible orders/shipments by origin, destination, time, capacity, service, temperature/hazmat, and handling constraints. |
| RO-COM-002 | Rate shopping and routing guide | COMMON | Target | Compare contract/spot rates, service, transit, capacity, accessorials, risk, and routing-guide waterfall. |
| RO-COM-003 | Carrier tendering | COMMON | Target | Sequential/broadcast/auction tender, response deadlines, counteroffer, reason codes, and fallback. |
| RO-COM-004 | Route planning and optimization | COMMON | Target | Stops, windows, service times, capacity, breaks/HOS, vehicle restrictions, traffic, distance, cost, and explainable constraints. |
| RO-COM-005 | Dispatch and driver communication | COMMON | Target | Assignment, release, acknowledgement, navigation context, changes, messages, and exception escalation. |
| RO-COM-006 | Shipment visibility and ETA | COMMON | Target | Carrier/telematics/manual events, geofence, milestones, ETA confidence, stale tracking, and customer-safe updates. |
| RO-COM-007 | Proof and transport documents | COMMON | Target | BOL, POD, signatures, photos, seal, temperature, damage, lumper, and packet completeness. |
| RO-COM-008 | Dock and yard coordination | COMMON | Target | Appointments, check-in, gate, door, trailer, drop/hook, dwell, detention, and warehouse notifications. |
| RO-COM-009 | Exception management | COMMON | Target | Late, refused, damaged, shortage, no-show, breakdown, weather, route deviation, temperature, and documentation exceptions. |
| RO-COM-010 | Claims management | COMMON | Target | Notice, affected goods, evidence, liability, carrier response, reserve/recovery context, and closure. |
| RO-COM-011 | Freight audit and settlement contribution | COMMON | Target | Compare tender/rate/accessorial/actual, validate evidence, dispute variance, and send approved payable/charge context to finance. |
| RO-COM-012 | Driver/equipment compliance and readiness | COMMON | Target | Identity, license/qualification, HOS/capacity, vehicle readiness, DVIR, permits, insurance, and assignment gates. |
| RO-COM-013 | Customer and carrier portals | COMMON | Target | Scoped status, tender response, appointments, documents, proof, exceptions, and messaging. |
| RO-COM-014 | Transportation analytics | COMMON | Target | On-time pickup/delivery, cost/mile/order, tender acceptance, dwell/detention, utilization, empty miles, claims, and ETA accuracy. |

### C. Commonly requested but widely underserved

These address recurring user friction, fragmented add-ons, poor transparency, inaccessible setup, or weak SMB usability.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| RO-UND-001 | Affordable optimization with understandable tradeoffs | UNDERSERVED | Target | Small fleets can compare cost, miles, service, overtime, risk, and customer impact without a black-box enterprise optimizer. |
| RO-UND-002 | One operations board across owned fleet and carriers | UNDERSERVED | Target | Plan private fleet, contracted carriers, couriers, owner-operators, and customer pickup without separate silos. |
| RO-UND-003 | End-to-end custody timeline | UNDERSERVED | Target | Connect supplier ready, pickup, gate, load, in-transit, dock, unload, staging, delivery, and proof with source/confidence. |
| RO-UND-004 | Low-connectivity driver execution | UNDERSERVED | Target | Trips, stops, documents, scans, signatures, photos, messages, and exceptions remain usable offline with visible sync state. |
| RO-UND-005 | Explainable ETA and promise risk | UNDERSERVED | Target | Show source freshness, confidence, assumptions, traffic/appointment/service impacts, and why an ETA changed. |
| RO-UND-006 | No-code routing and tender policies | UNDERSERVED | Target | Operations users can author and test routing-guide, service, capacity, customer, and exception rules with simulations and versioning. |
| RO-UND-007 | Small-carrier portal without EDI burden | UNDERSERVED | Target | Tender, status, documents, invoices/accessorial evidence, and claims through scoped web/mobile flows and email links. |
| RO-UND-008 | Driver-centered workflow | UNDERSERVED | Target | Minimum taps, large controls, voice support, glove mode, safe-driving lockouts, preloaded context, and no duplicate office data entry. |
| RO-UND-009 | True dock/warehouse/transport collaboration | UNDERSERVED | Target | RoutArr and LoadArr share appointments, ETA, door readiness, loading state, seal/proof, and delay reasons without changing ownership. |
| RO-UND-010 | Transparent accessorial and detention evidence | UNDERSERVED | Target | Automatic clocks plus human review, contractual thresholds, cause attribution, photos/documents, and dispute history. |
| RO-UND-011 | Customer self-service exception choices | UNDERSERVED | Target | Customers can approve reschedule, alternate delivery, partial, hold, pickup, or contact changes within policy and audit. |
| RO-UND-012 | Multimodal continuity for small shippers | UNDERSERVED | Target | Truck, LTL, parcel, rail, ocean, air, courier, and intermodal legs share one demand and milestone chain. |

### D. Advanced capabilities to democratize

These are commonly found only in enterprise tiers, specialist products, or expensive implementations. STL should make them practical without weakening governance.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| RO-DEM-001 | Network and scenario optimization | DEMOCRATIZE | Target | Model depots, routes, carriers, modes, consolidation, service levels, cost, capacity, emissions, and disruption scenarios. |
| RO-DEM-002 | Real-time dynamic replanning | DEMOCRATIZE | Target | Suggest safe route/stop/driver/equipment changes as conditions shift, with impact preview and dispatcher approval. |
| RO-DEM-003 | Carrier performance and procurement intelligence | DEMOCRATIZE | Target | Lane/service scorecards, acceptance, claims, tender behavior, capacity reliability, and total landed transport cost. |
| RO-DEM-004 | Predictive ETA and exception risk | DEMOCRATIZE | Target | Use historical dwell, route, weather, traffic, carrier, facility, and live signals with confidence and reason codes. |
| RO-DEM-005 | Freight marketplace and shared capacity | DEMOCRATIZE | Target | Controlled spot requests, trusted carrier network, quote comparison, insurance/authority checks, and fraud safeguards. |
| RO-DEM-006 | Transportation control tower | DEMOCRATIZE | Target | Cross-tenant-safe, multi-site global view of demand, in-transit risk, inventory/order impact, capacity, and response actions. |
| RO-DEM-007 | Carbon and alternative-energy planning | DEMOCRATIZE | Target | Estimate emissions, compare modes, EV range/charging, idle/dwell, and service/cost tradeoffs without greenwashing. |
| RO-DEM-008 | Autonomous/robotic handoff readiness | DEMOCRATIZE | Target | Standardized dispatch, yard, proof, exception, and safety contracts for drones, robots, and autonomous vehicles while retaining human control. |
| RO-DEM-009 | Automated freight audit and dispute | DEMOCRATIZE | Target | Contract-aware invoice comparison, duplicate detection, accessorial evidence, dispute workflow, and finance posting proposal. |
| RO-DEM-010 | Digital transportation document orchestration | DEMOCRATIZE | Target | Generate, exchange, sign, validate, and package BOL/eBOL, manifests, customs/permit docs, POD, and claims evidence. |

### E. Suite-wide foundation required in RoutArr

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| RO-FND-001 | Tenant-scoped authorization | FOUNDATION | Target | Every read and write is tenant-scoped server-side; product UI visibility never substitutes for authorization. |
| RO-FND-002 | StaffArr-backed action permissions | FOUNDATION | Target | Use product action keys, role scopes, delegated authority, and explicit denial reasons; ownership does not prevent permissioned delegated workflows. |
| RO-FND-003 | Unified suite shell | FOUNDATION | Target | Consistent navigation, page anatomy, terminology, responsive behavior, keyboard access, and light/dark contrast across products. |
| RO-FND-004 | Record lifecycle and history | FOUNDATION | Target | Human-readable statuses, timeline events, actor/time/source attribution, undo or correction paths, and immutable audit evidence. |
| RO-FND-005 | Quick create for missing references | FOUNDATION · UNDERSERVED | Target | Create the minimum valid cross-product reference without abandoning the current task; preserve context and allow later backfill. |
| RO-FND-006 | Saved views and personal preferences | COMMON | Target | Per-user filters, columns, density, sort, board/list preference, default landing view, and product-scoped preferences. |
| RO-FND-007 | Bulk operations with preview | COMMON | Target | Selection-aware actions, dry-run/impact preview, partial failure handling, downloadable results, and permission checks per record. |
| RO-FND-008 | Import with mapping and review | COMMON · UNDERSERVED | Target | Product-specific import separate from Smart Import, with templates, mappings, validation, dedupe, dry run, commit plan, and rollback/correction evidence. |
| RO-FND-009 | Export and portability | COMMON · DEMOCRATIZE | Target | CSV/PDF/JSON or package export appropriate to the domain, without trapping customer data behind enterprise tiers. |
| RO-FND-010 | Notifications, tasks, and inbox | COMMON | Target | Actionable notifications with ownership, due dates, escalation, snooze/delegation, deep links, and duplicate suppression. |
| RO-FND-011 | API, webhooks, and event outbox | COMMON · DEMOCRATIZE | Target | Stable versioned APIs, idempotency, service authentication, signed webhooks/events, retries, dead-letter handling, and correlation IDs. |
| RO-FND-012 | RecordArr evidence references | FOUNDATION | Target | Files and evidence are stored once in RecordArr and referenced by owning records; products do not create shadow document vaults. |
| RO-FND-013 | ReportArr projections and metrics | FOUNDATION | Target | Operational products emit facts/events; ReportArr builds read models and reports without becoming the source of operational truth. |
| RO-FND-014 | Compliance Core applicability and gates | FOUNDATION | Target | Products request explainable requirements, missing facts, evidence needs, and gate decisions rather than embedding independent legal logic. |
| RO-FND-015 | Field Companion execution surface | FOUNDATION · UNDERSERVED | Target | Mobile-first task, scan, capture, acknowledgement, and offline execution against the owning product API. |
| RO-FND-016 | Professional print and report view | COMMON | Target | Print output is a purpose-built report document without the app shell, not a screenshot of the current page. |
| RO-FND-017 | Accessible error and degraded states | FOUNDATION | Target | Explain what failed, what remains saved, whether retry is safe, what the user can do, and which administrator or integration owns the issue. |
| RO-FND-018 | Time, locale, and units | FOUNDATION | Target | Store canonical timestamps and units; display tenant/user locale, timezone, currency, and measurement preferences consistently. |
| RO-FND-019 | Retention, privacy, and legal hold awareness | FOUNDATION | Target | Product deletion/archive actions respect RecordArr retention, legal holds, privacy policy, and downstream references. |
| RO-FND-020 | AI-assisted proposals, never silent commits | UNDERSERVED | Target | AI may classify, summarize, suggest, or draft; consequential changes remain reviewable, permissioned, attributable, and reversible. |

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
<summary>Persistent entity sets (62)</summary>

| Entity type | DbSet/property | Source |
| --- | --- | --- |
| Trip | Trips | RoutArr.Api/Data/RoutArrDbContext.cs |
| TripLoad | TripLoads | RoutArr.Api/Data/RoutArrDbContext.cs |
| TripPartsDemandLine | TripPartsDemandLines | RoutArr.Api/Data/RoutArrDbContext.cs |
| TripPartsDemandStatusEvent | TripPartsDemandStatusEvents | RoutArr.Api/Data/RoutArrDbContext.cs |
| SupplyArrShipmentIntent | SupplyArrShipmentIntents | RoutArr.Api/Data/RoutArrDbContext.cs |
| SupplyArrShipmentIntentLine | SupplyArrShipmentIntentLines | RoutArr.Api/Data/RoutArrDbContext.cs |
| DispatchRoute | Routes | RoutArr.Api/Data/RoutArrDbContext.cs |
| DispatchPlan | DispatchPlans | RoutArr.Api/Data/RoutArrDbContext.cs |
| RouteStop | RouteStops | RoutArr.Api/Data/RoutArrDbContext.cs |
| DriverAvailability | DriverAvailabilities | RoutArr.Api/Data/RoutArrDbContext.cs |
| EquipmentAvailability | EquipmentAvailabilities | RoutArr.Api/Data/RoutArrDbContext.cs |
| DriverTimeEntry | DriverTimeEntries | RoutArr.Api/Data/RoutArrDbContext.cs |
| RoutArrAuditEvent | AuditEvents | RoutArr.Api/Data/RoutArrDbContext.cs |
| TenantDispatchNotificationSettings | TenantDispatchNotificationSettings | RoutArr.Api/Data/RoutArrDbContext.cs |
| TenantTripExecutionSettings | TenantTripExecutionSettings | RoutArr.Api/Data/RoutArrDbContext.cs |
| DispatchNotificationDispatch | DispatchNotificationDispatches | RoutArr.Api/Data/RoutArrDbContext.cs |
| TenantTripCompletionRollupSettings | TenantTripCompletionRollupSettings | RoutArr.Api/Data/RoutArrDbContext.cs |
| TripCompletionRollup | TripCompletionRollups | RoutArr.Api/Data/RoutArrDbContext.cs |
| TripCompletionEvent | TripCompletionEvents | RoutArr.Api/Data/RoutArrDbContext.cs |
| TripCompletionRollupRun | TripCompletionRollupRuns | RoutArr.Api/Data/RoutArrDbContext.cs |
| TenantDispatchBoardState | TenantDispatchBoardStates | RoutArr.Api/Data/RoutArrDbContext.cs |
| StaffarrPersonRef | StaffarrPersonRefs | RoutArr.Api/Data/RoutArrDbContext.cs |
| RoutarrVehicleRef | RoutarrVehicleRefs | RoutArr.Api/Data/RoutArrDbContext.cs |
| DispatchException | DispatchExceptions | RoutArr.Api/Data/RoutArrDbContext.cs |
| DispatchMessage | DispatchMessages | RoutArr.Api/Data/RoutArrDbContext.cs |
| TripDispatchReleaseSnapshot | TripDispatchReleaseSnapshots | RoutArr.Api/Data/RoutArrDbContext.cs |
| DispatchBlock | DispatchBlocks | RoutArr.Api/Data/RoutArrDbContext.cs |
| SupplyArrVendorOrderEventReceipt | SupplyArrVendorOrderEventReceipts | RoutArr.Api/Data/RoutArrDbContext.cs |
| TripProofRecord | TripProofRecords | RoutArr.Api/Data/RoutArrDbContext.cs |
| TripDvirInspection | TripDvirInspections | RoutArr.Api/Data/RoutArrDbContext.cs |
| TripCaptureAttachment | TripCaptureAttachments | RoutArr.Api/Data/RoutArrDbContext.cs |
| TenantAttachmentRetentionSettings | TenantAttachmentRetentionSettings | RoutArr.Api/Data/RoutArrDbContext.cs |
| AttachmentRetentionRun | AttachmentRetentionRuns | RoutArr.Api/Data/RoutArrDbContext.cs |
| AuditPackageGenerationJob | AuditPackageGenerationJobs | RoutArr.Api/Data/RoutArrDbContext.cs |
| TenantIntegrationEventSettings | TenantIntegrationEventSettings | RoutArr.Api/Data/RoutArrDbContext.cs |
| IntegrationOutboxEvent | IntegrationOutboxEvents | RoutArr.Api/Data/RoutArrDbContext.cs |
| TransportationDemand | TransportationDemands | RoutArr.Api/Data/RoutArrDbContext.cs |
| TransportationDemandLine | TransportationDemandLines | RoutArr.Api/Data/RoutArrDbContext.cs |
| TransportationDemandRequirement | TransportationDemandRequirements | RoutArr.Api/Data/RoutArrDbContext.cs |
| TransportationDemandSourceRef | TransportationDemandSourceRefs | RoutArr.Api/Data/RoutArrDbContext.cs |
| CarrierTender | CarrierTenders | RoutArr.Api/Data/RoutArrDbContext.cs |
| RoutingGuideStep | RoutingGuideSteps | RoutArr.Api/Data/RoutArrDbContext.cs |
| FreightRating | FreightRatings | RoutArr.Api/Data/RoutArrDbContext.cs |
| FreightAccessorial | FreightAccessorials | RoutArr.Api/Data/RoutArrDbContext.cs |
| TransportationVisibilityEvent | TransportationVisibilityEvents | RoutArr.Api/Data/RoutArrDbContext.cs |
| TransportationTrackingSnapshot | TransportationTrackingSnapshots | RoutArr.Api/Data/RoutArrDbContext.cs |
| TransportationPlanningScenario | TransportationPlanningScenarios | RoutArr.Api/Data/RoutArrDbContext.cs |
| TransportationPlanningSuggestion | TransportationPlanningSuggestions | RoutArr.Api/Data/RoutArrDbContext.cs |
| DriverCapacitySnapshot | DriverCapacitySnapshots | RoutArr.Api/Data/RoutArrDbContext.cs |
| TransportationYardEvent | TransportationYardEvents | RoutArr.Api/Data/RoutArrDbContext.cs |
| PortalCollaborationSubmission | PortalCollaborationSubmissions | RoutArr.Api/Data/RoutArrDbContext.cs |
| FreightClaim | FreightClaims | RoutArr.Api/Data/RoutArrDbContext.cs |
| TransportationDocumentPacketRequest | TransportationDocumentPacketRequests | RoutArr.Api/Data/RoutArrDbContext.cs |
| TransportationAppointmentClock | TransportationAppointmentClocks | RoutArr.Api/Data/RoutArrDbContext.cs |
| ModeSpecificRequirementRef | ModeSpecificRequirementRefs | RoutArr.Api/Data/RoutArrDbContext.cs |
| TransportationFinancePacketContribution | TransportationFinancePacketContributions | RoutArr.Api/Data/RoutArrDbContext.cs |
| RoutArrTenantSettings | RoutArrTenantSettings | RoutArr.Api/Data/RoutArrDbContext.cs |
| RoutArrTenantSettingValue | RoutArrTenantSettingValues | RoutArr.Api/Data/RoutArrDbContext.cs |
| RoutArrTenantSettingListItem | RoutArrTenantSettingListItems | RoutArr.Api/Data/RoutArrDbContext.cs |
| RoutArrTenantSettingOverride | RoutArrTenantSettingOverrides | RoutArr.Api/Data/RoutArrDbContext.cs |
| RoutArrTenantSettingOverrideListItem | RoutArrTenantSettingOverrideListItems | RoutArr.Api/Data/RoutArrDbContext.cs |
| RoutArrTenantSettingAuditEntry | RoutArrTenantSettingAuditEntries | RoutArr.Api/Data/RoutArrDbContext.cs |

</details>

<details>
<summary>Frontend page files (23)</summary>

| Page |
| --- |
| src/lib/createWorkspacePage.tsx |
| src/pages/LaunchPage.tsx |
| src/pages/TripWorkspacePage.tsx |
| src/workspace/RoutArrWorkspacePage.tsx |
| src/pages/availability/AvailabilityPage.tsx |
| src/pages/calendar/CalendarPage.tsx |
| src/pages/customer-portal/CustomerPortalPage.tsx |
| src/pages/dashboard/DashboardPage.tsx |
| src/pages/dispatch-plans/DispatchPlansPage.tsx |
| src/pages/dispatch/DispatchPage.tsx |
| src/pages/dock-appointments/DockAppointmentsPage.tsx |
| src/pages/driver-portal/DriverPortalPage.tsx |
| src/pages/exceptions/ExceptionsPage.tsx |
| src/pages/load-visibility/LoadVisibilityPage.tsx |
| src/pages/proof-review/ProofReviewPage.tsx |
| src/pages/reports/ReportsPage.tsx |
| src/pages/route-planner/RoutePlannerPage.tsx |
| src/pages/routes/RoutesPage.tsx |
| src/pages/settings/SettingsPage.tsx |
| src/pages/stops/StopsPage.tsx |
| src/pages/transportation-demands/TransportationDemandsPage.tsx |
| src/pages/trips/TripsPage.tsx |
| src/pages/validation-blockers/ValidationBlockersPage.tsx |

</details>

<details>
<summary>Endpoint source families (44)</summary>

| Endpoint source file | Discovered route declarations |
| --- | --- |
| DispatchEndpoints.cs | 29 |
| TmsRuntimeEndpoints.cs | 28 |
| IntegrationResourceEndpoints.cs | 26 |
| V1FeatureAliasEndpoints.cs | 26 |
| DriverPortalEndpoints.cs | 11 |
| RoutArrTenantSettingsEndpoints.cs | 11 |
| RouteEndpoints.cs | 11 |
| AuditPackageEndpoints.cs | 8 |
| AuthEndpoints.cs | 8 |
| TripEndpoints.cs | 8 |
| DispatchReportEndpoints.cs | 7 |
| ProofDvirReportEndpoints.cs | 7 |
| TripProofDvirEndpoints.cs | 6 |
| DispatchMessageEndpoints.cs | 5 |
| DriverAvailabilityEndpoints.cs | 5 |
| EquipmentAvailabilityEndpoints.cs | 5 |
| EntityExportEndpoints.cs | 4 |
| IntegrationValidationEndpoints.cs | 4 |
| RouteReportEndpoints.cs | 4 |
| TripCompletionRollupSettingsEndpoints.cs | 4 |
| AttachmentRetentionSettingsEndpoints.cs | 3 |
| DriverPortalTimeTrackingEndpoints.cs | 3 |
| IntegrationEndpoints.cs | 3 |
| IntegrationEventSettingsEndpoints.cs | 3 |
| NotificationSettingsEndpoints.cs | 3 |
| TripCompletionEndpoints.cs | 3 |
| TripExecutionCaptureEndpoints.cs | 3 |
| TripPartsDemandEndpoints.cs | 3 |
| DispatchOverrideReportEndpoints.cs | 2 |
| DriverEndpoints.cs | 2 |
| EventAndAuditEndpoints.cs | 2 |
| InternalAttachmentRetentionEndpoints.cs | 2 |
| InternalAuditPackageGenerationEndpoints.cs | 2 |
| InternalDispatchNotificationEndpoints.cs | 2 |
| InternalIntegrationEventEndpoints.cs | 2 |
| InternalTripCompletionRollupEndpoints.cs | 2 |
| VehicleRefEndpoints.cs | 2 |
| AssetDispatchabilityEndpoints.cs | 1 |
| DispatchWorkflowGateEndpoints.cs | 1 |
| DockAppointmentEndpoints.cs | 1 |
| DriverEligibilityEndpoints.cs | 1 |
| FieldInboxEndpoints.cs | 1 |
| LoadVisibilityEndpoints.cs | 1 |
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

Complete planning/tender/rating/visibility/claims/settlement integrations and keep transportation demand/execution distinct from orders and warehouse custody.

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
