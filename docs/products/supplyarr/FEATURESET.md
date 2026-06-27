# SupplyArr — SRM Full Feature Set

[Workflow catalog](./WORKFLOWS.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md) · [Rollout](./ROLLOUT.md)

## Document control

| Field | Value |
| --- | --- |
| Product | SupplyArr (SRM) |
| Category | Supplier Relationship Management and Procurement |
| Repository maturity | Durable |
| Audit basis | Static repository inspection on 2026-06-24 |
| Cataloged capabilities | 72 |
| Cataloged workflows | 16 |

## Product charter

SupplyArr is the tenant system of record for suppliers/vendors/dealers, sourced item/part commercial context, supplier onboarding and restrictions, sourcing events, purchase requests, approvals, RFQs, quotes, contracts, purchase orders, vendor acknowledgements, procurement exceptions, returns/warranty claims, supplier performance, and procurement coordination. LoadArr owns all physical inventory and warehouse movements.

> **Implementation reality — Durable:** SupplyArr has broad persistent supplier, onboarding, restriction, incident, procurement, sourcing, item, RFQ, quote, PO, receipt-reference, exception, return, warranty, contract, price/lead-time/availability, demand, portal/email, notification, integration, and worker-run models. It also currently contains WMS-like location, stock, reservation, stock-ledger, and outbound-shipment tables that conflict with the settled LoadArr ownership boundary and should be migrated or retired rather than expanded.

## Source-of-truth boundary

### SupplyArr owns

- Supplier/vendor/external-party records in the procurement context, contacts, compliance documents, onboarding, restrictions, incidents, and commercial status.
- Part/item catalogs, internal commercial item identity, manufacturer aliases, sourcing relationships, approved vendor links, and commercial attributes.
- Purchase requests, approval authority context, RFQs, vendor invitations, quotes, quote lines, awards, vendor orders, purchase orders, and line lifecycle.
- Contracts, price/lead-time/availability snapshots, sourcing decisions, vendor acknowledgements, backorders, procurement exceptions, returns, and warranty claims.
- Demand references from MaintainArr, RoutArr, TrainArr, StaffArr, OrdArr, and other products when procurement is needed.
- Supplier-facing portal/email interactions, magic links, document links, broker/vendor decisions, procurement notifications, coordination, integrations, and performance context.

### SupplyArr does not own

- Warehouse locations, inventory balance, stock ledger, reservations, receiving execution, putaway, picking, staging, or shipping; LoadArr owns them.
- Internal site/location identity; StaffArr owns canonical internal locations.
- Quality nonconformance, hold/release, CAPA, or SCAR decision; AssurArr owns quality workflows while SupplyArr owns supplier commercial consequences.
- Customer account truth; CustomArr owns customers.
- Transportation dispatch/trips; RoutArr owns movement execution.
- Bills, payments, or general-ledger posting; LedgArr owns finance execution.

## Suite access and authorization

- All normal tenant users may launch every product application in the suite; product actions and records remain permission-scoped server-side.
- Fixed-suite access is nonvariable. Deployment health, feature readiness, legal restrictions, external-provider setup, and permissions may still constrain a capability.
- Platform administration remains a NexArr-only responsibility and is not a tenant product surface.
- Compliance Core administrative/authoring UI is restricted, while Compliance Core runtime evaluations, fact requirements, evidence needs, and workflow gates serve every tenant and product.

## Primary users

- Requester
- Buyer/procurement specialist
- Category manager
- Supplier relationship owner
- Approver/budget owner
- Supplier contact
- Risk/compliance reviewer
- Returns/warranty coordinator
- Auditor

## Required integrations

- StaffArr
- LoadArr
- RoutArr
- MaintainArr
- OrdArr
- TrainArr
- AssurArr
- RecordArr
- Compliance Core
- ReportArr
- LedgArr
- NexArr
- Supplier/catalog/risk/EDI providers

## Product principles

- SupplyArr owns commercial procurement truth; LoadArr owns physical inventory and receiving execution.
- Supplier status and restrictions are scoped, effective-dated, explainable, and never inferred from a single opaque risk score.
- Every purchase line retains source-demand allocation so operational requesters can see status and procurement can consolidate responsibly.
- Legacy WMS entities in SupplyArr are migration debt and must not become the production inventory source of truth.

