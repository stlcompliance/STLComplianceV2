# SupplyArr — Scope, Ownership, and Boundaries

## Product purpose

SupplyArr is the supplier, vendor, dealer, sourcing, purchase request, purchase order, and procurement workflow product for the STL Compliance / ARR suite.

SupplyArr is not the WMS. LoadArr receives, stores, moves, counts, reserves, picks, and issues inventory. SupplyArr decides and tracks how items/services are sourced and purchased.

SupplyArr answers:

- Who is this supplier/vendor/dealer?
- Is this supplier approved?
- What compliance documents are required?
- What items/services can this supplier provide?
- What vendor part number, manufacturer part number, price snapshot, lead time, and MOQ apply?
- What purchase request exists?
- Who approved or rejected the request?
- What purchase order exists?
- What is expected to be received?
- What supplier quality/performance status should procurement consider?
- What external financial/accounting reference exists?

## SupplyArr owns

```text
- Supplier master
- Vendor master
- Dealer master
- Supplier contacts
- Supplier addresses
- Supplier status
- Supplier compliance requirement tracking
- Supplier document requirement references
- Supplier onboarding workflow
- Supplier approval/restriction/suspension workflow
- Procurement item sourcing records
- Supplier item catalog
- Vendor part numbers
- Manufacturer part numbers
- Price snapshots
- Lead time snapshots
- MOQ/package quantity snapshots
- Approved substitutes
- Purchase requests
- Purchase request approval workflow
- Purchase orders
- Purchase order lifecycle
- Purchase order line state
- PO expected receipt publication to LoadArr
- Supplier performance snapshots
- Procurement-origin events
```

## SupplyArr does not own

```text
- Platform login
- Tenant entitlement
- Person master
- Permission assignment truth
- Canonical internal location identity
- Training/certification truth
- Regulatory/rulepack meaning
- Document/file storage truth
- Asset truth
- Work order truth
- Inventory balance
- Stock ledger
- Warehouse receiving execution
- Putaway
- Pick/issue
- Route/trip execution
- Customer master
- Customer order lifecycle
- Quality hold/release decision
- Analytics read models
- Accounting execution
```

## External product dependencies

```text
NexArr
- Product entitlement
- Login/handoff
- Service tokens

StaffArr
- Buyer/approver/requester person references
- Site/location references
- Permission checks
- Personnel incidents when procurement issue involves people/process behavior

TrainArr
- Buyer/approver qualification if special procurement workflow requires trained/qualified personnel

Compliance Core
- Supplier compliance requirements
- Procurement document requirements
- Regulated item sourcing rules
- Evidence requirements
- Retention rules

RecordArr
- Supplier documents
- Contracts
- Insurance certificates
- PO PDFs
- Quotes
- Supplier corrective action responses
- Procurement evidence packages

LoadArr
- Inventory shortages
- Replenishment signals
- Expected receipts
- Receipt status
- Receiving discrepancies
- Supplier receipt performance facts

MaintainArr
- Work order parts demand
- Maintenance part/service purchase requests
- Vendor maintenance support references

RoutArr
- Supplier pickup/delivery transportation context
- Inbound ETA/appointment context if RoutArr controls the move

CustomArr
- Customer-specific procurement requirements where applicable
- Customer-owned inventory/customer requirement context if needed

OrdArr
- Order-driven procurement demand
- Fulfillment blockers related to procurement

AssurArr
- Supplier quality issues
- Supplier holds/restrictions
- SCARs
- Nonconformance
- Supplier quality status

ReportArr
- Procurement dashboards
- Supplier performance KPIs
- PO cycle time
- Emergency purchase metrics

Field Companion
- Supplier document upload
- Mobile receiving evidence where delegated
- Photo/document capture for procurement evidence
```

## Core source-of-truth rules

```text
1. SupplyArr owns supplier/vendor/dealer master.
2. SupplyArr owns purchase request and purchase order lifecycle.
3. SupplyArr owns sourcing records and supplier-item relationships.
4. LoadArr owns receiving execution and inventory truth.
5. StaffArr owns internal location identity.
6. RecordArr owns supplier/procurement documents and files.
7. AssurArr owns supplier quality nonconformance, holds, SCARs, and release decisions.
8. Compliance Core owns compliance meaning and evidence requirements.
9. MaintainArr owns maintenance demand; SupplyArr owns procurement response.
10. OrdArr owns order demand; SupplyArr owns procurement response.
11. External accounting owns bills, invoices, payments, tax, general ledger, and reconciliation.
12. SupplyArr may store external accounting IDs/status snapshots only.
```

## Standard SupplyArr object envelope

```text
SupplyArrObject
- id
- tenantId
- objectNumber
- objectType
- status
- title
- description
- sourceProduct
- sourceObjectRef
- supplierRef
- requesterPersonId
- ownerPersonId
- staffarrSiteId
- staffarrLocationId
- recordRefs
- complianceRefs
- externalFinancialRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- closedAt
- auditTrail
- eventLog
```

## SupplyArr object prefixes

```text
SUP    Supplier
VEN    Vendor
DLR    Dealer
SCON   Supplier contact
SADR   Supplier address
SREQ   Supplier compliance requirement
SRC    Sourcing record
SITEM  Supplier item
SUB    Substitute item relationship
PR     Purchase request
PRL    Purchase request line
PO     Purchase order
POL    Purchase order line
QTE    Quote
APP    Procurement approval
PERF   Supplier performance record
EXT    External financial reference
```

