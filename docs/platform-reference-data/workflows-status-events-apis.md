# Platform Reference Data service — Workflows, Statuses, Events, and APIs

## Workflow principles

Platform Reference Data service workflows turn raw reference input into published, reviewable, reusable reference truth.

Platform Reference Data service must not silently mutate product-owned operational records.

## Dataset lifecycle

```text
draft
review
active
publish_pending
published
superseded
withdrawn
archived
```

## Reference entity lifecycle

```text
candidate
review_required
active
merged
split
superseded
deprecated
archived
rejected
```

## Import job lifecycle

```text
created
queued
parsing
normalizing
matching
review_required
ready_to_publish
published
partially_failed
failed
canceled
```

## Staging record lifecycle

```text
parsed
normalized
candidate_created
exact_match
probable_match
multiple_candidates
conflict_detected
review_required
approved
merged
rejected
escalated
published
failed
```

## Workflow: create dataset

1. Platform admin creates dataset.
2. Platform Reference Data service validates dataset key.
3. Source priority policy is attached.
4. Dataset remains draft until sources and review rules are configured.
5. Dataset may be activated for import.
6. Dataset is not product-consumable until a version is published.

## Workflow: import reference data

1. Platform admin selects dataset.
2. Platform admin selects source.
3. File, connector, or manual intake is submitted.
4. Platform Reference Data service creates import job.
5. Raw intake metadata is preserved.
6. Parser creates source records.
7. Normalizer creates staging records.
8. Matcher checks identifiers, aliases, taxonomies, and crosswalks.
9. Low-risk exact matches may be auto-linked if policy allows.
10. Conflicts and low-confidence candidates go to review.
11. Approved records are included in a publish candidate.
12. Publish creates immutable dataset version.

## Workflow: review staging record

Allowed reviewer actions:

```text
- approve_new_entity
- link_to_existing_entity
- merge_with_existing_entity
- reject
- escalate
- request_source_correction
- defer
```

Review decision fields:

```text
- decision
- reviewerPersonId
- reason
- confidenceOverride
- affectedEntityRefs
- affectedCrosswalkRefs
- auditTrailRef
```

## Workflow: publish dataset version

1. Platform Reference Data service builds publish candidate.
2. Publish gate checks unresolved critical conflicts.
3. Version checksum is generated.
4. DatasetVersion is published.
5. Product-consumption APIs serve the new published version.
6. `platform.reference_data.dataset_version.published` is emitted.
7. Consumers refresh published caches or snapshots as needed.

## Workflow: product lookup

1. Product calls lookup route with service token.
2. Platform Reference Data service validates service scope.
3. Lookup runs against published data by default.
4. Response returns exact/probable/no match.
5. If review is required, the product may create a product-owned blocker or task.
6. If accepted, product stores reference attachment/snapshot locally.

## Workflow: request new reference candidate

1. Product submits unresolved value with context.
2. Platform Reference Data service creates candidate or review request.
3. Candidate is routed to review queue.
4. Product receives candidate status.
5. Product may continue with local-only value if its own workflow allows.
6. Published reference becomes available only after review/publish.

## Required statuses for product-facing lookup

```text
exact_match
probable_match
multiple_candidates
no_match
invalid_input
review_required
service_unavailable
dataset_not_published
```

## Event naming

All events must use canonical product key prefix:

```text
platform.reference_data.{resource}.{past_tense_fact}
```

## Recommended events

```text
platform.reference_data.dataset.created
platform.reference_data.dataset.activated
platform.reference_data.dataset_version.published
platform.reference_data.dataset_version.superseded
platform.reference_data.dataset_version.withdrawn

platform.reference_data.import_job.created
platform.reference_data.import_job.completed
platform.reference_data.import_job.failed
platform.reference_data.import_job.review_required

platform.reference_data.staging_record.created
platform.reference_data.staging_record.approved
platform.reference_data.staging_record.rejected
platform.reference_data.staging_record.escalated
platform.reference_data.staging_record.conflict_detected

platform.reference_data.reference_entity.created
platform.reference_data.reference_entity.updated
platform.reference_data.reference_entity.merged
platform.reference_data.reference_entity.split
platform.reference_data.reference_entity.superseded
platform.reference_data.reference_entity.deprecated

platform.reference_data.crosswalk.created
platform.reference_data.crosswalk.updated
platform.reference_data.crosswalk.review_required

platform.reference_data.resolution.requested
platform.reference_data.resolution.completed
platform.reference_data.resolution.review_required
```