## Repository implementation snapshot

| Static indicator | Count |
| --- | --- |
| Persistent DbSet declarations | 85 |
| Discovered server classes | 723 |
| Discovered HTTP route declarations | 423 |
| Frontend source files | 159 |
| Frontend page files | 29 |
| Documentation headings | 86 |

### Evidence used for the current-state classification

- Persistent ExternalParties, contacts, compliance documents, supplier onboardings/settings, restrictions, incidents, procurement exceptions, and StaffArr approval-authority mirrors.
- Persistent part catalogs/parts/manufacturer aliases/sources/vendor links, RFQs/lines/invitations, vendor quotes/lines, purchase requests/lines, vendor orders/status/magic links/documents/broker decisions, POs/lines, contracts, returns, warranty claims, backorders, and receipt/exception references.
- Persistent price, lead-time, and availability snapshots plus scheduled capture states/runs, procurement coordination, approval reminders, exception escalation, demand processing, integration inbox/outbox, vendor email intake, and notifications.
- Persistent demand refs/lines from MaintainArr, RoutArr, TrainArr, and StaffArr support cross-product procurement intake.
- supplyarr-frontend routes for dashboard, suppliers/parties/onboarding, RFQs/quotes, POs, catalog, contracts, documents, performance/risk/corrective actions, supplier portal, purchasing/approvals/exceptions/vendor orders/reports/settings/import/export.
- InventoryLocations, InventoryBins, PartStockLevels, PartStockReservations, WmsStockLedgerEntries, and WmsOutboundShipment tables are boundary debt under the current ownership constitution and should move to LoadArr.

> Counts are static-discovery indicators, not proof that every route or screen is complete, reachable, secure, migrated, or production-ready.

## Mandatory migration or refactor work

- Migrate WMS-owned stock, balance, ledger, and movement records to LoadArr and replace direct ownership with projections/API/event references.

## Feature catalog

### A. Currently implemented or meaningfully represented

These capabilities have repository evidence. Their state follows the product-level maturity and may still require hardening or completion.

| Feature ID | Capability | Class | State | Required behavior / evidence |
| --- | --- | --- | --- | --- |
| SU-CUR-001 | Supplier and external-party master | CURRENT | Durable | Supplier records, contacts, documents, onboarding, restrictions, incidents, and audit are durable. |
| SU-CUR-002 | Supplier onboarding and compliance documentation | CURRENT | Durable | Tenant onboarding settings, workflow records, documents, and external collaboration support controlled activation. |
| SU-CUR-003 | Supplier restrictions and procurement exceptions | CURRENT | Durable | Restriction and exception models support blocks, warnings, escalation, and review. |
| SU-CUR-004 | Part/item commercial catalog | CURRENT | Durable | Catalogs, parts, manufacturer aliases, item sources, and vendor relationships are persistent. |
| SU-CUR-005 | Purchase requests and demand intake | CURRENT | Durable | Purchase requests/lines plus MaintainArr, RoutArr, TrainArr, and StaffArr demand refs/lines support request origination. |
| SU-CUR-006 | Approval authority mirror and reminder workers | CURRENT | Durable | StaffArr authority context, reminder settings/states/runs, and escalation support procurement approval. |
| SU-CUR-007 | RFQ and vendor quote management | CURRENT | Durable | RFQs, lines, invitations, quotes, and quote lines are persistent. |
| SU-CUR-008 | Vendor orders, acknowledgements, and scoped magic links | CURRENT | Durable | Vendor order status updates, document links, magic links, broker decisions, and tenant settings support external status collection. |
| SU-CUR-009 | Purchase orders and line lifecycle | CURRENT | Durable | POs and lines are durable and integrated with receiving/procurement events. |
| SU-CUR-010 | Receiving references and procurement exceptions | CURRENT | Durable | Receipt/line/exception records mirror commercial receipt outcomes while LoadArr should own execution. |
| SU-CUR-011 | Backorders, vendor returns, and warranty claims | CURRENT | Durable | Reverse and unresolved procurement flows are modeled. |
| SU-CUR-012 | Supply contracts | CURRENT | Durable | Contracts and supplier commercial relationships are represented. |
| SU-CUR-013 | Price, lead-time, and availability history | CURRENT | Durable | Snapshots, capture states, schedules, and runs support supplier/item intelligence. |
| SU-CUR-014 | Procurement coordination and demand processing workers | CURRENT | Durable | Records/events/runs coordinate cross-product needs and status. |
| SU-CUR-015 | Supplier email/portal and integration event intake | CURRENT | Durable | Inbox/outbox, vendor email messages, event-processing runs, and notification dispatch are durable. |
| SU-CUR-016 | Legacy WMS tables in SupplyArr | CURRENT | Durable | Location, bin, stock, reservation, ledger, and outbound shipment models exist but should be Retire/Refactor into LoadArr. |

