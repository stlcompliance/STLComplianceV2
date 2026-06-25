# Compliance Core — GRC Workflow Catalog

[Feature catalog](./FEATURESET.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md)

## Workflow contract

This document defines the end-to-end business state machines for Compliance Core. A workflow is not complete because a page or endpoint exists: the trigger, authority, state transitions, validation, handoffs, evidence, exceptions, retries, conflict behavior, notifications, reporting, and final source-of-truth commit must all work together.

## Ownership rule

Own the normalized regulatory knowledge, applicability, fact, requirement, evidence, evaluation, risk, finding, exception, and explainability models that convert legal and policy obligations into auditable operational decisions. Compliance Core is the suite rules and GRC engine; it does not take ownership of operational records from other products.

- People, sites, assets, work orders, training records, shipments, inventory, suppliers, customers, orders, finance transactions, quality records, or documents owned by other products.
- Tenant operational actions; it advises, gates, opens findings/tasks, and records responses while the owning product commits domain changes.
- Legal advice or an assurance that a tenant is compliant merely because configured rules pass.
- Fixed-suite access. All tenant products may call Compliance Core runtime services under permission and service policy.
- Governing bodies as LedgArr legal entities; regulatory authorities and business legal entities remain distinct concepts.

## Workflow index

| Workflow ID | Workflow | Class | State | Trigger |
| --- | --- | --- | --- | --- |
| CC-WF-001 | Ingest and govern a regulatory source | CURRENT · COMMON | Durable | An authorized administrator imports or registers a regulation, standard, guidance document, customer requirement, or policy source. |
| CC-WF-002 | Author, test, publish, and roll back a rulepack | CURRENT · FOUNDATION | Durable | A new obligation, mapping change, defect, or planned content release requires a rulepack revision. |
| CC-WF-003 | Onboard a tenant compliance profile | CURRENT · UNDERSERVED | Durable | A tenant activates the suite, adds a legal entity/site/operation, or requests profile reassessment. |
| CC-WF-004 | Synchronize a product fact with provenance | CURRENT · FOUNDATION | Durable | An owning product emits a create/update/delete/status event or Compliance Core requests a permitted fact refresh. |
| CC-WF-005 | Map and reuse evidence across requirements | CURRENT · UNDERSERVED | Durable | A rule, audit, assessment, or product workflow identifies an evidence requirement. |
| CC-WF-006 | Evaluate compliance and explain the result | CURRENT · FOUNDATION | Durable | A product requests a decision, a fact/evidence/rule changes, a schedule runs, or an authorized user performs an evaluation. |
| CC-WF-007 | Request and enforce an operational workflow gate | CURRENT · UNDERSERVED | Durable | An owning product reaches a configured decision point such as dispatch, assignment, release, receipt, shipment, approval, payment, or return to service. |
| CC-WF-008 | Manage a finding through verified closure | CURRENT · COMMON | Durable | Evaluation, audit, assessment, control test, incident, complaint, external notice, or reviewer opens a finding. |
| CC-WF-009 | Request, approve, monitor, and expire an exception | CURRENT · COMMON | Durable | A user cannot meet a requirement or a rule explicitly permits an exception/exemption/waiver. |
| CC-WF-010 | Assess a regulatory change and drive adoption | CURRENT · DEMOCRATIZE | Durable | A monitored source publishes, amends, delays, interprets, or rescinds a requirement. |
| CC-WF-011 | Plan and execute a control test or assessment | COMMON · DEMOCRATIZE | Target | An assessment calendar, audit plan, change, incident, risk threshold, or continuous-monitoring signal triggers testing. |
| CC-WF-012 | Score risk and choose treatment | CURRENT · COMMON | Durable | A risk is identified or reassessment is due after change, incident, finding, control result, or schedule. |
| CC-WF-013 | Forecast compliance readiness | CURRENT · DEMOCRATIZE | Durable | Scheduled analytics or relevant product/rule/evidence change updates the forecast horizon. |
| CC-WF-014 | Evaluate a theoretical situation before change | CURRENT · UNDERSERVED · DEMOCRATIZE | Durable | A user proposes a new site, operation, asset, material, role, route, supplier, customer service, order flow, document, or financial structure. |
| CC-WF-015 | Manage SDS and hazard communication applicability | CURRENT · COMMON | Durable | A material/product is introduced or changed, an SDS is uploaded/updated/expired, or a location/activity inventory changes. |
| CC-WF-016 | Build and deliver an audit package | CURRENT · COMMON | Durable | An internal/external audit, regulator request, customer assurance review, certification, or management review requires delivery. |
| CC-WF-017 | Handle a Compliance Core service degradation | FOUNDATION | Target | Health monitoring, timeout, content defect, dependency outage, or repeated evaluation failure detects degradation. |

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

