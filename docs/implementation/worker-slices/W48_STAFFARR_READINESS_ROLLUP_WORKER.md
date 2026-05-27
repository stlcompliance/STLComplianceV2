# Worker 48 — StaffArr readiness rollup worker

## Slice name

M12 cross-product scheduled worker — `shared-worker` refreshes materialized team/site readiness rollup tables via StaffArr internal batch API, service token scope `staffarr.readiness.rollup`, supervisor-facing rollup read APIs, and StaffArr frontend supervisor panel

## Products touched

- **StaffArr API** (`apps/staffarr-api`): `staffarr_readiness_rollups` table, `ReadinessRollupService`, public `/api/readiness-rollups/*`, internal `/api/internal/readiness-rollups/*`, migration `StaffArrReadinessRollups`
- **shared-worker** (`workers/shared-worker`): `StaffArrReadinessRollupJob`, HTTP client, `StaffArrReadinessRollup` configuration
- **StaffArr Frontend** (`apps/staffarr-frontend`): `ReadinessRollupSupervisorPanel` on home for supervisor+ roles
- **Tests**: `ReadinessRollupRulesTests`; `StaffArrReadinessRollupWorkerTests` (5 cases); `ReadinessRollupSupervisorPanel.test.tsx`

## Schema

### Migration `StaffArrReadinessRollups`

Table `staffarr_readiness_rollups`:

- `scope_type` (`team` | `site`), `org_unit_id`, denormalized `org_unit_name`
- `total_members`, `ready_count`, `not_ready_count`, `override_count`, `ready_percent`
- `computed_at` for staleness detection
- Unique index `(tenant_id, scope_type, org_unit_id)`
- Index `(tenant_id, scope_type, computed_at)` for pending scans

## API + auth changes

### StaffArr public (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/readiness-rollups/teams` | `staffarr.certifications.read` roles (supervisor+) |
| GET | `/api/readiness-rollups/teams/{teamOrgUnitId}` | Same |
| GET | `/api/readiness-rollups/sites` | Same |
| GET | `/api/readiness-rollups/sites/{siteOrgUnitId}` | Same |

Optional `siteOrgUnitId` query filter on team list.

### StaffArr internal (service token)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/internal/readiness-rollups/pending` | NexArr service token: source `shared-worker`, target `staffarr`, scope `staffarr.readiness.rollup` |
| POST | `/api/internal/readiness-rollups/process-batch` | Same |

`process-batch` body: optional `tenantId`, optional `asOfUtc`, `batchSize` (1–500, default 50), `stalenessHours` (1–168, default 1). Response includes refreshed rollup summaries and per-scope skip reasons.

Pending scopes: active `team` and `site` org units with no rollup or `computed_at` older than `stalenessHours`. Each refresh aggregates active org assignments, calls person readiness calculation (certifications, training blockers, overrides), and upserts the rollup row. Batch audit: `readiness_rollup.refresh.batch`.

## shared-worker configuration

`StaffArrReadinessRollup` section:

| Key | Default | Purpose |
|-----|---------|---------|
| `Enabled` | `true` | Toggle job |
| `StaffArrBaseUrl` | `http://localhost:5102` | StaffArr API base |
| `ServiceToken` | `""` | Bearer for internal rollup API |
| `ScanIntervalMinutes` | `30` | Periodic scan interval |
| `BatchSize` | `50` | Max org units per run |
| `StalenessHours` | `1` | Refresh if rollup older than window |
| `TenantId` | `null` | Optional tenant filter |

## Tests

### Unit (`ReadinessRollupRulesTests`)

- Staleness boundary (`IsStale`)
- Ready percent rounding
- Aggregate count helper
- Batch size normalization

### Integration (`StaffArrReadinessRollupWorkerTests`)

- `Process_batch_rejects_missing_service_token`
- `Process_batch_rejects_trainarr_source_token`
- `List_pending_returns_team_and_site_org_units_before_processing`
- `Process_batch_refreshes_team_readiness_rollup_and_supervisor_can_read_it`
- `List_team_rollups_denies_tenant_member_without_supervisor_scope`

### Frontend unit

- `ReadinessRollupSupervisorPanel.test.tsx` renders team table and empty site state

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.Shared.Worker.Tests/STLCompliance.Shared.Worker.Tests.csproj" -c Release
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~ReadinessRollup"
```

## Remaining gaps

- No multi-tenant discovery loop in worker (optional `TenantId` only)
- Rollups are not invalidated immediately on assignment/readiness changes (staleness window driven)
- Permission projection materialization worker remains open (W14 computes at read time)

## Next recommended slice

**StaffArr permission projection worker** or **MaintainArr/RoutArr scheduled workers** per M12 milestone priority — see `docs/08_EVENTS_WORKERS_AND_INTEGRATION.md`.
