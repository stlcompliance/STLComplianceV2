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
- Customer identity is incomplete, exploratory, imported for review, or pre-onboarding.

onboarding
- Formal onboarding workflow has started and the customer is not yet generally usable.

active
- Customer may be used in downstream workflows if service eligibility allows.

inactive
- Customer is intentionally not in normal use but retained for history and possible reactivation.

archived
- Customer record is retained for history only and should not be selected in new operational workflows.
```

`CustomerAccount.status` is lifecycle state. It must not carry service eligibility answers such as `limited`, `blocked`, or `pending_review`; those belong to `serviceEligibilityStatus`, customer holds, restrictions, or requirements.

`CustomerAccount.onboardingStatus` is a denormalized account-header/search summary of the canonical `CustomerOnboarding.status`.

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
12. Customer status becomes active or remains in the appropriate lifecycle state.
13. serviceEligibilityStatus becomes eligible, limited, blocked, pending_review, or unknown.
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