## CC-WF-001 — Ingest and govern a regulatory source

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Convert an authoritative source into versioned, reviewable, cited compliance content without losing provenance. |
| Trigger | An authorized administrator imports or registers a regulation, standard, guidance document, customer requirement, or policy source. |

### Actors

- Compliance content administrator
- Legal/compliance reviewer
- Compliance Core

### State path

`registered → ingesting → staged → review → draft → approved → published → superseded → rejected`

### Required sequence

1. Register authority, jurisdiction, program, source type, publication/effective dates, canonical URI or RecordArr reference, checksum, and supersession relationship.
2. Ingest or stage source content and preserve immutable source/version provenance.
3. Segment content into citations/sections and propose vocabulary, keys, requirements, facts, evidence, exceptions, and mappings.
4. Validate identifiers, dates, references, duplicates, gaps, unsupported extraction, and jurisdiction.
5. Route reviewer decisions with comments and source-side comparison.
6. Approve staged content into a draft content version; rejected proposals remain traceable.
7. Run dependency, test, and impact analysis before publication.
8. Publish only through separated approval authority and retain the prior active version.

### Exception and recovery paths

- Source authenticity cannot be established, citation text differs from authoritative source, effective date is unknown, extraction confidence is low, duplicate/superseding sources conflict, or reviewer lacks publication authority.
- AI-extracted content remains a proposal until human review.

### Cross-product and external handoffs

- RecordArr → Compliance Core: governed source file/reference.
- Compliance Core → NexArr: privileged action/audit.
- Compliance Core → affected owners: change-impact notice.

### Evidence and audit record

- Authority/source metadata and checksum.
- Extracted/staged records and confidence.
- Review decisions and comments.
- Version approval/publication/supersession.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Source processing time.
- Extraction acceptance rate.
- Unresolved citations.
- Publication defects.
- Time from publication to governed rule availability.

## CC-WF-002 — Author, test, publish, and roll back a rulepack

| Field | Definition |
| --- | --- |
| Classification | CURRENT · FOUNDATION |
| Implementation state | Durable |
| Purpose | Turn reviewed requirements into a versioned executable rulepack with reproducible tests and safe release controls. |
| Trigger | A new obligation, mapping change, defect, or planned content release requires a rulepack revision. |

### Actors

- Rule author
- Compliance reviewer
- Publisher
- Compliance Core

### State path

`draft → validating → testing → review → approved → scheduled → published → rolled_back → retired`

### Required sequence

1. Create a draft from an active version and define scope, jurisdiction, effective period, dependencies, citations, requirements, facts, evidence, gates, severities, and product responses.
2. Build expressions/decision logic only from registered keys/operators and explicit unknown/conflict behavior.
3. Create positive, negative, boundary, unknown, conflict, exception, and regression test scenarios.
4. Run static validation for cycles, unreachable branches, missing citations, orphan requirements, incompatible types, and nondeterminism.
5. Execute tests against fixtures and representative tenant/product fact snapshots in noncommitting simulation.
6. Generate semantic diff and impact report covering facts, controls, evidence, products, tenants, and historical outcomes.
7. Require author/reviewer/publisher separation where configured, then schedule or publish atomically.
8. Monitor evaluation outcomes and roll back to the prior signed version if release criteria fail.

### Exception and recovery paths

- Tests fail, required fact has no collection path, effective dates overlap, expression is not explainable, dependency version is unavailable, publisher is also prohibited author, or rollback would invalidate a legal retention requirement.
- Past evaluations retain the exact rulepack version used.

### Cross-product and external handoffs

- Compliance Core → all products: versioned runtime decisions.
- Compliance Core → ReportArr: publication and outcome facts.
- Compliance Core → RecordArr: release evidence/package.

### Evidence and audit record

- Rule source/citations/version.
- Semantic diff and impact.
- Tests/fixtures/results.
- Approvals/publication/rollback reason.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Lead time to publish.
- Test coverage.
- Post-release finding delta.
- Rollback rate.
- Unexplained/unknown decision rate.

## CC-WF-003 — Onboard a tenant compliance profile

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Durable |
| Purpose | Establish an explainable starting compliance profile from plain-language answers and existing suite data. |
| Trigger | A tenant activates the suite, adds a legal entity/site/operation, or requests profile reassessment. |

### Actors

- Tenant compliance owner
- Operational owners
- Compliance Core
- Source products

### State path

`started → collecting → conflict_review → evaluating → profile_ready → reassessment_due`

### Required sequence

