# CustomArr — Workflows, Status Logic, Events, and APIs

## Major workflow: manual customer creation

```text
1. User creates CustomerAccount.
2. User enters customer identity, type, relationship role, and account owner.
3. CustomArr checks duplicate candidates and existing aliases.
4. User confirms creation or links to existing customer.
5. User creates primary contact and required locations.
6. User attaches required onboarding documents through RecordArr.
7. CustomArr evaluates requirements and service eligibility.
8. Approver reviews if required.
9. CustomerAccount.status becomes active or remains in the appropriate lifecycle state.
10. serviceEligibilityStatus becomes eligible, limited, blocked, pending_review, or unknown.
11. CustomArr emits customer created and status/eligibility events.
```

## Major workflow: customer onboarding

```text
1. Customer is placed in onboarding status.
2. CustomArr builds onboarding checklist from customer type, relationship role, location type, tenant settings, and customer-specific requirements.
3. User completes identity, contact, location, document, requirement, and approval steps.
4. CustomArr asks RecordArr, Compliance Core, TrainArr, AssurArr, or other owner for required checks.
5. Missing or failed requirements become required actions.
6. Reviewer approves, rejects, or requests additional information.
7. CustomArr updates account status and service eligibility.
```

## Major workflow: lead to opportunity

```text
1. User or integration creates a CustomArr lead with source, contact, service interest, fit, and next follow-up.
2. CustomArr de-duplicates against accounts, contacts, aliases, external mappings, and import candidates.
3. User converts the lead into an existing customer or a new customer account.
4. CustomArr creates an opportunity and links the lead, customer, owner, scope, estimated value, and source.
5. Lead status becomes converted; downstream products receive only refs or handoff requests when needed.
```

## Major workflow: opportunity/proposal handoff

```text
1. User advances a CustomArr opportunity through discovery, proposal, negotiation, and won/lost states.
2. Proposal records capture scope, pricing snapshot, terms snapshot, approval state, customer response, and valid-until date.
3. Agreement records capture CRM metadata and RecordArr/ContractArr refs; they do not own stored files or legal lifecycle.
4. When an opportunity is won or proposal is accepted, CustomArr records the commercial intent and creates an explicit handoff request or ref.
5. OrdArr or another execution owner creates downstream execution records only inside that owning product.
```

## Major workflow: downstream customer eligibility check

```text
1. Product submits customerId, customerLocationId, customerContactId, workflowKey, and sourceObjectRef.
2. CustomArr resolves account, location, contact, service profile, active holds, active requirements, and authorizations.
3. CustomArr asks owning products for checks when needed.
4. CustomArr returns eligible, limited, blocked, pending_review, or unknown.
5. Product proceeds, warns, blocks, or requests approval/waiver based on response.
6. CustomArr records requirement evaluation facts when appropriate.
```

## Major workflow: order creation check

```text
1. OrdArr requests customer order eligibility from CustomArr.
2. CustomArr validates customer, bill-to, ship-to, contacts, holds, and order-triggered requirements.
3. CustomArr returns allowed, warning, blocked, or approval_required.
4. OrdArr owns the order lifecycle and stores CustomArr refs.
5. CustomArr receives order facts/events for customer timeline and reporting.
```

## Major workflow: route dispatch / delivery check

```text
1. RoutArr requests pickup/dropoff/customer location eligibility.
2. CustomArr checks customer location status, access requirements, appointment requirements, contact authorization, and active holds.
3. CustomArr returns instructions, restrictions, warnings, or blockers.
4. RoutArr owns route/trip execution.
5. CustomArr receives route exceptions that affect customer relationship status or communication.
```

## Major workflow: warehouse release / fulfillment check

```text
1. LoadArr requests customer/location requirements before release, pickup, delivery, or staging when customer requirements apply.
2. CustomArr resolves ship-to/consignee/customer requirements.
3. CustomArr returns documentation, labeling, delivery, signature, or quality constraints.
4. LoadArr owns warehouse execution and stock movement.
5. CustomArr receives customer-facing exception facts if fulfillment is affected.
```

## Major workflow: customer hold blocks workflow

```text
1. Hold is created in CustomArr or mirrored from another product/external system.
2. CustomArr recalculates customer and location eligibility.
3. Affected products receive hold/eligibility events.
4. Product workflows are blocked, warned, or routed for approval.
5. Authorized reviewer resolves, overrides, expires, or cancels hold.
6. CustomArr recalculates eligibility and emits release events.
```

## Major workflow: customer requirement change

```text
1. User creates, edits, retires, waives, or activates a customer requirement.
2. CustomArr validates affected customer/account/location/product scopes.
3. CustomArr links RecordArr/Compliance Core/TrainArr/AssurArr refs when needed.
4. CustomArr recalculates eligibility.
5. CustomArr emits requirement changed and eligibility changed events.
6. Downstream products refresh cached requirement summaries.
```

