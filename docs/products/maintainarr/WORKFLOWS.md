# MaintainArr — CMMS Workflow Catalog

[Feature catalog](./FEATURESET.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md)

## Workflow contract

This document defines the end-to-end business state machines for MaintainArr. A workflow is not complete because a page or endpoint exists: the trigger, authority, state transitions, validation, handoffs, evidence, exceptions, retries, conflict behavior, notifications, reporting, and final source-of-truth commit must all work together.

## Ownership rule

MaintainArr owns physical asset identity and configuration in the maintenance domain, maintenance strategy, inspection and defect truth, work execution, downtime, asset readiness, and return-to-service. It requests parts from LoadArr/SupplyArr, people and qualifications from StaffArr/TrainArr, evidence storage from RecordArr, quality decisions from AssurArr, and compliance meaning from Compliance Core.

- Physical inventory balance, reservation, issue, return, or stock ledger; LoadArr owns those transactions.
- Supplier identity, sourcing, purchase order, pricing, lead time, or vendor performance; SupplyArr owns those truths.
- Person/employment/permission/location identity; StaffArr owns them.
- Qualification issuance; TrainArr owns it, though MaintainArr may require and check qualifications.
- Quality hold/release or CAPA; AssurArr owns quality decisions.
- Route/trip execution; RoutArr consumes readiness and reports operational defects/exceptions.
- Document binaries; RecordArr owns evidence files and packages.

## Workflow index

| Workflow ID | Workflow | Class | State | Trigger |
| --- | --- | --- | --- | --- |
| MA-WF-001 | Asset create, enrich, commission, and activate | CURRENT · COMMON | Durable | An authorized user creates/imports an asset or scans an unknown asset. |
| MA-WF-002 | Work request intake and triage | COMMON · UNDERSERVED | Partial | A worker, customer-facing user, integration, or Field Companion submits a maintenance concern. |
| MA-WF-003 | Inspection execution to defect | CURRENT · COMMON | Durable | A scheduled/ad hoc inspection is started for an asset. |
| MA-WF-004 | Defect to work order to parts to return-to-service | CURRENT · UNDERSERVED | Durable | A defect is created from inspection, operator report, route exception, PM, quality finding, or manual entry. |
| MA-WF-005 | Preventive maintenance forecast, generate, execute, and defer | CURRENT · COMMON | Durable | A PM due scan, meter reading, calendar threshold, or manual forecast occurs. |
| MA-WF-006 | Meter reading and condition threshold response | CURRENT · COMMON | Durable | A manual, telematics, sensor, or import reading arrives. |
| MA-WF-007 | Emergency breakdown and rapid dispatch | COMMON · UNDERSERVED | Partial | An operator reports a disabled or unsafe asset. |
| MA-WF-008 | Parts request, reservation, issue, return, and shortage procurement | CURRENT · COMMON | Partial | A planner/technician adds a required part or kit to a work order. |
| MA-WF-009 | Vendor/contractor maintenance work | CURRENT · COMMON | Partial | A work order is assigned or subcontracted to an external vendor. |
| MA-WF-010 | Asset recall campaign | CURRENT · DEMOCRATIZE | Durable | A manufacturer/regulator/provider campaign is ingested or created. |
| MA-WF-011 | Downtime start, reason change, and availability restoration | CURRENT · COMMON | Durable | An asset becomes unavailable/degraded or returns to service. |
| MA-WF-012 | Quality hold on asset or installed part | CURRENT · UNDERSERVED | Partial | AssurArr creates or expands a hold affecting an asset, component, part lot, or work result. |
| MA-WF-013 | Asset reservation and motor-pool readiness | COMMON · UNDERSERVED | Target | A worker requests an asset for a time window and purpose. |
| MA-WF-014 | Maintenance audit package and asset history | CURRENT · COMMON | Durable | An auditor, customer, supervisor, or incident reviewer requests a package. |

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

