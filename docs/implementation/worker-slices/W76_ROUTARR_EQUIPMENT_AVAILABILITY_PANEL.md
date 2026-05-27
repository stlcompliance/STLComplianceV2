# Worker 76 — RoutArr equipment availability panel

## Slice name

M9 equipment availability panel — availability records by opaque MaintainArr vehicle ref key (`vehicleRefKey`), conflict detection with assigned trips, CRUD APIs, dispatch panel read API, routarr-frontend panel, tests.

## Products touched

- **RoutArr API** (`apps/routarr-api`): `routarr_equipment_availability` table, `EquipmentAvailabilityService`, `EquipmentAvailabilityRules`, CRUD `/api/equipment-availability`, panel `GET /api/dispatch/equipment-availability`, contracts, audit events.
- **RoutArr Frontend** (`apps/routarr-frontend`): `EquipmentAvailabilityPanel` on home workspace below driver availability with shared daily/weekly scope toggle.
- **Tests**: `EquipmentAvailabilityRulesTests`, `RoutArrEquipmentAvailabilityTests`, `EquipmentAvailabilityPanel.test.tsx`, `client.test.ts`.

## Schema

Migration: `RoutArrEquipmentAvailability`

Added RoutArr table:

- `routarr_equipment_availability` — tenant-scoped equipment availability windows keyed by opaque `vehicleRefKey` (`availabilityStatus`, `startsAt`, `endsAt`, `reason`, `notes`, audit timestamps)

Notes:

- Separate RoutArr PostgreSQL database; vehicle ref keys are opaque MaintainArr asset references (no cross-database FK).
- Availability statuses: `available`, `unavailable`, `limited` (`unavailable`/`limited` block assignment when overlapping active assigned trips for the same vehicle).

## API + auth changes

### RoutArr API endpoints

- `GET /api/dispatch/equipment-availability?scope=daily|weekly` — panel view with summary counts, records, and per-record trip conflicts
- `GET /api/dispatch/equipment-availability?start=YYYY-MM-DD&end=YYYY-MM-DD` — custom date range (max 31 days, end exclusive)
- `GET /api/equipment-availability` — list records (optional `vehicleRefKey`, scope/range filters)
- `GET /api/equipment-availability/{availabilityId}` — detail with conflicting trips
- `POST /api/equipment-availability` — create availability window
- `PATCH /api/equipment-availability/{availabilityId}` — update window/status
- `DELETE /api/equipment-availability/{availabilityId}` — remove record

### Conflict detection

- Compares blocking availability (`unavailable`, `limited`) against active assigned trips (`planned`, `assigned`, `dispatched`, `in_progress`) with matching `vehicleRefKey`
- Overlap uses scheduled trip start/end (open-ended start uses start as end anchor)
- Panel and detail responses include `hasConflict`, `conflictingTripCount`, and trip conflict rows

### Authorization

Reuses trip read scopes for panel/list/detail:

- read: same as trips read (`RequireEquipmentAvailabilityRead` → `RequireTripsRead`)
- all authenticated trip readers see full tenant equipment availability (fleet-wide operational data)
- write: dispatch assign roles only (`RequireEquipmentAvailabilityWrite` → `CanViewAllTrips`); drivers cannot create equipment availability records

Reads are audited as `equipment_availability_panel.read`; mutations audit create/update/delete.

## Frontend changes

- `EquipmentAvailabilityPanel` rendered below `DriverAvailabilityPanel` on home workspace
- Shared daily/weekly scope toggle with dispatch board, calendar, and driver availability
- Summary cards (unavailable/limited/available/conflicts), record list with conflict highlighting, optional create form for dispatch roles

## Tests

### Backend unit (`EquipmentAvailabilityRulesTests`)

- Time range overlap detection
- Conflict flagging for unavailable windows vs active assigned trips on matching vehicle
- Available status does not produce conflicts
- Non-matching vehicle ref keys excluded from conflicts

### Backend integration (`RoutArrEquipmentAvailabilityTests`)

- empty tenant returns zero-count panel (daily scope)
- unavailable window overlapping assigned trip surfaces conflict on detail and panel
- driver cannot create equipment availability records
- requires authentication and `routarr` entitlement

### Frontend unit

- `src/api/client.test.ts` — equipment availability panel success parsing
- `src/components/EquipmentAvailabilityPanel.test.tsx` — records, conflict summary, weekly scope toggle

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
- SupplyArr pricing/lead-time snapshots (next M8 slice option)

## Next slice (Worker 77)

Recommended: **SupplyArr pricing/lead-time snapshots** or **RoutArr drag-and-drop assignment** per M8/M9 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
