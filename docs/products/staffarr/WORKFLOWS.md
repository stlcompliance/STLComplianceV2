# StaffArr — HRM Workflow Catalog

[Feature catalog](./FEATURESET.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md)

## Workflow contract

This document defines the end-to-end business state machines for StaffArr. A workflow is not complete because a page or endpoint exists: the trigger, authority, state transitions, validation, handoffs, evidence, exceptions, retries, conflict behavior, notifications, reporting, and final source-of-truth commit must all work together.

## Ownership rule

StaffArr is the tenant system of record for people in the workforce context, organizational structure, internal locations, assignments, delegated authority, personnel incidents, readiness context, and HR processes. It is also the practical administration surface for product roles and permission assignments. NexArr remains the source of truth for login credentials and sessions, while StaffArr may expose permissioned NexArr-backed account provisioning and account-edit actions.

- Platform credentials, authentication factors, sessions, or external IdP mappings; NexArr owns those truths.
- Training definitions, assignments, evaluations, certificates, or qualification issuance; TrainArr owns them and publishes outcomes to StaffArr.
- Payroll calculation, tax filing, payments, or general-ledger posting; StaffArr prepares time/compensation evidence and LedgArr or an external payroll system owns financial execution.
- Operational asset, inventory, route, order, quality, document, or compliance records.
- Customer/vendor identities; CustomArr and SupplyArr own external commercial parties in their respective domains.

## Workflow index

| Workflow ID | Workflow | Class | State | Trigger |
| --- | --- | --- | --- | --- |
| ST-WF-001 | Create person and establish employment relationship | CURRENT · COMMON | Durable | An authorized HR user creates a person, imports a record, or converts an accepted candidate. |
| ST-WF-002 | Recruiting requisition to accepted offer | CURRENT · COMMON | Durable | A manager or HR partner requests a new or replacement position. |
| ST-WF-003 | Onboarding and first-day readiness | COMMON · UNDERSERVED | Partial | A person is hired, rehired, transferred, or assigned a new role/location. |
| ST-WF-004 | Internal transfer, promotion, or assignment change | CURRENT · COMMON | Durable | An approved personnel change request reaches its effective date. |
| ST-WF-005 | Offboarding and separation | CURRENT · COMMON | Durable | A resignation, termination, contract end, retirement, death, or no-show is recorded. |
| ST-WF-006 | Clock event to approved timesheet | CURRENT · COMMON | Durable | A worker clocks in/out, enters time, or an operational product submits labor evidence. |
| ST-WF-007 | Leave request and return to work | CURRENT · COMMON | Durable | A worker or manager submits planned/unplanned leave. |
| ST-WF-008 | Personnel incident to restriction and retraining | CURRENT · UNDERSERVED | Durable | An incident is reported manually or by MaintainArr, RoutArr, LoadArr, AssurArr, or Field Companion. |
| ST-WF-009 | Role creation, permission assignment, and review | CURRENT · COMMON | Durable | A tenant administrator creates/edits a role or assigns it to a person. |
| ST-WF-010 | Performance review cycle | CURRENT · COMMON | Durable | HR publishes a review cycle or a manager starts an allowed check-in. |
| ST-WF-011 | Compensation change and approval | CURRENT · COMMON | Durable | A manager/HR user initiates merit, promotion, adjustment, market, or correction change. |
| ST-WF-012 | Benefits enrollment and qualifying life event | CURRENT · COMMON | Durable | An enrollment window opens or a qualifying life event is approved. |
| ST-WF-013 | Worker profile update request | CURRENT · UNDERSERVED | Durable | A worker submits a change to an editable or review-required field. |
| ST-WF-014 | Qualification-aware staffing and readiness check | CURRENT · UNDERSERVED | Partial | A manager staffs a shift, task, inspection, route, warehouse operation, or maintenance assignment. |
| ST-WF-015 | Scheduled workforce/payroll export and reconciliation | CURRENT · COMMON | Durable | A configured export schedule runs or an authorized user triggers a run. |

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

