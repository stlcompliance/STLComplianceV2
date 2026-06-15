# CustomArr — Scope, Ownership, and Boundaries

## Product purpose

CustomArr is the customer master and customer relationship control plane for tenant-owned business customers.

CustomArr is the source of truth for customers of tenants. These are not STL Compliance platform tenants and not internal employees. They are the external organizations, accounts, locations, contacts, consignees, shippers, bill-to parties, ship-to parties, service recipients, and customer-specific requirements that tenant operations reference across the suite.

CustomArr answers:

- Who are this tenant's customers?
- Which customer account is the canonical account?
- Which customer names, aliases, external IDs, and legacy references resolve to the same account?
- Which customer sites, bill-to locations, ship-to locations, pickup locations, dropoff locations, and service locations exist?
- Which customer contacts exist and what are they allowed to approve, receive, sign, request, or view?
- Is this customer active, inactive, on hold, blocked, limited, or archived?
- Is this customer/location eligible for order creation, dispatch, delivery, service, release, or other product workflows?
- What customer-specific requirements must be satisfied before work proceeds?
- What documents, contracts, requirements, preferences, holds, and exceptions are attached to the customer relationship?
- Which owning product should enforce a requirement or resolve a blocker?

## CustomArr owns

```text
- Tenant customer master
- Customer accounts
- Customer account hierarchy
- Customer groups
- Customer aliases
- Customer external system mappings
- Customer account status
- Customer onboarding status
- Customer service eligibility snapshot
- Customer operational hold status
- Customer contact master
- Customer contact authorization scope
- Customer communication preferences
- Customer portal contact linkage references
- Customer external locations
- Customer bill-to / ship-to / pickup / dropoff / service location identity
- Customer location status
- Customer location hours and access instructions
- Customer-specific operational requirements
- Customer-specific contractual requirement references
- Customer-specific safety / quality / documentation requirements
- Customer-specific service restrictions
- Customer-specific preferences
- Customer contract references and summaries
- Customer requirement waiver records
- Customer approval records
- Customer relationship risk snapshot
- Customer relationship notes and communications
- Customer duplicate detection and merge history
- Customer-origin events
- Customer audit trail
```

## CustomArr does not own

```text
- Platform tenant identity
- Platform login
- Tenant entitlement
- Internal person master
- Internal permission assignment truth
- Internal StaffArr location identity
- Employee training/certification truth
- Supplier/vendor master
- Procurement truth
- Purchase requests
- Purchase orders
- Item/product master
- Inventory balance
- Stock ledger
- Receiving
- Putaway
- Pick/issue/ship execution truth
- Customer order lifecycle
- Dispatch/route/trip execution
- Maintenance execution
- Asset readiness
- Quality hold/release truth
- Regulatory/rulepack meaning
- Actual document/file/evidence object
- Reporting read models
- Accounting execution
- General ledger
- Accounts receivable ledger
- Invoices
- Payments
- Tax calculation
- Credit ledger
- Sales opportunity pipeline unless explicitly added later
```

## Important naming boundary

```text
CustomArr customers
- External customers belonging to a tenant.
- Examples: a pallet broker's customers, a shipper, consignee, bill-to account, delivery recipient, service customer, facility customer, or contracted business account.

NexArr tenants
- STL Compliance platform tenants.
- Examples: tenant organizations that subscribe to STL Compliance products.

StaffArr people
- Internal people/person records used by the tenant.
- Examples: employees, technicians, managers, operators, dispatchers, warehouse users.

CustomArr contacts
- External contacts at customer accounts and customer locations.
- Examples: receiver, buyer, customer operations contact, customer compliance contact, billing contact, emergency contact.
```

## External product dependencies

