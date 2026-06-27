# AssurArr — QMS Workflow Catalog

[Feature catalog](./FEATURESET.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md) · [Rollout](./ROLLOUT.md)

## Workflow contract

This document defines the end-to-end business state machines for AssurArr. A workflow is not complete because a page or endpoint exists: the trigger, authority, state transitions, validation, handoffs, evidence, exceptions, retries, conflict behavior, notifications, reporting, and final source-of-truth commit must all work together.

## Ownership rule

AssurArr owns quality meaning and decisions: nonconformance, quality hold/release, containment, disposition, root cause, CAPA, corrective actions, verification/effectiveness, quality audits/findings, quality reviews, supplier quality issues/SCAR, customer complaint quality cases, quality risk and scorecards. It blocks or releases product-owned execution through explicit events but does not take ownership of inventory, assets, orders, suppliers, customers, documents, or training.

- Physical inventory balance/movement; LoadArr enforces quality status/hold on inventory.
- Asset readiness/work order; MaintainArr enforces asset quality blockers and executes corrective maintenance.
- Supplier/customer/order/transport truth; SupplyArr, CustomArr, OrdArr, and RoutArr own those records.
- Controlled-document storage/version or training assignment; RecordArr and TrainArr own them, with quality-impact handoffs.
- Regulatory interpretation/applicability; Compliance Core owns it.

## Workflow index

| Workflow ID | Workflow | Class | State | Trigger |
| --- | --- | --- | --- | --- |
| AS-WF-001 | Report, triage, and classify nonconformance | CURRENT · COMMON | Durable | Worker, customer, supplier, audit, inspection, system rule, or product event identifies a quality issue. |
| AS-WF-002 | Quality hold creation, propagation, and release | CURRENT · UNDERSERVED | Durable | A nonconformance, complaint, audit, supplier issue, failed inspection, or authorized reviewer requires a hold. |
| AS-WF-003 | Containment and disposition | CURRENT · COMMON | Durable | Nonconformance triage requires immediate containment or investigation reaches disposition. |
| AS-WF-004 | Root cause analysis and CAPA initiation | CURRENT · COMMON | Durable | A nonconformance/finding/complaint/recurrence meets CAPA criteria. |
| AS-WF-005 | CAPA action execution, verification, effectiveness, and closure | CURRENT · COMMON | Durable | CAPA is approved. |
| AS-WF-006 | Quality audit plan, execute, finding, and close | CURRENT · COMMON | Durable | Audit program/schedule or ad hoc risk event creates an audit. |
| AS-WF-007 | Supplier quality issue and SCAR | CURRENT · COMMON | Durable | Incoming quality, field failure, audit, complaint, or trend triggers supplier action. |
| AS-WF-008 | Customer complaint quality investigation | CURRENT · COMMON | Durable | CustomArr forwards a complaint classified as potential quality issue. |
| AS-WF-009 | Quality review and release | CURRENT · COMMON | Durable | Operational workflow requests quality review or configured criteria are met. |
| AS-WF-010 | Quality change control | COMMON · DEMOCRATIZE | Target | A process, specification, supplier, material, equipment, software, document, method, location, or requirement change is proposed. |
| AS-WF-011 | Deviation and temporary concession | COMMON · UNDERSERVED | Target | Operations requests deviation before work or identifies an unavoidable departure during execution. |
| AS-WF-012 | Risk/FMEA review and control action | COMMON · DEMOCRATIZE | Target | New/changed process/product/supplier/equipment or scheduled review requires risk assessment. |
| AS-WF-013 | Management quality review | CURRENT · COMMON | Partial | Scheduled management review period opens. |
| AS-WF-014 | Quality audit/evidence package | CURRENT · COMMON | Partial | Customer, regulator, auditor, supplier, or internal reviewer requests quality evidence. |

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

