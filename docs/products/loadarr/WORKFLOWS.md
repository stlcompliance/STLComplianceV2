# LoadArr — WMS Workflow Catalog

[Feature catalog](./FEATURESET.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md)

## Workflow contract

This document defines the end-to-end business state machines for LoadArr. A workflow is not complete because a page or endpoint exists: the trigger, authority, state transitions, validation, handoffs, evidence, exceptions, retries, conflict behavior, notifications, reporting, and final source-of-truth commit must all work together.

## Ownership rule

LoadArr owns warehouse execution and physical inventory truth: receiving, inventory status and custody, putaway, reservation/allocation, replenishment, picking, packing/staging, shipping confirmation, transfer, count, adjustment, return, and the immutable stock ledger. It references StaffArr-owned locations and SupplyArr-owned item/supplier/procurement context rather than duplicating them.

- Canonical internal locations; StaffArr owns location identity and hierarchy, while LoadArr owns warehouse meaning and movement using those IDs.
- Part/item commercial master, suppliers, purchase orders, pricing, or sourcing; SupplyArr owns them.
- Customer/order lifecycle; CustomArr/OrdArr own customer and order truth.
- Transportation trip/route/ETA; RoutArr owns transport and notifies LoadArr of inbound/outbound movements and appointments.
- Quality disposition/release; AssurArr owns it, though LoadArr enforces physical holds and movement blocks.
- Maintenance parts demand/work order; MaintainArr owns demand context while LoadArr reserves/issues inventory.
- General-ledger inventory valuation; LedgArr owns accounting while LoadArr supplies quantity/custody facts.

## Workflow index

| Workflow ID | Workflow | Class | State | Trigger |
| --- | --- | --- | --- | --- |
| LO-WF-001 | Initialize warehouse profile and map StaffArr locations | CURRENT · COMMON | Scaffold | A tenant enables a warehouse/site or adds operational areas. |
| LO-WF-002 | PO/ASN to expected receipt | COMMON | Scaffold | SupplyArr issues/changes a PO or ASN/shipment intent arrives. |
| LO-WF-003 | Dock check-in and receiving | COMMON · UNDERSERVED | Scaffold | Inbound shipment arrives or receiver starts an ad hoc receipt. |
| LO-WF-004 | Receipt discrepancy and quality hold | COMMON · UNDERSERVED | Scaffold | Receiving finds damage, mismatch, unknown item, missing evidence, or policy exception. |
| LO-WF-005 | Directed putaway | COMMON | Scaffold | Accepted inventory is ready for putaway. |
| LO-WF-006 | Demand reservation and allocation | COMMON | Scaffold | OrdArr, MaintainArr, TrainArr, RoutArr, or StaffArr submits material demand. |
| LO-WF-007 | Wave/waveless pick, pack, stage, and ship | COMMON · DEMOCRATIZE | Scaffold | Allocated demand is released for fulfillment. |
| LO-WF-008 | Replenishment and forward-pick maintenance | COMMON | Scaffold | Min/max, forecast, released work, or picker shortage triggers replenishment. |
| LO-WF-009 | Internal or inter-warehouse transfer | COMMON | Scaffold | A planner requests balancing, project staging, quarantine move, or inter-site transfer. |
| LO-WF-010 | Cycle count, discrepancy investigation, and adjustment | COMMON · UNDERSERVED | Scaffold | A schedule, risk event, zero/negative anomaly, user report, or audit triggers a count. |
| LO-WF-011 | Unexplained inventory investigation | CURRENT · UNDERSERVED | Scaffold | A worker finds unknown stock or expected stock cannot be located. |
| LO-WF-012 | Customer/vendor return and disposition | COMMON | Scaffold | An approved return/RMA arrives or warehouse initiates vendor return. |
| LO-WF-013 | Lot/serial recall and targeted inventory control | COMMON · DEMOCRATIZE | Target | AssurArr, SupplyArr, MaintainArr, or external source identifies affected lot/serial/item criteria. |
| LO-WF-014 | Offline mobile warehouse task and sync | UNDERSERVED | Target | A worker downloads/starts an offline-eligible task and connectivity degrades. |
| LO-WF-015 | Warehouse cutover from spreadsheet/legacy system | UNDERSERVED | Target | Tenant prepares a new warehouse or migration cutover. |

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