1. Load noncommercial tenant access context and obtain permitted legal entity, site, workforce, asset, material, transport, supplier, customer, document, and operation facts from owning products.
2. Present only unresolved high-value plain-language questions, grouped by business activity and consequence.
3. Save answers as attributable facts with source, confidence, scope, effective date, review state, and reusable default behavior.
4. Detect conflicts between answers and source-product facts and require review rather than silently overwriting.
5. Run applicability across candidate jurisdictions/programs/rulepacks and distinguish likely, confirmed, not applicable, and unknown.
6. Explain why each area is suggested and list missing facts/evidence.
7. Generate a prioritized setup checklist routed to owning products and responsible roles.
8. Reevaluate when operations or rules materially change.

### Exception and recovery paths

- Tenant cannot answer, data access is denied, multiple legal entities differ, inferred facts are weak, jurisdiction is ambiguous, or prior answers are stale.
- A questionnaire result is not legal advice or a final determination without review.

### Cross-product and external handoffs

- StaffArr/MaintainArr/RoutArr/LoadArr/SupplyArr/CustomArr/OrdArr/LedgArr/RecordArr → Compliance Core: facts.
- Compliance Core → owning products: setup tasks/evidence needs.
- ReportArr: profile/readiness metrics.

### Evidence and audit record

- Question/version/answer/actor.
- Imported facts and conflicts.
- Applicability trace.
- Checklist and reassessment triggers.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Completion rate.
- Questions avoided through reuse.
- Unknown/conflict closure.
- Time to actionable profile.
- Subsequent applicability corrections.

## CC-WF-004 — Synchronize a product fact with provenance

| Field | Definition |
| --- | --- |
| Classification | CURRENT · FOUNDATION |
| Implementation state | Durable |
| Purpose | Maintain compliance-relevant facts without duplicating operational ownership or trusting unverified payloads. |
| Trigger | An owning product emits a create/update/delete/status event or Compliance Core requests a permitted fact refresh. |

### Actors

- Owning product
- Compliance Core sync service
- Compliance reviewer

### State path

`received → validated → mapped → active → conflicted → superseded → rejected`

### Required sequence

1. Authenticate service/tenant/product and validate event envelope, schema version, aggregate ID, sequence, event time, and idempotency key.
2. Map approved source fields/events to canonical fact definitions and subject references.
3. Validate type, unit, vocabulary, effective time, confidence, deletion/correction semantics, and source authority.
4. Create a new fact/source version rather than overwriting historical provenance.
5. Detect disagreement with other sources and mark conflict/uncertainty and affected evaluations.
6. Reevaluate only impacted rules/subjects using the correct effective-time/version context.
7. Emit decisions/findings/tasks only when configured thresholds and suppression rules are met.
8. Reconcile cursors and allow signed replay/backfill without duplicate effects.

### Exception and recovery paths

- Unknown schema/key, out-of-order event, tenant mismatch, source lacks authority, unit conversion fails, source record was merged, or multiple sources disagree.
- No cross-product database foreign keys are created.

### Cross-product and external handoffs

- Product → Compliance Core: event/API fact.
- Compliance Core → product: missing-fact/query/response.
- Compliance Core → ReportArr: evaluation outcome.

### Evidence and audit record

- Event/source identity.
- Mapping/version/transformation.
- Fact value/effective/confidence.
- Conflict/evaluation/response.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Sync success.
- Lag.
- Conflict rate.
- Rejected events.
- Reevaluation scope and latency.

## CC-WF-005 — Map and reuse evidence across requirements

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Durable |
| Purpose | Satisfy multiple obligations from governed source evidence without duplicating files or losing scope/freshness. |
| Trigger | A rule, audit, assessment, or product workflow identifies an evidence requirement. |

### Actors

- Control/evidence owner
- Auditor/reviewer
- Compliance Core
- RecordArr
- Owning products

### State path

`needed → candidate → review → accepted → partial → rejected → expiring → expired → revoked`

### Required sequence

1. Resolve requirement scope, subject, period, freshness, evidence type, issuer, integrity, and acceptance criteria.
2. Search permitted product records and RecordArr metadata before requesting a new upload.
3. Propose mappings between source evidence and one or more requirements, controls, rulepacks, or assessments.
4. Validate provenance, subject/scope, dates, signatures/approvals, completeness, retention, and legal-hold state.
5. Approve/reject mapping and record reviewer rationale; do not mutate the underlying source record.
6. Track expiration, supersession, revocation, source deletion/merge, and affected decisions.
7. Notify owners before evidence becomes stale and request replacement through the owning workflow.
8. Preserve point-in-time mappings for completed audits/evaluations.

### Exception and recovery paths

- Evidence covers only part of scope, metadata is insufficient, a file changed after approval, source access is revoked, document is under hold, or the same evidence conflicts with another record.
- Evidence presence alone does not prove control effectiveness.

### Cross-product and external handoffs

- RecordArr/other products ↔ Compliance Core.
- Compliance Core → owner inbox/task.
- ReportArr: evidence health.

### Evidence and audit record

