# NexArr — IAM Workflow Catalog

[Feature catalog](./FEATURESET.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md)

## Workflow contract

This document defines the end-to-end business state machines for NexArr. A workflow is not complete because a page or endpoint exists: the trigger, authority, state transitions, validation, handoffs, evidence, exceptions, retries, conflict behavior, notifications, reporting, and final source-of-truth commit must all work together.

## Ownership rule

NexArr is the platform identity and trust plane. It authenticates people and machine clients, manages tenant membership and secure sessions, launches users into the suite, protects platform administration, and provides auditable credentials for cross-product integrations. It does not decide operational authority inside StaffArr, MaintainArr, RoutArr, or any other domain product.

- Employment/personnel truth, reporting lines, positions, teams, or internal locations; StaffArr owns those records.
- Domain permissions such as closing a work order or releasing a quality hold; each product enforces its own actions using StaffArr-backed assignments.
- Compliance applicability, legal meaning, or evidence sufficiency; Compliance Core owns those decisions.
- Operational business records, documents, reports, or finance records.
- Fixed-suite product availability for ordinary tenants; product actions remain permission-scoped and platform-admin boundaries remain separate.

## Workflow index

| Workflow ID | Workflow | Class | State | Trigger |
| --- | --- | --- | --- | --- |
| NX-WF-001 | User invitation and account activation | CURRENT · COMMON | Durable | An authorized administrator invites a user or StaffArr requests login provisioning. |
| NX-WF-002 | Interactive sign-in and tenant launch | CURRENT · COMMON | Durable | A user opens the login page or follows an authorized deep link. |
| NX-WF-003 | Password reset and account recovery | CURRENT · COMMON | Durable | A user requests recovery or an administrator starts a verified reset. |
| NX-WF-004 | External IdP connection and staged cutover | COMMON · DEMOCRATIZE | Target | A tenant administrator starts SAML or OIDC configuration. |
| NX-WF-005 | Joiner, mover, and leaver automation | COMMON · DEMOCRATIZE | Partial | StaffArr emits a hire, assignment, transfer, leave, or separation event. |
| NX-WF-006 | Session investigation and revocation | CURRENT · COMMON | Durable | A user reports suspicious activity or an administrator opens a session record. |
| NX-WF-007 | Service client registration and token rotation | CURRENT · COMMON | Durable | An authorized platform or integration administrator registers a service client or rotates a credential. |
| NX-WF-008 | Product launch and secure handoff | CURRENT | Durable | A user chooses a product or follows a cross-product link. |
| NX-WF-009 | Tenant creation, suspension, export, and closure | CURRENT · COMMON | Partial | A platform administrator creates a tenant or an authorized closure/suspension request is approved. |
| NX-WF-010 | Access review and certification campaign | UNDERSERVED · DEMOCRATIZE | Target | A scheduled campaign starts or a high-risk event triggers an ad hoc review. |
| NX-WF-011 | Temporary privileged access and break-glass use | UNDERSERVED · DEMOCRATIZE | Target | A user requests elevated access or invokes a pre-approved break-glass path. |
| NX-WF-012 | Integration credential and mapping administration | CURRENT · COMMON | Durable | A tenant administrator creates or changes an integration connection. |
| NX-WF-013 | Smart Import classification, review, and commit | CURRENT · UNDERSERVED | Durable | An authorized user uploads one or more files to Smart Import. |
| NX-WF-014 | Identity incident response and compromised credential containment | UNDERSERVED · DEMOCRATIZE | Target | A security signal, user report, or administrator flags an identity as compromised. |

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

## NX-WF-001 — User invitation and account activation

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Create a secure account and connect it to the correct tenant and optional StaffArr person. |
| Trigger | An authorized administrator invites a user or StaffArr requests login provisioning. |

### Actors

- Tenant administrator
- StaffArr administrator
- Invitee
- NexArr

### State path

`draft → invited → opened → verified → active → expired → revoked`

### Required sequence