## LO-WF-001 — Initialize warehouse profile and map StaffArr locations

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Configure warehouse operations without creating a second location hierarchy. |
| Trigger | A tenant enables a warehouse/site or adds operational areas. |

### Actors

- Warehouse administrator
- StaffArr location administrator
- LoadArr

### State path

`draft → mapping → validation → test → active → suspended`

### Required sequence

1. Select a StaffArr site and eligible child locations.
2. Assign warehouse meanings such as receiving, staging, reserve, forward pick, quarantine, packing, dock, yard, and shipping.
3. Configure capacity, handling, temperature/hazard, scan, count, and putaway attributes as LoadArr overlays.
4. Validate duplicate/overlapping operational mappings.
5. Configure devices, labels, printers, task queues, and default policies.
6. Import/verify opening balances only through a controlled cutover transaction.
7. Run test receive/move/pick/count flows.
8. Activate the warehouse profile and audit settings.

### Exception and recovery paths

- Required StaffArr locations missing, location is inactive, opening balance lacks evidence, device/printer unavailable, or incompatible zone rules.
- Quick-create a missing StaffArr location through delegated flow.

### Cross-product and external handoffs

- LoadArr ↔ StaffArr: location refs.
- LoadArr ↔ NexArr: devices/integrations.
- LoadArr → RecordArr: opening-balance package.

### Evidence and audit record

- Mappings/settings/version.
- Opening-balance source/approval.
- Test results.
- Activation audit.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to activate.
- Mapping conflicts.
- Opening balance variance.
- Test-pass rate.

## LO-WF-002 — PO/ASN to expected receipt

| Field | Definition |
| --- | --- |
| Classification | COMMON |
| Implementation state | Scaffold |
| Purpose | Create a warehouse receiving expectation from procurement or inbound transport. |
| Trigger | SupplyArr issues/changes a PO or ASN/shipment intent arrives. |

### Actors

- Receiving coordinator
- SupplyArr
- RoutArr
- LoadArr

### State path

`received → validation → expected → at_risk → arrived → partially_received → closed → canceled`

### Required sequence

1. Receive PO lines, supplier, ship-from, item/UOM/package, quantities, dates, and required documents.
2. Resolve item and warehouse/location mappings without copying commercial master data.
3. Create/update expected receipt idempotently.
4. Attach inbound RoutArr demand/trip/ETA/appointment when available.
5. Identify inspection, SDS, lot/serial/expiry, temperature, and quality requirements.
6. Detect changes after partial receipt and preserve versions.
7. Show receiving queue readiness/blockers.
8. Acknowledge expectation status to SupplyArr.

### Exception and recovery paths

- Unknown item/UOM, inactive warehouse, duplicate ASN, PO canceled, quantity changed after receipt, supplier restricted, or required evidence unknown.
- Receipt may arrive without PO.

### Cross-product and external handoffs

- SupplyArr → LoadArr: PO/ASN.
- RoutArr → LoadArr: ETA/appointment.
- Compliance Core/AssurArr → LoadArr: requirements/hold context.

### Evidence and audit record

- Source versions.
- Mappings and requirements.
- Change history.
- Readiness/acknowledgement.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Expected receipt accuracy.
- Mapping exception.
- Change-after-receipt rate.
- Queue readiness.

## LO-WF-003 — Dock check-in and receiving

| Field | Definition |
| --- | --- |
| Classification | COMMON · UNDERSERVED |
| Implementation state | Scaffold |
| Purpose | Receive goods accurately with custody, discrepancy, and evidence. |
| Trigger | Inbound shipment arrives or receiver starts an ad hoc receipt. |

### Actors

- Receiver
- Dock lead
- Driver/carrier
- LoadArr

### State path

`checked_in → unloading → counting → exception → held → received → partial → complete`

### Required sequence

1. Identify appointment/trip/PO/ASN or create review-required ad hoc receipt.
2. Capture carrier, vehicle/trailer, seal, arrival, packing units, and documents.
3. Unload and scan handling units/items; parse UOM, lot, serial, expiry, and quantity.
4. Compare to expectation and flag over/short/damage/unknown/temperature/document exceptions.
5. Capture photos, packing slip, SDS, and signatures in RecordArr.
6. Route quality-sensitive goods to hold/inspection; accepted goods enter receiving/staging status.
7. Post atomic receipt ledger transactions only after server validation.
8. Complete or partially close receipt and notify SupplyArr/RoutArr.

### Exception and recovery paths