- Requirement and acceptance criteria.
- Candidate source/version/hash.
- Mapping review/rationale.
- Expiry/revocation/affected evaluations.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Evidence reuse ratio.
- Time to fulfill.
- Stale evidence.
- Rejected mapping causes.
- Duplicate-upload reduction.

## CC-WF-006 — Evaluate compliance and explain the result

| Field | Definition |
| --- | --- |
| Classification | CURRENT · FOUNDATION |
| Implementation state | Durable |
| Purpose | Produce a deterministic, versioned, human-readable decision from scoped facts, rules, evidence, and exceptions. |
| Trigger | A product requests a decision, a fact/evidence/rule changes, a schedule runs, or an authorized user performs an evaluation. |

### Actors

- Compliance Core evaluation engine
- Requesting product/user
- Compliance reviewer

### State path

`requested → resolving → evaluating → pass → fail → warning → unknown → not_applicable → error`

### Required sequence

1. Validate tenant, subject, purpose, evaluation time, requested rulepack/version, and caller authorization.
2. Resolve applicable rulepacks from jurisdiction, operation, subject, and explicit profile facts.
3. Assemble effective-time facts and evidence with provenance, confidence, conflicts, unknowns, and approved exceptions.
4. Execute versioned rules deterministically and record every evaluated condition, input, mapping, short-circuit, and outcome.
5. Classify results as pass, fail, warning, unknown, not applicable, or error; do not collapse unknown into pass.
6. Generate plain-language explanation, citations, missing facts/evidence, affected controls, severity, and permitted next actions.
7. Create/suppress findings, gates, alerts, or reassessment tasks according to policy.
8. Return a signed/correlated result and persist the immutable evaluation trace.

### Exception and recovery paths

- Rule version unavailable, required source fact is stale/conflicted, evaluation times out, exception scope is ambiguous, caller requests prohibited subject data, or content defect is detected.
- An evaluation is evidence of system behavior, not a blanket legal certification.

### Cross-product and external handoffs

- Requesting product ↔ Compliance Core.
- Compliance Core → ReportArr/RecordArr/findings.
- NexArr: caller authorization/audit.

### Evidence and audit record

- Request context/version.
- Inputs/provenance.
- Trace/citations/outcome.
- Findings/gates/response.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Evaluation latency.
- Unknown/conflict rate.
- Explanation access.
- Finding precision.
- Reevaluation volume.

## CC-WF-007 — Request and enforce an operational workflow gate

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Durable |
| Purpose | Prevent or condition a consequential product action using a transparent compliance decision and controlled override path. |
| Trigger | An owning product reaches a configured decision point such as dispatch, assignment, release, receipt, shipment, approval, payment, or return to service. |

### Actors

- Owning product user
- Compliance Core
- Authorized approver

### State path

`requested → allow → conditional → warn → block → unknown → overridden → committed → abandoned → expired`

### Required sequence

1. Product sends tenant, actor, action, subject references, contextual facts, intended effective time, and correlation/idempotency key.
2. Compliance Core validates permitted gate type and resolves applicable rules, facts, evidence, exceptions, and prior decisions.
3. Return allow, allow-with-conditions, warn, block, or unable-to-determine with citations, missing items, expiry, and remediation actions.
4. Product renders the explanation before commit and does not reduce a block to a generic error.
5. For condition/warn, collect acknowledgement or required product action/evidence.
6. For eligible override, require defined authority, reason, evidence, compensating controls, duration, and separation of duties.
7. Owning product commits or refuses its own transaction and reports the actual outcome back.
8. Expire/recheck decisions when context changes or the validity window ends.

### Exception and recovery paths

- Compliance Core unavailable, facts stale, action changed after decision, duplicate commit, override authority missing, result expired, or product cannot prove final outcome.
- Offline mode may queue only explicitly offline-safe actions; hard gates must be revalidated online before final commit.

### Cross-product and external handoffs

- Any product ↔ Compliance Core.
- Compliance Core → StaffArr/RecordArr for approval/evidence references.
- Outcome → ReportArr.

### Evidence and audit record

- Gate request/context.
- Decision/trace/validity.
- Acknowledgement/override.
- Product commit outcome.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Gate latency.
- Block/override rate.
- Expired decision attempts.
- Missing-fact resolution.
- Post-gate incidents.

## CC-WF-008 — Manage a finding through verified closure

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Convert a compliance exception into accountable remediation and prove the fix is effective. |
| Trigger | Evaluation, audit, assessment, control test, incident, complaint, external notice, or reviewer opens a finding. |

### Actors

- Finding owner
- Control/process owner
- Reviewer/verifier
- Compliance Core

### State path

`open → triage → contained → plan → in_progress → verification → closed → reopened → risk_accepted`

### Required sequence