## Standard supplier reference

```text
SupplierRef
- supplierId
- supplierNumberSnapshot
- supplierNameSnapshot
- supplierTypeSnapshot
- statusSnapshot
- complianceStatusSnapshot
- qualityStatusSnapshot
- lastResolvedAt
```

## Standard sourcing reference

```text
SourcingRef
- sourcingRecordId
- supplierId
- itemRef
- supplierItemNumberSnapshot
- manufacturerPartNumberSnapshot
- vendorPartNumberSnapshot
- preferredSnapshot
- leadTimeDaysSnapshot
- priceSnapshot
- lastResolvedAt
```


---


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
  - pending_approval
  - approved
  - restricted
  - suspended
  - blocked
  - inactive
  - archived
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

inactive
- Supplier is not normally used but remains in records.

archived
- Supplier retained for history only.
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

## Supplier status change

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
7. Approver approves, restricts, suspends, or blocks supplier.
8. Approved supplier can be selected for sourcing and purchase orders.
```

## Supplier suspension/block workflow

```text
1. Compliance, quality, procurement, or admin issue occurs.
2. Supplier status changes to restricted, suspended, or blocked.
3. Open sourcing/PR/PO usage is evaluated.
4. Products receive supplier status event.
5. New purchasing may be blocked or require approval.
6. Existing POs may remain, pause, or cancel according to policy.
```

## Events

```text
supplyarr.supplier.created
supplyarr.supplier.updated
supplyarr.supplier.onboarding_started
supplyarr.supplier.pending_approval
supplyarr.supplier.approved
supplyarr.supplier.restricted
supplyarr.supplier.suspended
supplyarr.supplier.blocked
supplyarr.supplier.inactivated
supplyarr.supplier.archived

supplyarr.supplier_contact.created
supplyarr.supplier_contact.updated
supplyarr.supplier_address.created
supplyarr.supplier_address.updated

supplyarr.supplier_onboarding.item_submitted
supplyarr.supplier_onboarding.item_accepted
supplyarr.supplier_onboarding.item_rejected
supplyarr.supplier_onboarding.completed

supplyarr.supplier.status_changed
```


---


# SupplyArr — Sourcing and Supplier Item Catalog Model

## Sourcing record

A SourcingRecord defines how a particular item, material, part, service, or supply can be obtained from a supplier. It stores procurement-side source details and snapshots.

```text
SourcingRecord
- sourcingRecordId
- tenantId
- sourcingNumber
- itemRef
- itemDescriptionSnapshot
- supplierId
- supplierSnapshot
- supplierItemNumber
- manufacturerName
- manufacturerPartNumber
- vendorPartNumber
- alternatePartNumbers
- description
- sourcingType
  - item
  - service
  - repair
  - rental
  - contract
  - freight
  - other
- status
  - draft
  - active
  - inactive
  - discontinued
  - blocked
  - archived
- preferred
- approved
- approvedSubstitute
- restricted
- restrictedReason
- minimumOrderQuantity
- packageQuantity
- unitOfMeasure
- purchaseUnitOfMeasure
- conversionFactor
- priceSnapshot
- currency
- leadTimeDays
- leadTimeConfidence
  - low
  - medium
  - high
- lastQuotedAt
- lastPurchasedAt
- contractRef
- quoteRefs
- complianceRestrictionRefs
- qualityStatusSnapshot
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
```

## Supplier item

SupplierItem is the supplier-facing representation of an item/service.

```text
SupplierItem
- supplierItemId
- tenantId
- supplierId
- supplierItemNumber
- supplierItemName
- supplierDescription
- itemRef
- status
  - active
  - inactive
  - discontinued
  - blocked
- manufacturerPartNumber
- vendorPartNumber
- barcode
- unitOfMeasure
- packageQuantity
- minimumOrderQuantity
- leadTimeDays
- currentPriceSnapshot
- currency
- sourcingRecordRefs
- recordRefs
```

## Quote

```text
Quote
- quoteId
- tenantId
- quoteNumber
- supplierId
- status
  - requested
  - received
  - accepted
  - rejected
  - expired
  - archived
- requestedByPersonId
- requestedAt
- receivedAt
- expiresAt
- lineRefs
- quoteRecordRef
- notes
```

## Quote line

```text
QuoteLine
- quoteLineId
- quoteId
- sourcingRecordRef
- itemRef
- description
- quotedQuantity
- unitOfMeasure
- unitPrice
- currency
- leadTimeDays
- minimumOrderQuantity
- packageQuantity
- notes
```

## Substitute item relationship

```text
SubstituteItemRelationship
- substituteRelationshipId
- tenantId
- primaryItemRef
- substituteItemRef
- supplierId
- relationshipType
  - exact_substitute
  - approved_alternative
  - emergency_only
  - customer_approved
  - engineering_approved
  - not_recommended
- status
  - active
  - inactive
  - blocked
- approvalRequired
- approvedByPersonId
- approvedAt
- notes
```

## Sourcing restriction

```text
SourcingRestriction
- sourcingRestrictionId
- tenantId
- sourcingRecordId
- restrictionType
  - supplier_restricted
  - quality_hold
  - compliance_block
  - customer_restricted
  - expired_document
  - price_expired
  - contract_expired
  - discontinued
  - manual
