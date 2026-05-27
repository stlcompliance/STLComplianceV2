# Worker 82 — RoutArr dispatch closeout

## Slice name

M9 dispatch closeout — end-of-day closeout for remaining trips, routes, and stops; summary/preview/apply APIs, routarr-frontend closeout panel, tests.

## Products touched

- **RoutArr API** (`apps/routarr-api`): `DispatchCloseoutRules`, `DispatchCloseoutService`, `GET /api/dispatch/closeout/summary`, `POST /api/dispatch/closeout/preview`, `POST /api/dispatch/closeout/apply`, contracts, audit events.
- **RoutArr Frontend** (`apps/routarr-frontend`): `DispatchCloseoutPanel` on home workspace for dispatch assign scope.
- **Tests**: `DispatchCloseoutRulesTests`, `RoutArrDispatchCloseoutTests`, `DispatchCloseoutPanel.test.tsx`.

## Schema

No new migration — uses existing trips, routes, route stops tables and status fields.

## API + auth changes

### RoutArr API endpoints

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/dispatch/closeout/summary?scope=daily\|weekly` | `RequireTripsAssign` |
| POST | `/api/dispatch/closeout/preview` | `RequireTripsAssign` |
| POST | `/api/dispatch/closeout/apply` | `RequireTripsAssign` |

### Request body

```json
{
  "scope": "daily",
  "remainingTripDisposition": "complete|cancel",
  "openStopDisposition": "skip|complete"
}
```

### Closeout rules

- **Trips (cancel)**: all active dispatch statuses → `cancelled` (uses manage-capable status update path).
- **Trips (complete)**: `in_progress` → `completed`; `dispatched` → `in_progress` → `completed`; `assigned` with driver chains through dispatched; `planned` blocked (use cancel).
- **Stops (skip)**: `pending` / `arrived` → `skipped` where transitions allow.
- **Stops (complete)**: `arrived` → `completed`; `pending` blocked (use skip).
- **Routes**: after stops close, `complete` when all stops terminal; `cancel` when trip disposition is cancel.

Scope/window filtering reuses dispatch board daily/weekly window semantics.

Audit: `dispatch_closeout.summary`, `dispatch_closeout.preview`, `dispatch_closeout.apply`, plus per-entity `route_stop.closeout` / `route.closeout`.

## Frontend changes

- `DispatchCloseoutPanel` above bulk dispatch for assign-capable users
- Shows open trip/route/stop counts from summary API
- Preview then apply with confirmation

## Tests

### Backend unit (`DispatchCloseoutRulesTests`)

- Planned trip blocked on complete disposition
- Cancel allows planned trip
- Dispatched trip complete chain
- Stop skip/complete plans
- Route complete requires terminal stops

### Backend integration (`RoutArrDispatchCloseoutTests`)

- Summary lists open planned trip
- Preview blocks planned on complete
- Apply cancel closes trip and skips pending stop
- Apply complete closes in-progress trip

### Frontend unit

- `DispatchCloseoutPanel.test.tsx` — summary counts and preview flow

## Verification commands

```powershell
dotnet build "apps/routarr-api/RoutArr.Api/RoutArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~Closeout"
cd apps/routarr-frontend
npm install
npm run test
npm run build
```

## Remaining gaps

- Driver eligibility, asset dispatchability (M10)
- DVIR, proof capture, exceptions, route audit trail export
- SupplyArr demand intake from MaintainArr (next slice option)

## Next slice (Worker 83)

Recommended: **SupplyArr demand intake from MaintainArr** or **RoutArr driver eligibility** per M8/M9/M10 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
