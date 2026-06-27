# SupplyArr — SRM Workflow Catalog

[Feature catalog](./FEATURESET.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md) · [Rollout](./ROLLOUT.md)

## Workflow contract

This document defines the end-to-end business state machines for SupplyArr. A workflow is not complete because a page or endpoint exists: the trigger, authority, state transitions, validation, handoffs, evidence, exceptions, retries, conflict behavior, notifications, reporting, and final source-of-truth commit must all work together.

## Ownership rule

SupplyArr is the tenant system of record for suppliers/vendors/dealers, sourced item/part commercial context, supplier onboarding and restrictions, sourcing events, purchase requests, approvals, RFQs, quotes, contracts, purchase orders, vendor acknowledgements, procurement exceptions, returns/warranty claims, supplier performance, and procurement coordination. LoadArr owns all physical inventory and warehouse movements.

- Warehouse locations, inventory balance, stock ledger, reservations, receiving execution, putaway, picking, staging, or shipping; LoadArr owns them.
- Internal site/location identity; StaffArr owns canonical internal locations.
- Quality nonconformance, hold/release, CAPA, or SCAR decision; AssurArr owns quality workflows while SupplyArr owns supplier commercial consequences.
- Customer account truth; CustomArr owns customers.
- Transportation dispatch/trips; RoutArr owns movement execution.
- Bills, payments, or general-ledger posting; LedgArr owns finance execution.

## Workflow index

| Workflow ID | Workflow | Class | State | Trigger |
| --- | --- | --- | --- | --- |
| SU-WF-001 | Supplier registration, due diligence, approval, and activation | CURRENT · COMMON | Durable | A buyer invites a supplier or an external party submits a registration. |
| SU-WF-002 | Cross-product demand to purchase request | CURRENT · UNDERSERVED | Durable | MaintainArr, RoutArr, TrainArr, StaffArr, OrdArr, or a user submits a procurement demand. |
| SU-WF-003 | Purchase request approval | CURRENT · COMMON | Durable | A purchase request reaches approval-ready status. |
| SU-WF-004 | RFQ creation, supplier invitation, clarification, and quote intake | CURRENT · COMMON | Durable | An approved request requires competitive sourcing or buyer creates an RFQ. |
| SU-WF-005 | Quote normalization, comparison, negotiation, and award | CURRENT · COMMON | Durable | RFQ closes with one or more valid quotes. |
| SU-WF-006 | Purchase order create, approve, issue, and revise | CURRENT · COMMON | Durable | An approved request/award/contract release is converted to a PO. |
| SU-WF-007 | Vendor readiness, shipment intent, and inbound transport handoff | CURRENT · UNDERSERVED | Durable | An issued PO requires vendor readiness confirmation or supplier submits shipment details. |
| SU-WF-008 | Receipt outcome, three-way context, and procurement exception | CURRENT · COMMON | Partial | LoadArr posts partial/final receipt or discrepancy. |
| SU-WF-009 | Backorder and supplier delay management | CURRENT · UNDERSERVED | Durable | Supplier reports delay/partial, due date passes, or receipt is short. |
| SU-WF-010 | Supplier restriction, suspension, exception, and reinstatement | CURRENT · COMMON | Durable | Risk, quality, compliance, finance, incident, or administrative review creates a restriction. |
| SU-WF-011 | Supplier performance review and improvement plan | CURRENT · COMMON | Partial | A scheduled review period ends or performance crosses a threshold. |
| SU-WF-012 | Supplier corrective action request handoff | COMMON · UNDERSERVED | Partial | A significant or recurring supplier issue requires formal response. |
| SU-WF-013 | Contract create/reference, obligation, renewal, and expiry | CURRENT · COMMON | Partial | A sourcing award, negotiated agreement, or imported contract is approved. |
| SU-WF-014 | Vendor return, replacement, credit, and warranty claim | CURRENT · COMMON | Durable | A receiving/usage/maintenance/quality issue is approved for supplier return or warranty. |
| SU-WF-015 | Scheduled price, lead-time, and availability snapshot | CURRENT · COMMON | Durable | A scheduled capture run or manual refresh occurs. |
| SU-WF-016 | Procurement audit package and supplier history | CURRENT · COMMON | Durable | An audit, dispute, review, or management request selects a scope. |

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