## AS-WF-001 — Report, triage, and classify nonconformance

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Capture a quality problem, determine affected scope, and assign immediate response. |
| Trigger | Worker, customer, supplier, audit, inspection, system rule, or product event identifies a quality issue. |

### Actors

- Reporter
- Quality reviewer
- AssurArr

### State path

`reported → triage → contained → investigation → disposition → closed`

### Required sequence

1. Create nonconformance with source product/record, affected object refs, location, time, description, severity clues, and evidence.
2. Deduplicate/relate to existing issues and identify potential broader scope.
3. Evaluate immediate safety/customer/compliance/production impact.
4. Assign owner, classification, priority, response SLA, and confidentiality.
5. Create containment/hold when needed.
6. Notify affected product owners and preserve acknowledgement status.
7. Plan investigation and required evidence.
8. Record triage outcome and customer/supplier/incident handoffs.

### Exception and recovery paths

- Unknown item/lot/asset/order, possible duplicate, immediate danger, potential reportable event, confidential complaint, or affected scope unclear.
- One report may spawn multiple related nonconformances.

### Cross-product and external handoffs

- Origin product → AssurArr.
- AssurArr → LoadArr/MaintainArr/OrdArr/RoutArr/SupplyArr/CustomArr.
- RecordArr: evidence.
- Compliance Core: applicability.

### Evidence and audit record

- Original source/evidence.
- Classification/severity.
- Affected scope.
- Containment/notifications.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to triage.
- Duplicate rate.
- Containment latency.
- Scope expansion.
- Severity changes.

## AS-WF-002 — Quality hold creation, propagation, and release

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Durable |
| Purpose | Block affected use/movement/dispatch/closeout and safely release the correct scope. |
| Trigger | A nonconformance, complaint, audit, supplier issue, failed inspection, or authorized reviewer requires a hold. |

### Actors

- Quality reviewer
- Approver/releaser
- Affected product owners
- AssurArr

### State path

`draft → active → propagating → investigating → release_review → released → rejected → closed`

### Required sequence

1. Define hold scope by item/lot/serial/location/asset/order/trip/customer/supplier/record and prohibited actions.
2. Record reason, severity, owner, evidence, effective time, and release criteria.
3. Emit versioned hold event and require acknowledgements from affected products.
4. Monitor unresolved references and leakage attempts.
5. Conduct containment/investigation/testing/disposition.
6. Prepare release/reject/partial-release decision with evidence and authority.
7. Emit release scoped to exact affected refs/version; downstream products verify before clearing local blockers.
8. Confirm acknowledgements and close residual exceptions.

### Exception and recovery paths

- Scope ambiguous/expands, event arrives out of order, inventory moved before acknowledgement, multiple overlapping holds, emergency override request, or partial release.
- AssurArr cannot directly edit LoadArr balance or MaintainArr readiness.

### Cross-product and external handoffs

- AssurArr ↔ all affected products.
- AssurArr ↔ RecordArr/Compliance Core.
- ReportArr: hold aging/propagation.

### Evidence and audit record

- Hold scope/version.
- Acknowledgements/leakage.
- Investigation/evidence.
- Release authority/scope.
- Downstream unblock confirmations.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Propagation latency.
- Unacknowledged holds.
- Hold aging.
- Leakage prevented.
- Release-to-unblock time.

## AS-WF-003 — Containment and disposition

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Control affected material/service and decide its approved outcome. |
| Trigger | Nonconformance triage requires immediate containment or investigation reaches disposition. |

### Actors

- Quality owner
- Operations owner
- Approver
- AssurArr

### State path

`planned → in_progress → verified → disposition_review → approved → executing → complete`

### Required sequence

1. Define immediate containment actions, affected quantities/objects, locations, owner, and verification.
2. Execute segregate/stop-ship/inspect/sort/rework/customer notification/field action through owning products.
3. Verify containment effectiveness and update affected scope.
4. Collect investigation/test/customer/supplier/cost/feasibility context.
5. Select disposition: use-as-is, rework, repair, return, replace, scrap, downgrade, release, or other controlled outcome.
6. Obtain required engineering/customer/regulatory/financial approval.
7. Issue product-owned work/movement/order actions and monitor completion.
8. Verify final quantities/objects and close disposition.

