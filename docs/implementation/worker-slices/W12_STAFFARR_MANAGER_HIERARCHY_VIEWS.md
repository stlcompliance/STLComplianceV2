# Worker 12 - StaffArr manager hierarchy + manager/subordinate views

## Slice name

M4 workforce spine - manager hierarchy traversal, manager linkage update path with cycle prevention, and manager/subordinate UI views driven by StaffArr APIs and assignment primitives.

## Products touched

- **StaffArr API** (`apps/staffarr-api`): manager update/read APIs, hierarchy traversal service, subordinate detail rollups, authorization and audit coverage, migration for manager lookup index.
- **StaffArr Frontend** (`apps/staffarr-frontend`): manager/subordinate panel, manager-chain + subordinate list/detail queries, manager update mutation with permission-aware write fallback.

## Schema

Migration: `20260527101207_StaffArrManagerHierarchySlice`

Changes:

- Added index on `staffarr_people` for `(tenant_id, manager_person_id)` to support tenant-scoped subordinate traversal.

Notes:

- No cross-database foreign keys were introduced.
- Existing `manager_person_id` linkage on people is reused and now has explicit traversal APIs and mutation guardrails.

## API + auth changes

### StaffArr API endpoints

- `PUT /api/people/{personId}/manager` - set or clear a person manager.
- `GET /api/people/{personId}/manager-chain` - fetch manager chain above person.
- `GET /api/people/{personId}/subordinates?includeIndirect=true|false&limit=N` - list direct or recursive subordinates.
- `GET /api/people/{personId}/subordinates/{subordinatePersonId}` - fetch subordinate detail constrained to manager hierarchy.

### Role and entitlement enforcement

All endpoints require authenticated + entitled StaffArr users.

- manager-chain/subordinate reads require `RequirePeopleRead`
- manager update requires `RequirePeopleWrite`
- allowed write roles: `platform_admin`, `tenant_admin`, `staffarr_admin`, `hr_admin`
- denied write example: `supervisor`

### Hierarchy validations

- person and manager must exist in tenant.
- manager cannot be self.
- manager must be active.
- update path rejects manager cycles.
- subordinate detail endpoint rejects requests where target person is not a subordinate of requested manager.
- all traversal/list/detail responses are tenant-scoped.

### Assignment-primitive rollups

Subordinate responses include derived active assignment path from assignment primitives:

- `site / department / team / position` from latest active assignment.
- direct report counts per subordinate.

### Audit logging

Successful manager mutations emit:

- `people.manager_update`

## Frontend changes

- Added **Manager and subordinates** panel in StaffArr home:
  - manager chain view for selected person
  - subordinate tree list (depth-aware)
  - subordinate detail card (manager + active assignment path)
  - manager update form for permitted roles
- Permission-aware behavior:
  - writer roles can update manager linkage
  - non-writers see read-only fallback text
- Mutation errors mapped to user-facing categories:
  - forbidden (`403`)
  - conflict/cycle (`409`)
  - validation (`400`)

## Tests

### Backend integration (`tests/STLCompliance.StaffArr.Auth.Tests`)

Added coverage for:

- manager update happy path + manager chain traversal + subordinate list/detail responses
- manager update denied for non-writer role
- self-manager, cycle, and unknown-manager validation failures
- manager update audit event persistence

### Frontend unit (`apps/staffarr-frontend/src/components`)

Added coverage for:

- manager hierarchy panel read-only fallback
- manager update form submit dispatch
- mutation error classification copy mapping

## Verification commands

```powershell
dotnet ef migrations add StaffArrManagerHierarchySlice --project "apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj" --startup-project "apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj" --output-dir Migrations
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj"
dotnet build "apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj"
cd apps/staffarr-frontend
npm run test
npm run build
```

## Remaining gaps

- No historical version timeline for manager linkage changes beyond audit events.
- No bulk manager reassignment flow.
- Next slice should implement role templates + permission templates and assignment surfaces.
