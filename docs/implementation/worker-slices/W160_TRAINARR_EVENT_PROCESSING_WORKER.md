# Worker 160 — TrainArr event processing worker (M12)

## Slice name

M12 training domain event outbox + shared-worker processing into per-person training history

## Products touched

- **TrainArr API** — settings, domain event outbox, person training history, user + internal APIs, enqueue hooks
- **shared-worker** — `TrainArrEventProcessingJob`, client, options
- **trainarr-frontend** — `EventProcessingSettingsPanel`, `PersonTrainingHistoryPanel`
- **STLCompliance.Shared** — integration token profile `worker-trainarr-event-processing`

## Schema

| Table | Purpose |
|-------|---------|
| `trainarr_tenant_event_processing_settings` | Per-tenant enable, max attempts, retry interval |
| `trainarr_training_domain_events` | Pending/processed/abandoned domain event outbox |
| `trainarr_person_training_history_entries` | Materialized per-person training timeline |

## API + auth

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/api/event-processing-settings` | trainarr admin | Read tenant event processing settings |
| PUT | `/api/event-processing-settings` | same | Upsert settings |
| GET | `/api/event-processing-settings/events` | same | Recent domain event audit rows |
| GET | `/api/person-training-history?staffarrPersonId=` | admin/trainer/self | Materialized person training history |
| GET | `/api/people/{staffarrPersonId}/training-history` | same | Nested person route |
| GET | `/api/internal/training-events/pending` | service token `trainarr.events.process` | List pending events |
| POST | `/api/internal/training-events/process-batch` | same | Process pending events into history |

## Event kinds

- `assignment_created` — enqueued on training assignment create (including recertification worker path)
- `assignment_completed` — enqueued on assignment completion
- `qualification_issued` — enqueued when qualification issue is created
- `qualification_suspended` / `qualification_revoked` / `qualification_expired` — enqueued on lifecycle actions

## Worker

`TrainArrEventProcessingJob` calls `POST /api/internal/training-events/process-batch` on a configurable interval (default 5 minutes).

Render env: `TrainArrEventProcessing__TrainArrBaseUrl` (token auto-provisioned via `worker-trainarr-event-processing` profile).

## Tests

- `StaffArrTrainArrEventProcessingWorkerTests` — service token auth, process batch, person history read
- `EventProcessingRulesTests` — batch size + idempotency key normalization
- `EventProcessingSettingsPanel.test.tsx` — settings panel smoke

## Remaining gaps

- StaffArr integration read of TrainArr person training history not wired
- No team-level or export packaging for training history (separate M12 training audit package slice)
- Event processing does not yet fan out to external webhooks (notification dispatch remains separate)

## Next recommended slice

Next open M12 TrainArr backlog row: notification dispatch worker enhancements, rule-pack impact worker, evidence retention worker, or person training history UX expansion per `02_PRODUCT_IMPLEMENTATION_BACKLOG.md`.