### B. Common category baseline

These are expected for a credible Supplier Relationship Management and Procurement product. Where they overlap current behavior, the target definition is the acceptance standard rather than a duplicate feature.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| SU-COM-001 | Supplier qualification and onboarding | COMMON | Target | Registration, tax/banking identity, contacts, insurance, certifications, risk, documents, capabilities, approvals, and activation. |
| SU-COM-002 | Supplier segmentation and ownership | COMMON | Target | Strategic/critical/preferred/approved/conditional/blocked status, category ownership, business units, and relationship plans. |
| SU-COM-003 | Item and source management | COMMON | Target | Item specifications, manufacturer/brand, UOM/package, approved sources, alternates, substitutions, MOQ, lead time, pricing, and constraints. |
| SU-COM-004 | Purchase request and approval | COMMON | Target | Demand capture, accounting/category context, justification, budget/authority checks, sourcing route, approvals, and conversion. |
| SU-COM-005 | RFx sourcing | COMMON | Target | RFI/RFQ/RFP, lots/lines, invited suppliers, clarifications, sealed bids where needed, evaluation, negotiation, and award. |
| SU-COM-006 | Quote comparison and total cost | COMMON | Target | Price, freight, duty, tax, lead time, MOQ, payment terms, quality, service, risk, and lifecycle cost normalization. |
| SU-COM-007 | Purchase order lifecycle | COMMON | Target | Draft, approval, issue, acknowledgement, change order, shipment, receipt, close, cancel, and revision history. |
| SU-COM-008 | Supplier portal | COMMON | Target | Onboarding, profile/docs, RFQ/quote, PO acknowledgement, shipment/readiness, invoices or finance refs, disputes, corrective actions, and performance. |
| SU-COM-009 | Contract management | COMMON | Target | Draft/ref, terms, pricing schedules, obligations, renewals, notice dates, spend linkage, and document control. |
| SU-COM-010 | Supplier risk and compliance | COMMON | Target | Financial, operational, cyber, geographic, sanctions, insurance, certification, quality, continuity, and dependency risk. |
| SU-COM-011 | Supplier performance | COMMON | Target | Delivery, quality, responsiveness, cost, lead-time accuracy, documentation, service, corrective action, and review cadence. |
| SU-COM-012 | Returns and warranty | COMMON | Target | Return authorization, shipment, receipt outcome, credit/replacement, warranty eligibility, claim, recovery, and closure. |
| SU-COM-013 | Procurement analytics | COMMON | Target | Spend, savings, cycle time, maverick spend, supplier concentration, lead time, price variance, on-time delivery, quality, and contract coverage. |
| SU-COM-014 | Integration and EDI/API | COMMON | Target | Catalog, PO, acknowledgement, ASN, receipt, invoice refs, status, and master-data exchange with mapping/reconciliation. |

### C. Commonly requested but widely underserved