- No PO, unknown barcode, duplicate serial, damaged freight, seal mismatch, count variance, expired item, missing SDS, offline scan, or device failure.
- Blind receiving policy hides expected quantity from receiver.

### Cross-product and external handoffs

- RoutArr ↔ LoadArr: gate/door.
- LoadArr → AssurArr: quality issue/hold.
- LoadArr → RecordArr: evidence.
- LoadArr → SupplyArr: receipt status.

### Evidence and audit record

- Arrival/custody.
- Scans/counts/lots/serials.
- Discrepancies/evidence.
- Ledger transaction IDs.
- Receipt completion.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Dock-to-receive time.
- Receipt accuracy.
- Discrepancy rate.
- Evidence completeness.
- Offline rejection.

## LO-WF-004 — Receipt discrepancy and quality hold

| Field | Definition |
| --- | --- |
| Classification | COMMON · UNDERSERVED |
| Implementation state | Scaffold |
| Purpose | Contain suspect inventory and resolve quantity/quality/document discrepancies. |
| Trigger | Receiving finds damage, mismatch, unknown item, missing evidence, or policy exception. |

### Actors

- Receiver
- Inventory control
- Quality reviewer
- Buyer
- LoadArr
- AssurArr

### State path

`reported → contained → quality_review → supplier_review → released → returned → scrapped → closed`

### Required sequence

1. Create discrepancy linked to receipt line/handling unit with type, quantity, severity, and evidence.
2. Move affected inventory into a restricted status/location through a ledger transaction.
3. Notify AssurArr for nonconformance/hold and SupplyArr for commercial resolution.
4. Prevent allocation/pick/issue while hold is active.
5. Collect inspection, supplier response, documents, and corrected identification.
6. AssurArr decides release/reject/disposition; SupplyArr handles return/credit/replacement.
7. Execute release, reclassify, return, scrap, or adjustment through authorized transactions.
8. Close discrepancy and publish resolution.

### Exception and recovery paths

- Scope uncertain, mixed pallet, affected units already moved, release event out of order, supplier disputes, or inventory quantity changes during review.
- Partial release applies to subset lots/units.

### Cross-product and external handoffs

- LoadArr → AssurArr/SupplyArr.
- AssurArr → LoadArr: hold/release/disposition.
- LoadArr → RecordArr: evidence.
- LoadArr → LedgArr: adjustment/valuation fact.

### Evidence and audit record

- Discrepancy/evidence.
- Containment movements.
- Decisions/approvals.
- Final ledger/disposition.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to contain.
- Hold aging.
- Supplier response.
- Released vs rejected.
- Inventory leakage prevented.

## LO-WF-005 — Directed putaway

| Field | Definition |
| --- | --- |
| Classification | COMMON |
| Implementation state | Scaffold |
| Purpose | Move received or staged goods to an appropriate StaffArr-owned location. |
| Trigger | Accepted inventory is ready for putaway. |

### Actors

- Warehouse worker
- Supervisor
- LoadArr

### State path

`ready → assigned → in_progress → exception → moved → complete`

### Required sequence

1. Create putaway tasks from receipt/staging units.
2. Evaluate item status, size, weight, lot/expiry, hazard/temperature, compatibility, velocity, capacity, and zone rules.
3. Recommend destination and alternatives with reasons.
4. Worker scans source unit/location, travels, and scans destination.
5. Validate destination and capacity at commit time.
6. Post atomic movement and update task/handling-unit state.
7. Handle partial/split/consolidate or exception.
8. Close receipt putaway when all units are accounted for.

### Exception and recovery paths

- Destination full, wrong zone, incompatible stock, location blocked, label unreadable, unit split, worker offline, or another task changes capacity.
- Supervisor chooses an approved alternate location.

### Cross-product and external handoffs

- LoadArr ↔ StaffArr: location validity.
- Field Companion → LoadArr: scans/offline queue.
- LoadArr → SupplyArr/MaintainArr/OrdArr: availability status.

### Evidence and audit record

- Recommendation/rule version.
- Source/destination scans.
- Movement ledger.
- Exceptions/overrides.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Putaway cycle time.
- Travel distance.
- Rule compliance.
- Alternate-location rate.
- Dock-to-stock.

## LO-WF-006 — Demand reservation and allocation

