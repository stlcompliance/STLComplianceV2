# Cross-Product Workflow Implementation Plan

## Workflow 1 — New Employee to Qualified Worker

Milestones: M2, M4, M5, M6, M10, M12

Implementation path:

1. NexArr creates or links platform person identity and tenant membership.
2. StaffArr creates the workforce profile, site, department, position, team, manager, and active state.
3. StaffArr assigns product access intent and permission templates.
4. Compliance Core exposes relevant requirement keys, citation references, and rule context.
5. TrainArr maps role, site, department, position, equipment, material, and task requirements to programs.
6. TrainArr assigns training, captures evidence, tests, practical evaluations, remediation, and signoffs.
7. TrainArr issues qualification and publishes to StaffArr.
8. StaffArr recalculates readiness and exposes readiness API results to products.
9. MaintainArr, RoutArr, and SupplyArr gate assignments using StaffArr readiness and TrainArr qualification checks.
10. StaffArr and TrainArr produce a joint audit package proving the worker was ready or blocked.

Acceptance:

- A person cannot be assigned to safety-critical product work without the expected qualification/readiness signals.
- Denied states show plain reasons.
- Audit snapshots show person, role, requirement, evidence, signoff, rule context, and decision time.

---

## Workflow 2 — Asset to Dispatch-Ready

Milestones: M5, M7, M9, M10, M12

Implementation path:

1. MaintainArr creates and classifies the asset.
2. MaintainArr captures meters, required documents, PM baselines, and inspection requirements.
3. MaintainArr runs inspections, defects, work orders, PM, repair verification, and readiness calculation.
4. Compliance Core evaluates rule-sensitive maintenance facts and evidence requirements.
5. RoutArr requests dispatchability from MaintainArr before equipment assignment.
6. RoutArr stores a point-in-time readiness decision snapshot.
7. If blocked, RoutArr shows readiness reasons and links to MaintainArr source records.

Acceptance:

- RoutArr does not duplicate maintenance logic.
- MaintainArr explains ready, warning, restricted, not ready, out of service, or unknown states.
- Compliance Core facts are referenced where rule-sensitive.

---

## Workflow 3 — Failed Inspection to Work Order

Milestones: M5, M6, M7, M10, M12

Implementation path:

1. MaintainArr inspection runner records answers, timestamps, evidence, signatures, and failed items.
2. Failed answers create defects according to the template version.
3. Critical/safety defects restrict asset readiness.
4. MaintainArr auto-generates work orders when configured.
5. TrainArr checks whether assigned technician/inspector is qualified.
6. SupplyArr receives parts demand where needed.
7. Repairs, labor, parts, notes, photos, meter readings, and verification are recorded.
8. Compliance Core evaluates evidence completeness where required.
9. MaintainArr recalculates asset readiness and publishes events.
10. Audit package includes inspection, defect, WO, repair, evidence, cost snapshot, and readiness change.

Acceptance:

- The chain from failed answer to readiness recovery is provable without spreadsheet hunting.

---

## Workflow 4 — Work Order Parts Demand to SupplyArr

Milestones: M7, M8, M10, M12

Implementation path:

1. MaintainArr creates work-order parts demand lines.
2. SupplyArr checks availability, reservation, approved sources, pricing, and lead time.
3. SupplyArr creates purchase request if stock cannot satisfy demand.
4. StaffArr permission/authority controls approval workflow.
5. SupplyArr creates RFQ, quote comparison, PO, receiving, exception, return, and warranty records as needed.
6. MaintainArr displays status and receives cost/source snapshot when the part is consumed.
7. Compliance Core evaluates supplier/document/purchase evidence where configured.

Acceptance:

- A shop manager can request parts in MaintainArr while SupplyArr remains the source of truth for procurement.

---

## Workflow 5 — Route Assignment with Compliance Gates

Milestones: M4, M5, M6, M7, M9, M10, M12

Implementation path:

1. RoutArr creates route/trip/stop/load records.
2. RoutArr checks StaffArr person active state and permission scope.
3. RoutArr checks TrainArr qualification for driver, route type, equipment, material, and jurisdiction.
4. RoutArr checks MaintainArr asset readiness.
5. RoutArr checks Compliance Core route/load/workflow gates.
6. RoutArr stores the assignment decision snapshot.
7. Driver completes trip, stops, DVIR, proofs, exceptions, messages, and closeout.
8. Incidents and exceptions flow to StaffArr, TrainArr, MaintainArr, SupplyArr, or Compliance Core by ownership.

Acceptance:

- Dispatch cannot silently assign unqualified people or unavailable assets where configured blocking rules apply.

---

## Workflow 6 — Incident to Retraining

Milestones: M4, M5, M6, M9, M10, M12

Implementation path:

1. Product records the incident in the owning domain.
2. StaffArr records involved people and personnel impact.
3. Compliance Core evaluates whether the incident triggers required action.
4. TrainArr assigns remediation or retraining.
5. Completion publishes back to StaffArr.
6. StaffArr recalculates readiness.
7. Owning product receives readiness/qualification changes and applies blocks or warnings.

Acceptance:

- Remediation is traceable from incident cause to training completion to readiness restoration.

---

## Workflow 7 — Cross-Product Audit Package

Milestones: M5, M10, M12, M13

Implementation path:

1. Owning product gathers source records.
2. Related product references are resolved through APIs.
3. Point-in-time snapshots are included where needed.
4. Compliance Core includes rule/evaluation snapshots.
5. Export identifies the source product for every record.
6. Package exports in HTML/PDF/ZIP/JSON as appropriate.

Acceptance:

- An auditor can see what happened, who did it, which product owns the proof, and which rules were evaluated.
