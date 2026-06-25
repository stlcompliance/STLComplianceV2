# OrdArr — OMS Workflow Catalog

[Feature catalog](./FEATURESET.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md)

## Workflow contract

This document defines the end-to-end business state machines for OrdArr. A workflow is not complete because a page or endpoint exists: the trigger, authority, state transitions, validation, handoffs, evidence, exceptions, retries, conflict behavior, notifications, reporting, and final source-of-truth commit must all work together.

## Ownership rule

OrdArr owns the lifecycle of customer or internal orders/requests and coordinates execution handoffs. It captures demand, validates readiness, manages order status and holds, decomposes an order into product-owned execution requests, tracks handoff acceptance/block/completion, manages changes/cancellation/returns at the order level, and produces completion and finance-ready packet references. It does not execute warehouse, transport, maintenance, quality, procurement, customer, or finance work.

- Customer master, contacts, requirements, or agreements; CustomArr owns them.
- Inventory availability/balance, allocation, pick, ship; LoadArr owns warehouse execution.
- Transportation planning/trip/proof; RoutArr owns it.
- Service/maintenance work and asset readiness; MaintainArr owns them.
- Supplier/procurement records; SupplyArr owns them.
- Quality hold/release; AssurArr owns quality decisions.
- Invoices, bills, payments, tax, or GL; LedgArr owns financial execution.
- Files/evidence packages; RecordArr owns them.

## Workflow index

| Workflow ID | Workflow | Class | State | Trigger |
| --- | --- | --- | --- | --- |
| OR-WF-001 | Create, validate, and submit order | CURRENT · COMMON | Scaffold | User, portal, API, EDI, import, agreement, or integration submits an order. |
| OR-WF-002 | Order triage and execution decomposition | CURRENT · COMMON | Scaffold | An order reaches accepted/triage status. |
| OR-WF-003 | Order promise and customer confirmation | COMMON · UNDERSERVED | Target | A new/changed order needs commitment to a customer or requester. |
| OR-WF-004 | Warehouse fulfillment handoff | CURRENT · COMMON | Scaffold | Order decomposition includes warehouse-managed goods. |
| OR-WF-005 | Transportation handoff and delivery | CURRENT · COMMON | Scaffold | Order requires pickup, transfer, delivery, or service transport. |
| OR-WF-006 | Service or maintenance execution handoff | CURRENT · UNDERSERVED | Scaffold | Order includes installation, inspection, repair, maintenance, or asset service. |
| OR-WF-007 | Procurement or dropship handoff | CURRENT · COMMON | Scaffold | Order cannot be fulfilled from available inventory or is configured for dropship. |
| OR-WF-008 | Order hold, review, release, and override | CURRENT · COMMON | Scaffold | Validation, user, product event, risk, quality, compliance, customer, or finance context creates a hold. |
| OR-WF-009 | Order change with downstream impact and compensation | COMMON · UNDERSERVED | Target | Customer/internal user requests quantity, date, location, contact, item/service, requirement, or other change. |
| OR-WF-010 | Order cancellation | CURRENT · COMMON | Scaffold | Authorized customer/internal actor requests cancellation or policy triggers it. |
| OR-WF-011 | Partial fulfillment, substitution, or backorder decision | COMMON · UNDERSERVED | Target | LoadArr/SupplyArr/MaintainArr reports shortage, delay, or unavailable component/service. |
| OR-WF-012 | Return/RMA, exchange, and reverse fulfillment | CURRENT · COMMON | Scaffold | Customer/internal user requests a return, exchange, repair, or service correction. |
| OR-WF-013 | Order completion, evidence, and finance-ready packet | CURRENT · COMMON | Scaffold | All required handoffs report complete/canceled or coordinator initiates completion review. |
| OR-WF-014 | Order exception control tower and recovery | UNDERSERVED · DEMOCRATIZE | Target | Any handoff, promise, hold, customer, quality, supply, transport, service, or finance signal crosses a rule. |
| OR-WF-015 | Durable order event outbox and reconciliation | FOUNDATION | Target | An order transaction commits or reconciliation worker runs. |

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

## OR-WF-001 — Create, validate, and submit order

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Capture a valid order from any authorized channel and make it ready for orchestration. |
| Trigger | User, portal, API, EDI, import, agreement, or integration submits an order. |

