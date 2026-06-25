# CustomArr — CRM Workflow Catalog

[Feature catalog](./FEATURESET.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md)

## Workflow contract

This document defines the end-to-end business state machines for CustomArr. A workflow is not complete because a page or endpoint exists: the trigger, authority, state transitions, validation, handoffs, evidence, exceptions, retries, conflict behavior, notifications, reporting, and final source-of-truth commit must all work together.

## Ownership rule

CustomArr is the tenant customer and CRM system of record. It owns customer accounts and hierarchies, contacts and authorizations, addresses/locations in the customer context, commercial relationship history, requirements/preferences, leads, opportunities, proposals, agreements, customer onboarding, service cases, tasks/activities, health/success, and customer portal access. OrdArr owns order lifecycle; LedgArr owns invoices/payments; RecordArr owns files.

- Order/request lifecycle, fulfillment handoffs, or order completion; OrdArr owns them.
- Supplier/vendor truth; SupplyArr owns procurement external parties.
- Invoices, payments, tax, collections accounting, or general ledger; LedgArr owns financial truth while CustomArr may show references/summaries.
- Documents/files; RecordArr owns storage and controlled documents.
- Transportation, warehouse, maintenance, quality, or compliance execution.
- Platform authentication; NexArr owns login/session, though CustomArr may request scoped portal access.

## Workflow index

| Workflow ID | Workflow | Class | State | Trigger |
| --- | --- | --- | --- | --- |
| CU-WF-001 | Lead capture, dedupe, routing, and acknowledgement | CURRENT · COMMON | Durable | A web form, portal, integration, import, referral, email, or user creates a lead. |
| CU-WF-002 | Lead qualification and conversion | CURRENT · COMMON | Durable | A lead owner begins qualification. |
| CU-WF-003 | Create customer account, hierarchy, contacts, and locations | CURRENT · COMMON | Durable | A lead converts, an order/import requires a customer, or an authorized user creates one. |
| CU-WF-004 | Opportunity pipeline, stage validation, and forecast | CURRENT · COMMON | Durable | A qualified lead/customer need becomes an opportunity. |
| CU-WF-005 | Proposal, approval, agreement, and order handoff | CURRENT · COMMON | Partial | An opportunity is ready for a formal proposal or agreement. |
| CU-WF-006 | Customer requirements and preference change | CURRENT · UNDERSERVED | Durable | A customer/internal user submits or discovers a new/changed requirement. |
| CU-WF-007 | Customer onboarding and mutual action plan | CURRENT · UNDERSERVED | Durable | A customer agreement/order requires onboarding or a relationship is approved for activation. |
| CU-WF-008 | Customer portal invitation and access administration | CURRENT · COMMON | Durable | An authorized internal user invites a customer contact or contact requests access. |
| CU-WF-009 | Customer case intake, SLA, escalation, and resolution | CURRENT · COMMON | Durable | Customer or employee creates a case from portal, email, phone, form, or linked operational event. |
| CU-WF-010 | Customer complaint to quality and corrective action | CURRENT · UNDERSERVED | Partial | A case is classified as a potential quality complaint. |
| CU-WF-011 | Customer health review, success plan, and churn risk | CURRENT · COMMON | Durable | A scheduled review occurs or health signals change materially. |
| CU-WF-012 | Renewal, expansion, contraction, or termination | COMMON · UNDERSERVED | Partial | Agreement/commitment enters a renewal window or customer requests change/termination. |
| CU-WF-013 | Customer duplicate review and merge | CURRENT · COMMON | Durable | Import, create, integration, or scheduled matching identifies candidates. |
| CU-WF-014 | Customer import with mapping, validation, and rollback | CURRENT · COMMON | Durable | An authorized user uploads customer/contact/pipeline data or runs an integration migration. |
| CU-WF-015 | Field visit and offline account update | UNDERSERVED | Target | A field user opens an assigned visit/task or loses connectivity at a customer site. |
| CU-WF-016 | Customer audit/data-room package | UNDERSERVED · DEMOCRATIZE | Target | An authorized user or customer requests an onboarding, contract, service, audit, or data-room package. |

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

## CU-WF-001 — Lead capture, dedupe, routing, and acknowledgement

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Create a lead from any approved channel and route it quickly without duplicates. |
| Trigger | A web form, portal, integration, import, referral, email, or user creates a lead. |

### Actors

