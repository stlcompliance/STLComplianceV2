# Worker 213 — RoutArr M9 trip execution / driver portal

## Slice name

M9 driver portal — person-scoped schedule and trip execution APIs for assigned drivers, plus a dedicated Driver portal workspace route wired to real backend transitions (extends Worker 69 trip status rules).

## Products touched

- **RoutArr API** (`apps/routarr-api`): `DriverPortalService`, `/api/driver-portal/*` endpoints.
- **RoutArr Frontend** (`apps/routarr-frontend`): `DriverPortalPanel`, `/driver-portal` route and nav item.
- **Tests**: `RoutArrDriverPortalTests`, `DriverPortalPanel.test.tsx`.

## API

| Method | Route | Behavior |
|--------|-------|----------|
| `GET` | `/api/driver-portal/schedule` | Today's and upcoming trips for `personId` from JWT; audit `driver_portal.schedule.read` |
| `POST` | `/api/driver-portal/trips/{id}/dispatch` | `assigned` → `dispatched` (assignee only) |
| `POST` | `/api/driver-portal/trips/{id}/start` | → `in_progress` |
| `POST` | `/api/driver-portal/trips/{id}/complete` | → `completed` |
| `POST` | `/api/driver-portal/trips/{id}/close` | → `completed` (alias for in-progress close) |

### Schedule selection

- Filter: `AssignedDriverPersonId == principal.personId`
- Active dispatch statuses only (excludes completed/cancelled)
- **Today**: in-progress/dispatched always; scheduled start today; or assigned with `UpdatedAt` today when no schedule
- **Upcoming**: scheduled within next 7 days, status `planned`/`assigned`/`dispatched`

### Auth

- Read: `RequireDriverPortalRead` → `RequireTripsPerform` (`routarr_driver`, dispatcher, manager, admin)
- Execute: `RequireDriverPortalExecute` + assignee check (`driver_portal.not_assigned` 403)
- Reuses `TripService.UpdateDispatchStatusAsync` and W69 driver transition guards

No new migration.

## Frontend

- Nav: **Driver portal** (`/driver-portal`, Truck icon)
- Panel: today + upcoming lists with Dispatch / Start trip / Complete / Close actions from `can*` flags
- Invalidates driver schedule, trips, and active-trips queries on success

## Tests

| Suite | Coverage |
|-------|----------|
| `RoutArrDriverPortalTests` | Schedule lists assigned trip; dispatch → start → complete; completed trip leaves schedule; wrong assignee gets 403 on start |
| `DriverPortalPanel.test.tsx` | Renders schedule and calls start API |

## Verification commands

```powershell
dotnet test "tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~RoutArrDriverPortal"
cd apps/routarr-frontend
npm run test -- DriverPortalPanel
```

## Relationship to W69 and W209–212

W69 owns trip status transitions and driver role limits; W213 exposes a **driver-facing portal** with person-scoped schedule and POST actions without requiring dispatch workspace access. Dispatch command center (W209–212) remains operator-focused.

## Next recommended RoutArr slice

**RoutArr M12** — dispatch/transportation reporting cluster, or deeper M9 proof/DVIR execution surfaces per backlog priority.
