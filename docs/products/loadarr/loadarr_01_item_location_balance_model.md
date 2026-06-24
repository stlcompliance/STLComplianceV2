# LoadArr — Item, Location Behavior, and Balance Model

## Inventory execution item profile

LoadArr may maintain an inventory execution item profile. This is not the same as SupplyArr tenant commercial item/part/material/SKU truth or Platform Reference Data service shared public identity. LoadArr needs enough item metadata to receive, store, reserve, pick, count, and move inventory safely.

SupplyArr owns the commercial and supplier context. Platform Reference Data service owns shared identifiers, taxonomies, UOM normalization, manufacturer identity, and crosswalks. LoadArr owns inventory execution behavior and balances for the item profile.

```text
InventoryItem
- itemId
- tenantId
- itemNumber
- name
- description
- itemType
  - part
  - raw_material
  - finished_good
  - consumable
  - tool
  - safety_supply
  - packaging
  - hazmat
  - serialized_asset_candidate
  - maintenance_supply
  - other
- status
  - draft
  - active
  - inactive
  - discontinued
  - blocked
  - archived
- unitOfMeasure
- alternateUnits
- baseUnitOfMeasure
- lotTracked
- serialTracked
- expirationTracked
- conditionTracked
- hazardousFlag
- controlledItemFlag
- temperatureControlled
- storageRequirementRefs
- complianceRefs
- platformReferenceDataRefs
- supplyarrSourcingRefs
- recordRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
```

## Item unit conversion

```text
ItemUnitConversion
- conversionId
- itemId
- fromUnit
- toUnit
- factor
- status
  - active
  - inactive
- notes
```

## Item handling rule

```text
ItemHandlingRule
- handlingRuleId
- itemId
- ruleType
  - storage
  - hazmat
  - temperature
  - expiration
  - lot_control
  - serial_control
  - quarantine
  - inspection_required
  - restricted_issue
- complianceRef
- ruleText
- status
  - active
  - inactive
```

## WMS location profile

StaffArr owns the location identity. LoadArr owns whether the location can receive, pick, count, quarantine, stage, store, or move inventory.

```text
WmsLocationProfile
- wmsLocationProfileId
- tenantId
- staffarrLocationId
- locationNumberSnapshot
- locationNameSnapshot
- locationTypeSnapshot
- siteOrgUnitIdSnapshot
- siteNameSnapshot
- pathSnapshot
- status
  - draft
  - active
  - inactive
  - blocked
  - archived
- receivable
- pickable
- countable
- replenishable
- quarantine
- inspectionHold
- staging
- shippingStaging
- receivingStaging
- putawayQueue
- maintenanceHandoff
- technicianPickup
- serviceCounter
- allowsNegativeInventory
- requiresScan
- requiresLot
- requiresSerial
- hazmatAllowed
- temperatureControlled
- capacityRules
- storageRules
- allowedItemTypes
- blockedItemTypes
- allowedMovementTypes
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
```

## WMS location status definitions

```text
draft
- Location profile is being configured.

active
- Location can be used according to behavior flags.

inactive
- Location profile exists but should not be used for new movement.

blocked
- Location cannot be used because of safety, quality, system, or operational block.

archived
- Location profile is retained for history.
```

## Location capacity rule

```text
LocationCapacityRule
- capacityRuleId
- wmsLocationProfileId
- capacityType
  - quantity
  - volume
  - weight
  - pallet_positions
  - item_count
  - custom
- maxValue
- unitOfMeasure
- warningThreshold
- status
```

## Location storage rule

```text
LocationStorageRule
- storageRuleId
- wmsLocationProfileId
- ruleType
  - allowed_item_type
  - blocked_item_type
  - hazmat_class
  - temperature_range
  - expiration_min_days
  - lot_segregation
  - serial_required
  - quality_status
  - customer_owned
  - supplier_owned
- value
- complianceRef
- status
```

## Inventory balance

InventoryBalance represents the current quantity state for an item at a StaffArr location, with optional lot/serial/expiration/condition dimensions.

