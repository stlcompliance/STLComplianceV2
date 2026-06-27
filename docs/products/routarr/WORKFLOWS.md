# RoutArr — TMS Workflow Catalog

[Feature catalog](./FEATURESET.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md) · [Rollout](./ROLLOUT.md)

## Workflow contract

This document defines the end-to-end business state machines for RoutArr. A workflow is not complete because a page or endpoint exists: the trigger, authority, state transitions, validation, handoffs, evidence, exceptions, retries, conflict behavior, notifications, reporting, and final source-of-truth commit must all work together.

## Ownership rule

RoutArr owns transportation demand, planning, routing, carrier/tender decisions, dispatch, routes, trips, stops, movement execution, transportation visibility, proof, transportation exceptions, yard/gate events, claims context, detention/accessorial context, and finance contribution packets. It consumes orders, shipments, inventory readiness, asset readiness, drivers, qualifications, customer/supplier context, and documents from the products that own those truths.

- Order lifecycle and customer promise; OrdArr owns order coordination while CustomArr owns customer truth.
- Warehouse inventory, pick/pack/staging/shipping truth; LoadArr owns warehouse execution.
- Asset master, defects, maintenance, or readiness; MaintainArr owns them.
- People, employment, locations, permissions, or qualifications; StaffArr/TrainArr own them.
- Supplier/vendor commercial truth and purchase orders; SupplyArr owns them.
- Invoices, payments, GL, or final settlement; LedgArr or external finance owns them.
- File binaries; RecordArr owns proof and document files.

## Workflow index

| Workflow ID | Workflow | Class | State | Trigger |
| --- | --- | --- | --- | --- |
| RO-WF-001 | Create transportation demand from an order, PO, shipment, or manual request | CURRENT · COMMON | Durable | OrdArr, SupplyArr, LoadArr, CustomArr, or an authorized user submits transportation demand. |
| RO-WF-002 | Consolidate demand and build a load/plan | COMMON · DEMOCRATIZE | Partial | Planner selects ready demands or runs a planning scenario. |
| RO-WF-003 | Rate shop and routing-guide decision | CURRENT · COMMON | Partial | A demand/load is ready for carrier or mode selection. |
| RO-WF-004 | Carrier tender and acceptance | CURRENT · COMMON | Durable | A carrier-served load has an approved rate/routing-guide decision. |
| RO-WF-005 | Owned-fleet driver and equipment assignment with dispatch release | CURRENT · COMMON | Durable | A dispatch plan is ready for owned-fleet execution. |
| RO-WF-006 | Pickup, stop execution, and in-transit visibility | CURRENT · COMMON | Durable | A released trip begins or the driver approaches a stop. |
| RO-WF-007 | Transportation exception and dynamic replanning | CURRENT · UNDERSERVED | Durable | A driver, integration, dispatcher, customer, facility, or risk rule reports an exception. |
| RO-WF-008 | Dock appointment, gate, yard, and detention | CURRENT · COMMON | Partial | A load requires an appointment or arrives at a managed facility. |
| RO-WF-009 | Proof review and trip completion | CURRENT · COMMON | Durable | A driver/carrier marks the final stop or trip complete. |
| RO-WF-010 | Transportation claim for loss, damage, or delay | CURRENT · COMMON | Partial | Damage, loss, shortage, temperature excursion, or service failure is identified. |
| RO-WF-011 | Vendor order completion to transportation dispatch | CURRENT · UNDERSERVED | Partial | OrdArr/SupplyArr creates a vendor completion request tied to transportation need. |
| RO-WF-012 | Driver DVIR defect and equipment substitution | CURRENT · COMMON | Partial | Driver completes pre/post-trip DVIR or reports an equipment defect. |
| RO-WF-013 | Freight audit, accessorial review, and finance contribution | CURRENT · DEMOCRATIZE | Partial | Trip completion, carrier invoice intake, or accessorial event occurs. |
| RO-WF-014 | Multimodal leg handoff and continuity | CURRENT · DEMOCRATIZE | Partial | A demand is planned as rail/ocean/air/parcel/intermodal or changes mode. |
| RO-WF-015 | Transportation audit/document packet | CURRENT · COMMON | Partial | A trip completes, claim opens, audit occurs, or external party requests documents. |

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

