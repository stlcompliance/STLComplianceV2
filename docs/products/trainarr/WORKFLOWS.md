# TrainArr — LMS Workflow Catalog

[Feature catalog](./FEATURESET.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md)

## Workflow contract

This document defines the end-to-end business state machines for TrainArr. A workflow is not complete because a page or endpoint exists: the trigger, authority, state transitions, validation, handoffs, evidence, exceptions, retries, conflict behavior, notifications, reporting, and final source-of-truth commit must all work together.

## Ownership rule

TrainArr owns learning definitions, programs, versions, assignments, execution progress, evaluations, practical signoffs, remediation, qualifications, certificates, and training evidence. It converts role, asset, location, incident, and compliance requirements into demonstrable readiness. StaffArr owns the person and workforce assignment; Compliance Core owns regulatory meaning; RecordArr owns files.

- Person identity, employment, manager, organization, or location; StaffArr owns those records.
- Legal interpretation or applicability rules; Compliance Core supplies rule/fact/evidence context.
- Personnel incident truth; StaffArr owns the incident while TrainArr owns remediation/training outcomes.
- File binaries and controlled-document lifecycle; RecordArr owns them.
- Operational permission to perform a task; the owning product enforces action permission and may use TrainArr qualification results.

## Workflow index

| Workflow ID | Workflow | Class | State | Trigger |
| --- | --- | --- | --- | --- |
| TR-WF-001 | Create, review, version, and publish a training definition | CURRENT · COMMON | Durable | An authorized author creates or revises a definition. |
| TR-WF-002 | Build and publish a training program or learning path | CURRENT · COMMON | Durable | A program owner creates or revises a program. |
| TR-WF-003 | Automatic assignment from role, location, or requirement | CURRENT · COMMON | Durable | A StaffArr person/assignment changes, a rulepack impact is published, or a scheduled applicability run occurs. |
| TR-WF-004 | Learner completes an online or guided assignment | CURRENT · COMMON | Durable | A learner opens an assigned course or guided activity. |
| TR-WF-005 | Assessment and remediation | CURRENT · COMMON | Durable | A learner starts an assessment or an evaluator records a scored check. |
| TR-WF-006 | Practical evaluation and authorized signoff | CURRENT · UNDERSERVED | Durable | An assignment reaches a practical step or an evaluator opens a qualification check. |
| TR-WF-007 | Instructor-led session scheduling and attendance | COMMON | Target | An instructor or administrator schedules a session from a published offering. |
| TR-WF-008 | Certificate or qualification issue, renewal, suspension, and revocation | CURRENT · COMMON | Durable | Completion/evaluation satisfies issuance rules, a renewal window opens, or an incident/expiration changes validity. |
| TR-WF-009 | Incident-triggered retraining and qualification review | CURRENT · UNDERSERVED | Durable | StaffArr forwards a training-related personnel incident. |
| TR-WF-010 | Rulepack change impact and training update | CURRENT · DEMOCRATIZE | Durable | Compliance Core publishes a rulepack/version/change-impact event. |
| TR-WF-011 | External credential review and equivalency | COMMON · UNDERSERVED | Target | A learner/administrator submits an external certificate, transcript, license, or experience claim. |
| TR-WF-012 | Offline mobile learning, checklist, and sync | UNDERSERVED | Target | A learner/evaluator downloads an offline-capable assignment in Field Companion. |
| TR-WF-013 | Training reminder, escalation, and manager intervention | CURRENT · COMMON | Durable | A scheduled due/overdue/expiration worker runs or an assignment status changes. |
| TR-WF-014 | Evidence retention, orphan reference, and audit package | CURRENT · COMMON | Durable | A retention/orphan-reference worker runs or an auditor requests a package. |
| TR-WF-015 | Skills gap and development plan | UNDERSERVED · DEMOCRATIZE | Target | A manager, employee, staffing workflow, or scenario identifies a capability gap. |

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