1. Validate inviter authority and target tenant.
2. Find or create the platform user without duplicating an existing identity.
3. Create pending membership and optional StaffArr person link.
4. Issue a single-use, time-limited activation challenge.
5. Invitee verifies identity, sets or binds credentials/factors, and accepts tenant terms.
6. Activate membership, create the first session, and emit account/membership events.
7. Return the caller to the intended suite surface.

### Exception and recovery paths

- Email or identity already belongs to another user.
- Invitation expired, revoked, or opened in the wrong tenant context.
- StaffArr person is inactive or already linked to a different user.
- Required MFA enrollment cannot be completed.

### Cross-product and external handoffs

- StaffArr → NexArr: provision login request.
- NexArr → StaffArr: user-link and account-state result.
- NexArr → RecordArr/ReportArr: audit projection where configured.

### Evidence and audit record

- Invitation record and actor.
- Verification and factor-enrollment events.
- Membership and person-link result.
- Terms/policy acceptance.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Invitation acceptance rate.
- Median time to activation.
- Duplicate/conflict rate.
- MFA enrollment completion.

## NX-WF-002 — Interactive sign-in and tenant launch

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Authenticate a user, establish tenant context, and launch the suite safely. |
| Trigger | A user opens the login page or follows an authorized deep link. |

### Actors

- User
- NexArr
- External IdP when configured

### State path

`initiated → challenged → authenticated → tenant_selected → launched → denied`

### Required sequence

1. Resolve the requested tenant and allowed authentication methods.
2. Validate credentials or external identity assertion.
3. Apply account, tenant, factor, session, and risk policy.
4. Require step-up or recovery when necessary.
5. Create a tenant-scoped session and record sign-in evidence.
6. Validate return URL/callback against allowlists.
7. Launch into the suite or requested product route with no sensitive data in the URL.

### Exception and recovery paths

- Unknown tenant, suspended account, locked credential, invalid assertion, unavailable IdP, unsafe return URL, or excessive risk.
- User belongs to multiple tenants and no tenant was selected.
- A deep link targets an action the user cannot perform.

### Cross-product and external handoffs

- NexArr → product: authenticated tenant/user context.
- Product → StaffArr/local policy: action authorization after launch.

### Evidence and audit record

- Authentication method and result.
- Policy/risk decision.
- Session and tenant context.
- Launch target and correlation ID.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Sign-in success rate.
- MFA challenge rate.
- IdP failure rate.
- Time from sign-in to usable page.

## NX-WF-003 — Password reset and account recovery

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Restore access without revealing account existence or bypassing tenant policy. |
| Trigger | A user requests recovery or an administrator starts a verified reset. |

### Actors

- User
- Tenant administrator
- NexArr

### State path

`requested → challenge_sent → verified → completed → failed → revoked`

### Required sequence

1. Accept a generic recovery request.
2. Apply rate limits and risk checks.
3. Send a single-use recovery challenge through an approved channel.
4. Verify the challenge and additional factors as policy requires.
5. Reset or rebind credentials.
6. Revoke selected or all prior sessions and recovery tokens.
7. Notify the account owner and record the recovery event.

### Exception and recovery paths

- No verified recovery channel.
- Suspicious request, stale token, or repeated failures.
- Federated-only account must recover at its IdP.
- Administrator reset requires dual approval under policy.

### Cross-product and external handoffs

- NexArr ↔ external email/SMS provider.
- NexArr → ReportArr/security monitoring: recovery event.

### Evidence and audit record

- Request metadata without secret values.
- Verification result.
- Credential change and session revocation.
- Administrator reason/approval where used.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Completion rate.
- Abuse/rate-limit blocks.
- Mean recovery time.
- Post-recovery suspicious sign-ins.

## NX-WF-004 — External IdP connection and staged cutover

| Field | Definition |
| --- | --- |
| Classification | COMMON · DEMOCRATIZE |
| Implementation state | Target |
| Purpose | Connect a tenant-owned identity provider without locking out administrators. |
| Trigger | A tenant administrator starts SAML or OIDC configuration. |

### Actors

- Tenant administrator
- NexArr
- External IdP

### State path

`draft → validating → test → pilot → active → degraded → rolled_back`

### Required sequence