- status
  - active
  - resolved
  - expired
  - overridden
- sourceProduct
- sourceObjectRef
- reason
- effectiveAt
- expiresAt
- resolvedAt
```

## Item demand source mapping

SupplyArr may map common demand sources to sourcing records.

```text
ItemDemandSourceMapping
- mappingId
- tenantId
- sourceProduct
  - maintainarr
  - loadarr
  - ordarr
  - manual
- sourceDemandType
  - work_order_part
  - replenishment
  - customer_order
  - emergency_purchase
  - service_purchase
- itemRef
- preferredSourcingRecordRef
- fallbackSourcingRecordRefs
- status
```

## Sourcing selection result

```text
SourcingSelectionResult
- selectionResultId
- tenantId
- sourceProduct
- sourceObjectRef
- itemRef
- requestedQuantity
- selectedSourcingRecordRef
- selectedSupplierId
- selectionStatus
  - selected
  - no_source
  - restricted
  - approval_required
  - manual_review
- reason
- evaluatedAt
```

## Sourcing workflow

```text
1. Item/service need exists.
2. SupplyArr resolves known sourcing records.
3. Supplier status is checked.
4. Compliance restrictions are checked.
5. AssurArr quality status is checked.
6. Preferred supplier/source is selected if allowed.
7. If no source exists, buyer review is required.
8. Sourcing selection feeds purchase request/order.
```

## Substitute approval workflow

```text
1. Requested item is unavailable, restricted, or expensive/slow.
2. User requests substitute.
3. SupplyArr checks SubstituteItemRelationship.
4. Approval is required if substitute is conditional.
5. Approved substitute can be used in PR/PO.
6. Source product receives substitute decision if applicable.
```

## Quote workflow

```text
1. Buyer requests quote from supplier.
2. Quote document is stored in RecordArr.
3. Quote lines are entered or extracted.
4. Sourcing record price/lead time snapshot may update.
5. Buyer accepts/rejects quote.
6. Accepted quote can feed PR/PO.
```

## Events

```text
supplyarr.sourcing_record.created
supplyarr.sourcing_record.updated
supplyarr.sourcing_record.activated
supplyarr.sourcing_record.blocked
supplyarr.sourcing_record.discontinued
supplyarr.sourcing_record.archived

supplyarr.supplier_item.created
supplyarr.supplier_item.updated
supplyarr.supplier_item.discontinued

supplyarr.quote.requested
supplyarr.quote.received
supplyarr.quote.accepted
supplyarr.quote.rejected
supplyarr.quote.expired

supplyarr.substitute.created
supplyarr.substitute.approved
supplyarr.substitute.blocked

supplyarr.sourcing_selection.completed
supplyarr.sourcing_restriction.created
supplyarr.sourcing_restriction.resolved
```


---


# SupplyArr — Purchase Request and Purchase Order Model

## Purchase request

A PurchaseRequest is the request to buy something. It can come from a person, MaintainArr work-order demand, LoadArr replenishment, OrdArr order demand, AssurArr quality replacement, or manual procurement need.

```text
PurchaseRequest
- purchaseRequestId
- tenantId
- purchaseRequestNumber
- title
- description
- requestSource
  - manual
  - maintainarr_part_demand
  - loadarr_replenishment
  - ordarr_order_demand
  - assurarr_quality_replacement
  - routarr_transport_need
  - staffarr_admin_need
  - compliance_requirement
- sourceProduct
- sourceObjectRef
- status
  - draft
  - submitted
  - review
  - sourcing
  - pending_approval
  - approved
  - rejected
  - converted_to_po
  - partially_converted
  - canceled
  - closed
- priority
  - low
  - normal
  - high
  - urgent
  - emergency
- requestedByPersonId
- ownerBuyerPersonId
- requestedAt
- neededBy
- staffarrSiteId
- shipToStaffarrLocationId
- departmentOrgUnitId
- costCenterRef
- lineRefs
- approvalRefs
- blockerRefs
- justification
- rejectionReason
- recordRefs
- createdAt
- updatedAt
```

## Purchase request status definitions

```text
draft
- Request is being prepared.

submitted
- Request has been submitted.

review
- Buyer/supervisor is reviewing request.

sourcing
- Supplier/source selection is in progress.

pending_approval
- Approval is required.

approved
- Request is approved for PO creation.

rejected
- Request was denied.

converted_to_po
- Entire request was converted to purchase order.

partially_converted
- Some lines were converted to purchase order.

canceled
- Request was canceled.

closed
- Request is complete and no further action remains.
```

## Purchase request line

```text
PurchaseRequestLine
- purchaseRequestLineId
- tenantId
- purchaseRequestId
- lineNumber
- lineType
  - item
  - service
  - repair
  - rental
  - freight
  - other
- itemRef
- itemDescription
- requestedQuantity
- unitOfMeasure
- neededBy
- preferredSupplierId
- preferredSourcingRecordRef
- selectedSourcingRecordRef
- acceptableSubstituteRefs
- estimatedUnitCost
- estimatedExtendedCost
- currency
- sourceDemandRef
- complianceRequirementRefs
- status
  - open
  - sourcing
  - approved
  - rejected
  - converted_to_po
  - canceled
