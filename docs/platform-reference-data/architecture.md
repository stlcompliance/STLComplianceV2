# Platform Reference Data Architecture

## Purpose

Platform Reference Data is the shared identity and normalization layer for things that exist independent of one tenant or one product workflow.

It answers:

- What is this thing?
- How do we normalize it?
- What other systems call it?
- Which products may consume it?

It does not answer:

- What did a tenant buy?
- What did a warehouse receive?
- What asset was assigned?
- What rule means legally?
- What file was uploaded?

Those remain with the owning products.

## Ownership boundaries

- `NexArr` owns platform admin access, service identity, tenant validation, entitlement, and the admin shell.
- `ReferenceDataCore` is the platform-owned reference-data service. It relies on NexArr for platform-admin validation and service identity, but it owns shared reference identity and normalization.
- `SupplyArr`, `MaintainArr`, `LoadArr`, `RecordArr`, `StaffArr`, and `Compliance Core` keep their local domain truth.
- No product may directly join another product database.
- Cross-product access happens through APIs, service tokens, events, or read-only snapshots.

## Canonical lifecycle

1. Source selected.
2. Raw source saved.
3. Ingestion job created.
4. Staging records created.
5. Normalization performed.
6. Duplicate and conflict detection performed.
7. Candidate canonical entities proposed.
8. Confidence score assigned.
9. Reviewer approves, merges, rejects, or escalates.
10. Published dataset version created.
11. Products consume through API or events.
12. Publish history and audit trail preserved.

## Ingestion model

Each dataset has:

- one or more sources
- one or more ingestion jobs
- staged records for review
- canonical entities
- entity versions
- crosswalks
- publish history
- audit events

Raw intake is retained long enough to explain how a staging decision was reached, but the primary UI never exposes raw JSON as the default experience.

## Source priority

Source priority is a policy decision, not a hardcoded ingestion guess.

Suggested order:

1. Explicit platform-admin curated source
2. Direct product import from the owning system
3. Recognized connector feed
4. Vendor/public reference feed
5. Manual admin entry
6. Historical legacy import

When multiple sources disagree, the highest authority rank wins unless a reviewer overrides it.

## Normalization

Normalization converts raw source input into a canonical reference shape.

Examples:

- Vehicle: uppercase VIN, normalize year, standardize make/model, preserve decode evidence.
- GTIN/UPC: strip whitespace, validate numeric structure, preserve the original scanned code.
- SDS: normalize manufacturer and product name, capture revision date, preserve document reference.
- Chemical identity: normalize CAS and aliases without replacing Compliance Core rule meaning.

Normalization must preserve evidence. The canonical record should explain where each important field came from.

## Deduplication and conflict detection

The service should propose duplicates when:

- canonical keys match
- aliases normalize to the same identity
- public identifiers match
- evidence strongly overlaps

The service should flag conflicts when:

- authoritative sources disagree
- the same external key maps to multiple canonical entities
- a high-confidence match would override an existing published identity

Duplicate detection should produce a review decision, not an automatic destructive merge.

## Confidence scoring

Confidence is a review aid, not the final truth.

Suggested signals:

- exact public identifier match
- authoritative source rank
- field completeness
- evidence consistency
- reviewer history
- whether the record is already published

Scores should be visible to reviewers but should not replace human approval when the record affects downstream products.

## Review and approval

Review actions:

- approve as new canonical entity
- merge into existing entity
- reject
- escalate

Reviewers can edit proposed normalized fields before approval if the product has permission to do so.

Every review must record:

- reviewer
- reason
- previous state
- resulting state
- audit event

## Versioned publish

Published reference data is versioned.

Rules:

- A dataset can have many published versions.
- A reference entity can have many versions.
- The current published version is read-only to consuming products.
- Superseded versions stay visible in history.

Rollback should be implemented as supersession, not as an invisible overwrite.

## Product consumption

Products consume published reference data through:

- catalog queries
- entity lookups
- lookup-by-public-identifier routes
- crosswalk resolution routes
- service-token-protected APIs

Products may store:

- `referenceEntityId`
- `canonicalKey`
- display snapshots

Products may not store another product’s canonical source-of-truth record as if they owned it.

## Event publication

Reference-data events are emitted after publish or review changes:

- `reference.dataset.published`
- `reference.entity.created`
- `reference.entity.updated`
- `reference.entity.superseded`
- `reference.crosswalk.created`
- `reference.import.completed`
- `reference.import.failed`
- `reference.review.required`

Events are for sync and reporting, not for direct database coupling.

## Tenant overlays

Tenant overlays are allowed when the tenant needs a local label, visibility flag, or note for a shared entity.

Tenant overlays must not change canonical identity.

Allowed overlay examples:

- local display name
- hidden flag
- tenant-specific note
- local status label

Not allowed in overlay:

- vendor pricing
- inventory state
- document truth
- compliance interpretation

## Crosswalks

Crosswalks map an external system key to a canonical reference entity.

Each crosswalk should carry:

- external system
- external key
- source sourceId when known
- confidence
- status

Crosswalks are one of the main tools for migrating legacy free-text reference concepts into the new catalog.

## Audit logging

Every meaningful change should create an audit event with:

- actor
- action
- entity type
- entity id
- before snapshot
- after snapshot

Audit data belongs in the platform admin audit trail, not in raw JSON on the default review pages.

## Failure handling

If ingestion fails:

- keep the raw intake
- mark the job failed
- preserve the error summary
- allow retry or manual recovery

If a source is unavailable:

- the published catalog remains readable
- fresh ingest is deferred
- the UI should clearly label stale or unavailable source state

## Security

- Admin routes require NexArr platform-admin validation.
- Product-consumption routes require service tokens with `referencedata.read` or `referencedata.lookup` scope as appropriate.
- Import, review, and publish routes require stronger scopes such as `referencedata.import.manage`, `referencedata.review`, and `referencedata.publish`.
- No route should trust client-side role checks alone.

## Repo alignment

This repo already treats NexArr as the platform control plane. ReferenceDataCore should integrate with that pattern without becoming a NexArr-owned customer, supplier, or execution domain:

- admin UI lives under `/app/platform-admin/reference-data`
- admin APIs live under `/api/platform-admin/reference-data/*`
- product-consumption APIs live under `/api/v1/reference-data/*`
- platform-owned reference data stays in ReferenceDataCore
