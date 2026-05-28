# Worker 209 — RoutArr M9 dispatch command center (consolidation slice)

## Slice name

M9 dispatch command center — consolidates trip status columns, persisted board scope, StaffArr person mirrors for quick assign, and a single command-center read API atop existing dispatch board/trip foundations (Workers 69–82).

## Products touched

- **RoutArr API** (`apps/routarr-api`): `TenantDispatchBoardState`, `StaffarrPersonRef`, `DispatchBoardStateService`, `StaffarrPersonRefService`, `DispatchCommandCenterService`, migration `RoutArrDispatchCommandCenter`, assign-driver mirror upsert.
- **RoutArr Frontend** (`apps/routarr-frontend`): `DispatchCommandCenterPanel` on Dispatch workspace above `DispatchBoardPanel`.
- **Tests**: `RoutArrDispatchCommandCenterTests`, `DispatchCommandCenterPanel.test.tsx`.

## Schema (migration `20260528132036_RoutArrDispatchCommandCenter`)

| Table | Purpose |
|-------|---------|
| `routarr_tenant_dispatch_board_states` | Per-tenant default board scope (`daily` / `weekly`) |
| `routarr_staffarr_person_refs` | Local mirror of StaffArr `personId` + `displayName` for dispatch UI |

Core trip/route entities remain on `routarr_trips`, `routarr_routes`, `routarr_route_stops` (Workers 69–70).

## API + auth

| Method | Route | Permission / audit |
|--------|-------|-------------------|
| `GET` | `/api/dispatch/command-center?scope=daily\|weekly` | `RequireDispatchBoardRead`; audit `dispatch_command_center.read` |
| `GET` | `/api/dispatch/board-state` | dispatch read |
| `PUT` | `/api/dispatch/board-state` | `CanAssignTrips` / `RequireTripsAssign`; audit `dispatch_board_state.update` |
| `GET` | `/api/dispatch/driver-refs` | dispatch read |
| `PUT` | `/api/dispatch/driver-refs` | assign permission |
| `GET` | `/api/trips?dispatchStatus=` | existing trip list (status filter) |
| `PATCH` | `/api/trips/{id}/assign-driver` | assign + upsert `StaffarrPersonRef` when `driverDisplayName` provided |

`DispatchCommandCenterResponse` bundles: board state, `DispatchBoardResponse`, trip columns by status, driver refs, action manifest for UI.

## Frontend

- Dispatch workspace section title/subtitle updated for command center.
- Status-column kanban-style panel with daily/weekly scope (persists via board-state when user can assign).
- Quick assign/dispatch from mirrored driver refs on planned/assigned trips.
- Existing `DispatchBoardPanel`, assignment, bulk, and closeout panels unchanged below.

## Tests

### Backend (`RoutArrDispatchCommandCenterTests`)

- Command center returns trip columns and embedded board after trip create
- Assign driver upserts StaffArr person ref; listed on `/api/dispatch/driver-refs`
- Board state PUT/GET persists `weekly` default scope

### Frontend (`DispatchCommandCenterPanel.test.tsx`)

- Renders command center header, status column, and trip card from mocked API

## Verification commands

```powershell
dotnet build "apps/routarr-api/RoutArr.Api/RoutArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~RoutArrDispatchCommandCenter"
cd apps/routarr-frontend
npm run test -- DispatchCommandCenterPanel
npm run build
```

## Relationship to prior slices

| Worker | Capability reused by W209 |
|--------|---------------------------|
| 69 | Trips, assign-driver, status lifecycle |
| 70 | Routes/stops (board aggregates) |
| 71 | `DispatchBoardService` / board API |
| 78–80 | Assignment preview, bulk dispatch |
| 82 | Dispatch closeout |

W209 does **not** replace those APIs; it adds tenant board preferences, driver mirrors, and a unified command-center read surface.

## Remaining M9 gaps (backlog)

- Exception queue (dedicated entity/API/UI)
- Active trip map/list (geo or enhanced list beyond board rows)
- Unassigned work queue as first-class panel (partially on board work queue today)
- Trip execution / driver portal / proof surfaces

## Next recommended RoutArr slice

**Worker 210 — RoutArr M9 exception queue** (or **active trip map/list**): backlog `[M9] exception queue` and `[M9] active trip map/list` in `docs/implementation/02_PRODUCT_IMPLEMENTATION_BACKLOG.md`; builds on command center + board without cross-product DB coupling.
