# LedgArr — ERP Workflow Catalog

[Feature catalog](./FEATURESET.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md)

## Workflow contract

This document defines the end-to-end business state machines for LedgArr. A workflow is not complete because a page or endpoint exists: the trigger, authority, state transitions, validation, handoffs, evidence, exceptions, retries, conflict behavior, notifications, reporting, and final source-of-truth commit must all work together.

## Ownership rule

LedgArr is the suite financial ERP and legal-entity system of record. It owns legal entities in the corporate and accounting sense, financial configuration, chart of accounts and dimensions, subledger and general-ledger posting, AP, AR, cash/banking, close, inventory valuation, fixed assets, projects/job costing, budgets, tax configuration/results, intercompany/consolidation, payroll financial packets, controls, and external ERP/GL bridges. The broader operational ERP is intentionally distributed across the Arr products that own HR, orders, warehouse, transport, suppliers, customers, maintenance, quality, documents, compliance, and BI.

- Compliance Core governing bodies; LedgArr legal entities are companies/divisions/accounting entities, not regulators.
- Person/employee record, timekeeping source, benefits, or payroll calculation; StaffArr owns workforce/time facts and external payroll or a future payroll engine calculates payroll.
- Customer/supplier operational master beyond finance-required references; CustomArr/SupplyArr own relationship truth.
- Orders, warehouse inventory quantity, transportation, maintenance, quality, or document storage.
- Banking institution execution beyond configured payment/bank integrations; LedgArr records authorized financial truth and reconciles provider outcomes.

## Workflow index

| Workflow ID | Workflow | Class | State | Trigger |
| --- | --- | --- | --- | --- |
| LE-WF-001 | Configure legal entity, books, calendar, currency, and chart of accounts | CURRENT · COMMON | Durable | A tenant creates a finance profile or adds/reorganizes an entity. |
| LE-WF-002 | Ingest, validate, map, approve, and post a financial packet | CURRENT · UNDERSERVED | Durable | OrdArr, SupplyArr, LoadArr, RoutArr, MaintainArr, StaffArr/payroll, or an integration emits a finance packet. |
| LE-WF-003 | Manual or recurring journal entry | CURRENT · COMMON | Durable | Authorized finance user creates a manual/recurring/accrual/reclass journal. |
| LE-WF-004 | Supplier bill intake, match, approval, and posting | CURRENT · COMMON | Durable | Invoice arrives by portal/email/import/API or finance user enters a bill. |
| LE-WF-005 | Payment proposal, approval, execution, and reconciliation | CURRENT · COMMON | Partial | AP creates a payment run or due obligations meet scheduled criteria. |
| LE-WF-006 | Customer invoice, delivery, and accounts receivable | CURRENT · COMMON | Durable | OrdArr or another authorized source sends an invoice-ready packet or finance creates an approved billing event. |
| LE-WF-007 | Customer payment intake and cash application | CURRENT · COMMON | Durable | Bank feed, payment provider, lockbox/import, or finance user records a customer payment. |
| LE-WF-008 | Collections, dispute, promise-to-pay, and write-off | CURRENT · COMMON | Durable | Invoice ages past policy, customer disputes, or collector opens an account. |
| LE-WF-009 | Bank transaction import and reconciliation | CURRENT · COMMON | Durable | Bank feed/file/API import arrives or reconciliation period begins. |
| LE-WF-010 | Inventory valuation, COGS, and quantity-to-value reconciliation | CURRENT · COMMON | Durable | LoadArr movement/period event or cost update requires valuation. |
| LE-WF-011 | Landed cost allocation | CURRENT · COMMON | Durable | A freight/vendor/customs bill or shipment completion provides landed-cost facts. |
| LE-WF-012 | Fixed asset capitalize, depreciate, transfer, impair, and dispose | CURRENT · COMMON | Durable | Approved capital purchase/project completion or finance event creates/changes a fixed asset. |
| LE-WF-013 | Project/job budget, cost collection, and profitability | CURRENT · COMMON | Durable | Authorized user creates a project/job or operational packets reference one. |
| LE-WF-014 | Period close, reconciliation, consolidation, and reopen | CURRENT · COMMON | Durable | Close calendar opens or controller starts a close. |
| LE-WF-015 | Intercompany transaction, matching, settlement, and elimination | CURRENT · DEMOCRATIZE | Durable | A source packet or finance user identifies an intercompany transaction. |
| LE-WF-016 | Payroll financial packet and journal reconciliation | CURRENT · COMMON | Durable | External payroll or StaffArr-approved export produces a payroll result packet. |
| LE-WF-017 | Budget creation, revision, commitment control, and forecast | CURRENT · DEMOCRATIZE | Partial | Finance starts a budget cycle or owner requests a revision. |
| LE-WF-018 | Tax calculation, exemption, reporting, and filing package | CURRENT · COMMON | Partial | Invoice/bill/order/asset transaction requires tax or period reporting begins. |
| LE-WF-019 | External ERP/GL bridge, coexistence, and migration | CURRENT · COMMON | Durable | A scheduled/manual sync or migration run starts. |
| LE-WF-020 | Financial audit package and source-to-ledger trace | CURRENT · COMMON | Durable | Auditor/controller selects entity, account, period, transaction, control, or source scope. |