### Exception and recovery paths

- Containment incomplete, affected units already shipped/used, disposition conflicts with customer/regulation, rework fails, approver conflict, or quantity does not reconcile.
- Use-as-is requires explicit risk/authority.

### Cross-product and external handoffs

- AssurArr → LoadArr/MaintainArr/OrdArr/RoutArr/SupplyArr/CustomArr.
- RecordArr: evidence.
- LedgArr: cost/financial refs.

### Evidence and audit record

- Containment plan/results.
- Scope/quantity reconciliation.
- Disposition analysis/approvals.
- Execution and verification.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Containment time.
- Escaped units.
- Disposition cycle.
- Rework success.
- Quantity reconciliation.

## AS-WF-004 — Root cause analysis and CAPA initiation

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Determine verified causes and open proportionate corrective/preventive action. |
| Trigger | A nonconformance/finding/complaint/recurrence meets CAPA criteria. |

### Actors

- Quality investigator
- Process owner
- Subject-matter experts
- AssurArr

### State path

`investigation → hypotheses → cause_review → capa_proposed → approved → monitoring`

### Required sequence

1. Define problem statement, scope, impact, known facts, and investigation plan.
2. Collect process, people qualification, equipment, material, supplier, environment, method, measurement, document, and event evidence.
3. Develop hypotheses using approved methods and record supporting/refuting evidence.
4. Identify verified root/contributing causes and uncertainties.
5. Assess systemic/recurrence risk and related records/population.
6. Decide whether correction only, CAPA, broader change, or monitoring is warranted.
7. Create CAPA with objective, actions, owners, due dates, dependencies, verification/effectiveness plan.
8. Approve initiation and communicate affected owners.

### Exception and recovery paths

- Evidence insufficient, multiple plausible causes, source records missing, investigation bias/conflict, urgent action precedes final cause, or no root cause confirmed.
- Do not fabricate a single root cause merely to close the record.

### Cross-product and external handoffs

- AssurArr ↔ operational products/StaffArr/TrainArr/RecordArr/Compliance Core.
- AssurArr → ReportArr: cause trends.

### Evidence and audit record

- Problem/evidence sources.
- Hypotheses/tests.
- Cause decision/uncertainty.
- CAPA criteria/approval.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Investigation time.
- Evidence completeness.
- Cause recurrence.
- CAPA initiation appropriateness.

## AS-WF-005 — CAPA action execution, verification, effectiveness, and closure

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Implement corrective actions and prove they worked before closing. |
| Trigger | CAPA is approved. |

### Actors

- CAPA owner
- Action owners
- Verifier
- Approver
- AssurArr

### State path

`open → actions_in_progress → blocked → verification → effectiveness → effective → ineffective → closed → reopened`

### Required sequence

1. Break CAPA into corrections, corrective/preventive actions, document/training/process/system/supplier/equipment changes, and dependencies.
2. Assign owners, due dates, evidence, approval, and blocker paths.
3. Execute actions through owning products and link results rather than duplicating records.
4. Review action completion evidence and resolve blockers/overdue escalation.
5. Execute verification plan to confirm implementation.
6. Wait an appropriate observation window and measure effectiveness criteria.
7. Decide effective, partially effective, ineffective/reopen, or extended monitoring.
8. Approve closure only when residual risk and related records are addressed.

### Exception and recovery paths

- Action owner leaves, change delayed, training incomplete, effectiveness data unavailable, recurrence occurs, scope expands, or verifier is not independent.
- Deadline extension requires impact/risk and approval.

### Cross-product and external handoffs

- AssurArr ↔ RecordArr/TrainArr/MaintainArr/SupplyArr/StaffArr/Compliance Core.
- ReportArr: action/effectiveness metrics.