## SU-WF-001 — Supplier registration, due diligence, approval, and activation

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Create an approved supplier through a scoped, evidence-backed onboarding process. |
| Trigger | A buyer invites a supplier or an external party submits a registration. |

### Actors

- Supplier contact
- Buyer/category owner
- Compliance/risk reviewer
- Finance reviewer
- SupplyArr

### State path

`invited → registration → review → remediation → approved → conditional → rejected → blocked`

### Required sequence

1. Create a scoped invitation with tenant, purpose, due date, and required sections.
2. Supplier enters legal/contact/payment-capability information and uploads documents through secure portal.
3. Deduplicate against existing parties and verify identifiers where providers exist.
4. Evaluate required documents, sanctions/risk/insurance/certification, capability, and internal conflicts.
5. Route commercial, quality, compliance, finance, and security reviews according to supplier type/risk.
6. Approve, conditionally approve, request remediation, reject, or block with reasons.
7. Activate approved categories/sites/contacts and publish supplier status.
8. Schedule document expirations and periodic reassessment.

### Exception and recovery paths

- Duplicate supplier, unverifiable identity, expired insurance, banking change risk, sanctions hit requiring review, missing capability, conflict of interest, or supplier abandons onboarding.
- Supplier is approved only for certain categories/sites/amounts.

### Cross-product and external handoffs

- SupplyArr ↔ RecordArr: documents.
- SupplyArr ↔ AssurArr/Compliance Core: quality/compliance context.
- SupplyArr ↔ NexArr: scoped portal identity.
- SupplyArr → StaffArr/LedgArr: authority/finance refs.

### Evidence and audit record

- Invitation/access.
- Submitted facts/documents.
- Verification/review decisions.
- Approval scope/conditions.
- Expiration/reassessment.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Onboarding cycle time.
- First-pass completeness.
- Conditional/blocked rate.
- Document expiry closure.
- Supplier effort.

## SU-WF-002 — Cross-product demand to purchase request

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Durable |
| Purpose | Turn an operational need into a traceable request without losing source priority or ownership. |
| Trigger | MaintainArr, RoutArr, TrainArr, StaffArr, OrdArr, or a user submits a procurement demand. |

### Actors

- Requester
- Buyer
- SupplyArr
- Source product

### State path

`received → validated → consolidated → approval → sourcing → ordered → canceled`

### Required sequence

1. Receive source record/line, item/spec, quantity, need-by, priority, ship-to/use location, substitutes, and business impact.
2. Resolve item/catalog/source candidates and detect duplicate/open demand.
3. Check LoadArr availability/transfer/substitution before purchase where appropriate.
4. Create or consolidate purchase request lines while preserving allocation to each source.
5. Validate required approvals, budget/authority context, preferred contract/source, and compliance/quality needs.
6. Return request/status to source product.
7. Route for approval/sourcing/order.
8. Publish changes, cancellation, shortage, and expected-date updates.

### Exception and recovery paths

- Unknown item/spec, duplicate demand, quantity conflict, no approved supplier, budget unavailable, urgent need, or source request canceled after consolidation.
- Emergency purchase uses controlled exception.

### Cross-product and external handoffs

- Source product → SupplyArr: demand.
- SupplyArr ↔ LoadArr: stock/transfer check.
- SupplyArr → source: status.
- SupplyArr → LedgArr: budget/commitment context.

### Evidence and audit record

- Source demand/version.
- Catalog/match decision.
- Consolidation allocations.
- Approval/sourcing/order refs.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Demand-to-PR time.
- Duplicate avoidance.
- Stock/transfer avoidance of purchase.
- Need-by risk.