- notes
```

## Procurement approval

```text
ProcurementApproval
- approvalId
- tenantId
- sourceObjectType
  - purchase_request
  - purchase_request_line
  - purchase_order
  - purchase_order_line
  - emergency_purchase
  - supplier_approval
  - substitute_approval
- sourceObjectRef
- approvalType
  - supervisor
  - buyer
  - cost
  - compliance
  - quality
  - emergency
  - supplier
  - substitute
- status
  - pending
  - approved
  - rejected
  - canceled
  - expired
- requestedByPersonId
- requestedAt
- approverPersonId
- decisionAt
- decisionReason
- approvalLimitSnapshot
- evidenceRecordRefs
```

## Purchase order

A PurchaseOrder is SupplyArr’s procurement commitment/ordering document. It may be exported to an external financial/accounting system, but SupplyArr does not own bills/payments/GL.

```text
PurchaseOrder
- purchaseOrderId
- tenantId
- purchaseOrderNumber
- supplierId
- supplierSnapshot
- status
  - draft
  - pending_approval
  - approved
  - sent
  - acknowledged
  - partially_received
  - received
  - closed
  - canceled
  - rejected
- orderDate
- requestedByPersonId
- buyerPersonId
- approvedByPersonId
- approvedAt
- sentAt
- acknowledgedAt
- expectedReceiptDate
- shipToStaffarrSiteId
- shipToStaffarrLocationId
- billToRef
- paymentTermsSnapshot
- shippingTermsSnapshot
- freightTermsSnapshot
- lineRefs
- approvalRefs
- documentRefs
- quoteRefs
- loadarrExpectedReceiptRef
- routarrInboundTripRef
- externalAccountingRef
- notes
- createdAt
- updatedAt
- closedAt
- canceledAt
- cancelReason
```

## Purchase order status definitions

```text
draft
- PO is being prepared.

pending_approval
- PO requires approval.

approved
- PO is approved but not sent.

sent
- PO was sent to supplier.

acknowledged
- Supplier acknowledged PO.

partially_received
- Some lines have been received by LoadArr.

received
- All expected lines received or closed.

closed
- PO is administratively closed.

canceled
- PO was canceled.

rejected
- PO approval or supplier acknowledgement was rejected.
```

## Purchase order line

```text
PurchaseOrderLine
- purchaseOrderLineId
- tenantId
- purchaseOrderId
- lineNumber
- lineType
  - item
  - service
  - repair
  - rental
  - freight
  - other
- itemRef
- sourcingRecordRef
- supplierItemNumber
- manufacturerPartNumber
- vendorPartNumber
- description
- orderedQuantity
- receivedQuantitySnapshot
- canceledQuantity
- remainingQuantity
- unitOfMeasure
- unitCost
- extendedCost
- currency
- expectedAt
- status
  - open
  - partially_received
  - received
  - canceled
  - backordered
  - closed
- sourcePurchaseRequestLineRef
- sourceDemandRef
- complianceRequirementRefs
- notes
```

## Purchase order change

```text
PurchaseOrderChange
- poChangeId
- tenantId
- purchaseOrderId
- changeType
  - quantity
  - price
  - supplier
  - expected_date
  - cancellation
  - line_add
  - line_remove
  - terms
  - ship_to
- status
  - draft
  - pending_approval
  - approved
  - rejected
  - applied
  - canceled
- requestedByPersonId
- requestedAt
- approvedByPersonId
- approvedAt
- beforeSnapshot
- afterSnapshot
- reason
```

## External financial reference

SupplyArr stores references to external financial systems but does not own accounting execution.

```text
ExternalFinancialReference
- externalFinancialReferenceId
- tenantId
- sourceObjectType
  - supplier
  - purchase_order
  - purchase_order_line
  - purchase_request
- sourceObjectRef
- externalSystem
  - quickbooks
  - netsuite
  - dynamics
  - sap
  - other
- externalObjectType
  - vendor
  - purchase_order
  - bill
  - invoice
  - payment
  - account
  - class
  - cost_center
- externalObjectId
- externalObjectNumber
- statusSnapshot
- lastSyncedAt
```

## Purchase request workflow

```text
1. Demand is created manually or by another product.
2. SupplyArr creates PurchaseRequest.
3. Buyer reviews request.
4. Sourcing selection occurs.
5. Approval workflow runs if required.
6. Request is approved, rejected, canceled, or converted to PO.
7. Source product receives status update.
```

## Purchase order workflow

```text
1. Buyer converts approved PR lines or creates PO manually.
2. Supplier, ship-to location, lines, costs, and terms are selected.
3. Approval workflow runs if required.
4. PO is approved.
5. PO is sent to supplier.
6. Supplier acknowledges or rejects/changes.
7. SupplyArr sends ExpectedReceipt to LoadArr.
8. LoadArr receives goods/services evidence as applicable.
9. SupplyArr updates PO line received snapshots.
10. PO closes when complete.
```

## PO receiving update workflow

```text
1. LoadArr receives against ExpectedReceipt.
2. LoadArr sends receipt status update to SupplyArr.
3. SupplyArr updates PO lines with received quantity snapshot.
4. Discrepancies may create AssurArr supplier quality issue.
5. Supplier performance updates.
6. PO becomes partially_received, received, or discrepancy/exception state.
```

## Emergency purchase workflow

```text
1. User creates urgent/emergency PurchaseRequest.
2. Justification is required.
3. Emergency approval route is used.
4. Buyer may select restricted but allowed emergency source if policy permits.
5. PO is created/sent quickly.
6. Post-purchase review is required.
7. Audit trail is retained.
```

## Events

```text
supplyarr.purchase_request.created
supplyarr.purchase_request.submitted
supplyarr.purchase_request.review_started
supplyarr.purchase_request.sourcing_started
supplyarr.purchase_request.approval_requested
supplyarr.purchase_request.approved
supplyarr.purchase_request.rejected
supplyarr.purchase_request.converted_to_po
supplyarr.purchase_request.canceled
supplyarr.purchase_request.closed

