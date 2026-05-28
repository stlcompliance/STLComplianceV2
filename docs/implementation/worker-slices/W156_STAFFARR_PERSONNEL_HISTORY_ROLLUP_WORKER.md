# Worker 156 — StaffArr personnel history rollup worker (M12)

## Slice name

M12 personnel history rollup — materialized per-person workforce history read model, shared-worker scheduled refresh, JWT + TrainArr integration read APIs, People workspace summary panel, integration and frontend tests

## Products touched

- **StaffArr API** (`apps/staffarr-api`): `PersonnelHistoryService`, `PersonTimelineBuilder`, rollup/event tables, public + internal + integration endpoints
- **shared-worker** (`workers/shared-worker`): `StaffArrPersonnelHistoryRollupJob`, client, options
- **StaffArr Frontend** (`apps/staffarr-frontend`): `PersonHistorySummaryPanel`, person-history API client, People workspace wiring

## Schema

Migration `StaffArrPersonnelHistoryRollups`:

- `staffarr_personnel_history_rollups` — per-tenant/person summary (event counts by category, `LastEventAt`, `ComputedAt`)
- `staffarr_personnel_history_events` — materialized timeline entries keyed by `EntryId`

## API + auth changes

### StaffArr JWT (people read / self)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/person-history?personId=` | `RequirePersonHistoryRead` |
| GET | `/api/person-history/summary?personId=` | `RequirePersonHistoryRead` |
| GET | `/api/people/{personId}/person-history` | `RequirePersonHistoryRead` |
| GET | `/api/people/{personId}/person-history/summary` | `RequirePersonHistoryRead` |

Materialized history is used when fresh; otherwise live timeline aggregation via `PersonTimelineBuilder`.

### StaffArr internal (shared-worker)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/internal/personnel-history/pending` | source `shared-worker`, scope `staffarr.personnel.history.rollup` |
| POST | `/api/internal/personnel-history/process-batch` | Same |

### StaffArr integration (TrainArr)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/integrations/person-history` | source `trainarr`, scope `staffarr.personnel.history.read` |
| GET | `/api/integrations/person-history/summary` | Same |

## Permission keys

- JWT: reuses person timeline read rules (`RequirePersonHistoryRead`)
- Worker scope: `staffarr.personnel.history.rollup`
- Integration read scope: `staffarr.personnel.history.read`

## Worker behavior

`StaffArrPersonnelHistoryRollupJob` runs on a configurable interval (default 30 min), calls `POST /api/internal/personnel-history/process-batch` with a NexArr service token. Pending candidates are active people whose rollup is missing or older than `StalenessHours` (default 1h). Each refresh rebuilds events from incidents, certifications, permissions, readiness overrides, training blockers, notes, and documents.

## Frontend changes

- **PersonHistorySummaryPanel** on People workspace — category counts and last computed timestamp from materialized rollup API
- Live **PersonTimelinePanel** unchanged (on-demand aggregate)

## Tests

### Backend integration (`StaffArrPersonnelHistoryRollupWorkerTests`)

- Service token auth (missing, wrong source, TrainArr vs shared-worker)
- Pending list, process batch, supervisor summary/history read
- TrainArr integration read + scope separation

### Unit (`PersonnelHistoryRulesTests`)

- Staleness + category count aggregation

### Frontend unit

- `PersonHistorySummaryPanel.test.tsx`

## Remaining gaps

- RoutArr/MaintainArr integration consumers for person-history not wired
- No supervisor team-level history rollup (per-person only)
- Timeline endpoint still live-only; person-history is the materialized path

## Next recommended slice

Next open M12 worker backlog row from `00_SLICE_STATE.md`, or M4 follow-up if product-facing history UX needs pagination on materialized events only.