## SU-WF-003 — Purchase request approval

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Approve or reject a purchase using authority, policy, budget, risk, and separation of duties. |
| Trigger | A purchase request reaches approval-ready status. |

### Actors

- Requester
- Manager
- Budget owner
- Buyer
- Approver
- SupplyArr

### State path

`submitted → in_approval → changes_requested → approved → rejected → expired → canceled`

### Required sequence

1. Validate requester, lines, estimates, business need, source records, and attachments.
2. Determine approval route from StaffArr authority, amount, category, site, supplier, budget, and exception policy.
3. Check conflicts, split-request avoidance, blocked suppliers/items, and contract/source policy.
4. Present total impact and alternatives.
5. Approver approves, rejects, requests change, delegates, or expires with reason.
6. Apply all required approvals and lock the approved scope/version.
7. Route to sourcing or PO creation.
8. Notify requester and track overdue/escalation.

### Exception and recovery paths

- Approver unavailable, self-approval conflict, amount changes after approval, budget check stale, split transaction, supplier becomes restricted, or emergency exception.
- Parallel approvals may disagree.

### Cross-product and external handoffs

- StaffArr → SupplyArr: approval authority.
- LedgArr → SupplyArr: budget context.
- SupplyArr → requester/source product: status.

### Evidence and audit record

- Request/version.
- Routing rationale.
- Decisions/comments/delegation.
- Post-approval changes.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Approval cycle time.
- Overdue rate.
- Rework rate.
- Policy exceptions.
- Self-approval prevention.

## SU-WF-004 — RFQ creation, supplier invitation, clarification, and quote intake

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Collect comparable supplier offers with controlled communication and deadlines. |
| Trigger | An approved request requires competitive sourcing or buyer creates an RFQ. |

### Actors

- Buyer
- Supplier contacts
- Technical reviewer
- SupplyArr

### State path

`draft → issued → clarification → open → closed → evaluation → canceled`

### Required sequence

1. Create RFQ from approved demand with lines/specs, quantities, delivery, terms, response format, and evaluation criteria.
2. Select eligible suppliers and explain exclusions/restrictions.
3. Issue scoped invitations with deadline and document access.
4. Manage supplier questions and publish controlled clarifications/addenda to affected bidders.
5. Suppliers submit quotes, alternates, lead times, terms, validity, and documents.
6. Validate completeness, units/currency, and late/duplicate submissions.
7. Close bidding and freeze submissions for evaluation.
8. Notify nonresponses and preserve audit.

### Exception and recovery paths

- Wrong supplier contact, invitation forwarded, spec changes, supplier asks confidential question, late quote, currency/UOM mismatch, or no bids.
- Sealed-bid policy limits buyer visibility until close.

### Cross-product and external handoffs

- SupplyArr ↔ supplier portal/email.
- SupplyArr ↔ RecordArr: specs/quotes/docs.
- SupplyArr → NexArr: scoped access.

### Evidence and audit record

- RFQ/version/addenda.
- Invitations/access.
- Questions/answers.
- Submitted quote versions/timestamps.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Supplier response.
- Clarification volume.
- On-time quote rate.
- Buyer cycle time.
- No-bid reasons.

## SU-WF-005 — Quote normalization, comparison, negotiation, and award

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Make an explainable sourcing decision using total cost, service, quality, and risk. |
| Trigger | RFQ closes with one or more valid quotes. |

### Actors

- Buyer
- Technical reviewer
- Quality/risk reviewer
- Approver
- SupplyArr

### State path

`normalizing → technical_review → commercial_review → negotiation → recommended → approved → awarded → closed`

### Required sequence

1. Normalize items, UOM, packaging, quantity breaks, currency, freight, duty/tax assumptions, payment terms, and lead times.
2. Validate technical compliance and proposed alternates/substitutions.
3. Retrieve supplier performance, restrictions, risk, contract, and historical price/lead-time context.
4. Calculate comparison and scenario options including split award.
5. Record clarification/negotiation rounds as new quote versions.
6. Select recommended award with reasons and residual risk.
7. Obtain approval when thresholds/policy require.
8. Notify suppliers and convert award to PO/contract/source records.