## Universal workflow requirements

- **Authority:** resolve user/service identity, tenant, action permission, organizational/record scope, delegation, and separation of duties on the server.
- **State:** use explicit human-readable states and legal transitions; never infer final completion solely from a screen closing or an external request being sent.
- **Idempotency:** retries, double-clicks, event replay, import retry, webhooks, and offline sync cannot create duplicate effects.
- **Concurrency:** stale edits receive a conflict with current context and permitted resolution; never silently last-write-wins consequential data.
- **Evidence:** retain actor, source, version, time, reason, input/output, approvals, external calls, attachments by RecordArr reference, and correlation/causation.
- **Handoffs:** the receiving product accepts/rejects explicitly and emits an outcome; the sender does not mark downstream work complete merely because it dispatched a request.
- **Degradation:** state what is saved, what failed, whether retry is safe, and the manual or alternate path. Safety/compliance/financial hard gates never silently fail open.
- **Notifications:** notify only actionable audiences, deduplicate, respect preference/urgency/quiet-hour policy, escalate, and deep-link through a fresh permission check.
- **Mobile/offline:** only server-declared offline-safe actions queue; final authorization, concurrency, references, and hard gates are revalidated by the owning product.
- **Reporting:** emit events/facts to ReportArr with source/effective time and data-quality state; ReportArr never substitutes for the operational record.

## LE-WF-001 — Configure legal entity, books, calendar, currency, and chart of accounts

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Establish a controlled financial operating structure without confusing legal entities with regulators. |
| Trigger | A tenant creates a finance profile or adds/reorganizes an entity. |

### Actors

- Finance administrator
- Controller
- LedgArr

### State path

`draft → validation → testing → active → future_change → inactive`

### Required sequence

1. Create legal entity with registered identity, functional/reporting currency, ownership, fiscal calendar, and accounting books.
2. Define periods, close policy, numbering, bank/tax/reporting refs, and intercompany relationships.
3. Create/import chart of accounts and governed dimensions/values.
4. Map operational organizations/sites/projects without duplicating StaffArr structures.
5. Validate account types, retained earnings, currency, hierarchy, effective dates, and duplicate mappings.
6. Run sample posting previews and opening-balance dry run.
7. Approve/activate configuration.
8. Version future changes and preserve historical reporting mappings.

### Exception and recovery paths

- Duplicate legal entity, invalid ownership cycle, period overlap, missing retained earnings/account mapping, currency conflict, or opening balances do not reconcile.
- A regulator/governing body is incorrectly entered as a legal entity.

### Cross-product and external handoffs

- StaffArr/CustomArr/SupplyArr → LedgArr: reference mappings.
- RecordArr: formation/opening docs.
- ReportArr: finance models.

### Evidence and audit record

- Configuration/version.
- Validation/test postings.
- Opening balance/reconciliation.
- Approval/activation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to finance-ready.
- Mapping errors.
- Opening balance variance.
- Configuration changes after go-live.

## LE-WF-002 — Ingest, validate, map, approve, and post a financial packet

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Durable |
| Purpose | Turn product-owned operational facts into traceable subledger/GL entries. |
| Trigger | OrdArr, SupplyArr, LoadArr, RoutArr, MaintainArr, StaffArr/payroll, or an integration emits a finance packet. |

### Actors

- Source product
- Finance reviewer
- LedgArr

### State path

`received → validation → mapping → approval → posted → rejected → reversed → replaced`

### Required sequence

1. Accept packet idempotently with tenant, legal entity, source records/events, dates, currency, amounts/quantities, and evidence refs.
2. Validate schema, totals, period, entity, currency/rate, duplicate, source version, and required mappings.
3. Apply versioned posting/mapping rules and generate preview lines/dimensions.
4. Explain each proposed line and any missing/ambiguous mapping.
5. Auto-approve within policy or route exceptions/thresholds for review.
6. Commit subledger and balanced journal atomically; lock source packet version.
7. Acknowledge posting/reference to source product.
8. Handle correction through reversal/replacement packet and reconciliation.

### Exception and recovery paths

- Closed period, unbalanced packet, missing account/dimension/rate, duplicate source, changed source after preview, approval conflict, or posting database failure.
- Packet may remain accepted operationally but finance-pending.

### Cross-product and external handoffs

- Source products → LedgArr: packet.
- LedgArr → source: validation/posting status.
- RecordArr: evidence.
- ReportArr: financial projections.

### Evidence and audit record

- Packet/source/version.
- Validation/mapping/rule version.
- Approvals.
- Subledger/journal IDs.
- Correction chain.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Straight-through posting.
- Mapping exception.
- Posting latency.
- Duplicate prevention.
- Correction rate.