## Event payload minimums

Every event should include:

```text
- datasetKey
- datasetVersionRef where applicable
- referenceEntityRef where applicable
- sourceRef where applicable
- confidenceScore where applicable
- affectedProductKeys where known
- changedFields summary
- correlationId
```

## API routes

### Admin routes

```text
GET /api/v1/reference-data/datasets
POST /api/v1/reference-data/datasets
GET /api/v1/reference-data/datasets/{datasetId}
PATCH /api/v1/reference-data/datasets/{datasetId}
GET /api/v1/reference-data/datasets/{datasetId}/versions
POST /api/v1/reference-data/datasets/{datasetId}/publish

GET /api/v1/reference-data/sources
POST /api/v1/reference-data/sources
GET /api/v1/reference-data/imports
POST /api/v1/reference-data/imports
GET /api/v1/reference-data/imports/{jobId}
GET /api/v1/reference-data/imports/{jobId}/staging-records

POST /api/v1/reference-data/staging-records/{stagingRecordId}/approve
POST /api/v1/reference-data/staging-records/{stagingRecordId}/link
POST /api/v1/reference-data/staging-records/{stagingRecordId}/merge
POST /api/v1/reference-data/staging-records/{stagingRecordId}/reject
POST /api/v1/reference-data/staging-records/{stagingRecordId}/escalate

GET /api/v1/reference-data/entities
GET /api/v1/reference-data/entities/{referenceEntityId}
PATCH /api/v1/reference-data/entities/{referenceEntityId}
POST /api/v1/reference-data/entities/{referenceEntityId}/merge
POST /api/v1/reference-data/entities/{referenceEntityId}/split

GET /api/v1/reference-data/crosswalks
POST /api/v1/reference-data/crosswalks
PATCH /api/v1/reference-data/crosswalks/{crosswalkId}
```

### Product-consumption routes

```text
GET /api/v1/reference-data/catalogs/{datasetKey}/entities
GET /api/v1/reference-data/entities/{referenceEntityId}
POST /api/v1/reference-data/lookup
POST /api/v1/reference-data/crosswalks/resolve
POST /api/v1/reference-data/resolution-requests
GET /api/v1/reference-data/resolution-requests/{resolutionRequestId}
```

### Specialized lookup routes

```text
GET /api/v1/reference-data/vehicles/decode-vin?vin={vin}&modelYear={year}
GET /api/v1/reference-data/products/lookup-gtin?gtin={gtin}
GET /api/v1/reference-data/sds/lookup?manufacturer={name}&product={name}
GET /api/v1/reference-data/chemicals/lookup?cas={cas}
GET /api/v1/reference-data/uom/convert
```

## Service-token scopes

Recommended scopes:

```text
platform.reference_data.datasets.read
platform.reference_data.datasets.manage
platform.reference_data.imports.manage
platform.reference_data.review.manage
platform.reference_data.publish.manage
platform.reference_data.lookup.read
platform.reference_data.crosswalks.read
platform.reference_data.crosswalks.manage
platform.reference_data.resolution.request
```

Products should receive only the minimum scopes they need.

## Error behavior

Recommended error codes:

```text
reference_dataset_not_found
reference_dataset_not_published
reference_entity_not_found
reference_lookup_no_match
reference_lookup_multiple_candidates
reference_lookup_review_required
reference_conflict_unresolved
reference_source_not_authorized
reference_publish_blocked
reference_conversion_unknown
reference_conversion_low_confidence
```

Errors should be machine-readable and user-facing messages should be plain language.