### Evidence and audit record

- CAPA/action versions.
- Evidence/blockers/extensions.
- Verification results.
- Effectiveness data/decision.
- Closure/reopen.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- On-time action closure.
- Overdue aging.
- Effectiveness pass.
- Reopen/recurrence.
- Extension rate.

## AS-WF-006 — Quality audit plan, execute, finding, and close

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Run internal, supplier, process, product, or system audits with traceable evidence and follow-up. |
| Trigger | Audit program/schedule or ad hoc risk event creates an audit. |

### Actors

- Audit manager
- Auditor
- Auditee/owner
- Approver
- AssurArr

### State path

`planned → scheduled → fieldwork → report_review → findings_open → follow_up → closed`

### Required sequence

1. Define audit type, scope, criteria, objectives, team/independence, schedule, sites/processes, and checklist version.
2. Notify auditees and request documents/evidence.
3. Execute interviews/observations/sampling and link evidence.
4. Record conforming evidence, observations, opportunities, and findings with severity/criteria.
5. Review/approve report and communicate findings.
6. Auditee submits correction/cause/action plan; route CAPA if warranted.
7. Verify actions and close findings/audit.
8. Generate audit package and feed management review/risk.

### Exception and recovery paths

- Auditor conflict, scope change, unavailable evidence, remote/site access issue, disagreement, confidential evidence, or critical finding requiring immediate hold.
- External audit response may need controlled customer/regulator portal.

### Cross-product and external handoffs

- AssurArr ↔ RecordArr/Compliance Core/operational products.
- AssurArr → TrainArr/SupplyArr etc. for actions.
- ReportArr: audit metrics.

### Evidence and audit record

- Plan/criteria/team.
- Checklist/evidence.
- Findings/report.
- Responses/actions/verification.
- Closure/package.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Audit plan completion.
- Finding aging.
- Repeat findings.
- Action effectiveness.
- Evidence completeness.

## AS-WF-007 — Supplier quality issue and SCAR

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Obtain and verify supplier containment/corrective action while coordinating commercial consequences. |
| Trigger | Incoming quality, field failure, audit, complaint, or trend triggers supplier action. |

### Actors

- Supplier quality engineer
- Buyer/supplier owner
- Supplier contact
- AssurArr
- SupplyArr

### State path

`opened → containment → response_due → review → implementation → verification → closed → reopened`

### Required sequence

1. Create supplier quality issue with supplier/item/lot/PO/receipt/affected records and evidence.
2. Apply containment/hold and immediate supplier notification.
3. Define SCAR response requirements, deadlines, problem-solving method, and portal scope.
4. Supplier submits containment, root cause, correction, corrective action, and evidence.
5. AssurArr reviews adequacy and requests revision or approves implementation.
6. Verify effectiveness using subsequent receipts/tests/performance.
7. SupplyArr manages restriction/source/commercial consequences based on AssurArr status.
8. Close/reopen and update supplier quality scorecard.

### Exception and recovery paths

- Supplier nonresponsive, responsibility disputed, sub-tier cause, affected goods shipped, response confidential, corrective action fails, or supplier remains sole source.
- Commercial exception may permit limited controlled buys.

### Cross-product and external handoffs

- AssurArr ↔ SupplyArr/supplier portal/LoadArr/RecordArr.
- ReportArr: supplier quality.

### Evidence and audit record

- Affected scope/evidence.
- SCAR request/access.
- Supplier response/revisions.
- Verification/effectiveness.
- Closure/status.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Response time.
- Containment time.
- First-pass response approval.
- Recurrence.
- Supplier restriction duration.

## AS-WF-008 — Customer complaint quality investigation

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Investigate complaint quality while CustomArr owns customer communication and relationship. |
| Trigger | CustomArr forwards a complaint classified as potential quality issue. |

### Actors