### Actors

- Order entry user
- Customer portal/API client
- OrdArr

### State path

`draft → validation → submitted → blocked → accepted → rejected`

### Required sequence

1. Accept an idempotency key and source/channel metadata.
2. Resolve customer, authorized contact, addresses, agreement/requirement refs, and external identifiers from CustomArr.
3. Capture type, lines, quantities/UOM, requested dates, priority, service/transport/warehouse needs, notes, and evidence refs.
4. Validate required fields, duplicates, permissions, customer eligibility, item/service refs, and obvious conflicts.
5. Ask Compliance Core for missing facts/gates when configured.
6. Save draft or submit an immutable order version.
7. Create readiness checks and orchestration plan proposal.
8. Acknowledge source with order ID/status and validation results.

### Exception and recovery paths

- Duplicate external order, customer/contact unauthorized, invalid UOM/address, requirement conflict, missing agreement, item unavailable, or downstream service unavailable.
- Quick-create customer/item/location reference may be needed.

### Cross-product and external handoffs

- CustomArr → OrdArr: customer/requirements.
- Compliance Core → OrdArr: gates.
- OrdArr → source: acknowledgement.
- RecordArr: attachments.

### Evidence and audit record

- Source payload/hash.
- Validation/duplicates.
- Order version.
- Gate/readiness result.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Submission success.
- Validation rejection.
- Duplicate prevention.
- Time to accepted.

## OR-WF-002 — Order triage and execution decomposition

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Determine which product-owned work is required and create traceable handoffs. |
| Trigger | An order reaches accepted/triage status. |

### Actors

- Order coordinator
- OrdArr

### State path

`triage → plan_review → handoffs_requested → partially_accepted → in_progress → blocked`

### Required sequence

1. Evaluate each line and order requirement for inventory, transport, service/maintenance, procurement, quality, documents, customer approval, and external work.
2. Create a decomposition plan linking quantities/services to proposed handoffs and dependencies.
3. Check obvious readiness and avoid duplicate execution requests.
4. Present plan, blockers, and customer promise implications to the coordinator.
5. Approve or adjust decomposition.
6. Issue idempotent handoff requests to owning products.
7. Track acknowledgements and residual unassigned scope.
8. Move order to in-progress only when required handoffs are accepted or approved pending.

### Exception and recovery paths

- No owner for requested work, conflicting handoff quantities, product unavailable, duplicate source request, missing requirement, or one handoff depends on another.
- One order line splits across multiple warehouses/suppliers/services.

### Cross-product and external handoffs

- OrdArr → LoadArr/RoutArr/MaintainArr/SupplyArr/AssurArr/RecordArr.
- Products → OrdArr: accepted/rejected/blocked.

### Evidence and audit record

- Decomposition/version.
- Handoff payloads/idempotency.
- Acknowledgements/blockers.
- Residual scope.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Triage time.
- Handoff acceptance.
- Manual decomposition rate.
- Unowned scope.

## OR-WF-003 — Order promise and customer confirmation

| Field | Definition |
| --- | --- |
| Classification | COMMON · UNDERSERVED |
| Implementation state | Target |
| Purpose | Offer a credible date/service promise based on cross-product capability. |
| Trigger | A new/changed order needs commitment to a customer or requester. |

### Actors

- Order coordinator
- Customer/contact
- OrdArr

### State path

`calculating → options → approval → offered → committed → at_risk → renegotiation`

### Required sequence

1. Gather inventory availability/lead time, procurement options, service capacity, asset/people readiness, transportation time, calendars, quality/compliance holds, and customer requirements.
2. Generate one or more promise scenarios with dates, partial/split options, cost/service impact, confidence, and assumptions.
3. Explain the constraint driving each date.
4. Apply customer-specific promise and approval policy.
5. Present/confirm selected option internally or through portal.
6. Record committed window and source snapshot.
7. Monitor underlying facts and create at-risk event when confidence changes.
8. Renegotiate with customer through controlled change rather than silently moving dates.

### Exception and recovery paths

- No feasible date, stale source, customer requires all-or-nothing, supplier unconfirmed, capacity unavailable, or compliance fact unresolved.
- Expedite option requires approval/customer charge.