## RO-WF-001 — Create transportation demand from an order, PO, shipment, or manual request

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Capture transport need independently of execution planning. |
| Trigger | OrdArr, SupplyArr, LoadArr, CustomArr, or an authorized user submits transportation demand. |

### Actors

- Transportation planner
- Source product
- RoutArr

### State path

`draft → validation → blocked → ready_for_planning → planned → canceled`

### Required sequence

1. Validate source reference, parties/locations, lines, quantities, ready/due windows, service level, and ownership.
2. Normalize pickup/delivery locations and contacts through owning-product refs.
3. Capture handling, capacity, equipment, temperature, hazmat, appointment, and document requirements.
4. Detect duplicate/overlapping demand and consolidation candidates.
5. Evaluate missing facts and compliance requirements.
6. Set planning readiness, blockers, and accountable planner.
7. Acknowledge the source product with demand ID/status.
8. Emit demand-created/ready events.

### Exception and recovery paths

- Unknown location, missing dimensions/weight, conflicting windows, duplicate source, customer/supplier restriction, or compliance fact unknown.
- Demand may require multiple transport legs/modes.

### Cross-product and external handoffs

- Source product → RoutArr: demand.
- RoutArr ↔ Compliance Core/CustomArr/SupplyArr/LoadArr: requirements/context.
- RoutArr → source: status.

### Evidence and audit record

- Source snapshot.
- Normalized requirements.
- Duplicate/consolidation decisions.
- Blockers and readiness.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to planning-ready.
- Missing-data rate.
- Duplicate rate.
- Source acknowledgement latency.

## RO-WF-002 — Consolidate demand and build a load/plan

| Field | Definition |
| --- | --- |
| Classification | COMMON · DEMOCRATIZE |
| Implementation state | Partial |
| Purpose | Combine compatible demand into executable loads and routes with visible tradeoffs. |
| Trigger | Planner selects ready demands or runs a planning scenario. |

### Actors

- Transportation planner
- RoutArr optimization service

### State path

`scenario → suggested → review → accepted → published → superseded`

### Required sequence

1. Select scope, horizon, depots, modes, fleet/carriers, and objective weights.
2. Validate compatibility by windows, capacity, equipment, handling, customer, route, and regulatory constraints.
3. Generate scenarios for direct, consolidated, multi-stop, multi-leg, or split movement.
4. Show cost, miles, service risk, utilization, emissions, and constraint explanations.
5. Planner accepts, edits, or rejects suggestions.
6. Create dispatch plan, route/trip/load/stop structures and preserve source-demand allocations.
7. Revalidate before release.
8. Publish plan and remaining unplanned demand.

### Exception and recovery paths

- No feasible plan, stale capacity, conflicting constraints, customer prohibits consolidation, or a demand changes during planning.
- Partial planning leaves residual demand.

### Cross-product and external handoffs

- RoutArr ↔ StaffArr/TrainArr/MaintainArr: people/equipment readiness.
- RoutArr ↔ rate/carrier providers.
- RoutArr → OrdArr/LoadArr: plan status.

### Evidence and audit record

- Scenario inputs/objectives.
- Constraint results.
- Suggestions and human edits.
- Published allocations.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Planning time.
- Utilization.
- Unplanned demand.
- Planner edit rate.
- Cost/service delta.

## RO-WF-003 — Rate shop and routing-guide decision

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Choose a transport option using contract, spot, service, and risk context. |
| Trigger | A demand/load is ready for carrier or mode selection. |

### Actors

- Transportation planner
- Carrier/rate integrations
- RoutArr

### State path

`requested → rated → guide_evaluated → approval → selected → failed`

### Required sequence

1. Build a normalized rating request with lane, mode, equipment, freight, dates, and accessorial assumptions.
2. Retrieve contract/internal fleet/spot options and record source/freshness.
3. Apply routing-guide steps, carrier eligibility, customer/supplier restrictions, and capacity.
4. Normalize total cost, transit, service, risk, and likely accessorials.
5. Explain excluded options and assumptions.
6. Select automatically within policy or route for approval.
7. Snapshot the chosen rate/guide decision.
8. Prepare tender or owned-fleet assignment.