These address recurring user friction, fragmented add-ons, poor transparency, inaccessible setup, or weak SMB usability.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| SU-UND-001 | Fee-free, low-friction supplier portal | UNDERSERVED | Target | Suppliers can collaborate without buying seats, learning enterprise software, or exposing unrelated tenant data. |
| SU-UND-002 | Small-business guided procurement | UNDERSERVED | Target | A simple request-to-order path preserves approval, sourcing, evidence, and audit without forcing every purchase through a strategic-sourcing project. |
| SU-UND-003 | One demand status across maintenance, warehouse, transport, training, and HR | UNDERSERVED | Target | Requesters see requested, approved, sourcing, ordered, acknowledged, shipped, received, issued/available, blocked, and canceled with owner/reason. |
| SU-UND-004 | Transparent supplier total cost | UNDERSERVED | Target | Normalize freight, minimums, packaging, duty, payment terms, quality, delay, returns, and risk rather than ranking only unit price. |
| SU-UND-005 | Collaborative supplier data correction | UNDERSERVED | Target | Suppliers propose changes to contacts, documents, lead times, substitutions, and identifiers; tenant reviewers approve without rekeying. |
| SU-UND-006 | Shared supplier compliance passport | UNDERSERVED | Target | Reuse verified documents and structured facts across tenant processes with consent, expiry, source, and no uncontrolled cross-tenant sharing. |
| SU-UND-007 | Affordable low-volume EDI/API bridge | UNDERSERVED | Target | Map PO/acknowledgement/ASN/invoice/status for small suppliers using portal, CSV, email, and APIs without enterprise transaction fees. |
| SU-UND-008 | Explainable supplier blocks | UNDERSERVED | Target | Users see the restriction owner, scope, reason category, effective period, remediation, and allowed exception path. |
| SU-UND-009 | Quick alternate and substitution workflow | UNDERSERVED | Target | Compare technical fit, approvals, customer/quality/compliance impact, availability, and price without free-text workarounds. |
| SU-UND-010 | Supplier collaboration around forecast and capacity | UNDERSERVED | Target | Share bounded demand signals, acknowledge capacity, flag constraints, and preserve version/commitment without requiring full planning software. |
| SU-UND-011 | Procurement exception workbench | UNDERSERVED | Target | Overdue approvals, price/quantity variance, unacknowledged PO, missed ship date, receipt mismatch, and document expiry appear in one owned queue. |
| SU-UND-012 | Local, diverse, and resilient source discovery | UNDERSERVED | Target | Track location, ownership/diversity attributes, capacity, single-source risk, and qualification while avoiding unsupported claims. |

### D. Advanced capabilities to democratize

These are commonly found only in enterprise tiers, specialist products, or expensive implementations. STL should make them practical without weakening governance.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| SU-DEM-001 | Strategic sourcing optimization | DEMOCRATIZE | Target | Model split awards, capacity, minimums, risk, lead time, quality, geography, diversity, and total cost with explainable scenarios. |
| SU-DEM-002 | Continuous third-party risk monitoring | DEMOCRATIZE | Target | Monitor approved data sources and operational performance, triage signals, require review, and avoid opaque automatic blocking. |
| SU-DEM-003 | Supplier network and discovery | DEMOCRATIZE | Target | Consent-based searchable capabilities, products, certifications, locations, and performance references without pay-to-play lock-in. |
| SU-DEM-004 | Contract intelligence | DEMOCRATIZE | Target | Extract obligations, pricing, renewal/notice dates, clauses, service levels, and risks from approved documents with citation and review. |
| SU-DEM-005 | Autonomous procurement proposals | DEMOCRATIZE | Target | Recommend consolidation, reorder, alternate source, negotiation, and approval routing while keeping commitments human-approved. |
| SU-DEM-006 | Should-cost and market intelligence | DEMOCRATIZE | Target | Compare historical prices, indices, specifications, volumes, packaging, freight, and supplier quotes with confidence/source. |
| SU-DEM-007 | Supplier development and joint action plans | DEMOCRATIZE | Target | Shared objectives, milestones, evidence, coaching, investment, risk reduction, and benefit tracking. |
| SU-DEM-008 | Multi-tier supply chain mapping | DEMOCRATIZE | Target | Represent sub-tier dependencies, sites, materials, geographic/concentration risk, and disruption impact for smaller companies. |
| SU-DEM-009 | Dynamic discounting and working-capital options | DEMOCRATIZE | Target | Offer transparent early-payment choices and finance contributions without requiring a separate enterprise network. |
| SU-DEM-010 | Procurement control tower | DEMOCRATIZE | Target | Cross-demand, supplier, contract, inventory, shipment, quality, and financial risk view with owned response tasks. |

