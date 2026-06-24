# Platform Reference Data service — Public Identifier and Reference Entity Model

## Reference dataset

A ReferenceDataset groups related reference entities under a stable dataset key.

Examples:

```text
- vehicle_identity
- vehicle_taxonomy
- public_product_identity
- gtin_identity
- chemical_identity
- sds_metadata_identity
- manufacturer_identity
- brand_identity
- equipment_taxonomy
- package_uom
```

```text
ReferenceDataset
- datasetId
- datasetKey
- displayName
- description
- datasetCategory
  - public_identifier
  - taxonomy
  - manufacturer_brand
  - vehicle
  - equipment
  - product
  - chemical
  - sds_metadata
  - uom_package
  - crosswalk
  - other
- ownerProductKey
  - platform.reference_data
- status
  - draft
  - active
  - publish_pending
  - superseded
  - archived
- visibility
  - platform_admin_only
  - product_consumable
  - tenant_overlay_allowed
  - public_display_allowed
- sourcePriorityPolicyRef
- latestPublishedVersionRef
- notes
- auditTrail
```

## Reference dataset version

ReferenceDatasetVersion is the immutable published view products should consume.

```text
ReferenceDatasetVersion
- datasetVersionId
- datasetId
- versionKey
- publishNumber
- status
  - draft
  - review
  - published
  - superseded
  - withdrawn
- publishedAt
- publishedByPersonId
- sourceSummary
- entityCount
- changeSummary
- checksum
- previousDatasetVersionRef
- releaseNotes
- auditTrail
```

## Reference entity

A ReferenceEntity is the canonical shared identity for a thing inside a dataset.

```text
ReferenceEntity
- referenceEntityId
- datasetId
- canonicalKey
- displayName
- normalizedName
- entityType
  - vehicle_make
  - vehicle_model
  - vehicle_body_type
  - equipment_class
  - product_identity
  - chemical
  - manufacturer
  - brand
  - unit_of_measure
  - package
  - material
  - document_metadata_identity
  - other
- status
  - candidate
  - active
  - review_required
  - merged
  - split
  - superseded
  - deprecated
  - archived
- primaryIdentifierRef
- publicIdentifierRefs
- aliasRefs
- taxonomyRefs
- parentEntityRefs
- childEntityRefs
- sourceRefs
- confidenceScore
- publishedDatasetVersionRefs
- createdAt
- updatedAt
- auditTrail
```

## Reference entity version

Entity versions preserve history when a canonical reference entity changes.

```text
ReferenceEntityVersion
- entityVersionId
- referenceEntityId
- datasetVersionId
- canonicalKey
- displayName
- normalizedName
- attributeSnapshot
- identifierSnapshot
- aliasSnapshot
- sourceSnapshot
- changeType
  - created
  - updated
  - merged
  - split
  - superseded
  - deprecated
- effectiveAt
- supersededAt
- checksum
```

## Public identifier type

```text
PublicIdentifierType
- identifierTypeId
- identifierTypeKey
- displayName
- description
- formatRule
- normalizedFormatRule
- examples
- datasetApplicability
- status
  - active
  - deprecated
  - archived
```

Recommended identifier types:

```text
- vin
- gtin
- upc
- ean
- cas_number
- un_number
- na_number
- epa_registration_number
- manufacturer_part_number
- oem_part_number
- external_catalog_number
- sds_revision_identifier
- nhtsa_make_id
- nhtsa_model_id
- gs1_company_prefix
```

Manufacturer part numbers and OEM part numbers may be reference identifiers, but commercial supplier SKU truth remains SupplyArr.

## Public identifier

```text
PublicIdentifier
- publicIdentifierId
- referenceEntityId
- identifierTypeKey
- rawValue
- normalizedValue
- displayValue
- issuingAuthority
- countryOrRegion
- sourceRef
- confidenceScore
- status
  - active
  - review_required
  - duplicate
  - invalid
  - superseded
  - deprecated
- firstSeenAt
- lastVerifiedAt
- notes
```

## Identifier normalization examples

```text
VIN
- trim whitespace
- uppercase letters
- reject I/O/Q where validation requires
- preserve original captured value in source metadata
- store normalizedValue for lookup

UPC/GTIN
- trim whitespace
- preserve leading zeroes
- validate numeric length
- preserve original scan value
- avoid inferring package quantity unless confidence is high

CAS number
- normalize hyphenation
- validate check digit when possible
- preserve original source value

UN/NA number
- normalize prefix and numeric part
- keep hazardous material regulatory meaning in Compliance Core
```

## Reference source

ReferenceSource describes where reference values came from.

```text
ReferenceSource
- sourceId
- sourceKey
- displayName
- sourceType
  - platform_admin_curated
  - official_public_api
  - public_dataset
  - vendor_feed
  - product_import
  - csv_import
  - manual_entry
  - legacy_seed
  - document_metadata
- authorityRank
- ownerContact
- refreshCadence
- sourceUrlOrConnectorRef
- recordArrFileRef
- status
  - active
  - paused
  - deprecated
  - failed
- notes
```

## Source record

SourceRecord preserves traceability from raw input to normalized reference candidates.

```text
SourceRecord
- sourceRecordId
- sourceId
- ingestionJobId
- rawSourceRef
- rowNumber
- rawDisplaySummary
- parsedFieldSummary
- normalizedFieldSummary
- sourceChecksum
- receivedAt
- processingStatus
  - parsed
  - normalized
  - candidate_created
  - duplicate_detected
  - conflict_detected
  - rejected
  - failed
```

Raw payloads may be available in technical/admin panels, but reviewer UIs should default to plain-language summaries.

## Tenant reference overlay

TenantReferenceOverlay allows tenant-specific labels or preferences without changing platform canonical identity.

```text
TenantReferenceOverlay
- overlayId
- tenantId
- referenceEntityId
- preferredDisplayName
- tenantAliasRefs
- visibilityStatus
  - visible
  - hidden
  - restricted
- defaultUomRef
- defaultPackageRef
- notes
- createdByPersonId
- updatedByPersonId
```

## Product attachment

A consuming product may attach a reference entity to its local record.

```text
ReferenceAttachment
- attachmentId
- tenantId
- productKey
- localObjectType
- localObjectId
- referenceEntityId
- datasetKey
- datasetVersionRef
- attachmentSource
  - user_selected
  - scan_lookup
  - import_mapping
  - ai_suggested
  - service_matched
  - system_default
- confidenceScore
- status
  - suggested
  - accepted
  - rejected
  - stale
- acceptedByPersonId
- acceptedAt
```

ReferenceAttachment may live in the consuming product or in Platform Reference Data service depending on implementation, but the consuming product owns the local business record.

## Public identifier API behavior

Lookup routes should return:

```text
- referenceEntityRef
- datasetVersion
- displayName
- normalized identifiers
- confidence
- source summary
- whether review is required
- allowed consuming products
- suggested product-local snapshot fields
```

Lookup routes must not create SupplyArr items, MaintainArr assets, LoadArr inventory balances, or Compliance Core requirements by themselves.
