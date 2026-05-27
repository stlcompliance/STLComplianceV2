# Events, Workers, and Integration

## Integration Model

Products integrate through:

- APIs for synchronous checks and commands
- Events for lifecycle facts
- Service tokens for backend-to-backend access
- Local references for display and performance
- Product workers for async processing

## Event Envelope

```json
{
  "eventId": "uuid",
  "eventType": "string",
  "sourceProduct": "string",
  "tenantId": "uuid",
  "occurredAt": "datetime",
  "version": 1,
  "payload": {},
  "correlationId": "uuid",
  "causationId": "uuid"
}
```

## Outbox Rules

- Each product owns its own outbox table.
- Product worker processes its own outbox.
- Event handlers are idempotent.
- Failed events retry with backoff.
- Poison events are marked for review.
- Event payloads use stable contracts.

## Worker Jobs

| Worker | Jobs |
|---|---|
| nexarr-worker | token cleanup, entitlement reconciliation, licensing checks, service token cleanup, audit rollups |
| staffarr-worker | certification expiration, permission projection, readiness, history rollups, audit packages |
| trainarr-worker | due training, reminders, escalation, completion validation, qualification publication |
| maintainarr-worker | PM due-state, WO generation, inspection generation, defect escalation, asset status rollups |
| routarr-worker | route state, trip closeout, eligibility snapshots, DVIR follow-up, reference sync |
| supplyarr-worker | reorder evaluation, price snapshots, lead-time snapshots, procurement reminders, demand processing |
| compliancecore-worker | vocabulary maintenance, key normalization, mapping validation, rule publication, SDS/HazCom reference work |

## Example Events

- nexarr.entitlement.changed
- staffarr.person.created
- staffarr.certification.granted
- trainarr.training.completed
- trainarr.qualification.issued
- maintainarr.asset.readinessChanged
- maintainarr.workOrder.completed
- routarr.trip.completed
- routarr.dvir.failed
- supplyarr.purchaseRequest.created
- supplyarr.purchaseOrder.approved
- compliancecore.rulePack.published
- compliancecore.vocabulary.changed

## API vs Event

Use API calls for current decisions: readiness, authorization, dispatchability, publication commands, and user actions.
Use events for lifecycle facts, projections, reports, retryable processing, and audit history.