## LE-WF-003 — Manual or recurring journal entry

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Create controlled non-packet journals with support, approval, and reversal. |
| Trigger | Authorized finance user creates a manual/recurring/accrual/reclass journal. |

### Actors

- Accountant
- Approver
- Controller
- LedgArr

### State path

`draft → validation → approval → posted → reversed → rejected`

### Required sequence

1. Select entity/book/period, journal type, date, currency, reason, and source/support refs.
2. Enter balanced lines with accounts, dimensions, debit/credit, descriptions, and intercompany context.
3. Validate period/account/dimension status, currency/rates, duplicate/reference, and SoD.
4. Preview financial statement and control impact.
5. Route approval based on type/value/risk.
6. Post immutable journal and update balances.
7. Schedule reversal/recurrence when configured.
8. Correct by reversal and replacement, never edit posted lines.

### Exception and recovery paths

- Closed period, unbalanced lines, invalid dimension, approver conflict, duplicate accrual, missing support, or recurrence collides with changed configuration.
- Emergency entry in a closing period requires documented exception.

### Cross-product and external handoffs

- LedgArr ↔ RecordArr: support.
- LedgArr → ReportArr: journal/balance event.

### Evidence and audit record

- Journal/version.
- Validation/preview.
- Approval.
- Posting/reversal chain.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Approval time.
- Rejected journals.
- Manual journal volume.
- Late/post-close adjustments.
- Recurring failure.

## LE-WF-004 — Supplier bill intake, match, approval, and posting

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Create an AP liability from valid supplier billing evidence and operational receipts. |
| Trigger | Invoice arrives by portal/email/import/API or finance user enters a bill. |

### Actors

- AP clerk
- Buyer/receiver
- Approver
- Supplier
- LedgArr

### State path

`received → matching → exception → approval → posted → payment_hold → paid → credited`

### Required sequence

1. Capture invoice supplier, number, date, currency, lines, tax, terms, PO/receipt refs, and document.
2. Detect duplicates and validate supplier/payment details and change-risk controls.
3. Match to SupplyArr PO/revisions and LoadArr receipts/returns/tolerances.
4. Route price/quantity/tax/quality/receipt exceptions to owning reviewers.
5. Code non-PO lines using governed accounts/dimensions and approval.
6. Approve and post AP subledger/GL liability.
7. Schedule eligible payment and expose dispute/hold status.
8. Reconcile later credits/adjustments without rewriting original bill.

### Exception and recovery paths

- Duplicate invoice, supplier banking change, missing PO/receipt, tolerance exceeded, held goods, tax mismatch, closed period, or invoice currency/rate issue.
- Emergency/non-PO bill requires enhanced approval.

### Cross-product and external handoffs

- SupplyArr/LoadArr → LedgArr: PO/receipt refs.
- LedgArr ↔ supplier portal/email/payment provider.
- AssurArr: quality hold context.
- RecordArr: invoice.

### Evidence and audit record

- Invoice/source/document.
- Match/tolerance.
- Exceptions/decisions.
- Approvals/posting.
- Payment/credit chain.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Touchless match.
- Duplicate prevention.
- Exception cycle.
- Days payable.
- Late payment.

## LE-WF-005 — Payment proposal, approval, execution, and reconciliation

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Pay approved obligations securely with separation of duties and provider confirmation. |
| Trigger | AP creates a payment run or due obligations meet scheduled criteria. |

### Actors

- AP clerk
- Treasury approver
- Payment signer
- Bank/provider
- LedgArr

### State path

`proposal → approval → submitted → accepted → settled → rejected → returned → voided`

### Required sequence

1. Select entity/bank/currency/payment date and eligible approved bills/credits.
2. Apply due date, discount, cash, hold, supplier, method, amount, fraud, and duplicate rules.
3. Generate proposal and cash impact preview.
4. Route approvals/signatures with SoD and limits.
5. Create payment instructions/file/API request; never expose bank secrets to browser/logs.
6. Capture provider accepted/rejected/settled status and remittance.
7. Post payment/clearing entries and apply to AP.
8. Reconcile bank transaction and handle void/reissue/return through explicit workflow.

### Exception and recovery paths

- Insufficient cash, bank details recently changed, duplicate payment, provider unavailable, rejected account, partial settlement, returned payment, or signatory conflict.
- Manual check has controlled print/void numbering.

### Cross-product and external handoffs

- LedgArr ↔ bank/payment provider.
- LedgArr → supplier: remittance.
- RecordArr: payment file/evidence.
- NexArr: service credentials.

### Evidence and audit record

- Proposal/selection.
- Fraud/SoD checks.
- Approvals/instructions.
- Provider results.
- Posting/reconciliation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- On-time payment.
- Rejected/returned rate.
- Discount capture.
- Approval cycle.
- Duplicate prevented.

## LE-WF-006 — Customer invoice, delivery, and accounts receivable

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Create a correct receivable from an approved order/completion/billing event. |
| Trigger | OrdArr or another authorized source sends an invoice-ready packet or finance creates an approved billing event. |