```text
NexArr
- Product entitlement
- Login/handoff
- Service tokens
- Customer portal identity linkage when external customer contacts can log in

StaffArr
- Internal person references
- Internal account owner references
- Internal sales / operations owner references
- Internal approver references
- Product permission assignments
- Internal site/location references only when assigning responsibility

TrainArr
- Training/qualification status for tenant people who must satisfy customer-specific requirements
- Customer-required training assignment requests
- Remediation requests caused by customer incidents or failed requirements

Compliance Core
- Governing body catalogs
- Rulepacks
- Regulatory evaluations
- Regulatory meaning for customer-specific compliance requirements
- Evidence requirement definitions
- Requirement evaluation support when requirements have regulatory meaning

RecordArr
- Customer contracts
- Customer onboarding documents
- Insurance certificates
- Tax exemption documents
- Customer policies
- Customer correspondence records
- Customer requirement evidence
- Customer approval evidence
- Uploaded customer files

OrdArr
- Customer order lifecycle
- Customer order creation checks
- Customer order status facts
- Bill-to / ship-to / sold-to / consignee references

RoutArr
- Route/trip execution
- Pickup/dropoff customer location references
- Appointment and delivery exception facts
- Route customer impact facts

LoadArr
- Fulfillment / warehouse execution
- Ship-to customer location references
- Dock / staging / load facts tied to customer orders
- Shipment readiness facts where customer requirements affect release

MaintainArr
- Customer-owned asset references when tenant services customer assets
- Customer impact facts on work orders, defects, downtime, and service readiness

SupplyArr
- Supplier/vendor master
- Supplier linkage when the same legal entity is both customer and supplier
- Sourcing facts only when needed for customer-specific sourcing requirements

AssurArr
- Quality holds
- Nonconformance
- CAPA
- Customer complaint facts
- Quality release decisions

ReportArr
- Customer dashboards
- Customer KPIs
- Cross-product reporting views

Field Companion
- Mobile customer site check-in
- Customer signatures
- Customer contact confirmation
- Photo/evidence capture at customer locations
- Mobile execution of customer requirement prompts
```

## Core source-of-truth rules

```text
1. CustomArr owns tenant customer/account identity.
2. CustomArr owns external customer location identity.
3. CustomArr owns external customer contact identity.
4. CustomArr owns customer account status, customer location status, and customer relationship status.
5. CustomArr owns customer-specific requirements, preferences, restrictions, and operational hold records.
6. NexArr owns STL Compliance tenant identity and login.
7. StaffArr owns internal people and internal locations.
8. CustomArr must not create canonical StaffArr internal locations.
9. CustomArr customer contacts are not StaffArr people unless explicitly linked through a supported external-identity pattern.
10. OrdArr owns customer order lifecycle and references CustomArr customers, locations, and contacts.
11. RoutArr owns route/trip execution and references CustomArr pickup/dropoff/customer locations.
12. LoadArr owns warehouse execution and references CustomArr ship-to / consignee / customer locations when required.
13. SupplyArr owns supplier/vendor truth. If the same legal entity is both a customer and a supplier, CustomArr and SupplyArr records are linked, not merged.
14. MaintainArr owns maintenance execution and references CustomArr only for customer-owned assets or customer impact.
15. AssurArr owns quality hold/release decisions. CustomArr may surface customer-facing hold status snapshots but does not decide quality release.
16. Compliance Core owns regulatory meaning. CustomArr owns the customer requirement record and links to Compliance Core when regulatory interpretation is needed.
17. RecordArr owns actual document/file/evidence objects. CustomArr stores references, summaries, metadata, and status snapshots.
18. Accounting systems own invoices, payments, ledger, credit, and tax calculation. CustomArr may store customer account references and operational hold snapshots from accounting.
19. ReportArr owns reporting views, not customer truth.
20. No product should store free-text customer identity when a CustomArr customerRef can be used.
```

## Standard CustomArr object envelope

Every major CustomArr object should include:

```text
CustomArrObject
- id
- tenantId
- objectNumber
- objectType
- status
- title
- description
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- sourceProduct
- sourceObjectRef
- customerRef
- customerLocationRef
- customerContactRef
- recordRefs
- complianceRefs
- auditTrail
- eventLog
```

## Standard structured reference

```text
SuiteRef
- productKey
- objectType
- objectId
- humanReadableNumber
- displayNameSnapshot
- statusSnapshot
- versionSnapshot
- lastResolvedAt
```

## Standard customer reference

```text
CustomerRef
- customerId
- customerNumber
- displayNameSnapshot
- legalNameSnapshot
- statusSnapshot
- serviceEligibilitySnapshot
- versionSnapshot
- lastResolvedAt
```

## Standard customer location reference

```text
CustomerLocationRef
- customerLocationId
- customerId
- locationNumber
- displayNameSnapshot
- locationTypeSnapshot
- addressSnapshot
- statusSnapshot
- serviceEligibilitySnapshot
- versionSnapshot
- lastResolvedAt
```

## Standard customer contact reference

```text
CustomerContactRef
- customerContactId
- customerId
- displayNameSnapshot
- titleSnapshot
- contactTypeSnapshot
- authorizationScopeSnapshot
- statusSnapshot
- versionSnapshot
- lastResolvedAt
```

## CustomArr object prefixes