## ST-WF-001 — Create person and establish employment relationship

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Create the canonical workforce record without requiring a login. |
| Trigger | An authorized HR user creates a person, imports a record, or converts an accepted candidate. |

### Actors

- HR administrator
- Hiring manager
- StaffArr

### State path

`draft → pending_start → active → leave → inactive → terminated`

### Required sequence

1. Search for existing/current/former person matches.
2. Create the minimum person identity and employment relationship.
3. Assign worker type, status, effective dates, manager, org unit, position, and location.
4. Collect required fields using tenant and Compliance Core guidance.
5. Create onboarding plan and downstream requests.
6. Optionally provision a NexArr login through a delegated action.
7. Publish person/readiness references and record history.

### Exception and recovery paths

- Possible duplicate or rehire.
- Required location/position does not exist and must be quick-created.
- Future-dated start, no manager, or conflicting assignment.
- Login email already belongs to another NexArr user.

### Cross-product and external handoffs

- StaffArr → NexArr: optional login provisioning.
- StaffArr → TrainArr: role/location training applicability.
- StaffArr → downstream products: person/assignment reference publication.

### Evidence and audit record

- Source/consent and match decision.
- Effective-dated relationship.
- Assignment and approval history.
- Provisioning results.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to create.
- Duplicate rate.
- Missing-required-field rate.
- Onboarding plan created.

## ST-WF-002 — Recruiting requisition to accepted offer

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Move a staffing need through approval, application, assessment, interview, offer, and hire conversion. |
| Trigger | A manager or HR partner requests a new or replacement position. |

### Actors

- Hiring manager
- Recruiter
- Interviewers
- Candidate
- HR approver

### State path

`draft → approval → open → screening → interviewing → offer → filled → closed → canceled`

### Required sequence

1. Create and approve requisition with position, location, schedule, compensation range, and requirements.
2. Publish an application URL/form and capture consent.
3. Screen and deduplicate applications; create candidate records.
4. Advance through configured stages, interviews, tests, notes, and scorecards.
5. Capture references/background status as integrations or evidence references.
6. Draft, approve, issue, and accept/reject the offer.
7. Convert the accepted candidate to a person and onboarding plan without rekeying.
8. Close/reconcile remaining candidates and requisition.

### Exception and recovery paths

- Duplicate applicant, accommodation request, incomplete application, conflict-of-interest interviewer, offer approval failure, background exception, or candidate withdrawal.
- Multiple candidates are hired from one evergreen requisition.

### Cross-product and external handoffs

- StaffArr ↔ public application surface.
- StaffArr → RecordArr: resume/offer/evidence storage.
- StaffArr → NexArr/TrainArr: hire onboarding requests.

### Evidence and audit record

- Requisition approvals.
- Application and consent versions.
- Interview scorecards/notes.
- Offer versions and acceptance.
- Conversion audit.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to approve.
- Time to fill.
- Stage conversion.
- Offer acceptance.
- Candidate experience SLA.

## ST-WF-003 — Onboarding and first-day readiness

| Field | Definition |
| --- | --- |
| Classification | COMMON · UNDERSERVED |
| Implementation state | Partial |
| Purpose | Coordinate people, access, documents, training, equipment, location, and manager tasks for a ready first day. |
| Trigger | A person is hired, rehired, transferred, or assigned a new role/location. |

### Actors

- New worker
- Manager
- HR
- IT/identity administrator
- Trainer
- Operational product owners

### State path

`planned → in_progress → blocked → ready → started → completed → canceled`

### Required sequence

1. Instantiate a template based on role, location, worker type, and start date.
2. Create owned tasks for forms, policy acknowledgements, identity, equipment, training, medical/qualification, and local orientation.
3. Show blockers and due dates in one readiness view.
4. Allow quick create of missing references and delegated completion.
5. Receive outcomes from NexArr, TrainArr, MaintainArr/LoadArr, and RecordArr.
6. Escalate overdue or failed prerequisites.
7. Confirm start readiness or record an approved exception.
8. Close with an onboarding evidence package and feedback.

