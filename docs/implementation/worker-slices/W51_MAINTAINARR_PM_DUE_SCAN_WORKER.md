# Worker 51 — MaintainArr PM due scan worker

## Slice name

M7/M12 cross-product scheduled worker — `shared-worker` scans MaintainArr PM schedules past `nextDueAt`, marks due/overdue states via service-token internal API, PM schedule CRUD foundations, manager due list UI, unit and integration tests

## Products touched

- **shared-worker** (`workers/shared-worker`): `MaintainArrPmDueScanJob`, HTTP client to MaintainArr internal API, `appsettings` for interval/batch/token/overdue grace
- **MaintainArr API** (`apps/maintainarr-api`): `maintainarr_pm_schedules`, `PmDueScanService` + rules, internal endpoints, user PM CRUD + `/due` list, migration index for scan queries
- **maintainarr-frontend**: `PmDuePanel` on home workspace, `getDuePmSchedules` client
- **Tests**: `STLCompliance.Shared.Worker.Tests` (PM due rules), `MaintainArrPmDueScanWorkerTests` (cross-product)

## Schema

### MaintainArr migration `MaintainArrPmSchedulesAndDueScan`

- `maintainarr_pm_schedules` — tenant-scoped PM schedules linked to `maintainarr_assets` (`scheduleKey`, `intervalDays`, `nextDueAt`, `dueStatus`, `status`, `lastDueScanAt`)
- Unique index `(tenant_id, asset_id, schedule_key)`
- Scan index `(tenant_id, status, due_status, next_due_at)`

## API + auth changes

### MaintainArr user API (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/preventive-maintenance/schedules` | PM read (manager/technician+) |
| GET | `/api/preventive-maintenance/due` | PM read — due/overdue schedules for managers |
| GET | `/api/preventive-maintenance/schedules/{id}` | PM read |
| POST | `/api/preventive-maintenance/schedules` | PM manage (`maintainarr.pm.manage`) |
| PUT | `/api/preventive-maintenance/schedules/{id}` | PM manage |
| PATCH | `/api/preventive-maintenance/schedules/{id}/status` | PM manage |

### MaintainArr internal (service token)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/internal/pm/pending-due` | NexArr service token: source `shared-worker`, target `maintainarr`, scope `maintainarr.pm.scan` |
| POST | `/api/internal/pm/process-due-scan` | Same |

`process-due-scan` body: optional `tenantId`, optional `asOfUtc`, `batchSize` (1–500, default 100), `overdueGraceDays` (0–30, default 1). Response includes marked due/overdue counts and per-item skip reasons.

Scanner selects active schedules in `scheduled` or `due` status where `nextDueAt` is on or before `asOfUtc`. Each match transitions `dueStatus` to `due` or `overdue` (after grace period) and audits `pm_schedule.due_scan.due` / `pm_schedule.due_scan.overdue`.

## shared-worker configuration

`MaintainArrPmDueScan` section:

| Key | Default | Purpose |
|-----|---------|---------|
| `Enabled` | `true` | Toggle job |
| `MaintainArrBaseUrl` | `http://localhost:5104` | MaintainArr API base |
| `ServiceToken` | `""` | Bearer for internal PM scan API |
| `ScanIntervalMinutes` | `15` | Periodic scan interval |
| `BatchSize` | `100` | Max schedules per run |
| `OverdueGraceDays` | `1` | Days after due before overdue |
| `TenantId` | `null` | Optional tenant filter |

## Tests

### Unit (`PmDueScanRulesTests`)

- Scannable schedule status guard
- `ShouldMarkDue` boundary cases
- `ShouldMarkOverdue` grace-period behavior
- `ResolveTargetDueStatus` prefers overdue when past grace

### Integration (`MaintainArrPmDueScanWorkerTests`)

- `Process_due_scan_rejects_missing_service_token`
- `Process_due_scan_marks_past_due_schedule_as_due`
- `List_pending_due_returns_candidates_before_processing`
- `Due_list_returns_marked_due_schedules_for_managers`

### Frontend

- `PmDuePanel.test.tsx` — due/overdue rendering and empty state
- `client.test.ts` — due PM list success path

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.Shared.Worker.Tests/STLCompliance.Shared.Worker.Tests.csproj" -c Release
dotnet test "tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~PmDue"
cd apps/maintainarr-frontend
npm run test
npm run build
```

## Remaining gaps

- No multi-tenant discovery loop in worker (optional `TenantId` only)
- Inspection due scan deferred to a later slice (inspection templates/runner foundations first)
- Auto work-order generation on PM due implemented (Worker 57)
- `maintainarr-worker` remains heartbeat-only; cross-product PM scan lives in `shared-worker`

## Next recommended slice

**MaintainArr inspection template builder** or **meter tracking** per M7 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
