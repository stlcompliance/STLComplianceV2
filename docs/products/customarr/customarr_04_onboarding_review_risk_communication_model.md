# CustomArr — Onboarding, Review, Risk, Communication, and Exception Model

## Customer onboarding

Customer onboarding is the controlled setup process for a tenant customer. It collects customer identity, contacts, locations, requirements, documents, and approvals before operational use.

```text
CustomerOnboarding
- onboardingId
- tenantId
- customerId
- onboardingNumber
- title
- description
- onboardingType
  - new_customer
  - reactivation
  - new_location
  - customer_portal_setup
  - contract_refresh
  - requirement_refresh
  - import_review
  - other
- status
  - draft
  - started
  - submitted
  - in_review
  - waiting_customer
  - waiting_documents
  - waiting_internal_owner
  - waiting_approval
  - approved
  - rejected
  - canceled
- requestedByPersonId
- sponsorPersonId
- accountOwnerPersonId
- reviewerPersonIds
- approvalRefs
- requiredDocumentRefs
- receivedDocumentRefs
- missingDocumentRefs
- questionnaireAnswerRefs
- requirementRefs
- locationRefs
- contactRefs
- riskProfileRef
- serviceProfileRef
- dueAt
- submittedAt
- submittedByPersonId
- approvedAt
- approvedByPersonId
- rejectedAt
- rejectedByPersonId
- rejectionReason
- canceledAt
- canceledByPersonId
- cancelReason
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- auditTrail
```

`CustomerOnboarding.status` is the canonical onboarding workflow state. `CustomerAccount.onboardingStatus` is a denormalized summary used for account headers, search, and quick filters.

## Customer onboarding checklist item

```text
CustomerOnboardingChecklistItem
- checklistItemId
- tenantId
- onboardingId
- sequence
- title
- description
- itemType
  - identity
  - contact
  - location
  - document
  - requirement
  - approval
  - risk_review
  - service_profile
  - external_mapping
  - custom
- required
- ownerProduct
  - customarr
  - recordarr
  - compliancecore
  - trainarr
  - assurarr
  - staffarr
  - external
- status
  - not_started
  - in_progress
  - completed
  - failed
  - waived
  - not_applicable
- sourceObjectRef
- evidenceRecordRefs
- completedAt
- completedByPersonId
- failureReason
- waiverRef
- auditTrail
```

## Customer approval

```text
CustomerApproval
- approvalId
- tenantId
- customerId
- customerLocationId
- sourceProduct
- sourceObjectRef
- approvalType
  - onboarding
  - activation
  - reactivation
  - location_activation
  - requirement_waiver
  - hold_override
  - contract_acceptance
  - customer_merge
  - portal_access
  - service_exception
  - risk_acceptance
  - other
- status
  - pending
  - approved
  - rejected
  - canceled
  - expired
- requestedAt
- requestedByPersonId
- approverPersonId
- decisionAt
- decisionReason
- expiresAt
- evidenceRecordRefs
- auditTrail
```

## Customer risk profile

CustomArr owns customer relationship risk snapshots. It does not replace Compliance Core regulatory interpretation, AssurArr quality decisions, or accounting credit ledgers.

```text
CustomerRiskProfile
- riskProfileId
- tenantId
- customerId
- customerLocationId
- riskLevel
  - low
  - normal
  - elevated
  - high
  - critical
  - unknown
- safetyRiskLevel
  - low
  - normal
  - elevated
  - high
  - critical
  - unknown
- complianceRiskLevel
  - low
  - normal
  - elevated
  - high
  - critical
  - unknown
- qualityRiskLevel
  - low
  - normal
  - elevated
  - high
  - critical
  - unknown
- operationalRiskLevel
  - low
  - normal
  - elevated
  - high
  - critical
  - unknown
- accountingRiskSnapshot
  - not_checked
  - ok
  - warning
  - hold
  - blocked
  - unknown
- riskDrivers
- sourceSignals
- recommendedActions
- ownerPersonId
- lastReviewedAt
- lastReviewedByPersonId
- nextReviewAt
- status
  - current
  - stale
  - retired
- auditTrail
```

## Customer review

```text
CustomerReview
- reviewId
- tenantId
- customerId
- customerLocationId
- reviewNumber
- reviewType
  - onboarding_review
  - annual_review
  - requirement_review
  - contract_review
  - risk_review
  - hold_review
  - duplicate_review
  - compliance_review
  - quality_review
  - other
- status
  - scheduled
  - in_progress
  - completed
  - failed
  - canceled
- reviewerPersonId
- dueAt
- startedAt
- completedAt
- result
  - approved
  - approved_with_conditions
  - rejected
  - needs_followup
  - not_applicable
- summary
- actionItems
- followUpRefs
- evidenceRecordRefs
- createdAt
- updatedAt
- auditTrail
```