### Exception and recovery paths

- Start date changes, worker never starts, login cannot be provisioned, equipment unavailable, qualification pending, or required document declined.
- A task contains sensitive data not visible to the manager.

### Cross-product and external handoffs

- StaffArr → NexArr: account/access.
- StaffArr → TrainArr: assignments.
- StaffArr → RecordArr: documents/acknowledgements.
- StaffArr → MaintainArr/LoadArr: equipment or issue requests where modeled.

### Evidence and audit record

- Plan template/version.
- Task ownership and outcomes.
- Exceptions/approvals.
- Readiness confirmation and package.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Ready by start date.
- Overdue task count.
- First-day access failures.
- New-hire completion/feedback.

## ST-WF-004 — Internal transfer, promotion, or assignment change

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Apply effective-dated worker changes and reconcile access, training, scheduling, and responsibilities. |
| Trigger | An approved personnel change request reaches its effective date. |

### Actors

- HR administrator
- Current manager
- New manager
- Employee
- Access/training owners

### State path

`draft → impact_review → approval → scheduled → effective → reconciling → completed`

### Required sequence

1. Draft new position, manager, org unit, location, schedule, compensation, and effective date.
2. Preview downstream role, access, training, timekeeping, and readiness impacts.
3. Collect approvals and employee acknowledgement where required.
4. Schedule the change without rewriting current history.
5. At effective time, end old assignments and activate new assignments.
6. Trigger access, training, scheduling, and product-reference updates.
7. Reconcile partial failures and confirm the worker/new manager view.

### Exception and recovery paths

- Overlapping assignment, missing position/location, pay change not approved, required training not complete, or access change would remove critical coverage.
- Temporary concurrent assignment must remain active.

### Cross-product and external handoffs

- StaffArr → NexArr: lifecycle/access change.
- StaffArr → TrainArr: applicability recalculation.
- StaffArr → products: assignment/location update.

### Evidence and audit record

- Before/after snapshot.
- Impact preview.
- Approvals/acknowledgement.
- Downstream reconciliation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Changes effective on time.
- Partial failure rate.
- Access/training drift.
- Manager confirmation.

## ST-WF-005 — Offboarding and separation

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | End a workforce relationship while protecting evidence, continuity, property, and access. |
| Trigger | A resignation, termination, contract end, retirement, death, or no-show is recorded. |

### Actors

- HR administrator
- Manager
- NexArr administrator
- Asset/property owners
- Payroll/finance contact

### State path

`planned → in_progress → access_revoked → property_pending → completed → legal_hold`

### Required sequence

1. Record reason category, effective time, rehire eligibility, confidentiality, and approval.
2. Instantiate a role/location-specific offboarding checklist.
3. Inventory active assignments, open approvals/tasks, assets, inventory, documents, cases, service ownership, and access.
4. Transfer work and accountable ownership.
5. Schedule or immediately revoke login, sessions, roles, portals, and credentials through NexArr/product APIs.
6. Collect property and final time/expense/benefit information.
7. Apply retention/legal hold/privacy rules.
8. Close with unresolved items, approvals, and evidence package.

### Exception and recovery paths

- Immediate safety/security termination, legal hold, inaccessible worker, unreturned property, open incident, or the worker is last administrator/approver.
- Person remains an external contractor/customer contact after employment ends.

### Cross-product and external handoffs

- StaffArr → NexArr: suspend/revoke account.
- StaffArr → products: end assignments/delegate work.
- StaffArr → RecordArr/LedgArr or payroll bridge: final evidence/financial inputs.

### Evidence and audit record

- Separation approval and timing.
- Checklist results.
- Access revocation acknowledgements.
- Property/open-item disposition.
- Retention/hold decisions.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Access revoked by effective time.
- Open task/asset count.
- Offboarding cycle time.
- Rehire data quality.

