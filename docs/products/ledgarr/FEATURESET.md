# LedgArr — ERP Full Feature Set

[Workflow catalog](./WORKFLOWS.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md) · [Rollout](./ROLLOUT.md)

## Document control

| Field | Value |
| --- | --- |
| Product | LedgArr (ERP) |
| Category | Enterprise Resource Planning and Financial Management |
| Repository maturity | Durable |
| Audit basis | Static repository inspection on 2026-06-24 |
| Cataloged capabilities | 75 |
| Cataloged workflows | 20 |

## Product charter

LedgArr is the suite financial ERP and legal-entity system of record. It owns legal entities in the corporate and accounting sense, financial configuration, chart of accounts and dimensions, subledger and general-ledger posting, AP, AR, cash/banking, close, inventory valuation, fixed assets, projects/job costing, budgets, tax configuration/results, intercompany/consolidation, payroll financial packets, controls, and external ERP/GL bridges. The broader operational ERP is intentionally distributed across the Arr products that own HR, orders, warehouse, transport, suppliers, customers, maintenance, quality, documents, compliance, and BI.

> **Implementation reality — Durable:** LedgArr is the deepest persistent domain in the repository, with roughly 140 DbSets spanning tenant finance profiles, legal entities, intercompany, fiscal calendars/periods, currencies/rates, numbering, chart of accounts, dimensions, posting rules/previews/batches, journals/reversals/approvals, financial packets, subledger, AP, AR, banking/reconciliation, inventory valuation, fixed assets, projects/job costing, budgets, taxes, external finance bridges, controls/SoD, tenant settings, and payroll financial exports. The main gaps are end-to-end polished workflows, external bank/tax/payment/payroll connectivity, and accessible automation for smaller organizations.

## Source-of-truth boundary

### LedgArr owns

- Financial legal entities, ownership/consolidation relationships, intercompany rules, books, fiscal calendars/periods, currencies, exchange rates, and financial numbering.
- Chart of accounts, GL accounts, dimensions, mappings, posting rules, previews, batches, journals, reversals, approvals, attachments/refs, and immutable financial audit.
- Financial packet intake, validation, mapping, subledger, posting, rejection/correction, and source-to-ledger lineage.
- Accounts payable bills/credits/disputes/aging/payments/payment runs and accounts receivable invoices/credits/payments/applications/statements/collections/aging.
- Bank accounts, bank transactions, cash matching, reconciliation, cash position, and finance-side payment context.
- Inventory accounting/valuation, cost layers, COGS, landed cost, and reconciliation using LoadArr quantity events and procurement/order facts.
- Fixed assets, capitalization, depreciation, impairment, revaluation, disposal, and project/job cost/budget accounting.
- Tax configuration/calculation results, financial controls, approvals, separation of duties, close/locks, external bridges/sync, and payroll financial export/journal snapshots.

### LedgArr does not own

- Compliance Core governing bodies; LedgArr legal entities are companies/divisions/accounting entities, not regulators.
- Person/employee record, timekeeping source, benefits, or payroll calculation; StaffArr owns workforce/time facts and external payroll or a future payroll engine calculates payroll.
- Customer/supplier operational master beyond finance-required references; CustomArr/SupplyArr own relationship truth.
- Orders, warehouse inventory quantity, transportation, maintenance, quality, or document storage.
- Banking institution execution beyond configured payment/bank integrations; LedgArr records authorized financial truth and reconciles provider outcomes.

## Suite access and authorization

- All normal tenant users may launch every product application in the suite; product actions and records remain permission-scoped server-side.
- Fixed-suite access is nonvariable. Deployment health, feature readiness, legal restrictions, external-provider setup, and permissions may still constrain a capability.
- Platform administration remains a NexArr-only responsibility and is not a tenant product surface.
- Compliance Core administrative/authoring UI is restricted, while Compliance Core runtime evaluations, fact requirements, evidence needs, and workflow gates serve every tenant and product.

## Primary users

- Controller
- Accountant
- AP clerk
- AR/billing clerk
- Cash/treasury user
- Cost accountant
- Fixed asset accountant
- Project accountant/manager
- Finance planner
- Tax administrator
- External accountant/auditor
- Integration administrator

## Required integrations

- All operational products
- RecordArr
- ReportArr
- NexArr
- StaffArr
- External banks/payment providers
- External payroll
- Tax/e-invoice providers
- External ERP/GL systems

## Product principles

- LedgArr legal entities are corporate/accounting entities and must never be confused with Compliance Core governing bodies.
- Operational products send versioned financial packets; LedgArr validates, maps, posts, and returns status without taking over operational records.
- Posted financial truth is corrected by reversal/replacement, never destructive edit.
- Sensitive finance and payroll detail is purpose-limited while source-to-ledger lineage remains complete.

## Repository implementation snapshot