- Customer service owner
- Quality investigator
- Operations owner
- AssurArr

### State path

`received → triage → investigation → action → response_ready → closed`

### Required sequence

1. Receive complaint ref, affected customer/order/product/service/lot/asset, severity clues, and evidence.
2. Triage safety/reportability/recall/containment needs and notify CustomArr of approved status.
3. Trace affected scope and related complaints/nonconformances.
4. Collect return/inspection/test/service/supplier/transport evidence.
5. Determine cause, disposition, CAPA, and reportability recommendations with Compliance Core where configured.
6. Coordinate customer remedy through CustomArr/OrdArr and operational products.
7. Provide customer-safe findings/status approved for release.
8. Close quality case and feed trend/management review.

### Exception and recovery paths

- Potential injury/reportable event, no sample/lot, customer refuses return, multiple affected customers, legal claim, or investigation cannot confirm cause.
- Internal cause details may not be releasable.

### Cross-product and external handoffs

- CustomArr ↔ AssurArr.
- AssurArr ↔ LoadArr/MaintainArr/SupplyArr/RoutArr/OrdArr/RecordArr/Compliance Core.

### Evidence and audit record

- Complaint/source/evidence.
- Triage/containment.
- Investigation/cause.
- Actions/customer-safe response.
- Closure.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Triage time.
- Investigation cycle.
- Customer update latency.
- Repeat complaint.
- No-fault/unknown rate.

## AS-WF-009 — Quality review and release

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Review a lot, service, asset, order, process, or package and issue an explicit quality disposition/release. |
| Trigger | Operational workflow requests quality review or configured criteria are met. |

### Actors

- Quality reviewer
- Approver
- AssurArr

### State path

`requested → evidence_review → blocked → approved → conditional → rejected → superseded`

### Required sequence

1. Resolve target object/version and required inspections/tests/documents/approvals/holds/deviations.
2. Collect results and verify source, method, equipment/calibration refs, person qualification, and evidence completeness.
3. Evaluate specification/customer/regulatory criteria and active nonconformance/CAPA context.
4. Request missing evidence or create issue/hold.
5. Decide release, conditional release, reject, rework, or escalation with reason and residual conditions.
6. Capture required signature/approval and immutable release version.
7. Notify owning products and confirm acknowledgement.
8. Monitor conditions/expiry and revoke superseded release only through new decision.

### Exception and recovery paths

- Conflicting test, missing evidence, out-of-tolerance equipment, person unqualified, customer waiver pending, active hold, or object changed after review.
- Conditional release has time/quantity/customer limits.

### Cross-product and external handoffs

- AssurArr ↔ owning product/RecordArr/Compliance Core/TrainArr/MaintainArr.
- ReportArr: release metrics.

### Evidence and audit record

- Target/version.
- Criteria/source evidence.
- Decision/conditions/signatures.
- Acknowledgements.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Review cycle.
- First-pass release.
- Missing evidence.
- Conditional release.
- Downstream acknowledgement.

## AS-WF-010 — Quality change control

| Field | Definition |
| --- | --- |
| Classification | COMMON · DEMOCRATIZE |
| Implementation state | Target |
| Purpose | Evaluate and implement changes without unintended quality, document, training, operational, or compliance impact. |
| Trigger | A process, specification, supplier, material, equipment, software, document, method, location, or requirement change is proposed. |

### Actors

- Change owner
- Quality reviewer
- Affected product owners
- Approvers
- AssurArr

### State path

`proposed → impact_assessment → approval → implementation → verification → effective → rolled_back → closed`

### Required sequence

1. Create change proposal with current/proposed state, reason, scope, urgency, and source.
2. Identify affected products/processes/items/assets/suppliers/customers/orders/docs/training/rules/data/integrations and open work.
3. Assess quality, compliance, validation, safety, continuity, financial, and rollback risk.
4. Define implementation, testing/validation, document/training, communication, effective date, and transition plan.
5. Obtain cross-functional approvals and customer/regulatory approval if required.
6. Execute versioned changes through owning products and track acknowledgements.
7. Verify implementation/effectiveness and resolve deviations.
8. Approve effective/close, rollback, or reopen.

