# Worker 78 — RoutArr drag-and-drop assignment

## Slice name

M9 dispatcher drag-and-drop assignment — assign drivers and vehicles to trips with availability/overlap conflict checks, preview API, enhanced assign endpoints, routarr-frontend assignment panel, tests.

## Products touched

- **RoutArr API** (`apps/routarr-api`): `DispatchAssignmentRules`, `DispatchAssignmentService`, `POST /api/dispatch/assignments/preview`, enhanced `PATCH /api/trips/{id}/assign-driver`, new `PATCH /api/trips/{id}/assign-vehicle`, contracts, audit events.
- **RoutArr Frontend** (`apps/routarr-frontend`): `DispatchAssignmentPanel` with HTML5 drag-and-drop on home workspace below dispatch board.
- **Tests**: `DispatchAssignmentRulesTests`, `RoutArrDispatchAssignmentTests`, `DispatchAssignmentPanel.test.tsx`.

## Schema

No new migration — uses existing trips, driver availability, and equipment availability tables.

## API + auth changes

### RoutArr API endpoints

- `POST /api/dispatch/assignments/preview` — dry-run assignment conflict check (`assignmentKind`: `driver` | `vehicle`)
- `PATCH /api/trips/{tripId}/assign-driver` — now validates blocking driver availability and overlapping driver trips (409 unless `ignoreAvailabilityConflicts: true`)
- `PATCH /api/trips/{tripId}/assign-vehicle` — assign or clear `vehicleRefKey` with equipment availability and overlapping vehicle trip checks

### Conflict rules

- **Driver**: blocking `unavailable`/`limited` availability windows overlapping trip schedule; other active assigned trips for same person with overlapping schedule
- **Vehicle**: blocking equipment availability windows; other active trips using same `vehicleRefKey` with overlapping schedule
- Trips without `scheduledStartAt` skip time-based overlap checks

### Authorization

- preview + assign: `RequireTripsAssign` (`routarr.dispatch.assign` scope)

Audit: `dispatch_assignment.preview`, `trip.assign_driver`, `trip.assign_vehicle`.

## Frontend changes

- `DispatchAssignmentPanel` below dispatch board when user can assign
- Draggable driver/equipment chips sourced from availability panels (daily/weekly scope shared with board)
- Drop targets on active trips; preview then assign; confirm override on blocking conflicts

## Tests

### Backend unit (`DispatchAssignmentRulesTests`)

- Blocking driver availability on overlap
- Overlapping driver trips exclude target trip
- Blocking equipment `limited` status

### Backend integration (`RoutArrDispatchAssignmentTests`)

- Preview blocks driver with unavailable window; assign returns 409; override flag succeeds
- Vehicle assign blocked by equipment maintenance window; override succeeds
- Preview detects overlapping driver trips

### Frontend unit

- `DispatchAssignmentPanel.test.tsx` — renders chips/trips, drop triggers preview + assign
- `client.test.ts` — assignment preview/assign client parsing (when extended)

## Verification commands

```powershell
dotnet build "apps/routarr-api/RoutArr.Api/RoutArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj" -c Release
cd apps/routarr-frontend
npm install
npm run test
npm run build
```

## Remaining gaps

- Bulk assignment actions, dispatch closeout
- Driver eligibility and asset dispatchability checks (M10)
- DVIR, proof capture, exceptions, route audit trail export
- SupplyArr availability snapshots (next M8 slice option)

## Next slice (Worker 79)

Recommended: **SupplyArr availability snapshots** or **RoutArr bulk dispatch actions** per M8/M9 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
