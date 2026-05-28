# Worker 129 — SupplyArr notification settings (M12)

## Scope

Vertical slice mirroring TrainArr W123 / MaintainArr W125 / RoutArr W127:

- `supplyarr_tenant_notification_settings` — tenant webhook URL and procurement event toggles
- `supplyarr_notification_dispatches` — outbox with dispatch status and webhook audit fields
- User APIs: `GET/PUT /api/notification-settings`, `GET /api/notification-settings/dispatches`
- Internal APIs: `GET /api/internal/procurement-notifications/pending`, `POST .../process-batch` (service token `supplyarr.notifications.dispatch`)
- Hooks: `PurchaseRequestService` submit/approve; `PurchaseOrderService` issue; `ReceivingService` post
- `shared-worker` `SupplyArrNotificationDispatchJob` + `StlIntegrationTokenCatalog` profile `worker-supplyarr-notifications`
- `supplyarr-frontend` `NotificationSettingsPanel` (tenant admin / supplyarr admin)
- Tests: `SupplyArrNotificationTests`, `ProcurementNotificationRulesTests`

## Event kinds

| Kind | Trigger |
|------|---------|
| `purchase_request_submitted` | PR submitted for approval |
| `purchase_request_approved` | PR approved |
| `purchase_order_issued` | PO issued to vendor |
| `receiving_receipt_posted` | Receiving receipt posted to inventory |

## Webhook payloads

- `supplyarr.purchase_request.submitted` — `tenantId`, `vendorPartyId`, `purchaseRequestId`
- `supplyarr.purchase_request.approved` — `tenantId`, `vendorPartyId`, `purchaseRequestId`
- `supplyarr.purchase_order.issued` — `tenantId`, `vendorPartyId`, `purchaseOrderId`
- `supplyarr.receiving_receipt.posted` — `tenantId`, `vendorPartyId`, `receivingReceiptId`

## Configuration

- API: standard JWT for settings; `shared-worker` bearer for internal batch
- Worker: `SupplyArrNotificationDispatch__SupplyArrBaseUrl`, `SupplyArrNotificationDispatch__ServiceToken`
- Render: env on `shared-worker` service in `render.yaml`
