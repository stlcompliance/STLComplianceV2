# Platform Reference Data service — Crosswalk, Alias, and Resolution Model

## Purpose

Crosswalks connect external identifiers, product-local identifiers, legacy values, and tenant aliases to canonical reference entities.

Platform Reference Data service owns shared crosswalk and resolution rules.

Products own their local records and may choose whether to accept a suggested resolution.

## Crosswalk

```text
ReferenceCrosswalk
- crosswalkId
- referenceEntityId
- datasetKey
- externalSystemKey
- externalEntityType
- externalIdentifierType
- externalIdentifierValue
- normalizedExternalIdentifierValue
- sourceRef
- tenantId where tenant-specific
- productKey where product-specific
- status
  - candidate
  - active
  - review_required
  - rejected
  - superseded
  - archived
- confidenceScore
- firstSeenAt
- lastVerifiedAt
- notes
```

## External system key

```text
ExternalSystemKey
- externalSystemKey
- displayName
- systemType
  - public_api
  - vendor_feed
  - supplier_catalog
  - customer_system
  - accounting_system
  - wms
  - tms
  - cmms
  - legacy_import
  - csv_source
  - other
- ownerProductKey
- credentialOwnerProductKey
- status
```

External credentials are governed by integration and security constitutions. Platform Reference Data service should not become a general credential vault.

## Resolution request

A consuming product may ask Platform Reference Data service to resolve an identifier or alias.

```text
ReferenceResolutionRequest
- resolutionRequestId
- tenantId
- productKey
- requesterType
  - human
  - service
  - integration
  - import_job
  - mobile_scan
  - ai_suggestion
- localObjectType
- localObjectId
- inputType
  - identifier
  - alias
  - scanned_code
  - document_metadata
  - free_text
  - import_mapping
- inputValue
- contextFields
- requestedAt
- correlationId
```

## Resolution result

```text
ReferenceResolutionResult
- resolutionResultId
- resolutionRequestId
- matchStatus
  - exact_match
  - probable_match
  - multiple_candidates
  - no_match
  - invalid_input
  - review_required
- candidateRefs
- recommendedReferenceEntityRef
- confidenceScore
- explanation
- warningRefs
- allowedActions
  - accept
  - reject
  - create_candidate
  - send_to_review
  - request_more_information
- datasetVersionRef
- sourceSummary
```

## Candidate resolution

```text
ReferenceResolutionCandidate
- candidateId
- resolutionResultId
- referenceEntityId
- displayName
- matchedOn
  - exact_identifier
  - normalized_identifier
  - alias
  - fuzzy_name
  - source_crosswalk
  - taxonomy_context
  - document_metadata
  - tenant_overlay
- confidenceScore
- explanation
- conflictRefs
```

## Conflict

```text
ReferenceConflict
- conflictId
- conflictType
  - duplicate_identifier
  - conflicting_identifier
  - source_disagreement
  - taxonomy_disagreement
  - alias_collision
  - tenant_overlay_collision
  - stale_crosswalk
  - low_confidence_match
- affectedEntityRefs
- affectedCrosswalkRefs
- severity
  - low
  - medium
  - high
  - critical
- status
  - open
  - in_review
  - resolved
  - rejected
  - deferred
- ownerQueue
- resolution
- resolvedByPersonId
- resolvedAt
```

## Source priority

When sources disagree, Platform Reference Data service should use the configured source-priority policy.

Suggested default authority order:

```text
1. Platform-admin curated canonical source
2. Official public API connector
3. Direct product import from the owning system
4. Recognized vendor feed
5. Manual CSV import
6. Manual single-record entry
7. Historical migration seed
```

A reviewer may override source priority, but the override must be audited.

## Alias collision handling

Alias collision occurs when the same alias could refer to multiple entities.

Examples:

```text
- common abbreviation maps to two manufacturers
- vendor catalog short name maps to two products
- old brand name maps to successor and discontinued brand
```

Alias collision should return `multiple_candidates` or `review_required`.

It must not silently attach the first match.

## Stale reference behavior

A reference attachment becomes stale when:

```text
- the attached entity is merged
- the attached entity is split
- the dataset version is withdrawn
- the crosswalk is superseded
- the source is invalidated
- a product's stored reference snapshot no longer matches current canonical identity
```

Stale references should not necessarily break historical records, but they may block future operational actions if current identity is required.

## Product acceptance

Products should explicitly accept or reject suggested matches when the match affects operational truth.

Examples:

```text
- SupplyArr accepts a GTIN match for an internal item.
- LoadArr accepts a package conversion before posting receipt quantity.
- MaintainArr accepts a VIN decode before creating an asset.
- RecordArr accepts SDS metadata mapping before packaging evidence.
```

Platform Reference Data service records or returns the resolution evidence; the product records the operational decision.

## Merge review

```text
ReferenceMergeReview
- mergeReviewId
- candidateEntityRefs
- proposedSurvivorEntityRef
- proposedCanonicalKey
- affectedIdentifiers
- affectedAliases
- affectedCrosswalks
- affectedProductAttachments
- riskSummary
- status
  - draft
  - review
  - approved
  - rejected
  - applied
- reviewerPersonId
- decisionReason
- appliedAt
```

## Split review

```text
ReferenceSplitReview
- splitReviewId
- sourceEntityRef
- proposedNewEntityRefs
- affectedIdentifiers
- affectedAliases
- affectedCrosswalks
- affectedProductAttachments
- riskSummary
- status
  - draft
  - review
  - approved
  - rejected
  - applied
- reviewerPersonId
- decisionReason
- appliedAt
```

## Events

Recommended events:

```text
- platform.reference_data.crosswalk.created
- platform.reference_data.crosswalk.updated
- platform.reference_data.crosswalk.review_required
- platform.reference_data.alias.created
- platform.reference_data.reference_entity.merged
- platform.reference_data.reference_entity.split
- platform.reference_data.reference_entity.superseded
- platform.reference_data.resolution.review_required
- platform.reference_data.dataset_version.withdrawn
```

Consumers should use event payloads to decide whether to refresh local snapshots, create review tasks, or mark references stale.
