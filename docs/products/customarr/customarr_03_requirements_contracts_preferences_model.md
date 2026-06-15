# CustomArr — Requirements, Contracts, Preferences, Service Rules, and Holds Model

## Customer requirement

A CustomerRequirement is a customer-specific requirement that may apply before a product workflow proceeds. It can be contractual, operational, safety-related, quality-related, documentation-related, or regulatory-adjacent. CustomArr owns the customer requirement record, while Compliance Core owns regulatory meaning when regulation interpretation is needed.

```text
CustomerRequirement
- requirementId
- tenantId
- customerId
- customerLocationId
- requirementNumber
- title
- description
- requirementType
  - documentation
  - insurance
  - tax_document
  - contract
  - training
  - ppe
  - site_access
  - appointment
  - delivery_window
  - pickup_window
  - packaging
  - pallet_condition
  - temperature_control
  - chain_of_custody
  - photo_evidence
  - signature
  - proof_of_delivery
  - sds
  - permit
  - background_check
  - vehicle_type
  - equipment_type
  - communication
  - quality_release
  - invoice_delivery
  - labeling
  - edi
  - other
- sourceType
  - manual
  - customer_policy
  - contract
  - onboarding
  - import
  - external_system
  - compliance_core
  - assurarr
  - ordarr
  - routarr
  - loadarr
- appliesToProductKeys
  - customarr
  - ordarr
  - routarr
  - loadarr
  - maintainarr
  - assurarr
  - supplyarr
  - trainarr
  - recordarr
  - field_companion
- appliesToWorkflowKeys
- trigger
  - before_customer_activation
  - before_order_creation
  - before_order_acceptance
  - before_dispatch
  - before_pickup
  - before_delivery
  - before_fulfillment_release
  - before_quality_release
  - before_service_work
  - before_invoice
  - on_exception
  - always
- severity
  - info
  - warning
  - block
- status
  - draft
  - active
  - waived
  - expired
  - retired
  - superseded
- validationOwnerProduct
  - customarr
  - compliancecore
  - recordarr
  - trainarr
  - assurarr
  - ordarr
  - routarr
  - loadarr
  - external
- requiredEvidenceType
  - none
  - record
  - signature
  - photo
  - training_completion
  - approval
  - compliance_evaluation
  - external_confirmation
  - other
- evidenceRequirementRefs
- recordRefs
- complianceRefs
- defaultActionOnFail
  - warn
  - block
  - require_approval
  - require_waiver
  - create_exception
- waiverAllowed
- waiverApprovalType
- effectiveAt
- expiresAt
- reviewIntervalDays
- nextReviewAt
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- auditTrail
```

## Customer requirement evaluation

```text
CustomerRequirementEvaluation
- evaluationId
- tenantId
- customerId
- customerLocationId
- requirementId
- sourceProduct
- sourceWorkflowKey
- sourceObjectRef
- evaluatedAt
- evaluatedByPersonId
- evaluatedByServiceClientId
- result
  - pass
  - warning
  - fail
  - blocked
  - not_applicable
  - unknown
- resultReason
- missingEvidenceRefs
- satisfiedEvidenceRefs
- blockerRefs
- waiverRef
- requiredAction
- expiresAt
- auditTrail
```

## Customer requirement waiver

```text
CustomerRequirementWaiver
- waiverId
- tenantId
- customerId
- customerLocationId
- requirementId
- sourceProduct
- sourceObjectRef
- waiverReason
- scope
  - single_object
  - single_workflow
  - customer_location
  - customer_account
  - time_limited
- status
  - requested
  - approved
  - rejected
  - expired
  - revoked
  - canceled
- requestedByPersonId
- requestedAt
- approvedByPersonId
- approvedAt
- rejectedByPersonId
- rejectedAt
- rejectionReason
- effectiveAt
- expiresAt
- revokedByPersonId
- revokedAt
- revokeReason
- evidenceRecordRefs
- auditTrail
```

## Customer contract reference

CustomArr stores contract references and operational summaries. RecordArr owns the actual contract file; accounting owns invoices, payments, and ledger execution.

```text
CustomerContractRef
- contractRefId
- tenantId
- customerId
- customerLocationId
- contractNumberSnapshot
- contractTitle
- contractType
  - master_service_agreement
  - statement_of_work
  - rate_agreement
  - service_level_agreement
  - customer_policy
  - insurance_requirement
  - access_agreement
  - quality_agreement
  - other
- status
  - draft
  - active
  - pending_signature
  - expired
  - terminated
  - superseded
  - archived
- recordRef
- sourceSystemRef
- effectiveAt
- expiresAt
- renewalNoticeAt
- ownerPersonId
- summary
- serviceScopeSummary
- requirementRefs
- preferenceRefs
- holdRefs
- externalSystemRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- auditTrail
```