- Prospect
- Sales/BD user
- CustomArr

### State path

`received → matching → assigned → contacted → qualified → disqualified → converted`

### Required sequence

1. Capture source, campaign/referral, contact/account clues, need, consent/preferences, urgency, and submitted evidence.
2. Validate spam/security and normalize contact/company data.
3. Search existing leads, contacts, and accounts using configured duplicate rules.
4. Merge/link or create a new lead with source provenance.
5. Apply assignment/territory/capacity/SLA rules and explain the owner decision.
6. Acknowledge receipt through the permitted channel.
7. Create first-response task and track SLA.
8. Record qualification/disqualification outcome and source attribution.

### Exception and recovery paths

- Possible duplicate, no consent for requested channel, invalid contact, conflicting territory, owner unavailable, spam, or sensitive data submitted.
- High-priority inquiry requires immediate escalation.

### Cross-product and external handoffs

- Public site/portal → CustomArr.
- CustomArr → NexArr/notification provider.
- CustomArr → ReportArr: attribution/SLA.

### Evidence and audit record

- Submission/source/consent.
- Match decision.
- Routing rule/version.
- Acknowledgement and contact attempts.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Speed to lead.
- Duplicate rate.
- Contact rate.
- Qualification rate.
- Source conversion.

## CU-WF-002 — Lead qualification and conversion

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Determine fit and convert valid demand into account/contact/opportunity without rekeying. |
| Trigger | A lead owner begins qualification. |

### Actors

- Sales/BD user
- Prospect
- CustomArr

### State path

`new → working → information_required → qualified → nurture → disqualified → converted`

### Required sequence

1. Confirm person/account identity, need, timing, authority/stakeholders, scope, location, and preferred communication.
2. Ask configurable eligibility/requirement questions and request missing facts.
3. Check existing customer relationship, restrictions, duplicates, and conflicts.
4. Record qualification evidence and next step.
5. Disqualify/nurture with reason or convert.
6. On conversion, create/link customer, contacts, addresses, requirements, opportunity, and activities atomically.
7. Assign owners/team and create follow-up plan.
8. Preserve original lead and source attribution.

### Exception and recovery paths

- Prospect is supplier not customer, duplicate existing opportunity, requirements unknown, ineligible/blocked service, no response, or consent withdrawn.
- One lead relates to multiple sites/opportunities.

### Cross-product and external handoffs

- CustomArr ↔ Compliance Core: guided facts/eligibility.
- CustomArr → OrdArr later via opportunity/agreement.
- RecordArr: submitted documents.

### Evidence and audit record

- Qualification criteria/version.
- Answers/evidence.
- Conversion mapping.
- Disqualification reason.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Qualification cycle.
- Conversion rate.
- Rework/duplicate conversion.
- Nurture reactivation.

## CU-WF-003 — Create customer account, hierarchy, contacts, and locations

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Establish the canonical customer relationship record and hierarchy. |
| Trigger | A lead converts, an order/import requires a customer, or an authorized user creates one. |

### Actors

- Account administrator
- Sales/service owner
- CustomArr

### State path

`draft → review → active → restricted → inactive → merged`

### Required sequence

1. Search identifiers, names, domains, addresses, contacts, and external IDs for candidates.
2. Create minimum customer identity, classification, lifecycle, owner, and primary relationship.
3. Add parent/child/related accounts and explain relationship type/effective dates.
4. Add contacts with roles, authorization, consent, and preferred channels.
5. Add billing/service/shipping/other addresses with verification and effective dates.
6. Collect required customer fields/requirements or mark backfill tasks.
7. Run eligibility/risk checks and create onboarding if needed.
8. Publish customer refs to allowed products and retain match/provenance.

### Exception and recovery paths

- Duplicate/merge candidate, hierarchy cycle, address ambiguity, contact already belongs elsewhere, customer inactive, or required identifier unavailable.
- Quick create from OrdArr then backfill through onboarding.

### Cross-product and external handoffs

- CustomArr → OrdArr/RoutArr/etc.: customer refs.
- CustomArr ↔ NexArr: portal identity.
- RecordArr/Compliance Core: evidence/requirements.

### Evidence and audit record

- Match/provenance.
- Hierarchy/contacts/addresses.
- Required-field status.
- Eligibility/activation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to active.
- Duplicate rate.
- Backfill aging.
- Hierarchy integrity.