### Exception and recovery paths

- No rate, stale contract, carrier restricted, capacity unavailable, rate limit/provider outage, ambiguous accessorial, or approval threshold exceeded.
- Manual rate is entered with evidence.

### Cross-product and external handoffs

- RoutArr ↔ rating/carrier providers.
- SupplyArr/CustomArr → RoutArr: restrictions/requirements.
- RoutArr → LedgArr: planned cost contribution.

### Evidence and audit record

- Rating request/options.
- Exclusions and assumptions.
- Routing-guide version.
- Selection/approval.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Rate response time.
- Guide compliance.
- Manual rate rate.
- Planned vs actual cost.

## RO-WF-004 — Carrier tender and acceptance

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Offer a load to eligible carriers and obtain an auditable commitment. |
| Trigger | A carrier-served load has an approved rate/routing-guide decision. |

### Actors

- Transportation planner
- Carrier dispatcher
- RoutArr

### State path

`draft → sent → opened → accepted → declined → countered → expired → withdrawn`

### Required sequence

1. Create tender with scope, rate, equipment, windows, requirements, response deadline, and documents.
2. Send through API/EDI/email/scoped portal according to carrier capability.
3. Track delivered/opened/acknowledged and response clock.
4. Accept, decline, counter, or request clarification.
5. Validate accepting carrier authority/insurance/restrictions and capacity.
6. If declined/expired, advance routing-guide fallback or spot workflow.
7. On acceptance, confirm assignment and publish carrier context.
8. Retain all offers/responses without overwriting.

### Exception and recovery paths

- No response, counter exceeds authority, carrier becomes restricted, duplicate acceptance, changed load, or invitation forwarded to unauthorized user.
- Broadcast tender receives multiple acceptances.

### Cross-product and external handoffs

- RoutArr ↔ carrier integration/portal.
- RoutArr ↔ SupplyArr/NexArr: carrier party/contact/scoped access.
- RoutArr → source products: assigned status.

### Evidence and audit record

- Tender/version and recipients.
- Delivery/open/response.
- Eligibility check.
- Acceptance/withdrawal/fallback.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Acceptance rate.
- Response time.
- Guide depth.
- Counter variance.
- Unauthorized access blocks.

## RO-WF-005 — Owned-fleet driver and equipment assignment with dispatch release

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Release a trip only when people, equipment, load, documents, and requirements are ready. |
| Trigger | A dispatch plan is ready for owned-fleet execution. |

### Actors

- Dispatcher
- Driver
- RoutArr
- StaffArr
- TrainArr
- MaintainArr

### State path

`planned → assigned → blocked → released → acknowledged → departed → canceled`

### Required sequence

1. Select candidate drivers/equipment from availability and capacity snapshots.
2. Check StaffArr assignment/permission, TrainArr qualification, HOS/rest, MaintainArr readiness, DVIR/defects, permits, and service constraints.
3. Explain eligible/warning/blocked results.
4. Assign driver/equipment and create a release snapshot.
5. Confirm load/warehouse readiness, stops/windows, documents, and outstanding exceptions.
6. Driver reviews and acknowledges dispatch.
7. Release trip and notify LoadArr/OrdArr/customer status as appropriate.
8. Revalidate critical readiness changes until departure.

### Exception and recovery paths

- Driver unavailable/unqualified, HOS risk, equipment down/held, load not staged, document missing, route infeasible, or dispatcher override lacks approval.
- Driver/equipment substitution after release.

### Cross-product and external handoffs

- StaffArr/TrainArr/MaintainArr → RoutArr: readiness.
- LoadArr → RoutArr: load state.
- RoutArr → driver/OrdArr/CustomArr: dispatch status.

### Evidence and audit record

- Candidate/readiness snapshot.
- Blocks/warnings/override.
- Release version.
- Driver acknowledgement and substitutions.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Release success.
- Block causes.
- Last-minute substitutions.
- Dispatch delay.

## RO-WF-006 — Pickup, stop execution, and in-transit visibility

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Execute stops and maintain a trustworthy movement timeline. |
| Trigger | A released trip begins or the driver approaches a stop. |

### Actors

- Driver
- Dispatcher
- Facility contact
- RoutArr

