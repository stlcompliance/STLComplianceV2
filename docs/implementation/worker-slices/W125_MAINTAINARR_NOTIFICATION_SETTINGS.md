# Worker 125 — MaintainArr notification settings foundations (M12)

## Scope

Vertical slice mirroring TrainArr W123 / StaffArr W121:

- `maintainarr_tenant_notification_settings` — tenant webhook URL and event toggles
- `maintainarr_notification_dispatches` — outbox with dispatch status and webhook audit fields
- User APIs: `GET/PUT /api/notification-settings`, `GET /api/notification-settings/dispatches`
- Internal APIs: `GET /api/internal/maintenance-notifications/pending`, `POST .../process-batch` (service token `maintainarr.notifications.dispatch`)
- Hooks: work order create (manual, defect, PM-generated); PM due scan transitions to due/overdue; batch enqueue for schedules already due/overdue
- `shared-worker` `MaintainArrNotificationDispatchJob` + `StlIntegrationTokenCatalog` profile `worker-maintainarr-notifications`
- `maintainarr-frontend` `NotificationSettingsPanel` (tenant admin / maintainarr admin)
- Tests: `MaintainArrNotificationTests`, `MaintenanceNotificationRulesTests`

## Event kinds

| Kind | Trigger |
|------|---------|
| `work_order_created` | `WorkOrderService` create paths |
| `pm_schedule_due` | PM due scan marks schedule due; worker batch backfill |
| `pm_schedule_overdue` | PM due scan marks schedule overdue; worker batch backfill |

## Webhook payloads

- `maintainarr.work_order.created` — `tenantId`, `assetId`, `workOrderId`
- `maintainarr.pm_schedule.due` — `tenantId`, `assetId`, `pmScheduleId`
- `maintainarr.pm_schedule.overdue` — `tenantId`, `assetId`, `pmScheduleId`

## Configuration

- API: standard JWT for settings; `shared-worker` bearer for internal batch
- Worker: `MaintainArrNotificationDispatch__MaintainArrBaseUrl`, `MaintainArrNotificationDispatch__ServiceToken`
- Render: env on `shared-worker` service in `render.yaml`