1. Create a draft connection with issuer, endpoints, keys, and claim mappings.
2. Validate metadata, signatures, callback URLs, time skew, and required claims.
3. Run test sign-ins for named test users while local login remains available.
4. Preview account linking and duplicate/conflict outcomes.
5. Approve staged activation for a pilot group or domain.
6. Monitor failures and preserve a break-glass local path.
7. Complete cutover, schedule certificate rotation, and document rollback.

### Exception and recovery paths

- Invalid signature/certificate, duplicate email, missing immutable subject, domain mismatch, or IdP outage.
- Administrator attempts to disable the only tested recovery path.
- Claim mapping would move users to the wrong tenant.

### Cross-product and external handoffs

- NexArr ↔ IdP metadata and authentication endpoints.
- NexArr → tenant administrators: health and rollover notifications.

### Evidence and audit record

- Configuration versions without private key disclosure.
- Test results and mapped claims.
- Activation approvals.
- Certificate rotation history.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to configure.
- Test failure causes.
- Federated sign-in success.
- Lockout incidents.

## NX-WF-005 — Joiner, mover, and leaver automation

| Field | Definition |
| --- | --- |
| Classification | COMMON · DEMOCRATIZE |
| Implementation state | Partial |
| Purpose | Align account and access lifecycle with StaffArr employment/person changes. |
| Trigger | StaffArr emits a hire, assignment, transfer, leave, or separation event. |

### Actors

- HR administrator
- Manager
- Access reviewer
- StaffArr
- NexArr
- Product services

### State path

`received → planned → approval_required → applying → completed → partial_failure → reconciled`

### Required sequence

1. Receive the StaffArr lifecycle event idempotently.
2. Resolve linked user and evaluate lifecycle rules in preview mode.
3. Create, modify, suspend, or schedule membership changes.
4. Request approvals for privileged or exceptional access.
5. Apply product action-role changes through owning permission APIs.
6. Revoke sessions/tokens and external access when separation becomes effective.
7. Produce a completion report with unresolved dependencies.

### Exception and recovery paths

- Person has no linked user, multiple candidate users, future-dated transfer, legal hold, active emergency duty, or failed downstream revocation.
- Leaver still owns service clients or integrations.
- A role change would remove the last tenant administrator.

### Cross-product and external handoffs

- StaffArr → NexArr: lifecycle event.
- NexArr → product authorization services: assignment changes.
- NexArr → RecordArr: exported evidence package when requested.

### Evidence and audit record

- Source StaffArr event.
- Rule evaluation and preview.
- Approvals and changes by system.
- Failures, retries, and final reconciliation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Automation coverage.
- Time to provision/deprovision.
- Orphan account count.
- Partial-failure resolution time.

## NX-WF-006 — Session investigation and revocation

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Let users and administrators safely inspect and terminate active access. |
| Trigger | A user reports suspicious activity or an administrator opens a session record. |

### Actors

- User
- Tenant administrator
- Platform administrator
- NexArr

### State path

`active → revocation_requested → revoked → expired → investigating`

### Required sequence

1. List current and recent sessions with safe device, time, tenant, and location context.
2. Select one session, all sessions, or all except the current session.
3. Require step-up authentication for high-impact revocation.
4. Revoke tokens and mark the session terminated.
5. Notify the user and dependent services.
6. Open an investigation record if suspicious activity is alleged.

### Exception and recovery paths

- Current session cannot perform step-up.
- Session already expired.
- Downstream token cache does not acknowledge revocation.
- Location/device data is unavailable or privacy-restricted.

### Cross-product and external handoffs

- NexArr → products/API gateway: revocation propagation.
- NexArr → ReportArr/security analytics: suspicious-session event.

### Evidence and audit record

- Session metadata.
- Revocation actor/reason.
- Propagation acknowledgements.
- Investigation links.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Revocation propagation latency.
- Suspicious session volume.
- User self-service percentage.

## NX-WF-007 — Service client registration and token rotation

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Provision least-privilege machine access and rotate it without downtime. |
| Trigger | An authorized platform or integration administrator registers a service client or rotates a credential. |

### Actors

