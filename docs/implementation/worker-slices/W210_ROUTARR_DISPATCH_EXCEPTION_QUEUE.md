# Worker 210 — RoutArr M9 dispatch exception queue

## Slice name

M9 exception queue — tenant-scoped dispatch exceptions with triage APIs (list open, assign, resolve, link to trip), auth, audit, and Dispatch workspace panel atop W209 command center.

## Products touched

- **RoutArr API** (`apps/routarr-api`): `DispatchException` entity, `DispatchExceptionService`, migration `RoutArrDispatchExceptionQueue`, `/api/dispatch/exceptions` endpoints.
- **RoutArr Frontend** (`apps/routarr-frontend`): `DispatchExceptionQueuePanel` on Dispatch workspace.
- **Tests**: `RoutArrDispatchExceptionQueueTests`, `DispatchExceptionQueuePanel.test.tsx`.

## Schema (migration `RoutArrDispatchExceptionQueue`)

| Table | Purpose |
|-------|---------|
| `routarr_dispatch_exceptions` | Dispatch exception records (key, title, category, status, optional `trip_id`, assignee, resolution) |

Statuses: `open`, `assigned`, `resolved`, `cancelled`. Open queue = `open` + `assigned`.

Categories: `delay`, `driver`, `vehicle`, `route`, `stop`, `compliance`, `other`.

## API + auth

| Method | Route | Permission / audit |
|--------|-------|-------------------|
| `GET` | `/api/dispatch/exceptions?status=open` | `RequireDispatchExceptionRead`; audit `dispatch_exception.list` |
| `POST` | `/api/dispatch/exceptions` | `RequireDispatchExceptionTriage` (= assign); audit `dispatch_exception.create` |
| `PATCH` | `/api/dispatch/exceptions/{id}/assign` | triage; audit `dispatch_exception.assign` |
| `PATCH` | `/api/dispatch/exceptions/{id}/resolve` | triage; audit `dispatch_exception.resolve` |
| `PATCH` | `/api/dispatch/exceptions/{id}/link-trip` | triage + trip access check; audit `dispatch_exception.link_trip` |

Trip link validates trip exists in tenant and caller has trip read access (same rules as trip APIs).

## Frontend

- `DispatchExceptionQueuePanel` below command center on Dispatch workspace
- Lists open queue, create form (dispatchers), assign-to-me, link trip by id, resolve with notes
- Read-only list for users without assign permission

## Tests

### Backend (`RoutArrDispatchExceptionQueueTests`)

- Full lifecycle: create → list open → assign → link trip → resolve → absent from open list

### Frontend (`DispatchExceptionQueuePanel.test.tsx`)

- Renders queue header and exception row from mocked API

## Verification commands

```powershell
dotnet build "apps/routarr-api/RoutArr.Api/RoutArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~RoutArrDispatchExceptionQueue"
cd apps/routarr-frontend
npm run test -- DispatchExceptionQueuePanel
```

## Relationship to W209

W209 command center remains the primary status-column surface. W210 adds operational exception triage without cross-product DB coupling — `trip_id` is a local FK within RoutArr only.

## Next recommended RoutArr slice

**Worker 211 — RoutArr M9 active trip map/list** (backlog `[M9] active trip map/list`): geo or enhanced active-trip panel building on board `activeTrips` and command center; alternative: unassigned work queue depth or trip execution/driver portal per backlog priority.