### Actors

- Billing clerk
- Approver
- Customer contact
- LedgArr

### State path

`ready → validation → approval → posted → delivered → disputed → paid → credited`

### Required sequence

1. Validate customer finance ref, legal entity, agreement/order/completion, lines, tax, currency, terms, and required proof.
2. Apply billing rules, numbering, tax, discounts, retainage/deposit/credit context, and dimensions.
3. Preview and route exceptions/approval.
4. Post AR invoice/subledger/GL and freeze invoice version.
5. Generate RecordArr document and deliver through portal/email/e-invoice integration.
6. Track delivery/open/dispute and due dates.
7. Issue credit/rebill through explicit documents.
8. Feed statements, collections, and cash application.

### Exception and recovery paths

- Customer billing profile missing, order incomplete, proof missing, tax jurisdiction uncertain, duplicate invoice, closed period, or e-invoice rejection.
- Progress/milestone billing requires schedule.

### Cross-product and external handoffs

- OrdArr/CustomArr → LedgArr: invoice packet/customer refs.
- LedgArr ↔ RecordArr/e-invoice provider/customer portal.
- ReportArr: billing metrics.

### Evidence and audit record

- Source packet.
- Calculation/tax/rules.
- Approval/posting.
- Document/delivery.
- Credits/disputes.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Invoice accuracy.
- Time to invoice.
- Delivery failures.
- Dispute rate.
- Days sales outstanding.

## LE-WF-007 — Customer payment intake and cash application

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Record cash and apply it accurately to customer receivables. |
| Trigger | Bank feed, payment provider, lockbox/import, or finance user records a customer payment. |

### Actors

- AR clerk
- Customer
- Bank/provider
- LedgArr

### State path

`received → matching → review → applied → unapplied → reversed → reconciled`

### Required sequence

1. Capture payer, amount, currency, date, method, bank transaction/reference, remittance, and fees.
2. Detect duplicates and validate settlement status.
3. Suggest customer/invoice applications using reference, amount, payer, date, and remittance.
4. Apply full, partial, multi-invoice, unapplied, on-account, short-pay, or overpay with reason.
5. Route deductions/disputes/unknown payer for review.
6. Post cash/clearing/AR application entries.
7. Notify customer/account team as configured.
8. Reconcile provider/bank settlement and later reversals/chargebacks.

### Exception and recovery paths

- Unknown payer, duplicate, currency mismatch, fee/net settlement, short pay, chargeback, returned check, or payment spans entities.
- Customer intentionally pays on account.

### Cross-product and external handoffs

- Bank/provider → LedgArr.
- LedgArr → CustomArr: approved account status signal.
- RecordArr: remittance.
- ReportArr: cash metrics.

### Evidence and audit record

- Payment/source.
- Match suggestions/decisions.
- Applications.
- Posting/reconciliation/reversal.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Auto-application.
- Unapplied cash aging.
- Match override.
- Chargeback rate.
- Reconciliation time.

## LE-WF-008 — Collections, dispute, promise-to-pay, and write-off

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Manage overdue receivables with customer context, fairness, and controlled accounting. |
| Trigger | Invoice ages past policy, customer disputes, or collector opens an account. |

### Actors

- Collector
- Account manager
- Customer contact
- Approver
- LedgArr

### State path

`open → contacted → dispute → promise → plan → escalated → resolved → written_off`

### Required sequence

1. Prioritize accounts using amount, age, risk, relationship, dispute, and commitment context with explainable rules.
2. Review invoices, delivery/proof, payments, credits, cases, and customer contact preferences.
3. Contact through permitted channels and record communication.
4. Create dispute, task, promise-to-pay, installment, hold recommendation, or escalation.
5. Coordinate operational/account owner response without exposing restricted finance details broadly.
6. Track promise fulfillment and update collections stage.
7. Approve credit, adjustment, write-off, agency/legal handoff where authorized.
8. Close with outcome and audit.

### Exception and recovery paths

- Customer disputes service/quality, bankruptcy/legal hold, payment already sent, wrong contact, vulnerable customer policy, or write-off exceeds authority.
- Operational hold decision remains with owning policy/product.

### Cross-product and external handoffs

- LedgArr ↔ CustomArr/OrdArr/AssurArr/RecordArr.
- LedgArr → ReportArr: collections facts.

### Evidence and audit record

- Priority/rationale.
- Communications/preferences.
- Dispute/promise/actions.
- Approval/accounting outcome.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- DSO.
- Promise kept.
- Dispute resolution.
- Recovery rate.
- Write-off rate.

## LE-WF-009 — Bank transaction import and reconciliation

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Prove the ledger and bank agree and explain every unmatched item. |
| Trigger | Bank feed/file/API import arrives or reconciliation period begins. |

### Actors

- Cash accountant
- Reviewer
- Bank integration
- LedgArr

### State path

