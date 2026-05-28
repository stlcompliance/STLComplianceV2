# Worker 127 — RoutArr dispatch notification hooks (M12)

## Scope

Vertical slice mirroring TrainArr W123 / MaintainArr W125:

- `routarr_tenant_notification_settings` — tenant webhook URL and dispatch event toggles
- `routarr_notification_dispatches` — outbox with dispatch status and webhook audit fields
- User APIs: `GET/PUT /api/notification-settings`, `GET /api/notification-settings/dispatches`
- Internal APIs: `GET /api/internal/dispatch-notifications/pending`, `POST .../process-batch` (service token `routarr.notifications.dispatch`)
- Hooks: `TripService` driver assign (status → `assigned`) and `UpdateDispatchStatusAsync` when dispatch status changes (bulk/closeout paths included)
- `shared-worker` `RoutArrNotificationDispatchJob` + `StlIntegrationTokenCatalog` profile `worker-routarr-notifications`
- `routarr-frontend` `NotificationSettingsPanel` (tenant admin / routarr admin)
- Tests: `RoutArrNotificationTests`, `DispatchNotificationRulesTests`

## Event kinds

| Kind | Trigger |
|------|---------|
| `trip_assigned` | Driver assigned; trip dispatch status becomes `assigned` |
| `trip_dispatched` | Dispatch status → `dispatched` |
| `trip_in_progress` | Dispatch status → `in_progress` |
| `trip_completed` | Dispatch status → `completed` |
| `trip_cancelled` | Dispatch status → `cancelled` |

## Webhook payloads

- `routarr.trip.assigned` — `tenantId`, `tripId`, `driverPersonId`, `dispatchStatus`
- `routarr.trip.dispatched` — `tenantId`, `tripId`, `driverPersonId`, `dispatchStatus`
- `routarr.trip.in_progress` — `tenantId`, `tripId`, `driverPersonId`, `dispatchStatus`
- `routarr.trip.completed` — `tenantId`, `tripId`, `driverPersonId`, `dispatchStatus`
- `routarr.trip.cancelled` — `tenantId`, `tripId`, `driverPersonId`, `dispatchStatus`

## Configuration

- API: standard JWT for settings; `shared-worker` bearer for internal batch
- Worker: `RoutArrNotificationDispatch__RoutArrBaseUrl`, `RoutArrNotificationDispatch__ServiceToken`
- Render: env on `shared-worker` service in `render.yaml`