## Customer communication log

```text
CustomerCommunicationLog
- communicationId
- tenantId
- customerId
- customerLocationId
- contactId
- sourceProduct
- sourceObjectRef
- communicationType
  - note
  - call
  - email
  - sms
  - portal_message
  - meeting
  - edi_message
  - webhook
  - mailed_document
  - other
- direction
  - inbound
  - outbound
  - internal
- channel
  - phone
  - email
  - sms
  - portal
  - in_person
  - edi
  - other
- subject
- body
- visibility
  - internal
  - customer_visible
  - account_owner_only
  - compliance_only
  - quality_only
  - auditor_visible
- relatedRequirementRefs
- relatedHoldRefs
- relatedOrderRefs
- relatedRouteRefs
- relatedQualityRefs
- recordRefs
- occurredAt
- createdAt
- createdByPersonId
- editedAt
- editedByPersonId
- pinned
- auditTrail
```

## Customer exception

A CustomerException is a customer-facing issue or exception routed from any product. The owning product still owns its operational truth.

```text
CustomerException
- exceptionId
- tenantId
- customerId
- customerLocationId
- contactId
- exceptionNumber
- title
- description
- exceptionType
  - customer_complaint
  - access_denied
  - delivery_rejected
  - pickup_failed
  - appointment_missed
  - document_missing
  - requirement_failed
  - signature_dispute
  - quality_issue
  - safety_issue
  - order_issue
  - route_issue
  - service_issue
  - billing_question_external
  - duplicate_customer
  - other
- sourceProduct
- sourceObjectRef
- owningProduct
  - customarr
  - ordarr
  - routarr
  - loadarr
  - maintainarr
  - assurarr
  - supplyarr
  - recordarr
  - external
- severity
  - low
  - moderate
  - high
  - critical
- status
  - open
  - in_review
  - waiting_customer
  - waiting_internal_owner
  - routed
  - resolved
  - closed
  - canceled
- customerImpact
  - none
  - possible
  - confirmed
  - severe
- operationalImpact
  - none
  - low
  - moderate
  - high
  - critical
- requiredAction
- ownerPersonId
- dueAt
- resolvedAt
- resolvedByPersonId
- resolutionSummary
- recordRefs
- communicationRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- auditTrail
```

## Customer portal access record

```text
CustomerPortalAccessRecord
- portalAccessId
- tenantId
- customerId
- contactId
- nexarrExternalIdentityRef
- portalRole
  - customer_viewer
  - customer_requester
  - customer_approver
  - customer_admin
  - customer_billing_viewer
  - customer_quality_contact
  - customer_compliance_contact
- status
  - pending
  - active
  - suspended
  - revoked
- allowedCustomerLocationRefs
- authorizationRefs
- invitedByPersonId
- activatedAt
- suspendedAt
- suspendedByPersonId
- revokedAt
- revokedByPersonId
- revokeReason
- lastAccessSnapshotAt
- auditTrail
```

## Onboarding and review workflows

```text
Customer onboarding workflow
1. User creates customer account as prospect or onboarding.
2. CustomArr checks duplicate accounts and aliases.
3. User adds required contacts and locations.
4. CustomArr determines onboarding checklist from customer type, relationship role, location type, and tenant configuration.
5. User attaches required documents through RecordArr.
6. CustomArr evaluates requirements and asks owning products for checks as needed.
7. Reviewer approves, rejects, or requests more information.
8. CustomArr activates customer or sets limited/blocked eligibility.
9. CustomArr emits customer activated or customer service eligibility changed event.

Customer review workflow
1. Review is scheduled manually or by due date.
2. CustomArr gathers status, holds, requirements, documents, exceptions, and owner assignments.
3. Reviewer confirms whether account, contacts, locations, and requirements remain valid.
4. Reviewer updates risk profile and service profile.
5. Review creates action items or closes successfully.
6. Downstream products receive changed facts when eligibility or requirements change.

Customer exception routing workflow
1. Product reports a customer-facing exception to CustomArr.
2. CustomArr stores relationship context and customer impact.
3. CustomArr routes operational resolution to the owning product.
4. CustomArr tracks communication and customer-facing closure.
5. Owning product updates resolution facts.
6. CustomArr closes or escalates the exception.
```

## Customer portal access events

```text
customarr.customer_portal_access.created
customarr.customer_portal_access.updated
customarr.customer_portal_access.suspended
customarr.customer_portal_access.revoked
customarr.customer_portal_access.role_changed
customarr.customer_portal_access.location_scope_changed
```