### E. Suite-wide foundation required in SupplyArr

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| SU-FND-001 | Tenant-scoped authorization | FOUNDATION | Target | Every read and write is tenant-scoped server-side; product UI visibility never substitutes for authorization. |
| SU-FND-002 | StaffArr-backed action permissions | FOUNDATION | Target | Use product action keys, role scopes, delegated authority, and explicit denial reasons; ownership does not prevent permissioned delegated workflows. |
| SU-FND-003 | Unified suite shell | FOUNDATION | Target | Consistent navigation, page anatomy, terminology, responsive behavior, keyboard access, and light/dark contrast across products. |
| SU-FND-004 | Record lifecycle and history | FOUNDATION | Target | Human-readable statuses, timeline events, actor/time/source attribution, undo or correction paths, and immutable audit evidence. |
| SU-FND-005 | Quick create for missing references | FOUNDATION · UNDERSERVED | Target | Create the minimum valid cross-product reference without abandoning the current task; preserve context and allow later backfill. |
| SU-FND-006 | Saved views and personal preferences | COMMON | Target | Per-user filters, columns, density, sort, board/list preference, default landing view, and product-scoped preferences. |
| SU-FND-007 | Bulk operations with preview | COMMON | Target | Selection-aware actions, dry-run/impact preview, partial failure handling, downloadable results, and permission checks per record. |
| SU-FND-008 | Import with mapping and review | COMMON · UNDERSERVED | Target | Product-specific import separate from Smart Import, with templates, mappings, validation, dedupe, dry run, commit plan, and rollback/correction evidence. |
| SU-FND-009 | Export and portability | COMMON · DEMOCRATIZE | Target | CSV/PDF/JSON or package export appropriate to the domain, without trapping customer data behind enterprise tiers. |
| SU-FND-010 | Notifications, tasks, and inbox | COMMON | Target | Actionable notifications with ownership, due dates, escalation, snooze/delegation, deep links, and duplicate suppression. |
| SU-FND-011 | API, webhooks, and event outbox | COMMON · DEMOCRATIZE | Target | Stable versioned APIs, idempotency, service authentication, signed webhooks/events, retries, dead-letter handling, and correlation IDs. |
| SU-FND-012 | RecordArr evidence references | FOUNDATION | Target | Files and evidence are stored once in RecordArr and referenced by owning records; products do not create shadow document vaults. |
| SU-FND-013 | ReportArr projections and metrics | FOUNDATION | Target | Operational products emit facts/events; ReportArr builds read models and reports without becoming the source of operational truth. |
| SU-FND-014 | Compliance Core applicability and gates | FOUNDATION | Target | Products request explainable requirements, missing facts, evidence needs, and gate decisions rather than embedding independent legal logic. |
| SU-FND-015 | Field Companion execution surface | FOUNDATION · UNDERSERVED | Target | Mobile-first task, scan, capture, acknowledgement, and offline execution against the owning product API. |
| SU-FND-016 | Professional print and report view | COMMON | Target | Print output is a purpose-built report document without the app shell, not a screenshot of the current page. |
| SU-FND-017 | Accessible error and degraded states | FOUNDATION | Target | Explain what failed, what remains saved, whether retry is safe, what the user can do, and which administrator or integration owns the issue. |
| SU-FND-018 | Time, locale, and units | FOUNDATION | Target | Store canonical timestamps and units; display tenant/user locale, timezone, currency, and measurement preferences consistently. |
| SU-FND-019 | Retention, privacy, and legal hold awareness | FOUNDATION | Target | Product deletion/archive actions respect RecordArr retention, legal holds, privacy policy, and downstream references. |
| SU-FND-020 | AI-assisted proposals, never silent commits | UNDERSERVED | Target | AI may classify, summarize, suggest, or draft; consequential changes remain reviewable, permissioned, attributable, and reversible. |

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
<summary>Persistent entity sets (85)</summary>

