# Worker 80 — RoutArr bulk dispatch

## Slice name

M9 bulk dispatch actions — batch assign drivers/vehicles/status to multiple trips with conflict preview, apply API, routarr-frontend bulk panel, tests.

## Products touched

- **RoutArr API** (`apps/routarr-api`): `BulkDispatchRules`, `BulkDispatchService`, `POST /api/dispatch/bulk/preview`, `POST /api/dispatch/bulk/apply`, contracts, audit events.
- **RoutArr Frontend** (`apps/routarr-frontend`): `BulkDispatchPanel` on home workspace for dispatchers with assign scope.
- **Tests**: `BulkDispatchRulesTests`, `RoutArrBulkDispatchTests`, `BulkDispatchPanel.test.tsx`, `client.test.ts` bulk parsing.

## Schema

No new migration — uses existing trips, driver availability, and equipment availability tables.

## API + auth changes

### RoutArr API endpoints

- `POST /api/dispatch/bulk/preview` — dry-run batch driver/vehicle/status changes with per-trip conflict summary
- `POST /api/dispatch/bulk/apply` — apply batch changes; partial success per trip; `ignoreAvailabilityConflicts` override

### Batch item shape

Each item: `tripId`, optional `driverPersonId`, optional `vehicleRefKey`, optional `dispatchStatus`. At least one field required per item; max 100 items.

### Conflict rules

- Reuses W78 driver/vehicle availability and overlapping-trip checks
- Simulates prior batch items so intra-batch overlaps are detected in preview order
- Status preview validates transitions and driver requirements; cancellation requires manage scope

### Authorization

- preview + apply: `RequireTripsAssign` (`routarr.dispatch.assign` scope)
- bulk cancel items: `RequireTripsManage`

Audit: `dispatch_bulk.preview`, `dispatch_bulk.apply`.

## Frontend changes

- `BulkDispatchPanel` below dispatch board for assign-capable users
- Multi-select active trips, optional driver/vehicle/status fields
- Preview conflicts then apply with confirm on blocked items

## Tests

### Backend unit (`BulkDispatchRulesTests`)

- Status transition driver requirement
- Cancel without manage scope
- Simulation updates for intra-batch preview

### Backend integration (`RoutArrBulkDispatchTests`)

- Preview blocks driver with unavailable window
- Apply with override assigns driver and status
- Preview detects intra-batch driver overlap
- Apply without override fails blocked item

### Frontend unit

- `BulkDispatchPanel.test.tsx` — select trip, preview + apply flow
- `client.test.ts` — bulk preview response parsing

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

- Dispatch closeout, driver eligibility, asset dispatchability (M10)
- DVIR, proof capture, exceptions, route audit trail export
- SupplyArr reorder evaluation (next M8 slice option)

## Next slice (Worker 81)

Recommended: **SupplyArr reorder evaluation** or **RoutArr dispatch closeout** per M8/M9 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
