# STL Compliance Operational Readiness and Blocker Constitution

## 1. Purpose

This constitution defines shared behavior for readiness, blockers, warnings, overrides, and proceed/stop decisions across STL Compliance.

Many products need to answer some version of:

- Can this asset return to service?
- Can this person perform the task?
- Can this driver be dispatched?
- Can this order be fulfilled?
- Can this inventory be picked or issued?
- Can this supplier be used?
- Can this customer receive service?
- Can this quality hold be released?
- Can this evidence package close?

The suite must make those decisions consistent without creating a generic central owner for all blockers.

## 2. Scope

This constitution applies to:

- Readiness checks
- Product blockers
- Cross-product blockers
- Warnings
- Overrides
- Hold/release behavior
- Task and inbox surfacing
- Field Companion blocked states
- ReportArr readiness projections

## 3. Prime directive

The product that owns the underlying truth owns the blocker decision.

A shared blocker/readiness model may standardize display, audit, and handoff behavior, but it must not take ownership away from the source product.

## 4. Readiness

Readiness is a product-owned decision about whether an object can proceed.

Examples:

```text
MaintainArr owns asset readiness.
RoutArr owns dispatch/trip readiness.
TrainArr owns qualification readiness.
LoadArr owns inventory availability readiness.
SupplyArr owns supplier/procurement readiness.
CustomArr owns customer eligibility/readiness.
AssurArr owns quality release readiness.
OrdArr owns order orchestration readiness.
RecordArr owns evidence package completeness.
Compliance Core owns requirement/evidence evaluation results.
```

## 5. Blocker

A blocker prevents a workflow from proceeding.

```text
Blocker
- blockerId
- tenantId
- owningProduct
- sourceProduct
- sourceRecordRef
- affectedProduct
- affectedRecordRef
- blockerType
  - compliance
  - qualification
  - readiness
  - inventory
  - quality
  - customer_requirement
  - supplier_requirement
  - evidence_missing
  - approval_required
  - data_conflict
  - reference_resolution
  - external_dependency
  - integration_failure
  - safety
  - security
  - other
- severity
  - critical
  - high
  - medium
  - low
  - info
- blockingEffect
  - warning_only
  - blocks_current_step
  - blocks_dispatch
  - blocks_issue
  - blocks_receiving
  - blocks_closeout
  - blocks_release
  - blocks_order_completion
- title
- plainLanguageReason
- requiredClearingAction
- ownerQueue
- assignedPersonId
- overrideAllowed
- overridePolicyRef
- status
  - open
  - acknowledged
  - in_review
  - cleared
  - overridden
  - expired
  - canceled
- createdAt
- clearedAt
- overriddenAt
- correlationId
- auditTrailRef
```

## 6. Warning

A warning informs but does not block.

Warnings should use the same display pattern as blockers but must clearly show that progress is allowed.

Warnings may become blockers if severity, time, or status changes.

## 7. Override

An override allows progress despite a blocker.

Overrides must be rare, explicit, permission-gated, and audited.

```text
BlockerOverride
- overrideId
- blockerId
- tenantId
- approvedByPersonId
- permissionKeyUsed
- authorityContext
- reason
- riskAcknowledgement
- expiresAt
- evidenceRef
- createdAt
- auditTrailRef
```

Override must not delete the original blocker.

Override records should remain visible in audit packets and reports.

## 8. Shared readiness result

Products may expose a readiness endpoint.

```text
ReadinessResult
- tenantId
- productKey
- subjectType
- subjectRef
- readinessStatus
  - ready
  - ready_with_warnings
  - not_ready
  - unknown
  - review_required
- blockerRefs
- warningRefs
- checkedAt
- sourceSnapshot
- explanation
- recommendedActions
```

## 9. Cross-product blocker handoff

When Product A needs Product B to clear a blocker:

1. Product A creates or records the blocker against its workflow.
2. Product A creates a handoff to Product B.
3. Product B accepts, rejects, blocks, or completes the handoff.
4. Product B emits status events.
5. Product A updates its local blocker state based on accepted source truth.

Product A must not directly mutate Product B's source record.

## 10. UI rules

Blocked UI should show:

```text
- what is blocked
- why it is blocked
- who owns the clearing action
- what action clears it
- whether override is allowed
- what evidence is required
- link to owning product record
- last checked time
```

Blocked UI should not show raw event payloads, raw rule JSON, service-token claims, or database IDs as the primary explanation.

## 11. Field Companion rules

Field Companion should support:

```text
- blocked task cards
- warning badges
- offline capture of evidence to clear blockers
- clear "cannot proceed" messages
- retry after sync
- link back to owning product when online
```

Field Companion must not override blockers unless the owning product exposes an explicit permission-gated action.

## 12. Event recommendations

```text
{productKey}.readiness.checked
{productKey}.readiness.changed
{productKey}.blocker.created
{productKey}.blocker.acknowledged
{productKey}.blocker.cleared
{productKey}.blocker.overridden
{productKey}.blocker.expired
```

Event payload should include blocker IDs, source refs, affected refs, severity, blockingEffect, and correlation ID.

## 13. Reporting

ReportArr may report:

```text
- open blockers by product
- blocker age
- overridden blockers
- blockers by source rule
- blockers by customer/supplier/site
- readiness trend
- repeated blocker patterns
```

Corrections happen in the owning product.

## 14. Non-goals

This constitution does not create a new BlockerArr or ReadinessArr product.

It standardizes blocker behavior across existing product owners.