## Major workflow: customer merge

```text
1. CustomArr identifies duplicate customer candidates by identity, alias, external refs, address, contact, or import conflict.
2. User reviews duplicates and selects survivor record.
3. CustomArr proposes field resolution, moved contacts, moved locations, moved requirements, and moved mappings.
4. Authorized approver approves merge.
5. CustomArr updates references through event-driven remapping guidance.
6. CustomArr stores merge record and emits customer merged event.
```

## Customer service eligibility calculation inputs

```text
CustomerServiceEligibilityInputs
- customer.status
- customer.onboardingStatus
- customer.serviceProfile.status
- customerLocation.status
- customerLocation.serviceEligibilityStatus
- active customer holds
- active customer location holds
- active blocking requirements
- missing required documents
- expired required documents
- failed requirement evaluations
- required contact authorization missing
- contract active/expired status snapshot
- external accounting hold snapshot
- active quality hold snapshot from AssurArr
- active compliance blocker snapshot from Compliance Core
- manual authorized override
```

## Suggested eligibility logic

```text
blocked
- Customer or location service eligibility is blocked.
- Active critical hold blocks the requested workflow.
- Blocking requirement failed and no waiver exists.
- Required customer/location/contact is archived or invalid.

limited
- Active non-critical hold affects the requested workflow.
- Warning requirement exists.
- Customer/location is active but has restrictions.
- Customer/location is allowed only for selected products or workflows.

eligible
- Customer and location are active.
- Required contacts, requirements, and documents are acceptable.
- No active holds block the requested workflow.

pending_review
- Onboarding or review is incomplete.
- Requirement evaluation requires human review.
- Duplicate/merge conflict is unresolved.

unknown
- Required customer/location/contact facts are missing.
- Owning product check could not be completed.
```

## CustomArr emitted events

```text
customarr.customer.created
customarr.customer.updated
customarr.customer.status_changed
customarr.customer.onboarding_started
customarr.customer.onboarding_submitted
customarr.customer.onboarding_approved
customarr.customer.onboarding_rejected
customarr.customer.activated
customarr.customer.inactivated
customarr.customer.archived
customarr.customer.merged

customarr.customer_alias.created
customarr.customer_alias.retired
customarr.customer_external_mapping.created
customarr.customer_external_mapping.updated
customarr.customer_external_mapping.conflict_detected

customarr.customer_group.created
customarr.customer_group.updated
customarr.customer_group.membership_changed

customarr.lead.created
customarr.lead.converted
customarr.opportunity.created
customarr.opportunity.won
customarr.proposal.created
customarr.proposal.accepted
customarr.agreement.created
customarr.agreement.updated
customarr.customer_case.created
customarr.customer_case.updated
customarr.customer_activity.logged
customarr.customer_task.created
customarr.customer_task.completed

customarr.customer_location.created
customarr.customer_location.updated
customarr.customer_location.status_changed
customarr.customer_location.eligibility_changed
customarr.customer_location.archived

customarr.customer_contact.created
customarr.customer_contact.updated
customarr.customer_contact.status_changed
customarr.customer_contact.authorization_changed
customarr.customer_contact.portal_invited
customarr.customer_contact.portal_linked
customarr.customer_contact.portal_access_revoked

customarr.customer_portal_access.created
customarr.customer_portal_access.updated
customarr.customer_portal_access.suspended
customarr.customer_portal_access.revoked
customarr.customer_portal_access.role_changed
customarr.customer_portal_access.location_scope_changed

customarr.customer_requirement.created
customarr.customer_requirement.updated
customarr.customer_requirement.activated
customarr.customer_requirement.waived
customarr.customer_requirement.expired
customarr.customer_requirement.retired
customarr.customer_requirement.evaluation_passed
customarr.customer_requirement.evaluation_warned
customarr.customer_requirement.evaluation_failed
customarr.customer_requirement.evaluation_blocked

customarr.customer_preference.created
customarr.customer_preference.updated
customarr.customer_preference.retired

customarr.customer_contract.linked
customarr.customer_contract.updated
customarr.customer_contract.expired
customarr.customer_contract.renewal_due
customarr.customer_contract.terminated

customarr.customer_hold.created
customarr.customer_hold.resolved
customarr.customer_hold.overridden
customarr.customer_hold.expired
customarr.customer_hold.canceled

customarr.customer_service_profile.updated
customarr.customer_service_eligibility.changed

customarr.customer_approval.requested
customarr.customer_approval.approved
customarr.customer_approval.rejected
customarr.customer_approval.expired

customarr.customer_review.scheduled
customarr.customer_review.completed
customarr.customer_risk_profile.updated

customarr.customer_exception.created
customarr.customer_exception.routed
customarr.customer_exception.resolved
customarr.customer_exception.closed

customarr.customer_communication.logged

customarr.customer_eligibility.checked
customarr.customer_onboarding.created
customarr.customer_health.updated
customarr.customer_import_batch.created
customarr.customer_dedupe_candidate.created
customarr.customer_merge.proposed
customarr.customer_merge.completed
```

