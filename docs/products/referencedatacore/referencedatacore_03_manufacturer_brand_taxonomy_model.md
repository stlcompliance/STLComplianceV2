# ReferenceDataCore — Manufacturer, Brand, and Taxonomy Model

## Purpose

This model defines reusable manufacturer, brand, product, vehicle, equipment, chemical, and material taxonomy identity.

ReferenceDataCore centralizes shared taxonomy and identity.

Products keep local operational context.

## Taxonomy hierarchy and ownership

ReferenceDataCore treats taxonomy as a governed hierarchy, not a flat value list.

The owning category exposes the relevant class for its domain, and that class gates the valid types and subtypes underneath it.

Rules:

- A category owns and exposes the canonical class for that branch of the taxonomy.
- The class owns and exposes the allowed types for that branch.
- Each type owns and exposes the allowed subtypes beneath it.
- Products may consume published class/type/subtype snapshots, but they must not invent sibling values outside the owning hierarchy.
- Parent references and canonical keys must preserve historical resolvability when the hierarchy changes.

Example:

```text
Vehicle category
  -> vehicle class
    -> vehicle types
      -> vehicle subtypes
```

```text
Material category
  -> material class
    -> material types
      -> material subtypes
```

## Manufacturer identity

```text
Manufacturer
- manufacturerId
- canonicalKey
- displayName
- normalizedName
- legalName
- aliasRefs
- brandRefs
- publicIdentifierRefs
- countryOrRegion
- websiteSummary
- status
  - candidate
  - active
  - review_required
  - merged
  - superseded
  - deprecated
  - archived
- sourceRefs
- confidenceScore
- notes
```

## Brand identity

```text
Brand
- brandId
- canonicalKey
- displayName
- normalizedName
- manufacturerRefs
- aliasRefs
- publicIdentifierRefs
- status
  - candidate
  - active
  - review_required
  - merged
  - superseded
  - deprecated
  - archived
- sourceRefs
- confidenceScore
```

## Product identity

ProductIdentity describes a public/reference product identity.

It is not a SupplyArr internal SKU.

```text
ProductIdentity
- productIdentityId
- canonicalKey
- displayName
- normalizedName
- manufacturerRef
- brandRef
- productCategoryRefs
- publicIdentifierRefs
- gtinRefs
- manufacturerPartNumberRefs
- defaultUomRef
- defaultPackageRefs
- status
  - candidate
  - active
  - review_required
  - merged
  - superseded
  - deprecated
  - archived
- sourceRefs
- confidenceScore
```

SupplyArr owns internal item records, supplier/vendor items, prices, lead times, purchase settings, and commercial catalog status.

## Vehicle taxonomy

```text
VehicleTaxonomyEntity
- vehicleTaxonomyId
- canonicalKey
- entityType
  - make
  - model
  - model_year
  - body_class
  - vehicle_type
  - fuel_type
  - engine_type
  - brake_system
  - gvwr_class
  - trailer_type
  - equipment_type
- displayName
- parentRefs
- publicIdentifierRefs
- sourceRefs
- status
```

MaintainArr owns assets. RoutArr owns trip/dispatch equipment context.

ReferenceDataCore may provide decode identity and taxonomy suggestions, but it does not create or manage assets.

## Equipment taxonomy

```text
EquipmentTaxonomyEntity
- equipmentTaxonomyId
- canonicalKey
- displayName
- normalizedName
- equipmentFamily
- equipmentClass
- parentRefs
- relatedVehicleTaxonomyRefs
- examples
- status
- sourceRefs
```

Examples:

```text
- tractor
- straight_truck
- trailer
- forklift
- pallet_jack
- conveyor
- compressor
- generator
- shop_tool
- dock_equipment
```

A taxonomy entry does not imply that an asset exists in MaintainArr.

## Chemical identity

```text
ChemicalIdentity
- chemicalIdentityId
- canonicalKey
- displayName
- normalizedName
- casNumberRefs
- unNumberRefs
- naNumberRefs
- synonymRefs
- chemicalFamilyRefs
- status
- sourceRefs
- confidenceScore
```

Compliance Core owns hazard meaning, regulatory classification, applicability, and rule interpretation.

RecordArr owns SDS document files.

ReferenceDataCore owns shared chemical identifier matching.

## SDS metadata identity

```text
SdsMetadataIdentity
- sdsMetadataIdentityId
- canonicalKey
- productIdentityRef
- manufacturerRef
- normalizedProductName
- revisionDate
- language
- countryOrRegion
- recordArrDocumentRef
- chemicalRefs
- sourceRefs
- status
  - candidate
  - active
  - review_required
  - superseded
  - archived
```

SDS document upload may originate in RecordArr, SupplyArr, or Compliance Core, but the file remains RecordArr truth.

## Material taxonomy

```text
MaterialTaxonomyEntity
- materialTaxonomyId
- canonicalKey
- displayName
- normalizedName
- materialFamily
- parentRefs
- chemicalRefs
- productCategoryRefs
- status
- sourceRefs
```

Compliance Core may use material taxonomy as facts or evidence context, but Compliance Core owns the regulatory consequences.

## Taxonomy relationship

```text
TaxonomyRelationship
- relationshipId
- sourceEntityRef
- targetEntityRef
- relationshipType
  - parent_child
  - equivalent_to
  - narrower_than
  - broader_than
  - commonly_confused_with
  - supersedes
  - related_to
- confidenceScore
- sourceRefs
- status
```

## Alias

```text
ReferenceAlias
- aliasId
- referenceEntityRef
- aliasValue
- normalizedAliasValue
- aliasType
  - spelling
  - abbreviation
  - trade_name
  - legacy_name
  - supplier_name
  - tenant_name
  - external_system_name
  - common_name
- tenantId where tenant-specific
- sourceRefs
- confidenceScore
- status
```

## Merge and split rules

A merge means two or more candidate entities are the same reference identity.

A split means one candidate has been incorrectly used for multiple distinct identities.

Merge/split review must preserve:

```text
- previous entity refs
- new canonical entity refs
- affected aliases
- affected identifiers
- affected crosswalks
- affected published versions
- downstream product notification needs
- reviewer
- reason
- time
```

Products may need stale-reference warnings when a previously attached reference entity is merged, split, superseded, or withdrawn.

## Taxonomy display rules

Products may display friendly labels from ReferenceDataCore, but they should retain local snapshots for audit when important decisions were made.

Example:

```text
A work order created while an asset was classified as `tractor` should retain the classification snapshot used at that time even if the taxonomy later changes.
```