`imported → matching → review → reconciled → signed_off → reopened`

### Required sequence

1. Import transactions idempotently with account, date, amount, currency, reference, counterparty, and provider IDs.
2. Detect duplicates, reversals, pending/posted transitions, and opening balance.
3. Suggest matches to payments, deposits, transfers, fees, payroll, and journal entries.
4. Create approved entries for bank-only items using governed rules.
5. Split/merge matches and explain confidence.
6. Investigate unmatched/stale differences and assign owner.
7. Complete reconciliation to statement ending balance with reviewer signoff.
8. Lock snapshot and preserve later correction/reopen.

### Exception and recovery paths

- Feed gap, duplicate pending/posted entries, wrong account, foreign currency, timing difference, unknown fee, stale check, or balance does not reconcile.
- Manual statement import used when no bank API.

### Cross-product and external handoffs

- LedgArr ↔ bank provider.
- LedgArr ↔ AP/AR/payroll/source products.
- RecordArr: statement/reconciliation package.

### Evidence and audit record

- Import/provider IDs.
- Match decisions.
- Created entries.
- Difference resolution.
- Signoff/reopen.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Auto-match.
- Unreconciled items.
- Days to reconcile.
- Feed gaps.
- Reopen rate.

## LE-WF-010 — Inventory valuation, COGS, and quantity-to-value reconciliation

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Translate LoadArr quantity movements and procurement costs into accurate financial value. |
| Trigger | LoadArr movement/period event or cost update requires valuation. |

### Actors

- Cost accountant
- Inventory controller
- LedgArr
- LoadArr

### State path

`received → valued → posted → variance → investigation → reconciled → closed`

### Required sequence

1. Ingest item/location/status/lot/quantity movement with source transaction and legal entity context.
2. Resolve valuation method, cost layer, currency/rate, landed cost, and accounting mappings.
3. Create/consume/transfer/adjust cost layers and calculate COGS/variance as appropriate.
4. Post subledger/GL entries with source lineage.
5. Reconcile on-hand quantities by item/location/status to financial inventory value.
6. Investigate timing, missing movement, UOM, negative inventory, cost, return, transfer, or mapping variances.
7. Post approved corrections/revaluations through explicit packets/journals.
8. Close period inventory reconciliation with signoff.

### Exception and recovery paths

- Out-of-order movement, negative inventory, missing cost, retroactive receipt, currency/rate change, return to prior layer, intercompany transfer, or LoadArr correction.
- Physical count adjustment has quality/incident context.

### Cross-product and external handoffs

- LoadArr/SupplyArr → LedgArr: quantity/cost facts.
- LedgArr → LoadArr/SupplyArr: rejection/reconciliation status.
- ReportArr: valuation metrics.

### Evidence and audit record

- Movement/source.
- Cost method/layers/rules.
- Posting.
- Quantity-value comparison.
- Corrections/signoff.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Valuation latency.
- Uncosted movements.
- Quantity-value variance.
- Negative inventory impact.
- Close readiness.

## LE-WF-011 — Landed cost allocation

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Allocate freight, duty, brokerage, and other acquisition costs to inventory or expense with evidence. |
| Trigger | A freight/vendor/customs bill or shipment completion provides landed-cost facts. |

### Actors

- Cost accountant
- AP clerk
- LedgArr

### State path

`draft → validation → preview → approval → allocated → posted → reversed`

### Required sequence

1. Identify source shipment/PO/receipt/items/quantities/values and cost components.
2. Validate that costs are eligible and not already allocated.
3. Choose approved allocation basis such as value, quantity, weight, volume, line, or custom driver.
4. Preview allocation and inventory/COGS impact, including sold/remaining quantities.
5. Approve exceptions and post cost-layer adjustments/expense split.
6. Link AP bill/transport contribution and source evidence.
7. Reconcile totals and publish updated cost/profitability facts.
8. Correct by reversal/reallocation.

### Exception and recovery paths

- Missing shipment/receipt link, partial receipts, sold inventory, multi-entity/currency, duplicate freight invoice, or allocation basis unavailable.
- Some costs expense immediately by policy.

### Cross-product and external handoffs

- RoutArr/SupplyArr/LoadArr → LedgArr: shipment/receipt/cost refs.
- RecordArr: invoices/customs docs.
- ReportArr: margin/cost.

### Evidence and audit record

- Source costs.
- Allocation basis/calculation.
- Approval.
- Layer/posting/reconciliation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Allocation cycle.
- Unallocated cost.
- Duplicate prevention.
- Margin adjustment.
- Correction rate.

## LE-WF-012 — Fixed asset capitalize, depreciate, transfer, impair, and dispose

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Manage the accounting lifecycle of capital assets while referencing MaintainArr operational assets. |
| Trigger | Approved capital purchase/project completion or finance event creates/changes a fixed asset. |

### Actors

- Fixed asset accountant
- Approver
- LedgArr
- MaintainArr

### State path

`draft → capitalized → in_service → depreciating → transferred → impaired → disposed`