### Exception and recovery paths

- Quotes not comparable, hidden freight/minimum, alternate not approved, supplier risk changes, capacity conflict, tie, or buyer overrides recommendation.
- Partial award leaves unresolved demand.

### Cross-product and external handoffs

- SupplyArr ↔ AssurArr/Compliance Core: quality/risk.
- SupplyArr ↔ LedgArr: budget/currency.
- SupplyArr → RecordArr: award package.

### Evidence and audit record

- Normalization assumptions.
- Evaluation scores/evidence.
- Negotiation versions.
- Award recommendation/approval.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Savings/avoidance.
- Evaluation cycle.
- Award concentration.
- Override rate.
- Quote-to-actual variance.

## SU-WF-006 — Purchase order create, approve, issue, and revise

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Create a controlled commercial commitment and preserve revision history. |
| Trigger | An approved request/award/contract release is converted to a PO. |

### Actors

- Buyer
- Approver
- Supplier contact
- SupplyArr

### State path

`draft → approval → issued → acknowledged → change_proposed → partially_fulfilled → closed → canceled`

### Required sequence

1. Create PO header/lines from approved source with supplier, ship-to, dates, price, terms, taxes/freight assumptions, and references.
2. Validate supplier status, contract/source, totals, budget/authority, item/UOM, and required documents.
3. Route final approval if commitment differs from approved request/award.
4. Issue immutable revision through portal/API/email/EDI and record delivery.
5. Supplier acknowledges, rejects, proposes changes, or confirms dates/quantities.
6. Buyer accepts change through a new PO revision and impact review.
7. Publish expected receipt/shipment readiness to LoadArr/RoutArr.
8. Close/cancel lines only with received/invoiced/return context and reason.

### Exception and recovery paths

- Supplier becomes blocked, price/quantity change, duplicate PO, transmission failure, unacknowledged order, canceled demand, or partial closure.
- Emergency verbal order requires after-the-fact controlled documentation.

### Cross-product and external handoffs

- SupplyArr → supplier portal/integration.
- SupplyArr → LoadArr: expected receipt.
- SupplyArr → RoutArr: shipment intent.
- SupplyArr → LedgArr: commitment/bill context.

### Evidence and audit record

- Source/approval.
- PO revisions/hashes.
- Delivery/acknowledgement.
- Change decisions.
- Close/cancel reasons.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Issue time.
- Acknowledgement time.
- PO change rate.
- Transmission failure.
- Close accuracy.

## SU-WF-007 — Vendor readiness, shipment intent, and inbound transport handoff

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Durable |
| Purpose | Collect supplier completion/shipping status and coordinate transport/receiving. |
| Trigger | An issued PO requires vendor readiness confirmation or supplier submits shipment details. |

### Actors

- Supplier contact
- Buyer
- Transportation planner
- Receiving coordinator

### State path

`requested → submitted → review → accepted → transport_planned → shipped → received → exception`

### Required sequence

1. Send a scoped readiness request with required response/evidence and due date.
2. Supplier reports ready, complete, partial, delayed, cannot complete, or exception plus quantities/ready date.
3. Buyer reviews material changes/partials and updates PO expectations.
4. Supplier provides packing units, dimensions/weight, pickup location/window, documents, and carrier if known.
5. Create/update RoutArr shipment intent/transportation demand when tenant controls transport.
6. Create/update LoadArr ASN/expected receipt.
7. Track pickup, ETA, receipt, and discrepancies through source-product events.
8. Measure supplier readiness accuracy and delay cause.

### Exception and recovery paths

- No response, partial quantity, quality concern, missing export/SDS docs, pickup address mismatch, buyer rejects change, or freight is supplier-controlled.
- Readiness revoked after transport is dispatched.

### Cross-product and external handoffs

