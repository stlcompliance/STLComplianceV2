# Platform Reference Data service — Unit of Measure and Package Normalization Model

## Purpose

Platform Reference Data service owns shared unit-of-measure and package normalization so products do not create conflicting conversion logic.

This model supports:

- receiving
- putaway
- picking
- issuing
- purchase requests
- purchase orders
- parts usage
- order fulfillment
- report normalization
- import mapping
- scanned product lookup
- vendor catalog mapping

Operational quantities remain with the owning products.

Examples:

- LoadArr owns an inventory balance.
- SupplyArr owns a purchase order line.
- MaintainArr owns a work order parts usage line.
- OrdArr owns an order line request.
- Platform Reference Data service owns shared unit/package normalization that helps those products interpret quantities consistently.

## Unit of measure

```text
UnitOfMeasure
- uomId
- uomKey
- displayName
- pluralDisplayName
- abbreviation
- measurementFamily
  - count
  - length
  - area
  - volume
  - mass
  - time
  - temperature
  - pressure
  - electrical
  - speed
  - custom
- baseUnitRef
- conversionToBase
- precision
- roundingRule
  - none
  - round_half_up
  - round_half_even
  - floor
  - ceiling
  - product_specific
- status
  - active
  - deprecated
  - archived
- sourceRefs
```

Recommended initial UOM examples:

```text
- each
- case
- box
- pallet
- pack
- roll
- gallon
- quart
- liter
- milliliter
- ounce
- pound
- kilogram
- foot
- inch
- hour
```

## Measurement family rule

A conversion is safe only inside a compatible measurement family unless a package rule explicitly defines the conversion.

Examples:

```text
Safe direct conversion:
- pound to kilogram
- gallon to liter
- inch to foot

Requires package context:
- case to each
- pallet to case
- box to each
- roll to linear feet
```

## Package definition

```text
PackageDefinition
- packageId
- packageKey
- displayName
- packageType
  - each
  - inner_pack
  - box
  - case
  - pallet
  - roll
  - drum
  - tote
  - kit
  - bundle
  - custom
- defaultQuantity
- defaultUomRef
- dimensionSummary
- weightSummary
- status
- sourceRefs
```

## Package conversion rule

PackageConversionRule defines how one package or UOM converts to another for a reference entity, product family, or tenant overlay.

```text
PackageConversionRule
- conversionRuleId
- referenceEntityId
- datasetKey
- fromPackageRef
- fromUomRef
- toPackageRef
- toUomRef
- conversionFactor
- conversionBasis
  - manufacturer_spec
  - vendor_catalog
  - product_import
  - tenant_overlay
  - manual_review
  - inferred_low_confidence
- confidenceScore
- validFrom
- validTo
- status
  - active
  - review_required
  - deprecated
  - invalid
- sourceRefs
- notes
```

## Quantity normalization

A normalized quantity captures both original and normalized forms.

```text
NormalizedQuantity
- originalQuantity
- originalUom
- originalPackage
- normalizedQuantity
- normalizedUomRef
- normalizedPackageRef
- conversionRuleRef
- conversionConfidence
- roundingApplied
- warningRefs
```

Consuming products should preserve original captured quantity and show normalized quantity only when useful.

## Product usage rules

### SupplyArr

SupplyArr may use Platform Reference Data service to normalize vendor package and UOM context.

SupplyArr still owns internal SKU, vendor SKU, vendor price, lead time, preferred vendor, purchase order line quantity, and commercial pack terms.

### LoadArr

LoadArr may use Platform Reference Data service to normalize scanned receiving quantities and putaway package quantities.

LoadArr still owns inventory balance, stock ledger, receiving transaction, movement transaction, discrepancy, and adjustment.

### MaintainArr

MaintainArr may use Platform Reference Data service to normalize part usage UOM display or attach a known reference entity to a part demand.

MaintainArr still owns work order parts demand, part usage/install record, and return-to-service decision.

### OrdArr

OrdArr may use Platform Reference Data service to normalize requested quantities for orchestration and customer-facing display.

OrdArr still owns order/request line, order lifecycle, and handoff coordination.

## Low-confidence conversion behavior

Low-confidence conversions must not silently post inventory, purchase, issue, or fulfillment quantities.

Low-confidence conversion should produce one of:

```text
- review_required
- choose_package
- choose_uom
- product_owner_required
- tenant_overlay_needed
```

## Package mismatch blocker

A package mismatch may create a blocker in the owning product when operational work cannot proceed.

Examples:

```text
- LoadArr receiving cannot post because case quantity is unknown.
- SupplyArr purchase order needs buyer review because vendor pack changed.
- OrdArr fulfillment handoff is blocked because customer requested pallet quantity but item has no pallet conversion.
```

Platform Reference Data service supplies the reason and suggested conversion candidates. The owning product owns the blocker.

## User-facing display rules

Products should display:

```text
- original captured quantity
- normalized quantity where available
- confidence state
- review reason if blocked
- source summary
```

Products should not display raw conversion JSON by default.

## Events

Recommended Platform Reference Data service events:

```text
- platform.reference_data.uom.created
- platform.reference_data.uom.updated
- platform.reference_data.package.created
- platform.reference_data.package_conversion.created
- platform.reference_data.package_conversion.review_required
- platform.reference_data.package_conversion.published
- platform.reference_data.dataset_version.published
```