## TR-WF-001 — Create, review, version, and publish a training definition

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Build reusable training with clear outcomes, evidence, and version history. |
| Trigger | An authorized author creates or revises a definition. |

### Actors

- Training author
- Subject-matter expert
- Approver
- TrainArr

### State path

`draft → review → approved → published → superseded → retired`

### Required sequence

1. Define title, purpose, audience, objectives, owner, estimated duration, and validity.
2. Add ordered steps, content references, activities, completion rules, and branches.
3. Add assessment, practical signoff, evidence, citation, and accessibility requirements.
4. Validate broken references, unsupported branches, missing pass rules, and permissions.
5. Preview learner/instructor/evaluator views in light/dark and mobile layouts.
6. Route for subject and compliance review.
7. Publish an immutable version with migration policy for active assignments.
8. Notify affected program owners and retain the prior version for history.

### Exception and recovery paths

- Referenced RecordArr document is obsolete, required evaluator role missing, branch has no terminal path, or active assignment migration is unsafe.
- Regulatory citation changes during review.

### Cross-product and external handoffs

- TrainArr ↔ RecordArr: content/evidence refs.
- TrainArr ↔ Compliance Core: citation/requirement validation.
- TrainArr → ReportArr: publication metrics.

### Evidence and audit record

- Definition/version diff.
- Review comments and approvals.
- Validation results.
- Publication/migration decisions.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Authoring cycle time.
- Review rework.
- Broken references.
- Active learners migrated safely.

## TR-WF-002 — Build and publish a training program or learning path

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Compose definitions into a role-, objective-, or qualification-oriented program. |
| Trigger | A program owner creates or revises a program. |

### Actors

- Program owner
- Training author
- Approver
- TrainArr

### State path

`draft → review → published → superseded → retired`

### Required sequence

1. Define program purpose, target audience, owner, completion window, and resulting qualification/credit.
2. Add versioned training definitions and external/content references.
3. Configure order, prerequisites, optional/elective branches, equivalencies, and completion policy.
4. Estimate workload and detect circular prerequisites.
5. Preview applicability against sample StaffArr people/roles.
6. Review and approve.
7. Publish a version and decide treatment of active assignments.
8. Monitor adoption and retire superseded versions.

### Exception and recovery paths

- Circular prerequisite, missing definition version, incompatible completion rules, excessive workload, or qualification mapping conflict.
- Learner has partial credit from a prior program version.

### Cross-product and external handoffs

- StaffArr → TrainArr: sample role/person context.
- TrainArr → StaffArr: resulting qualification publication.
- RecordArr/Compliance Core refs as applicable.

### Evidence and audit record

- Program/version composition.
- Prerequisite validation.
- Approvals and migration decisions.
- Qualification mapping.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to publish.
- Program completion.
- Elective/path usage.
- Migration exceptions.

## TR-WF-003 — Automatic assignment from role, location, or requirement

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Create the right assignment for the right person and explain why. |
| Trigger | A StaffArr person/assignment changes, a rulepack impact is published, or a scheduled applicability run occurs. |

### Actors

- Training administrator
- Manager
- StaffArr
- Compliance Core
- TrainArr

### State path

`evaluating → assigned → not_required → credit_applied → waiver_pending → blocked → canceled`

### Required sequence

1. Receive person, role, location, assignment, or requirement change.
2. Evaluate applicability profile and training matrix with versioned inputs.
3. Find existing valid credit, equivalency, waiver, or active assignment.
4. Create, update, cancel, or leave an assignment with reason and due-date calculation.
5. Notify learner and manager; explain source requirement.
6. Publish readiness impact to StaffArr.
7. Reconcile stale or conflicting assignments during recalculation.

### Exception and recovery paths

- Missing person reference, conflicting matrices, future-dated assignment, waiver pending, person inactive, or duplicate external certificate.
- Training is required but no published program exists.

### Cross-product and external handoffs