### Exception and recovery paths

- Emergency change, incomplete impact map, conflicting approvers, validation fails, training incomplete, old inventory remains, or customer rejects change.
- Temporary change has explicit expiry and conversion/rollback.

### Cross-product and external handoffs

- AssurArr ↔ all affected products/RecordArr/TrainArr/Compliance Core.
- ReportArr: change performance.

### Evidence and audit record

- Proposal/diff.
- Impact/risk/approvals.
- Implementation acknowledgements.
- Verification/effective/rollback.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Change cycle.
- Implementation on time.
- Post-change issue rate.
- Incomplete training/docs.
- Rollback rate.

## AS-WF-011 — Deviation and temporary concession

| Field | Definition |
| --- | --- |
| Classification | COMMON · UNDERSERVED |
| Implementation state | Target |
| Purpose | Authorize a bounded departure from an approved process/specification with risk and expiry. |
| Trigger | Operations requests deviation before work or identifies an unavoidable departure during execution. |

### Actors

- Requester
- Quality reviewer
- Technical/customer/compliance approver
- AssurArr

### State path

`requested → assessment → approved → active → expired → rejected → closed`

### Required sequence

1. Capture requirement/process/spec, proposed deviation, affected quantity/time/orders/customers, reason, and alternatives.
2. Assess safety, quality, compliance, customer, validation, traceability, and recurrence risk.
3. Define controls, inspections, labels, evidence, approvals, and expiration/quantity limits.
4. Approve, reject, or request additional data; obtain customer/regulator approval where needed.
5. Publish scoped concession to affected products and prevent use outside scope.
6. Track execution and required enhanced inspection/evidence.
7. Review outcome and disposition affected items/work.
8. Close at expiry/quantity and decide permanent change/CAPA.

### Exception and recovery paths

- Deviation is illegal/unsafe, scope expands, approver unavailable, customer refuses, execution exceeds quantity/date, or enhanced inspection fails.
- Emergency use requires immediate containment and post-review.

### Cross-product and external handoffs

- AssurArr ↔ Compliance Core/CustomArr/LoadArr/MaintainArr/OrdArr/RecordArr.
- ReportArr: deviations.

### Evidence and audit record

- Request/scope.
- Risk/approvals.
- Published controls.
- Execution/evidence.
- Expiry/outcome.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Approval time.
- Expired-active prevention.
- Deviation recurrence.
- Failed concession.
- Conversion to change/CAPA.

## AS-WF-012 — Risk/FMEA review and control action

| Field | Definition |
| --- | --- |
| Classification | COMMON · DEMOCRATIZE |
| Implementation state | Target |
| Purpose | Identify quality risks and connect controls to actual operations and evidence. |
| Trigger | New/changed process/product/supplier/equipment or scheduled review requires risk assessment. |

### Actors

- Process owner
- Quality/reliability team
- AssurArr

### State path

`draft → workshop → review → actions_open → accepted → monitoring → superseded`

### Required sequence

1. Define scope, function/requirement, process steps, products/items/assets, and assumptions.
2. Identify failure modes/hazards, effects, causes, existing preventive/detective controls, and evidence.
3. Score or otherwise prioritize severity/occurrence/detection using approved method and uncertainty.
4. Link incidents/nonconformances/complaints/audits/measurements to validate assumptions.
5. Create actions for control improvement, detection, design/process/supplier/training/document changes.
6. Approve residual risk and accountable owner.
7. Monitor indicators and trigger reassessment on events/change.
8. Version and retain prior assessments.

### Exception and recovery paths

- Data sparse, scoring disagreement, method changes, high risk lacks owner, action overdue, or operational evidence contradicts assumptions.
- Numeric score never substitutes for severity judgment.