```text
CUS     Customer account
CGRP    Customer group
CAL     Customer alias
CEXT    Customer external system mapping
CLOC    Customer location
CCON    Customer contact
CMET    Customer contact method
CHRS    Customer location hours
CACC    Customer access requirement
CREQ    Customer requirement
CEVL    Customer requirement evaluation
CWAV    Customer requirement waiver
CPRF    Customer preference
CSVC    Customer service profile
CHLD    Customer hold
CCTR    Customer contract reference
CONB    Customer onboarding
CAPR    Customer approval
CRSK    Customer risk profile
CCOM    Customer communication log
CEXC    Customer exception
CMRG    Customer merge record
CINV    Customer portal invite
```


---

# CustomArr — Customer Account and Profile Model

## Customer account

A CustomerAccount is the canonical customer identity for a tenant's external customer. It may represent a legal entity, business account, bill-to party, shipper, consignee, broker customer, service recipient, government entity, nonprofit, individual customer, or one-time customer.

```text
CustomerAccount
- customerId
- tenantId
- customerNumber
- legalName
- displayName
- dbaNames
- shortName
- description
- customerType
  - business
  - individual
  - government
  - nonprofit
  - internal_affiliate
  - brokered_customer
  - consignee_only
  - shipper_only
  - bill_to_only
  - ship_to_only
  - one_time
  - other
- accountClass
  - strategic
  - key_account
  - standard
  - managed
  - house_account
  - one_time
  - test
  - other
- relationshipRole
  - buyer
  - bill_to
  - ship_to
  - sold_to
  - consignee
  - shipper
  - broker_customer
  - service_recipient
  - third_party
  - mixed
- status
  - prospect
  - onboarding
  - active
  - inactive
  - on_hold
  - blocked
  - suspended
  - archived
- onboardingStatus
  - not_started
  - in_progress
  - awaiting_customer
  - awaiting_documents
  - awaiting_internal_review
  - awaiting_approval
  - approved
  - rejected
  - waived
  - not_required
- serviceEligibilityStatus
  - eligible
  - limited
  - blocked
  - pending_review
  - unknown
- complianceStatus
  - compliant
  - warning
  - noncompliant
  - not_applicable
  - unknown
- operationalRiskLevel
  - low
  - normal
  - elevated
  - high
  - critical
  - unknown
- parentCustomerId
- rootCustomerId
- hierarchyPath
- customerGroupRefs
- primaryContactId
- billingContactId
- operationsContactId
- complianceContactId
- qualityContactId
- escalationContactId
- emergencyContactId
- defaultBillToLocationId
- defaultShipToLocationId
- defaultServiceLocationId
- defaultPickupLocationId
- defaultDropoffLocationId
- accountOwnerPersonId
- operationsOwnerPersonId
- complianceOwnerPersonId
- customerTags
- industry
- naicsCode
- sicCode
- website
- externalSystemRefs
- accountingCustomerRef
- crmRef
- erpRef
- taxDocumentRefs
- contractRefs
- requirementRefs
- preferenceRefs
- serviceProfileRef
- holdRefs
- riskProfileRef
- documentRefs
- noteRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- activatedAt
- activatedByPersonId
- archivedAt
- archivedByPersonId
- archiveReason
- auditTrail
```

## Customer account status definitions

```text
prospect
- Customer may be evaluated or onboarded but is not yet usable for active operations.

onboarding
- Customer is being set up, reviewed, or approved.

active
- Customer may be used in downstream workflows if service eligibility allows.

inactive
- Customer is intentionally not in normal use but retained for history and possible reactivation.

on_hold
- Customer has one or more active holds that restrict at least one workflow.

blocked
- Customer is blocked from operational use until an authorized release or override occurs.

suspended
- Customer relationship is temporarily suspended for business, compliance, quality, legal, or operational reasons.

archived
- Customer record is retained for history only and should not be selected in new operational workflows.
```

## Service eligibility definitions

```text
eligible
- Customer can be used normally for permitted workflows.

limited
- Customer can be used only with restrictions, approvals, requirements, or allowed product scopes.

blocked
- Customer cannot be used for blocked workflows.

pending_review
- Customer cannot be assumed eligible until review is completed.

unknown
- Eligibility has not been calculated or required inputs are missing.
```

## Customer account detail page sections