supplyarr.procurement_approval.requested
supplyarr.procurement_approval.approved
supplyarr.procurement_approval.rejected
supplyarr.procurement_approval.expired

supplyarr.purchase_order.created
supplyarr.purchase_order.approval_requested
supplyarr.purchase_order.approved
supplyarr.purchase_order.sent
supplyarr.purchase_order.acknowledged
supplyarr.purchase_order.partially_received
supplyarr.purchase_order.received
supplyarr.purchase_order.closed
supplyarr.purchase_order.canceled
supplyarr.purchase_order.change_requested
supplyarr.purchase_order.change_applied

supplyarr.external_financial_ref.created
supplyarr.external_financial_ref.synced
```


---


# SupplyArr — Supplier Compliance, Quality, and Performance Model

## Supplier compliance requirement

A SupplierComplianceRequirement tracks a required supplier document, certification, insurance, license, contract, tax document, safety document, or quality document.

Compliance Core owns the requirement meaning. RecordArr owns the document file. SupplyArr owns the supplier-side requirement tracking status.

```text
SupplierComplianceRequirement
- supplierComplianceRequirementId
- tenantId
- supplierId
- requirementNumber
- requirementType
  - insurance
  - certification
  - contract
  - license
  - tax_document
  - safety_document
  - quality_document
  - compliance_document
  - banking_reference
  - other
- title
- description
- complianceCoreRequirementRef
- evidenceTypeRef
- required
- status
  - missing
  - requested
  - submitted
  - under_review
  - accepted
  - rejected
  - expired
  - waived
  - not_applicable
- recordRefs
- requestedAt
- submittedAt
- reviewedByPersonId
- reviewedAt
- expiresAt
- waiverReason
- rejectionReason
- notes
```

## Supplier compliance status snapshot

```text
SupplierComplianceStatusSnapshot
- complianceStatusSnapshotId
- tenantId
- supplierId
- overallStatus
  - compliant
  - warning
  - noncompliant
  - missing_documents
  - expired_documents
  - unknown
- missingRequirementCount
- expiredRequirementCount
- rejectedRequirementCount
- warningCount
- lastEvaluatedAt
- complianceCoreEvaluationRef
```

## Supplier quality status snapshot

AssurArr owns quality hold/nonconformance decisions. SupplyArr stores/uses a snapshot.

```text
SupplierQualityStatusSnapshot
- qualityStatusSnapshotId
- tenantId
- supplierId
- overallStatus
  - acceptable
  - warning
  - on_hold
  - blocked
  - unknown
- activeHoldRefs
- openNonconformanceRefs
- openScarRefs
- repeatIssueCount
- lastQualityIssueAt
- lastResolvedAt
- sourceProduct: assurarr
```

## Supplier performance record

```text
SupplierPerformanceRecord
- performanceRecordId
- tenantId
- supplierId
- periodStart
- periodEnd
- status
  - draft
  - calculated
  - reviewed
  - archived
- onTimeDeliveryRate
- averageLeadTimeDays
- leadTimeVarianceDays
- receiptDiscrepancyCount
- qualityIssueCount
- nonconformanceRefs
- scarRefs
- lateDeliveryCount
- emergencyPurchaseCount
- priceVariancePercent
- complianceIssueCount
- responseTimeHours
- overallScore
- performanceStatus
  - excellent
  - acceptable
  - warning
  - poor
  - blocked
  - unknown
- generatedAt
- reviewedByPersonId
- reviewedAt
```

## Supplier issue

SupplyArr may track procurement-facing supplier issues. Quality issues should route to AssurArr.

```text
SupplierIssue
- supplierIssueId
- tenantId
- issueNumber
- supplierId
- issueType
  - late_delivery
  - no_response
  - pricing_dispute
  - missing_document
  - compliance_issue
  - quality_issue
  - wrong_item
  - damaged_goods
  - service_issue
  - other
- severity
  - low
  - moderate
  - high
  - critical
- status
  - open
  - investigating
  - waiting_supplier
  - escalated_to_assurarr
  - resolved
  - closed
  - canceled
- sourceProduct
- sourceObjectRef
- purchaseOrderRef
- receiptRef
- ownerPersonId
- recordRefs
- assurarrNonconformanceRef
- resolutionSummary
- openedAt
- closedAt
```

## Supplier communication

```text
SupplierCommunication
- communicationId
- tenantId
- supplierId
- communicationType
  - email
  - phone
  - meeting
  - portal_message
  - document_request
  - quote_request
  - po_sent
  - complaint
  - corrective_action