## MA-WF-001 — Asset create, enrich, commission, and activate

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Create a trustworthy asset record and make it operationally ready. |
| Trigger | An authorized user creates/imports an asset or scans an unknown asset. |

### Actors

- Asset administrator
- Technician
- Manager
- MaintainArr

### State path

`draft → review → commissioning → active → inactive → disposed`

### Required sequence

1. Search identifiers, VIN/serial, external mappings, and likely duplicates.
2. Select class/type and load the data-driven fieldset.
3. Capture minimum identity, ownership, site/location, status, criticality, and responsible team.
4. Optionally query approved reference providers and review enrichment suggestions.
5. Add components, meters, documents, warranty, recall, and compliance context.
6. Create initial inspection/PM/readiness checks.
7. Commission/activate after required evidence and approvals.
8. Emit asset-created/readiness events and retain enrichment provenance.

### Exception and recovery paths

- Duplicate/ambiguous identifier, missing location, unsupported type, provider conflict, active recall, or required inspection not complete.
- Asset must be created quickly from a work request and backfilled later.

### Cross-product and external handoffs

- StaffArr → MaintainArr: location/person refs.
- RecordArr → MaintainArr: document refs.
- Compliance Core/AssurArr: requirements/holds.
- RoutArr/LoadArr: asset reference as needed.

### Evidence and audit record

- Source/import/provenance.
- Fieldset and values.
- Enrichment accept/reject.
- Commissioning checks and approvals.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to active.
- Duplicate prevention.
- Backfill completion.
- Enrichment acceptance/accuracy.

## MA-WF-002 — Work request intake and triage

| Field | Definition |
| --- | --- |
| Classification | COMMON · UNDERSERVED |
| Implementation state | Partial |
| Purpose | Turn a simple request into the right defect, inspection, or work order without burdening the requester. |
| Trigger | A worker, customer-facing user, integration, or Field Companion submits a maintenance concern. |

### Actors

- Requester
- Planner/supervisor
- MaintainArr

### State path

`submitted → triage → merged → accepted → converted → rejected → closed`

### Required sequence

1. Identify asset or location by search/scan; allow unknown-asset capture.
2. Collect concise symptom, impact, urgency, availability window, photos, and contact.
3. Suggest related open requests/defects to prevent duplicates.
4. Apply safety/criticality triage and immediate out-of-service action when authorized.
5. Classify as information, defect, inspection, planned work, emergency, or external service.
6. Create/link the owned record and assign triage owner.
7. Notify requester of status and next action.
8. Track conversion, rejection, merge, and completion.

### Exception and recovery paths

- Unsafe condition, no asset match, duplicate request, non-maintenance issue, malicious/irrelevant submission, or insufficient information.
- Requester cannot see confidential corrective details.

### Cross-product and external handoffs

- Field Companion/portal → MaintainArr.
- MaintainArr → RecordArr: evidence.
- MaintainArr → StaffArr/TrainArr: responder/qualification context.

### Evidence and audit record

- Original request and media.
- Triage decision/reason.
- Linked defect/work order.
- Requester communications.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to triage.
- Duplicate rate.
- Emergency response time.
- Requester satisfaction.

## MA-WF-003 — Inspection execution to defect

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Execute a versioned inspection and create actionable findings. |
| Trigger | A scheduled/ad hoc inspection is started for an asset. |

### Actors

- Inspector/operator
- Supervisor
- MaintainArr

### State path

`scheduled → in_progress → paused → review → passed → failed → voided`

### Required sequence

1. Validate asset, template/version, inspector permission/qualification, and prerequisite safety state.
2. Capture start time, meter, location, and preconditions.
3. Guide each checklist item with conditional logic, measurements, evidence, and pause/resume.
4. Apply item and overall pass/fail/warning rules server-side.
5. Create defects for failed or concerning responses with severity/readiness impact.
6. Require critical acknowledgement, supervisor review, or immediate lockout as configured.
7. Complete/sign the inspection and store evidence refs.
8. Update readiness and trigger work/remediation.