| Field | Definition |
| --- | --- |
| Classification | COMMON |
| Implementation state | Scaffold |
| Purpose | Reserve specific available stock for an order, work order, training, route, or internal demand. |
| Trigger | OrdArr, MaintainArr, TrainArr, RoutArr, or StaffArr submits material demand. |

### Actors

- Inventory planner
- Source product
- LoadArr

### State path

`requested → allocated → partial → backordered → at_risk → released → consumed`

### Required sequence

1. Validate demand source, item, quantity, need-by, priority, location/owner scope, and substitutions.
2. Calculate available-to-promise excluding holds, expiry, restrictions, and prior reservations.
3. Apply allocation policy such as FEFO/FIFO, lot/serial/customer, zone, and partial/backorder rules.
4. Create reservation/allocation with source-line traceability.
5. Return fulfilled/partial/blocked status and explanations.
6. Protect allocated stock from conflicting use.
7. Reallocate/release on demand change, expiry, cancel, or supervisor action.
8. Monitor at-risk reservations before execution.

### Exception and recovery paths

- Insufficient stock, wrong status/location, lot/customer restriction, stale balance, concurrent allocation, substitution approval needed, or demand canceled.
- Emergency higher-priority demand may require approved reallocation.

### Cross-product and external handoffs

- Source product → LoadArr: demand.
- LoadArr → source: status.
- LoadArr → SupplyArr: shortage.
- AssurArr → LoadArr: hold changes.

### Evidence and audit record

- Demand/version.
- Availability calculation.
- Allocation/lot/location.
- Reallocation/release reason.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Fill rate.
- Allocation latency.
- Reservation churn.
- Shortage age.
- At-risk prevention.

## LO-WF-007 — Wave/waveless pick, pack, stage, and ship

| Field | Definition |
| --- | --- |
| Classification | COMMON · DEMOCRATIZE |
| Implementation state | Scaffold |
| Purpose | Execute outbound warehouse work with scan proof and order/transport coordination. |
| Trigger | Allocated demand is released for fulfillment. |

### Actors

- Warehouse planner
- Picker
- Packer
- Loader
- LoadArr

### State path

`released → picking → short → packing → staged → loading → shipped → canceled`

### Required sequence

1. Group or continuously prioritize work by SLA, route/trip, zone, carrier, temperature, equipment, and labor.
2. Create pick tasks and assign eligible workers/equipment.
3. Picker scans source/location/item/lot/serial/quantity; handle short/skip/substitute.
4. Move picked goods to packing or staging and post ledger movements.
5. Pack into handling units, verify quantity/condition, capture weight/dimensions, labels, and documents.
6. Stage by route/trip/stop sequence and notify RoutArr of readiness.
7. At loadout, scan trip/equipment/handling units, seal, and confirm custody.
8. Post shipment/issue transactions and notify OrdArr/source products.

### Exception and recovery paths

- Short pick, wrong lot/serial, damaged item, hold applied mid-pick, packing variance, label failure, trip changed, load capacity issue, or offline device.
- Partial shipment/backorder requires order decision.

### Cross-product and external handoffs

- OrdArr/source → LoadArr: fulfillment release.
- LoadArr ↔ RoutArr: staging/load readiness.
- LoadArr → RecordArr: shipping proof.
- LoadArr → OrdArr/LedgArr: completion facts.

### Evidence and audit record

- Task assignments/scans.
- Short/substitution decisions.
- Handling-unit/label data.
- Load/seal/proof.
- Ledger/shipment events.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Pick accuracy.
- Lines/hour with safety context.
- Short rate.
- Dock-to-ship.
- On-time staging.

## LO-WF-008 — Replenishment and forward-pick maintenance

| Field | Definition |
| --- | --- |
| Classification | COMMON |
| Implementation state | Scaffold |
| Purpose | Keep active pick locations supplied without creating congestion or starving other demand. |
| Trigger | Min/max, forecast, released work, or picker shortage triggers replenishment. |

### Actors

- Warehouse planner
- Replenishment worker
- LoadArr

### State path

`triggered → planned → assigned → in_progress → complete → exception → canceled`

### Required sequence

1. Identify forward location need and protected demand.
2. Find eligible reserve stock using status, lot/expiry, owner, UOM, and movement rules.
3. Prioritize by pick urgency, travel, congestion, and available labor/equipment.
4. Create move task and reserve source quantity.
5. Worker scans source, quantity/handling unit, and destination.
6. Validate capacity and concurrent pick state.
7. Post movement and release replenished stock.
8. Escalate unresolved shortage or repeated slot-capacity issue.