## ST-WF-006 — Clock event to approved timesheet

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Turn raw punches and operational labor evidence into a reviewed period record. |
| Trigger | A worker clocks in/out, enters time, or an operational product submits labor evidence. |

### Actors

- Worker
- Manager
- Timekeeper
- Payroll administrator
- StaffArr

### State path

`open → exception → employee_review → manager_review → approved → exported → corrected`

### Required sequence

1. Validate worker, tenant, timezone, allowed location/device, and pay policy.
2. Record immutable raw clock event and derive work session.
3. Merge or compare operational labor evidence and allocations.
4. Detect missed punches, overlaps, meal/rest, overtime, schedule, and pay-code exceptions.
5. Let worker submit corrections with reason and evidence.
6. Manager/timekeeper reviews exceptions and period totals.
7. Worker and manager attest as policy requires.
8. Lock/export the approved timesheet and retain later correction workflow.

### Exception and recovery paths

- Offline punch arrives late, duplicate punch, clock across midnight/timezone, multiple concurrent assignments, disputed time, or manager unavailable.
- Policy changes after the work occurred.

### Cross-product and external handoffs

- Field Companion → StaffArr: clock/offline action.
- Products → StaffArr: labor evidence.
- StaffArr → LedgArr/external payroll: approved time packet.

### Evidence and audit record

- Raw events and source/device.
- Derived sessions and rule version.
- Corrections/approvals/attestations.
- Export and reconciliation result.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Exception rate.
- Approval timeliness.
- Late punch sync.
- Payroll reconciliation variance.

## ST-WF-007 — Leave request and return to work

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Request, evaluate, schedule, document, and close an absence while respecting privacy. |
| Trigger | A worker or manager submits planned/unplanned leave. |

### Actors

- Worker
- Manager
- HR/leave administrator
- StaffArr

### State path

`draft → submitted → information_required → approved → denied → in_progress → return_review → closed`

### Required sequence

1. Capture leave type, dates/partial days, contact preference, and minimum necessary reason/evidence.
2. Evaluate balance, eligibility, overlap, blackout, coverage, and protected-leave flags without making unsupported legal conclusions.
3. Route to manager and HR separately where confidentiality requires.
4. Approve, deny, request information, or propose dates.
5. Update availability/schedule and notify affected owners with privacy-safe status.
6. Track documentation and expected return.
7. Complete return-to-work steps, restrictions, or accommodation handoff.
8. Reconcile balances/time and close.

### Exception and recovery paths

- Insufficient balance, overlapping leave, emergency absence, intermittent leave, missing evidence, denied request, or return restrictions.
- Manager must not see medical details.

### Cross-product and external handoffs

- StaffArr → scheduling/timekeeping: availability and pay code.
- StaffArr → RecordArr: restricted evidence.
- StaffArr → Compliance Core: guided requirement questions where configured.

### Evidence and audit record

- Request and policy snapshot.
- Eligibility/approval decisions.
- Restricted evidence refs.
- Schedule/time impacts and return outcome.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Decision time.
- Coverage conflicts.
- Balance accuracy.
- Return-to-work completion.

## ST-WF-008 — Personnel incident to restriction and retraining

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Durable |
| Purpose | Capture an incident once and coordinate investigation, readiness, retraining, and recurrence monitoring. |
| Trigger | An incident is reported manually or by MaintainArr, RoutArr, LoadArr, AssurArr, or Field Companion. |

### Actors

- Reporter
- Supervisor
- HR/safety reviewer
- Affected worker
- TrainArr

### State path

`reported → triage → investigating → restricted → remediation → review → closed`

### Required sequence

1. Create the StaffArr incident with source record, people, location, severity, and immediate actions.
2. Protect confidential statements/medical/disciplinary material with need-to-know access.
3. Classify operational and personnel impacts.
4. Apply temporary readiness restriction or assignment block when authorized.
5. Route training-related context to TrainArr for remediation evaluation.
6. Collect evidence through RecordArr and track notes/actions.
7. Receive qualification/training outcome and update readiness/history.
8. Close with findings, corrective actions, recurrence links, and reporting projection.