| Entity type | DbSet/property | Source |
| --- | --- | --- |
| ExternalParty | ExternalParties | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| PartyContact | PartyContacts | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| PartyComplianceDocument | PartyComplianceDocuments | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| PartySupplierOnboarding | PartySupplierOnboardings | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| TenantSupplierOnboardingSettings | TenantSupplierOnboardingSettings | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| VendorRestriction | VendorRestrictions | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| SupplierIncident | SupplierIncidents | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| ProcurementException | ProcurementExceptions | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| StaffarrProcurementApprovalAuthorityMirror | StaffarrProcurementApprovalAuthorityMirrors | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| SupplyArrAuditEvent | AuditEvents | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| PartCatalog | PartCatalogs | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| Part | Parts | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| PartManufacturerAlias | PartManufacturerAliases | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| PartSource | PartSources | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| PartVendorLink | PartVendorLinks | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| InventoryLocation | InventoryLocations | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| InventoryBin | InventoryBins | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| PartStockLevel | PartStockLevels | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| PartStockReservation | PartStockReservations | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| WmsStockLedgerEntry | WmsStockLedgerEntries | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| WmsOutboundShipment | WmsOutboundShipments | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| WmsOutboundShipmentLine | WmsOutboundShipmentLines | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| PurchaseRequest | PurchaseRequests | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| PurchaseRequestLine | PurchaseRequestLines | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| VendorOrder | VendorOrders | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| VendorOrderStatusUpdate | VendorOrderStatusUpdates | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| VendorOrderMagicLink | VendorOrderMagicLinks | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| VendorOrderDocumentLink | VendorOrderDocumentLinks | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| VendorOrderBrokerDecision | VendorOrderBrokerDecisions | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| TenantVendorOrderSettings | TenantVendorOrderSettings | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| Rfq | Rfqs | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| RfqLine | RfqLines | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| RfqVendorInvitation | RfqVendorInvitations | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| VendorQuote | VendorQuotes | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| VendorQuoteLine | VendorQuoteLines | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| PurchaseOrder | PurchaseOrders | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| PurchaseOrderLine | PurchaseOrderLines | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| ReceivingReceipt | ReceivingReceipts | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| ReceivingReceiptLine | ReceivingReceiptLines | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| ReceivingException | ReceivingExceptions | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| Backorder | Backorders | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| VendorReturn | VendorReturns | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| VendorReturnLine | VendorReturnLines | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| WarrantyClaim | WarrantyClaims | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| SupplyContract | SupplyContracts | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| PartVendorPricingSnapshot | PartVendorPricingSnapshots | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| PartVendorLeadTimeSnapshot | PartVendorLeadTimeSnapshots | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| PartVendorAvailabilitySnapshot | PartVendorAvailabilitySnapshots | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| MaintainArrDemandRef | MaintainArrDemandRefs | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| MaintainArrDemandRefLine | MaintainArrDemandRefLines | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| RoutArrDemandRef | RoutArrDemandRefs | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| RoutArrDemandRefLine | RoutArrDemandRefLines | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| TrainArrDemandRef | TrainArrDemandRefs | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| TrainArrDemandRefLine | TrainArrDemandRefLines | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| StaffArrDemandRef | StaffArrDemandRefs | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| StaffArrDemandRefLine | StaffArrDemandRefLines | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| TenantProcurementNotificationSettings | TenantProcurementNotificationSettings | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| ProcurementNotificationDispatch | ProcurementNotificationDispatches | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| TenantPriceSnapshotSettings | TenantPriceSnapshotSettings | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| PartVendorPriceCaptureState | PartVendorPriceCaptureStates | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| PriceSnapshotRun | PriceSnapshotRuns | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| TenantLeadTimeSnapshotSettings | TenantLeadTimeSnapshotSettings | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| PartVendorLeadTimeCaptureState | PartVendorLeadTimeCaptureStates | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| LeadTimeSnapshotRun | LeadTimeSnapshotRuns | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| TenantAvailabilitySnapshotSettings | TenantAvailabilitySnapshotSettings | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| PartVendorAvailabilityCaptureState | PartVendorAvailabilityCaptureStates | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| AvailabilitySnapshotRun | AvailabilitySnapshotRuns | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| TenantProcurementCoordinationSettings | TenantProcurementCoordinationSettings | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| ProcurementCoordinationRecord | ProcurementCoordinationRecords | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| ProcurementCoordinationEvent | ProcurementCoordinationEvents | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| ProcurementCoordinationRun | ProcurementCoordinationRuns | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| TenantApprovalReminderSettings | TenantApprovalReminderSettings | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| ApprovalReminderState | ApprovalReminderStates | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| ApprovalReminderRun | ApprovalReminderRuns | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| TenantProcurementExceptionEscalationSettings | TenantProcurementExceptionEscalationSettings | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| ProcurementExceptionEscalationEvent | ProcurementExceptionEscalationEvents | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| ProcurementExceptionEscalationRun | ProcurementExceptionEscalationRuns | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| TenantDemandProcessingSettings | TenantDemandProcessingSettings | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| DemandProcessingState | DemandProcessingStates | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| DemandProcessingRun | DemandProcessingRuns | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| TenantIntegrationEventSettings | TenantIntegrationEventSettings | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| IntegrationOutboxEvent | IntegrationOutboxEvents | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| IntegrationInboxEvent | IntegrationInboxEvents | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| VendorEmailInboxMessage | VendorEmailInboxMessages | SupplyArr.Api/Data/SupplyArrDbContext.cs |
| IntegrationEventProcessingRun | IntegrationEventProcessingRuns | SupplyArr.Api/Data/SupplyArrDbContext.cs |