- SupplyArr ↔ vendor portal.
- SupplyArr → RoutArr: shipment intent.
- SupplyArr → LoadArr: ASN/expectation.
- RoutArr/LoadArr → SupplyArr: status.

### Evidence and audit record

- Request/access.
- Supplier response/evidence.
- Buyer decision.
- Shipment/receipt refs and variance.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Response time.
- Ready-date accuracy.
- ASN completeness.
- Inbound delay.
- Partial fulfillment.

## SU-WF-008 — Receipt outcome, three-way context, and procurement exception

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Reconcile PO expectation with LoadArr physical receipt and route commercial exceptions. |
| Trigger | LoadArr posts partial/final receipt or discrepancy. |

### Actors

- Buyer
- Receiver
- Supplier contact
- SupplyArr
- LoadArr

### State path

`received → matched → exception → supplier_review → decision → reconciled → closed`

### Required sequence

1. Receive receipt line quantities, condition/status, lot/serial evidence refs, and discrepancy categories from LoadArr.
2. Match to PO revision/line/UOM and update received/remaining commercial status.
3. Create exception for over, short, wrong, damaged, unapproved substitute, missing document, or late delivery.
4. Coordinate AssurArr quality hold and supplier response where needed.
5. Decide accept, reject, return, replace, credit, price adjustment, or PO change.
6. Publish decision to LoadArr for physical disposition.
7. Create bill/match context for LedgArr while preserving receipt ownership.
8. Close line when accepted quantities and exceptions reconcile.

### Exception and recovery paths

- UOM conversion unknown, receipt references old PO revision, duplicate event, mixed disposition, supplier disputes, invoice already paid, or quality decision pending.
- Tolerance permits auto-accept with warning.

### Cross-product and external handoffs

- LoadArr → SupplyArr: receipt/discrepancy.
- SupplyArr ↔ AssurArr/supplier.
- SupplyArr → LoadArr: commercial disposition.
- SupplyArr → LedgArr: match context.

### Evidence and audit record

- PO/receipt match.
- Exception/evidence.
- Communications/decision.
- Financial/physical reconciliation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Receipt match rate.
- Exception cycle time.
- Supplier-caused variance.
- Credit/replacement recovery.

## SU-WF-009 — Backorder and supplier delay management

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Durable |
| Purpose | Manage missed supply commitments and protect affected operations. |
| Trigger | Supplier reports delay/partial, due date passes, or receipt is short. |

### Actors

- Buyer
- Supplier contact
- Source-product requester
- SupplyArr

### State path

`detected → supplier_update → impact_review → recovery_planned → in_progress → resolved → canceled`

### Required sequence

1. Identify affected PO lines, source demands, reservations, work/orders, and promised dates.
2. Request supplier cause, confirmed quantity, new date, and recovery options.
3. Evaluate alternate supplier, substitute, transfer, expedite, partial allocation, or cancel options.
4. Show total impact and approval/cost/risk tradeoffs.
5. Coordinate decisions with source product and LoadArr/RoutArr.
6. Revise PO or create new sourcing/order action.
7. Notify affected users/customers through owning products.
8. Close when fulfilled/canceled and record supplier performance.

### Exception and recovery paths

- No alternate, critical outage, substitute not technically approved, expedite cost exceeds authority, customer decision needed, or supplier repeatedly slips.
- Multiple demands compete for partial quantity.

### Cross-product and external handoffs

- SupplyArr ↔ source products/LoadArr/RoutArr.
- SupplyArr ↔ supplier portal.
- AssurArr/Compliance Core for substitution constraints.

### Evidence and audit record

- Original commitment.
- Delay updates/causes.
- Impact/options/decision.
- Revised orders and fulfillment outcome.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Backorder age.
- Commit-date accuracy.
- Recovery time.
- Expedite cost.
- Affected-demand service.

## SU-WF-010 — Supplier restriction, suspension, exception, and reinstatement

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Apply scoped commercial blocks and make the reason/remediation visible. |
| Trigger | Risk, quality, compliance, finance, incident, or administrative review creates a restriction. |