- StaffArr → TrainArr: person/assignment event.
- Compliance Core → TrainArr: requirement/rulepack change.
- TrainArr → StaffArr: assignment/readiness publication.

### Evidence and audit record

- Applicability inputs/rules.
- Assignment reason and due-date rule.
- Credit/waiver decision.
- Publication result.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Auto-assignment precision.
- Manual override rate.
- Duplicate assignment rate.
- Time from change to assignment.

## TR-WF-004 — Learner completes an online or guided assignment

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Deliver content and capture valid progress through completion. |
| Trigger | A learner opens an assigned course or guided activity. |

### Actors

- Learner
- TrainArr
- Instructor/evaluator when required

### State path

`not_started → in_progress → awaiting_evaluation → completed → failed → expired → canceled`

### Required sequence

1. Validate learner, assignment state, due date, prerequisites, and content version.
2. Resume at the last valid step and show workload/progress.
3. Present content/activity in an accessible format.
4. Record completion attempts, time, responses, and evidence without trusting client-only state.
5. Evaluate branch/completion rules server-side.
6. Queue instructor/evaluator tasks when required.
7. Mark complete only when all mandatory rules pass.
8. Issue outcome, certificate/qualification request, notifications, and history entry.

### Exception and recovery paths

- Offline progress conflicts with server version, content unavailable, prerequisite revoked, failed attempt limit, accommodation needed, or assignment expires during session.
- Learner disputes completion/score.

### Cross-product and external handoffs

- Field Companion/web player → TrainArr: progress/evidence.
- TrainArr → RecordArr: evidence.
- TrainArr → StaffArr: readiness outcome.

### Evidence and audit record

- Content/version presented.
- Attempts/responses/time.
- Completion-rule evaluation.
- Evidence and final outcome.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Completion rate.
- Abandonment step.
- Time vs estimate.
- Sync conflict rate.
- Learner feedback.

## TR-WF-005 — Assessment and remediation

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Measure knowledge, explain gaps, and assign focused remediation. |
| Trigger | A learner starts an assessment or an evaluator records a scored check. |

### Actors

- Learner
- Evaluator
- TrainArr

### State path

`ready → in_progress → grading → passed → failed → remediation → voided`

### Required sequence

1. Validate attempt policy, identity assurance, accommodations, and question-bank version.
2. Select/randomize items and present one accessible attempt.
3. Record responses and tamper-evident timing/attempt data.
4. Score objective items and route subjective items to an evaluator.
5. Apply pass, partial mastery, or fail rules.
6. Show allowed feedback without disclosing protected item-bank data.
7. Create targeted remediation/retry plan from failed objectives.
8. Finalize score, evidence, and qualification impact after all grading.

### Exception and recovery paths

- Connectivity loss, suspected misconduct, invalid item, evaluator disagreement, accommodation request, or item-bank error after use.
- Question is challenged and later invalidated.

### Cross-product and external handoffs

- TrainArr → RecordArr: supporting evidence.
- TrainArr → StaffArr: qualification/readiness.
- TrainArr → ReportArr: item and cohort analytics.

### Evidence and audit record

- Attempt policy/version.
- Presented items/responses.
- Scoring and evaluator revisions.
- Remediation and final outcome.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Pass rate.
- Item discrimination/invalidations.
- Retry count.
- Remediation effectiveness.

## TR-WF-006 — Practical evaluation and authorized signoff

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Durable |
| Purpose | Prove that a learner can perform a task under observation. |
| Trigger | An assignment reaches a practical step or an evaluator opens a qualification check. |

### Actors

- Learner
- Qualified evaluator
- Manager
- TrainArr

### State path

`scheduled → in_progress → paused → passed → failed → remediation → voided`

### Required sequence

