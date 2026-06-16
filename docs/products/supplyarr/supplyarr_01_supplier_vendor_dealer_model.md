# SupplyArr — Supplier, Vendor, Dealer, Contact, and Address Model

## Supplier

A Supplier is an external party that can provide materials, parts, goods, services, labor, transportation, repair, or other procurement needs.

SupplyArr may use the term supplier broadly while still supporting vendor/dealer/manufacturer/distributor/service provider distinctions.

```text
Supplier
- supplierId
- tenantId
- supplierNumber
- legalName
- displayName
- supplierType
  - vendor
  - dealer
  - manufacturer
  - distributor
  - service_provider
  - carrier
  - contractor
  - broker
  - consultant
  - customer_supplier
  - other
- status
  - prospect
  - onboarding
  - active
  - inactive
  - archived
- supplierEligibilityStatus
  - pending_approval
  - approved
  - restricted
  - suspended
  - blocked
  - unknown
- riskStatus
  - low
  - moderate
  - high
  - blocked
  - unknown
- complianceStatus
  - compliant
  - warning
  - noncompliant
  - missing_documents
  - expired_documents
  - unknown
- qualityStatus
  - acceptable
  - warning
  - on_hold
  - blocked
  - unknown
- taxIdRef
- paymentTermsSnapshot
- shippingTermsSnapshot
- primaryContactRef
- addressRefs
- documentRefs
- insuranceRecordRefs
- contractRecordRefs
- qualityStatusRef
- preferredFlag
- restrictedReason
- suspendedReason
- blockedReason
- onboardingChecklistRefs
- sourcingRecordRefs
- performanceSummaryRef
- externalFinancialRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- approvedAt
- approvedByPersonId
- suspendedAt
- suspendedByPersonId
- archivedAt
- auditTrail
```

## Supplier status definitions

```text
prospect
- Supplier exists as a potential source but is not ready for use.

onboarding
- Supplier is being set up and required information/documents are being collected.

active
- Supplier record is active and may be considered for use if supplierEligibilityStatus allows.

inactive
- Supplier is not normally used but remains in records.

archived
- Supplier retained for history only.
```

`Supplier.status` is lifecycle state. It must not carry approval or usage decisions. Those belong to `supplierEligibilityStatus`.

## Supplier eligibility status definitions

```text
pending_approval
- Supplier is awaiting approval.

approved
- Supplier may be used according to policies and restrictions.

restricted
- Supplier may be used only under certain conditions.

suspended
- Supplier cannot be used temporarily.

blocked
- Supplier cannot be used.

unknown
- Eligibility has not been calculated or required inputs are missing.
```

## Supplier contact

```text
SupplierContact
- contactId
- tenantId
- supplierId
- displayName
- firstName
- lastName
- title
- email
- phone
- mobilePhone
- contactRole
  - sales
  - support
  - accounting
  - compliance
  - quality
  - shipping
  - receiving
  - emergency
  - technical
  - warranty
  - dispatch
  - other
- primary
- status
  - active
  - inactive
  - archived
- notes
```

## Supplier address

SupplierAddress is postal, billing, remittance, and mailing address context.

```text
SupplierAddress
- addressId
- tenantId
- supplierId
- addressType
  - billing
  - remittance
  - shipping
  - warehouse
  - headquarters
  - service_location
  - pickup
  - return
  - other
- name
- line1
- line2
- city
- state
- postalCode
- country
- geoCoordinates
- status
  - active
  - inactive
  - archived
- instructions
- dockInstructions
- hoursOfOperation
```

## Supplier location

SupplierLocation is the operational external supplier/vendor/dealer location model used for pickup, delivery, return, service, warehouse, dock, and site workflows.

```text
SupplierLocation
- supplierLocationId
- tenantId
- supplierId
- locationNumber
- locationType
  - headquarters
  - billing
  - remittance
  - shipping
  - warehouse
  - service_location
  - pickup
  - return
  - dock
  - yard
  - other
- name
- addressRef
- addressSnapshot
- primaryContactRef
- receivingContactRef
- shippingContactRef
- dispatchContactRef
- status
  - active
  - inactive
  - blocked
  - archived
- appointmentRequired
- dockInstructions
- pickupInstructions
- deliveryInstructions
- returnInstructions
- hoursOfOperation
- accessRequirementRefs
- externalSystemRefs
- createdAt
- updatedAt
```

## Supplier onboarding checklist

```text
SupplierOnboardingChecklist
- checklistId
- tenantId
- supplierId
- status
  - open
  - in_progress
  - complete
  - blocked
  - canceled
- itemRefs
- assignedPersonId
- dueAt
- completedAt
```

## Supplier onboarding checklist item

```text
SupplierOnboardingChecklistItem
- checklistItemId
- checklistId
- itemType
  - profile
  - contact
  - address
  - tax_document
  - insurance
  - contract
  - compliance_document
  - quality_review
  - banking_reference
  - accounting_setup_reference
  - approval
  - other
- title
- required
- status
  - pending
  - submitted
  - accepted
  - rejected
  - waived
- recordRefs
- reviewerPersonId
- reviewedAt
- notes
```

## Supplier relationship note

```text
SupplierRelationshipNote
- noteId
- tenantId
- supplierId
- noteType
  - general
  - pricing
  - quality
  - compliance
  - delivery
  - negotiation
  - support
  - warning
- body
- visibility
  - internal
  - procurement
  - quality
  - compliance
- createdAt
- createdByPersonId
- pinned
```

## Supplier lifecycle status change

```text
SupplierStatusChange
- statusChangeId
- tenantId
- supplierId
- previousStatus
- newStatus
- reason
- changedByPersonId
- changedAt
- sourceProduct
- sourceObjectRef
- evidenceRecordRefs
```

## Supplier lifecycle workflow

```text
1. User creates Supplier.
2. Supplier starts as prospect or onboarding.
3. Required profile, contact, address, document, compliance, and quality items are collected.
4. RecordArr stores supplier documents.
5. Compliance Core evaluates document/evidence requirements where applicable.
6. AssurArr quality status is checked if required.
7. Approver sets supplierEligibilityStatus to approved, restricted, suspended, or blocked.
8. Approved supplier can be selected for sourcing and purchase orders.
```

## Supplier suspension/block workflow

```text
1. Compliance, quality, procurement, or admin issue occurs.
2. Supplier eligibility changes to restricted, suspended, or blocked.
3. Open sourcing/PR/PO usage is evaluated.
4. Products receive supplier eligibility event.
5. New purchasing may be blocked or require approval.
6. Existing POs may remain, pause, or cancel according to policy.
```

## Events

```text
supplyarr.supplier.created
supplyarr.supplier.updated
supplyarr.supplier.onboarding_started
supplyarr.supplier_eligibility.changed
supplyarr.supplier.inactivated
supplyarr.supplier.archived

supplyarr.supplier_contact.created
supplyarr.supplier_contact.updated
supplyarr.supplier_address.created
supplyarr.supplier_address.updated
supplyarr.supplier_location.created
supplyarr.supplier_location.updated
supplyarr.supplier_location.status_changed

supplyarr.supplier_onboarding.item_submitted
supplyarr.supplier_onboarding.item_accepted
supplyarr.supplier_onboarding.item_rejected
supplyarr.supplier_onboarding.completed

supplyarr.supplier.status_changed
supplyarr.supplier_eligibility.changed
```