- direction
  - inbound
  - outbound
  - internal_note
- subject
- summary
- contactRef
- personId
- sourceProduct
- sourceObjectRef
- recordRefs
- occurredAt
```

## Supplier document request

```text
SupplierDocumentRequest
- documentRequestId
- tenantId
- supplierId
- requirementRef
- requestedDocumentType
- status
  - draft
  - sent
  - viewed
  - submitted
  - accepted
  - rejected
  - expired
  - canceled
- requestedByPersonId
- requestedAt
- dueAt
- secureUploadSessionRef
- submittedRecordRefs
- reviewedByPersonId
- reviewedAt
```

## Compliance workflow

```text
1. Supplier is created or updated.
2. SupplyArr determines required documents based on supplier type, services, items, and Compliance Core requirements.
3. SupplierComplianceRequirements are created.
4. Supplier documents are requested.
5. RecordArr stores submitted documents.
6. Compliance Core evaluates evidence where applicable.
7. Reviewer accepts/rejects/waives requirements.
8. Supplier compliance status snapshot updates.
9. Supplier approval/restriction/block status may change.
```

## Supplier quality workflow

```text
1. LoadArr or AssurArr reports quality issue.
2. SupplyArr records supplier issue/performance impact.
3. AssurArr owns nonconformance, quality hold, and SCAR.
4. SupplyArr updates supplier quality status snapshot.
5. Supplier may become restricted/suspended/blocked based on policy.
```

## Supplier performance workflow

```text
1. LoadArr sends receipt/discrepancy facts.
2. SupplyArr sends PO/lead time facts.
3. AssurArr sends quality issue/SCAR facts.
4. SupplyArr calculates SupplierPerformanceRecord.
5. Buyer reviews scorecard.
6. Supplier status/preferred flag/restrictions may be updated.
7. ReportArr consumes supplier performance metrics.
```

## Supplier document request workflow

```text
1. Buyer/compliance user requests supplier document.
2. SupplyArr creates SupplierDocumentRequest.
3. RecordArr/Field Companion secure upload session may be created.
4. Supplier submits document.
5. RecordArr stores document.
6. SupplyArr routes review.
7. Requirement is accepted/rejected/expired.
```

## Events

```text
supplyarr.supplier_compliance_requirement.created
supplyarr.supplier_compliance_requirement.requested
supplyarr.supplier_compliance_requirement.submitted
supplyarr.supplier_compliance_requirement.accepted
supplyarr.supplier_compliance_requirement.rejected
supplyarr.supplier_compliance_requirement.expired
supplyarr.supplier_compliance_requirement.waived

supplyarr.supplier_compliance_status.changed
supplyarr.supplier_quality_status.changed

supplyarr.supplier_performance.calculated
supplyarr.supplier_performance.reviewed

supplyarr.supplier_issue.created
supplyarr.supplier_issue.escalated_to_assurarr
supplyarr.supplier_issue.resolved
supplyarr.supplier_issue.closed

supplyarr.supplier_document_request.created
supplyarr.supplier_document_request.sent
supplyarr.supplier_document_request.submitted
supplyarr.supplier_document_request.accepted
supplyarr.supplier_document_request.rejected

supplyarr.supplier_communication.created
```


---


# SupplyArr — Workflows, Status Logic, Events, and APIs

## Major workflow: supplier onboarding

```text
1. User creates Supplier.
2. Supplier starts as prospect or onboarding.
3. User enters contacts, addresses, supplier type, terms snapshots, and notes.
4. SupplyArr creates onboarding checklist.
5. Compliance requirements are generated from supplier type and Compliance Core.
6. Supplier documents are requested and stored in RecordArr.
7. Compliance Core evaluates evidence where applicable.
8. AssurArr quality status is checked if needed.
9. Approver approves, restricts, suspends, or blocks supplier.
10. Supplier becomes available for sourcing and purchase orders if approved.
```

## Major workflow: maintenance part demand to purchase order

```text
1. MaintainArr creates PartDemand.
2. LoadArr checks inventory and cannot fulfill.
3. LoadArr creates ReplenishmentSignal.
4. SupplyArr creates PurchaseRequest.
5. Buyer reviews demand and needed-by date.
6. SupplyArr selects sourcing record/supplier.
7. Approval workflow runs.
8. PurchaseOrder is created.
9. PO is sent to supplier.
10. SupplyArr publishes ExpectedReceipt to LoadArr.
11. LoadArr receives goods.
12. MaintainArr receives availability/issue status through LoadArr.
```

## Major workflow: order-driven procurement

```text
1. OrdArr creates order demand.
2. LoadArr cannot fulfill from stock or requires direct purchase.
3. SupplyArr creates PurchaseRequest.
4. Buyer selects supplier/source.
5. PurchaseOrder is created.
6. ExpectedReceipt is sent to LoadArr.
7. Receipt and fulfillment status update OrdArr through LoadArr/SupplyArr events.
```

## Major workflow: purchase request approval

```text
1. PurchaseRequest is submitted.
2. SupplyArr evaluates sourcing, supplier status, estimated cost, urgency, and compliance.
3. Approval route is selected.
4. Approver approves or rejects.
5. If approved, PR can convert to PO.
6. If rejected, source product receives rejection/blocker status.
```

## Major workflow: purchase order receiving update

```text
1. SupplyArr sends ExpectedReceipt to LoadArr.
2. LoadArr receives against expected lines.
3. LoadArr sends receipt status updates.
4. SupplyArr updates PO line received quantity snapshots.
5. Discrepancies create supplier issue or AssurArr nonconformance.
6. PO moves to partially_received, received, closed, or exception handling.
```

## Major workflow: supplier document request

```text
1. Compliance requirement is missing/expired/rejected.
2. SupplyArr creates SupplierDocumentRequest.
3. Secure upload session is created through RecordArr/Field Companion if needed.
4. Supplier submits document.
5. RecordArr stores document.
6. Compliance Core evaluates evidence.
7. Reviewer accepts or rejects.
8. Supplier compliance status updates.
```

## Major workflow: supplier quality restriction

```text
1. AssurArr creates supplier quality issue, hold, or SCAR.
2. SupplyArr receives quality status event.
3. SupplierQualityStatusSnapshot updates.
4. Supplier may become restricted, suspended, or blocked.
5. Open PRs/POs/sourcing records are evaluated.
6. Buyers are warned or blocked when selecting supplier.
```

## SupplyArr emitted events

```text
supplyarr.supplier.created
supplyarr.supplier.updated
supplyarr.supplier.onboarding_started
supplyarr.supplier.pending_approval
supplyarr.supplier.approved
supplyarr.supplier.restricted
supplyarr.supplier.suspended
supplyarr.supplier.blocked
supplyarr.supplier.inactivated
supplyarr.supplier.archived