| Static indicator | Count |
| --- | --- |
| Persistent DbSet declarations | 142 |
| Discovered server classes | 279 |
| Discovered HTTP route declarations | 164 |
| Frontend source files | 13 |
| Frontend page files | 3 |
| Documentation headings | 19 |

### Evidence used for the current-state classification

- Persistent tenant finance profile, legal entities, intercompany relationships/rules, fiscal calendars/periods, currencies/exchange rates, numbering sequences, close configurations, and period locks.
- Persistent chart of accounts, GL accounts, dimensions/values, posting rules/previews/batches, journals/lines, reversals, approvals, attachments, and audit/control records.
- Persistent financial packets/lines/mappings/validations/postings, subledger entries, AP bills/credits/payments/runs/disputes/aging, AR invoices/credits/payments/applications/statements/collections/aging.
- Persistent bank accounts/transactions/reconciliations, inventory valuation/cost layers/COGS/landed cost/reconciliation, fixed assets/depreciation/disposals/impairment/revaluation, projects/job costing/budgets, and taxes.
- Persistent external finance bridges/sync, approval/SoD/control exceptions, tenant settings, payroll calendars/code mappings/batches/export packets/journal snapshots.
- ledgarr-frontend routes for dashboard, legal entities, GL, AP, AR, cash/banking, billing, budgets, cost accounting, projects, fixed assets, payroll financials, taxes, intercompany, consolidation, close, reports, packets, journals, valuation, and settings.

> Counts are static-discovery indicators, not proof that every route or screen is complete, reachable, secure, migrated, or production-ready.

## Feature catalog

### A. Currently implemented or meaningfully represented

These capabilities have repository evidence. Their state follows the product-level maturity and may still require hardening or completion.

| Feature ID | Capability | Class | State | Required behavior / evidence |
| --- | --- | --- | --- | --- |
| LE-CUR-001 | Financial legal entities and intercompany | CURRENT | Durable | Durable legal entity, relationship, intercompany, and consolidation configuration models. |
| LE-CUR-002 | Fiscal calendars, periods, locks, and close configuration | CURRENT | Durable | Calendars, periods, close, lock, and reopening controls are persistent. |
| LE-CUR-003 | Currencies and exchange rates | CURRENT | Durable | Multi-currency configuration and rate records support transactional and reporting currency. |
| LE-CUR-004 | Financial numbering and document sequences | CURRENT | Durable | Configurable sequences support controlled financial identifiers. |
| LE-CUR-005 | Chart of accounts and dimensions | CURRENT | Durable | Accounts, dimensions, values, mappings, and effective configuration are durable. |
| LE-CUR-006 | Posting rules and preview | CURRENT | Durable | Rule-driven mapping and preview allow source packets to be validated before journals post. |
| LE-CUR-007 | Financial packets and source lineage | CURRENT | Durable | Packet/line/mapping/validation/posting models preserve operational source-to-ledger traceability. |
| LE-CUR-008 | General ledger journals, approvals, and reversals | CURRENT | Durable | Journals/lines, batches, approvals, attachments, reversals, and audit are durable. |
| LE-CUR-009 | Accounts payable | CURRENT | Durable | Vendor bills, credits, disputes, payment records/runs, and aging are persistent. |
| LE-CUR-010 | Accounts receivable | CURRENT | Durable | Invoices, credit memos, customer payments/applications, statements, collections, and aging are persistent. |
| LE-CUR-011 | Banking and reconciliation | CURRENT | Durable | Bank accounts, transactions, matching/reconciliation, and cash-facing workflows are represented. |
| LE-CUR-012 | Inventory valuation and cost accounting | CURRENT | Durable | Cost layers, valuation, COGS, landed cost, and reconciliation models connect quantity truth to finance. |
| LE-CUR-013 | Fixed assets | CURRENT | Durable | Capital asset, depreciation, disposal, impairment, and revaluation models are persistent. |
| LE-CUR-014 | Projects, job costing, and budgets | CURRENT | Durable | Projects, cost accumulation, budgets, and related reporting structures are durable. |
| LE-CUR-015 | Tax configuration and transaction context | CURRENT | Durable | Tax models support jurisdiction/rule/result context; external filing/connectivity remains a completion area. |
| LE-CUR-016 | Controls, approvals, SoD, and exceptions | CURRENT | Durable | Approval policies, separation-of-duties checks, control exceptions, and audit support financial governance. |
| LE-CUR-017 | External finance bridge and sync | CURRENT | Durable | External ERP/GL bridge profiles, mappings, exports, sync runs, and reconciliation support coexistence/migration. |
| LE-CUR-018 | Payroll financial export and journal snapshot | CURRENT | Durable | Payroll calendars, code mappings, batches, export packets, and journal snapshots support payroll-to-ledger integration. |
| LE-CUR-019 | Tenant settings and broad finance UI | CURRENT | Durable | Settings and route coverage span the expected finance operating model. |