## CU-WF-004 — Opportunity pipeline, stage validation, and forecast

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Manage a commercial pursuit with meaningful next actions and evidence. |
| Trigger | A qualified lead/customer need becomes an opportunity. |

### Actors

- Account executive
- Sales manager
- Solution/operations reviewer
- CustomArr

### State path

`discovery → qualified → solution → proposal → negotiation → commit → won → lost → on_hold`

### Required sequence

1. Create opportunity with account, need, products/services, value range, timing, stakeholders, source, owner, and next step.
2. Apply stage entry/exit criteria and required fields/tasks.
3. Track activities, risks, competitors, customer commitments, internal dependencies, and probability/forecast category.
4. Request operational/eligibility feasibility before firm commitments.
5. Create proposal/agreement versions and approvals.
6. Advance, regress, hold, split, merge, win, or lose with reason/evidence.
7. On win, hand off onboarding/order creation with full requirements.
8. Preserve stage history for forecast/velocity analysis.

### Exception and recovery paths

- No decision process, stale opportunity, duplicate pursuit, feasibility blocked, price approval needed, customer changes scope, or deal is lost/no-decision.
- Opportunity spans multiple customer entities/sites.

### Cross-product and external handoffs

- CustomArr ↔ OrdArr/operational products: feasibility/handoff.
- CustomArr ↔ RecordArr/e-sign: proposal/agreement.
- CustomArr → ReportArr: pipeline/forecast.

### Evidence and audit record

- Stage history/criteria.
- Activities/commitments.
- Feasibility/approvals.
- Win/loss reason and handoff.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Pipeline velocity.
- Stage aging.
- Forecast accuracy.
- Win rate.
- No-next-step rate.

## CU-WF-005 — Proposal, approval, agreement, and order handoff

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Partial |
| Purpose | Turn an approved commercial offer into a controlled agreement and executable order. |
| Trigger | An opportunity is ready for a formal proposal or agreement. |

### Actors

- Sales user
- Approver
- Customer signer
- CustomArr
- OrdArr

### State path

`draft → internal_review → sent → negotiation → accepted → rejected → expired → handed_off`

### Required sequence

1. Create proposal from customer, opportunity, requirements, items/services, pricing refs, terms, dates, and assumptions.
2. Validate operational feasibility, eligibility, capacity, requirements, and required approvals.
3. Generate a RecordArr-controlled document and route internal review.
4. Send through scoped portal/e-sign provider and track views/questions/version changes.
5. Capture acceptance/rejection/expiry and signed evidence.
6. Create/update agreement with effective dates, obligations, renewal, and document refs.
7. Send a versioned order/request payload to OrdArr with customer/requirement/agreement refs.
8. Confirm OrdArr acceptance or resolve validation blockers.

### Exception and recovery paths

- Pricing/terms outside authority, customer requests edits, proposal expires, signer unauthorized, requirements changed, or OrdArr rejects incomplete handoff.
- Multiple orders arise from one agreement.

### Cross-product and external handoffs

- CustomArr ↔ RecordArr/e-sign.
- CustomArr → OrdArr: order handoff.
- OrdArr → CustomArr: acceptance/status.
- LedgArr: pricing/billing refs where appropriate.

### Evidence and audit record

- Proposal/agreement versions.
- Approvals/signatures.
- Customer interactions.
- Handoff payload/result.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Proposal cycle.
- Approval time.
- Acceptance rate.
- Handoff rejection.
- Time to first order.

## CU-WF-006 — Customer requirements and preference change

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Durable |
| Purpose | Capture structured customer requirements and propagate changes safely. |
| Trigger | A customer/internal user submits or discovers a new/changed requirement. |

### Actors

- Customer portal user
- Account/service owner
- Operations reviewer
- CustomArr

### State path

`submitted → review → clarification → approved → rejected → published → superseded`

### Required sequence

1. Identify requirement type, scope, affected sites/orders/services, effective date, priority, and source.
2. Capture structured value plus supporting RecordArr evidence and customer acknowledgement.
3. Compare with prior version and detect conflicts/unsupported requests.
4. Route operational, quality, compliance, legal, or finance review as needed.
5. Approve, reject, request clarification, or accept with limitation.
6. Publish versioned requirement to affected products and identify active records needing review.
7. Confirm downstream acknowledgement or blockers.
8. Notify customer and retain superseded history.

### Exception and recovery paths