1. Confirm evaluator identity, authority, qualification, independence, and location/context.
2. Load the correct checklist/version and safety prerequisites.
3. Observe each step and record pass/fail/not-observed, notes, measurements, and evidence.
4. Pause/abort safely if a critical step fails.
5. Capture learner/evaluator acknowledgement or signature.
6. Apply overall result and required remediation.
7. Issue/renew qualification only after all rules pass.
8. Store evidence and publish readiness.

### Exception and recovery paths

- Evaluator qualification expired, conflict of interest, unsafe condition, equipment unavailable, offline evidence conflict, or learner disputes observation.
- Partial observation must be completed by another evaluator.

### Cross-product and external handoffs

- StaffArr → TrainArr: evaluator/person context.
- MaintainArr/other product → TrainArr: asset/task context.
- TrainArr → RecordArr/StaffArr: evidence and readiness.

### Evidence and audit record

- Evaluator authority snapshot.
- Checklist responses/evidence.
- Critical failure/abort.
- Signatures and qualification outcome.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- First-pass rate.
- Evaluator consistency.
- Time to signoff.
- Safety aborts.
- Dispute rate.

## TR-WF-007 — Instructor-led session scheduling and attendance

| Field | Definition |
| --- | --- |
| Classification | COMMON |
| Implementation state | Target |
| Purpose | Schedule and deliver classroom, virtual, or field sessions with capacity and attendance evidence. |
| Trigger | An instructor or administrator schedules a session from a published offering. |

### Actors

- Training administrator
- Instructor
- Learners
- Manager

### State path

`draft → open → full → in_progress → completed → canceled`

### Required sequence

1. Choose offering/version, instructor, location/link, capacity, dates, prerequisites, and resources.
2. Check instructor qualification and location/resource conflicts.
3. Invite/assign learners, manage waitlist, and sync calendars.
4. Send preparation reminders and accessible materials.
5. Capture attendance, late arrival/early departure, participation, and session evidence.
6. Launch any assessments/practical signoffs.
7. Resolve no-shows, makeups, and instructor changes.
8. Close session and update assignments/credit.

### Exception and recovery paths

- Overcapacity, canceled venue, instructor unavailable, prerequisite missing, virtual outage, or attendee identity mismatch.
- Attendance alone is insufficient for qualification.

### Cross-product and external handoffs

- TrainArr ↔ calendar/conferencing provider.
- TrainArr ↔ StaffArr: roster/manager context.
- TrainArr → RecordArr: attendance/session package.

### Evidence and audit record

- Session/version/resources.
- Invitations and waitlist.
- Attendance and changes.
- Assessments/signoffs and closure.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Fill rate.
- No-show rate.
- Instructor utilization.
- Makeup completion.

## TR-WF-008 — Certificate or qualification issue, renewal, suspension, and revocation

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Maintain a truthful, explainable credential lifecycle. |
| Trigger | Completion/evaluation satisfies issuance rules, a renewal window opens, or an incident/expiration changes validity. |

### Actors

- Training administrator
- Evaluator
- Learner
- StaffArr
- TrainArr

### State path

`pending → active → expiring → expired → suspended → revoked → superseded`

### Required sequence

1. Evaluate all required program, score, practical, evidence, experience, and approval inputs.
2. Check duplicates, prior credential, validity, issuer authority, and any active suspension.
3. Issue or renew an immutable credential with verification identifier and validity window.
4. Publish the result to StaffArr and affected products.
5. Schedule reminders and recertification assignments.
6. Suspend or revoke only through permissioned reasoned action or rule outcome.
7. Notify affected person/manager and operational products.
8. Preserve full history and correction path.

### Exception and recovery paths

- Evidence missing, conflicting external credential, issuer unavailable, incident review pending, renewal completed late, or publication delivery fails.
- Credential was issued in error and requires correction rather than silent deletion.

### Cross-product and external handoffs

- TrainArr → StaffArr/products: credential status.
- TrainArr → RecordArr: credential/evidence package.
- Compliance Core → TrainArr: validity/requirement context.

### Evidence and audit record