## Workspace APIs CustomArr should expose

```text
GET /api/v1/workspace/accounts
GET /api/v1/workspace/locations
GET /api/v1/workspace/contacts
GET /api/v1/workspace/leads
POST /api/v1/workspace/leads
POST /api/v1/workspace/leads/{leadId}/convert
GET /api/v1/workspace/opportunities
POST /api/v1/workspace/opportunities
POST /api/v1/workspace/opportunities/{opportunityId}/won
GET /api/v1/workspace/proposals
POST /api/v1/workspace/proposals
POST /api/v1/workspace/proposals/{proposalId}/accept
GET /api/v1/workspace/agreements
GET /api/v1/workspace/cases
POST /api/v1/workspace/cases
GET /api/v1/workspace/activities
POST /api/v1/workspace/activities
GET /api/v1/workspace/tasks
POST /api/v1/workspace/tasks
GET /api/v1/workspace/portal-access
GET /api/v1/workspace/requirements
GET /api/v1/workspace/eligibility
POST /api/v1/workspace/eligibility
GET /api/v1/workspace/onboarding
GET /api/v1/workspace/health
GET /api/v1/workspace/imports
POST /api/v1/workspace/imports
GET /api/v1/workspace/merge-review
POST /api/v1/workspace/merge-review
GET /api/v1/workspace/integration-references
```

## Integration APIs CustomArr should expose

```text
GET /api/v1/integrations/customers
GET /api/v1/integrations/customers/{customerId}
POST /api/v1/integrations/customers
POST /api/v1/integrations/customers/{customerId}/status-updates
POST /api/v1/integrations/customers/{customerId}/archive
POST /api/v1/integrations/customers/{customerId}/merge-proposals
POST /api/v1/integrations/customer-resolutions

GET /api/v1/integrations/customers/{customerId}/service-profile
POST /api/v1/integrations/customer-eligibility-checks
POST /api/v1/integrations/customer-requirement-evaluations
POST /api/v1/integrations/customer-activity-events
POST /api/v1/integrations/customer-external-mappings
POST /api/v1/integrations/opportunities/{opportunityId}/ordarr-handoffs
POST /api/v1/integrations/proposals/{proposalId}/ordarr-handoffs

GET /api/v1/integrations/customers/{customerId}/locations
GET /api/v1/integrations/customer-locations/{customerLocationId}
POST /api/v1/integrations/customer-locations
POST /api/v1/integrations/customer-locations/{customerLocationId}/status-updates

GET /api/v1/integrations/customers/{customerId}/contacts
GET /api/v1/integrations/customer-contacts/{contactId}
POST /api/v1/integrations/customer-contacts
POST /api/v1/integrations/customer-contacts/{contactId}/authorizations
POST /api/v1/integrations/customer-portal-invites

GET /api/v1/integrations/customers/{customerId}/requirements
GET /api/v1/integrations/customer-requirements/{requirementId}
POST /api/v1/integrations/customer-requirements
POST /api/v1/integrations/customer-requirements/{requirementId}/waivers
POST /api/v1/integrations/customer-requirements/{requirementId}/status-updates

GET /api/v1/integrations/customers/{customerId}/holds
POST /api/v1/integrations/customer-holds
POST /api/v1/integrations/customer-holds/{holdId}/resolve
POST /api/v1/integrations/customer-holds/{holdId}/override

GET /api/v1/integrations/customers/{customerId}/contract-refs
POST /api/v1/integrations/customer-contract-refs
POST /api/v1/integrations/customer-contract-refs/{contractRefId}/status-updates

POST /api/v1/integrations/customer-exceptions
POST /api/v1/integrations/customer-communications
POST /api/v1/integrations/customer-risk-signals
POST /api/v1/integrations/customer-external-mappings
```

## APIs CustomArr should consume