1. Create finding with source, requirement/control, subject/scope, severity, evidence, due date policy, and initial risk.
2. Deduplicate or relate recurring/overlapping findings without hiding separate occurrences.
3. Assign accountable owner and collaborators through StaffArr-backed permissions.
4. Record containment, root-cause analysis, corrective/preventive plan, milestones, resources, and risk acceptance/exception request when appropriate.
5. Track tasks through owning products and collect source evidence by reference.
6. Escalate overdue/high-risk work and reassess when operations/rules change.
7. Require independent verification of implementation and effectiveness where policy demands.
8. Close, reopen, or convert to accepted residual risk with full rationale and follow-up schedule.

### Exception and recovery paths

- Owner inactive, finding scope expands, due date extension lacks authority, evidence is insufficient, remediation creates another risk, recurrence occurs, or verifier is conflicted.
- Closing tasks does not automatically close the finding.

### Cross-product and external handoffs

- Compliance Core ↔ StaffArr/RecordArr/owning products.
- Compliance Core → ReportArr: status/risk.
- NexArr: escalations/authorization.

### Evidence and audit record

- Finding source/requirement/evidence.
- Ownership/risk/due changes.
- Plans/tasks/product outcomes.
- Verification/closure/reopen.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to assign/contain/close.
- Overdue rate.
- Recurrence.
- Verification failure.
- Residual risk.

## CC-WF-009 — Request, approve, monitor, and expire an exception

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Allow a narrowly scoped departure only when justified, authorized, compensated, time-limited, and continuously reviewable. |
| Trigger | A user cannot meet a requirement or a rule explicitly permits an exception/exemption/waiver. |

### Actors

- Requester
- Process/control owner
- Compliance/risk approver
- Compliance Core

### State path

`draft → submitted → review → approved → active → condition_failed → expired → revoked → closed → denied`

### Required sequence

1. Identify exact requirement/control/gate, subject/scope, dates, reason, legal basis, business impact, and requested treatment.
2. Determine whether the case is a rule-defined exemption, internal exception, temporary waiver, interpretation, or risk acceptance.
3. Collect evidence, alternatives considered, residual risk, compensating controls, monitoring, and exit/remediation plan.
4. Check conflicts, aggregate exposure, prior exceptions, authority thresholds, and separation of duties.
5. Route tiered approvals and legal/compliance review based on type/risk/duration.
6. Activate only the approved scope/version and make it visible in future evaluation traces.
7. Monitor conditions, evidence, incidents, and expiration reminders; reevaluate on relevant change.
8. Expire, revoke, renew through a new review, or close after remediation; never silently auto-renew.

### Exception and recovery paths

- No legal authority, request is too broad, compensating controls are untestable, approver is conflicted, risk exceeds authority, condition fails, or rule change invalidates the basis.
- Historical evaluations retain the exception version applied.

### Cross-product and external handoffs

- Compliance Core ↔ StaffArr/RecordArr/owning products.
- Compliance Core → ReportArr: exception exposure.

### Evidence and audit record

- Request/basis/scope.
- Risk/controls/evidence.
- Approvals/version.
- Monitoring/expiry/revocation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Approval cycle.
- Active exposure.
- Renewal rate.
- Condition failures.
- Exceptions converted to permanent fix.

## CC-WF-010 — Assess a regulatory change and drive adoption

| Field | Definition |
| --- | --- |
| Classification | CURRENT · DEMOCRATIZE |
| Implementation state | Durable |
| Purpose | Translate a source change into governed rule/control updates and concrete operational work. |
| Trigger | A monitored source publishes, amends, delays, interprets, or rescinds a requirement. |

### Actors

- Regulatory analyst
- Compliance owner
- Affected product/process owners
- Compliance Core

### State path

`detected → triage → analysis → approved_interpretation → plan → implementation → verification → closed → monitoring`

### Required sequence

1. Capture source event, authority, publication/effective dates, prior/new versions, and confidence.
2. Compare citations and classify additions, removals, threshold/date/definition/reporting/enforcement changes.
3. Map potential impact to rulepacks, facts, controls, evidence, policies, training, products, tenants, legal entities, sites, roles, assets, materials, suppliers, customers, and workflows.
4. Route analyst review and document interpretation/assumptions with citations.
5. Create a change plan with rule/control/document/training/system/process actions, owners, dependencies, due dates, and validation.
6. Author/test/publish required content versions and coordinate product configuration/workflow changes.
7. Communicate affected users with plain-language impact and track acknowledgements/tasks.
8. Verify adoption and post-effective-date outcomes, then close or continue monitoring.

### Exception and recovery paths

- Source is unofficial, effective date disputed, applicability uncertain, change is retroactive, content dependencies conflict, or product implementation cannot meet date.
- Urgent containment may precede final interpretation but remains explicitly provisional.

### Cross-product and external handoffs

- Compliance Core ↔ RecordArr/TrainArr/all affected products.
- ReportArr: impact/readiness.
- NexArr: notifications.