### Exception and recovery paths

- Offline sync conflict, template superseded, inspector loses qualification, critical failure, skipped required item, or evidence upload fails.
- Inspection is abandoned or taken over by another inspector.

### Cross-product and external handoffs

- Field Companion ↔ MaintainArr: run/answers/evidence.
- MaintainArr → RecordArr: evidence.
- MaintainArr → StaffArr/TrainArr: inspector context.
- MaintainArr → RoutArr/LoadArr: readiness change.

### Evidence and audit record

- Template/version.
- Answer/timestamp/pause history.
- Evidence and defects.
- Signature/review and readiness result.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Completion time.
- Critical fail rate.
- Defects per inspection.
- Evidence completeness.
- Offline conflict rate.

## MA-WF-004 — Defect to work order to parts to return-to-service

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Durable |
| Purpose | Coordinate the full corrective-maintenance loop while preserving ownership boundaries. |
| Trigger | A defect is created from inspection, operator report, route exception, PM, quality finding, or manual entry. |

### Actors

- Technician
- Planner
- Supervisor
- MaintainArr
- LoadArr
- SupplyArr

### State path

`open → triaged → planned → waiting_parts → scheduled → in_progress → testing → closed → returned_to_service`

### Required sequence

1. Classify severity, failure mode, readiness, and immediate containment.
2. Link/create a work order with tasks, estimates, permit, skill, and evidence requirements.
3. Check/assign qualified technicians through StaffArr/TrainArr context.
4. Create parts demand; LoadArr reserves/issues available stock and SupplyArr procures shortages.
5. Execute tasks, labor, measurements, parts installation/return, comments, and evidence.
6. Resolve blockers and complete required post-repair inspection/test.
7. Review closeout and make an explicit return-to-service decision.
8. Update readiness/history/downtime and assemble RecordArr evidence package.

### Exception and recovery paths

- Unsafe asset, technician unqualified, part unavailable/held, permit missing, external vendor needed, test fails, compliance fact unknown, or approval absent.
- Temporary repair requires a time-limited restriction and follow-up work.

### Cross-product and external handoffs

- MaintainArr → LoadArr: reserve/issue/return.
- LoadArr → SupplyArr: shortage/procurement.
- MaintainArr ↔ StaffArr/TrainArr: assignee/qualification.
- MaintainArr ↔ AssurArr/Compliance Core/RecordArr.
- MaintainArr → RoutArr: readiness impact.

### Evidence and audit record

- Defect/source and severity.
- Work plan/tasks/labor/parts.
- Blockers/overrides.
- Test/inspection evidence.
- Return-to-service approval/package.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Downtime.
- MTTR.
- Parts delay.
- Repeat defect.
- First-time fix.
- Evidence completeness.

## MA-WF-005 — Preventive maintenance forecast, generate, execute, and defer

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Create due work from reliable schedules and manage exceptions explicitly. |
| Trigger | A PM due scan, meter reading, calendar threshold, or manual forecast occurs. |

### Actors

- Planner
- Technician
- Supervisor
- MaintainArr

### State path

`forecast → due → generated → scheduled → in_progress → completed → deferred → canceled`

### Required sequence

1. Evaluate active PM programs/schedules against calendar, meter, usage, season, and asset status.
2. Forecast due/overdue windows and detect duplicate/conflicting work.
3. Generate occurrence/work order at the configured lead time.
4. Plan labor, parts, permits, downtime window, and dependencies.
5. Execute and capture task/evidence/meter results.
6. If deferred, require reason, risk, new date/trigger, and approval.
7. Close occurrence and calculate next due without drift or double generation.
8. Update PM compliance/effectiveness reporting.

### Exception and recovery paths

- Missing/stale meter, asset unavailable, duplicate schedule, seasonal shutdown, part shortage, repeated deferral, or asset disposed.
- PM finds a defect that changes the work scope.

### Cross-product and external handoffs