- Requirement conflicts with law/policy, cannot be fulfilled, applies retroactively, evidence missing, customer contact unauthorized, or downstream products unavailable.
- Temporary exception applies to one order only.

### Cross-product and external handoffs

- CustomArr ↔ Compliance Core/AssurArr/OrdArr/RoutArr/LoadArr/MaintainArr.
- RecordArr: evidence.
- ReportArr: impact.

### Evidence and audit record

- Source/version/diff.
- Review decisions.
- Published scope.
- Downstream acknowledgements/exceptions.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Review time.
- Downstream acknowledgement.
- Requirement-related exceptions.
- Customer clarification cycles.

## CU-WF-007 — Customer onboarding and mutual action plan

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Durable |
| Purpose | Coordinate activation across customer and internal teams with transparent readiness. |
| Trigger | A customer agreement/order requires onboarding or a relationship is approved for activation. |

### Actors

- Customer contact
- Account owner
- Implementation/service team
- CustomArr

### State path

`planned → in_progress → customer_action → blocked → readiness_review → active → closed`

### Required sequence

1. Instantiate onboarding template based on segment/service/location/requirements.
2. Create customer and internal tasks for contacts, locations, documents, portal, integrations, training, billing refs, compliance facts, and operational setup.
3. Assign owners, due dates, dependencies, visibility, and evidence requirements.
4. Expose a customer-safe mutual action plan in the portal.
5. Track blockers, questions, changes, and completion evidence.
6. Run eligibility/readiness review across affected products.
7. Activate service/customer status or approve limited go-live with exceptions.
8. Close with handoff to ongoing success/support and onboarding feedback.

### Exception and recovery paths

- Customer does not provide documents, integration fails, location not ready, requirements conflict, billing setup incomplete, or project scope changes.
- Some tasks are confidential/internal only.

### Cross-product and external handoffs

- CustomArr ↔ OrdArr/NexArr/RecordArr/Compliance Core/operational products.
- CustomArr → ReportArr: onboarding metrics.

### Evidence and audit record

- Template/version.
- Task/dependency history.
- Customer portal actions.
- Readiness/exceptions/activation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to value.
- On-time task rate.
- Customer blocker aging.
- First-use success.
- Onboarding satisfaction.

## CU-WF-008 — Customer portal invitation and access administration

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Provide scoped customer self-service without exposing unrelated data. |
| Trigger | An authorized internal user invites a customer contact or contact requests access. |

### Actors

- Customer contact
- Account/portal administrator
- NexArr
- CustomArr

### State path

`invited → verified → active → suspended → expired → revoked`

### Required sequence

1. Verify contact identity, role, account relationship, and authorization scope.
2. Create portal access record with allowed accounts/sites/actions, expiry/review, and terms.
3. Issue NexArr-backed invitation or external scoped session.
4. Contact verifies identity and enrolls required factors.
5. Show only permitted customer records/actions and log access.
6. Allow internal admin to modify/suspend/revoke and customer to manage sessions/preferences.
7. Review access when contact role/account relationship changes.
8. Preserve portal submissions and access history.

### Exception and recovery paths

- Contact already linked elsewhere, signer/admin role unverified, forwarded invitation, account inactive, compromised session, or user requests cross-account access.
- External user may represent multiple related customer accounts with explicit grants.

### Cross-product and external handoffs

- CustomArr ↔ NexArr: identity/session.
- CustomArr ↔ RecordArr/OrdArr: scoped content/actions.
- ReportArr: access metrics.

### Evidence and audit record

- Authorization basis/scope.
- Invitation/verification.
- Access and changes.
- Review/revocation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Activation rate.
- Unauthorized attempts.
- Access review overdue.
- Self-service completion.

## CU-WF-009 — Customer case intake, SLA, escalation, and resolution

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Resolve customer questions/issues and coordinate operational action without losing communication context. |
| Trigger | Customer or employee creates a case from portal, email, phone, form, or linked operational event. |

### Actors

- Customer contact
- Service agent
- Case owner
- Operational resolver
- CustomArr

### State path

`new → triage → assigned → waiting_customer → waiting_internal → resolved → closed → reopened`

### Required sequence