### Exception and recovery paths

- No reserve stock, destination full, source held, task conflicts with pick, UOM conversion error, or equipment unavailable.
- Emergency top-up is performed during active picking.

### Cross-product and external handoffs

- LoadArr → SupplyArr: stockout/shortage.
- Field Companion → LoadArr: task scans.
- ReportArr: slot/replenishment metrics.

### Evidence and audit record

- Trigger/calculation.
- Source/destination selection.
- Scans/movement.
- Exception/resolution.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Stockout at pick face.
- Replenishment travel.
- Late replenishment.
- Repeated capacity exception.

## LO-WF-009 — Internal or inter-warehouse transfer

| Field | Definition |
| --- | --- |
| Classification | COMMON |
| Implementation state | Scaffold |
| Purpose | Move stock with traceable custody between locations or warehouses. |
| Trigger | A planner requests balancing, project staging, quarantine move, or inter-site transfer. |

### Actors

- Inventory planner
- Warehouse workers
- LoadArr
- RoutArr when transported

### State path

`requested → approved → allocated → in_transit → received → exception → complete → canceled`

### Required sequence

1. Create transfer request with source/destination, items, quantities, need-by, reason, and owner.
2. Validate availability, destination compatibility/capacity, and approvals.
3. Reserve stock and create pick/pack/stage tasks.
4. For inter-site, create RoutArr transportation demand and shipment documents.
5. Ship from source using issue/in-transit status.
6. Receive at destination against transfer, reconcile over/short/damage, and put away.
7. Close only when source, in-transit, and destination quantities reconcile.
8. Record exceptions, loss, and financial valuation context.

### Exception and recovery paths

- Destination invalid/full, source shortage, transit damage/loss, partial receipt, transfer canceled after ship, or duplicate receiving.
- Intra-warehouse move does not require transportation leg.

### Cross-product and external handoffs

- LoadArr ↔ RoutArr: transport demand/status.
- LoadArr ↔ AssurArr: transit quality.
- LoadArr → LedgArr: inter-site valuation facts.

### Evidence and audit record

- Request/approval.
- Source issue.
- Transport/custody.
- Destination receipt/putaway.
- Reconciliation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Transfer cycle time.
- In-transit aging.
- Variance/loss.
- Destination stockout avoided.

## LO-WF-010 — Cycle count, discrepancy investigation, and adjustment

| Field | Definition |
| --- | --- |
| Classification | COMMON · UNDERSERVED |
| Implementation state | Scaffold |
| Purpose | Verify physical stock and correct ledger truth through controlled evidence. |
| Trigger | A schedule, risk event, zero/negative anomaly, user report, or audit triggers a count. |

### Actors

- Counter
- Inventory control
- Approver
- LoadArr

### State path

`scheduled → counting → variance → recount → investigation → approval → adjusted → closed`

### Required sequence

1. Create blind or guided count tasks by location/item/lot/serial/handling unit.
2. Freeze, snapshot, or account for concurrent movements according to policy.
3. Counter scans location and physical units; system records raw count independently.
4. Compare to expected and require recount above tolerance.
5. Investigate recent movements, receipts, picks, transfers, devices/users, and neighboring locations.
6. Record cause, evidence, and corrective action.
7. Approve and post an adjustment ledger transaction rather than editing balance.
8. Close task and feed risk/count frequency.

### Exception and recovery paths

- Movement during count, inaccessible location, mixed/unknown item, serial duplicate, recount disagreement, suspected theft, or offline count conflict.
- High-risk variance requires separation of duties.

### Cross-product and external handoffs

- Field Companion → LoadArr: count scans.
- LoadArr → AssurArr/StaffArr: quality/incident context when needed.
- LoadArr → LedgArr/ReportArr: adjustment facts.

### Evidence and audit record

- Count snapshot/raw counts.
- Recounts and movement review.
- Cause/evidence.
- Approval/adjustment transaction.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Inventory accuracy.
- Variance value/units.
- Adjustment approval time.
- Repeat discrepancy.
- Count productivity.

## LO-WF-011 — Unexplained inventory investigation

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Scaffold |
| Purpose | Resolve found, missing, mislocated, or unidentified stock without unsafe balance edits. |
| Trigger | A worker finds unknown stock or expected stock cannot be located. |

### Actors

- Warehouse worker
- Inventory control
- Supervisor
- LoadArr

