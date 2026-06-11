# STL Compliance Platform Reference Data and Ingestion Constitution

## 1. Purpose

This constitution defines how STL Compliance imports, stages, reviews, approves, updates, and serves platform reference data.

Platform reference data helps products use shared controlled values without turning tenant-owned operational data into global truth.

## 2. Scope

This constitution applies to platform-owned or platform-curated datasets such as:

- Vehicle taxonomy
- Asset class/type catalogs
- Make/model/year reference data
- SDS and hazardous material catalogs where platform-curated
- Governing body catalogs
- Compliance vocabulary
- Part/category taxonomy
- UPC/SKU/item reference catalogs where platform-neutral
- Unit-of-measure catalogs
- Location type vocabulary
- Document/evidence type catalogs
- Common external system type catalogs

It does not apply to tenant-owned operational records such as a tenant's actual assets, people, vendors, customers, inventory balances, work orders, trips, documents, or orders.

## 3. Prime directive

Platform reference data must not silently become tenant-owned truth.

Tenant operational data must not silently become platform reference data.

All imports are staged first, then reviewed and approved before becoming canonical platform reference data.

## 4. Dataset ownership

A platform dataset must have one owner.

Possible owners:

- Compliance Core for regulatory vocabulary, governing bodies, citations, rulepacks, evidence types, applicability terms
- MaintainArr for asset taxonomy consumption and maintenance-oriented reference fields when not regulatory
- SupplyArr for item/material/part category reference consumption
- LoadArr for inventory/warehouse operational vocabulary consumption
- StaffArr for internal location type vocabulary consumption
- NexArr/platform admin for tenant/product/package/platform catalogs

Where a dataset is shared across products, the ownership constitution decides the owner.

## 5. Platform reference vs tenant-owned data

Platform reference examples:

- `vehicle.class.passenger_car`
- `asset.type.semi_tractor`
- `governing_body.fmcsa`
- `evidence_type.driver_qualification_file`
- `material.hazard_class.flammable_gas`
- `uom.each`

Tenant-owned examples:

- Tenant's truck `TRK-1042`
- Tenant's employee `Marcus Hill`
- Tenant's warehouse bin count
- Tenant's O'Reilly vendor account
- Tenant's work order
- Tenant's SDS file attachment
- Tenant's customer requirement note

Do not include tenant-owned or tenant-derived operational values in platform reference imports unless explicitly promoted through a governance process.

## 6. Import routing columns

Reference-data imports should support routing columns:

- `product`
- `dataset`
- `dataset_key`

or:

- `product`
- `dataset`

The routing fields tell the import system which product/dataset owner should review the row.

## 7. Identity columns

Reference-data imports should include:

- `entity_type`
- `canonical_key`
- `display_name`

The `canonical_key` should be stable, readable, and namespaced enough to avoid collisions.

Examples:

- `asset.class.passenger_vehicle`
- `asset.type.semi_tractor`
- `governing_body.osha`
- `docs.req.driver_qualification_file`
- `material.hazard_class.flammable_gas`

## 8. Optional provenance columns

Imports should support:

- `source_system`
- `source_key`
- `confidence`
- `fields_json`

Additional useful columns:

- `description`
- `parent_key`
- `status`
- `effective_date`
- `deprecated_at`
- `replacement_key`
- `notes`

## 9. Staging state

Every imported row starts staged.

Recommended states:

- `staged`
- `needs_mapping`
- `duplicate_candidate`
- `needs_review`
- `approved`
- `rejected`
- `merged`
- `deprecated`
- `superseded`

Rows must not become canonical merely because they were imported.

## 10. Review behavior

Review must be row-by-row or batch-with-review-summary.

A reviewer should see:

- Proposed canonical key
- Display name
- Dataset
- Source system
- Source key
- Confidence
- Duplicate candidates
- Parent/category mapping
- Changed fields
- Missing required fields
- Suggested merge/supersede behavior

Ambiguous imports must stay reviewable.

## 11. Confidence

Confidence is a review aid, not final truth.

Suggested confidence scale:

- `1.0` exact trusted match
- `0.8-0.99` high confidence
- `0.5-0.79` needs review
- `<0.5` low confidence/manual review

Low-confidence rows must not auto-approve.

## 12. Canonical keys

Canonical keys should be:

- Stable
- Lowercase
- Dot-separated or similarly namespaced
- Human readable
- Not tenant-specific
- Not source-system-specific unless the dataset intentionally maps that source

Do not use database IDs as portable canonical keys.

## 13. Field JSON

`fields_json` may carry dataset-specific structured fields.

Rules:

- It must validate against the dataset schema.
- It must not hide required identity/routing fields.
- It must not contain tenant-owned operational values.
- It must not be exposed to normal users as raw JSON.
- UI must render fields in readable form.

## 14. Updates and deprecation

Reference data updates must preserve historical resolvability.

Do not delete canonical keys that may exist on historical records.

Use:

- `deprecated`
- `replacement_key`
- `superseded`
- `merged`

Records referencing old keys should still resolve with a warning or replacement suggestion.

## 15. Product consumption

Products should consume reference data through APIs/catalog providers, not copied CSVs.

Catalog providers should return:

- Canonical key
- Display name
- Description
- Parent/grouping
- Status
- Effective/deprecated state
- Source/provenance where useful

## 16. Source provenance

Approved rows must preserve provenance.

Provenance should include:

- Source system
- Source key
- Import batch ID
- Import time
- Reviewed by
- Reviewed time
- Confidence
- Merge/supersede history

## 17. Import batch audit

Each import batch should record:

- Batch ID
- Uploaded by
- Uploaded time
- File name/source
- Target product/dataset
- Row counts
- Validation errors
- Approved/rejected counts
- Review status

## 18. Anti-patterns

The following are not allowed:

- Auto-promoting tenant values to platform catalogs
- Product-specific duplicate catalogs for shared platform reference data
- Free-text controlled values where catalog values exist
- Deleting deprecated keys that historical records reference
- Raw JSON shown to ordinary users
- Importing rows without source/provenance
- Auto-approving ambiguous source data
- Using tenant asset names, vendor names, or customer names as platform reference values

## 19. Minimum acceptable implementation

A platform reference import is minimally acceptable when it has:

1. Product/dataset routing
2. Canonical key and display name
3. Source/provenance
4. Staging before approval
5. Review state
6. Duplicate/merge/deprecation handling
7. Tenant-owned data exclusion
8. Catalog API/provider for product consumption
9. Historical key resolvability
