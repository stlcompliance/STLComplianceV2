# ReferenceDataCore — Scope, Ownership, and Boundaries

## Product purpose

ReferenceDataCore is the shared reference identity and normalization service for STL Compliance.

It owns reusable identity for real-world things that exist outside a single tenant's internal workflow.

ReferenceDataCore answers:

- What public or shared identifier describes this thing?
- What normalized reference entity does this external value point to?
- What names, aliases, codes, and external identifiers refer to the same thing?
- Which dataset/version produced this reference value?
- Which source was used, and how confident is the match?
- Which products may consume this reference value?
- What should the suite use as the canonical public/reference identity?
- Which tenant overlays or product snapshots are allowed without changing canonical truth?

ReferenceDataCore does not answer:

- What did a tenant buy?
- What is physically in a warehouse?
- Which supplier offered a price?
- Which customer requested service?
- Which asset is ready for service?
- Which rule applies legally?
- Which document file is stored?
- Which employee or internal location exists?
- Which order should be invoiced?

Those remain with the owning products.

## ReferenceDataCore owns

```text
- Reference datasets
- Dataset versions
- Public identifier definitions
- Public identifier normalization
- Reference entities
- Reference entity versions
- Entity aliases
- Crosswalks
- External identifier mappings
- Source authority ranking
- Source ingestion jobs
- Raw reference intake metadata
- Staging records
- Candidate entity resolution
- Duplicate detection
- Merge/split review history
- Published reference catalogs
- Manufacturer identity
- Brand identity
- Public product identity
- UPC/GTIN normalization
- VIN/decode identity and reference snapshots
- CAS/chemical identifier identity
- Public chemical/material identity
- Shared vehicle taxonomy
- Shared equipment taxonomy
- Shared product taxonomy
- Unit-of-measure catalog
- Package/unit conversion rules
- Reference data publish events
- Reference lookup APIs
- Reference-data audit trail
```

## ReferenceDataCore does not own

```text
- Platform login
- Tenant entitlement
- Platform admin identity
- Person master
- Internal organization structure
- Internal location hierarchy
- Staff permissions
- Customer master
- Customer contacts
- Customer locations
- Customer requirements
- Supplier/vendor master
- Supplier performance
- Vendor pricing
- Purchase requests
- Purchase orders
- Internal SKU truth
- Warehouse inventory balance
- Stock ledger
- Receiving execution
- Asset master
- Asset readiness
- Work orders
- Defects
- Inspections
- Training programs
- Training assignments
- Certificates
- Regulatory meaning
- Applicability decisions
- Evidence requirements
- Stored files
- Document retention
- Order/request lifecycle
- Dispatch/trip execution
- Quality holds
- CAPA decisions
- Report definitions
- Accounting execution
```

## External product dependencies

```text
NexArr
- Platform-admin validation
- Service-client identity
- Tenant validation
- Entitlement and launch context

StaffArr
- Platform-admin person references
- Internal owner/reviewer authority context where applicable
- Internal location references only when a reference-data workflow needs internal routing

Compliance Core
- Regulatory vocabulary consumption
- Chemical/material identity consumption
- SDS metadata identity consumption
- Citation/reference-data separation
- Regulatory meaning remains in Compliance Core

RecordArr
- Stored source files
- SDS files
- Import files when retained as records
- Evidence of source documents
- Generated export packages

SupplyArr
- Internal SKU mapping
- Vendor SKU mapping
- Supplier/vendor item context
- Purchase/procurement context
- Commercial overlays

LoadArr
- Inventory item lookups
- Scanned UPC/GTIN resolution
- Package/UOM conversion assistance
- Warehouse movement remains LoadArr truth

MaintainArr
- VIN/decode lookup
- Asset make/model/category normalization assistance
- Equipment taxonomy consumption
- Asset truth remains MaintainArr

RoutArr
- Vehicle/equipment identity lookup
- Transportation equipment snapshots
- Trip/dispatch execution remains RoutArr

AssurArr
- Quality classification and nonconformance reference context
- Hold/release truth remains AssurArr

ReportArr
- Reference dataset reporting
- Reference data quality dashboards
- Cross-product normalized dimension labels

Field Companion
- Mobile scan/lookup surfaces
- Offline reference cache consumption

STL Compliance Site
- Public product/industry reference copy where approved for public display
```