```text
CustomerAccountDetail
- Header
  - customerNumber
  - displayName
  - legalName
  - status
  - serviceEligibilityStatus
  - complianceStatus
  - operationalRiskLevel
  - active holds
  - open requirements
- Identity
  - legal name
  - DBA names
  - account class
  - customer type
  - relationship role
  - aliases
  - external references
- Hierarchy
  - parent customer
  - child customers
  - customer groups
  - related supplier/vendor link if applicable
- Contacts
  - primary contact
  - billing contact
  - operations contact
  - compliance contact
  - quality contact
  - escalation contact
- Locations
  - bill-to locations
  - ship-to locations
  - service locations
  - pickup/dropoff locations
  - blocked/limited locations
- Service profile
  - allowed products
  - blocked products
  - eligible workflows
  - restrictions
  - required approvals
- Requirements
  - active requirements
  - missing evidence
  - waivers
  - requirement evaluation history
- Contracts and documents
  - contract refs
  - onboarding docs
  - insurance docs
  - tax docs
  - customer policy docs
- Holds and exceptions
  - active holds
  - resolved holds
  - customer exceptions
  - customer complaints or quality facts
- Preferences
  - communication preferences
  - scheduling preferences
  - delivery/pickup preferences
  - document delivery preferences
- Timeline
  - status changes
  - requirement changes
  - hold changes
  - onboarding events
  - merge/split events
```

## Customer account creation workflow

```text
1. User selects customer type and relationship role.
2. User enters required identity fields.
3. CustomArr checks duplicate candidates by legal name, DBA, address, phone, email domain, and external refs.
4. User confirms new customer or links to existing customer.
5. User selects account owner and operational owner from StaffArr person references.
6. User adds primary contact.
7. User adds at least one relevant customer location when required by relationship role.
8. User adds bill-to / ship-to / service defaults where applicable.
9. User attaches onboarding documents through RecordArr if required.
10. CustomArr evaluates onboarding requirements and service eligibility.
11. Required approvals are requested.
12. Customer becomes active, limited, or blocked based on review results.
```

## Customer group

Customer groups let tenant operations group customers without changing canonical account identity.

```text
CustomerGroup
- customerGroupId
- tenantId
- groupNumber
- name
- description
- groupType
  - parent_company
  - buying_group
  - franchise_network
  - region
  - route_group
  - billing_group
  - reporting_group
  - service_group
  - compliance_group
  - custom
- status
  - active
  - inactive
  - archived
- parentGroupId
- memberCustomerRefs
- ownerPersonId
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- auditTrail
```

## Customer alias

Aliases prevent duplicate customer creation and allow products/imports to resolve legacy names to the canonical customer.

```text
CustomerAlias
- aliasId
- tenantId
- customerId
- aliasType
  - legal_name
  - dba
  - short_name
  - legacy_name
  - import_name
  - misspelling
  - acquired_company
  - former_name
  - external_system_name
  - other
- aliasValue
- normalizedAliasValue
- status
  - active
  - inactive
  - blocked
- sourceProduct
- sourceObjectRef
- createdAt
- createdByPersonId
- retiredAt
- retiredByPersonId
- auditTrail
```

## Customer external system mapping

External mappings connect CustomArr's canonical customer to accounting, ERP, CRM, WMS, TMS, carrier, customer portal, or imported legacy IDs.

```text
CustomerExternalSystemMapping
- mappingId
- tenantId
- customerId
- customerLocationId
- customerContactId
- externalSystemName
- externalSystemType
  - accounting
  - erp
  - crm
  - wms
  - tms
  - ecommerce
  - customer_portal
  - legacy_import
  - edi
  - other
- externalKey
- externalDisplayName
- status
  - active
  - inactive
  - conflict
  - retired
- sourceProduct
- lastVerifiedAt
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- auditTrail
```

## Customer merge record

CustomArr owns duplicate resolution for customer master data.

```text
CustomerMergeRecord
- mergeId
- tenantId
- survivorCustomerId
- mergedCustomerIds
- mergeReason
- mergeStrategy
  - manual_review
  - imported_duplicate
  - acquisition
  - external_system_reconciliation
  - cleanup
- fieldResolutionSummary
- movedLocationRefs
- movedContactRefs
- movedRequirementRefs
- movedRecordRefs
- movedExternalMappingRefs
- status
  - proposed
  - approved
  - completed
  - canceled
  - reversed
- proposedByPersonId
- approvedByPersonId
- completedAt
- completedByPersonId
- reversalReason
- auditTrail
```


---

# CustomArr — Contacts and External Customer Locations Model

## Customer contact

A CustomerContact is an external person/contact point belonging to a tenant customer. It is not a StaffArr person unless linked through an external identity pattern.

