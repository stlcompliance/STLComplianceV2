# Worker 176 — RoutArr trip completion rollup worker (M12)

**Products:** RoutArr, shared-worker, routarr-frontend  
**Milestone:** M12  
**Backlog:** RoutArr `[M12] trip completion rollup worker`

## Summary

Scheduled worker materializes trip and route completion summaries for completed or cancelled trips, including stop/load counts, duration metrics, and milestone events. Tenant admins configure enablement and staleness; dispatch users read materialized-first completion APIs.

## Backend (RoutArr)

### Schema

Migration: `RoutArrTripCompletionRollupWorker`

- `routarr_tenant_trip_completion_rollup_settings` — tenant worker policy
- `routarr_trip_completion_rollups` — materialized completion summary per terminal trip
- `routarr_trip_completion_events` — milestone events (trip/route/stop lifecycle)
- `routarr_trip_completion_rollup_runs` — batch run audit

### Tenant admin APIs (JWT + RoutArr admin)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/trip-completion-rollup-settings` | Read worker settings |
| PUT | `/api/trip-completion-rollup-settings` | Upsert worker settings |
| GET | `/api/trip-completion-rollup-settings/pending` | Preview pending rollups |
| GET | `/api/trip-completion-rollup-settings/runs` | Recent worker runs |

### Completion read APIs (JWT + trip read)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/trip-completions` | List trip completion summaries |
| GET | `/api/trip-completions/{tripId}` | Trip completion detail + events |
| GET | `/api/route-completions` | Route-level completion summaries |

### Internal APIs (service token)

| Method | Path | Scope |
|--------|------|-------|
| GET | `/api/internal/trip-completion-rollups/pending` | `routarr.trips.completion.rollup` |
| POST | `/api/internal/trip-completion-rollups/process-batch` | same |

## Shared worker

- `RoutArrTripCompletionRollupJob` — default 30 min interval, batch 50, staleness 1h
- Config: `RoutArrTripCompletionRollup__RoutArrBaseUrl`, `RoutArrTripCompletionRollup__ServiceToken`

## Frontend (routarr-frontend)

- Settings → `TripCompletionRollupSettingsPanel` — enable toggle, staleness, pending/runs preview

## Tests

- `TripCompletionRollupRulesTests` — staleness, pending, duration, terminal status
- `RoutArrTripCompletionRollupWorkerTests` — auth, pending preview, batch materialize, read APIs
- `TripCompletionRollupSettingsPanel.test.tsx` — panel render

## Next slice

Per backlog: SupplyArr `[M12] procurement coordination worker`.