### Required sequence

1. Create fixed asset with entity/book/class, acquisition/capitalization date, basis, useful life, method, salvage, dimensions, and MaintainArr asset ref when applicable.
2. Validate source bill/project/receipt and capitalization policy.
3. Approve and post capitalization.
4. Run depreciation preview/approval/posting by period/book.
5. Process transfers, componentization, improvements, impairment, revaluation, or life changes with effective dates.
6. Reconcile operational existence/location/status without making MaintainArr accounting owner.
7. Dispose/sell/retire with proceeds, gain/loss, tax/book differences, and approvals.
8. Preserve complete schedule/posting/evidence history.

### Exception and recovery paths

- Asset not in service, component split, retroactive capitalization, missing operational asset, partial disposal, lost/stolen asset, closed period, or disposal before return/clearance.
- Low-value asset is expensed, not capitalized.

### Cross-product and external handoffs

- SupplyArr/LoadArr/Projects → LedgArr: acquisition.
- MaintainArr ↔ LedgArr: asset refs/status.
- RecordArr: support.
- ReportArr: asset financials.

### Evidence and audit record

- Source/capitalization decision.
- Depreciation schedules/postings.
- Changes/approvals.
- Disposal/gain-loss.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Uncapitalized project aging.
- Depreciation success.
- Asset reconciliation.
- Disposal cycle.
- Book-tax differences.

## LE-WF-013 — Project/job budget, cost collection, and profitability

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Track financial performance for projects, jobs, contracts, or major work. |
| Trigger | Authorized user creates a project/job or operational packets reference one. |

### Actors

- Project manager
- Project accountant
- Approver
- LedgArr

### State path

`planned → approved → active → at_risk → complete → financial_close → closed`

### Required sequence

1. Create project/job with entity, customer/internal sponsor, dates, manager, dimensions, budget, billing/revenue policy refs, and status.
2. Approve baseline budget and revisions.
3. Collect labor, parts/material, procurement, transport, vendor, expense, overhead, and other source packets.
4. Validate project/task/category mappings and commit controls.
5. Calculate actual, committed, forecast, WIP, revenue refs, and variance/profitability.
6. Route budget overruns/change requests for approval.
7. Reconcile source products and billing/order refs.
8. Close project after costs, billing, assets, obligations, and documentation complete.

### Exception and recovery paths

- Missing project mapping, cost arrives after close, budget unavailable, scope change, intercompany labor/cost, unbilled WIP, or disputed customer billing.
- Maintenance work order may be cost object without a formal project.

### Cross-product and external handoffs

- StaffArr/MaintainArr/SupplyArr/RoutArr/OrdArr → LedgArr: cost/revenue refs.
- CustomArr: customer/agreement.
- RecordArr: project package.

### Evidence and audit record

- Budget versions.
- Source cost/revenue packets.
- Variance/change approvals.
- Close reconciliation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Budget variance.
- Cost timeliness.
- Margin forecast accuracy.
- Unbilled WIP.
- Close time.

## LE-WF-014 — Period close, reconciliation, consolidation, and reopen

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Close a financial period with evidence, dependencies, approvals, and controlled reopen. |
| Trigger | Close calendar opens or controller starts a close. |

### Actors

- Controller
- Accountants
- Reviewers
- LedgArr

### State path

`not_started → in_progress → blocked → review → locked → reopened → reclosed`

### Required sequence

1. Instantiate close checklist by entity/book/period with owners/dependencies and due dates.
2. Validate operational packet completeness, AP/AR, bank, inventory, fixed assets, payroll, tax, intercompany, and prior exceptions.
3. Complete reconciliations, accruals, allocations, depreciation, valuation, and review journals.
4. Resolve or approve residual exceptions with impact.
5. Perform intercompany matching/elimination and currency translation/consolidation.
6. Review trial balance and financial statements.
7. Approve and lock period; generate close/audit package.
8. If reopen is approved, show downstream statement/report impact, post corrections, and reclose with history.

### Exception and recovery paths

- Late source packet, unreconciled bank, inventory variance, unbalanced intercompany, failed depreciation, tax issue, missing approval, or post-close error.
- Some subledgers may soft-close before GL.

### Cross-product and external handoffs

- All source products → LedgArr: completeness/status.
- LedgArr → ReportArr/RecordArr: statements/package.
- NexArr/StaffArr: approvals/access.

### Evidence and audit record

- Checklist/dependencies.
- Reconciliations/exceptions.
- Adjustments/approvals.
- Consolidation/translation.
- Lock/reopen history.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Days to close.
- Late adjustments.
- Open exceptions.
- Reopen rate.
- Packet completeness.

## LE-WF-015 — Intercompany transaction, matching, settlement, and elimination

| Field | Definition |
| --- | --- |
| Classification | CURRENT · DEMOCRATIZE |
| Implementation state | Durable |
| Purpose | Record and reconcile activity between legal entities without manual spreadsheet elimination. |
| Trigger | A source packet or finance user identifies an intercompany transaction. |