### Cross-product and external handoffs

- OrdArr ↔ LoadArr/SupplyArr/RoutArr/MaintainArr/StaffArr/TrainArr/Compliance Core.
- OrdArr ↔ CustomArr portal/customer communication.

### Evidence and audit record

- Inputs/source freshness.
- Scenarios/explanations.
- Customer/approver selection.
- Commitment and risk changes.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Promise accuracy.
- Time to promise.
- At-risk lead time.
- Renegotiation rate.
- Customer acceptance.

## OR-WF-004 — Warehouse fulfillment handoff

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Request inventory allocation and fulfillment while keeping physical execution in LoadArr. |
| Trigger | Order decomposition includes warehouse-managed goods. |

### Actors

- Order coordinator
- Warehouse planner
- OrdArr
- LoadArr

### State path

`requested → accepted → partial → allocated → picking → shipped → blocked → complete`

### Required sequence

1. Send fulfillment request with order/line quantities, need-by, customer/ship-to, allocation/substitution/partial policy, and requirements.
2. LoadArr validates item/location/availability and responds accepted, partial, backordered, or blocked with explanations.
3. OrdArr records allocation/fulfillment handoff without copying stock balances.
4. Coordinate customer decision for substitution/split/backorder when required.
5. Track pick/pack/stage/ship milestones and exceptions.
6. Receive shipment quantities, proof refs, and remaining/backorder status.
7. Update order line fulfillment and downstream transport/financial readiness.
8. Close handoff when quantities and exceptions reconcile.

### Exception and recovery paths

- No stock, quality hold, wrong UOM/lot, warehouse closed, short pick, shipment damaged, or order changes after allocation.
- Dropship bypasses tenant warehouse.

### Cross-product and external handoffs

- OrdArr ↔ LoadArr.
- OrdArr ↔ CustomArr: customer decisions.
- LoadArr ↔ RoutArr: shipment movement.

### Evidence and audit record

- Request/version.
- LoadArr responses.
- Customer decisions.
- Milestones/quantities/proof.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Fill rate.
- Handoff acceptance.
- Backorder aging.
- Order-to-ship.
- Quantity reconciliation.

## OR-WF-005 — Transportation handoff and delivery

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Request transport and track delivery without owning trip execution. |
| Trigger | Order requires pickup, transfer, delivery, or service transport. |

### Actors

- Order coordinator
- Transportation planner
- OrdArr
- RoutArr

### State path

`requested → planning → scheduled → dispatched → in_transit → delivered → exception → complete`

### Required sequence

1. Create transportation handoff with order/line/load refs, origins/destinations, windows, contacts, service level, handling, and proof requirements.
2. RoutArr creates/links transportation demand and returns readiness/blockers.
3. OrdArr tracks plan/tender/dispatch milestones as summarized events.
4. LoadArr/MaintainArr/SupplyArr readiness may gate dispatch.
5. RoutArr executes stops and exceptions.
6. OrdArr evaluates customer promise impact and coordinates customer choices through CustomArr.
7. Receive delivered/partial/refused/damaged/proof outcome.
8. Close or create return/claim/quality follow-up.

### Exception and recovery paths

- No feasible route/carrier, shipment not ready, customer unavailable, damage/refusal, missing proof, address change, or trip canceled.
- Customer pickup has different handoff.

### Cross-product and external handoffs

- OrdArr ↔ RoutArr.
- OrdArr ↔ LoadArr/SupplyArr/MaintainArr/CustomArr/AssurArr.

### Evidence and audit record

- Demand/handoff.
- Milestone summaries.
- Promise/customer actions.
- Delivery/proof/exception.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- On-time delivery.
- Dispatch readiness.
- Exception rate.
- Proof completion.
- Promise impact.

## OR-WF-006 — Service or maintenance execution handoff

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Scaffold |
| Purpose | Coordinate ordered service/work while MaintainArr owns execution and asset readiness. |
| Trigger | Order includes installation, inspection, repair, maintenance, or asset service. |

### Actors

- Order coordinator
- Maintenance/service planner
- OrdArr
- MaintainArr

### State path

`requested → accepted → scheduled → in_progress → blocked → service_complete → accepted → complete`