- Issuance rule/input snapshot.
- Credential document/hash.
- Status changes and reasons.
- Notifications/publication acknowledgements.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Issuance latency.
- Renewal before expiry.
- Publication drift.
- Suspension/revocation resolution.

## TR-WF-009 — Incident-triggered retraining and qualification review

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Durable |
| Purpose | Turn StaffArr incident context into focused remediation and a defensible qualification decision. |
| Trigger | StaffArr forwards a training-related personnel incident. |

### Actors

- StaffArr reviewer
- Training administrator
- Manager
- Learner
- Evaluator

### State path

`received → review → restricted → assigned → evaluation → restored → suspended → closed`

### Required sequence

1. Receive incident reference, person, context, severity, and requested review without copying confidential details.
2. Determine affected qualifications, programs, skills, or tasks.
3. Apply temporary qualification hold/suspension when authorized.
4. Create focused remediation assignment and evaluator tasks.
5. Track completion, signoff, overdue escalation, and evidence.
6. Decide restore, restrict, renew, suspend, or revoke based on defined criteria.
7. Publish outcome to StaffArr and operational products.
8. Link recurrence and close the remediation record.

### Exception and recovery paths

- Incident under investigation, person contests facts, no suitable remediation exists, evaluator conflict, overdue assignment, or operational emergency requires exception.
- Confidential incident details must remain in StaffArr.

### Cross-product and external handoffs

- StaffArr → TrainArr: remediation context.
- TrainArr → StaffArr/products: qualification outcome.
- TrainArr → RecordArr: completion package.

### Evidence and audit record

- Incident reference and scope.
- Decision criteria.
- Assignment/evaluation evidence.
- Outcome/publication.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to assign.
- Completion by due date.
- Qualification restore time.
- Repeat incident rate.

## TR-WF-010 — Rulepack change impact and training update

| Field | Definition |
| --- | --- |
| Classification | CURRENT · DEMOCRATIZE |
| Implementation state | Durable |
| Purpose | Identify which learning assets, credentials, and people are affected by a regulatory requirement change. |
| Trigger | Compliance Core publishes a rulepack/version/change-impact event. |

### Actors

- Compliance administrator
- Training owner
- Subject-matter expert
- TrainArr

### State path

`detected → triage → impact_review → remediation_planned → in_progress → effective → closed`

### Required sequence

1. Receive changed requirement/citation identifiers and effective dates.
2. Find linked training definitions, programs, content, qualifications, matrices, and active assignments.
3. Classify impact: no change, review, content update, reassessment, retraining, or credential invalidation risk.
4. Assign owners and due dates; show affected populations.
5. Revise/review/publish training versions.
6. Create transition assignments or grace-period logic.
7. Monitor completion before effective date.
8. Close impact with evidence and residual exceptions.

### Exception and recovery paths

- Mapping missing, source rule uncertain, effective date too near, content owner absent, or a change applies only to certain tenant facts.
- Prior evidence remains valid for some workers but not others.

### Cross-product and external handoffs

- Compliance Core → TrainArr: change event.
- TrainArr ↔ RecordArr: content/evidence refs.
- TrainArr → StaffArr/products: new requirements/readiness.

### Evidence and audit record

- Changed rules and mapping.
- Affected asset/population snapshot.
- Decisions/approvals.
- Published versions and transition completion.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to triage.
- Affected people ready by effective date.
- Unmapped requirements.
- Overdue content updates.

## TR-WF-011 — External credential review and equivalency

| Field | Definition |
| --- | --- |
| Classification | COMMON · UNDERSERVED |
| Implementation state | Target |
| Purpose | Evaluate prior training or third-party credentials without losing source and confidence context. |
| Trigger | A learner/administrator submits an external certificate, transcript, license, or experience claim. |

### Actors

- Learner
- Training administrator
- Evaluator
- TrainArr

### State path

`submitted → verification → mapping → accepted → partial → rejected → expired`

### Required sequence

