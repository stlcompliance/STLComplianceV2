# Worker 69 — RoutArr trip/dispatch foundations

## Slice name

M9 dispatch spine — trips, trip loads, dispatch status lifecycle, driver assignment (opaque StaffArr person id), NexArr handoff auth, routarr-frontend shell.

## Products touched

- **RoutArr API** (`apps/routarr-api`): trip/load domain tables, auth spine, trip CRUD + assign/status endpoints, audit events.
- **RoutArr Frontend** (`apps/routarr-frontend`): handoff launch, session storage, trips/dispatch workspace.
- **NexArr API** (`apps/nexarr-api`): RoutArr launch profile updated to frontend port 5180.

## Schema

Migration: `RoutArrTripDispatchFoundations`

Added RoutArr tables:

- `routarr_trips` — tenant-scoped trips (`tripNumber`, `title`, `dispatchStatus`, opaque `assignedDriverPersonId`, optional `vehicleRefKey`, lifecycle timestamps)
- `routarr_trip_loads` — tenant-scoped loads linked to trips (`loadKey`, `loadType`, `status`, sequence, origin/destination labels)
- `routarr_audit_events` — write audit trail for trip mutations

Notes:

- Separate RoutArr PostgreSQL database; no cross-database foreign keys.
- Driver references are opaque StaffArr person ids stored as strings.
- Dispatch status lifecycle: `planned` → `assigned` → `dispatched` → `in_progress` → `completed` (or `cancelled` with manage scope).

## API + auth changes

### RoutArr API endpoints

- `POST /api/auth/handoff/redeem` — NexArr handoff redeem (anonymous)
- `GET /api/session`, `GET /api/me` — session bootstrap and profile
- `GET/POST /api/trips` — list/create trips (optional inline loads on create)
- `GET /api/trips/{tripId}` — trip detail with loads
- `PATCH /api/trips/{tripId}/assign-driver` — assign driver (opaque person id)
- `PATCH /api/trips/{tripId}/status` — dispatch status transitions

### Authorization

`RoutArrAuthorizationService` enforces:

- product entitlement (`routarr`) required for all protected routes
- read: platform admin, tenant admin, routarr admin/manager/dispatcher/driver, tenant member
- create/assign: platform admin, tenant admin, routarr admin/manager/dispatcher (`routarr.routes.create`, `routarr.dispatch.assign`)
- perform status: above plus routarr driver on assigned trips (`routarr.trips.perform`)
- manage/cancel: platform admin, tenant admin, routarr admin/manager (`routarr.dispatch.manage`)
- dispatchers/managers see all trips; drivers see created or assigned trips only

## Frontend changes

- New `apps/routarr-frontend` on port **5180** with Vite proxy to RoutArr API (5105)
- `/launch?handoff=` redeem flow and `stl.routarr.session` storage
- Home workspace renders trip list, create form, driver assignment, and status transitions from real APIs
- Create/assign controls shown only when role permits

## Tests

### Backend unit (`TripDispatchStatusRulesTests`)

- Lifecycle transition matrix for planned/assigned/dispatched/in_progress/completed/cancelled

### Backend integration (`tests/STLCompliance.RoutArr.Auth.Tests`)

- handoff redeem + `/api/me` happy path
- trip create, assign driver, dispatch status lifecycle with loads
- trip create denied for driver role
- driver cannot cancel trip
- `/api/me` forbidden without routarr entitlement

### Frontend unit

- `src/api/client.test.ts` — trip list success/forbidden parsing
- `src/components/TripsPanel.test.tsx` — trip list + create form rendering

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

- Routes, stops, dispatch board/calendar UI
- Driver eligibility and asset dispatchability checks (M10)
- DVIR, proof capture, exceptions, route audit trail
- SupplyArr backorders (next M8 slice option)

## Next slice (Worker 70)

Recommended: **RoutArr route/stop foundations** or **SupplyArr backorders** per M8/M9 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.

Delivered in Worker 70 — see `W70_ROUTARR_ROUTE_STOP_FOUNDATIONS.md`.
