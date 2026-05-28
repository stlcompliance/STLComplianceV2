# TrainArr notification settings foundations

## Slice name

M12 TrainArr tenant notification preferences + shared-worker dispatch for assignment / qualification lifecycle webhooks

## Products touched

- **TrainArr API** — settings persistence, dispatch outbox, user + internal APIs, enqueue hooks
- **shared-worker** — `TrainArrNotificationDispatchJob` scheduled batch
- **trainarr-frontend** — `NotificationSettingsPanel` on home workspace
- **STLCompliance.Shared** — integration token profile `worker-trainarr-notifications`

## Schema

| Table | Purpose |
|-------|---------|
| `trainarr_tenant_training_notification_settings` | Per-tenant webhook URL, enable flags, expiring lead days |
| `trainarr_training_notification_dispatches` | Pending/sent/failed/skipped dispatch audit rows |

## API + auth

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/api/notification-settings` | `tenant_admin` / `trainarr_admin` | Read tenant notification settings |
| PUT | `/api/notification-settings` | same | Upsert settings (HTTPS webhook validation) |
| GET | `/api/notification-settings/dispatches` | same | Recent dispatch audit rows |
| GET | `/api/internal/training-notifications/pending` | service token `trainarr.notifications.dispatch` | List pending dispatches |
| POST | `/api/internal/training-notifications/process-batch` | same | Enqueue expiring qualifications + POST webhooks |

## Event kinds

- `assignment_created` — enqueued when a training assignment is created (if enabled)
- `qualification_expiring` — enqueued by dispatch worker scan within `ExpiringLeadDays`
- `qualification_expired` — enqueued when qualification expire lifecycle completes (worker or API)

## Worker

`TrainArrNotificationDispatch` in `shared-worker` calls `POST /api/internal/training-notifications/process-batch` on a configurable interval (default 15 minutes).

Render env: `TrainArrNotificationDispatch__TrainArrBaseUrl` (token auto-provisioned via `worker-trainarr-notifications` profile).

## Tests

- `TrainArrTrainingNotificationTests` — assignment enqueue, webhook dispatch, auth + validation
- `TrainingNotificationRulesTests` — webhook URL + list limit normalization

## Operator notes

1. TrainArr admin saves webhook URL and event toggles in **Training notifications** panel.
2. Ensure `shared-worker` has `TrainArrNotificationDispatch__ServiceToken` (auto-provision on deploy when enabled).
3. Dispatches appear in **Recent dispatches** after worker runs or manual internal batch in staging.