### State path

`reported → contained → investigating → candidate_found → approval → resolved → closed`

### Required sequence

1. Create case with scanned identifiers, location, quantity, photos, and found/missing type.
2. Quarantine or protect affected units from allocation.
3. Search item/lot/serial/handling-unit, open receipts, transfers, picks, counts, and neighboring locations.
4. Build a custody timeline and rank plausible matches/causes with confidence.
5. Ask targeted questions or create physical search/count tasks.
6. Resolve by relabel/move/link/return/adjustment/quality review with approval.
7. Post required ledger transactions and update affected demands.
8. Close with cause and preventive action.

### Exception and recovery paths

- No identity, counterfeit/suspect item, multiple possible owners, serial duplicate, evidence of theft, or affected stock already shipped.
- Found inventory belongs to another tenant/client.

### Cross-product and external handoffs

- LoadArr ↔ SupplyArr/RecordArr/AssurArr/StaffArr as needed.
- LoadArr → ReportArr: root cause metrics.

### Evidence and audit record

- Original report/evidence.
- Search/custody timeline.
- Candidate decisions.
- Approved resolution/ledger.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Resolution time.
- Recovered inventory.
- Unknown-item recurrence.
- Adjustment avoided.

## LO-WF-012 — Customer/vendor return and disposition

| Field | Definition |
| --- | --- |
| Classification | COMMON |
| Implementation state | Scaffold |
| Purpose | Receive returned goods and route them to restock, hold, repair, vendor return, scrap, or other disposition. |
| Trigger | An approved return/RMA arrives or warehouse initiates vendor return. |

### Actors

- Returns receiver
- Quality reviewer
- Customer service/buyer
- LoadArr

### State path

`expected → received → inspection → disposition → restocked → returned_to_vendor → scrapped → closed`

### Required sequence

1. Validate return authorization/source order/PO, item, quantity, reason, and expected condition.
2. Check in and scan units/lots/serials; capture packaging, condition, and evidence.
3. Place in return/inspection status and prevent unrestricted allocation.
4. AssurArr or authorized inspector chooses disposition criteria.
5. Restock, relabel, repackage, repair handoff, vendor return, quarantine, or scrap through explicit movements.
6. Notify OrdArr/CustomArr/SupplyArr of accepted/rejected quantities and disposition.
7. Generate financial contribution/credit context.
8. Close when physical and commercial outcomes reconcile.

### Exception and recovery paths

- No authorization, wrong item, counterfeit/suspect, missing serial, hazardous return, customer data on device, vendor rejects return, or partial disposition.
- Recall return follows campaign-specific rules.

### Cross-product and external handoffs

- OrdArr/CustomArr/SupplyArr ↔ LoadArr.
- LoadArr ↔ AssurArr/MaintainArr/RecordArr.
- LoadArr → LedgArr: quantity/value facts.

### Evidence and audit record

- Authorization/source.
- Receipt/condition/evidence.
- Disposition approval.
- Movements and external outcome.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Return cycle time.
- Restock rate.
- Unauthorized returns.
- Disposition aging.
- Credit reconciliation.

## LO-WF-013 — Lot/serial recall and targeted inventory control

| Field | Definition |
| --- | --- |
| Classification | COMMON · DEMOCRATIZE |
| Implementation state | Target |
| Purpose | Locate, block, trace, and disposition affected inventory and shipments. |
| Trigger | AssurArr, SupplyArr, MaintainArr, or external source identifies affected lot/serial/item criteria. |

### Actors

- Inventory control
- Quality reviewer
- LoadArr

### State path

`received → tracing → blocked → recovering → disposition → released → closed`

### Required sequence

1. Receive recall/hold scope and normalize item/lot/serial/date/supplier criteria.
2. Find current on-hand, reserved, picked, staged, in-transit, issued, returned, and shipped units from ledger/custody history.
3. Immediately block affected current stock and tasks.
4. Notify affected demand/order/work/transport owners with specific unit refs.
5. Create search/count/recovery tasks for uncertain units.
6. Execute return, inspection, rework, relabel, destroy, or release only from authorized quality decision.
7. Reconcile affected population and prove no leakage.
8. Generate traceability package and close residual exceptions.

### Exception and recovery paths

- Scope ambiguous, commingled inventory, missing lot capture, already delivered units, out-of-order release, or customer recovery required.
- Partial lot/serial release.

