# Worker 161 — TrainArr notification dispatch worker enhancements (M12)

## Slice name

M12 TrainArr notification dispatch retry + expanded lifecycle webhooks + domain-event fan-out

## Products touched

- **TrainArr API** — settings/dispatch schema, retry semantics, event fan-out, payloads
- **shared-worker** — existing `TrainArrNotificationDispatchJob` (unchanged schedule)
- **trainarr-frontend** — `NotificationSettingsPanel` expanded toggles + retry fields
- **STLCompliance.Shared** — unchanged `worker-trainarr-notifications` profile

## Schema

| Table / column | Purpose |
|----------------|---------|
| `trainarr_tenant_training_notification_settings` | Added notify toggles for assignment completed + qualification issued/suspended/revoked; `MaxAttempts`, `RetryIntervalMinutes` |
| `trainarr_training_notification_dispatches` | Added `AttemptCount`, `NextRetryAt`, `UpdatedAt`; status `abandoned` after max attempts |

## API + auth

Existing routes unchanged; request/response bodies extended:

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET/PUT | `/api/notification-settings` | trainarr admin | Settings include new toggles + retry policy |
| GET | `/api/notification-settings/dispatches` | same | Dispatch rows include attempt/retry fields |
| GET/POST | `/api/internal/training-notifications/*` | `trainarr.notifications.dispatch` | Batch processes due retries; response includes `retriedCount`, `abandonedCount` |

## Event kinds

- Existing: `assignment_created`, `qualification_expiring`, `qualification_expired`
- New: `assignment_completed`, `qualification_issued`, `qualification_suspended`, `qualification_revoked`
- Fan-out: after successful domain-event processing, map domain event kind → notification enqueue (idempotent with source hooks)

## Worker

`TrainArrNotificationDispatchJob` continues `POST /api/internal/training-notifications/process-batch` (default 15 min). Failed webhooks stay `pending` with `NextRetryAt` until sent or `abandoned`.

## Tests

- `TrainArrTrainingNotificationTests` — assignment webhook, invalid URL, retry-after-failure
- `TrainingNotificationRulesTests` — domain-event mapping, max attempts normalization
- `StaffArrTrainArrEventProcessingWorkerTests` — fan-out enqueue after event processing

## Remaining gaps

- No dedicated notification replay/admin API beyond worker batch
- Qualification expiring scan still worker-driven (not event-sourced)
- StaffArr integration read of TrainArr person training history (separate slice)

## Next recommended slice

M12 TrainArr rule-pack impact worker or evidence retention worker per `02_PRODUCT_IMPLEMENTATION_BACKLOG.md`.
