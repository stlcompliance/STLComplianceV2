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