- MaintainArr ↔ LoadArr/SupplyArr: planned parts.
- MaintainArr ↔ StaffArr/TrainArr: labor qualification.
- MaintainArr → ReportArr: PM facts.

### Evidence and audit record

- Schedule/program version.
- Due calculation inputs.
- Generated work and deferral approval.
- Completion and next-due calculation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- PM compliance.
- Schedule adherence.
- Deferral aging.
- PM-generated defects.
- Emergency work after PM.

## MA-WF-006 — Meter reading and condition threshold response

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Capture trustworthy readings and act on abnormal conditions. |
| Trigger | A manual, telematics, sensor, or import reading arrives. |

### Actors

- Operator/technician
- Integration
- MaintainArr

### State path

`received → validated → accepted → suspect → corrected → actioned`

### Required sequence

1. Resolve asset/meter and validate unit, timestamp, source, monotonicity, and plausible range.
2. Detect duplicates, resets, rollover, backdated readings, and anomalies.
3. Store accepted reading with confidence/source.
4. Recompute PM due state and condition thresholds.
5. Create warning, inspection, defect, or work order proposal as configured.
6. Route suspect readings for review rather than silently discarding.
7. Publish due/readiness changes and retain correction history.

### Exception and recovery paths

- Unknown asset/meter, unit mismatch, impossible decrease, stale integration, sensor drift, duplicate, or a legitimate meter replacement/reset.
- Reading arrives offline after later readings.

### Cross-product and external handoffs

- External telemetry/Field Companion → MaintainArr.
- MaintainArr → ReportArr/Compliance Core as required.
- MaintainArr → planner inbox.

### Evidence and audit record

- Raw source payload/ref.
- Validation/anomaly decision.
- Corrections/reset rationale.
- Triggered actions.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Reading acceptance.
- Suspect rate.
- Threshold-to-action time.
- PM forecast accuracy.

## MA-WF-007 — Emergency breakdown and rapid dispatch

| Field | Definition |
| --- | --- |
| Classification | COMMON · UNDERSERVED |
| Implementation state | Partial |
| Purpose | Contain a breakdown and restore service with minimum navigation while preserving controls. |
| Trigger | An operator reports a disabled or unsafe asset. |

### Actors

- Operator
- Dispatcher/supervisor
- Technician
- MaintainArr
- RoutArr when mobile asset

### State path

`reported → contained → dispatched → in_progress → stabilized → tested → returned → recovery_required`

### Required sequence

1. Create emergency defect/work request by scan, asset selection, or location.
2. Mark immediate safety and service impact; change readiness if authorized.
3. Notify supervisor/dispatcher and identify qualified available responders.
4. Create emergency work order with minimum required fields and incident/route context.
5. Check parts, tools, tow/vendor, permit, and access needs.
6. Execute repair/temporary stabilization with live status and evidence.
7. Test and obtain return-to-service or recovery/tow decision.
8. Backfill planning/cost/root-cause fields after stabilization and review.

### Exception and recovery paths

- Person injury, environmental release, inaccessible asset, no qualified responder, no part, unsafe roadside/site condition, or temporary repair only.
- Active trip/orders require reroute or customer notification.

### Cross-product and external handoffs

- RoutArr → MaintainArr: trip breakdown context.
- MaintainArr → RoutArr: readiness/recovery status.
- MaintainArr ↔ StaffArr/TrainArr/LoadArr/SupplyArr/RecordArr.

### Evidence and audit record

- Initial report/location.
- Safety/readiness actions.
- Dispatch/response timeline.
- Repair/test/evidence.
- Post-event review.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Response time.
- Service restoration time.
- Tow/vendor rate.
- Temporary repair follow-up.
- Customer impact.

## MA-WF-008 — Parts request, reservation, issue, return, and shortage procurement

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Get the right material to maintenance without duplicating inventory truth. |
| Trigger | A planner/technician adds a required part or kit to a work order. |

### Actors

- Planner
- Technician
- LoadArr
- SupplyArr
- MaintainArr

### State path