1. Capture issuer, credential type, identifier, issue/expiry, jurisdiction, scope, and source evidence.
2. Check duplicates and known issuer/credential mappings.
3. Verify through provider/API/manual review where available.
4. Compare covered outcomes and validity to internal requirement.
5. Grant full, partial, temporary, or no equivalency with reason.
6. Create gap training/evaluation for uncovered outcomes.
7. Publish accepted qualification with external-source attribution.
8. Schedule re-verification/expiry and preserve review evidence.

### Exception and recovery paths

- Issuer cannot be verified, document appears altered, scope is ambiguous, credential expired, jurisdiction mismatch, or internal standard changed.
- Worker may appeal with more evidence.

### Cross-product and external handoffs

- TrainArr ↔ external credential provider.
- TrainArr → RecordArr: source evidence.
- TrainArr → StaffArr: accepted readiness.

### Evidence and audit record

- Original evidence/hash.
- Verification attempts.
- Outcome mapping and reviewer.
- Equivalency/expiry decision.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Verification time.
- Accepted/partial/rejected rate.
- Fraud flags.
- Gap completion.

## TR-WF-012 — Offline mobile learning, checklist, and sync

| Field | Definition |
| --- | --- |
| Classification | UNDERSERVED |
| Implementation state | Target |
| Purpose | Allow authorized training execution when connectivity is unreliable without creating unverifiable completion. |
| Trigger | A learner/evaluator downloads an offline-capable assignment in Field Companion. |

### Actors

- Learner
- Evaluator
- Field Companion
- TrainArr

### State path

`downloaded → offline_in_progress → queued → syncing → conflict → committed → rejected`

### Required sequence

1. Validate device/session, assignment, content license, sensitivity, and offline eligibility.
2. Download encrypted, version-pinned content and checklist data with expiry.
3. Execute activities and capture timestamps, responses, signatures, and evidence locally.
4. Show queued state and prohibit actions that require live validation.
5. On reconnect, upload in dependency order with idempotency keys.
6. Server validates assignment/version/authority and detects conflicts.
7. Resolve conflicts through automatic safe merge or explicit review.
8. Commit accepted progress and securely remove expired local data.

### Exception and recovery paths

- Assignment revoked, content version superseded, device compromised, duplicate action, clock skew, missing evidence, or both server and device changed the same step.
- Qualification cannot issue until server validation completes.

### Cross-product and external handoffs

- Field Companion ↔ NexArr: secure session/offline queue.
- Field Companion ↔ TrainArr: content/progress/evidence.
- TrainArr → RecordArr: uploaded evidence.

### Evidence and audit record

- Downloaded version/expiry.
- Local action log and device identity.
- Sync validation/conflicts.
- Final committed outcome.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Offline completion volume.
- Sync latency.
- Conflict/rejection rate.
- Lost-progress incidents.

## TR-WF-013 — Training reminder, escalation, and manager intervention

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Drive timely completion without duplicate or unactionable notifications. |
| Trigger | A scheduled due/overdue/expiration worker runs or an assignment status changes. |

### Actors

- Learner
- Manager
- Training administrator
- TrainArr

### State path

`scheduled → sent → delivered → acted → escalated → suppressed → failed`

### Required sequence

1. Evaluate tenant reminder/escalation policy and assignment state.
2. Suppress duplicates, completed/canceled items, leave periods, and quiet hours as configured.
3. Send an actionable notice with due reason, workload, blockers, and deep link.
4. Escalate to manager/owner based on thresholds and severity.
5. Create an inbox task only when an action is required.
6. Record delivery/open/action status where available.
7. Stop or adapt reminders after completion, approved extension, or reassignment.
8. Report chronic blockers and policy effectiveness.

### Exception and recovery paths

- Invalid contact channel, manager missing, leave/absence, inaccessible content, disputed assignment, or notification provider outage.
- High-risk qualification expiry may require operational block rather than more reminders.

### Cross-product and external handoffs

