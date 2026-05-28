# Worker 217 — RoutArr M9 proof/DVIR persistence & workflow

## Slice name

M9 trip proof and DVIR — durable pickup/delivery proof records and pre/post-trip DVIR inspections linked to trips, personId-scoped driver capture, dispatcher read, driver portal and dispatch UI surfaces (prerequisite for W218 proof/DVIR reporting).

## Products touched

- **RoutArr API** (`apps/routarr-api`): entities, migration, `TripProofDvirService`, trip + driver-portal endpoints.
- **RoutArr Frontend** (`apps/routarr-frontend`): proof/DVIR capture on `DriverPortalPanel`; read-only `TripProofDvirReadPanel` on Dispatch workspace.
- **Tests**: `RoutArrTripProofDvirTests`, `DriverPortalPanel.test.tsx`, `TripProofDvirReadPanel.test.tsx`.

## Database

| Table | Purpose |
|-------|---------|
| `routarr_trip_proof_records` | Pickup/delivery proof per trip (`proof_type`, `captured_by_person_id`, `reference_key`, `notes`, `captured_at`) |
| `routarr_trip_dvir_inspections` | Pre/post DVIR per trip (unique `tenant_id` + `trip_id` + `phase`); upsert on submit |

Migration: `20260528134119_RoutArrTripProofDvir`.

## API (JWT)

### Trip-scoped (dispatcher read + assigned driver write where noted)

| Method | Route | Auth | Behavior |
|--------|-------|------|----------|
| `GET` | `/api/trips/{tripId}/proofs` | Read: dispatcher (`CanViewAllTrips`) or assigned driver | List proof records |
| `POST` | `/api/trips/{tripId}/proofs` | Write: assigned driver only | Create pickup/delivery proof |
| `GET` | `/api/trips/{tripId}/dvir` | Read: dispatcher or assigned driver | List DVIR inspections |
| `POST` | `/api/trips/{tripId}/dvir` | Write: assigned driver only | Upsert pre_trip / post_trip DVIR |
| `GET` | `/api/trips/{tripId}/execution` | Read: dispatcher or assigned driver | Combined proofs + DVIR summary |

### Driver portal hooks

| Method | Route | Behavior |
|--------|-------|----------|
| `GET` | `/api/driver-portal/trips/{tripId}/execution` | Same as execution summary (assignee-scoped) |
| `POST` | `/api/driver-portal/trips/{tripId}/proofs` | Create proof |
| `POST` | `/api/driver-portal/trips/{tripId}/dvir` | Submit DVIR |

### Schedule enrichment

`GET /api/driver-portal/schedule` trip rows include `proofCount`, `hasPreTripDvir`, `hasPostTripDvir`.

### Authorization

- `RequireTripProofRead` — dispatcher/admin view-all OR driver with `RequireTripsPerform`
- `RequireTripProofWrite` / `RequireDvirPerform` — `RequireDriverPortalExecute` + assignee match
- Read access for non-dispatchers limited to trips where `AssignedDriverPersonId == personId`

### Audit actions

- `trip_proof.create`, `trip_proof.list`
- `trip_dvir.submit`, `trip_dvir.list`
- `trip_execution.summary.read`

## Frontend

- **Driver portal**: proof capture (pickup/delivery + reference/notes), pre/post DVIR submit on dispatched/in-progress trips; schedule shows proof/DVIR flags.
- **Dispatch** (assign-capable roles): `TripProofDvirReadPanel` — load execution summary by trip ID (read-only).

## Tests

| Suite | Coverage |
|-------|----------|
| `RoutArrTripProofDvirTests` | Driver proof + DVIR; dispatcher execution/proof list; unassigned driver 403; schedule flags |
| `DriverPortalPanel.test.tsx` | Schedule render + start action (mock execution query) |
| `TripProofDvirReadPanel.test.tsx` | Dispatcher load execution summary |

## Verification commands

```powershell
dotnet test "tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~RoutArrTripProofDvir"
cd apps/routarr-frontend
npm run test -- --run DriverPortalPanel TripProofDvirReadPanel
```

## Next slice

**Worker 218** — proof/DVIR reporting (summary/export on `routarr_trip_proof_records` and `routarr_trip_dvir_inspections`).
