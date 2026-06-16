# STL Compliance V2 Integration Contracts and Review Queues

## Purpose

This document defines implementation guidance for inbound integrations, outbound integrations, external mappings, retry behavior, and review queues.

The external systems integration constitution remains the governing platform rule.

## Prime directive

External systems remain external unless STL explicitly builds a replacement product.

STL may integrate, snapshot, map, review, and hand off.

STL must not silently become the system of record for finance, payroll, banking, tax, certified hardware, or specialized external systems.

## Integration definition

```text
IntegrationDefinition
- integrationId
- tenantId
- integrationKey
- displayName
- externalSystemKey
- owningProduct
- direction
  - inbound
  - outbound
  - bidirectional
  - read_only
  - writeback
- status
  - draft
  - active
  - paused
  - failing
  - disabled
  - archived
- credentialRef
- mappingPolicyRef
- retryPolicyRef
- reviewPolicyRef
- createdAt
- updatedAt
```

## External mapping

```text
ExternalMapping
- mappingId
- tenantId
- owningProduct
- stlEntityType
- stlEntityId
- externalSystemKey
- externalEntityType
- externalEntityId
- syncDirection
  - inbound
  - outbound
  - bidirectional
- mappingStatus
  - active
  - review_required
  - conflict
  - stale
  - rejected
  - archived
- lastVerifiedAt
- lastSyncedAt
- lastError
```

## Integration message envelope

```text
IntegrationMessage
- messageId
- tenantId
- integrationId
- direction
- externalSystemKey
- messageType
- sourceProduct
- targetProduct
- externalReference
- stlReference
- receivedAt
- processedAt
- status
  - received
  - validated
  - mapped
  - review_required
  - accepted
  - rejected
  - failed
  - retried
  - dead_letter
- idempotencyKey
- correlationId
- payloadSummary
- rawPayloadRef
```

Raw payloads should be available only to authorized technical/admin users.

Default UI should show plain-language summaries.

## Inbound integration flow

1. Receive webhook, file, connector response, or API message.
2. Authenticate and validate external source.
3. Store raw payload or payload hash as appropriate.
4. Build plain-language payload summary.
5. Resolve tenant and external mapping.
6. Validate target product ownership.
7. If mapping is clear and low-risk, submit to owning product API.
8. If mapping is unclear or risky, create review queue item.
9. Emit accepted/rejected/review-required event.
10. Preserve audit and retry state.

## Outbound integration flow

1. Owning product produces an outbound handoff or message.
2. Integration layer validates mapping and credential availability.
3. Message is transformed to external format.
4. Idempotency key is assigned.
5. Send is attempted.
6. Result is recorded.
7. External status snapshot is updated.
8. Failure creates retry or review item.

## Review queue item

```text
IntegrationReviewItem
- reviewItemId
- tenantId
- integrationId
- sourceProduct
- targetProduct
- sourceMessageRef
- reason
  - missing_mapping
  - multiple_matches
  - validation_failed
  - low_confidence
  - external_conflict
  - stale_reference
  - permission_required
  - dangerous_writeback
  - unsupported_payload
- severity
- status
  - open
  - in_review
  - accepted
  - rejected
  - escalated
  - expired
- proposedAction
- reviewerPersonId
- decisionReason
- createdAt
- resolvedAt
```

## Review queue routing

Suggested routing:

```text
Customer/order mapping issues -> CustomArr or OrdArr
Supplier/vendor/item mapping issues -> SupplyArr
Inventory/receipt movement issues -> LoadArr
Asset/maintenance issues -> MaintainArr
Training/qualification issues -> TrainArr
Compliance/evidence issues -> Compliance Core or RecordArr
Quality/hold issues -> AssurArr
Dispatch/trip issues -> RoutArr
Reference identity issues -> ReferenceDataCore
Platform identity/entitlement issues -> NexArr
```

## Idempotency

Every inbound and outbound material integration message should include or derive an idempotency key.

```text
Idempotency inputs may include:
- tenantId
- integrationId
- externalSystemKey
- externalEntityId
- externalEventId
- messageType
- source timestamp
- payload hash
```

The same message must not create duplicate orders, receipts, work orders, evidence packages, or payments.

## Retry policy

```text
RetryPolicy
- retryPolicyId
- maxAttempts
- firstRetryDelay
- backoffStrategy
  - fixed
  - linear
  - exponential
- deadLetterAfter
- manualRetryAllowed
- alertAfterFailures
```

Retry should not repeatedly submit dangerous writebacks without safeguards.

## Dead-letter record

```text
DeadLetterRecord
- deadLetterId
- tenantId
- integrationId
- sourceMessageRef
- failureReason
- lastErrorSummary
- attempts
- status
  - open
  - reviewed
  - retried
  - ignored
  - resolved
- ownerQueue
- createdAt
- resolvedAt
```

## Writeback risk rules

Writebacks require review or high confidence when they affect:

```text
- financial handoff packets
- customer commitments
- supplier status
- inventory balances
- stock ledger
- asset readiness
- dispatch state
- training/certificate status
- quality hold/release
- compliance evidence satisfaction
```

## Finance system handoff

External finance systems own:

```text
- invoices
- bills
- payments
- tax
- general ledger
- bank reconciliation
- accounting close
```

STL may produce:

```text
- invoice-ready packet
- bill-ready packet
- completion packet
- external status snapshot
- export artifact
- integration review item
```

## Portal and integration overlap

External portal submissions should use the same review and mapping concepts as integrations.

A vendor portal completion update is an external submission and may produce an integration-style review item if it affects dispatch or order closeout.

## Metrics

ReportArr may report:

```text
- integration success rate
- review queue age
- mapping confidence
- retry volume
- dead-letter count
- writeback failure rate
- external status freshness
```
