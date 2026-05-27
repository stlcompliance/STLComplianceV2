# Worker 59 — MaintainArr maintenance history

## Slice name

M7 maintenance spine — aggregated maintenance timeline from inspections, defects, work orders, and PM events; JWT read API; maintainarr-frontend history panel; integration and frontend tests.

## Products touched

- **MaintainArr API** (`apps/maintainarr-api`): `MaintenanceHistoryService`, `GET /api/maintenance-history`, JWT asset-read auth.
- **maintainarr-frontend**: `MaintenanceHistoryPanel` on home workspace, maintenance history API client method.

## Schema

No new tables. Timeline is aggregated at read time from existing MaintainArr tables:

- `maintainarr_inspection_runs`
- `maintainarr_defects`
- `maintainarr_work_orders`
- `maintainarr_pm_schedules`

## API + auth changes

### MaintainArr user API (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/maintenance-history?assetId={guid}&page=&pageSize=` | Maintenance history read (asset read / MaintainArr entitlement) |

`MaintainArrAuthorizationService.RequireMaintenanceHistoryRead` reuses asset read rules.

Timeline categories and event types:

- **inspection** — `inspection_started`, `inspection_completed`
- **defect** — `defect_reported`, `defect_resolved`
- **work_order** — `work_order_created`, `work_order_started`, `work_order_completed`, `work_order_cancelled`
- **pm** — `pm_schedule_created`, `pm_completed`, `pm_marked_due`, `pm_marked_overdue`

Response shape: paginated `MaintenanceHistoryEntryResponse` (`entryId`, `assetId`, `category`, `eventType`, `title`, `detail`, `occurredAt`, `actorUserId`, `sourceEntityType`, `sourceEntityId`, `relatedEntityId`).

## Frontend changes

- `MaintenanceHistoryPanel` — asset picker, chronological event list with category badges
- Home workspace integrates panel below asset registry
- API client: `getMaintenanceHistory(accessToken, assetId, page, pageSize)`

## Tests

### Backend integration (`STLCompliance.MaintainArr.Auth.Tests`)

- `Maintenance_history_aggregates_inspections_defects_work_orders_and_pm`
- `Maintenance_history_pagination_returns_has_next_page_when_events_exceed_page_size`
- `Maintenance_history_missing_asset_returns_not_found`
- `Maintenance_history_requires_maintainarr_entitlement`

### Frontend unit

- `MaintenanceHistoryPanel.test.tsx` — list, empty asset prompt, empty timeline state
- `client.test.ts` — maintenance history success path

## Verification commands

```powershell
dotnet build "apps/maintainarr-api/MaintainArr.Api/MaintainArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~MaintenanceHistory"
cd apps/maintainarr-frontend
npm run test
npm run build
```

## Remaining gaps

- Asset readiness endpoint (`/api/asset-readiness`)
- Meter reading events in maintenance history
- Persisted maintenance history / audit stream for immutable evidence export
- Cross-asset fleet maintenance history rollups

## Next recommended slice

**MaintainArr asset readiness endpoint** per M7 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