### Required sequence

1. Send service handoff with customer/site, asset or asset-creation need, scope, dates, requirements, parts, acceptance, and evidence.
2. MaintainArr validates asset/location, skills, capacity, parts, permits, and readiness; accepts/blocks with reasons.
3. OrdArr records work order reference and customer promise dependencies.
4. Coordinate parts procurement/warehouse and transportation as separate handoffs.
5. Track scheduled/in-progress/waiting/completed/return-to-service milestones.
6. Receive service completion, evidence package, residual recommendations, and acceptance status.
7. Coordinate customer signoff or follow-up.
8. Close handoff and include service facts in completion/finance packet.

### Exception and recovery paths

- Unknown asset, unqualified technician, part shortage, customer site unavailable, failed test, additional work approval, or asset not returned to service.
- Service creates recurring PM/agreement obligations.

### Cross-product and external handoffs

- OrdArr ↔ MaintainArr.
- OrdArr ↔ LoadArr/SupplyArr/RoutArr/CustomArr/RecordArr.

### Evidence and audit record

- Scope/version.
- Work acceptance/blockers.
- Milestones/additional authorization.
- Completion/evidence/customer acceptance.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Schedule adherence.
- First-time completion.
- Parts delay.
- Additional work approval.
- Customer acceptance.

## OR-WF-007 — Procurement or dropship handoff

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Source unavailable or direct-ship items while SupplyArr owns procurement. |
| Trigger | Order cannot be fulfilled from available inventory or is configured for dropship. |

### Actors

- Order coordinator
- Buyer
- OrdArr
- SupplyArr

### State path

`requested → approval → sourcing → ordered → shipped → received_or_delivered → blocked → complete`

### Required sequence

1. Send procurement demand with item/spec, quantity, need-by, ship-to or warehouse, customer restrictions, substitution, and order impact.
2. SupplyArr checks contracts/sources/approvals and returns accepted, sourcing, ordered, blocked, or unavailable status.
3. OrdArr records expected dates/confidence and promise impact.
4. For dropship, coordinate supplier readiness, transport, documents, and customer delivery requirements.
5. Track PO/vendor acknowledgement/shipment/receipt or direct delivery milestones.
6. Receive accepted quantities, exceptions, substitutions, and commercial status.
7. Update order fulfillment and customer communication.
8. Close when goods delivered/received or demand canceled.

### Exception and recovery paths

- No approved source, price approval, supplier delay, minimum quantity, substitution conflict, direct ship restriction, or supplier/customer quality issue.
- Partial purchase supports partial order.

### Cross-product and external handoffs

- OrdArr ↔ SupplyArr.
- SupplyArr ↔ RoutArr/LoadArr.
- OrdArr ↔ CustomArr for customer decisions.

### Evidence and audit record

- Demand/source.
- Procurement status/dates.
- Customer decisions.
- Fulfillment outcome.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Need-by attainment.
- Procurement cycle.
- Dropship on-time.
- Substitution rate.
- Customer notification.

## OR-WF-008 — Order hold, review, release, and override

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Stop unsafe or unauthorized progression and make resolution ownership clear. |
| Trigger | Validation, user, product event, risk, quality, compliance, customer, or finance context creates a hold. |

### Actors

- Order coordinator
- Hold owner/reviewer
- Approver
- OrdArr

### State path

`active → review → remediation → release_requested → released → overridden → rejected → expired`

### Required sequence

1. Create hold with type, scope, severity, affected lines/handoffs, owner product/person, reason category, evidence, and release criteria.
2. Immediately prevent only the prohibited transitions/actions.
3. Notify accountable owner and show next action/dependencies.
4. Collect missing information, external decisions, remediation, or approval.
5. Receive owner release/reject event or authorized override request.
6. Validate scope/version and separation of duties.
7. Release/expire/deny/override with reason and residual warning.
8. Re-evaluate readiness and affected handoffs.

### Exception and recovery paths

- Multiple overlapping holds, release arrives out of order, owner unavailable, emergency override, changed order scope, or underlying record remains blocked.
- AssurArr quality holds cannot be self-released by OrdArr.

### Cross-product and external handoffs

