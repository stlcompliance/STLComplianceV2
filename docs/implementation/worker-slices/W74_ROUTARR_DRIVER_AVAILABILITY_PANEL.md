# Worker 74 — RoutArr driver availability panel

## Slice name

M9 driver availability panel — availability records by opaque StaffArr person id, conflict detection with assigned trips, CRUD APIs, dispatch panel read API, routarr-frontend panel, tests.

## Products touched

- **RoutArr API** (`apps/routarr-api`): `routarr_driver_availability` table, `DriverAvailabilityService`, `DriverAvailabilityRules`, CRUD `/api/driver-availability`, panel `GET /api/dispatch/driver-availability`, contracts, audit events.
- **RoutArr Frontend** (`apps/routarr-frontend`): `DriverAvailabilityPanel` on home workspace below route calendar with shared daily/weekly scope toggle.
- **Tests**: `DriverAvailabilityRulesTests`, `RoutArrDriverAvailabilityTests`, `DriverAvailabilityPanel.test.tsx`, `client.test.ts`.

## Schema

Migration: `RoutArrDriverAvailability`

Added RoutArr table:

- `routarr_driver_availability` — tenant-scoped driver availability windows keyed by opaque `personId` (`availabilityStatus`, `startsAt`, `endsAt`, `reason`, `notes`, audit timestamps)

Notes:

- Separate RoutArr PostgreSQL database; person ids are opaque StaffArr references (no cross-database FK).
- Availability statuses: `available`, `unavailable`, `limited` (`unavailable`/`limited` block assignment when overlapping active assigned trips).

## API + auth changes

### RoutArr API endpoints

- `GET /api/dispatch/driver-availability?scope=daily|weekly` — panel view with summary counts, records, and per-record trip conflicts
- `GET /api/dispatch/driver-availability?start=YYYY-MM-DD&end=YYYY-MM-DD` — custom date range (max 31 days, end exclusive)
- `GET /api/driver-availability` — list records (optional `personId`, scope/range filters)
- `GET /api/driver-availability/{availabilityId}` — detail with conflicting trips
- `POST /api/driver-availability` — create availability window
- `PATCH /api/driver-availability/{availabilityId}` — update window/status
- `DELETE /api/driver-availability/{availabilityId}` — remove record

### Conflict detection

- Compares blocking availability (`unavailable`, `limited`) against active assigned trips (`planned`, `assigned`, `dispatched`, `in_progress`) for the same opaque `personId`
- Overlap uses scheduled trip start/end (open-ended start uses start as end anchor)
- Panel and detail responses include `hasConflict`, `conflictingTripCount`, and trip conflict rows

### Authorization

Reuses trip read scopes for panel/list/detail:

- read: same as trips read (`RequireDriverAvailabilityRead` → `RequireTripsRead`)
- dispatchers/managers see full tenant availability; drivers see only their own `personId` records
- write: dispatch assign roles (`CanViewAllTrips`) for any person, or `routarr_driver` for own `personId` only (`RequireDriverAvailabilityWrite`)

Reads are audited as `driver_availability_panel.read`; mutations audit create/update/delete.

## Frontend changes

- `DriverAvailabilityPanel` rendered below `RouteCalendarPanel` on home workspace
- Shared daily/weekly scope toggle with dispatch board and calendar
- Summary cards (unavailable/limited/available/conflicts), record list with conflict highlighting, optional create form when user can manage availability

## Tests

### Backend unit (`DriverAvailabilityRulesTests`)

- Time range overlap detection
- Conflict flagging for unavailable windows vs active assigned trips
- Available status does not produce conflicts

### Backend integration (`RoutArrDriverAvailabilityTests`)

- empty tenant returns zero-count panel (daily scope)
- unavailable window overlapping assigned trip surfaces conflict on detail and panel
- driver can create own availability but not another person's record
- requires authentication and `routarr` entitlement

### Frontend unit

- `src/api/client.test.ts` — driver availability panel success parsing
- `src/components/DriverAvailabilityPanel.test.tsx` — records, conflict summary, weekly scope toggle

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

- Drag-and-drop assignment, bulk actions, dispatch closeout
- Driver eligibility and asset dispatchability checks (M10)
- DVIR, proof capture, exceptions, route audit trail export
- SupplyArr returns (next M8 slice option)

## Next slice (Worker 75)

Recommended: **SupplyArr returns** or **RoutArr equipment availability panel** per M8/M9 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
