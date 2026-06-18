# STL Compliance Events, Handoffs, and Read Models Constitution

## 1. Purpose

This constitution defines how STL Compliance products publish facts, request work, coordinate cross-product workflows, and build read models without breaking ownership boundaries.

Events, handoffs, and read models allow the suite to act coordinated while preserving product ownership.

## 2. Scope

This constitution applies to:

- Product domain events
- Integration events
- Outbox/inbox processing
- Handoff records
- Cross-product workflow requests
- Read models
- Mirrors
- Dashboard projections
- Reporting projections
- Event replay
- Dead-letter and review queues

## 3. Core definitions

### Event

An event is a fact that already happened.

Examples:

- `work_order.created`
- `asset.readiness_changed`
- `training_assignment.completed`
- `inventory_movement.posted`
- `route.dispatched`
- `evidence.uploaded`
- `capa.opened`

An event does not command another product to mutate blindly.

### Handoff

A handoff is an explicit request for another product to review, accept, reject, block, or complete work.

Examples:

- MaintainArr requests parts fulfillment from LoadArr.
- StaffArr forwards incident context to TrainArr for retraining evaluation.
- OrdArr requests fulfillment from LoadArr.
- AssurArr requests corrective repair from MaintainArr.
- RoutArr notifies LoadArr of an inbound dock appointment.
- RoutArr contributes transportation freight facts to an OrdArr invoice-ready packet, SupplyArr bill-ready packet, or LedgArr financial packet while leaving financial records in LedgArr.

### Read model

A read model is a purpose-built projection used for dashboards, lists, queues, reports, or cross-product display.

A read model is not automatically the source of truth.

### Mirror

A mirror is a local read-only copy or projection of selected source fields from another product.

A mirror exists for performance, availability, filtering, or reporting convenience. It must not become a competing owner.

## 4. Prime directive

Events and read models may inform decisions, but source-of-truth corrections happen in the owning product.

A product must not repair another product's source record through event consumption unless an approved API/handoff explicitly grants that action.

## 5. Event envelope

Every cross-product event must include:

- Event ID
- Event type
- Event schema version
- Tenant ID
- Source product
- Source resource type
- Source resource ID
- Occurred time
- Emitted time
- Actor type: `human`, `service`, `integration`, `system`
- Actor ID, using `personId` when human
- Correlation ID
- Causation ID where applicable
- Idempotency key where applicable
- Payload

Recommended shape:

```json
{
  "eventId": "...",
  "eventType": "maintainarr.work_order.created",
  "schemaVersion": "1.0",
  "tenantId": "...",
  "sourceProduct": "MaintainArr",
  "source": {
    "resourceType": "work_order",
    "resourceId": "..."
  },
  "occurredAt": "2026-06-10T00:00:00Z",
  "emittedAt": "2026-06-10T00:00:01Z",
  "actor": {
    "type": "human",
    "personId": "..."
  },
  "correlationId": "...",
  "causationId": "...",
  "payload": {}
}
```

## 6. Event naming

Event names should be past tense facts.

Good:

- `asset.created`
- `route.dispatched`
- `certification.expired`
- `inventory_hold.released`
- `record.superseded`

Bad:

- `create_asset`
- `dispatch_route_now`
- `make_driver_ready`
- `fix_inventory`

Commands may exist internally, but cross-product event streams should publish facts.

## 7. Event payload rules

Payloads should include enough context for consumers to decide whether to fetch more detail.

Payloads should not dump entire records unless explicitly intended for a projection.

Payloads must not include secrets, raw service-token claims, unrestricted PII, or sensitive notes unless the event channel is explicitly authorized for that data.

Cross-product events should prefer stable IDs and summary fields.

## 8. Outbox rule

Products that publish material events should use an outbox pattern or equivalent reliability mechanism.

A state change and its event publication must not drift silently.

If event publication fails, the event must remain retryable or visible for operations/admin review.

## 9. Idempotent consumers

Consumers must treat events as at-least-once delivery unless the infrastructure proves otherwise.

Every event handler must be idempotent by event ID and tenant.

Reprocessing the same event must not duplicate:

- Tasks
- Notifications
- Inventory movements
- Handoffs
- Records
- Report snapshots
- External writebacks
- Approvals

## 10. Ordering and conflict rules

Consumers must not assume perfect global ordering across products.

When ordering matters, use:

- Source product sequence number
- Source resource version
- Occurred time
- Emitted time
- Last processed event ID
- Product-owned validation fetch

