# W187 — SupplyArr M8 integration event outbox/inbox

## Scope

Durable tenant-scoped async integration for SupplyArr-owned domain mutations and inbound cross-product events (mirror pattern, no cross-DB FKs).

## Persistence

| Table | Purpose |
|-------|---------|
| `supplyarr_tenant_integration_event_settings` | Per-tenant enable, max attempts, retry interval |
| `supplyarr_integration_outbox_events` | Outbound domain events (idempotency key, payload JSON, status, correlation) |
| `supplyarr_integration_inbox_events` | Inbound events from source products (typed handlers) |
| `supplyarr_integration_event_processing_runs` | Batch processing audit |

Migration: `20260528114808_SupplyArrIntegrationEventOutboxInbox`.

## APIs

### Tenant admin (JWT)

- `GET/PUT /api/integration-event-settings`
- `GET /api/integration-event-settings/outbox?limit=`
- `GET /api/integration-event-settings/inbox?limit=`
- `POST /api/integration-event-settings/outbox/{id}/abandon`
- `POST /api/integration-event-settings/inbox/{id}/abandon`

Auth: `RequireIntegrationEventSettingsManage` (tenant_admin / supplyarr_admin / supplyarr_manager).

### Internal (service token)

- `GET /api/internal/integration-events/pending`
- `POST /api/internal/integration-events/process-batch` — scope `supplyarr.integration.events.process`, source `shared-worker`
- `POST /api/internal/integration-events/inbox/enqueue` — MaintainArr `supplyarr.demand_intake.write` or shared-worker `supplyarr.integration.inbox.enqueue`

## Outbox publish hooks (minimum)

- `party.created`, `part.created`
- `purchase_request.submitted`, `purchase_request.approved`
- `purchase_order.issued`, `receiving_receipt.posted`
- `maintainarr.demand.received`

Outbox processor fans procurement kinds into `ProcurementNotificationEnqueueService` (idempotent with direct enqueue).

## Inbox handler (minimum)

- `maintainarr.demand.ingest` → deserialize `IngestMaintainarrDemandRequest` → `MaintainArrDemandIntakeService.IngestAsync`

Direct `/api/integrations/maintainarr-demand` remains for synchronous intake.

## Worker

- `SupplyArrIntegrationEventsJob` + `SupplyArrIntegrationEventsClient` in `workers/shared-worker`
- Config section `SupplyArrIntegrationEvents` (`ServiceToken`, `SupplyArrBaseUrl`, `BatchSize`, `ScanIntervalMinutes`)
- Token profile `worker-supplyarr-integration-events` in `StlIntegrationTokenCatalog`

## UI

- `IntegrationEventSettingsPanel` on Settings workspace (manage notifications permission)

## Tests

- `SupplyArrIntegrationEventTests` — auth, outbox idempotency, inbox ingest + process batch
- `IntegrationEventSettingsPanel.test.tsx`

## Next slice

Per `02_PRODUCT_IMPLEMENTATION_BACKLOG.md` M8: **RFQs** and **quote comparison** (or **supplier onboarding** workflow depth). Cross-milestone: **M10** RoutArr/TrainArr demand intake mirrors.