### Exception and recovery paths

- Unknown person, anonymous report, immediate danger, conflicting accounts, legal hold, active workers-comp claim, or qualification remains suspended.
- Incident spans multiple people/locations/products.

### Cross-product and external handoffs

- Originating product → StaffArr: incident context.
- StaffArr → TrainArr: remediation request.
- StaffArr/TrainArr → RecordArr: evidence package.
- StaffArr → products: readiness change.

### Evidence and audit record

- Original report and source.
- Classifications/restrictions.
- Statements/evidence and access log.
- Remediation/qualification outcome.
- Closure and recurrence links.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to triage.
- Restriction latency.
- Remediation completion.
- Repeat incident rate.

## ST-WF-009 — Role creation, permission assignment, and review

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Create understandable cross-product roles and apply them with scoped authority. |
| Trigger | A tenant administrator creates/edits a role or assigns it to a person. |

### Actors

- Tenant permission administrator
- Manager
- System owner
- Access reviewer
- StaffArr

### State path

`draft → review → active → superseded → retired`

### Required sequence

1. Choose a job/task-oriented role name and accountable owner.
2. Select product action permissions from the catalog, not raw route names or internal IDs.
3. Define scope by org, location, team, assignment, or record relationship.
4. Preview effective access, conflicts, and separation-of-duties warnings.
5. Approve high-risk permissions and save a versioned role.
6. Assign to people with effective/expiry dates and reason.
7. Publish projections to products and reconcile acknowledgements.
8. Review usage and certify or remove stale access.

### Exception and recovery paths

- Unknown permission, conflicting deny/allow, circular scope, last administrator removal, unqualified worker, or product catalog unavailable.
- Temporary access should not be embedded permanently in a role.

### Cross-product and external handoffs

- Products → StaffArr: permission catalog.
- StaffArr → products: assignment/projection.
- StaffArr ↔ NexArr: account/access review context.

### Evidence and audit record

- Role versions.
- Permission/scope diff.
- Approvals and assignments.
- Projection/acknowledgement.
- Review decisions.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Role reuse.
- Direct-exception count.
- Projection drift.
- Overprivileged assignments.
- Review completion.

## ST-WF-010 — Performance review cycle

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Run fair, evidence-backed goals and performance conversations with calibration and development outcomes. |
| Trigger | HR publishes a review cycle or a manager starts an allowed check-in. |

### Actors

- Employee
- Manager
- HR
- Calibration reviewer

### State path

`planned → open → self_review → manager_review → calibration → released → acknowledged → closed`

### Required sequence

1. Define cycle population, dates, templates, competencies, rating policy, and visibility.
2. Snapshot manager/assignment relationships and resolve exceptions.
3. Collect employee self-review, goals, evidence, and feedback.
4. Manager completes review with required examples and development actions.
5. Run calibration with privacy controls and audit.
6. Approve and release the review to the employee.
7. Capture acknowledgement/disagreement without rewriting the review.
8. Create goals, development, training, or PIP actions and close the cycle.

### Exception and recovery paths

- Manager changed, employee on leave, missing reviewer, disputed rating, confidential feedback, or cycle reopened.
- Automated analytics must not make final employment decisions.

### Cross-product and external handoffs

- StaffArr → TrainArr: development/training assignments.
- StaffArr → RecordArr: review package where policy requires.
- StaffArr → ReportArr: governed aggregate metrics.

### Evidence and audit record

- Cycle/template/version.
- Reviews and supporting evidence.
- Calibration changes/reasons.
- Release/acknowledgement.
- Follow-up actions.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Completion by due date.
- Rating distribution with privacy safeguards.
- Goal follow-through.
- Dispute resolution time.