1. Identify customer/contact authorization, channel consent, affected order/service/site/product, and urgency.
2. Deduplicate related cases and classify type/priority/SLA.
3. Acknowledge through the requested permitted channel and assign owner/team.
4. Gather facts, communications, attachments, and customer impact.
5. Create linked tasks/handoffs to OrdArr, RoutArr, LoadArr, MaintainArr, AssurArr, SupplyArr, or finance owner.
6. Track response/resolution SLA, escalations, and customer updates.
7. Confirm resolution/acceptance or reopen with reason.
8. Close with cause, knowledge candidate, satisfaction, and follow-up.

### Exception and recovery paths

- Safety/quality incident, abusive/spam message, unauthorized contact, missing source record, multi-customer issue, legal threat, or outage affecting many cases.
- Case needs confidential internal notes separate from customer-visible conversation.

### Cross-product and external handoffs

- CustomArr ↔ operational products/AssurArr/RecordArr.
- NexArr notification/email integrations.
- ReportArr: SLA/volume.

### Evidence and audit record

- Original communications/source.
- Classification/SLA.
- Handoffs/actions.
- Customer updates.
- Resolution/cause/satisfaction.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- First response.
- Resolution time.
- Reopen rate.
- SLA breach.
- Customer satisfaction.

## CU-WF-010 — Customer complaint to quality and corrective action

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Partial |
| Purpose | Route product/service quality complaints to AssurArr while maintaining customer ownership and communication. |
| Trigger | A case is classified as a potential quality complaint. |

### Actors

- Customer service agent
- Quality reviewer
- Customer contact
- CustomArr
- AssurArr

### State path

`reported → contained → quality_review → investigating → customer_action → resolved → closed`

### Required sequence

1. Capture complaint, affected product/service/order/lot/asset, dates, severity, impact, and evidence.
2. Perform immediate customer/safety containment and escalation.
3. Create/link AssurArr customer complaint quality case and nonconformance/CAPA as appropriate.
4. Keep customer communications and commitments in CustomArr; quality investigation truth remains AssurArr.
5. Coordinate returns, inspections, transport, or service work through owning products.
6. Receive quality findings/disposition and translate into customer-safe update.
7. Track replacement/refund/service commitments through OrdArr/LedgArr refs.
8. Close customer case only after communication and corrective follow-up are complete.

### Exception and recovery paths

- Potential reportable event, injury, recall, unknown lot, multiple customers, legal claim, customer refuses return, or quality investigation remains open.
- Quality details may be confidential until approved for release.

### Cross-product and external handoffs

- CustomArr → AssurArr: complaint context.
- AssurArr → CustomArr: approved status/outcome.
- CustomArr ↔ OrdArr/LoadArr/RoutArr/MaintainArr/RecordArr.

### Evidence and audit record

- Complaint/source/evidence.
- Containment and customer communications.
- AssurArr refs/status.
- Customer remedy and closure.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to quality triage.
- Customer update cadence.
- Complaint recurrence.
- Closure alignment.

## CU-WF-011 — Customer health review, success plan, and churn risk

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Use governed signals and account context to plan retention and value outcomes. |
| Trigger | A scheduled review occurs or health signals change materially. |

### Actors

- Customer success/account owner
- Manager
- Service/operations stakeholders
- CustomArr

### State path

`scheduled → review → plan_open → at_risk → recovering → healthy → renewed → churned`

### Required sequence

1. Collect relationship, activity, case/SLA, onboarding, order/service, quality, delivery, payment refs, requirement, and stakeholder signals.
2. Validate freshness and distinguish missing data from negative performance.
3. Calculate explainable health dimensions and allow accountable qualitative adjustment with reason.
4. Review customer goals, value delivered, unresolved issues, stakeholder coverage, risks, and opportunities.
5. Create success/retention plan with milestones, owners, dates, and customer-visible subset.
6. Escalate urgent churn/service risk to appropriate owners.
7. Track actions and changes in health.
8. Close/review outcome at renewal, recovery, expansion, or churn.

### Exception and recovery paths

- Sparse customer history, disputed metrics, recent major incident, no executive sponsor, payment issue under dispute, or health model bias.
- Do not expose internal profitability/risk details to customer.

### Cross-product and external handoffs

- Operational products/LedgArr → CustomArr: approved signals.
- CustomArr → ReportArr: governed health metrics.
- CustomArr ↔ customer portal: mutual plan.

### Evidence and audit record

- Signal sources/freshness.
- Health rationale/adjustments.
- Plan/actions.
- Outcome/lessons.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Risk detection lead time.
- Action closure.
- Renewal/retention.
- False-risk rate.
- Stakeholder coverage.

