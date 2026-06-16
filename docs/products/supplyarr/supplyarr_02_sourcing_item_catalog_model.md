# SupplyArr — Sourcing and Supplier Item Catalog Model

## Item ownership rule

SupplyArr owns tenant commercial item, part, material, and SKU context used for sourcing and purchasing. ReferenceDataCore owns shared public identifiers, public taxonomies, UOM normalization, UPC/GTIN normalization, manufacturer identity, and crosswalks. LoadArr owns inventory execution profiles, balances, and stock movement for physical stock.

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
3. Supplier eligibility is checked.
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