### Cross-product and external handoffs

- AssurArr ↔ MaintainArr/SupplyArr/TrainArr/Compliance Core/ReportArr/RecordArr.

### Evidence and audit record

- Scope/method/version.
- Failure modes/controls/evidence.
- Scores/uncertainty.
- Actions/residual acceptance.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- High-risk action closure.
- Risk reduction.
- Prediction vs events.
- Review timeliness.

## AS-WF-013 — Management quality review

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Review quality-system performance, resources, risks, and actions with evidence and accountability. |
| Trigger | Scheduled management review period opens. |

### Actors

- Quality leader
- Executives/process owners
- AssurArr

### State path

`planned → inputs_collecting → meeting → actions_open → approved → closed`

### Required sequence

1. Assemble prior actions, objectives, metrics, nonconformance/CAPA, complaints, audits, supplier quality, holds, risk, changes, training/document, compliance, and resource inputs.
2. Validate metric definitions, source freshness, and unresolved data quality.
3. Prepare trends, significant events, systemic issues, and decisions needed.
4. Conduct review with attendees, quorum, agenda, discussion, and decisions.
5. Create actions, owners, due dates, resource/priority decisions, and escalation.
6. Approve minutes/package with signatures.
7. Track actions to closure and feed next review.
8. Restrict confidential/legal content appropriately.

### Exception and recovery paths

- Missing data, absent decision maker, disputed metric, urgent issue discovered, action owner unavailable, or prior actions overdue.
- Review may cover multiple sites/entities with local appendices.

### Cross-product and external handoffs

- AssurArr ↔ ReportArr/RecordArr/all product metrics.
- AssurArr → StaffArr/TrainArr/etc.: actions.

### Evidence and audit record

- Agenda/input snapshot.
- Metrics/source.
- Attendance/discussion/decisions.
- Actions/signatures/package.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Review on time.
- Action closure.
- Repeat systemic issues.
- Data completeness.
- Decision follow-through.

## AS-WF-014 — Quality audit/evidence package

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Create a scoped, immutable package of quality decisions and supporting source evidence, with review-detail visibility into required versus submitted RecordArr evidence. |
| Trigger | Customer, regulator, auditor, supplier, or internal reviewer requests quality evidence. |

### Actors

- Quality administrator
- Auditor/recipient
- AssurArr
- RecordArr

### State path

`requested → collecting → validation → assembled → approved → shared → closed`

### Required sequence

1. Define scope by nonconformance/CAPA/audit/supplier/customer/lot/asset/order/period and recipient authority.
2. Collect quality records, timelines, holds/releases, actions, verification/effectiveness, signatures, metrics, and affected-object refs.
3. Resolve source evidence from RecordArr and operational products.
4. Validate missing refs, confidentiality, legal hold, retention, and redaction.
5. Generate manifest and package snapshot.
6. Review/approve and share through scoped expiring access.
7. Log access/questions/supplemental responses.
8. Preserve original package and close request.

### Exception and recovery paths

- Missing evidence, confidential customer/supplier data, legal privilege, external recipient lacks authority, package too large, or source record corrected after snapshot.
- Regulator/customer package may require approved narrative.

### Cross-product and external handoffs

- AssurArr ↔ RecordArr/all source products/NexArr.
- ReportArr: package metrics.

### Evidence and audit record

- Scope/authority.
- Manifest/source versions.
- Redactions/gaps.
- Package/access/supplements.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Generation time.
- Missing evidence.
- External question cycle.
- Unauthorized access.
- Supplement rate.



## Workflow definition of done

A workflow is releasable only when its happy path, every listed exception, dependency outage, duplicate/retry, stale/concurrent update, permission denial, cancellation, correction/void, archival/retention, import/API/event path, notification, audit trail, reporting projection, mobile behavior, and professional printable output have automated and human-usable acceptance coverage.