### State path

`en_route → arrived → at_gate → at_door → servicing → departed → exception → completed`

### Required sequence

1. Start trip and record actual departure/source.
2. Navigate/check in or geofence arrival; reconcile appointment/window.
3. Capture gate/door/load or unload milestones, counts/seal/temperature as required.
4. Collect signatures/photos/documents and exceptions.
5. Depart stop and calculate next ETA with source/confidence.
6. Ingest telematics/carrier/manual events and suppress duplicates/out-of-order updates.
7. Notify authorized customers/source products with safe status.
8. Continue until all stops and legs are complete.

### Exception and recovery paths

- Offline device, denied entry, freight not ready, shortage/damage/refusal, no contact, route deviation, breakdown, stale telematics, or signature unavailable.
- Stop is skipped/resequenced with dispatcher approval.

### Cross-product and external handoffs

- Field Companion/carrier → RoutArr: stop events/proof.
- RoutArr ↔ LoadArr: dock/load state.
- RoutArr → OrdArr/CustomArr: customer-safe milestones.
- RecordArr: proof files.

### Evidence and audit record

- Milestones/source/confidence.
- Gate/door/service timestamps.
- Proof and exceptions.
- ETA revisions.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- On-time pickup/delivery.
- Stop dwell.
- ETA accuracy.
- Proof completeness.
- Manual event rate.

## RO-WF-007 — Transportation exception and dynamic replanning

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Durable |
| Purpose | Triage disruption, choose a response, and communicate impact across affected records. |
| Trigger | A driver, integration, dispatcher, customer, facility, or risk rule reports an exception. |

### Actors

- Driver
- Dispatcher
- Planner
- Customer service
- RoutArr

### State path

`reported → triage → response_planned → in_progress → monitoring → resolved → closed`

### Required sequence

1. Create exception with trip/stop/load/demand scope, severity, source, time, and evidence.
2. Determine immediate safety/custody action.
3. Calculate affected ETA, appointments, HOS, orders, inventory, customer promises, and cost.
4. Generate options such as resequence, reschedule, swap equipment/driver, split, recovery carrier, return, hold, or cancel.
5. Dispatcher selects/approves response and records reason.
6. Update plans and send targeted notifications/handoffs.
7. Track resolution tasks, proof, cost/accessorial, and claims potential.
8. Close with cause, impact, and recurrence classification.

### Exception and recovery paths

- Source conflict, no feasible response, emergency services required, customer decision pending, asset breakdown, severe weather, or multi-trip cascade.
- Exception becomes quality/incident/claim case.

### Cross-product and external handoffs

- RoutArr → MaintainArr/StaffArr/AssurArr/OrdArr/CustomArr as appropriate.
- RoutArr → RecordArr: evidence.
- RoutArr → ReportArr: impact facts.

### Evidence and audit record

- Original event/source.
- Impact analysis/options.
- Decision/approval.
- Notifications/actions.
- Resolution/cause/cost.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to acknowledge.
- Time to response.
- Late minutes avoided.
- Exception recurrence.
- Customer notification latency.

## RO-WF-008 — Dock appointment, gate, yard, and detention

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Coordinate arrival through departure and produce defensible dwell/accessorial evidence. |
| Trigger | A load requires an appointment or arrives at a managed facility. |

### Actors

- Transportation coordinator
- Warehouse/dock staff
- Driver/carrier
- RoutArr
- LoadArr

### State path

`requested → confirmed → at_risk → checked_in → yard → at_door → servicing → checked_out`

### Required sequence

1. Request/confirm appointment with facility, door/area, load, equipment, and service needs.
2. Track ETA and suggest reschedule when risk crosses threshold.
3. At arrival, capture gate check-in, driver/trailer/seal, appointment, and queue position.
4. Assign yard/door/drop/hook events in coordination with LoadArr.
5. Capture service start/end, load state, delays, cause owner, and evidence.
6. Calculate contractual free time and detention/accessorial proposal.
7. Obtain review/approval or dispute.
8. Record gate-out and publish final dwell timeline.

### Exception and recovery paths

- Early/late arrival, no appointment, facility closed, door unavailable, load not ready, trailer mismatch, rejected freight, or clock disagreement.
- Carrier/facility timestamps differ.