- Integration administrator
- Platform administrator
- Service owner
- NexArr

### State path

`draft → pending_approval → active → rotating → revoked → expired → compromised`

### Required sequence

1. Record owner, purpose, environments, callback/network constraints, and requested scopes.
2. Review and approve high-risk scopes.
3. Issue a secret or certificate once and store only protected verifier/metadata.
4. Test access against a non-destructive endpoint.
5. Activate the credential and monitor last-used/usage patterns.
6. Rotate using an overlap window and update the consumer.
7. Revoke the old credential after confirmation; escalate abandoned clients.

### Exception and recovery paths

- No accountable owner, excessive scopes, consumer cannot rotate, credential exposed, or old credential remains in use.
- Rotation would interrupt a critical integration.
- Client has cross-tenant scope.

### Cross-product and external handoffs

- NexArr ↔ consuming product/integration.
- NexArr → ReportArr/security monitoring: usage and anomaly events.

### Evidence and audit record

- Owner/purpose/scope approval.
- Issuance fingerprint and expiry.
- Rotation confirmation.
- Usage and revocation history.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Credentials past expiry.
- Mean credential age.
- Unused client count.
- Rotation success without outage.

## NX-WF-008 — Product launch and secure handoff

| Field | Definition |
| --- | --- |
| Classification | CURRENT |
| Implementation state | Durable |
| Purpose | Carry authenticated identity and tenant context into a product without leaking reusable credentials. |
| Trigger | A user chooses a product or follows a cross-product link. |

### Actors

- User
- NexArr
- Target product

### State path

`requested → issued → redeemed → expired → rejected`

### Required sequence

1. Validate active session, tenant membership, and fixed-suite product availability.
2. Validate the requested return route and target product callback.
3. Create a short-lived single-use handoff code bound to user, tenant, product, and nonce.
4. Redirect to the product callback.
5. Target product redeems the code server-to-server and establishes local context.
6. Target product enforces its own action permissions and opens the requested route.
7. Audit redemption, rejection, expiry, and replay attempts.

### Exception and recovery paths

- Unsafe callback, expired/replayed code, tenant mismatch, inactive user, unavailable product, or denied target action.
- Platform administrator attempts to use tenant product surfaces outside permitted support delegation.

### Cross-product and external handoffs

- NexArr → target product: one-time handoff.
- Target product → NexArr: redemption and session validation.

### Evidence and audit record

- Requested route and product.
- Code issuance/redemption metadata.
- Failure reason category.
- Correlation ID.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Launch success.
- Redeem latency.
- Replay attempts.
- Invalid callback blocks.

## NX-WF-009 — Tenant creation, suspension, export, and closure

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Manage the tenant lifecycle without losing evidence or leaving active integrations behind. |
| Trigger | A platform administrator creates a tenant or an authorized closure/suspension request is approved. |

### Actors

- Platform administrator
- Tenant owner
- NexArr
- All product services
- RecordArr

### State path

`provisioning → active → suspended → closure_planned → exporting → closing → closed → restored`

### Required sequence

1. Create tenant identity, primary administrator, default policies, and suite launch configuration.
2. Initialize product data planes and integration/audit settings.
3. For suspension, block new sessions and high-risk writes while preserving required access paths.
4. For closure, inventory memberships, service clients, integrations, records, retention, legal holds, and outstanding workflows.
5. Generate export and closure plan for review.
6. Revoke credentials and integrations in controlled order.
7. Archive or delete according to retention/legal obligations and issue completion evidence.

### Exception and recovery paths

- Last administrator missing, legal hold active, unpaid/contractual restriction, product export failure, or dependent external integration remains active.
- Closure is canceled during the reversible window.

### Cross-product and external handoffs

- NexArr ↔ all products: tenant lifecycle commands and acknowledgements.
- Products → RecordArr: export packages.
- ReportArr → tenant owner: closure status report.

### Evidence and audit record

- Approvals and reason.
- Data-plane initialization results.
- Dependency inventory.
- Export manifests and revocation acknowledgements.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Provisioning completion time.
- Closure unresolved dependencies.
- Export completeness.
- Credential revocation completeness.