### Actors

- Supplier owner
- Risk/quality reviewer
- Buyer
- Approver
- SupplyArr

### State path

`draft → active → exception_open → remediation → review → reinstated → blocked`

### Required sequence

1. Create restriction with supplier, scope, categories/sites/amounts/actions, effective time, reason category, evidence, and owner.
2. Assess open RFQs, quotes, POs, receipts, contracts, demands, and source dependencies.
3. Block or warn new procurement actions according to scope.
4. Notify accountable owners and supplier when appropriate.
5. Track remediation, corrective actions, documents, and review date.
6. Allow a time-limited exception only with authority, reason, and affected transaction.
7. Approve reinstate, narrow, extend, or permanently block.
8. Publish status and retain all decisions.

### Exception and recovery paths

- Emergency sole source, legal/confidential investigation, conflicting restrictions, supplier merges/legal-name change, or active goods in transit.
- Quality hold may apply to specific lots rather than entire supplier.

### Cross-product and external handoffs

- AssurArr/Compliance Core/StaffArr/LedgArr → SupplyArr: signals/context.
- SupplyArr → procurement/source products: restriction.
- RecordArr: evidence.

### Evidence and audit record

- Restriction scope/version.
- Impact analysis.
- Exceptions/approvals.
- Remediation/review/reinstatement.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Restriction propagation.
- Blocked transaction prevention.
- Exception rate.
- Remediation time.
- Repeat restriction.

## SU-WF-011 — Supplier performance review and improvement plan

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Combine objective operational facts and relationship context into a fair supplier review. |
| Trigger | A scheduled review period ends or performance crosses a threshold. |

### Actors

- Supplier owner
- Buyer
- Quality/operations stakeholders
- Supplier contact

### State path

`collecting → validation → internal_review → supplier_review → plan_open → closed`

### Required sequence

1. Collect on-time delivery, lead-time accuracy, receipt variance, quality issues, responsiveness, price/contract, documentation, claim/return, and risk facts.
2. Validate metric provenance and exclude unresolved/data-quality issues.
3. Apply category/criticality-specific weights and targets.
4. Review trends, root causes, context, and business impact.
5. Hold internal calibration and supplier review meeting.
6. Record rating, strengths, concerns, corrective/development actions, owners, and due dates.
7. Adjust preferred/conditional/restriction status only through approval.
8. Track plan outcomes and next review.

### Exception and recovery paths

- Sparse volume, disputed metric, source-system outage, one severe event skews score, supplier refuses review, or relationship owner conflict.
- Supplier-facing score excludes confidential internal data.

### Cross-product and external handoffs

- LoadArr/RoutArr/AssurArr/LedgArr → SupplyArr: performance facts.
- SupplyArr ↔ RecordArr: review package.
- SupplyArr → ReportArr: governed scorecard.

### Evidence and audit record

- Metric definitions/sources.
- Score and adjustments.
- Meeting notes/acknowledgement.
- Action plan/status.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- On-time delivery.
- Defect/return rate.
- Action closure.
- Score improvement.
- Data dispute rate.

## SU-WF-012 — Supplier corrective action request handoff

| Field | Definition |
| --- | --- |
| Classification | COMMON · UNDERSERVED |
| Implementation state | Partial |
| Purpose | Coordinate commercial supplier relationship while AssurArr owns the quality corrective-action process. |
| Trigger | A significant or recurring supplier issue requires formal response. |

### Actors

- Supplier quality reviewer
- Buyer/supplier owner
- Supplier contact
- AssurArr
- SupplyArr

### State path

`opened → containment → response_due → review → implementation → verification → closed → reopened`

### Required sequence

1. Create/link AssurArr supplier quality issue and SCAR with affected receipts/items/lots/POs.
2. SupplyArr provides supplier contacts, contracts, performance, open orders, and restriction context.
3. Send scoped supplier response request with containment, root cause, corrective action, evidence, and deadlines.
4. Supplier submits response; AssurArr evaluates technical adequacy/effectiveness.
5. SupplyArr tracks commercial risk, communication, sourcing alternatives, and restrictions.
6. AssurArr closes/reopens SCAR based on verification.
7. SupplyArr updates performance/status and open procurement decisions.
8. Record recurrence and final package.

