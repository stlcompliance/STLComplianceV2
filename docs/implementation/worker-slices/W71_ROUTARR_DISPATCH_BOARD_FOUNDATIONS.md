# Worker 71 — RoutArr dispatch board foundations

## Slice name

M9 dispatch command center — aggregated trips/routes/stops by status, daily/weekly scope, work queue, late/at-risk highlighting, `/api/dispatch/board`, routarr-frontend dispatch board panel.

## Products touched

- **RoutArr API** (`apps/routarr-api`): `DispatchBoardService`, `DispatchBoardRules`, `GET /api/dispatch/board`, contracts, audit read event.
- **RoutArr Frontend** (`apps/routarr-frontend`): `DispatchBoardPanel` as default landing on home workspace with daily/weekly toggle.
- **Tests**: `DispatchBoardRulesTests`, `RoutArrDispatchBoardTests`, `DispatchBoardPanel.test.tsx`, `client.test.ts`.

## Schema

No new migration — aggregates existing `routarr_trips`, `routarr_routes`, and `routarr_route_stops` tables from Workers 69–70.

## API + auth changes

### RoutArr API endpoints

- `GET /api/dispatch/board?scope=daily|weekly` — tenant-scoped dispatch snapshot (default `daily`)

### Response aggregates (live queries, scoped window)

- **Trips**: counts by dispatch status; late/at-risk totals
- **Routes**: counts by route status
- **Stops**: counts by stop status
- **Work queue**: unassigned-driver trips, unlinked editable routes, pending stops
- **Assigned trips** / **Active trips**: row lists with late/at-risk flags, route and pending-stop counts
- **Window**: UTC day (daily) or 7-day (weekly); includes active non-terminal entities plus scheduled/overlapping items

Late = active trip past scheduled start (not started) or past scheduled end (not completed). At-risk = active trip with start/end within next 2 hours.

### Authorization

Reuses trip read scopes via `RequireDispatchBoardRead`:

- read: same as trips read (`RequireTripsRead`)
- dispatchers/managers see full tenant board; drivers see scoped trips/routes/stops they created or drive

Reads are audited as `dispatch_board.read`.

## Frontend changes

- `DispatchBoardPanel` rendered above trips/routes panels on home workspace
- Daily/weekly scope toggle
- Work queue cards, status summary grids, active/assigned trip lists with late (red) and at-risk (amber) highlighting

## Tests

### Backend unit (`DispatchBoardRulesTests`)

- Late trip when scheduled start passed and not started
- At-risk trip when end within two hours
- Completed trip is neither late nor at-risk

### Backend integration (`RoutArrDispatchBoardTests`)

- empty tenant returns zero counts
- after trip/route/stop seeding, board reflects work queue, status counts, assigned/active rows, at-risk flag
- weekly scope accepted
- requires authentication and `routarr` entitlement

### Frontend unit

- `src/api/client.test.ts` — dispatch board success parsing
- `src/components/DispatchBoardPanel.test.tsx` — board counts, trip rows, weekly scope toggle

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

- Route calendar, driver/equipment availability panels
- Drag-and-drop assignment, bulk actions, dispatch closeout
- Driver eligibility and asset dispatchability checks (M10)
- DVIR, proof capture, exceptions, route audit trail export
- SupplyArr backorders (next M8 slice option)

## Next slice (Worker 72)

Recommended: **SupplyArr backorders** or **RoutArr driver availability panel** per M8/M9 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.

Delivered in Worker 72 — see `W72_ROUTARR_ROUTE_CALENDAR.md`.