```text
CustomerContact
- contactId
- tenantId
- customerId
- contactNumber
- displayName
- firstName
- lastName
- title
- department
- contactType
  - primary
  - billing
  - operations
  - receiving
  - shipping
  - procurement
  - safety
  - compliance
  - quality
  - executive
  - escalation
  - emergency
  - after_hours
  - site_access
  - customer_portal_admin
  - other
- status
  - active
  - inactive
  - do_not_contact
  - blocked
  - archived
- preferredChannel
  - email
  - phone
  - sms
  - portal
  - in_person
  - edi
  - none
  - unknown
- preferredLanguage
- timeZone
- authorizationScopes
  - place_orders
  - approve_order_changes
  - approve_dispatch_changes
  - receive_delivery
  - sign_proof_of_delivery
  - approve_service_work
  - approve_quality_release
  - receive_quality_notice
  - receive_safety_notice
  - receive_compliance_notice
  - receive_invoices
  - submit_service_request
  - approve_requirement_waiver
  - emergency_contact
  - portal_admin
- relatedLocationRefs
- primaryLocationId
- contactMethodRefs
- communicationPreferenceRefs
- hasPortalLogin
- nexarrExternalIdentityRef
- portalAccessStatus
  - none
  - invited
  - active
  - suspended
  - revoked
- externalSystemRefs
- notes
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- archivedAt
- archivedByPersonId
- archiveReason
- auditTrail
```

## Customer contact method

```text
CustomerContactMethod
- contactMethodId
- tenantId
- customerId
- contactId
- methodType
  - email
  - phone
  - mobile
  - fax
  - portal
  - edi
  - webhook
  - mailing_address
  - other
- value
- normalizedValue
- label
- priority
  - primary
  - secondary
  - backup
  - emergency
- status
  - active
  - inactive
  - invalid
  - do_not_use
- verifiedAt
- verifiedByPersonId
- verificationSource
  - manual
  - import
  - customer_portal
  - email_confirmation
  - phone_confirmation
  - external_system
- notes
- createdAt
- updatedAt
```

## Customer contact authorization

Contact authorization prevents products from treating every customer contact as an approver.

```text
CustomerContactAuthorization
- authorizationId
- tenantId
- customerId
- contactId
- authorizationType
  - place_orders
  - approve_order_changes
  - approve_dispatch_changes
  - sign_delivery
  - receive_delivery
  - approve_service_work
  - approve_quote
  - approve_quality_release
  - receive_compliance_notice
  - receive_invoices
  - portal_admin
  - emergency_contact
- appliesToLocationRefs
- appliesToProductKeys
- status
  - active
  - inactive
  - expired
  - revoked
- source
  - manual
  - contract
  - customer_policy
  - onboarding
  - import
  - portal_request
- evidenceRecordRefs
- effectiveAt
- expiresAt
- grantedByPersonId
- revokedByPersonId
- revokedAt
- revocationReason
- auditTrail
```

## Customer location

A CustomerLocation is an external location belonging to, used by, or associated with a tenant customer. It is not a StaffArr internal location.

```text
CustomerLocation
- customerLocationId
- tenantId
- customerId
- locationNumber
- name
- description
- locationType
  - headquarters
  - billing
  - shipping
  - receiving
  - service_site
  - pickup
  - dropoff
  - consignee
  - shipper
  - warehouse
  - yard
  - plant
  - terminal
  - jobsite
  - retail_location
  - office
  - crossdock
  - other
- status
  - draft
  - active
  - inactive
  - on_hold
  - blocked
  - archived
- serviceEligibilityStatus
  - eligible
  - limited
  - blocked
  - pending_review
  - unknown
- complianceStatus
  - compliant
  - warning
  - noncompliant
  - not_applicable
  - unknown
- addressLine1
- addressLine2
- city
- region
- postalCode
- countryCode
- addressNormalized
- addressValidationStatus
  - unvalidated
  - valid
  - corrected
  - invalid
  - partial
- latitude
- longitude
- geocodeAccuracy
  - rooftop
  - street
  - city
  - postal_code
  - unknown
- timeZone
- primaryContactId
- receivingContactId
- shippingContactId
- siteAccessContactId
- emergencyContactId
- locationHoursRefs
- accessRequirementRefs
- customerRequirementRefs
- appointmentRequired
- appointmentInstructions
- dockInstructions
- gateInstructions
- checkInInstructions
- accessInstructions
- driverInstructions
- parkingInstructions
- equipmentConstraints
- vehicleConstraints
- deliveryConstraints
- pickupConstraints
- photosAllowed
- signatureRequired
- externalSystemRefs
- documentRefs
- noteRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- activatedAt
- activatedByPersonId
- archivedAt
- archivedByPersonId
- archiveReason
- auditTrail
```

