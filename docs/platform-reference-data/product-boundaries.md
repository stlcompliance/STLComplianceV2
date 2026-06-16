# Platform Reference Data Product Boundaries

## Shared rule

ReferenceDataCore centralizes shared identity and normalization only.

Products keep their own operational truth.

## MaintainArr

May consume:

- vehicle decode
- equipment taxonomy
- public identity snapshots

Must keep:

- asset master
- asset assignment
- readiness
- work orders
- inspections

## SupplyArr

May consume:

- UPC/GTIN identity
- manufacturer identity
- public product categories
- SDS metadata identity

Must keep:

- internal SKU
- vendor SKU
- vendor pricing
- lead time
- procurement context

## LoadArr

May consume:

- UPC/GTIN lookup
- crosswalked product identity

Must keep:

- inventory balances
- stock ledger
- receiving transactions
- movement events

## RecordArr

May consume:

- SDS metadata matching
- document-linked reference identity

Must keep:

- stored files
- hashes
- versioning
- retention evidence

## Compliance Core

May consume:

- chemical identifiers
- SDS metadata identity
- reference vocabulary

Must keep:

- regulatory meaning
- citations
- hazard mappings
- compliance interpretation

## StaffArr

May consume:

- canonical person/location references when needed by product workflows

Must keep:

- people
- org structure
- internal locations
- permissions

## No direct joins

Products must not query ReferenceDataCore tables directly from their own databases.

Cross-product access is only through APIs, events, or approved service-token flows.