## CU-WF-012 — Renewal, expansion, contraction, or termination

| Field | Definition |
| --- | --- |
| Classification | COMMON · UNDERSERVED |
| Implementation state | Partial |
| Purpose | Manage an expiring relationship and coordinate operational/financial consequences. |
| Trigger | Agreement/commitment enters a renewal window or customer requests change/termination. |

### Actors

- Account owner
- Customer contact
- Approver
- CustomArr

### State path

`review_due → discovery → proposal → negotiation → renewed → expanded → contracted → terminated`

### Required sequence

1. Review agreement terms, notice dates, usage/service/order/quality/payment refs, requirements, health, and open obligations.
2. Confirm customer goals, scope, sites, contacts, pricing/terms, and changes.
3. Create renewal/expansion/contraction opportunity and proposal.
4. Obtain feasibility and approvals.
5. Negotiate/sign updated agreement or record termination/nonrenewal.
6. Create OrdArr changes/new orders and onboarding/offboarding tasks.
7. Update portal access, customer lifecycle, service requirements, and owner plan.
8. Close with reason, transition, final evidence, and retention/exit feedback.

### Exception and recovery paths

- Notice deadline missed, disputed performance, unresolved case/claim, customer requests retroactive change, signer unauthorized, or operational exit incomplete.
- Partial site/service termination.

### Cross-product and external handoffs

- CustomArr ↔ RecordArr/e-sign/OrdArr/operational products/LedgArr refs.
- CustomArr → ReportArr: renewal outcomes.

### Evidence and audit record

- Terms/notice snapshot.
- Performance/health review.
- Proposal/approval/signature.
- Operational transition and outcome reason.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Renewal forecast accuracy.
- Notice compliance.
- Gross/net retention.
- Transition completion.
- Churn reasons.

## CU-WF-013 — Customer duplicate review and merge

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Resolve duplicate accounts/contacts without losing references or history. |
| Trigger | Import, create, integration, or scheduled matching identifies candidates. |

### Actors

- Data steward
- Account owner
- CustomArr

### State path

`candidate → review → approved → merging → partial → complete → reversed`

### Required sequence

1. Present candidate records, match reasons/confidence, owners, hierarchies, contacts, identifiers, activity, open work, portal access, and downstream refs.
2. Validate whether records are duplicate, related, or distinct.
3. Select survivor and field-by-field survivorship; preserve alternate identifiers/names.
4. Preview downstream reference remapping and conflicts.
5. Obtain approval for high-impact merge.
6. Execute idempotent merge, remap allowed refs, and create tombstone/redirect for merged IDs.
7. Reconcile failed downstream updates and notify owners.
8. Retain merge record, before/after, and unmerge/correction strategy.

### Exception and recovery paths

- Different legal entities with similar names, active contracts/orders on both, conflicting external IDs, portal users, hierarchy cycle, or downstream product unavailable.
- Contacts may merge independently of accounts.

### Cross-product and external handoffs

- CustomArr ↔ all referencing products/integrations.
- RecordArr: merge evidence.
- ReportArr: data quality.

### Evidence and audit record

- Match inputs/rules.
- Steward decision.
- Survivorship/remap preview.
- Execution/reconciliation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Duplicate rate.
- False positive.
- Merge completion.
- Downstream orphan refs.
- Time to review.

## CU-WF-014 — Customer import with mapping, validation, and rollback

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Durable |
| Purpose | Load CRM data without silently creating duplicates or invalid relationships. |
| Trigger | An authorized user uploads customer/contact/pipeline data or runs an integration migration. |

### Actors

- Data steward
- CRM administrator
- CustomArr

### State path

`uploaded → mapping → validation → matching → review → committing → complete → partial`

### Required sequence

1. Select import type/template and securely upload source.
2. Map fields, value catalogs, owners, lifecycle, contact roles, address types, and external IDs.
3. Validate required fields, formats, hierarchy cycles, permissions, and reference candidates.
4. Run duplicate matching and show create/update/link/ignore proposals.
5. Preview totals, errors, downstream effects, and sample records.
6. Approve and commit idempotently in batches with row-level results.
7. Reconcile partial failures and correct source/records through supported workflows.
8. Retain source hash, mapping template, decisions, and rollback/correction plan.

### Exception and recovery paths