- TrainArr → NexArr notification infrastructure.
- TrainArr → StaffArr: manager/absence context.
- TrainArr → ReportArr: effectiveness metrics.

### Evidence and audit record

- Policy/version.
- Eligibility/suppression decision.
- Dispatch result.
- Action/escalation outcome.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Completion after reminder.
- Notification-to-action time.
- Suppression/duplicate rate.
- Overdue aging.

## TR-WF-014 — Evidence retention, orphan reference, and audit package

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Keep training proof valid, discover broken dependencies, and assemble audit-ready records. |
| Trigger | A retention/orphan-reference worker runs or an auditor requests a package. |

### Actors

- Training administrator
- Records administrator
- Auditor
- TrainArr
- RecordArr

### State path

`requested → validating → remediation → assembling → complete → supplemented → closed`

### Required sequence

1. Select person, program, qualification, location, requirement, and period scope.
2. Resolve assignment, progress, assessment, signoff, certificate, citation, and publication records.
3. Validate RecordArr links, content versions, evaluator authority, and retention status.
4. Flag missing/orphaned evidence and assign remediation.
5. Apply legal hold/retention rules before deletion or disposal.
6. Generate a manifest and request RecordArr package assembly.
7. Lock/finalize the audit snapshot and record access/download.
8. Track auditor questions and supplemental packages.

### Exception and recovery paths

- Record missing, content/evidence disposed incorrectly, evaluator person link missing, package too large, or legal hold prevents disposal.
- Confidential learner information requires redaction.

### Cross-product and external handoffs

- TrainArr ↔ RecordArr: evidence validation/package.
- TrainArr ↔ StaffArr: person/evaluator context.
- TrainArr → ReportArr: audit readiness metrics.

### Evidence and audit record

- Scope and snapshot time.
- Validation findings.
- Manifest/file hashes.
- Package generation/access.
- Supplemental responses.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Package time.
- Missing evidence rate.
- Orphan remediation time.
- Audit exceptions.

## TR-WF-015 — Skills gap and development plan

| Field | Definition |
| --- | --- |
| Classification | UNDERSERVED · DEMOCRATIZE |
| Implementation state | Target |
| Purpose | Turn role demand and demonstrated capability into an actionable learning plan. |
| Trigger | A manager, employee, staffing workflow, or scenario identifies a capability gap. |

### Actors

- Employee
- Manager
- Training administrator
- StaffArr
- TrainArr

### State path

`identified → assessed → planned → in_progress → ready → revised → closed`

### Required sequence

1. Define target role/task/skill profile and effective date.
2. Collect current qualifications, assessments, observations, experience, and self-declared skills with confidence.
3. Calculate explainable gaps without treating missing data as failure.
4. Recommend training, practice, mentor, project, or evaluation options.
5. Let employee/manager agree priorities and schedule.
6. Create assignments and coaching/evaluation tasks.
7. Track skill evidence and readiness progression.
8. Reassess and close or revise the plan.

### Exception and recovery paths

- Target role changes, skill taxonomy conflict, no suitable learning exists, manager/employee disagreement, or evidence is stale.
- Sensitive performance data must not be exposed beyond authorized participants.

### Cross-product and external handoffs

- StaffArr ↔ TrainArr: role/person/career context.
- TrainArr → operational products: readiness result only where needed.
- ReportArr: governed skills analytics.

### Evidence and audit record

- Target profile and evidence sources.
- Gap calculation/version.
- Agreed plan and assignments.
- Observed progression/outcome.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to readiness.
- Plan completion.
- Internal fill rate.
- Skill evidence recency.



## Workflow definition of done

A workflow is releasable only when its happy path, every listed exception, dependency outage, duplicate/retry, stale/concurrent update, permission denial, cancellation, correction/void, archival/retention, import/API/event path, notification, audit trail, reporting projection, mobile behavior, and professional printable output have automated and human-usable acceptance coverage.
