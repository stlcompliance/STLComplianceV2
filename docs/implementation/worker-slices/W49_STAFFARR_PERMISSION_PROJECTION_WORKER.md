# Worker 49 — StaffArr permission projection worker

## Slice name

M12 cross-product scheduled worker — `shared-worker` materializes effective permission keys per person via StaffArr internal batch API, service token scope `staffarr.permissions.project`, materialized read path with compute fallback, unit and integration tests

## Products touched

- **StaffArr API** (`apps/staffarr-api`): `staffarr_person_permission_projections` + `staffarr_person_permission_projection_entries` tables, `PermissionProjectionService`, internal `/api/internal/permission-projections/*`, materialized-first `GET /api/people/{personId}/permissions/effective`, migration `StaffArrPersonPermissionProjections`
- **shared-worker** (`workers/shared-worker`): `StaffArrPermissionProjectionJob`, HTTP client, `StaffArrPermissionProjection` configuration
- **Tests**: `PermissionProjectionRulesTests`; `StaffArrPermissionProjectionWorkerTests` (4 cases)

## Schema

### Migration `StaffArrPersonPermissionProjections`

Table `staffarr_person_permission_projections`:

- `tenant_id`, `person_id`, `permission_count`, `computed_at`
- Unique index `(tenant_id, person_id)`
- Index `(tenant_id, computed_at)` for pending scans

Table `staffarr_person_permission_projection_entries`:

- `tenant_id`, `person_id`, `projection_id`, `permission_key`, `permission_name`, `scope_type`, `scope_value`
- Unique index `(tenant_id, person_id, permission_key, scope_type, scope_value)`

## API + auth changes

### StaffArr public (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/people/{personId}/permissions/effective` | Existing permission projection read rules; returns materialized projection when fresh, otherwise computes at read time |

### StaffArr internal (service token)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/internal/permission-projections/pending` | NexArr service token: source `shared-worker`, target `staffarr`, scope `staffarr.permissions.project` |
| POST | `/api/internal/permission-projections/process-batch` | Same |

`process-batch` body: optional `tenantId`, optional `asOfUtc`, `batchSize` (1–500, default 100), `stalenessHours` (1–168, default 1). Response includes refreshed projection summaries and per-person skip reasons.

Pending people: active employment status with no projection or `computed_at` older than `stalenessHours`. Each refresh recomputes effective permissions from active role assignments + templates and upserts header + entry rows. Batch audit: `permission_projection.refresh.batch`.

## shared-worker configuration

`StaffArrPermissionProjection` section:

| Key | Default | Purpose |
|-----|---------|---------|
| `Enabled` | `true` | Toggle job |
| `StaffArrBaseUrl` | `http://localhost:5102` | StaffArr API base |
| `ServiceToken` | `""` | Bearer for internal projection API |
| `ScanIntervalMinutes` | `30` | Periodic scan interval |
| `BatchSize` | `100` | Max people per run |
| `StalenessHours` | `1` | Refresh if projection older than window |
| `TenantId` | `null` | Optional tenant filter |

## Tests

### Unit (`PermissionProjectionRulesTests`)

- Staleness boundary (`IsStale`)
- Permission identity key builder
- Batch size normalization
- Staleness hours normalization

### Integration (`StaffArrPermissionProjectionWorkerTests`)

- `Process_batch_rejects_missing_service_token`
- `Process_batch_rejects_trainarr_source_token`
- `List_pending_returns_active_people_before_processing`
- `Process_batch_refreshes_projection_and_effective_read_uses_materialized_rows`

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.Shared.Worker.Tests/STLCompliance.Shared.Worker.Tests.csproj" -c Release
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~PermissionProjection"
```

## Remaining gaps

- No multi-tenant discovery loop in worker (optional `TenantId` only)
- Projections are not invalidated immediately on assignment/template changes (staleness window driven)
- Materialized entries omit source assignment details (sources empty on read); full source graph still requires compute fallback or future enrichment
- MaintainArr/RoutArr scheduled workers remain open per M12 milestone

## Next recommended slice

**MaintainArr/RoutArr scheduled workers** or **StaffArr permission projection source enrichment** — see `docs/08_EVENTS_WORKERS_AND_INTEGRATION.md` and `docs/implementation/worker-slices/00_SLICE_STATE.md`.
