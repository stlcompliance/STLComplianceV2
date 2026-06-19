# OrdArr - Order and Request Model

## OrderRequest

OrderRequest is the parent business object for a customer order, internal request, service request, or work request.

Fields:

- orderRequestId
- orderRequestNumber
- tenantId
- requestType
  - customer_order
  - internal_request
  - service_request
  - work_request
  - fulfillment_request
  - transport_request
  - maintenance_request
  - procurement_request
- sourceChannel
  - manual_entry
  - customer_portal
  - internal_portal
  - api
  - import
  - integration
  - product_handoff
- sourceProductKey
- customerRef
- billToCustomerRef
- shipToCustomerLocationRef
- requesterPersonRef
- requesterTeamRef
- internalLocationRef
- primaryContactRef
- title
- description
- priority
  - low
  - normal
  - high
  - urgent
  - emergency
- requestedAt
- neededBy
- promisedBy
- status
- lifecycleCategory
- serviceEligibilitySnapshot
- complianceRequirementSnapshot
- ownerPersonRef
- ownerTeamRef
- relatedProductRefs
- externalReferenceRefs
- tags
- createdAt
- updatedAt

Customer refs must point to CustomArr. Internal person and location refs must point to StaffArr.

## MVP header fields currently implemented

The OrdArr workspace and API currently support:

- customerRef
- customerName
- requestType
- sourceChannel
- orderType
- priority
- ownerPersonId
- buyerPoNumber
- billToRef
- shipToRef
- paymentTerms
- shippingMethodPreference
- requested / promised windows
- customerNotes
- internalNotes
- sourceReference
- order lines on create

## OrderLine

OrderLine describes requested goods, services, work, movement, or deliverables inside an OrderRequest.

Fields:

- orderLineId
- orderRequestId
- lineNumber
- lineType
  - item
  - service
  - labor
  - transport
  - maintenance
  - inspection
  - procurement
  - document
  - other
- itemRef
- itemSnapshot
- description
- quantity
- unitOfMeasure
- requestedDate
- neededBy
- sourceRequirementRef
- targetProductKey
- targetHandoffRef
- fulfillmentStatusSnapshot
- status
- createdAt
- updatedAt

SupplyArr owns tenant commercial item/part/material/SKU context. ReferenceDataCore owns shared public identifiers and taxonomies. LoadArr owns inventory balances and movement.

## OrderPartyRole

OrderPartyRole records how external and internal parties participate in an order without merging their source records.

Fields:

- orderPartyRoleId
- orderRequestId
- roleType
  - sold_to
  - bill_to
  - ship_to
  - consignee
  - shipper
  - pickup
  - delivery
  - requester
  - approver
  - internal_owner
  - supplier
  - carrier
  - broker
- partyRef
- contactRef
- locationRef
- displaySnapshot
- sourceProductKey
- statusSnapshot
- snapshotAt

Customer roles use CustomArr refs. Supplier/vendor roles use SupplyArr refs. Internal roles use StaffArr refs.

## OrderRequirement

OrderRequirement is an order-scoped requirement, blocker, warning, or approval need compiled from source products.

Fields:

- orderRequirementId
- orderRequestId
- sourceProductKey
- sourceRequirementRef
- requirementType
  - customer_requirement
  - compliance_requirement
  - document_requirement
  - approval_requirement
  - inventory_requirement
  - dispatch_requirement
  - maintenance_requirement
  - procurement_requirement
  - quality_requirement
- severity
  - info
  - warning
  - approval_required
  - blocked
- status
  - open
  - satisfied
  - waived
  - blocked
  - not_applicable
  - unknown
- requiredAction
- ownerProductKey
- ownerQueueRef
- resolvedByRef
- resolvedAt
- evidenceRefs
- notes

OrdArr may store the compiled order requirement, but the source product still owns requirement meaning and clearing actions.

## OrderException

OrderException records order-level issues that affect orchestration, communication, or closeout.

Fields:

- orderExceptionId
- orderRequestId
- sourceProductKey
- sourceRecordRef
- exceptionType
  - intake_issue
  - customer_issue
  - eligibility_blocker
  - inventory_shortage
  - dispatch_exception
  - maintenance_blocker
  - procurement_delay
  - document_missing
  - quality_hold
  - compliance_blocker
  - billing_packet_issue
- severity
  - info
  - warning
  - blocked
  - critical
- status
  - open
  - acknowledged
  - in_review
  - waiting_on_source
  - resolved
  - cancelled
- summary
- ownerProductKey
- ownerPersonRef
- ownerTeamRef
- requiredAction
- openedAt
- resolvedAt
- resolutionSummary

## ExternalOrderMapping

ExternalOrderMapping links an OrdArr order/request to outside systems without making external IDs canonical STL IDs.

Fields:

- externalOrderMappingId
- orderRequestId
- externalSystem
- externalEntityType
- externalId
- mappingStatus
  - candidate
  - active
  - stale
  - failed
  - retired
- syncDirection
  - inbound
  - outbound
  - bidirectional
  - read_only
  - writeback
- lastVerifiedAt
- lastSyncAt
- lastError

External finance systems own invoices, bills, payments, taxes, ledger, and accounting close.

## MVP notes

- Order lines are persisted in the in-memory OrdArr store for the current workspace.
- Holds, timeline entries, and return records are coordinated by OrdArr and can reference other products, but they do not transfer ownership of those products' canonical records.
- Full quote versioning, advanced pricing, and customer-portal orchestration are deferred to a later phase.