### Evidence and audit record

- Source/diff/citations.
- Interpretation/impact map.
- Plan/tasks/releases.
- Communication/verification.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Detection-to-triage.
- Impact cycle.
- Tasks complete before effective date.
- Late adoption.
- Post-change findings.

## CC-WF-011 — Plan and execute a control test or assessment

| Field | Definition |
| --- | --- |
| Classification | COMMON · DEMOCRATIZE |
| Implementation state | Target |
| Purpose | Evaluate control design and operation using reproducible scope, populations, samples, procedures, evidence, and review. |
| Trigger | An assessment calendar, audit plan, change, incident, risk threshold, or continuous-monitoring signal triggers testing. |

### Actors

- Assessment owner
- Tester
- Control owner
- Reviewer
- Compliance Core

### State path

`planned → population_ready → sampling → fieldwork → owner_response → review → approved → follow_up → closed`

### Required sequence

1. Define objective, criteria, controls/requirements, subjects, period, population source, sampling method, procedures, evidence, independence, and rating scale.
2. Resolve population from owning products with an immutable as-of query/snapshot and document exclusions.
3. Select reproducible samples or continuous tests and preserve selection logic/seed/version.
4. Execute procedures, capture workpapers/evidence references, observations, deviations, and tester conclusions.
5. Route exceptions to control owner response and open findings when thresholds are met.
6. Perform reviewer challenge, resolve notes, and approve design/operating-effectiveness ratings.
7. Update risk/control readiness without treating one test as universal proof.
8. Schedule follow-up or continuous monitoring and finalize RecordArr audit workpapers.

### Exception and recovery paths

- Population incomplete, source data stale, tester lacks independence, sample cannot be reproduced, procedure changed mid-test, evidence inaccessible, or management disputes conclusion.
- Overrides and excluded items remain visible.

### Cross-product and external handoffs

- Compliance Core ↔ all source products/RecordArr/ReportArr.
- StaffArr: tester/owner authority.

### Evidence and audit record

- Plan/criteria/population.
- Sample/procedure/evidence.
- Exceptions/responses.
- Review/rating/follow-up.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Planning-to-close.
- Population coverage.
- Exception rate.
- Review rework.
- Control effectiveness trend.

## CC-WF-012 — Score risk and choose treatment

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Make risk evaluation transparent, attributable, and connected to controls, incidents, obligations, and operational plans. |
| Trigger | A risk is identified or reassessment is due after change, incident, finding, control result, or schedule. |

### Actors

- Risk owner
- Risk/compliance reviewer
- Executive approver
- Compliance Core

### State path

`identified → analysis → review → treatment → accepted → monitoring → reassessment → closed`

### Required sequence

1. Define risk statement, category, cause, event, consequence, scope, owner, horizon, and related requirements/processes/assets/parties.
2. Record rating methodology/version and qualitative or quantitative inputs with source, range/confidence, and assumptions.
3. Calculate inherent risk and map existing controls with design/operating-effectiveness evidence.
4. Calculate residual risk without hiding overrides or model uncertainty.
5. Compare appetite/tolerance/authority and consider avoid, reduce, transfer, accept, or exploit treatment as applicable.
6. Create treatment actions through owning products and document cost/benefit/dependencies.
7. Route risk acceptance/escalation and set monitoring indicators/review date.
8. Reassess from events, control results, findings, regulatory changes, or scheduled review.

### Exception and recovery paths

- Risk model/version changed, inputs conflict, aggregation double-counts exposure, owner lacks authority, treatment shifts risk elsewhere, or uncertainty is too high for a numeric score.
- The displayed score includes methodology and assumptions.

### Cross-product and external handoffs

- Compliance Core ↔ ReportArr/StaffArr/owning products/LedgArr where financial context is permitted.
- RecordArr: approvals/evidence.

### Evidence and audit record

- Risk statement/scope.
- Method/inputs/assumptions.
- Controls/residual rating.
- Treatment/acceptance/reassessment.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to assess.
- Risks over appetite.
- Treatment completion.
- Rating volatility.
- Loss/incident correlation.

## CC-WF-013 — Forecast compliance readiness

| Field | Definition |
| --- | --- |
| Classification | CURRENT · DEMOCRATIZE |
| Implementation state | Durable |
| Purpose | Warn owners about likely future compliance gaps before they become failed gates, findings, or audit surprises. |
| Trigger | Scheduled analytics or relevant product/rule/evidence change updates the forecast horizon. |

### Actors

- Compliance owner
- Operational owners
- Compliance Core
- ReportArr

### State path

`scheduled → collecting → forecasted → review → actions_open → monitoring → realized → dismissed`

### Required sequence

