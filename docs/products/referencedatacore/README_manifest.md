# ReferenceDataCore Granular End-Goal Markdown Package

This package defines ReferenceDataCore at the domain-object level.

ReferenceDataCore is the platform-owned shared reference identity and normalization service for STL Compliance.

## Files

- `referencedatacore_00_scope_and_boundaries.md`
- `referencedatacore_01_public_identifier_model.md`
- `referencedatacore_02_uom_package_normalization_model.md`
- `referencedatacore_03_manufacturer_brand_taxonomy_model.md`
- `referencedatacore_04_crosswalk_alias_resolution_model.md`
- `referencedatacore_05_workflows_status_events_apis.md`

## Purpose

ReferenceDataCore owns shared public/reference identity and normalization for things that exist outside one tenant's operational workflow.

It covers:

- Public identifiers
- UPC/GTIN normalization
- VIN/decode identity
- CAS/chemical identifier identity
- Manufacturer and brand identity
- Unit-of-measure and package normalization
- Shared public product, vehicle, equipment, chemical, and material taxonomies
- Crosswalks and aliases across external feeds and product-local snapshots
- Published reference dataset versions
- Reference-data import staging, review, merge, reject, and publish lifecycle

ReferenceDataCore does not own tenant operations, commercial terms, inventory, procurement, regulatory interpretation, document files, internal locations, asset readiness, order lifecycle, customer truth, supplier truth, or accounting execution.

## Relationship to existing platform-reference-data docs

The existing `platform-reference-data/` folder remains useful as architecture, source priority, API, import-process, and target-inventory planning material.

This product package is the canonical product-level constitution/model set for the `referencedatacore` product key.