- AssurArr/Compliance Core/CustomArr/LoadArr/etc. ↔ OrdArr.
- OrdArr → affected handoffs: block/release.
- RecordArr: evidence.

### Evidence and audit record

- Hold/source/version.
- Affected scope/actions.
- Review/remediation.
- Release/override authority.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Hold aging.
- Time to owner action.
- Unauthorized progression prevented.
- Override rate.
- Repeat holds.

## OR-WF-009 — Order change with downstream impact and compensation

| Field | Definition |
| --- | --- |
| Classification | COMMON · UNDERSERVED |
| Implementation state | Target |
| Purpose | Change an active order safely after execution has begun. |
| Trigger | Customer/internal user requests quantity, date, location, contact, item/service, requirement, or other change. |

### Actors

- Customer/requester
- Order coordinator
- Affected product owners
- OrdArr

### State path

`requested → impact_review → approval → applying → partial → completed → rejected → canceled`

### Required sequence

1. Create change request against a specific order version and capture reason/effective urgency.
2. Validate requester authority and policy/change window.
3. Compute affected promises, allocations, picks/shipments, POs, trips, work orders, documents, quality/compliance, and finance refs.
4. Present feasible options, cost/fee, irreversible actions, and customer impact.
5. Obtain required customer/internal approvals.
6. Issue versioned amendments/cancel/compensating handoffs idempotently.
7. Reconcile accept/reject/partial results from each owner.
8. Publish new order version/promise and preserve the prior execution history.

### Exception and recovery paths

- Shipment already departed, service completed, supplier order noncancelable, address invalid, price/terms approval, partial downstream acceptance, or customer withdraws change.
- Some changes apply only to unfulfilled quantity.

### Cross-product and external handoffs

- OrdArr ↔ all affected products.
- OrdArr ↔ CustomArr: customer communication.
- OrdArr → LedgArr: financial adjustment refs.

### Evidence and audit record

- Requested diff/authority.
- Impact/options.
- Approvals.
- Downstream results/compensation.
- New version/promise.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Change cycle.
- Partial failure.
- Irreversible-change discovery.
- Customer acceptance.
- Rework cost.

## OR-WF-010 — Order cancellation

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Cancel remaining order scope while handling work already committed or completed. |
| Trigger | Authorized customer/internal actor requests cancellation or policy triggers it. |

### Actors

- Customer/requester
- Order coordinator
- Approver
- OrdArr

### State path

`requested → impact_review → approved → canceling → partial → canceled → denied → closed`

### Required sequence

1. Validate authority, cancelable scope, reason, and order version.
2. Identify unstarted, in-progress, shipped/delivered, procured, scheduled, completed, and financial states.
3. Calculate cancellation options, fees/credits refs, return/recovery actions, and customer impact.
4. Obtain approval when commitments exist.
5. Send cancel/stop/return/void requests to owning products.
6. Track accepted/rejected/too-late results and compensating work.
7. Update order/line status and customer communication.
8. Close when remaining obligations and financial/evidence refs reconcile.

### Exception and recovery paths

- Shipment delivered, noncancelable supplier PO, technician already onsite, quality/legal hold, partial quantities, refund dispute, or downstream service unavailable.
- Cancellation may become return for completed/shipped lines.

### Cross-product and external handoffs

- OrdArr ↔ LoadArr/RoutArr/MaintainArr/SupplyArr/LedgArr refs.
- CustomArr: customer communication.
- RecordArr: evidence.

### Evidence and audit record

- Request/authority/reason.
- Impact/fees.
- Downstream cancel results.
- Final status/financial refs.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Cancellation cycle.
- Too-late rate.
- Recovered commitments.
- Customer dispute.
- Partial cancellation accuracy.

## OR-WF-011 — Partial fulfillment, substitution, or backorder decision

| Field | Definition |
| --- | --- |
| Classification | COMMON · UNDERSERVED |
| Implementation state | Target |
| Purpose | Coordinate a clear decision when full requested fulfillment is unavailable. |
| Trigger | LoadArr/SupplyArr/MaintainArr reports shortage, delay, or unavailable component/service. |

### Actors

- Order coordinator
- Customer/contact
- Planner/buyer
- OrdArr

### State path