### Actors

- Entity accountants
- Intercompany accountant
- Approver
- LedgArr

### State path

`created → posted_one_side → matched → difference → settled → eliminated → closed`

### Required sequence

1. Identify sending/receiving entities, transaction type, agreement/rule, currency, amounts, tax, and source refs.
2. Generate mirrored due-to/due-from or invoice/bill entries using entity-specific mappings.
3. Validate dates/rates/dimensions and obtain approvals.
4. Post both sides with a shared intercompany ID or route unmatched external-side exception.
5. Match balances and investigate timing/currency/value differences.
6. Settle through bank/netting entries where applicable.
7. Generate consolidation elimination entries.
8. Close period reconciliation and preserve corrections.

### Exception and recovery paths

- One side missing, entities use different periods/currencies, tax treatment differs, rate changes, source packet corrected, or settlement crosses banks.
- Partial ownership/consolidation scope requires special treatment.

### Cross-product and external handoffs

- Operational source products → LedgArr.
- LedgArr ↔ bank provider.
- ReportArr: entity/consolidation views.

### Evidence and audit record

- Source/shared ID.
- Mirrored postings.
- Match differences.
- Settlement/elimination.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Unmatched aging.
- Manual elimination.
- Currency variance.
- Settlement cycle.
- Close delay.

## LE-WF-016 — Payroll financial packet and journal reconciliation

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Post approved payroll results to finance without making LedgArr the payroll calculator. |
| Trigger | External payroll or StaffArr-approved export produces a payroll result packet. |

### Actors

- Payroll administrator
- Accountant
- StaffArr/external payroll
- LedgArr

### State path

`received → validation → mapping → approval → posted → reconciliation → corrected`

### Required sequence

1. Receive pay period, legal entity, payroll run, employee aggregate/detail according to privacy, earnings/deductions/taxes/benefits/employer costs, payments/liabilities, and source refs.
2. Validate run identity, totals, closed period, duplicates, code mappings, dimensions, and balancing.
3. Map payroll codes to accounts/dimensions and preview journal.
4. Route mapping/variance exceptions to payroll/finance owner.
5. Approve and post payroll journal/subledger/clearing/liabilities.
6. Reconcile cash/payment/tax/benefit provider outcomes.
7. Correct through replacement run or reversal/delta packet.
8. Retain privacy-appropriate audit package.

### Exception and recovery paths

- Duplicate run, missing code mapping, off-cycle payroll, retro adjustment, multi-entity worker, net pay mismatch, tax liability mismatch, or closed period.
- Finance users may not be authorized to see individual compensation detail.

### Cross-product and external handoffs

- StaffArr/external payroll → LedgArr.
- LedgArr ↔ bank/tax/benefit providers.
- RecordArr: restricted package.

### Evidence and audit record

- Run/source/totals.
- Mappings/preview.
- Approval/posting.
- Provider reconciliation/corrections.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Posting latency.
- Mapping exceptions.
- Reconciliation variance.
- Privacy access violations.
- Off-cycle volume.

## LE-WF-017 — Budget creation, revision, commitment control, and forecast

| Field | Definition |
| --- | --- |
| Classification | CURRENT · DEMOCRATIZE |
| Implementation state | Partial |
| Purpose | Plan financial resources and compare commitments/actuals without spreadsheet fragmentation. |
| Trigger | Finance starts a budget cycle or owner requests a revision. |

### Actors

- Finance planner
- Department/project owner
- Approver
- LedgArr

### State path

`planning → owner_input → review → approved → active → revised → closed`

### Required sequence

1. Define scenario/version, entities, accounts/dimensions/projects, period granularity, assumptions, and owners.
2. Seed from history, run rate, approved operational plans, or zero-based input.
3. Collect owner submissions with comments and supporting evidence.
4. Run validation, allocation, currency, and consolidation rules.
5. Compare scenarios and route approval/calibration.
6. Publish approved budget and commitment-control policy.
7. Track actual, open PO/order/project commitments, forecast, and variance.
8. Process controlled revisions/supplements/transfers and retain versions.

### Exception and recovery paths

- Owner missing, allocation does not reconcile, currency assumptions change, budget conflict, confidential payroll plan, or commitment exceeds available budget.
- Forecast remains separate from approved budget.

### Cross-product and external handoffs

- StaffArr/SupplyArr/OrdArr/Projects → LedgArr: planning/commitment refs.
- ReportArr: dashboards/scenarios.
- RecordArr: support.

### Evidence and audit record

- Scenario/assumptions.
- Submissions/allocations.
- Approvals/versions.
- Commitments/actuals/forecasts.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Cycle time.
- Submission completion.
- Forecast accuracy.
- Budget variance.
- Unapproved commitment blocks.

## LE-WF-018 — Tax calculation, exemption, reporting, and filing package

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Apply and document transaction tax decisions and prepare filing/reconciliation output. |
| Trigger | Invoice/bill/order/asset transaction requires tax or period reporting begins. |