</details>

<details>
<summary>Frontend page files (29)</summary>

| Page |
| --- |
| src/lib/createWorkspacePage.tsx |
| src/pages/LaunchPage.tsx |
| src/workspace/SupplyArrWorkspacePage.tsx |
| src/pages/catalog/CatalogPage.tsx |
| src/pages/contracts/ContractsPage.tsx |
| src/pages/corrective-actions/CorrectiveActionsPage.tsx |
| src/pages/dashboard/DashboardPage.tsx |
| src/pages/documents/DocumentsPage.tsx |
| src/pages/imports/ImportsPage.tsx |
| src/pages/onboarding/OnboardingPage.tsx |
| src/pages/parties/PartiesPage.tsx |
| src/pages/performance/PerformancePage.tsx |
| src/pages/planning/PlanningPage.tsx |
| src/pages/pricing/PricingPage.tsx |
| src/pages/purchase-orders/PurchaseOrdersPage.tsx |
| src/pages/purchasing/PurchasingPage.tsx |
| src/pages/quotes/QuotesPage.tsx |
| src/pages/readiness/ReadinessPage.tsx |
| src/pages/reports/ReportsPage.tsx |
| src/pages/rfqs/RfqsPage.tsx |
| src/pages/risk/RiskPage.tsx |
| src/pages/settings/SettingsPage.tsx |
| src/pages/supplier-portal/SupplierPortalPage.tsx |
| src/pages/suppliers/SuppliersPage.tsx |
| src/pages/vendor-orders/VendorOrderCreatePage.tsx |
| src/pages/vendor-orders/VendorOrderDetailPage.tsx |
| src/pages/vendor-orders/VendorOrderPortalPage.tsx |
| src/pages/vendor-orders/VendorOrdersPage.tsx |
| src/pages/vendor-portal/VendorPortalPage.tsx |

</details>

<details>
<summary>Endpoint source families (75)</summary>