`detected → options → customer_decision → applying → backordered → partial_complete → resolved`

### Required sequence

1. Identify affected quantity/service, promise, customer requirement, and cause.
2. Build options: wait, partial now/later, split ship, substitute, alternate source/site, expedite, reschedule service, customer pickup, or cancel.
3. Validate technical/quality/compliance/customer eligibility for substitutes/options.
4. Show date, cost/fee, service, transport, and risk impacts.
5. Auto-select only when customer policy explicitly allows; otherwise request customer/internal decision.
6. Issue approved downstream changes.
7. Update promises, handoffs, backorder quantities, and communications.
8. Monitor residual quantity and close decision when complete/canceled.

### Exception and recovery paths

- No customer response, substitute not approved, price difference, all-or-nothing requirement, supply date uncertain, or option becomes stale.
- Emergency operations may approve temporary partial.

### Cross-product and external handoffs

- LoadArr/SupplyArr/MaintainArr → OrdArr.
- OrdArr ↔ CustomArr/portal.
- OrdArr → affected products.

### Evidence and audit record

- Cause/source.
- Options/assumptions.
- Decision/authority.
- Downstream changes/promises.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Decision time.
- Backorder aging.
- Substitution acceptance.
- Promise recovery.
- No-response rate.

## OR-WF-012 — Return/RMA, exchange, and reverse fulfillment

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Authorize and coordinate returned goods/service correction through inspection and financial outcome. |
| Trigger | Customer/internal user requests a return, exchange, repair, or service correction. |

### Actors

- Customer/contact
- Service agent
- Order coordinator
- OrdArr

### State path

`requested → authorized → in_transit → received → inspection → replacement_or_credit → exception → closed`

### Required sequence

1. Validate original order/line, eligibility window, reason, quantity/serial/condition, customer authorization, and required evidence.
2. Create RMA/return order with instructions, labels/transport, expected item, and refund/replacement/repair policy.
3. Coordinate RoutArr/customer drop-off and LoadArr receipt/inspection.
4. AssurArr/MaintainArr decides quality/repair context as needed.
5. Track received quantity, disposition, replacement/service handoff, and customer updates.
6. Create credit/refund/charge/restocking financial refs for LedgArr.
7. Resolve missing/wrong/damaged return and disputes.
8. Close when physical, customer, and financial outcomes reconcile.

### Exception and recovery paths

- Outside policy, wrong item, serial mismatch, hazardous goods, counterfeit/suspect, lost return, customer damage, replacement unavailable, or refund dispute.
- No-return refund is permitted by policy for selected cases.

### Cross-product and external handoffs

- OrdArr ↔ CustomArr/LoadArr/RoutArr/AssurArr/MaintainArr/LedgArr/RecordArr.

### Evidence and audit record

- Eligibility/authorization.
- Return custody/evidence.
- Inspection/disposition.
- Replacement/credit refs.
- Customer communication.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Authorization time.
- Return cycle.
- Disposition aging.
- Refund/replacement reconciliation.
- Fraud/abuse review.

## OR-WF-013 — Order completion, evidence, and finance-ready packet

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Close an order only when required work, evidence, customer status, and financial facts are reconciled. |
| Trigger | All required handoffs report complete/canceled or coordinator initiates completion review. |

### Actors

- Order coordinator
- Approver
- OrdArr
- RecordArr
- LedgArr

### State path

`completion_review → evidence_gap → operational_complete → finance_ready → completed → closed → reopened`

### Required sequence

1. Snapshot latest order version, lines, holds, promises, handoffs, quantities/services, returns, and exceptions.
2. Verify required handoffs are complete/canceled with approved reason and no active blocking holds.
3. Validate customer acceptance/proof and required documents through RecordArr/Compliance Core.
4. Resolve quantity/status mismatches and accepted residual warnings.
5. Create completion record and customer-safe summary.
6. Create invoice-ready/bill-ready/finance packet refs with source facts, not accounting entries.
7. Send packet to LedgArr and reconcile acceptance/validation errors.
8. Mark completed/closed and preserve reopen/correction workflow.

### Exception and recovery paths

- Missing proof, unresolved partial, active quality hold, customer dispute, unposted return, handoff status stale, or finance mapping fails.
- Operationally complete but finance packet pending.