## Customer preference

Preferences guide product workflows but should not override active requirements or holds.

```text
CustomerPreference
- preferenceId
- tenantId
- customerId
- customerLocationId
- contactId
- preferenceType
  - communication
  - scheduling
  - delivery
  - pickup
  - appointment
  - document_delivery
  - notification
  - preferred_carrier
  - packaging
  - pallet_spec
  - labeling
  - inspection
  - photo_evidence
  - invoice_delivery
  - language
  - timezone
  - other
- preferenceKey
- preferenceValue
- priority
  - low
  - normal
  - high
  - required_if_possible
- appliesToProductKeys
- appliesToWorkflowKeys
- status
  - active
  - inactive
  - retired
- sourceType
  - manual
  - customer_policy
  - contract
  - onboarding
  - import
  - portal
  - external_system
- effectiveAt
- expiresAt
- notes
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- auditTrail
```

## Customer service profile

The service profile summarizes what downstream products can do with a customer or location.

```text
CustomerServiceProfile
- serviceProfileId
- tenantId
- customerId
- customerLocationId
- profileNumber
- status
  - draft
  - active
  - limited
  - blocked
  - retired
- serviceEligibilityStatus
  - eligible
  - limited
  - blocked
  - pending_review
  - unknown
- allowedProductKeys
- blockedProductKeys
- allowedWorkflowKeys
- blockedWorkflowKeys
- requiredApprovalTypes
- requiredRequirementRefs
- activeHoldRefs
- restrictions
- serviceLevel
  - standard
  - priority
  - emergency_only
  - limited
  - blocked
  - custom
- defaultBillToCustomerId
- defaultBillToLocationId
- defaultShipToLocationId
- defaultContactId
- accountingCustomerRef
- externalCreditStatusSnapshot
  - not_checked
  - ok
  - warning
  - hold
  - blocked
  - unknown
- externalCreditStatusSource
- lastEligibilityCalculatedAt
- lastEligibilityReason
- createdAt
- updatedAt
- auditTrail
```

## Customer hold

Customer holds restrict customer usage in one or more workflows. A hold can originate inside CustomArr or be mirrored from another product or external system. Quality release still belongs to AssurArr, and accounting execution still belongs to accounting systems.

```text
CustomerHold
- holdId
- tenantId
- customerId
- customerLocationId
- holdNumber
- title
- description
- holdType
  - operational
  - documentation
  - onboarding
  - compliance
  - safety
  - quality
  - accounting_external
  - legal
  - customer_requested
  - duplicate_review
  - system
  - other
- sourceProduct
- sourceObjectRef
- sourceSystemRef
- severity
  - low
  - moderate
  - high
  - critical
- status
  - active
  - resolved
  - overridden
  - canceled
  - expired
- blockCustomerActivation
- blockOrderCreation
- blockOrderAcceptance
- blockDispatch
- blockPickup
- blockDelivery
- blockFulfillmentRelease
- blockServiceWork
- blockQualityRelease
- blockInvoiceRelease
- requiredAction
- ownerPersonId
- createdAt
- createdByPersonId
- resolvedAt
- resolvedByPersonId
- overrideApprovalRef
- overrideReason
- expiresAt
- recordRefs
- auditTrail
```

## Customer requirement and hold workflows

```text
Requirement creation workflow
1. User selects customer account and optional customer location.
2. User selects requirement type and affected products/workflows.
3. User defines trigger, severity, evidence expectation, and validation owner.
4. CustomArr links Compliance Core refs if regulatory meaning exists.
5. CustomArr links RecordArr evidence requirements if documents are required.
6. Requirement is reviewed and activated.
7. Downstream products receive requirement change events.

Requirement evaluation workflow
1. Product calls CustomArr for an eligibility or requirement check.
2. CustomArr resolves customer, location, contact, active holds, and active requirements.
3. CustomArr delegates to RecordArr, TrainArr, Compliance Core, AssurArr, or other owner when needed.
4. CustomArr returns pass, warning, fail, blocked, not_applicable, or unknown with required actions.
5. Product either proceeds, warns, blocks, or requests approval/waiver.

Customer hold workflow
1. Hold is created manually, by import, by integration, or by another product event.
2. CustomArr recalculates customer and location service eligibility.
3. Downstream products receive hold and eligibility change events.
4. Workflows affected by hold are blocked or warned.
5. Authorized user resolves, overrides, or cancels hold.
6. CustomArr recalculates eligibility and emits release/update events.
```
