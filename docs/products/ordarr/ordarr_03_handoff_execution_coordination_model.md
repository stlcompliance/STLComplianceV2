# OrdArr - Handoff and Execution Coordination Model

OrdArr coordinates work through explicit handoffs. A handoff does not transfer ownership of the source order/request, and it does not make OrdArr the owner of target execution records.

## OrderHandoff

OrderHandoff is a request from OrdArr to a target product.

Fields:

- orderHandoffId
- orderRequestId
- orderLineIds
- tenantId
- sourceProductKey
- targetProductKey
- targetActionType
  - fulfill_inventory
  - receive_goods
  - dispatch_transport
  - perform_maintenance
  - procure_goods
  - review_quality
  - attach_document
  - evaluate_compliance
  - capture_field_evidence
- targetRecordType
- targetRecordRef
- requestedByPersonRef
- requestedAt
- neededBy
- priority
- status
  - requested
  - received
  - accepted
  - rejected
  - blocked
  - in_progress
  - waiting_on_source
  - waiting_on_target
  - completed
  - cancelled
  - expired
  - failed
- reason
- sourceSummary
- requiredNextAction
- statusMessage
- correlationId
- idempotencyKey
- lastUpdatedAt

## Target product patterns

LoadArr:

- Receives fulfillment, reservation, pick, issue, receiving, putaway, and inventory availability handoffs.
- Owns stock movements, inventory balances, receiving execution, and fulfillment status.

RoutArr:

- Receives transportation, dispatch, pickup, delivery, route, trip, and stop handoffs.
- Owns route/trip execution, proof, ETA/status, and transportation exceptions.

MaintainArr:

- Receives maintenance, inspection, repair, defect, and asset readiness handoffs.
- Owns asset and maintenance execution records.

SupplyArr:

- Receives procurement, sourcing, supplier quote, purchase request, and purchase intent handoffs.
- Owns supplier/vendor/item/procurement context and operational PO metadata.

RecordArr:

- Receives document storage, attachment, retention, and package assembly requests.
- Owns files, records, retention, access history, and controlled document lifecycle.

Compliance Core:

- Receives rule/evidence evaluation requests.
- Owns rule meaning, applicability, evidence requirements, exemptions, exceptions, and evaluation logic.

AssurArr:

- Receives nonconformance, quality hold/release, complaint, and CAPA coordination requests.
- Owns assurance case and quality release decision truth.

Field Companion:

- May surface mobile tasks and capture actions for source products.
- Does not own final operational records.

## Handoff acceptance

Target products may accept, reject, block, or request more context.

Required target responses:

- orderHandoffId
- targetProductKey
- targetRecordRef when created or accepted
- status
- statusMessage
- blockerRefs when blocked
- expectedCompletionAt when known
- correlationId

## Handoff events

- `ordarr.order_handoff.requested`
- `ordarr.order_handoff.received`
- `ordarr.order_handoff.accepted`
- `ordarr.order_handoff.rejected`
- `ordarr.order_handoff.blocked`
- `ordarr.order_handoff.in_progress`
- `ordarr.order_handoff.waiting_on_source`
- `ordarr.order_handoff.waiting_on_target`
- `ordarr.order_handoff.completed`
- `ordarr.order_handoff.cancelled`
- `ordarr.order_handoff.expired`
- `ordarr.order_handoff.failed`

## Coordination rules

1. Handoffs must be idempotent by tenant, order, target product, operation, and idempotency key.
2. Handoffs must preserve source and target product ownership.
3. Target products own their accepted work records.
4. OrdArr may show target progress and source/freshness metadata.
5. OrdArr must not infer target completion from missing events; it should reconcile through target APIs.
6. Rejections and blockers must be plain-language and source-attributed.

## Read models

OrdArr may own order coordination read models:

- Order orchestration board
- Order handoff status board
- Request triage queue
- Customer request timeline
- Completion readiness projection

These read models must show source products and freshness. They do not replace source product records.