```text
NexArr
- POST /api/v1/platform/handoff/redeem
- POST /api/v1/platform/service-tokens/introspect
- GET /api/v1/platform/tenants/{tenantId}/entitlements/{productKey}
- POST /external-identities/invites when customer portal access exists
- GET /external-identities/{identityId}

StaffArr
- GET /persons/{personId}
- GET /persons/{personId}/permissions
- GET /locations/{locationId} for internal owner/context only
- GET /org-units/{orgUnitId}

TrainArr
- POST /qualification-checks
- GET /persons/{personId}/qualifications
- POST /training-assignment-requests
- POST /remediation-requests

Compliance Core
- GET /catalogs/governing-bodies
- GET /rulepacks
- POST /evaluations
- POST /evidence-mapping/suggest
- POST /requirement-interpretations

RecordArr
- POST /records
- GET /records/{recordId}
- POST /upload-sessions
- POST /record-packages
- POST /record-requirement-checks

OrdArr
- GET /orders/{orderId}
- POST /customer-order-facts
- POST /order-customer-exceptions

RoutArr
- GET /routes/{routeId}
- GET /trips/{tripId}
- POST /customer-route-facts
- POST /customer-location-exceptions

LoadArr
- GET /shipments/{shipmentId}
- GET /loads/{loadId}
- POST /customer-fulfillment-facts
- POST /customer-release-facts

MaintainArr
- GET /assets/{assetId}
- GET /work-orders/{workOrderId}
- POST /customer-impact-facts

SupplyArr
- GET /suppliers/{supplierId}
- POST /supplier-customer-link-checks

AssurArr
- GET /holds
- GET /holds/{holdId}
- POST /quality-events
- POST /customer-complaint-facts

ReportArr
- POST /events
```

## Permission examples

```text
customarr.customers.read
customarr.customers.create
customarr.customers.update
customarr.customers.activate
customarr.customers.archive
customarr.customers.merge
customarr.customers.manage_external_refs
customarr.accounts.read
customarr.accounts.manage

customarr.leads.read
customarr.leads.manage
customarr.leads.convert

customarr.opportunities.read
customarr.opportunities.manage
customarr.opportunities.handoff

customarr.proposals.read
customarr.proposals.manage
customarr.proposals.accept

customarr.agreements.manage

customarr.cases.read
customarr.cases.manage

customarr.eligibility.check
customarr.portal_access.manage
customarr.imports.read
customarr.imports.manage
customarr.integration_references.manage

customarr.customer_groups.read
customarr.customer_groups.manage

customarr.customer_locations.read
customarr.customer_locations.create
customarr.customer_locations.update
customarr.customer_locations.activate
customarr.customer_locations.block
customarr.customer_locations.archive

customarr.customer_contacts.read
customarr.customer_contacts.create
customarr.customer_contacts.update
customarr.customer_contacts.manage_authorizations
customarr.customer_contacts.invite_portal
customarr.customer_contacts.revoke_portal_access

customarr.customer_requirements.read
customarr.customer_requirements.create
customarr.customer_requirements.update
customarr.customer_requirements.activate
customarr.customer_requirements.waive
customarr.customer_requirements.retire
customarr.customer_requirements.evaluate

customarr.customer_contracts.read
customarr.customer_contracts.manage_refs

customarr.customer_preferences.read
customarr.customer_preferences.manage

customarr.customer_holds.read
customarr.customer_holds.apply
customarr.customer_holds.resolve
customarr.customer_holds.override

customarr.customer_onboarding.read
customarr.customer_onboarding.manage
customarr.customer_onboarding.approve
customarr.customer_onboarding.reject

customarr.customer_reviews.read
customarr.customer_reviews.manage
customarr.customer_risk.read
customarr.customer_risk.update

customarr.customer_exceptions.read
customarr.customer_exceptions.create
customarr.customer_exceptions.route
customarr.customer_exceptions.resolve
customarr.customer_communications.read
customarr.customer_communications.create
```

## Default role examples

```text
Customer Viewer
- read customers, contacts, locations, requirements, preferences, holds, and contract refs

Customer Coordinator
- create/update contacts and locations
- log communications
- manage non-blocking preferences
- submit onboarding packets

Customer Account Manager
- create/update customer accounts
- manage account owner context
- manage customer groups and aliases
- request requirement waivers
- manage communications and exceptions

Customer Operations User
- read customer service profiles
- perform eligibility checks
- view customer instructions and location requirements
- report customer exceptions

Customer Onboarding Reviewer
- review onboarding packets
- approve/reject customer activation
- request missing documents
- complete onboarding reviews

Customer Compliance Reviewer
- review compliance-related customer requirements
- approve compliance waivers
- review customer evidence status
- coordinate Compliance Core and RecordArr checks

Customer Quality Reviewer
- view quality-related customer requirements and holds
- coordinate AssurArr quality facts
- review customer quality exceptions

Customer Portal Support
- invite customer contacts
- manage customer portal role linkage
- suspend/revoke portal access references

Customer Admin
- full CustomArr configuration and management
- manage customer fieldsets, requirement templates, merge rules, and integrations
```
