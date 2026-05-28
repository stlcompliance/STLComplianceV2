# Worker 216 — RoutArr M12 entity bulk export

## Slice decision

**Proof/DVIR tables:** Not present in `routarr-api` (no entities, migrations, or endpoints). Worker 216 implements **entity bulk export** per MaintainArr W207 pattern instead of proof/DVIR reporting.

## Slice name

M12 data exports — manifest + tenant-scoped CSV downloads for trips, routes, and dispatch exceptions; coordinated with W214–215 report CSV endpoints; Reports workspace panel below route reports.

## Products touched

- **RoutArr API** (`apps/routarr-api`): `RoutArrEntityBulkExportService`, `/api/exports/*`
- **RoutArr Frontend** (`apps/routarr-frontend`): `DataExportsPanel` on Reports workspace
- **Tests**: `RoutArrEntityBulkExportTests`, `DataExportsPanel.test.tsx`

## Backend (RoutArr)

### APIs (JWT)

| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/api/exports/manifest` | Lists entity CSV exports + report export routes |
| GET | `/api/exports/trips` | Full tenant trip CSV (`dispatchStatus` filter) |
| GET | `/api/exports/routes` | Route CSV with trip number + stop count (`routeStatus` filter) |
| GET | `/api/exports/dispatch-exceptions` | Exception CSV with trip context (`status` filter) |

No new migration — materialized from owned tables.

### Authorization

- `RequireEntityExport` → `RequireDispatchReportExport` (manager, admin, tenant admin)

### Audit actions

- `routarr.exports.trips` — `reasonCode` = row count
- `routarr.exports.routes`
- `routarr.exports.dispatch_exceptions`

### Manifest `reportExports`

- Dispatch report CSV (`/api/reports/dispatch/summary/export`) — W214
- Route execution report CSV (`/api/reports/routes/summary/export`) — W215

`auditPackageFormats` is empty (RoutArr has no audit package export surface yet).

## Frontend

- `DataExportsPanel` below dispatch and route report panels on `/reports`
- Uses `canExportDispatchReports` for manifest + download buttons
- Manifest-driven entity list with per-entity **Download CSV**

## Tests

| Suite | Coverage |
|-------|----------|
| `RoutArrEntityBulkExportTests` | Manifest; trips/routes/exceptions CSV; driver denied |
| `DataExportsPanel.test.tsx` | Export buttons; role gate message |

## Verification commands

```powershell
dotnet test "tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~RoutArrEntityBulkExport"
cd apps/routarr-frontend
npm run test -- DataExportsPanel
```

## Next recommended RoutArr slice

**Worker 217** — proof/DVIR **persistence + workflow** slice (tables, capture APIs, driver surfaces) before proof/DVIR **reporting**; or **TrainArr M12** / **Compliance Core M12** backlog per suite priority.