## Customer location status definitions

```text
draft
- Location is being created and has not been approved for operational use.

active
- Location may be used in downstream workflows if eligibility allows.

inactive
- Location exists but should not be selected for normal use.

on_hold
- Location has an active hold restricting one or more workflows.

blocked
- Location is blocked from operational use until release or override.

archived
- Location is retained for history only.
```

## Customer location hours

```text
CustomerLocationHours
- hoursId
- tenantId
- customerId
- customerLocationId
- hoursType
  - receiving
  - shipping
  - office
  - appointment
  - service
  - after_hours
  - holiday
  - custom
- dayOfWeek
  - monday
  - Tuesday
  - Wednesday
  - Thursday
  - Friday
  - Saturday
  - Sunday
  - holiday
- opensAtLocal
- closesAtLocal
- closed
- appointmentOnly
- notes
- effectiveAt
- expiresAt
- createdAt
- updatedAt
```

## Customer access requirement

```text
CustomerAccessRequirement
- accessRequirementId
- tenantId
- customerId
- customerLocationId
- title
- description
- requirementType
  - appointment
  - gate_code
  - check_in
  - badge
  - escort
  - ppe
  - safety_video
  - training
  - background_check
  - vehicle_restriction
  - trailer_restriction
  - temperature_control
  - photos_restricted
  - signature
  - document
  - other
- severity
  - info
  - warning
  - block
- appliesToProductKeys
- appliesToWorkflowKeys
- status
  - draft
  - active
  - waived
  - expired
  - retired
- sourceType
  - manual
  - customer_policy
  - contract
  - onboarding
  - import
  - external_system
- evidenceRecordRefs
- complianceRefs
- effectiveAt
- expiresAt
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- auditTrail
```

## Customer portal invite

Customer contacts can be linked to a customer portal identity, but CustomArr does not own authentication.

```text
CustomerPortalInvite
- inviteId
- tenantId
- customerId
- contactId
- email
- portalRole
  - customer_viewer
  - customer_requester
  - customer_approver
  - customer_admin
  - customer_billing_viewer
  - customer_quality_contact
  - customer_compliance_contact
- status
  - draft
  - sent
  - accepted
  - expired
  - revoked
  - failed
- nexarrInviteRef
- nexarrExternalIdentityRef
- invitedByPersonId
- invitedAt
- acceptedAt
- expiresAt
- revokedAt
- revokedByPersonId
- revokeReason
- auditTrail
```

## Customer contact/location workflows

```text
Contact creation workflow
1. User selects customer account.
2. User enters contact identity and role.
3. User adds at least one contact method.
4. User selects authorization scopes.
5. User links contact to customer locations if applicable.
6. CustomArr validates duplicate contact candidates.
7. CustomArr saves contact and emits contact created event.
8. Portal invite may be proposed if contact needs access.

Location creation workflow
1. User selects customer account.
2. User chooses location type.
3. User enters address and operating instructions.
4. CustomArr validates address and duplicate location candidates.
5. User assigns primary/receiving/shipping contacts.
6. User adds hours, access rules, appointment requirements, and constraints.
7. CustomArr evaluates location eligibility.
8. Location becomes active, limited, blocked, or pending review.
```


---

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


---

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
  - active
  - suspended
  - revoked
  - pending
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
1. User creates customer account in draft/onboarding status.
2. CustomArr checks duplicate accounts and aliases.
3. User adds required contacts and locations.
4. CustomArr determines onboarding checklist from customer type, relationship role, location type, and tenant configuration.
5. User attaches required documents through RecordArr.
6. CustomArr evaluates requirements and asks owning products for checks as needed.
7. Reviewer approves, rejects, or requests more information.
8. CustomArr activates customer or sets limited/blocked eligibility.
9. CustomArr emits customer activated or customer blocked event.

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


---

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
9. Customer becomes active, limited, blocked, or remains onboarding.
10. CustomArr emits customer created and status/eligibility events.
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
- Customer or location status is blocked.
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
customarr.customer.blocked
customarr.customer.unblocked
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
- POST /handoff/redeem
- POST /service-tokens/introspect
- GET /entitlements/{productKey}
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
