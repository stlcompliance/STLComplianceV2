# Worker 131 — Companion operational notification hooks (M12)

## Scope

Vertical slice on NexArr companion API path and `companion-frontend`:

- `nexarr_companion_notification_settings` + `nexarr_companion_notification_dispatches`
- Tenant admin APIs: `GET/PUT /api/companion/notification-settings`, `GET .../dispatches`
- Internal APIs: `GET/POST /api/internal/companion-notifications/*` (scope `nexarr.companion.notifications.dispatch`)
- `shared-worker` `NexArrCompanionNotificationDispatchJob` + profile `worker-nexarr-companion-notifications`
- Lifecycle hooks: handoff redeem (`CompanionAuthService`), field inbox refresh (`CompanionFieldInboxService`)
- `NotificationSettingsPanel` on companion home for tenant admins
- Tests: `NexArrCompanionNotificationTests`, `CompanionNotificationRulesTests`

## Event kinds

| Kind | Trigger |
|------|---------|
| `handoff_redeemed` | Successful `POST /api/companion/auth/handoff/redeem` |
| `field_inbox_refreshed` | Successful `GET /api/companion/field-inbox` |

## Configuration

- Worker: `NexArrCompanionNotificationDispatch__NexArrBaseUrl`, `NexArrCompanionNotificationDispatch__ServiceToken`
- Render: env on `shared-worker` in `render.yaml`