### B. Common category baseline

These are expected for a credible Enterprise Resource Planning and Financial Management product. Where they overlap current behavior, the target definition is the acceptance standard rather than a duplicate feature.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| LE-COM-001 | Multi-entity financial management | COMMON | Target | Legal entities, books, ownership, branches/sites, consolidation groups, currencies, calendars, and intercompany. |
| LE-COM-002 | General ledger | COMMON | Target | Chart of accounts, dimensions, journals, recurring entries, allocations, accruals, reversals, reclasses, period controls, and trial balance. |
| LE-COM-003 | Accounts payable | COMMON | Target | Supplier bills, matching, approvals, credits, disputes, payment runs, remittance, aging, and vendor statement reconciliation. |
| LE-COM-004 | Accounts receivable and billing | COMMON | Target | Invoices, credits, cash application, customer statements, collections, disputes, dunning, aging, and write-offs. |
| LE-COM-005 | Cash and banking | COMMON | Target | Bank feeds/import, matching, reconciliation, cash position, transfers, deposits, payment provider outcomes, and forecast. |
| LE-COM-006 | Budgeting and forecasting | COMMON | Target | Budgets by entity/account/dimension/project, versions, approvals, forecasts, commitments, actuals, and variance. |
| LE-COM-007 | Inventory accounting | COMMON | Target | Valuation methods, cost layers, COGS, adjustments, landed cost, transfers, returns, write-downs, and quantity-to-value reconciliation. |
| LE-COM-008 | Fixed asset accounting | COMMON | Target | Capitalization, books, classes, depreciation, transfers, impairment, revaluation, disposal, and asset-to-maintenance reference. |
| LE-COM-009 | Project/job costing | COMMON | Target | Projects/jobs, budgets, labor/material/subcontract/overhead costs, revenue refs, WIP, profitability, and close. |
| LE-COM-010 | Tax | COMMON | Target | Sales/use/VAT/GST and other transaction tax configuration, exemptions, calculations, reporting, filing/export, and audit trail. |
| LE-COM-011 | Close and consolidation | COMMON | Target | Task checklist, reconciliations, accruals, eliminations, currency translation, consolidation, locks, review, and financial statements. |
| LE-COM-012 | Financial reporting | COMMON | Target | Trial balance, P&L, balance sheet, cash flow, AP/AR aging, budget variance, project, inventory, fixed asset, tax, and entity consolidation. |
| LE-COM-013 | Audit and internal controls | COMMON | Target | Approval, SoD, immutable posting history, attachments, certification, exception, close signoff, and source lineage. |
| LE-COM-014 | External integration | COMMON | Target | Banks, payment processors, tax engines, payroll, e-commerce/order, procurement, WMS, expense, external ERP/GL, and accountant export/import. |

### C. Commonly requested but widely underserved

These address recurring user friction, fragmented add-ons, poor transparency, inaccessible setup, or weak SMB usability.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| LE-UND-001 | Enterprise controls with small-business usability | UNDERSERVED | Target | Guided defaults, clear language, preview, and progressive disclosure provide real accounting rigor without requiring a large finance team. |
| LE-UND-002 | Explain every posting | UNDERSERVED | Target | From journal line, show source packet, operational record, rule, account/dimensions, rate, approvals, corrections, and evidence in plain language. |
| LE-UND-003 | One finance inbox | UNDERSERVED | Target | Bills, packet errors, approvals, bank matches, collections, close tasks, tax exceptions, and sync failures are prioritized by owner and impact. |
| LE-UND-004 | Operational-to-financial reconciliation | UNDERSERVED | Target | Order, receipt, shipment, work, labor, inventory, transport, quality, and return facts reconcile to finance without spreadsheets or shadow exports. |
| LE-UND-005 | External accountant portal | UNDERSERVED | Target | Scoped period/entity review, questions, proposed adjustments, documents, signoff, and export without broad tenant access. |
| LE-UND-006 | Continuous close for small teams | UNDERSERVED | Target | Automated recurring tasks, reconciliations, missing packet detection, and daily/weekly readiness make month-end less disruptive. |
| LE-UND-007 | No-code posting and mapping rules | UNDERSERVED | Target | Finance admins can version, test, simulate, approve, and roll back mappings without developer intervention. |
| LE-UND-008 | Cash flow with confidence and source | UNDERSERVED | Target | Forecast cash using open AP/AR, orders, procurement, payroll refs, taxes, payment terms, and scenarios with uncertainty. |
| LE-UND-009 | Multi-entity without enterprise licensing | UNDERSERVED | Target | Legal entities, currencies, intercompany, consolidation, and eliminations remain accessible to growing non-enterprise companies. |
| LE-UND-010 | Transparent inventory valuation reconciliation | UNDERSERVED | Target | Explain quantity/value differences by movement, timing, cost layer, UOM, landed cost, return, adjustment, or integration gap. |
| LE-UND-011 | Correction instead of deletion | UNDERSERVED | Target | Every posted error has reversal/correcting workflow and source reconciliation; draft mistakes remain distinguishable from posted truth. |
| LE-UND-012 | Portable financial archive | UNDERSERVED | Target | Export complete journals, master/config, source lineage, documents, reports, and audit packages without vendor lock-in. |