### Actors

- Tax/finance administrator
- LedgArr
- External tax provider

### State path

`calculated → review → posted → reconciled → filed → rejected → amended`

### Required sequence

1. Resolve legal entity, jurisdiction, customer/vendor exemptions, item/service tax category, location, date, and amount.
2. Calculate tax using internal configuration or external engine with source/version.
3. Validate exemption certificates/requirements through RecordArr/Compliance Core refs.
4. Post tax liability/recoverable lines and preserve calculation detail.
5. Aggregate by jurisdiction/period and reconcile GL to transaction detail.
6. Prepare return/e-invoice/filing package and approval.
7. Submit through provider or export; record acknowledgement/payment refs.
8. Correct amended periods through explicit adjustment.

### Exception and recovery paths

- Jurisdiction ambiguous, exemption expired, provider unavailable, rate changed retroactively, rounding difference, multi-jurisdiction service, or filing rejected.
- Legal advice is outside automated system output.

### Cross-product and external handoffs

- LedgArr ↔ tax/e-invoice provider.
- CustomArr/SupplyArr/OrdArr → LedgArr: tax context.
- RecordArr/Compliance Core: certificates/rules refs.

### Evidence and audit record

- Inputs/source/rate.
- Calculation detail.
- Reconciliation.
- Filing package/acknowledgement.
- Amendment.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Calculation exceptions.
- Exemption expiry.
- GL-return variance.
- On-time filing.
- Rejected filings.

## LE-WF-019 — External ERP/GL bridge, coexistence, and migration

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Exchange finance data safely with an external accounting system during integration or transition. |
| Trigger | A scheduled/manual sync or migration run starts. |

### Actors

- Integration administrator
- Finance administrator
- External ERP
- LedgArr

### State path

`draft → testing → dry_run → active → degraded → reconciliation → cutover → retired`

### Required sequence

1. Define system of record by object/period/entity and prohibit dual-write ambiguity.
2. Configure credentials, mappings, dimensions, currencies, numbering, and sync direction.
3. Run connection/schema tests and dry-run extraction/transform.
4. Export/import master/transaction/balance packets with idempotent external IDs.
5. Validate totals and row-level errors before commit/post.
6. Reconcile LedgArr and external balances/statuses.
7. Resolve conflicts through owned correction workflow.
8. Monitor health, rotate credentials, and execute cutover/decommission plan.

### Exception and recovery paths

- External API limit/schema drift, duplicate IDs, period closed on one side, mapping changed, partial batch, currency/rounding variance, or network outage.
- Migration opening balances require signed control totals.

### Cross-product and external handoffs

- LedgArr ↔ external ERP/GL.
- NexArr: credentials/health.
- RecordArr: migration manifests.
- ReportArr: sync monitoring.

### Evidence and audit record

- Ownership/config/mappings.
- Run payload/hashes.
- Validation/reconciliation.
- Conflict/correction.
- Cutover evidence.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Sync success.
- Unmapped records.
- Balance variance.
- Latency.
- Manual correction.

## LE-WF-020 — Financial audit package and source-to-ledger trace

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Produce a complete, permissioned trail from financial statement line to operational source and evidence. |
| Trigger | Auditor/controller selects entity, account, period, transaction, control, or source scope. |

### Actors

- Auditor
- Controller
- Accountant
- LedgArr
- RecordArr

### State path

`requested → collecting → validation → assembled → review → shared → supplemented → closed`

### Required sequence

1. Define scope and snapshot date, materiality, recipient, and redaction/privacy rules.
2. Collect configuration/rules, balances, journals, subledger, packets, approvals, reconciliations, controls, exceptions, and source refs.
3. Resolve operational records and RecordArr evidence without copying their truth.
4. Validate missing evidence, orphan refs, post-close changes, and access restrictions.
5. Generate drillable manifest from statement/account/journal/line to source packet/event/record/document.
6. Request RecordArr package/export and reviewer signoff.
7. Log access/questions/supplemental requests.
8. Preserve original snapshot and close audit response.

### Exception and recovery paths

- Missing source event, disposed document, confidential payroll/customer/supplier data, legal hold, external system unavailable, or changed mapping after period.
- Auditor receives read-only scoped access.

### Cross-product and external handoffs

- LedgArr ↔ all source products/RecordArr/ReportArr.
- NexArr/StaffArr: scoped access.

### Evidence and audit record

- Scope/snapshot.
- Lineage/manifest.
- Evidence/gaps/exceptions.
- Package/access/questions.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Package generation.
- Trace completeness.
- Audit adjustments.
- Question cycle.
- Unauthorized access attempts.



## Workflow definition of done

A workflow is releasable only when its happy path, every listed exception, dependency outage, duplicate/retry, stale/concurrent update, permission denial, cancellation, correction/void, archival/retention, import/API/event path, notification, audit trail, reporting projection, mobile behavior, and professional printable output have automated and human-usable acceptance coverage.