### Cross-product and external handoffs

- OrdArr ↔ all handoff owners.
- OrdArr ↔ RecordArr/Compliance Core.
- OrdArr → LedgArr.
- OrdArr → CustomArr: customer status.

### Evidence and audit record

- Snapshot/handoff reconciliation.
- Evidence/gates.
- Completion decision.
- Finance packet/acknowledgement.
- Reopen/correction.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Order cycle time.
- First-pass completion.
- Evidence gaps.
- Finance rejection.
- Reopen rate.

## OR-WF-014 — Order exception control tower and recovery

| Field | Definition |
| --- | --- |
| Classification | UNDERSERVED · DEMOCRATIZE |
| Implementation state | Target |
| Purpose | Give operations one prioritized queue of order risks and owned recovery actions. |
| Trigger | Any handoff, promise, hold, customer, quality, supply, transport, service, or finance signal crosses a rule. |

### Actors

- Order coordinator
- Operations manager
- Affected product owner
- OrdArr

### State path

`detected → triage → assigned → recovery → monitoring → resolved → closed`

### Required sequence

1. Normalize exception with order/line/handoff, severity, customer impact, promise risk, value, owner, and source confidence.
2. Deduplicate related signals and link root exception.
3. Prioritize using transparent SLA/customer/safety/financial/risk rules.
4. Show next action, response options, dependencies, and who owns each blocker.
5. Assign/escalate tasks and record communication plan.
6. Apply selected recovery through product-owned actions.
7. Monitor resulting promise and customer outcome.
8. Close with cause, avoided/realized impact, and recurrence tags.

### Exception and recovery paths

- Cascading event affects many orders, source conflict, no owner, no feasible recovery, customer decision pending, or automated suggestion is unsafe.
- Mass event requires bulk impact simulation.

### Cross-product and external handoffs

- All products → OrdArr: normalized risk/status.
- OrdArr → product owners/CustomArr: actions/communication.
- ReportArr: control-tower analytics.

### Evidence and audit record

- Source signals/correlation.
- Priority/ownership.
- Options/decision/actions.
- Outcome/cause.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to detect/own.
- Promise recovered.
- Customer notification.
- Exception aging.
- Repeat cause.

## OR-WF-015 — Durable order event outbox and reconciliation

| Field | Definition |
| --- | --- |
| Classification | FOUNDATION |
| Implementation state | Target |
| Purpose | Guarantee order events and handoff commands survive restart, retry safely, and reconcile with downstream truth. |
| Trigger | An order transaction commits or reconciliation worker runs. |

### Actors

- OrdArr service
- Product services
- Integration administrator

### State path

`pending → published → acknowledged → retry → dead_letter → reconciled`

### Required sequence

1. Commit order state/version and outbox event atomically in durable storage.
2. Publish signed/versioned event with tenant, correlation, causation, aggregate version, and idempotency key.
3. Consumer validates and processes idempotently, then acknowledges or records failure.
4. Retry transient errors with backoff; quarantine poison messages.
5. Reconcile handoff state against downstream source APIs/events.
6. Detect gaps, duplicates, version conflicts, and stale projections.
7. Repair through replay or explicit operator action with audit.
8. Report delivery health and unresolved drift.

### Exception and recovery paths

- Database unavailable, duplicate publish, out-of-order event, consumer schema mismatch, tenant mismatch, poison payload, or downstream record deleted/corrected.
- Never “fix” drift by blindly overwriting source truth.

### Cross-product and external handoffs

- OrdArr ↔ all product event endpoints.
- NexArr: service identity/outbox standards.
- ReportArr: health metrics.

### Evidence and audit record

- Atomic transaction/event.
- Delivery attempts/acks.
- Reconciliation comparisons.
- Repair decisions.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Event delivery latency.
- Retry/dead-letter rate.
- State drift.
- Replay success.
- Duplicate suppression.



## Workflow definition of done

A workflow is releasable only when its happy path, every listed exception, dependency outage, duplicate/retry, stale/concurrent update, permission denial, cancellation, correction/void, archival/retention, import/API/event path, notification, audit trail, reporting projection, mobile behavior, and professional printable output have automated and human-usable acceptance coverage.
