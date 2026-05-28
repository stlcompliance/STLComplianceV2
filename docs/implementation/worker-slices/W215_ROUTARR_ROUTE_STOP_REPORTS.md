# Worker 215 — RoutArr M12 route/stop execution reporting

## Slice name

M12 route/stop execution reporting — second RoutArr M12 transportation report slice. Tenant-scoped rollups on owned `routes` and `route_stops` with completion metrics, detail routes, CSV export, Reports workspace panel below dispatch reports.

## Products touched

- **RoutArr API** (`apps/routarr-api`): `RouteReportService`, `/api/reports/routes/*`.
- **RoutArr Frontend** (`apps/routarr-frontend`): `RouteReportsPanel` on Reports workspace.
- **Tests**: `RoutArrRouteReportTests`, `RouteReportsPanel.test.tsx`.

## Backend (RoutArr)

### APIs (JWT)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/reports/routes/summary?scope=daily\|weekly` | Route + stop rollups for reporting window |
| GET | `/api/reports/routes/summary/export` | CSV export of scoped route rows |
| GET | `/api/reports/routes/{routeId}` | Route detail with ordered stops + completion % |
| GET | `/api/reports/routes/stops/{stopId}` | Stop detail with route/trip context |

### Rollup metrics

- Route counts by `routeStatus`; stop counts by `stopStatus` and `stopType`
- Per-route: pending/arrived/completed/skipped stop counts and completion percent
- Scope/window aligned with dispatch board (`daily` / `weekly`)

No new migration.

### Authorization (reuses W214)

- read: `RequireDispatchReportRead`
- export: `RequireDispatchReportExport`

### Audit actions

- `routarr.reports.routes.summary`
- `routarr.reports.routes.export`
- `routarr.reports.routes.route.detail`
- `routarr.reports.routes.stop.detail`

## Frontend (routarr-frontend)

- `RouteReportsPanel` below `DispatchReportsPanel` on `/reports`
- Same session gates as dispatch reports (`canReadDispatchReports`, `canExportDispatchReports`)
- API client: `getRouteReportSummary`, route/stop detail, `exportRouteReportSummaryCsv`

## Tests

| Suite | Coverage |
|-------|----------|
| `RoutArrRouteReportTests` | Summary rollups; route/stop detail; CSV export; driver denied |
| `RouteReportsPanel.test.tsx` | Panel render, export button, permission gate |

## Verification commands

```powershell
dotnet test "tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~RoutArrRouteReport"
cd apps/routarr-frontend
npm run test -- RouteReportsPanel
```

## Relationship to W214

W214 covers trip/exception/delay dispatch reporting; W215 adds **route and stop execution** metrics on the same owned operational tables used by the dispatch board and stop status APIs.

## Next recommended RoutArr M12 slice

**Worker 216** — proof/DVIR reporting rollups when proof tables are worker-sliced, or **RoutArr entity bulk export** (MaintainArr W207 pattern) for audit/history CSV packages.
