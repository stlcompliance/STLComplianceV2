# Worker 72 — RoutArr route calendar

## Slice name

M9 route calendar — day-bucketed calendar view API by date range for trips/routes/stops, routarr-frontend calendar panel, tests.

## Products touched

- **RoutArr API** (`apps/routarr-api`): `RouteCalendarService`, `RouteCalendarRules`, `GET /api/dispatch/calendar`, contracts, audit read event.
- **RoutArr Frontend** (`apps/routarr-frontend`): `RouteCalendarPanel` on home workspace below dispatch board with daily/weekly toggle.
- **Tests**: `RouteCalendarRulesTests`, `RoutArrRouteCalendarTests`, `RouteCalendarPanel.test.tsx`, `client.test.ts`.

## Schema

No new migration — aggregates existing `routarr_trips`, `routarr_routes`, and `routarr_route_stops` tables from Workers 69–70.

## API + auth changes

### RoutArr API endpoints

- `GET /api/dispatch/calendar?scope=daily|weekly` — tenant-scoped calendar (default `daily`, same UTC window as dispatch board)
- `GET /api/dispatch/calendar?start=YYYY-MM-DD&end=YYYY-MM-DD` — custom date range (max 31 days, end exclusive)

### Response shape

- **Days**: UTC day columns with ordered **events** (`trip`, `route`, `stop`)
- **Events**: label, status, scheduled timestamps, trip/route linkage, late/at-risk flags for trips
- **Summary**: trip/route/stop counts plus late/at-risk trip totals
- Multi-day trips appear on each overlapping day within the window

Event schedule anchors:

- Trips: `scheduledStartAt` (fallback `createdAt`)
- Routes: linked trip schedule, else `activatedAt`, else `createdAt`
- Stops: `scheduledArrivalAt` (fallback `createdAt`)

Scoping and access filters match the dispatch board (active/non-terminal entities plus scheduled/overlapping items).

### Authorization

Reuses trip read scopes via `RequireRouteCalendarRead`:

- read: same as trips read (`RequireTripsRead`)
- dispatchers/managers see full tenant calendar; drivers see scoped trips/routes/stops they created or drive

Reads are audited as `route_calendar.read`.

## Frontend changes

- `RouteCalendarPanel` rendered below `DispatchBoardPanel` on home workspace
- Shared daily/weekly scope toggle with dispatch board
- Day-column grid with trip/route/stop events and late (red) / at-risk (amber) trip highlighting

## Tests

### Backend unit (`RouteCalendarRulesTests`)

- Day enumeration for a window
- Single-day event bucketing
- Multi-day event spanning midnight

### Backend integration (`RoutArrRouteCalendarTests`)

- empty tenant returns one empty day (daily scope)
- after trip/route/stop seeding, calendar buckets events on the scheduled day
- weekly scope returns seven day columns
- custom start/end date range accepted
- requires authentication and `routarr` entitlement

### Frontend unit

- `src/api/client.test.ts` — route calendar success parsing
- `src/components/RouteCalendarPanel.test.tsx` — day events, at-risk summary, weekly scope toggle

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

- Driver/equipment availability panels, drag-and-drop assignment, bulk actions, dispatch closeout
- Driver eligibility and asset dispatchability checks (M10)
- DVIR, proof capture, exceptions, route audit trail export
- SupplyArr backorders (next M8 slice option)

## Next slice (Worker 73)

Recommended: **SupplyArr backorders** or **RoutArr driver availability panel** per M8/M9 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