- Malformed file, unsupported encoding, ambiguous owner, duplicate external ID, hierarchy cycle, restricted field, stale update, or integration rate limit.
- Import should not overwrite newer changes without version check.

### Cross-product and external handoffs

- CustomArr ↔ NexArr Smart Import where used.
- CustomArr ↔ integrations/products for refs.
- RecordArr: source package.

### Evidence and audit record

- Source/mapping.
- Validation/matches.
- Review decisions.
- Row commit/results/corrections.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Import success.
- Duplicate prevention.
- Reviewer overrides.
- Partial failure.
- Time to usable data.

## CU-WF-015 — Field visit and offline account update

| Field | Definition |
| --- | --- |
| Classification | UNDERSERVED |
| Implementation state | Target |
| Purpose | Support a field customer interaction with scoped offline context and reliable follow-up. |
| Trigger | A field user opens an assigned visit/task or loses connectivity at a customer site. |

### Actors

- Field sales/service user
- Field Companion
- CustomArr

### State path

`downloaded → visit → queued → syncing → conflict → committed → closed`

### Required sequence

1. Download only assigned account/contact/location, open tasks/cases, permitted requirements, and visit agenda with expiry.
2. Check in according to tenant/privacy policy.
3. Capture notes, contacts, requirements, photos/documents, consent/signature, and follow-up tasks locally.
4. Show which changes are draft/pending and prohibit unavailable live-only actions.
5. On reconnect, validate record versions, authorization, duplicates, and required fields.
6. Resolve conflicts explicitly and commit accepted updates.
7. Create operational/order/case handoffs only after server validation.
8. Purge local data after sync/expiry and retain visit audit.

### Exception and recovery paths

- Contact/owner changed, account merged, consent withdrawn, task canceled, duplicate note, device compromised, or conflicting requirement update.
- Sensitive data is not cached offline unless explicitly permitted.

### Cross-product and external handoffs

- Field Companion ↔ NexArr: device/session.
- Field Companion ↔ CustomArr: scoped data/actions.
- CustomArr → RecordArr/products after commit.

### Evidence and audit record

- Offline package/scope.
- Visit actions/evidence.
- Sync/conflicts.
- Final updates/handoffs.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Visit completion.
- Sync success.
- Conflict rate.
- Follow-up closure.
- Offline data purge.

## CU-WF-016 — Customer audit/data-room package

| Field | Definition |
| --- | --- |
| Classification | UNDERSERVED · DEMOCRATIZE |
| Implementation state | Target |
| Purpose | Assemble a scoped customer relationship or due-diligence package with secure sharing. |
| Trigger | An authorized user or customer requests an onboarding, contract, service, audit, or data-room package. |

### Actors

- Account owner
- Customer contact
- Auditor/reviewer
- CustomArr
- RecordArr

### State path

`requested → collecting → review → assembled → shared → revoked → closed`

### Required sequence

1. Define purpose, recipient, accounts/sites, date range, record categories, and expiration.
2. Collect customer profile, contacts/authorizations, requirements, agreements, onboarding, portal, cases, selected order/service/quality refs, and approved metrics.
3. Validate permissions, consent, retention, confidentiality, and legal hold.
4. Apply redaction and customer-visible/internal-only rules.
5. Request RecordArr package with manifest, watermarks/signature, and expiring share.
6. Track requests, questions, acknowledgement, downloads, and revocation.
7. Add supplemental versions without mutating prior package.
8. Close and retain access evidence.

### Exception and recovery paths

- Mixed internal/customer data, legal privilege, subject request scope, expired agreement, external recipient forwards link, or source record changes after snapshot.
- Customer may request correction to profile data rather than package only.

### Cross-product and external handoffs

- CustomArr ↔ RecordArr/NexArr.
- CustomArr ↔ referenced products for approved snapshots.
- ReportArr: package metrics.

### Evidence and audit record

- Purpose/authority/scope.
- Manifest/redactions.
- Share/access/acknowledgement.
- Supplement/revocation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Generation time.
- Unauthorized attempts.
- Recipient completion.
- Supplement requests.
- Revocation propagation.



## Workflow definition of done

A workflow is releasable only when its happy path, every listed exception, dependency outage, duplicate/retry, stale/concurrent update, permission denial, cancellation, correction/void, archival/retention, import/API/event path, notification, audit trail, reporting projection, mobile behavior, and professional printable output have automated and human-usable acceptance coverage.