## NX-WF-010 — Access review and certification campaign

| Field | Definition |
| --- | --- |
| Classification | UNDERSERVED · DEMOCRATIZE |
| Implementation state | Target |
| Purpose | Regularly prove that human and machine access remains appropriate. |
| Trigger | A scheduled campaign starts or a high-risk event triggers an ad hoc review. |

### Actors

- Campaign owner
- Managers
- System owners
- Tenant administrators
- NexArr
- StaffArr

### State path

`planned → collecting → in_review → remediating → exception_open → closed`

### Required sequence

1. Define scope by tenant, product actions, roles, privileged accounts, service clients, or stale use.
2. Snapshot access, ownership, last use, StaffArr assignment, and qualification context.
3. Assign reviewers with conflict-of-interest controls.
4. Present keep, modify, revoke, delegate, or exception decisions with explanations.
5. Apply approved changes through owning systems.
6. Reconcile failures and require evidence for exceptions.
7. Close the campaign with coverage and residual-risk reporting.

### Exception and recovery paths

- Reviewer unavailable, account has no manager/owner, access is inherited through multiple paths, or revocation breaks a critical process.
- Reviewer attempts to certify own privileged access without secondary review.

### Cross-product and external handoffs

- StaffArr → NexArr: manager/assignment context.
- NexArr ↔ products: effective action access and remediation.
- NexArr → RecordArr/ReportArr: campaign evidence and metrics.

### Evidence and audit record

- Campaign definition and snapshot.
- Reviewer decisions/comments.
- Remediation acknowledgements.
- Exceptions and closure report.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Coverage.
- Revocation rate.
- Overdue reviews.
- Unowned privileged identities.
- Remediation cycle time.

## NX-WF-011 — Temporary privileged access and break-glass use

| Field | Definition |
| --- | --- |
| Classification | UNDERSERVED · DEMOCRATIZE |
| Implementation state | Target |
| Purpose | Grant emergency or time-limited authority with strong controls and automatic expiry. |
| Trigger | A user requests elevated access or invokes a pre-approved break-glass path. |

### Actors

- Requester
- Approver
- Security reviewer
- NexArr
- Target product

### State path

`requested → approved → active → expired → revoked → post_review`

### Required sequence

1. Capture reason, scope, target action, tenant, start/end time, and incident/change reference.
2. Evaluate requester eligibility and separation-of-duties constraints.
3. Obtain required approval or validate break-glass conditions.
4. Issue short-lived elevation bound to the approved scope.
5. Notify stakeholders and prominently indicate elevated mode.
6. Log privileged actions and automatically expire/revoke access.
7. Require post-use attestation and review.

### Exception and recovery paths

- No approver available, request conflicts with own approval, active incident requires extension, or revocation propagation fails.
- Emergency account is used outside declared scope.

### Cross-product and external handoffs

- NexArr → target product: temporary authorization claim/reference.
- Target product → NexArr: privileged action audit.
- NexArr → ReportArr/RecordArr: review package.

### Evidence and audit record

- Request and approvals.
- Elevation grant and expiry.
- Actions performed.
- Post-use review and findings.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Temporary grants by reason.
- Expired-on-time percentage.
- Out-of-scope attempts.
- Post-review completion.

## NX-WF-012 — Integration credential and mapping administration

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Connect an external provider with controlled secrets, mappings, sync health, and recoverable failures. |
| Trigger | A tenant administrator creates or changes an integration connection. |

### Actors

- Tenant integration administrator
- NexArr
- External provider
- Owning product

### State path

`draft → testing → mapping → dry_run → active → degraded → disabled`

### Required sequence

1. Choose provider and owning product use case.
2. Enter credentials through a protected secret flow; never echo stored secrets.
3. Test authentication and least-privilege access.
4. Map external identifiers to tenant/product records with conflict review.
5. Configure sync direction, schedule, rate limits, and failure policy.
6. Run an initial dry run and review proposed changes.
7. Activate, monitor provider health, rotate credentials, and resolve unmapped records.

### Exception and recovery paths

