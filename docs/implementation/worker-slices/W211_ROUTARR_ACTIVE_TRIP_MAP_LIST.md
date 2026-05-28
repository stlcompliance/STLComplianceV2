# Worker 211 — RoutArr M9 active trip map/list

## Slice name

M9 active trip map/list — enhanced dispatched/in-progress trip panel with late/at-risk flags, timeline map strip, and dedicated read API reusing dispatch board rules.

## Products touched

- **RoutArr API** (`apps/routarr-api`): `ActiveTripsService`, `GET /api/dispatch/active-trips`, contracts with timeline positioning.
- **RoutArr Frontend** (`apps/routarr-frontend`): `ActiveTripsPanel` (list + map toggle) on Dispatch workspace.
- **Tests**: `RoutArrActiveTripsTests`, `ActiveTripsTimelineTests`, `ActiveTripsPanel.test.tsx`.

## API

| Method | Route | Behavior |
|--------|-------|----------|
| `GET` | `/api/dispatch/active-trips?scope=daily\|weekly` | Dispatched + in-progress trips from board scope; enriches with vehicle/dispatch timestamps; timeline % for map strip; audit `dispatch_active_trips.read` |

Reuses `DispatchBoardService` for trip scope, access filters, late/at-risk (`DispatchBoardRules`), and active status definition (dispatched + in_progress).

### Response highlights

- `summary`: total, late, at-risk, dispatched, in-progress counts
- `items[]`: board row fields + `vehicleRefKey`, `dispatchedAt`, `startedAt`, `timelineOffsetPercent`, `timelineWidthPercent`

No new migration — read-only aggregation over `routarr_trips`.

## Frontend

- `ActiveTripsPanel` below exception queue on Dispatch workspace
- **List** view: enhanced cards (late red, at-risk amber) with vehicle and execution timestamps
- **Map** view: horizontal timeline strip with trip blocks positioned by schedule within board window

## Tests

| Suite | Coverage |
|-------|----------|
| `ActiveTripsTimelineTests` | Timeline offset/width calculation |
| `RoutArrActiveTripsTests` | Dispatched late trip in active list; empty tenant shape |
| `ActiveTripsPanel.test.tsx` | List + map toggle rendering |

## Verification commands

```powershell
dotnet test "tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~ActiveTrips"
cd apps/routarr-frontend
npm run test -- ActiveTripsPanel
```

## Relationship to W209–210

Command center (W209) shows status columns; exception queue (W210) handles triage. W211 focuses operators on **live execution** trips with map/list UX without geo coordinates (schedule-based timeline until routable coordinates exist).

## Next recommended RoutArr slice

**Worker 212 — RoutArr M9 unassigned work queue panel** (dedicated surface for `workQueue.unassignedDriverTripCount` + bulk assign shortcuts), or **trip execution / driver portal** per backlog priority; alternatively **RoutArr M12** transportation reporting cluster.