If a read model receives an older event after a newer event, it must preserve the newest known state or mark the projection for reconciliation.

## 11. Handoff lifecycle

All cross-product handoffs must use explicit states.

Recommended states:

- `requested`
- `received`
- `accepted`
- `rejected`
- `blocked`
- `in_progress`
- `waiting_on_source`
- `waiting_on_target`
- `completed`
- `cancelled`
- `expired`
- `failed`

Handoff state must not be inferred only from free-text notes or generic activity entries.

## 12. Handoff record fields

A handoff should include:

- Handoff ID
- Tenant ID
- Source product
- Source record type
- Source record ID
- Target product
- Target action type
- Target record type and ID when created/accepted
- Requested by actor or service
- Requested at
- Current state
- Priority/severity
- Due/needed by time where applicable
- Reason
- Required next action
- Source summary
- Related references
- Correlation ID
- Idempotency key
- Last updated time

## 13. Handoff authority

A handoff does not transfer ownership of the source record.

The target product owns its accepted work and target record.

Examples:

- MaintainArr owns the work order that created parts demand.
- LoadArr owns the stock reservation, pick, issue, and inventory movement.
- AssurArr owns the CAPA case.
- MaintainArr owns the corrective repair work order.
- StaffArr owns the incident/personnel history impact.
- TrainArr owns retraining assignment and completion.

## 14. Handoff acceptance

A target product may accept, reject, or block a handoff based on its own rules.

The source product may show target handoff state but must not force target acceptance.

A target product must explain rejection/blocking in plain language.

## 15. Read model ownership

The product that owns a read model owns that projection's schema, refresh logic, and display contract.

The read model owner does not automatically own the underlying source truth.

Examples:

- ReportArr may own a cross-product KPI read model.
- RoutArr may own a dispatch release readiness projection for dispatch screens.
- RoutArr may own a transportation control tower projection from external visibility events, provided source/freshness is shown.
- LoadArr may own an inbound dock readiness board that includes RoutArr appointment context.
- Compliance Core may own an evidence gap projection based on RecordArr documents and product events.

## 16. Read model metadata

Read models must expose source and freshness.

Recommended fields:

- Projection ID
- Tenant ID
- Projection owner
- Source products
- Source record references
- Last refreshed time
- Last source event time
- Last processed event ID or cursor
- Freshness state
- Staleness reason where applicable
- Confidence where applicable

## 17. Freshness states

Recommended freshness states:

- `live`
- `near_live`
- `cached`
- `stale`
- `partial`
- `source_unavailable`
- `rebuilding`
- `unknown`

UI must not show stale projections as live truth.

## 18. Reconciliation

Read models and mirrors must have a way to reconcile with source products.

Reconciliation may be:

- Scheduled rebuild
- Event replay
- On-demand refresh
- Source API revalidation
- Admin repair action
- Dead-letter retry

A projection with known gaps must be marked degraded or stale.

## 19. Dead-letter and review queues

Failed event processing must not disappear.

Failures should preserve:

- Event ID
- Tenant ID
- Source product
- Consumer product
- Handler name
- Error code
- Last error
- Retry count
- First failed at
- Last failed at
- Next retry at
- Manual review status

Business failures and technical failures should be distinguishable.

## 20. Dashboard and reporting use

Dashboards and ReportArr may consume events, handoffs, mirrors, and read models.

They must show source and freshness when data affects operational or compliance decisions.

Dashboard and report projections must not mutate source records.

## 21. Event replay

Events should be replayable where practical.

Consumers must handle replay without duplicating business effects.

Replay must be permission/system controlled, tenant-scoped, and auditable.

## 22. Anti-patterns

The following are not allowed:

- Treating events as guaranteed commands
- Updating source records in another product from an event handler without an approved API or handoff
- Building dashboards from undocumented event payload guesses
- Ignoring event schema versions
- Dropping failed events without visibility
- Using handoffs as vague notes instead of stateful records
- Hiding stale read models
- Showing mirrored data as live source truth
- Using external events to bypass product ownership

## 23. Minimum acceptable implementation

A cross-product event/handoff/read-model flow is minimally acceptable when it has:

1. Clear source product
2. Clear target/consumer product
3. Tenant-scoped event envelope
4. Schema version
5. Idempotent processing
6. Explicit handoff state when work is requested
7. Source/freshness metadata on projections
8. Dead-letter or review path
9. Plain-language failure state
10. No ownership ambiguity
