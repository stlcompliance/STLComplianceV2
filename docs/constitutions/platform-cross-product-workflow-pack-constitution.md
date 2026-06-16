# STL Compliance Cross-Product Workflow Pack Constitution

## 1. Purpose

This constitution defines how to write, implement, and test cross-product workflow packs.

A workflow pack describes how real work moves across products while preserving source-of-truth ownership.

## 2. Scope

This constitution applies to:

- Order-to-fulfillment workflows
- Procure-to-receive workflows
- Defect-to-work-order workflows
- Incident-to-retraining workflows
- Quality hold/release workflows
- Vendor/customer portal workflows
- Integration-triggered workflows
- Field Companion execution flows
- Any workflow touching more than one product

## 3. Prime directive

A cross-product workflow coordinates owners.

It does not erase ownership boundaries.

A workflow pack may define events, handoffs, blockers, tasks, read models, and evidence packages, but the owning product remains the source of truth for its business record.

## 4. Required workflow pack sections

Every workflow pack must include:

```text
- Purpose
- Trigger
- Participating products
- Source-of-truth table
- Preconditions
- Main flow
- Alternate flows
- Blockers
- Required events
- Required handoffs
- Required tasks
- Required APIs
- Evidence and audit
- Field Companion behavior
- External portal behavior if applicable
- Reporting/read-model effects
- Failure and retry behavior
- Closeout behavior
- Non-goals
```

## 5. Source-of-truth table

A workflow pack must list each business truth and its owner.

Example:

```text
Business truth | Owner
Customer identity | CustomArr
Order lifecycle | OrdArr
Inventory balance | LoadArr
Supplier commercial context | SupplyArr
Stored evidence files | RecordArr
Regulatory meaning | Compliance Core
```

## 6. Trigger rules

A trigger starts the workflow.

Valid trigger types:

```text
- user_action
- product_event
- scheduled_job
- integration_event
- mobile_action
- external_portal_submission
- import_review_decision
- compliance_evaluation
```

A trigger must not directly mutate another product's source record without an approved API or handoff.

## 7. Event vs handoff

An event is a fact that already happened.

A handoff is a request for another product to review, accept, reject, block, or complete work.

Workflow packs must not use events as hidden commands.

## 8. Handoff record requirements

A handoff should include:

```text
- handoffId
- tenantId
- sourceProduct
- sourceRecordRef
- targetProduct
- targetWorkflowKey
- requestedAction
- status
  - requested
  - accepted
  - rejected
  - blocked
  - in_progress
  - completed
  - canceled
  - expired
- reason
- priority
- dueAt
- blockerRefs
- evidenceRefs
- correlationId
- auditTrailRef
```

The target product owns whether the request is accepted and how the target work is performed.

## 9. Blocker rules

A blocker must identify the owner of the blocker and the action needed to clear it.

A workflow pack may describe blockers, but it must not create a central blocker owner unless a product already owns the underlying truth.

## 10. Task rules

Workflow-level tasks must point to product-owned work.

An inbox or Field Companion surface may aggregate tasks, but the task's source product remains the owner.

## 11. Field Companion rules

Field Companion may:

```text
- show assigned tasks
- capture photos/signatures/answers
- collect offline evidence
- submit status updates through owning product APIs
- show context snapshots
```

Field Companion must not become the source of truth for the underlying record.

## 12. External portal rules

External portal actors may submit updates, evidence, or approvals only through limited scoped access.

External portal submissions must create reviewable product-owned records or updates.

They must not bypass tenant identity, authority, audit, or validation rules.

## 13. Reporting and read-model rules

ReportArr may project the full workflow state.

ReportArr must not correct source data.

Read models must include source product and source record references so users can drill into the owning product.

## 14. Closeout rules

A cross-product workflow may close only when all required product-owned closeout states are met.

Closeout should preserve:

```text
- source records
- completion events
- evidence package references
- unresolved warnings
- overrides
- external handoff status
- audit trail
```

## 15. Testing requirements

Every implemented workflow pack should have tests for:

```text
- happy path
- missing prerequisite
- blocked handoff
- rejected handoff
- duplicate event
- idempotent retry
- stale reference
- permission denied
- external portal expired link
- offline/mobile replay where applicable
- evidence package generation
```