- Authentication failure, insufficient provider scope, duplicate external mapping, rate limit, schema drift, or target record missing.
- Connection is shared by workflows with incompatible ownership.

### Cross-product and external handoffs

- NexArr ↔ external provider.
- NexArr → owning product: normalized intake/event.
- Owning product → NexArr: mapping/commit result.

### Evidence and audit record

- Connection owner and purpose.
- Credential rotation metadata.
- Mapping decisions.
- Sync runs, errors, retries, and manual overrides.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Sync success rate.
- Unmapped record count.
- Provider health time.
- Credential age.
- Manual correction rate.

## NX-WF-013 — Smart Import classification, review, and commit

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Durable |
| Purpose | Turn an uploaded file into reviewable, product-owned record proposals without silent AI commits. |
| Trigger | An authorized user uploads one or more files to Smart Import. |

### Actors

- Uploader
- Reviewer
- NexArr intake service
- AI assistant
- Target product

### State path

`uploaded → classified → extracted → matching → review → planned → committing → completed → partial_failure`

### Required sequence

1. Create a batch and securely store/import file references.
2. Classify likely product, record type, and sensitivity with confidence and reasons.
3. Extract candidate fields and normalize obvious formats.
4. Search target products for match and duplicate candidates.
5. Present proposed create/update/link/ignore decisions for human review.
6. Build a dependency-aware commit plan with permission and validation checks.
7. Execute approved steps idempotently against product APIs.
8. Record success, partial failure, rollback/correction options, and final audit.

### Exception and recovery paths

- Ambiguous product ownership, unsupported file, low confidence, duplicate candidates, missing permission, validation failure, or downstream outage.
- File contains secrets or malicious prompt/instruction content.
- A proposed change conflicts with a newer record version.

### Cross-product and external handoffs

- NexArr → RecordArr: secure file/evidence storage as appropriate.
- NexArr ↔ target products: match, validate, and commit APIs.
- NexArr → ReportArr: intake metrics.

### Evidence and audit record

- Source file hash and uploader.
- Model/tool versions and confidence.
- Reviewer decisions.
- Commit plan/results and correction links.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to reviewed proposal.
- Auto-match precision.
- Reviewer override rate.
- Commit success.
- Duplicate prevention.

## NX-WF-014 — Identity incident response and compromised credential containment

| Field | Definition |
| --- | --- |
| Classification | UNDERSERVED · DEMOCRATIZE |
| Implementation state | Target |
| Purpose | Contain suspected account or service-client compromise while preserving investigation evidence. |
| Trigger | A security signal, user report, or administrator flags an identity as compromised. |

### Actors

- Security responder
- Tenant administrator
- Affected user/service owner
- NexArr

### State path

`reported → triaged → contained → investigating → recovering → monitoring → closed`

### Required sequence

1. Open an incident and classify affected identities, tenants, sessions, factors, and service credentials.
2. Revoke active sessions/tokens and block new authentication as policy requires.
3. Preserve relevant audit and integration evidence.
4. Notify the affected owner through a trusted alternate channel.
5. Reset/rebind credentials and verify clean recovery.
6. Inspect dependent mappings, API activity, and privileged actions.
7. Restore access gradually and close with root cause and control changes.

### Exception and recovery paths

- Signal is false positive, owner unavailable, critical service cannot be stopped, evidence retention conflict, or attacker changed recovery data.
- Cross-tenant exposure is suspected.

### Cross-product and external handoffs

- NexArr → products: account/client containment event.
- Products → NexArr/RecordArr: correlated activity evidence.
- NexArr → ReportArr: incident metrics.

### Evidence and audit record

- Detection source.
- Containment actions/timestamps.
- Affected resources and audit links.
- Recovery verification and lessons learned.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to contain.
- Sessions/tokens revoked.
- Affected products/tenants.
- Recurrence.
- False-positive rate.



## Workflow definition of done

A workflow is releasable only when its happy path, every listed exception, dependency outage, duplicate/retry, stale/concurrent update, permission denial, cancellation, correction/void, archival/retention, import/API/event path, notification, audit trail, reporting projection, mobile behavior, and professional printable output have automated and human-usable acceptance coverage.