### Cross-product and external handoffs

- RoutArr ↔ LoadArr: appointment/door/load milestones.
- RoutArr ↔ carrier/facility portal.
- RoutArr → LedgArr: approved accessorial contribution.

### Evidence and audit record

- Appointment versions.
- ETA/reschedule.
- Gate/yard/door events.
- Delay cause/evidence.
- Detention calculation/approval.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Appointment adherence.
- Gate-to-door.
- Dwell.
- Detention cost/recovery.
- Cause attribution.

## RO-WF-009 — Proof review and trip completion

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Validate required proof and close transport execution without losing exceptions. |
| Trigger | A driver/carrier marks the final stop or trip complete. |

### Actors

- Driver/carrier
- Dispatcher
- Proof reviewer
- RoutArr

### State path

`driver_complete → proof_review → exception → accepted → completed → corrected`

### Required sequence

1. Collect final stop statuses, quantities, signatures, photos, documents, seal/temperature, and exception outcomes.
2. Validate completeness against customer, mode, compliance, and finance requirements.
3. Route missing/illegible/contradictory proof to review.
4. Resolve or accept warnings with reason/authority.
5. Calculate trip completion rollup and final milestone.
6. Request RecordArr transportation document package.
7. Create finance packet contribution and claims/quality follow-up where needed.
8. Publish completed status to OrdArr/source products and lock execution facts except correction workflow.

### Exception and recovery paths

- Missing signature, damaged goods, quantity variance, document unreadable, unsigned BOL, unresolved stop, or offline proof not synced.
- Customer refuses delivery or proof is later challenged.

### Cross-product and external handoffs

- RoutArr → RecordArr: proof package.
- RoutArr → OrdArr/LedgArr: completion/finance contribution.
- RoutArr → AssurArr/CustomArr: damage/complaint context.

### Evidence and audit record

- Stops and quantities.
- Proof files/signatures.
- Review decisions.
- Completion rollup.
- Finance/claim refs.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Proof first-pass acceptance.
- Time to completion.
- Missing document rate.
- Correction rate.

## RO-WF-010 — Transportation claim for loss, damage, or delay

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Create and manage a claim with linked custody, proof, quality, customer, and financial context. |
| Trigger | Damage, loss, shortage, temperature excursion, or service failure is identified. |

### Actors

- Claims coordinator
- Carrier
- Customer service
- Quality reviewer
- RoutArr

### State path

`draft → notified → investigating → evidence_required → offered → disputed → settled → denied → closed`

### Required sequence

1. Create claim from trip/load/stop/exception with claimant/respondent and affected lines.
2. Preserve custody timeline, proof, photos, inspection, value, notices, and deadlines.
3. Notify carrier/party through scoped portal/integration.
4. Coordinate AssurArr nonconformance and CustomArr customer case as needed.
5. Track acknowledgement, investigation, requested evidence, reserve/expected recovery, offer, dispute, and decision.
6. Record disposition, recovery context, and unresolved customer/order impact.
7. Send approved financial contribution to LedgArr.
8. Close with cause/recurrence and document package.

### Exception and recovery paths

- Late notice, unclear custody, missing value/evidence, multiple carriers, customer settlement before carrier decision, denied claim, or legal escalation.
- Sensitive/legal communications require restricted access.

### Cross-product and external handoffs

- RoutArr ↔ AssurArr/CustomArr/OrdArr/RecordArr/LedgArr.
- RoutArr ↔ carrier portal/integration.

### Evidence and audit record

- Claim notice/deadlines.
- Custody/proof/evidence.
- Communications/offers.
- Decision/recovery.
- Cause/closure.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Claim rate.
- Notice timeliness.
- Recovery percentage.
- Cycle time.
- Recurring carrier/lane causes.

## RO-WF-011 — Vendor order completion to transportation dispatch

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Partial |
| Purpose | Dispatch only after an external supplier confirms readiness and required evidence. |
| Trigger | OrdArr/SupplyArr creates a vendor completion request tied to transportation need. |

### Actors

- Order coordinator
- Supplier/vendor contact
- Transportation planner
- RoutArr

### State path