| Endpoint source file | Discovered route declarations |
| --- | --- |
| CoverageAliasEndpoints.cs | 35 |
| ReceivingEndpoints.cs | 21 |
| InventoryEndpoints.cs | 18 |
| PartCatalogEndpoints.cs | 16 |
| ProcurementExceptionEndpoints.cs | 14 |
| RfqEndpoints.cs | 14 |
| SupplierOnboardingEndpoints.cs | 14 |
| VendorOrderEndpoints.cs | 12 |
| IntegrationEndpoints.cs | 11 |
| SupplierIncidentEndpoints.cs | 11 |
| AuthEndpoints.cs | 10 |
| PartyRegistryEndpoints.cs | 10 |
| PurchaseOrderEndpoints.cs | 10 |
| PurchaseRequestEndpoints.cs | 10 |
| WarrantyClaimEndpoints.cs | 9 |
| WmsMovementEndpoints.cs | 9 |
| SupplyReadinessEndpoints.cs | 8 |
| EmergencyPurchaseEndpoints.cs | 7 |
| VendorRestrictionEndpoints.cs | 7 |
| IntegrationEventSettingsEndpoints.cs | 6 |
| ProcurementExceptionEscalationSettingsEndpoints.cs | 6 |
| VendorReturnEndpoints.cs | 6 |
| WorkflowAliasEndpoints.cs | 6 |
| BackorderEndpoints.cs | 5 |
| QuoteAliasEndpoints.cs | 5 |
| ReferenceIntegrationEndpoints.cs | 5 |
| StockReservationEndpoints.cs | 5 |
| VendorDocumentEndpoints.cs | 5 |
| ApprovalReminderSettingsEndpoints.cs | 4 |
| AvailabilitySnapshotSettingsEndpoints.cs | 4 |
| ComplianceReportEndpoints.cs | 4 |
| DemandProcessingEndpoints.cs | 4 |
| DemandProcessingSettingsEndpoints.cs | 4 |
| LeadTimeSnapshotSettingsEndpoints.cs | 4 |
| PartAliasEndpoints.cs | 4 |
| PartsInventoryReportEndpoints.cs | 4 |
| PriceSnapshotSettingsEndpoints.cs | 4 |
| ProcurementCoordinationSettingsEndpoints.cs | 4 |
| PurchasingReportEndpoints.cs | 4 |
| ReorderEvaluationEndpoints.cs | 4 |
| VendorPortalEndpoints.cs | 4 |
| AvailabilitySnapshotEndpoints.cs | 3 |
| DemandRefEndpoints.cs | 3 |
| ExternalReferenceEndpoints.cs | 3 |
| InternalIntegrationEventEndpoints.cs | 3 |
| LeadTimeSnapshotEndpoints.cs | 3 |
| NotificationSettingsEndpoints.cs | 3 |
| PricingSnapshotEndpoints.cs | 3 |
| RoutArrDemandRefEndpoints.cs | 3 |
| StaffArrDemandRefEndpoints.cs | 3 |
| SupplyContractEndpoints.cs | 3 |
| TrainArrDemandRefEndpoints.cs | 3 |
| VendorAccessEndpoints.cs | 3 |
| VendorReportEndpoints.cs | 3 |
| EventAndAuditEndpoints.cs | 2 |
| ForgivingSearchEndpoints.cs | 2 |
| InternalApprovalReminderEndpoints.cs | 2 |
| InternalAvailabilitySnapshotEndpoints.cs | 2 |
| InternalDemandProcessingEndpoints.cs | 2 |
| InternalLeadTimeSnapshotEndpoints.cs | 2 |
| InternalPriceSnapshotEndpoints.cs | 2 |
| InternalProcurementCoordinationEndpoints.cs | 2 |
| InternalProcurementExceptionAutomationEndpoints.cs | 2 |
| InternalProcurementExceptionEscalationEndpoints.cs | 2 |
| InternalProcurementNotificationEndpoints.cs | 2 |
| InternalReorderEvaluationEndpoints.cs | 2 |
| ProcurementCoordinationEndpoints.cs | 2 |
| ReportIndexEndpoints.cs | 2 |
| VendorEmailInboxEndpoints.cs | 2 |
| VendorOrderSettingsEndpoints.cs | 2 |
| ApprovalReminderEndpoints.cs | 1 |
| AuditHistoryEndpoints.cs | 1 |
| FieldInboxEndpoints.cs | 1 |
| SettingsEndpoints.cs | 1 |
| VendorCatalogApiEndpoints.cs | 1 |

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

Keep supplier/procurement truth; migrate WMS-like stock balances and movement ledger to LoadArr while preserving event/API handoffs.

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