### Cross-product and external handoffs

- AssurArr → LoadArr: hold/decision.
- LoadArr → OrdArr/CustomArr/MaintainArr/RoutArr/SupplyArr: affected refs.
- RecordArr: package.

### Evidence and audit record

- Scope/version.
- Trace population.
- Block/recovery actions.
- Disposition/release.
- Reconciliation package.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Trace time.
- Affected-unit coverage.
- Leakage.
- Recovery completion.
- Residual unknowns.

## LO-WF-014 — Offline mobile warehouse task and sync

| Field | Definition |
| --- | --- |
| Classification | UNDERSERVED |
| Implementation state | Target |
| Purpose | Continue safe scan work during connectivity loss without lying about committed inventory. |
| Trigger | A worker downloads/starts an offline-eligible task and connectivity degrades. |

### Actors

- Warehouse worker
- Field Companion
- LoadArr

### State path

`downloaded → offline → queued → syncing → conflict → committed → rejected`

### Required sequence

1. Validate worker, device, task, location scope, and allowed offline transaction types.
2. Download versioned task, expected identifiers, and limited reference data encrypted with expiry.
3. Execute scans/actions locally with sequence, timestamp, device, and idempotency IDs.
4. Show inventory effects as pending, not committed.
5. On reconnect, sync in dependency order and revalidate balance, task state, location, hold, and concurrent changes.
6. Auto-accept safe actions and route conflicts for explicit resolution.
7. Update worker with accepted/rejected outcomes and next action.
8. Purge expired local data and audit the sync.

### Exception and recovery paths

- Task reassigned/canceled, stock moved, hold applied, duplicate scan, location closed, device compromised, or sequence gap.
- Some actions, such as final adjustment approval, are never offline-eligible.

### Cross-product and external handoffs

- Field Companion ↔ NexArr: device/session/offline queue.
- Field Companion ↔ LoadArr: task/transactions.
- LoadArr → source products: committed result only.

### Evidence and audit record

- Downloaded version.
- Local action sequence.
- Server validation/conflicts.
- Final transaction IDs.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Offline task success.
- Sync latency.
- Conflict/rejection rate.
- Duplicate prevention.

## LO-WF-015 — Warehouse cutover from spreadsheet/legacy system

| Field | Definition |
| --- | --- |
| Classification | UNDERSERVED |
| Implementation state | Target |
| Purpose | Move opening inventory and active work into the durable ledger with proof and rollback control. |
| Trigger | Tenant prepares a new warehouse or migration cutover. |

### Actors

- Warehouse administrator
- Implementation lead
- Inventory control
- LoadArr

### State path

`discovery → mapping → dry_run → freeze → cutover → reconciliation → active → correcting`

### Required sequence

1. Inventory and clean StaffArr locations, SupplyArr items/UOMs, statuses, lots/serials, handling units, and open receipts/orders.
2. Create mapping templates and validate duplicates, impossible quantities, missing IDs, and unsupported states.
3. Run dry imports and compare control totals by item/location/status/value context.
4. Freeze legacy movement or define a delta window.
5. Perform physical verification/count for high-risk items.
6. Post opening-balance transactions with source manifest and approval.
7. Import/recreate active receipts, reservations, picks, transfers, holds, and counts with idempotent references.
8. Reconcile, sign off, and retain rollback/correction plan.

### Exception and recovery paths

- Legacy negative/ambiguous stock, missing lot/serial, location mismatch, duplicate open work, cutover movement, or totals do not reconcile.
- Rollback must use reversing/correcting transactions, not delete the ledger.

### Cross-product and external handoffs

- LoadArr ↔ StaffArr/SupplyArr/OrdArr/AssurArr.
- LoadArr → RecordArr: source/manifests.
- LoadArr → LedgArr/ReportArr: reconciliation.

### Evidence and audit record

- Source files/hashes.
- Mappings/errors.
- Dry-run/control totals.
- Opening transactions.
- Signoff/corrections.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Cutover variance.
- Unmapped records.
- Downtime.
- Post-cutover corrections.
- Inventory accuracy.



## Workflow definition of done

A workflow is releasable only when its happy path, every listed exception, dependency outage, duplicate/retry, stale/concurrent update, permission denial, cancellation, correction/void, archival/retention, import/API/event path, notification, audit trail, reporting projection, mobile behavior, and professional printable output have automated and human-usable acceptance coverage.
