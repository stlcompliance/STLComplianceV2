# Worker 70 — RoutArr route/stop foundations

## Slice name

M9 route planning spine — routes linked to trips, ordered stops, stop sequencing/status lifecycle, `/api/routes` + `/api/stops`, routarr-frontend routes workspace.

## Products touched

- **RoutArr API** (`apps/routarr-api`): route/stop domain tables, route CRUD + trip link + stop reorder, stop list/status endpoints, audit events.
- **RoutArr Frontend** (`apps/routarr-frontend`): routes & stops panel on home workspace, trip-filtered route list, stop arrive/complete actions.

## Schema

Migration: `RoutArrRouteStopFoundations`

Added RoutArr tables:

- `routarr_routes` — tenant-scoped routes (`routeNumber`, `title`, `routeStatus`, optional `tripId` FK, lifecycle timestamps)
- `routarr_route_stops` — tenant-scoped ordered stops (`stopKey`, `label`, `addressLabel`, `stopType`, `stopStatus`, `sequenceNumber`, arrival/completion timestamps)

Notes:

- Separate RoutArr PostgreSQL database; trip link is a nullable FK with unique `(tenantId, tripId)` when set.
- Route status lifecycle: `draft` → `planned` → `active` → `completed` (or `cancelled` reserved for future manage scope).
- Stop status lifecycle: `pending` → `arrived` → `completed` (or `skipped`); sequence enforcement blocks advancing later stops until earlier stops are terminal.
- Entity class `DispatchRoute` maps to `routarr_routes` (avoids ASP.NET Core routing name collision).

## API + auth changes

### RoutArr API endpoints

- `GET/POST /api/routes` — list/create routes (optional inline stops, optional trip link on create)
- `GET /api/routes/{routeId}` — route detail with ordered stops
- `PATCH /api/routes/{routeId}/link-trip` — attach route to an existing trip
- `PUT /api/routes/{routeId}/stops/reorder` — reorder all stops on editable routes
- `POST /api/routes/{routeId}/stops` — add a stop to an editable route
- `GET /api/stops?routeId=` — list ordered stops for a route
- `PATCH /api/stops/{stopId}/status` — arrive/complete/skip stop

### Authorization

Reuses trip dispatch scopes via `RoutArrAuthorizationService`:

- read: same as trips read (`RequireRoutesRead`)
- create/link/reorder/add: `routarr.routes.create` roles (`RequireRoutesCreate`)
- stop status perform: `routarr.trips.perform` roles (`RequireStopsPerform`)
- route access: dispatchers see all; drivers/creators see routes they created or linked trips they own/drive (`RequireRouteAccess`)

## Frontend changes

- `RoutesPanel` on home workspace below trips panel
- Create route with first stop; filter routes by selected trip
- Link unlinked route to selected trip
- Ordered stop list with arrive/complete action buttons when role permits

## Tests

### Backend unit (`RouteStopStatusRulesTests`)

- Stop lifecycle transition matrix for pending/arrived/completed/skipped

### Backend integration (`tests/STLCompliance.RoutArr.Auth.Tests`)

- route create, link trip, reorder stops, stop arrive/complete lifecycle, list by trip
- route create denied for driver role
- stop cannot complete before arrival

### Frontend unit

- `src/api/client.test.ts` — route list success parsing
- `src/components/RoutesPanel.test.tsx` — route list + create form rendering

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

- Dispatch board/calendar UI, driver/equipment availability panels
- Driver eligibility and asset dispatchability checks (M10)
- DVIR, proof capture, exceptions, route audit trail export
- SupplyArr backorders (next M8 slice option)

## Next slice (Worker 71)

Recommended: **RoutArr dispatch board foundations** or **SupplyArr backorders** per M8/M9 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.

Delivered in Worker 71 — see `W71_ROUTARR_DISPATCH_BOARD_FOUNDATIONS.md`.