`requested → reserved → shortage → ordered → received → issued → returned → canceled`

### Required sequence

1. Create a demand line with part/spec, quantity, need-by, work order/task, acceptable substitutes, and criticality.
2. Ask LoadArr for availability across permitted locations and lots/statuses.
3. Reserve or stage available stock and show custody/status in MaintainArr.
4. For shortage, send demand to SupplyArr for sourcing/approval/order.
5. Receive shipment/receipt/putaway status from SupplyArr/LoadArr.
6. Issue/scan parts to the work order; record installed component/serial/lot as needed.
7. Return unused/removed/core material through LoadArr and create warranty/quality actions where applicable.
8. Reconcile demand quantity/status/cost contribution at closeout.

### Exception and recovery paths

- Unknown part, no stock, held/expired item, substitute not approved, partial receipt, lost reservation, wrong serial/lot, or emergency local purchase.
- Technician needs a minimal quick-created part reference pending catalog review.

### Cross-product and external handoffs

- MaintainArr → LoadArr: demand/reserve/issue/return.
- LoadArr → SupplyArr: shortage.
- SupplyArr/LoadArr → MaintainArr: status.
- AssurArr: held/suspect part.

### Evidence and audit record

- Demand and substitutions.
- Reservation/order/receipt refs.
- Issue/return scans.
- Installed-component and cost refs.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Parts fill rate.
- Shortage delay.
- Reservation accuracy.
- Unused return rate.
- Emergency buy rate.

## MA-WF-009 — Vendor/contractor maintenance work

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Authorize external work and capture scope, safety, progress, proof, and outcome. |
| Trigger | A work order is assigned or subcontracted to an external vendor. |

### Actors

- Planner
- Vendor contact/technician
- Supervisor
- SupplyArr
- MaintainArr

### State path

`draft → offered → accepted → scheduled → in_progress → vendor_complete → acceptance → closed`

### Required sequence

1. Select approved vendor from SupplyArr and verify restrictions/insurance/qualification context.
2. Create vendor-work scope, asset, location, access window, safety requirements, not-to-exceed amount, and evidence requirements.
3. Issue a scoped, expiring portal invitation or dispatch.
4. Vendor accepts/declines, schedules, checks in, and submits updates/evidence.
5. Internal supervisor reviews deviations, parts, additional authorization, and quality concerns.
6. Vendor completes and submits service report/invoice context.
7. Internal qualified person inspects/tests and decides closeout/return-to-service.
8. Update vendor performance/warranty/financial packet refs.

### Exception and recovery paths

- Vendor restricted, insurance expired, access denied, quote exceeds limit, hidden damage, incomplete evidence, failed acceptance test, or disputed invoice.
- Emergency vendor work begins before full PO approval under controlled exception.

### Cross-product and external handoffs

- MaintainArr ↔ SupplyArr: vendor/commercial context.
- MaintainArr ↔ RecordArr: portal evidence.
- MaintainArr → LedgArr/OrdArr as financial/service context requires.

### Evidence and audit record

- Scope/authorization.
- Portal access/acceptance.
- Updates/evidence/changes.
- Acceptance and return-to-service.
- Cost/warranty refs.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Vendor response.
- Schedule adherence.
- Acceptance pass.
- Cost variance.
- Evidence completeness.

## MA-WF-010 — Asset recall campaign

| Field | Definition |
| --- | --- |
| Classification | CURRENT · DEMOCRATIZE |
| Implementation state | Durable |
| Purpose | Identify affected assets, control risk, complete campaign work, and prove coverage. |
| Trigger | A manufacturer/regulator/provider campaign is ingested or created. |

### Actors

- Recall administrator
- Planner
- Technician
- MaintainArr

### State path

`ingested → matching → review → open → scheduled → remedied → submitted → closed`

### Required sequence