### Exception and recovery paths

- Supplier nonresponsive, confidential sub-tier data, disputed responsibility, open claim, containment insufficient, or corrective action fails.
- Commercial exception permits limited buys during remediation.

### Cross-product and external handoffs

- SupplyArr ↔ AssurArr: supplier/context/status.
- SupplyArr ↔ supplier portal.
- RecordArr: evidence.
- LoadArr/OrdArr/MaintainArr: affected records.

### Evidence and audit record

- Issue/affected scope.
- Supplier communications/response.
- AssurArr decisions.
- Restrictions/commercial actions.
- Closure/performance.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Supplier response time.
- Containment time.
- Action closure/effectiveness.
- Recurrence.
- Commercial impact.

## SU-WF-013 — Contract create/reference, obligation, renewal, and expiry

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Manage supplier commercial commitments and prevent missed renewals or off-contract buying. |
| Trigger | A sourcing award, negotiated agreement, or imported contract is approved. |

### Actors

- Contract owner
- Buyer
- Legal reviewer
- Supplier contact
- SupplyArr

### State path

`draft → review → signature → active → renewal → expired → terminated → superseded`

### Required sequence

1. Create contract record with parties, scope, categories/items/sites, dates, value/limits, owner, notice/renewal, and RecordArr document refs.
2. Capture pricing schedules, service levels, insurance/certification, reporting, rebates, termination, and obligations as structured terms where useful.
3. Route review/approval and electronic signature through integrated provider if configured.
4. Activate and link eligible sources/POs.
5. Monitor consumption, pricing, obligations, documents, and notice dates.
6. Initiate renew/renegotiate/terminate with impact and sourcing options.
7. Issue amendments as versioned changes.
8. Close/expire and prevent unintended use while preserving history.

### Exception and recovery paths

- Missing signed document, conflicting price schedule, auto-renewal notice missed, supplier restricted, amendment not reflected in PO, or evergreen agreement.
- Legal interpretation remains with authorized counsel, not AI extraction.

### Cross-product and external handoffs

- SupplyArr ↔ RecordArr/e-sign provider.
- SupplyArr ↔ LedgArr/LoadArr/source products: contract refs.
- ReportArr: coverage/obligation metrics.

### Evidence and audit record

- Contract versions/signatures.
- Structured terms/source citations.
- Obligation/notice events.
- Amendments/closure.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Contract coverage.
- Renewal lead time.
- Off-contract spend.
- Obligation completion.
- Price compliance.

## SU-WF-014 — Vendor return, replacement, credit, and warranty claim

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Recover value and resolve defective, wrong, excess, or warrantable supplied goods. |
| Trigger | A receiving/usage/maintenance/quality issue is approved for supplier return or warranty. |

### Actors

- Buyer/returns coordinator
- Supplier contact
- LoadArr
- MaintainArr
- AssurArr
- SupplyArr

### State path

`draft → authorization → authorized → shipped → supplier_review → replacement → credit → denied → closed`

### Required sequence

1. Create return/claim from PO/receipt/item/lot/serial/work/defect with reason, quantity, condition, value, and evidence.
2. Validate warranty/return terms, deadlines, supplier authorization, and required tests/documents.
3. Request RMA/claim response through portal/integration.
4. Coordinate LoadArr hold, pick/pack/ship, and custody; RoutArr if transport is arranged.
5. Supplier receives and approves replacement, repair, credit, denial, or additional evidence.
6. Track replacement receipt or credit memo/financial context.
7. Update warranty recovery, supplier performance, and affected demand/work.
8. Close when physical and financial outcomes reconcile.

### Exception and recovery paths