## Core source-of-truth rules

```text
1. ReferenceDataCore owns public/reference identity.
2. Product owners keep tenant operational truth.
3. SupplyArr owns internal SKU and supplier/vendor commercial context.
4. LoadArr owns inventory balances and stock ledger.
5. MaintainArr owns asset registry and readiness.
6. StaffArr owns internal people and internal places.
7. CustomArr owns customer identity.
8. Compliance Core owns rule meaning and evidence requirements.
9. RecordArr owns stored files and retained evidence.
10. ReferenceDataCore may suggest matches, but the consuming product chooses whether and how to attach the reference.
11. Products may store snapshots of reference labels for display and audit.
12. A product snapshot must not become a competing canonical reference dataset.
13. Direct cross-database joins are forbidden.
14. Products consume ReferenceDataCore through APIs, events, service-token flows, or approved read-only published cache mechanisms.
```

## Standard ReferenceDataCore object envelope

Every major ReferenceDataCore object should include:

```text
- tenantId where tenant-specific overlay is allowed
- referenceDataCoreId
- objectType
- datasetKey
- datasetVersion
- canonicalKey
- displayName
- status
- sourceAuthority
- confidenceScore
- createdAt
- createdByPersonId or createdByServiceClient
- updatedAt
- updatedByPersonId or updatedByServiceClient
- publishedAt
- supersededAt
- correlationId
- auditTrailRef
```

Platform-level canonical reference data may be global, but any tenant overlay, tenant alias, or tenant visibility rule must include tenant context.

## ReferenceDataCore object prefixes

Object prefixes are product-scoped. A globally meaningful reference must include `productKey`, `objectType`, and stable ID.

Suggested ReferenceDataCore prefixes:

```text
- RDC - ReferenceDataCore entity
- RDS - Reference dataset
- RDV - Reference dataset version
- RID - Reference public identifier
- RXW - Reference crosswalk
- RAL - Reference alias
- RUM - Reference unit of measure
- RPK - Reference package rule
- RST - Reference staging record
- RIM - Reference import job
- RMR - Reference merge review
- RPV - Reference publish version
```

## Access model

ReferenceDataCore has two broad access classes:

```text
Platform-admin access
- manage datasets
- create sources
- review staged records
- merge/split reference entities
- publish dataset versions
- inspect source history
- manage source priority
- resolve high-risk conflicts

Product-consumption access
- lookup published reference data
- resolve crosswalks
- request a new reference candidate
- attach a reference to a product-local record
- receive published reference events
```

Platform-admin routes require server-side NexArr validation.

Product-consumption routes require service-token scopes.

Tenant users should not be able to directly modify platform canonical reference data through product UIs.

## Published data rule

Products should consume published reference dataset versions by default.

Draft, staging, rejected, and conflict records are admin/review artifacts, not product-facing truth.

Products may surface "reference lookup unavailable" or "match requires review" states, but they must not silently create canonical reference data without a ReferenceDataCore workflow.

## Tenant overlay rule

Tenant overlays may exist for:

```text
- tenant-specific aliases
- tenant preferred display labels
- tenant item visibility flags
- tenant default package/UOM preferences
- tenant-local mapping to an internal SKU or item number
```

Tenant overlays must not override platform canonical identity for all tenants.

## Implementation guardrails

- Do not move SupplyArr commercial terms into ReferenceDataCore.
- Do not move LoadArr inventory balances into ReferenceDataCore.
- Do not move Compliance Core regulatory meaning into ReferenceDataCore.
- Do not move RecordArr stored files into ReferenceDataCore.
- Do not make ReferenceDataCore a hidden monolith for every dropdown.
- Do not expose raw JSON as the default reviewer UI.
- Do not rely on source names alone for identity matching.
- Do not direct-join product databases.