1. Create/import campaign source, identifiers, affected criteria, remedy, dates, and evidence.
2. Normalize make/model/component aliases and match assets with confidence.
3. Review ambiguous matches and mark not-applicable decisions with reason.
4. Create asset recall cases and apply readiness warning/block as required.
5. Plan parts, vendor appointments, work orders, and customer/operations coordination.
6. Execute remedy and capture campaign-specific evidence/claim context.
7. Submit/record completion with external provider where available.
8. Monitor open population, newly matched assets, and residual risk.

### Exception and recovery paths

- Ambiguous VIN/component, provider unavailable, remedy unavailable, asset sold/disposed, duplicate campaign, or completion rejected.
- Interim instructions change before remedy is available.

### Cross-product and external handoffs

- External provider → MaintainArr: campaign data.
- MaintainArr ↔ LoadArr/SupplyArr: remedy parts.
- MaintainArr → RoutArr/StaffArr: readiness/assignment impact.
- RecordArr: evidence.

### Evidence and audit record

- Source/version.
- Match rationale/confidence.
- Applicability decisions.
- Work/remedy evidence.
- Provider submission/result.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Population coverage.
- Ambiguous match aging.
- Time to remedy.
- Rejected completions.
- Residual open risk.

## MA-WF-011 — Downtime start, reason change, and availability restoration

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Maintain a reliable availability timeline independent of work-order status assumptions. |
| Trigger | An asset becomes unavailable/degraded or returns to service. |

### Actors

- Operator
- Supervisor
- Technician
- MaintainArr

### State path

`available → degraded → down_planned → down_unplanned → testing → available`

### Required sequence

1. Start downtime with asset, timestamp, planned/unplanned, reason, impact, and source.
2. Link related defect/work order/trip/quality hold without requiring one.
3. Update reason/impact through append-only events when conditions change.
4. Track waiting states such as diagnosis, labor, part, vendor, approval, or test.
5. Record restored time only after readiness decision, not merely task completion.
6. Split/merge overlapping downtime safely and correct errors with reason.
7. Generate availability snapshots/rollups.
8. Publish availability/readiness to dependent products.

### Exception and recovery paths

- Late report, unknown exact time, overlapping events, asset partially available, work order closed prematurely, or quality hold remains.
- Timezone/clock-source discrepancy.

### Cross-product and external handoffs

- MaintainArr ↔ RoutArr/LoadArr: operational availability.
- MaintainArr → ReportArr: downtime events/rollups.
- AssurArr → MaintainArr: hold state.

### Evidence and audit record

- Downtime event history.
- Reason/impact changes.
- Linked records.
- Restoration/approval.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Availability.
- MTTR.
- Waiting-state duration.
- Late-entry correction.
- Planned vs unplanned.

## MA-WF-012 — Quality hold on asset or installed part

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Partial |
| Purpose | Respect AssurArr quality decisions while maintaining local readiness and work context. |
| Trigger | AssurArr creates or expands a hold affecting an asset, component, part lot, or work result. |

### Actors

- Quality reviewer
- Maintenance supervisor
- Technician
- AssurArr
- MaintainArr

### State path

`hold_received → blocked → containment → rework → awaiting_release → released → rejected`

### Required sequence

1. Receive hold event idempotently and resolve affected asset/component/work order.
2. Create local blocker and update readiness explanation.
3. Prevent return-to-service or affected parts installation as the hold scope requires.
4. Create inspection/rework/containment work requested by AssurArr.
5. Capture maintenance evidence and send outcome to AssurArr.
6. Wait for explicit hold release/reject/disposition.
7. Verify release scope/version and clear only matching local blockers.
8. Record residual warnings and final readiness decision.

### Exception and recovery paths

- Affected scope ambiguous, hold event missing, release arrives out of order, part already installed, emergency override requested, or rework fails.
- Multiple holds from different quality cases overlap.

### Cross-product and external handoffs

- AssurArr → MaintainArr: hold/release.
- MaintainArr → AssurArr: work/evidence outcome.
- MaintainArr ↔ RecordArr/LoadArr/RoutArr.

### Evidence and audit record