### D. Advanced capabilities to democratize

These are commonly found only in enterprise tiers, specialist products, or expensive implementations. STL should make them practical without weakening governance.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| LE-DEM-001 | Advanced consolidation and eliminations | DEMOCRATIZE | Target | Multi-level groups, partial ownership, currency translation, eliminations, minority interest context, and consolidation workpapers. |
| LE-DEM-002 | Planning, budgeting, and scenario modeling | DEMOCRATIZE | Target | Driver-based models, rolling forecast, workforce/capacity/order scenarios, approvals, versions, and variance explanations. |
| LE-DEM-003 | Treasury and liquidity management | DEMOCRATIZE | Target | Cash positioning, bank connectivity, payment fraud controls, forecasts, sweeps/transfers, debt/investment refs, and scenario stress tests. |
| LE-DEM-004 | Revenue recognition and contract accounting | DEMOCRATIZE | Target | Performance obligations, schedules, allocations, modifications, deferred revenue, and order/agreement lineage where applicable. |
| LE-DEM-005 | Lease accounting | DEMOCRATIZE | Target | Lease inventory, schedules, modifications, payments, ROU assets/liabilities, journal generation, and disclosures. |
| LE-DEM-006 | Automated close management | DEMOCRATIZE | Target | Dependency-aware close checklist, reconciliations, anomaly detection, evidence, reviewer signoff, and reopen impact. |
| LE-DEM-007 | Financial anomaly and fraud detection | DEMOCRATIZE | Target | Duplicate bills/payments, unusual journals, bank anomalies, vendor/customer changes, split approvals, and pattern alerts with explainability. |
| LE-DEM-008 | Continuous audit and control monitoring | DEMOCRATIZE | Target | Test controls using operational/financial events, flag exceptions, assign remediation, and preserve evidence without a separate enterprise GRC tool. |
| LE-DEM-009 | Advanced cost accounting | DEMOCRATIZE | Target | Standard/actual/average costing, overhead, activity-based cost, variance, co/by-products, service cost, and profitability. |
| LE-DEM-010 | Global tax and e-invoicing orchestration | DEMOCRATIZE | Target | Jurisdiction rules, electronic invoice formats/status, tax engine integration, filing packs, and change monitoring for smaller multinational firms. |

### E. Suite-wide foundation required in LedgArr

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| LE-FND-001 | Tenant-scoped authorization | FOUNDATION | Target | Every read and write is tenant-scoped server-side; product UI visibility never substitutes for authorization. |
| LE-FND-002 | StaffArr-backed action permissions | FOUNDATION | Target | Use product action keys, role scopes, delegated authority, and explicit denial reasons; ownership does not prevent permissioned delegated workflows. |
| LE-FND-003 | Unified suite shell | FOUNDATION | Target | Consistent navigation, page anatomy, terminology, responsive behavior, keyboard access, and light/dark contrast across products. |
| LE-FND-004 | Record lifecycle and history | FOUNDATION | Target | Human-readable statuses, timeline events, actor/time/source attribution, undo or correction paths, and immutable audit evidence. |
| LE-FND-005 | Quick create for missing references | FOUNDATION · UNDERSERVED | Target | Create the minimum valid cross-product reference without abandoning the current task; preserve context and allow later backfill. |
| LE-FND-006 | Saved views and personal preferences | COMMON | Target | Per-user filters, columns, density, sort, board/list preference, default landing view, and product-scoped preferences. |
| LE-FND-007 | Bulk operations with preview | COMMON | Target | Selection-aware actions, dry-run/impact preview, partial failure handling, downloadable results, and permission checks per record. |
| LE-FND-008 | Import with mapping and review | COMMON · UNDERSERVED | Target | Product-specific import separate from Smart Import, with templates, mappings, validation, dedupe, dry run, commit plan, and rollback/correction evidence. |
| LE-FND-009 | Export and portability | COMMON · DEMOCRATIZE | Target | CSV/PDF/JSON or package export appropriate to the domain, without trapping customer data behind enterprise tiers. |
| LE-FND-010 | Notifications, tasks, and inbox | COMMON | Target | Actionable notifications with ownership, due dates, escalation, snooze/delegation, deep links, and duplicate suppression. |
| LE-FND-011 | API, webhooks, and event outbox | COMMON · DEMOCRATIZE | Target | Stable versioned APIs, idempotency, service authentication, signed webhooks/events, retries, dead-letter handling, and correlation IDs. |
| LE-FND-012 | RecordArr evidence references | FOUNDATION | Target | Files and evidence are stored once in RecordArr and referenced by owning records; products do not create shadow document vaults. |
| LE-FND-013 | ReportArr projections and metrics | FOUNDATION | Target | Operational products emit facts/events; ReportArr builds read models and reports without becoming the source of operational truth. |
| LE-FND-014 | Compliance Core applicability and gates | FOUNDATION | Target | Products request explainable requirements, missing facts, evidence needs, and gate decisions rather than embedding independent legal logic. |
| LE-FND-015 | Field Companion execution surface | FOUNDATION · UNDERSERVED | Target | Mobile-first task, scan, capture, acknowledgement, and offline execution against the owning product API. |
| LE-FND-016 | Professional print and report view | COMMON | Target | Print output is a purpose-built report document without the app shell, not a screenshot of the current page. |
| LE-FND-017 | Accessible error and degraded states | FOUNDATION | Target | Explain what failed, what remains saved, whether retry is safe, what the user can do, and which administrator or integration owns the issue. |
| LE-FND-018 | Time, locale, and units | FOUNDATION | Target | Store canonical timestamps and units; display tenant/user locale, timezone, currency, and measurement preferences consistently. |
| LE-FND-019 | Retention, privacy, and legal hold awareness | FOUNDATION | Target | Product deletion/archive actions respect RecordArr retention, legal holds, privacy policy, and downstream references. |
| LE-FND-020 | AI-assisted proposals, never silent commits | UNDERSERVED | Target | AI may classify, summarize, suggest, or draft; consequential changes remain reviewable, permissioned, attributable, and reversible. |

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
<summary>Persistent entity sets (142)</summary>