supplyarr.supplier_contact.created
supplyarr.supplier_contact.updated
supplyarr.supplier_address.created
supplyarr.supplier_address.updated

supplyarr.sourcing_record.created
supplyarr.sourcing_record.updated
supplyarr.sourcing_record.activated
supplyarr.sourcing_record.blocked
supplyarr.sourcing_record.discontinued
supplyarr.sourcing_selection.completed

supplyarr.quote.requested
supplyarr.quote.received
supplyarr.quote.accepted
supplyarr.quote.rejected
supplyarr.quote.expired

supplyarr.purchase_request.created
supplyarr.purchase_request.submitted
supplyarr.purchase_request.review_started
supplyarr.purchase_request.sourcing_started
supplyarr.purchase_request.approval_requested
supplyarr.purchase_request.approved
supplyarr.purchase_request.rejected
supplyarr.purchase_request.converted_to_po
supplyarr.purchase_request.canceled
supplyarr.purchase_request.closed

supplyarr.purchase_order.created
supplyarr.purchase_order.approval_requested
supplyarr.purchase_order.approved
supplyarr.purchase_order.sent
supplyarr.purchase_order.acknowledged
supplyarr.purchase_order.partially_received
supplyarr.purchase_order.received
supplyarr.purchase_order.closed
supplyarr.purchase_order.canceled

supplyarr.supplier_compliance_status.changed
supplyarr.supplier_quality_status.changed
supplyarr.supplier_performance.calculated
supplyarr.supplier_issue.created
supplyarr.supplier_issue.escalated_to_assurarr
supplyarr.supplier_issue.closed
```

## Integration APIs SupplyArr should expose

```text
GET /api/v1/integrations/suppliers
GET /api/v1/integrations/suppliers/{supplierId}
POST /api/v1/integrations/suppliers
PATCH /api/v1/integrations/suppliers/{supplierId}
POST /api/v1/integrations/suppliers/{supplierId}/status-changes

GET /api/v1/integrations/suppliers/{supplierId}/contacts
POST /api/v1/integrations/suppliers/{supplierId}/contacts
GET /api/v1/integrations/suppliers/{supplierId}/addresses
POST /api/v1/integrations/suppliers/{supplierId}/addresses

GET /api/v1/integrations/sourcing-records
GET /api/v1/integrations/sourcing-records/{sourcingRecordId}
POST /api/v1/integrations/sourcing-records
POST /api/v1/integrations/sourcing-selection

GET /api/v1/integrations/supplier-items
POST /api/v1/integrations/supplier-items
GET /api/v1/integrations/substitutes
POST /api/v1/integrations/substitutes

POST /api/v1/integrations/quotes
GET /api/v1/integrations/quotes/{quoteId}

GET /api/v1/integrations/purchase-requests
GET /api/v1/integrations/purchase-requests/{purchaseRequestId}
POST /api/v1/integrations/purchase-requests
POST /api/v1/integrations/purchase-requests/{purchaseRequestId}/submit
POST /api/v1/integrations/purchase-requests/{purchaseRequestId}/approve
POST /api/v1/integrations/purchase-requests/{purchaseRequestId}/reject
POST /api/v1/integrations/purchase-requests/{purchaseRequestId}/convert-to-po

GET /api/v1/integrations/purchase-orders
GET /api/v1/integrations/purchase-orders/{purchaseOrderId}
POST /api/v1/integrations/purchase-orders
POST /api/v1/integrations/purchase-orders/{purchaseOrderId}/approve
POST /api/v1/integrations/purchase-orders/{purchaseOrderId}/send
POST /api/v1/integrations/purchase-orders/{purchaseOrderId}/acknowledge
POST /api/v1/integrations/purchase-orders/{purchaseOrderId}/cancel
POST /api/v1/integrations/purchase-orders/{purchaseOrderId}/close