## ST-WF-011 — Compensation change and approval

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Propose an effective-dated pay change with budget, equity, authority, and payroll evidence. |
| Trigger | A manager/HR user initiates merit, promotion, adjustment, market, or correction change. |

### Actors

- Manager
- HR/compensation administrator
- Budget approver
- Payroll/finance reviewer

### State path

`draft → validation → approval → scheduled → effective → exported → rejected → reversed`

### Required sequence

1. Select person and change reason; retrieve current effective compensation.
2. Enter proposed amount/rate, currency, frequency, effective date, and affected assignments.
3. Validate range, minimums, authorization, budget, retroactivity, and pay-equity indicators.
4. Route through configured approvals with conflict-of-interest controls.
5. Notify the employee at the appropriate time and store the document in RecordArr.
6. Activate the new effective-dated profile.
7. Send a payroll/financial packet and reconcile acknowledgement.
8. Preserve correction/reversal rather than overwriting history.

### Exception and recovery paths

- Out-of-range proposal, unavailable budget, retroactive period closed, concurrent change, payroll mapping missing, or approver is subject of change.
- Confidential executive compensation requires restricted workflow.

### Cross-product and external handoffs

- StaffArr → LedgArr/external payroll: compensation packet.
- StaffArr → RecordArr: letter/evidence.
- StaffArr → ReportArr: restricted aggregate metrics.

### Evidence and audit record

- Before/after amounts and reason.
- Range/equity/budget checks.
- Approvals.
- Employee notice.
- Payroll acknowledgement.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Approval cycle time.
- Out-of-range rate.
- Payroll reconciliation.
- Retroactive adjustment volume.

## ST-WF-012 — Benefits enrollment and qualifying life event

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Enroll eligible people and dependents while preserving evidence and carrier reconciliation. |
| Trigger | An enrollment window opens or a qualifying life event is approved. |

### Actors

- Employee
- Benefits administrator
- Carrier/integration
- StaffArr

### State path

`open → employee_action → review → submitted → accepted → exception → closed`

### Required sequence

1. Determine eligibility, plan options, effective date, and required evidence.
2. Present plan comparison, costs, coverage, and beneficiary/dependent requirements.
3. Employee makes elections or waives coverage and attests accuracy.
4. Benefits administrator reviews exceptions and life-event evidence.
5. Approve and create effective-dated enrollment/dependent/beneficiary records.
6. Export to carrier/payroll and reconcile confirmation/errors.
7. Notify employee of accepted coverage and unresolved items.
8. Close the event with evidence retention.

### Exception and recovery paths

- Dependent verification missing, duplicate coverage, event outside window, carrier rejects mapping, or employee does not respond.
- Sensitive health information must be minimized and segregated.

### Cross-product and external handoffs

- StaffArr ↔ RecordArr: evidence/documents.
- StaffArr ↔ carrier/payroll integration.
- StaffArr → ReportArr: privacy-safe enrollment metrics.

### Evidence and audit record

- Eligibility snapshot.
- Election/waiver and attestation.
- Evidence review.
- Carrier/payroll transaction and reconciliation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Enrollment completion.
- Carrier rejection rate.
- Unresolved evidence.
- Time to coverage confirmation.

## ST-WF-013 — Worker profile update request

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Durable |
| Purpose | Let workers request corrections or updates with transparent review and downstream effects. |
| Trigger | A worker submits a change to an editable or review-required field. |

### Actors

- Worker
- Manager when appropriate
- HR reviewer
- StaffArr

### State path

`draft → submitted → information_required → approved → rejected → applied → canceled`

### Required sequence

1. Show which fields are directly editable, review-required, or restricted.
2. Capture proposed value, effective date, reason, and supporting evidence.
3. Validate format and identify downstream implications.
4. Route to the correct reviewer without exposing unrelated data.
5. Approve, reject with useful reason, or request more information.
6. Apply the effective-dated change and notify downstream systems.
7. Show the worker final status and correction/appeal path.