1. Choose horizon, scope, rule/control set, confidence threshold, and included operational signals.
2. Collect expiration, due-date, staffing, training, maintenance, document, supplier, inventory, quality, transport, order, finance, rule-change, and evidence-health signals from permitted sources.
3. Evaluate known future effective dates and simulate expected fact/evidence state without mutating live records.
4. Calculate explainable readiness risks with drivers, confidence, dependencies, and earliest intervention dates.
5. Suppress duplicate/no-action noise and prioritize by consequence, lead time, owner, and dependency criticality.
6. Open recommended tasks in owning products or route review when forecast is uncertain.
7. Track whether owners act and compare forecast to actual outcomes to calibrate models.
8. Retain forecast version and inputs for audit and model review.

### Exception and recovery paths

- Source has no reliable future dates, event is speculative, model overfits sparse history, cross-product schedule conflicts, or recommendations exceed authorization.
- Forecasts are labeled estimates, never presented as current noncompliance.

### Cross-product and external handoffs

- All products → Compliance Core/ReportArr.
- Compliance Core → owning products: proposed actions.
- RecordArr: forecast packages where needed.

### Evidence and audit record

- Model/version/horizon.
- Inputs/drivers/confidence.
- Recommendations/owner decisions.
- Actual outcome/calibration.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Lead time gained.
- Forecast precision/recall.
- Prevented expirations/failures.
- Dismissal reasons.
- Model calibration.

## CC-WF-014 — Evaluate a theoretical situation before change

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED · DEMOCRATIZE |
| Implementation state | Durable |
| Purpose | Test a planned operation against compliance obligations and constraints without creating production records. |
| Trigger | A user proposes a new site, operation, asset, material, role, route, supplier, customer service, order flow, document, or financial structure. |

### Actors

- Planner/requester
- Compliance reviewer
- Compliance Core

### State path

`draft → baseline_loaded → modeling → evaluating → review → approved_plan → rejected → expired → archived`

### Required sequence

1. Create an isolated scenario with purpose, owner, baseline scope, rule/content as-of version, and expiration.
2. Clone only permitted baseline facts by reference and clearly distinguish them from hypothetical overrides/additions/removals.
3. Collect scenario-specific answers through plain-language questionnaires and source-product proposals.
4. Evaluate applicability, requirements, evidence, gates, risks, controls, filings, training, inspections, documents, and operational dependencies.
5. Show result differences from baseline with citations, unknowns, conflicts, costs/lead times where available, and confidence.
6. Let reviewers compare alternatives and record assumptions/decisions.
7. Generate an implementation checklist routed as proposals to owning products; no live record is silently created.
8. Archive scenario evidence or promote approved inputs through normal product workflows with new validation.

### Exception and recovery paths

- Scenario uses unavailable rule version, baseline facts changed materially, hypothetical values violate data type, sensitive data should not be cloned, or uncertainty prevents a reliable decision.
- Scenarios never satisfy live evidence or gate requirements.

### Cross-product and external handoffs

- Compliance Core ↔ all products for read/proposal.
- ReportArr: scenario comparison.
- RecordArr: approved study/package.

### Evidence and audit record

- Scenario/baseline/version.
- Hypotheses/assumptions.
- Trace/delta/results.
- Decision/checklist/promotion links.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Scenario cycle.
- Unknowns resolved.
- Alternative comparisons.
- Changes caught before launch.
- Plan completion.

## CC-WF-015 — Manage SDS and hazard communication applicability

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Connect authoritative safety data and material facts to HazCom obligations, evidence, training, labeling, and operational gates. |
| Trigger | A material/product is introduced or changed, an SDS is uploaded/updated/expired, or a location/activity inventory changes. |

### Actors

- EHS/compliance owner
- Supply/warehouse/maintenance owner
- Compliance Core
- RecordArr

### State path

`identified → document_pending → mapping_review → active → conflicted → expiring → superseded → restricted`

### Required sequence

1. Identify product/material key, manufacturer/supplier, formulation/product identifier, revision date, language, jurisdiction, and source document.
2. Store the SDS in RecordArr and register immutable reference/hash/metadata in Compliance Core.
3. Extract or map controlled hazard, composition/threshold, PPE, first-aid, fire, spill, handling/storage, transport, disposal, and regulatory facts as reviewable proposals.
4. Resolve material/product aliases and conflicts across suppliers/revisions without merging distinct formulations.
5. Evaluate applicability by tenant/site/activity/quantity/use/storage and determine labeling, access, training, PPE, storage, reporting, and emergency requirements.
6. Route missing/expired SDS, translation, container label, training, inventory/location, and evidence actions to owning products.
7. Gate receiving/use/transfer only where configured and explain exact missing requirements.
8. Reevaluate on revision, composition, quantity, site, activity, or rule change and retain prior versions.

### Exception and recovery paths