POST /api/v1/integrations/expected-receipts/publish
POST /api/v1/integrations/receipt-status-updates

GET /api/v1/integrations/supplier-compliance-requirements
POST /api/v1/integrations/supplier-compliance-requirements
POST /api/v1/integrations/supplier-document-requests
POST /api/v1/integrations/supplier-document-requests/{documentRequestId}/review

POST /api/v1/integrations/supplier-quality-events
GET /api/v1/integrations/supplier-performance/{supplierId}
POST /api/v1/integrations/supplier-performance/calculate
```

## APIs SupplyArr should consume

```text
NexArr
- POST /handoff/redeem
- POST /service-tokens/introspect
- GET /entitlements/{productKey}

StaffArr
- GET /persons/{personId}
- GET /persons/{personId}/permissions
- GET /locations/{locationId}
- GET /sites
- POST /permission-checks
- POST /incidents

TrainArr
- POST /qualification-checks

Compliance Core
- GET /catalogs/governing-bodies
- GET /evidence-types
- GET /rulepacks
- POST /evaluations

RecordArr
- POST /records
- GET /records/{recordId}
- POST /upload-sessions
- POST /record-packages

LoadArr
- POST /expected-receipts
- GET /receipts/{receiptId}
- GET /discrepancies
- GET /replenishment-signals

MaintainArr
- GET /work-orders/{workOrderId}
- POST /part-demand-status-updates

OrdArr
- GET /orders/{orderId}
- POST /orders/{orderId}/blockers
- POST /orders/{orderId}/status-updates

RoutArr
- POST /trips
- GET /trips/{tripId}
- POST /dock-appointments

AssurArr
- GET /holds
- GET /supplier-quality-issues
- GET /nonconformances
- GET /scar/{scarId}
- POST /quality-events

ReportArr
- POST /events
```

## Permission examples

```text
supplyarr.suppliers.read
supplyarr.suppliers.create
supplyarr.suppliers.update
supplyarr.suppliers.approve
supplyarr.suppliers.restrict
supplyarr.suppliers.suspend
supplyarr.suppliers.block
supplyarr.suppliers.archive

supplyarr.sourcing.read
supplyarr.sourcing.create
supplyarr.sourcing.update
supplyarr.sourcing.approve
supplyarr.sourcing.block

supplyarr.quotes.read
supplyarr.quotes.request
supplyarr.quotes.review

supplyarr.purchase_requests.read
supplyarr.purchase_requests.create
supplyarr.purchase_requests.review
supplyarr.purchase_requests.approve
supplyarr.purchase_requests.reject
supplyarr.purchase_requests.convert_to_po

supplyarr.purchase_orders.read
supplyarr.purchase_orders.create
supplyarr.purchase_orders.approve
supplyarr.purchase_orders.send
supplyarr.purchase_orders.cancel
supplyarr.purchase_orders.close

supplyarr.compliance.read
supplyarr.compliance.review
supplyarr.supplier_documents.request
supplyarr.supplier_documents.review

supplyarr.performance.read
supplyarr.performance.review
supplyarr.admin
```

## Default role examples

```text
SupplyArr Viewer
- Read suppliers, sourcing records, PRs, POs, and supplier status.

Requester
- Create purchase requests.
- View own request status.

Buyer
- Review PRs.
- Select suppliers/sourcing records.
- Create POs.
- Send POs where allowed.

Procurement Approver
- Approve/reject purchase requests and purchase orders within assigned limits.

Supplier Manager
- Create/update suppliers.
- Manage contacts, addresses, onboarding, restrictions.

Supplier Compliance Reviewer
- Review supplier compliance requirements and documents.

Supplier Quality Coordinator
- View supplier quality status.
- Coordinate with AssurArr on supplier issues/SCARs.

SupplyArr Admin
- Manage procurement settings, sourcing rules, approval routes, and product configuration.
```

## SupplyArr UI surfaces

```text
/app/supplyarr
- dashboard
- suppliers
- supplier detail
- supplier onboarding
- supplier compliance
- supplier performance
- sourcing records
- supplier item catalog
- substitutes
- quotes
- purchase requests
- purchase request detail
- purchase orders
- purchase order detail
- approvals
- supplier issues
- document requests
- settings
```

## Supplier detail UI

```text
SupplierDetailPage
- Header
  - supplierNumber
  - displayName
  - status
  - complianceStatus
  - qualityStatus
  - riskStatus
- Profile
- Contacts
- Addresses
- Compliance requirements
- Documents
- Sourcing records
- Purchase history
- Open PRs/POs
- Quality issues
- Performance
- Notes/communications
- Timeline
```

## Purchase request detail UI

```text
PurchaseRequestDetailPage
- Header
- Source demand
- Lines
- Sourcing selection
- Estimated cost
- Justification
- Approvals
- Blockers
- Conversion to PO
- Timeline
```

## Purchase order detail UI

```text
PurchaseOrderDetailPage
- Header
- Supplier
- Ship-to
- Lines
- Terms snapshots
- Documents/quotes
- Approvals
- Sent/acknowledged state
- Expected receipt
- Receipt status
- Discrepancies
- External financial refs
- Timeline
```