```text
InventoryBalance
- balanceId
- tenantId
- itemId
- staffarrLocationId
- wmsLocationProfileId
- lotNumber
- serialNumber
- expirationDate
- condition
  - good
  - damaged
  - expired
  - suspect
  - refurbished
  - used
  - unknown
- ownershipType
  - company_owned
  - customer_owned
  - supplier_owned
  - consigned
  - unknown
- quantityOnHand
- quantityAvailable
- quantityReserved
- quantityAllocated
- quantityPicked
- quantityStaged
- quantityOnHold
- quantityDamaged
- quantityExpired
- quantityInInspection
- quantityInTransit
- quantityQuarantined
- unitOfMeasure
- status
  - available
  - reserved
  - hold
  - damaged
  - expired
  - quarantine
  - inspection
  - blocked
  - zero
- activeHoldRefs
- lastMovementAt
- lastCountAt
- createdAt
- updatedAt
```

## Balance quantity definitions

```text
quantityOnHand
- Physical/system quantity at location.

quantityAvailable
- Quantity available to reserve or use.

quantityReserved
- Quantity reserved for known demand.

quantityAllocated
- Quantity assigned to a demand but not picked/issued.

quantityPicked
- Quantity picked but not issued/shipped/consumed.

quantityStaged
- Quantity staged for shipment, issue, or handoff.

quantityOnHold
- Quantity blocked by hold.

quantityDamaged
- Quantity marked damaged.

quantityExpired
- Quantity expired.

quantityInInspection
- Quantity awaiting inspection.

quantityInTransit
- Quantity moving between locations/sites.

quantityQuarantined
- Quantity isolated pending quality decision.
```

## Inventory status snapshot

```text
InventoryStatusSnapshot
- snapshotId
- tenantId
- itemId
- staffarrLocationId
- status
  - healthy
  - low_stock
  - out_of_stock
  - overstock
  - on_hold
  - blocked
  - unknown
- quantityOnHand
- quantityAvailable
- reorderPoint
- preferredStockLevel
- openDemandQuantity
- openReplenishmentQuantity
- generatedAt
```

## Availability check

Other products ask LoadArr whether inventory is available.

```text
AvailabilityCheck
- availabilityCheckId
- tenantId
- sourceProduct
  - maintainarr
  - ordarr
  - routarr
  - assurarr
  - manual
- sourceObjectRef
- itemId
- requestedQuantity
- unitOfMeasure
- neededBy
- staffarrSiteId
- preferredLocationId
- allowSubstitute
- status
  - available
  - partially_available
  - unavailable
  - blocked
  - unknown
- availableQuantity
- reservedQuantity
- suggestedLocationRefs
- substituteSuggestions
- blockerRefs
- checkedAt
```

## Inventory item lifecycle

```text
1. Inventory execution profile is created manually, imported, linked from SupplyArr, or enriched from Platform Reference Data service.
2. Tracking rules are defined.
3. WMS storage/handling rules are attached.
4. Item becomes active.
5. Profile can be received, stored, reserved, picked, issued, counted, and transferred.
6. Profile may become blocked/discontinued/inactive for inventory execution.
7. Historical movement remains available after archive.
```

## Location profile workflow

```text
1. StaffArr creates internal location.
2. LoadArr imports/resolves StaffArr location.
3. LoadArr creates WmsLocationProfile.
4. User sets receivable/pickable/countable/hold/staging behavior.
5. LoadArr validates storage and capacity rules.
6. Location becomes active for WMS use.
7. Inventory can move through the location according to behavior flags.
```

## Balance recalculation workflow

```text
1. StockMovement is posted.
2. LoadArr recalculates affected balances.
3. Holds/reservations/allocations are applied.
4. Availability is recalculated.
5. Balance changed event is emitted.
```

## Events

```text
loadarr.item.created
loadarr.item.updated
loadarr.item.status_changed
loadarr.location_profile.created
loadarr.location_profile.updated
loadarr.location_profile.status_changed
loadarr.balance.created
loadarr.balance.changed
loadarr.balance.zeroed
loadarr.availability_check.completed
loadarr.inventory_status.changed
```