`requested → sent → submitted → review → accepted → ready_for_dispatch → dispatched → exception`

### Required sequence

1. Receive order/vendor/shipment context and required readiness response/evidence.
2. Issue a scoped, time-limited portal request to the authorized vendor contact.
3. Vendor responds ready, complete, partial, delayed, cannot complete, or exception with dates/quantities/evidence.
4. Owning order/procurement product accepts or requests review.
5. RoutArr creates/updates transportation demand only when readiness policy passes.
6. Plan/tender/assign transport and communicate pickup status.
7. At pickup, reconcile actual readiness/quantity and record discrepancy.
8. Track vendor response and dispatch delay metrics.

### Exception and recovery paths

- Vendor does not respond, forwarded invitation, partial completion, quantity mismatch, quality issue, evidence missing, or driver arrives before ready.
- Readiness is revoked after dispatch.

### Cross-product and external handoffs

- OrdArr/SupplyArr ↔ vendor portal.
- OrdArr/SupplyArr → RoutArr: readiness/demand.
- RoutArr → RecordArr/AssurArr: proof/quality.

### Evidence and audit record

- Request/access log.
- Vendor response/evidence.
- Acceptance decision.
- Demand/dispatch/pickup result.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Vendor response time.
- Ready-at-pickup accuracy.
- Dispatch delay.
- Unauthorized access blocks.

## RO-WF-012 — Driver DVIR defect and equipment substitution

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Capture vehicle inspection defects and protect dispatch readiness. |
| Trigger | Driver completes pre/post-trip DVIR or reports an equipment defect. |

### Actors

- Driver
- Dispatcher
- Maintenance supervisor
- RoutArr
- MaintainArr

### State path

`inspection → defect_reported → maintenance_review → blocked → substituted → cleared`

### Required sequence

1. Load vehicle/trailer and inspection requirements.
2. Capture item results, readings, photos, notes, and driver certification.
3. Classify safety-critical vs non-critical findings using approved rules.
4. Create MaintainArr defect with trip/driver/context and evidence refs.
5. Receive readiness decision and block or warn dispatch accordingly.
6. Find substitute equipment and revalidate capacity/requirements.
7. Update trip assignment, notify stakeholders, and preserve original release history.
8. Track repair/return-to-service outcome.

### Exception and recovery paths

- Unknown asset, offline report, defect severity disputed, no substitute, trailer/load already sealed, or defect discovered after departure.
- Roadside failure requires emergency workflow.

### Cross-product and external handoffs

- RoutArr → MaintainArr: defect/DVIR.
- MaintainArr → RoutArr: readiness.
- RoutArr ↔ StaffArr/TrainArr: driver context.
- RecordArr: evidence.

### Evidence and audit record

- DVIR/template/version.
- Defect/evidence.
- Readiness result.
- Substitution and acknowledgements.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- DVIR completion.
- Critical defect capture.
- Dispatch blocks honored.
- Substitution time.

## RO-WF-013 — Freight audit, accessorial review, and finance contribution

| Field | Definition |
| --- | --- |
| Classification | CURRENT · DEMOCRATIZE |
| Implementation state | Partial |
| Purpose | Compare planned and actual transport cost and send a controlled financial packet. |
| Trigger | Trip completion, carrier invoice intake, or accessorial event occurs. |

### Actors

- Transportation finance reviewer
- Planner
- Carrier
- RoutArr
- LedgArr

### State path

`intake → matched → exception → disputed → approved → contributed → reconciled`

### Required sequence

1. Collect selected rate, tender, contract/routing-guide, trip facts, miles, stops, quantities, appointment clocks, proof, and carrier invoice data.
2. Normalize line items and detect duplicates, missing references, and unauthorized accessorials.
3. Calculate expected vs billed variance with contractual rules and evidence.
4. Auto-approve within policy or route disputes/approvals.
5. Communicate dispute to carrier and track response/adjustment.
6. Create final transportation finance packet contribution by cost object/order/load.
7. Send to LedgArr/AP and reconcile acceptance/posting reference.
8. Retain corrections/credit context without altering original evidence.

### Exception and recovery paths

- Invoice missing, rate not found, contract changed, accessorial evidence disputed, currency/tax mismatch, duplicate invoice, or LedgArr mapping fails.
- Customer recharge differs from carrier payable.