- No proof of purchase, deadline expired, supplier disputes misuse, serial mismatch, hazardous return, lost shipment, partial credit, or replacement fails.
- Core return/deposit must be tracked separately.

### Cross-product and external handoffs

- SupplyArr ↔ LoadArr/RoutArr/MaintainArr/AssurArr/RecordArr/LedgArr.
- SupplyArr ↔ supplier portal.

### Evidence and audit record

- Eligibility/terms.
- Authorization/evidence.
- Shipment/custody.
- Supplier decision.
- Replacement/credit reconciliation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Recovery value.
- Authorization time.
- Denial rate.
- Cycle time.
- Repeat failure.

## SU-WF-015 — Scheduled price, lead-time, and availability snapshot

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Maintain source intelligence with provenance and detect meaningful changes. |
| Trigger | A scheduled capture run or manual refresh occurs. |

### Actors

- Buyer/category manager
- Integration worker
- SupplyArr

### State path

`scheduled → running → captured → review → published → partial → failed`

### Required sequence

1. Select active part-vendor sources and provider limits.
2. Retrieve or ingest price, currency, quantity break, fees, availability, lead time, MOQ, and timestamp.
3. Normalize units/currency while preserving raw source and confidence.
4. Detect stale/invalid/outlier/duplicate responses and route review.
5. Store immutable snapshot and update current source view only when valid.
6. Alert on configured material changes and affected open demand/POs.
7. Feed sourcing comparisons and forecasts.
8. Report provider health and coverage.

### Exception and recovery paths

- Provider unavailable/rate-limited, page/API changed, currency/UOM unknown, suspicious price, supplier-specific login expired, or no response.
- Snapshot is indicative and not a binding quote.

### Cross-product and external handoffs

- SupplyArr ↔ supplier/catalog providers.
- NexArr: credential/connection health.
- SupplyArr → ReportArr/source products: signals.

### Evidence and audit record

- Raw response/ref.
- Normalization/version.
- Validation/outlier decision.
- Published snapshot/alerts.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Coverage.
- Freshness.
- Provider success.
- Outlier rate.
- Price/lead-time change.

## SU-WF-016 — Procurement audit package and supplier history

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Assemble a traceable supplier, sourcing, purchase, or exception record. |
| Trigger | An audit, dispute, review, or management request selects a scope. |

### Actors

- Buyer
- Auditor
- Supplier owner
- SupplyArr
- RecordArr

### State path

`requested → collecting → gap_review → assembling → complete → shared → supplemented`

### Required sequence

1. Define scope by supplier, item, RFQ, PO, contract, demand, period, restriction, return, or exception.
2. Snapshot source demand, approvals, supplier status, sourcing/quotes, award, PO revisions, acknowledgements, receipts, exceptions, returns, and performance.
3. Resolve RecordArr documents, StaffArr authority, LoadArr receipt, AssurArr quality, RoutArr shipment, and LedgArr financial refs.
4. Validate missing evidence, orphan links, and retention/legal hold.
5. Create manifest and package request with redaction/external-sharing policy.
6. Review gaps and document accepted exceptions.
7. Finalize, share securely, and log access.
8. Support supplemental response without rewriting original snapshot.

### Exception and recovery paths

- Missing supplier response, orphan source demand, confidential competing quote, legal privilege, package size, or source product unavailable.
- External supplier receives only its own scoped records.

### Cross-product and external handoffs

- SupplyArr ↔ RecordArr and all referenced products.
- SupplyArr → ReportArr: audit readiness.

### Evidence and audit record

- Scope/snapshot.
- Manifest/source versions.
- Gaps/exceptions/redactions.
- Package/access/supplements.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Package time.
- Missing evidence.
- Approval trace coverage.
- External dispute resolution.



## Workflow definition of done

A workflow is releasable only when its happy path, every listed exception, dependency outage, duplicate/retry, stale/concurrent update, permission denial, cancellation, correction/void, archival/retention, import/API/event path, notification, audit trail, reporting projection, mobile behavior, and professional printable output have automated and human-usable acceptance coverage.
