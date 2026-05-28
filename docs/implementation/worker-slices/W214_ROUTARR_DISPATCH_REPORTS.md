# Worker 214 — RoutArr M12 dispatch/transportation reporting

## Slice name

M12 dispatch/transportation reporting (first RoutArr M12 cluster slice) — tenant-scoped rollups across owned trips and dispatch exceptions (including delay category), summary + detail + CSV export, Reports workspace, auth, and audit. Pattern: Worker 203 MaintainArr maintenance reports.

## Products touched

- **RoutArr API** (`apps/routarr-api`): `DispatchReportService`, `/api/reports/dispatch/*`.
- **RoutArr Frontend** (`apps/routarr-frontend`): `DispatchReportsPanel`, `/reports` workspace route.
- **Tests**: `RoutArrDispatchReportTests`, `DispatchReportsPanel.test.tsx`.

## Backend (RoutArr)

### APIs (JWT)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/reports/dispatch/summary?scope=daily\|weekly` | Trip + exception rollups for reporting window |
| GET | `/api/reports/dispatch/summary/export?scope=…` | CSV export of scoped trip rows |
| GET | `/api/reports/dispatch/trips/{tripId}` | Trip detail with route/stop/exception counts |
| GET | `/api/reports/dispatch/exceptions/{exceptionId}` | Exception detail with linked trip context |

### Data sources (owned tables only)

- `routarr` trips — status, schedule, late/at-risk/unassigned flags via `DispatchBoardRules`
- `routarr_dispatch_exceptions` — status/category counts; **delays** = `category == delay`
- Routes/stops — route and pending-stop counts for trip rows

No new migration.

### Authorization

- read: `RequireDispatchReportRead` → `RequireTripsAssign` (dispatcher, manager, admin)
- export: `RequireDispatchReportExport` → `RequireTripsManage` (manager, admin)

Drivers cannot read fleet-wide reports.

### Audit actions

- `routarr.reports.dispatch.summary`
- `routarr.reports.dispatch.export`
- `routarr.reports.dispatch.trip.detail`
- `routarr.reports.dispatch.exception.detail`

## Frontend (routarr-frontend)

- Workspace section `reports` with `DispatchReportsPanel`
- Nav item **Reports** (`/reports`, BarChart3 icon)
- Session gates: `canReadDispatchReports`, `canExportDispatchReports`
- API client: `getDispatchReportSummary`, trip/exception detail, `exportDispatchReportSummaryCsv`

## Tests

| Suite | Coverage |
|-------|----------|
| `RoutArrDispatchReportTests` | Summary with trip + delay metrics; trip/exception detail; CSV export; driver denied read/export |
| `DispatchReportsPanel.test.tsx` | Panel render, export button, permission gate |

## Verification commands

```powershell
dotnet test "tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~RoutArrDispatchReport"
cd apps/routarr-frontend
npm run test -- DispatchReportsPanel
```

## Relationship to M9 and W203

M9 operational surfaces (command center, driver portal) own workflow; W214 adds **read-only transportation reporting** on the same owned facts without cross-product DB access. Mirrors MaintainArr W203 summary/detail/export shape for suite consistency.

## Next recommended RoutArr M12 slice

**Worker 215** — route/stop execution or proof/DVIR reporting rollups (second transportation report), or **entity bulk export** aligned with MaintainArr W207 if prioritized for RoutArr audit packages.