### Exception and recovery paths

- Identity conflict, duplicate identifier, address affects tax/payroll jurisdiction, evidence missing, or request becomes stale after another update.
- Sensitive fields require HR-only review.

### Cross-product and external handoffs

- StaffArr → NexArr: login/contact update when NexArr-owned.
- StaffArr → payroll/benefits/products: relevant changed references.
- StaffArr → RecordArr: evidence.

### Evidence and audit record

- Original/proposed values.
- Review decisions and reasons.
- Effective update and downstream acknowledgements.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Decision time.
- Self-service completion.
- Rejection reasons.
- Downstream sync failures.

## ST-WF-014 — Qualification-aware staffing and readiness check

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Partial |
| Purpose | Find and assign people who are available, permitted, and qualified for work. |
| Trigger | A manager staffs a shift, task, inspection, route, warehouse operation, or maintenance assignment. |

### Actors

- Manager/dispatcher
- StaffArr
- TrainArr
- Owning operational product

### State path

`requested → candidates_found → warning → assigned → blocked → overridden → canceled`

### Required sequence

1. Define work, role, location, time, and required competencies/qualifications.
2. Retrieve candidate people from active assignments and scoped authority.
3. Check availability, leave, schedule/rest, restrictions, certification/qualification, and conflicts.
4. Explain why candidates are eligible, warning, or blocked.
5. Assign the selected person through the owning product.
6. Record override approval when policy allows.
7. Monitor readiness changes before execution.

### Exception and recovery paths

- No eligible candidate, qualification data stale, worker becomes unavailable, assignment exceeds rest rules, or manager lacks scope.
- Emergency assignment requires temporary authority and post-review.

### Cross-product and external handoffs

- StaffArr ↔ TrainArr: qualification/readiness.
- StaffArr ↔ operational product: assignment.
- StaffArr → Compliance Core: gate context where required.

### Evidence and audit record

- Requirements and candidate snapshot.
- Eligibility explanations.
- Assignment/override.
- Readiness changes.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Fill rate.
- Time to qualified assignment.
- Override rate.
- Last-minute disqualification.

## ST-WF-015 — Scheduled workforce/payroll export and reconciliation

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Deliver approved workforce/time/compensation data repeatably and prove what the receiver accepted. |
| Trigger | A configured export schedule runs or an authorized user triggers a run. |

### Actors

- Payroll/HR administrator
- StaffArr worker
- External payroll/benefits system

### State path

`scheduled → validating → ready → delivered → partial → failed → reconciled`

### Required sequence

1. Select an approved preset, population, period, fields, format, and destination.
2. Snapshot source records and validate completeness, mappings, closed periods, and permissions.
3. Generate preview totals and exception list.
4. Approve or automatically release according to policy.
5. Transmit through the configured secure channel.
6. Capture provider acknowledgement, row-level errors, and reconciliation totals.
7. Correct errors through source workflows and issue a delta/replacement run.
8. Retain manifest, hash, and delivery notification.

### Exception and recovery paths

- Destination unavailable, mapping drift, duplicate run, unapproved timesheet, changed data after snapshot, or partial provider acceptance.
- Export contains fields not permitted for destination/purpose.

### Cross-product and external handoffs

- StaffArr → LedgArr/external payroll/benefits provider.
- StaffArr → RecordArr: manifest/package.
- StaffArr → ReportArr: run metrics.

### Evidence and audit record

- Preset/version.
- Source snapshot and totals.
- File/package hash.
- Delivery and provider response.
- Corrections/deltas.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- On-time delivery.
- Rejected rows.
- Reconciliation variance.
- Manual correction rate.



## Workflow definition of done

A workflow is releasable only when its happy path, every listed exception, dependency outage, duplicate/retry, stale/concurrent update, permission denial, cancellation, correction/void, archival/retention, import/API/event path, notification, audit trail, reporting projection, mobile behavior, and professional printable output have automated and human-usable acceptance coverage.