### Cross-product and external handoffs

- Carrier/invoice source → RoutArr.
- RoutArr → LedgArr: finance packet.
- RoutArr ↔ OrdArr/CustomArr: customer charge context.
- RecordArr: invoice/evidence.

### Evidence and audit record

- Rate/contract snapshot.
- Invoice and normalized lines.
- Variance/evidence/approvals.
- Finance contribution and reconciliation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Auto-match.
- Cost variance.
- Dispute recovery.
- Days to approval.
- Posting rejection.

## RO-WF-014 — Multimodal leg handoff and continuity

| Field | Definition |
| --- | --- |
| Classification | CURRENT · DEMOCRATIZE |
| Implementation state | Partial |
| Purpose | Manage linked transport legs and preserve custody/milestones across modes. |
| Trigger | A demand is planned as rail/ocean/air/parcel/intermodal or changes mode. |

### Actors

- Transportation planner
- Carriers/forwarders
- Facility users
- RoutArr

### State path

`planned → booked → origin → in_transit → interchange → held → final_mile → completed`

### Required sequence

1. Define journey and legs with mode, carrier, terminals, equipment, service, documents, and milestones.
2. Validate mode-specific requirements and transfer windows.
3. Tender/book each leg and capture confirmation/reference identifiers.
4. Track origin pickup, terminal acceptance, departure/arrival, customs/inspection, interchange, and final delivery.
5. Reconcile container/trailer/seal/custody and quantities at handoffs.
6. Propagate delay impact across downstream legs and customer promises.
7. Replan missed connections and document costs/claims.
8. Complete one journey package and finance contribution while preserving leg detail.

### Exception and recovery paths

- Booking rejected, customs hold, missed connection, container mismatch, transload damage, mode-specific document missing, or milestone provider stale.
- A leg is operated by customer/supplier rather than tenant.

### Cross-product and external handoffs

- RoutArr ↔ carrier/forwarder/terminal providers.
- RoutArr ↔ RecordArr/Compliance Core.
- RoutArr → OrdArr/CustomArr: journey status.

### Evidence and audit record

- Journey/leg plan.
- Bookings/references.
- Milestones/custody.
- Exceptions/replans.
- Documents/finance.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Connection success.
- Milestone completeness.
- End-to-end transit.
- Transfer dwell.
- Claims at handoff.

## RO-WF-015 — Transportation audit/document packet

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Assemble a complete, scoped transport record for customer, carrier, audit, claim, or finance. |
| Trigger | A trip completes, claim opens, audit occurs, or external party requests documents. |

### Actors

- Dispatcher
- Document reviewer
- Auditor
- RoutArr
- RecordArr

### State path

`requested → validating → missing_items → assembling → complete → shared → revoked`

### Required sequence

1. Define packet type/scope and permitted recipient.
2. Collect demand, plan, rate, tender, carrier, route/trip/stops, visibility, appointments, proof, exceptions, claims, and finance refs.
3. Validate required documents by mode/customer/compliance context.
4. Request missing documents or approve a reasoned exception.
5. Apply redaction and external-sharing policy.
6. Request RecordArr package with manifest/version/hash.
7. Deliver through a scoped expiring share or internal link.
8. Log access and supplemental/revocation actions.

### Exception and recovery paths

- Missing/illegible document, legal hold, confidential rate/customer data, external share expired, or source record correction after snapshot.
- Recipient disputes packet completeness.

### Cross-product and external handoffs

- RoutArr ↔ RecordArr: packet/share.
- RoutArr ↔ Compliance Core: required evidence.
- RoutArr → ReportArr: completeness metrics.

### Evidence and audit record

- Scope/recipient/authority.
- Manifest/validation.
- Exceptions/redactions.
- Package/share/access log.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Packet generation time.
- Missing item rate.
- External access success.
- Supplemental requests.



## Workflow definition of done

A workflow is releasable only when its happy path, every listed exception, dependency outage, duplicate/retry, stale/concurrent update, permission denial, cancellation, correction/void, archival/retention, import/API/event path, notification, audit trail, reporting projection, mobile behavior, and professional printable output have automated and human-usable acceptance coverage.