- Hold/release event/version.
- Affected object resolution.
- Local blockers/actions.
- Evidence and readiness outcome.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Hold propagation latency.
- Blocked returns prevented.
- Release-to-unblock latency.
- Scope mismatch.

## MA-WF-013 — Asset reservation and motor-pool readiness

| Field | Definition |
| --- | --- |
| Classification | COMMON · UNDERSERVED |
| Implementation state | Target |
| Purpose | Reserve a shared asset only when it will be safe, available, and appropriate. |
| Trigger | A worker requests an asset for a time window and purpose. |

### Actors

- Requester
- Fleet/asset coordinator
- MaintainArr
- StaffArr
- TrainArr

### State path

`requested → approved → reserved → checked_out → in_use → returned → inspection → closed → canceled`

### Required sequence

1. Capture purpose, time, pickup/return location, capacity/equipment needs, and driver/operator.
2. Search assets by type, location, availability, readiness, assignment, maintenance forecast, and qualification requirements.
3. Show explainable conflicts and suitable alternatives.
4. Reserve with approval or auto-approval policy.
5. Perform pre-use inspection, meter, custody handoff, and acknowledgement.
6. Track usage, incidents/defects, fuel/charge, and extension conflicts.
7. Perform return inspection, meter, damage/cleanliness, and key/equipment handoff.
8. Close reservation and update readiness/next booking.

### Exception and recovery paths

- Asset becomes down, recall/hold, overdue prior user, operator unqualified, overlapping reservation, no-show, late return, or damage found.
- Emergency reassignment displaces a reservation.

### Cross-product and external handoffs

- StaffArr/TrainArr → MaintainArr: operator identity/qualification.
- Field Companion → MaintainArr: inspections/custody.
- RoutArr: trip context when used.

### Evidence and audit record

- Request/approval.
- Readiness snapshot.
- Checkout/return inspections.
- Custody/meter/damage.
- Conflict/override.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Utilization.
- Ready reservation fulfillment.
- Late return.
- Damage rate.
- No-show/cancellation.

## MA-WF-014 — Maintenance audit package and asset history

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Assemble an asset- or work-scoped history with traceable evidence. |
| Trigger | An auditor, customer, supervisor, or incident reviewer requests a package. |

### Actors

- Auditor
- Maintenance administrator
- RecordArr
- MaintainArr

### State path

`requested → collecting → gap_review → assembling → complete → supplemented → closed`

### Required sequence

1. Define scope by asset/component, work order, date, campaign, defect, or readiness decision.
2. Snapshot asset configuration/status/history and relevant PM/inspection/work/downtime records.
3. Resolve people/qualification, part custody, vendor, quality, compliance, and route references.
4. Validate evidence availability, signatures, timestamps, and retention/legal hold.
5. Request RecordArr package with a manifest and redaction policy.
6. Review gaps and create remediation tasks or accepted exceptions.
7. Finalize/lock package and log access/download.
8. Support supplemental response without rewriting the original snapshot.

### Exception and recovery paths

- Missing evidence, orphan person/part reference, confidential HR data, legal hold, package size, or source product unavailable.
- External recipient requires only a limited subset.

### Cross-product and external handoffs

- MaintainArr ↔ RecordArr: package.
- MaintainArr ↔ StaffArr/TrainArr/LoadArr/SupplyArr/AssurArr/Compliance Core: referenced evidence.
- ReportArr: metrics.

### Evidence and audit record

- Scope/snapshot.
- Manifest and source versions.
- Gap/exception decisions.
- Package hash/access.
- Supplemental items.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Package time.
- Missing evidence.
- Reference resolution.
- External question cycle time.



## Workflow definition of done

A workflow is releasable only when its happy path, every listed exception, dependency outage, duplicate/retry, stale/concurrent update, permission denial, cancellation, correction/void, archival/retention, import/API/event path, notification, audit trail, reporting projection, mobile behavior, and professional printable output have automated and human-usable acceptance coverage.