- SDS is not authoritative, formulation differs, trade secret limits detail, revision is older than tenant copy, language unavailable, material identity ambiguous, or extracted facts conflict.
- Compliance Core does not own physical inventory or storage locations.

### Cross-product and external handoffs

- SupplyArr/LoadArr/MaintainArr/StaffArr/TrainArr/RecordArr ↔ Compliance Core.
- AssurArr: receipt/nonconformance.
- ReportArr: HazCom readiness.

### Evidence and audit record

- Material/SDS source/version/hash.
- Extracted/mapped facts/review.
- Applicability/requirements.
- Actions/gates/supersession.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- SDS coverage.
- Days to obtain/review.
- Expired/conflicted SDS.
- Training/label gaps.
- Blocked receipt/use.

## CC-WF-016 — Build and deliver an audit package

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Produce a repeatable point-in-time package of scope, rules, facts, controls, evidence, tests, findings, responses, and approvals. |
| Trigger | An internal/external audit, regulator request, customer assurance review, certification, or management review requires delivery. |

### Actors

- Audit/package owner
- Compliance reviewer
- External recipient
- Compliance Core
- RecordArr
- ReportArr

### State path

`requested → collecting → gaps → review → approved → finalized → shared → supplemented → revoked`

### Required sequence

1. Define audience, authority/purpose, legal entity/site/product/subject scope, period/as-of date, rule/control versions, requested artifacts, redaction, and due date.
2. Resolve permitted evaluations, facts, evidence mappings, controls, tests, findings, exceptions, changes, actions, metrics, and source references as of the requested point.
3. Generate manifest with stable identifiers, versions, hashes, lineage, exclusions, and unresolved gaps.
4. Route evidence requests to owning products instead of collecting duplicate copies.
5. Review completeness, legal privilege/confidentiality, sensitive data, redaction, and recipient scope.
6. Approve/finalize immutable package in RecordArr and produce professional indexed export/delivery.
7. Share through scoped, expiring, revocable access and log view/download/question activity.
8. Create supplemental versions for corrections or additional requests without altering the original.

### Exception and recovery paths

- Historical source version unavailable, evidence under legal hold/privilege, recipient scope changes, redaction removes essential context, source record was corrected, or package is too large for delivery.
- A package distinguishes generated summaries from original evidence.

### Cross-product and external handoffs

- Compliance Core ↔ RecordArr/ReportArr/all source products/NexArr.
- External portal/delivery provider.

### Evidence and audit record

- Scope/as-of/version.
- Manifest/gaps/lineage.
- Review/redaction/approval.
- Final artifact/access/activity.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Package cycle.
- Evidence reuse.
- Gap count.
- Recipient questions.
- Supplement/revocation rate.

## CC-WF-017 — Handle a Compliance Core service degradation

| Field | Definition |
| --- | --- |
| Classification | FOUNDATION |
| Implementation state | Target |
| Purpose | Keep product behavior safe and understandable when evaluations, content, or synchronization are unavailable. |
| Trigger | Health monitoring, timeout, content defect, dependency outage, or repeated evaluation failure detects degradation. |

### Actors

- Platform operator
- Compliance administrator
- Product owner
- Compliance Core

### State path

`detected → contained → degraded_policy → repair → replay → validation → restored → postmortem`

### Required sequence

1. Classify affected tenants/products/rulepacks/gates/functions and whether cached decisions remain valid.
2. Freeze content publication or affected rule versions when integrity is uncertain.
3. Apply explicit per-gate fail-open, fail-closed, warn, cached-until, or manual-review policy configured by consequence—not a single global default.
4. Return structured degraded responses with correlation ID, saved state, retry safety, and manual path.
5. Notify affected owners and prevent duplicate finding/task storms.
6. Repair, replay fact/events/evaluations idempotently, and reconcile product commit outcomes.
7. Run regression and data-integrity checks before restoring normal service.
8. Document incident, decisions, affected actions, recovery, and prevention.

### Exception and recovery paths

- Hard safety gate cannot be evaluated, cached decision expired, product already committed, rule content corruption, cross-tenant concern, or replay produces different historical result.
- Offline clients cannot bypass a required online gate.

### Cross-product and external handoffs

- Compliance Core ↔ all products/NexArr/ReportArr/RecordArr.
- Operations notification channels.

### Evidence and audit record

- Incident/scope/classification.
- Degraded policy decisions.
- Replay/reconciliation.
- Validation/restoration/postmortem.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Detection/recovery time.
- Affected actions.
- Unsafe bypasses.
- Replay mismatches.
- Duplicate-task suppression.



## Workflow definition of done

A workflow is releasable only when its happy path, every listed exception, dependency outage, duplicate/retry, stale/concurrent update, permission denial, cancellation, correction/void, archival/retention, import/API/event path, notification, audit trail, reporting projection, mobile behavior, and professional printable output have automated and human-usable acceptance coverage.