| Entity type | DbSet/property | Source |
| --- | --- | --- |
| TenantFinancialProfile | TenantFinancialProfiles | LedgArr.Api/Data/LedgArrDbContext.cs |
| FinancialLegalEntity | FinancialLegalEntities | LedgArr.Api/Data/LedgArrDbContext.cs |
| FinancialLegalEntityRelationship | FinancialLegalEntityRelationships | LedgArr.Api/Data/LedgArrDbContext.cs |
| IntercompanyTransaction | IntercompanyTransactions | LedgArr.Api/Data/LedgArrDbContext.cs |
| FinancialLegalEntityRegistration | FinancialLegalEntityRegistrations | LedgArr.Api/Data/LedgArrDbContext.cs |
| FinancialLegalEntityAddressSnapshot | FinancialLegalEntityAddressSnapshots | LedgArr.Api/Data/LedgArrDbContext.cs |
| FiscalCalendar | FiscalCalendars | LedgArr.Api/Data/LedgArrDbContext.cs |
| FiscalYear | FiscalYears | LedgArr.Api/Data/LedgArrDbContext.cs |
| FiscalPeriod | FiscalPeriods | LedgArr.Api/Data/LedgArrDbContext.cs |
| Currency | Currencies | LedgArr.Api/Data/LedgArrDbContext.cs |
| ExchangeRate | ExchangeRates | LedgArr.Api/Data/LedgArrDbContext.cs |
| NumberingSequence | NumberingSequences | LedgArr.Api/Data/LedgArrDbContext.cs |
| FinancialCloseRun | FinancialCloseRuns | LedgArr.Api/Data/LedgArrDbContext.cs |
| PeriodLockAudit | PeriodLockAudits | LedgArr.Api/Data/LedgArrDbContext.cs |
| ChartOfAccounts | ChartsOfAccounts | LedgArr.Api/Data/LedgArrDbContext.cs |
| GLAccount | GLAccounts | LedgArr.Api/Data/LedgArrDbContext.cs |
| AccountAlias | AccountAliases | LedgArr.Api/Data/LedgArrDbContext.cs |
| AccountMapping | AccountMappings | LedgArr.Api/Data/LedgArrDbContext.cs |
| FinancialDimensionType | FinancialDimensionTypes | LedgArr.Api/Data/LedgArrDbContext.cs |
| FinancialDimensionValue | FinancialDimensionValues | LedgArr.Api/Data/LedgArrDbContext.cs |
| FinancialDimensionCombination | FinancialDimensionCombinations | LedgArr.Api/Data/LedgArrDbContext.cs |
| DimensionRequirementRule | DimensionRequirementRules | LedgArr.Api/Data/LedgArrDbContext.cs |
| DimensionMappingRule | DimensionMappingRules | LedgArr.Api/Data/LedgArrDbContext.cs |
| SourceDimensionMapping | SourceDimensionMappings | LedgArr.Api/Data/LedgArrDbContext.cs |
| PostingRule | PostingRules | LedgArr.Api/Data/LedgArrDbContext.cs |
| PostingRuleLine | PostingRuleLines | LedgArr.Api/Data/LedgArrDbContext.cs |
| PostingPreview | PostingPreviews | LedgArr.Api/Data/LedgArrDbContext.cs |
| PostingPreviewLine | PostingPreviewLines | LedgArr.Api/Data/LedgArrDbContext.cs |
| PostingBatch | PostingBatches | LedgArr.Api/Data/LedgArrDbContext.cs |
| PostingBatchLine | PostingBatchLines | LedgArr.Api/Data/LedgArrDbContext.cs |
| JournalEntry | JournalEntries | LedgArr.Api/Data/LedgArrDbContext.cs |
| JournalLine | JournalLines | LedgArr.Api/Data/LedgArrDbContext.cs |
| JournalEntryReversal | JournalEntryReversals | LedgArr.Api/Data/LedgArrDbContext.cs |
| JournalAttachmentRef | JournalAttachmentRefs | LedgArr.Api/Data/LedgArrDbContext.cs |
| JournalApproval | JournalApprovals | LedgArr.Api/Data/LedgArrDbContext.cs |
| JournalAuditTrail | JournalAuditTrails | LedgArr.Api/Data/LedgArrDbContext.cs |
| FinancialPacket | FinancialPackets | LedgArr.Api/Data/LedgArrDbContext.cs |
| FinancialPacketLine | FinancialPacketLines | LedgArr.Api/Data/LedgArrDbContext.cs |
| FinancialPacketSourceRef | FinancialPacketSourceRefs | LedgArr.Api/Data/LedgArrDbContext.cs |
| FinancialPacketStatusHistory | FinancialPacketStatusHistory | LedgArr.Api/Data/LedgArrDbContext.cs |
| FinancialPacketValidationIssue | FinancialPacketValidationIssues | LedgArr.Api/Data/LedgArrDbContext.cs |
| FinancialPacketMappingResult | FinancialPacketMappingResults | LedgArr.Api/Data/LedgArrDbContext.cs |
| FinancialPacketPostingResult | FinancialPacketPostingResults | LedgArr.Api/Data/LedgArrDbContext.cs |
| FinancialPacketIdempotencyKey | FinancialPacketIdempotencyKeys | LedgArr.Api/Data/LedgArrDbContext.cs |
| BillableEvent | BillableEvents | LedgArr.Api/Data/LedgArrDbContext.cs |
| SubledgerDocument | SubledgerDocuments | LedgArr.Api/Data/LedgArrDbContext.cs |
| SubledgerDocumentLine | SubledgerDocumentLines | LedgArr.Api/Data/LedgArrDbContext.cs |
| SubledgerApplication | SubledgerApplications | LedgArr.Api/Data/LedgArrDbContext.cs |
| SubledgerReconciliationRun | SubledgerReconciliationRuns | LedgArr.Api/Data/LedgArrDbContext.cs |
| SubledgerReconciliationIssue | SubledgerReconciliationIssues | LedgArr.Api/Data/LedgArrDbContext.cs |
| VendorFinancialProfile | VendorFinancialProfiles | LedgArr.Api/Data/LedgArrDbContext.cs |
| VendorBill | VendorBills | LedgArr.Api/Data/LedgArrDbContext.cs |
| VendorBillLine | VendorBillLines | LedgArr.Api/Data/LedgArrDbContext.cs |
| VendorBillApproval | VendorBillApprovals | LedgArr.Api/Data/LedgArrDbContext.cs |
| VendorBillMatch | VendorBillMatches | LedgArr.Api/Data/LedgArrDbContext.cs |
| VendorBillVariance | VendorBillVariances | LedgArr.Api/Data/LedgArrDbContext.cs |
| VendorCredit | VendorCredits | LedgArr.Api/Data/LedgArrDbContext.cs |
| APPayment | APPayments | LedgArr.Api/Data/LedgArrDbContext.cs |
| APPaymentLine | APPaymentLines | LedgArr.Api/Data/LedgArrDbContext.cs |
| PaymentRun | PaymentRuns | LedgArr.Api/Data/LedgArrDbContext.cs |
| PaymentExportBatch | PaymentExportBatches | LedgArr.Api/Data/LedgArrDbContext.cs |
| APDispute | APDisputes | LedgArr.Api/Data/LedgArrDbContext.cs |
| APAgingSnapshot | APAgingSnapshots | LedgArr.Api/Data/LedgArrDbContext.cs |
| CustomerFinancialProfile | CustomerFinancialProfiles | LedgArr.Api/Data/LedgArrDbContext.cs |
| CustomerInvoice | CustomerInvoices | LedgArr.Api/Data/LedgArrDbContext.cs |
| CustomerInvoiceLine | CustomerInvoiceLines | LedgArr.Api/Data/LedgArrDbContext.cs |
| CustomerInvoiceApproval | CustomerInvoiceApprovals | LedgArr.Api/Data/LedgArrDbContext.cs |
| CustomerCreditMemo | CustomerCreditMemos | LedgArr.Api/Data/LedgArrDbContext.cs |
| CustomerPayment | CustomerPayments | LedgArr.Api/Data/LedgArrDbContext.cs |
| CustomerPaymentApplication | CustomerPaymentApplications | LedgArr.Api/Data/LedgArrDbContext.cs |
| CustomerStatement | CustomerStatements | LedgArr.Api/Data/LedgArrDbContext.cs |
| CollectionStatus | CollectionStatuses | LedgArr.Api/Data/LedgArrDbContext.cs |
| ARAgingSnapshot | ARAgingSnapshots | LedgArr.Api/Data/LedgArrDbContext.cs |
| BankAccount | BankAccounts | LedgArr.Api/Data/LedgArrDbContext.cs |
| BankTransaction | BankTransactions | LedgArr.Api/Data/LedgArrDbContext.cs |
| BankReconciliation | BankReconciliations | LedgArr.Api/Data/LedgArrDbContext.cs |
| InventoryValuationProfile | InventoryValuationProfiles | LedgArr.Api/Data/LedgArrDbContext.cs |
| ItemCostProfile | ItemCostProfiles | LedgArr.Api/Data/LedgArrDbContext.cs |
| InventoryCostLayer | InventoryCostLayers | LedgArr.Api/Data/LedgArrDbContext.cs |
| InventoryValuationMovement | InventoryValuationMovements | LedgArr.Api/Data/LedgArrDbContext.cs |
| InventoryValuationAdjustment | InventoryValuationAdjustments | LedgArr.Api/Data/LedgArrDbContext.cs |
| LandedCostAllocation | LandedCostAllocations | LedgArr.Api/Data/LedgArrDbContext.cs |
| LandedCostAllocationLine | LandedCostAllocationLines | LedgArr.Api/Data/LedgArrDbContext.cs |
| InventorySubledgerBalance | InventorySubledgerBalances | LedgArr.Api/Data/LedgArrDbContext.cs |
| COGSPostingRun | COGSPostingRuns | LedgArr.Api/Data/LedgArrDbContext.cs |
| InventoryReconciliationRun | InventoryReconciliationRuns | LedgArr.Api/Data/LedgArrDbContext.cs |
| FixedAssetFinancialRecord | FixedAssetFinancialRecords | LedgArr.Api/Data/LedgArrDbContext.cs |
| AssetCapitalizationEvent | AssetCapitalizationEvents | LedgArr.Api/Data/LedgArrDbContext.cs |
| AssetDepreciationBook | AssetDepreciationBooks | LedgArr.Api/Data/LedgArrDbContext.cs |
| AssetDepreciationSchedule | AssetDepreciationSchedules | LedgArr.Api/Data/LedgArrDbContext.cs |
| AssetDepreciationRun | AssetDepreciationRuns | LedgArr.Api/Data/LedgArrDbContext.cs |
| AssetDisposal | AssetDisposals | LedgArr.Api/Data/LedgArrDbContext.cs |
| AssetImpairment | AssetImpairments | LedgArr.Api/Data/LedgArrDbContext.cs |
| AssetRevaluation | AssetRevaluations | LedgArr.Api/Data/LedgArrDbContext.cs |
| FinancialProject | FinancialProjects | LedgArr.Api/Data/LedgArrDbContext.cs |
| FinancialProjectTask | FinancialProjectTasks | LedgArr.Api/Data/LedgArrDbContext.cs |
| JobCostCode | JobCostCodes | LedgArr.Api/Data/LedgArrDbContext.cs |
| ProjectBudget | ProjectBudgets | LedgArr.Api/Data/LedgArrDbContext.cs |
| ProjectBudgetLine | ProjectBudgetLines | LedgArr.Api/Data/LedgArrDbContext.cs |
| ProjectActualCost | ProjectActualCosts | LedgArr.Api/Data/LedgArrDbContext.cs |
| ProjectCommittedCost | ProjectCommittedCosts | LedgArr.Api/Data/LedgArrDbContext.cs |
| ProjectCostAllocation | ProjectCostAllocations | LedgArr.Api/Data/LedgArrDbContext.cs |
| ProjectBillingStatus | ProjectBillingStatuses | LedgArr.Api/Data/LedgArrDbContext.cs |
| Budget | Budgets | LedgArr.Api/Data/LedgArrDbContext.cs |
| BudgetVersion | BudgetVersions | LedgArr.Api/Data/LedgArrDbContext.cs |
| BudgetLine | BudgetLines | LedgArr.Api/Data/LedgArrDbContext.cs |
| BudgetApproval | BudgetApprovals | LedgArr.Api/Data/LedgArrDbContext.cs |
| BudgetActualSnapshot | BudgetActualSnapshots | LedgArr.Api/Data/LedgArrDbContext.cs |
| BudgetVarianceSnapshot | BudgetVarianceSnapshots | LedgArr.Api/Data/LedgArrDbContext.cs |
| TaxCode | TaxCodes | LedgArr.Api/Data/LedgArrDbContext.cs |
| TaxJurisdiction | TaxJurisdictions | LedgArr.Api/Data/LedgArrDbContext.cs |
| TaxRate | TaxRates | LedgArr.Api/Data/LedgArrDbContext.cs |
| TaxRule | TaxRules | LedgArr.Api/Data/LedgArrDbContext.cs |
| TaxPosting | TaxPostings | LedgArr.Api/Data/LedgArrDbContext.cs |
| TaxAdjustment | TaxAdjustments | LedgArr.Api/Data/LedgArrDbContext.cs |
| TaxExemptionCertificateRef | TaxExemptionCertificateRefs | LedgArr.Api/Data/LedgArrDbContext.cs |
| TaxReportingRun | TaxReportingRuns | LedgArr.Api/Data/LedgArrDbContext.cs |
| ExternalFinanceSystem | ExternalFinanceSystems | LedgArr.Api/Data/LedgArrDbContext.cs |
| ExternalFinanceConnection | ExternalFinanceConnections | LedgArr.Api/Data/LedgArrDbContext.cs |
| ExternalAccountMapping | ExternalAccountMappings | LedgArr.Api/Data/LedgArrDbContext.cs |
| ExternalDimensionMapping | ExternalDimensionMappings | LedgArr.Api/Data/LedgArrDbContext.cs |
| ExternalCustomerMapping | ExternalCustomerMappings | LedgArr.Api/Data/LedgArrDbContext.cs |
| ExternalVendorMapping | ExternalVendorMappings | LedgArr.Api/Data/LedgArrDbContext.cs |
| ExternalItemMapping | ExternalItemMappings | LedgArr.Api/Data/LedgArrDbContext.cs |
| ExternalPostingBatch | ExternalPostingBatches | LedgArr.Api/Data/LedgArrDbContext.cs |
| ExternalPostingResult | ExternalPostingResults | LedgArr.Api/Data/LedgArrDbContext.cs |
| ExternalSyncRun | ExternalSyncRuns | LedgArr.Api/Data/LedgArrDbContext.cs |
| ExternalSyncIssue | ExternalSyncIssues | LedgArr.Api/Data/LedgArrDbContext.cs |
| FinancialAuditEvent | FinancialAuditEvents | LedgArr.Api/Data/LedgArrDbContext.cs |
| ApprovalPolicy | ApprovalPolicies | LedgArr.Api/Data/LedgArrDbContext.cs |
| ApprovalStep | ApprovalSteps | LedgArr.Api/Data/LedgArrDbContext.cs |
| ApprovalDecision | ApprovalDecisions | LedgArr.Api/Data/LedgArrDbContext.cs |
| SegregationOfDutiesRule | SegregationOfDutiesRules | LedgArr.Api/Data/LedgArrDbContext.cs |
| FinancialControlException | FinancialControlExceptions | LedgArr.Api/Data/LedgArrDbContext.cs |
| LedgArrTenantSettingSection | LedgArrTenantSettingSections | LedgArr.Api/Data/LedgArrDbContext.cs |
| LedgArrTenantSettingsAudit | LedgArrTenantSettingsAudits | LedgArr.Api/Data/LedgArrDbContext.cs |
| PayrollCalendar | PayrollCalendars | LedgArr.Api/Data/LedgArrDbContext.cs |
| PayrollCodeMapping | PayrollCodeMappings | LedgArr.Api/Data/LedgArrDbContext.cs |
| PayrollBatch | PayrollBatches | LedgArr.Api/Data/LedgArrDbContext.cs |
| PayrollBatchLine | PayrollBatchLines | LedgArr.Api/Data/LedgArrDbContext.cs |
| PayrollExportPacket | PayrollExportPackets | LedgArr.Api/Data/LedgArrDbContext.cs |
| PayrollJournalSnapshot | PayrollJournalSnapshots | LedgArr.Api/Data/LedgArrDbContext.cs |

</details>

<details>
<summary>Frontend page files (3)</summary>

| Page |
| --- |
| src/LaunchPage.tsx |
| src/pages/PayrollPage.tsx |
| src/pages/settings/LedgArrSettingsPage.tsx |

</details>

<details>
<summary>Endpoint source families (3)</summary>

| Endpoint source file | Discovered route declarations |
| --- | --- |
| LedgArrEndpoints.cs | 136 |
| PayrollEndpoints.cs | 22 |
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

Complete controlled posting, subledgers, reconciliation, period close, tax/bank/payment integrations, and cross-product contribution contracts without absorbing operational products.

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
