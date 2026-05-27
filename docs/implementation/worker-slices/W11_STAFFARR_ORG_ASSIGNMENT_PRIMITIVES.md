# Worker 11 - StaffArr org-unit assignment primitives

## Slice name

M4 workforce spine - org-unit assignment primitives (`site/department/team/position` linkage) with tenant-scoped assignment create/read/update/status flows, permission-aware UI management, hierarchy consistency validation, and audit logging.

## Products touched

- **StaffArr API** (`apps/staffarr-api`): assignment entity + migration, person-assignment APIs, hierarchy/link validation, audit events.
- **StaffArr Frontend** (`apps/staffarr-frontend`): assignment management UI for selected person, API integration, permission-aware controls, error mapping for forbidden/conflict/validation.

## Schema

Migration: `20260527100032_StaffArrOrgUnitAssignmentPrimitives`

Changes:

- `staffarr_org_unit_assignments`
  - columns: `tenant_id`, `person_id`, `site_org_unit_id`, `department_org_unit_id`, `team_org_unit_id`, `position_org_unit_id`, `status`, timestamps
  - unique constraint on `(tenant_id, person_id, site_org_unit_id, department_org_unit_id, team_org_unit_id, position_org_unit_id)`
  - indexes for tenant/person/status lookup and linked org-unit joins

Notes:

- Tenant scoping is enforced in all queries and validations.
- No cross-database foreign keys were introduced.

## API + auth changes

### StaffArr API endpoints

- `GET /api/people/{personId}/org-assignments` - list assignments for a person
- `POST /api/people/{personId}/org-assignments` - create assignment
- `PUT /api/people/{personId}/org-assignments/{assignmentId}` - update assignment linkage
- `PATCH /api/people/{personId}/org-assignments/{assignmentId}/status` - activate/deactivate assignment

### Role and entitlement enforcement

All endpoints require authenticated + entitled StaffArr users.

- read path requires `RequirePeopleRead`
- write/status paths require `RequirePeopleWrite`
- allowed write roles: `platform_admin`, `tenant_admin`, `staffarr_admin`, `hr_admin`
- denied example: `supervisor`

### Validation and hierarchy constraints

- person must exist in tenant
- all referenced org units must exist in tenant
- required org-unit types are exact: `site`, `department`, `team`, `position`
- referenced org units for write/activate must be `active`
- hierarchy linkage must hold:
  - department descends from selected site
  - team descends from selected department
  - position descends from selected team
- duplicate assignment tuples for the same person are rejected
- assignment status constrained to `active|inactive`

### Audit logging

Successful write actions emit tenant-scoped audit records:

- `org_assignment.create`
- `org_assignment.update`
- `org_assignment.status_update`

## Frontend changes

- Added **Org-unit assignments** management panel on StaffArr home.
- UI is tied to selected person from the directory list.
- Create and edit flows require choosing site/department/team/position from active org units.
- Status toggle supports activate/deactivate on selected assignment.
- Permission-aware behavior:
  - write roles can mutate
  - non-writers get read-only view and explicit notice
- API errors are surfaced with status-aware messaging:
  - forbidden (`403`)
  - conflict (`409`)
  - validation (`400`)

## Tests

### Backend integration (`tests/STLCompliance.StaffArr.Auth.Tests`)

Added coverage for:

- assignment write happy path (create -> list -> update -> status)
- assignment write denied for non-writer role
- invalid linkage, inactive reference, and duplicate tuple conflicts
- audit event persistence for assignment write actions

### Frontend unit (`apps/staffarr-frontend/src/components`)

Added coverage for:

- assignment manager read-only fallback for non-writers
- status toggle dispatch from assignment UI
- mutation error classification for forbidden/conflict/validation responses

## Verification commands

```powershell
dotnet ef migrations add StaffArrOrgUnitAssignmentPrimitives --project "apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj" --startup-project "apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj" --output-dir Migrations
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj"
dotnet build "apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj"
cd apps/staffarr-frontend
npm run test
npm run build
```

## Remaining gaps

- No delete/archive endpoint for org assignments yet.
- No assignment history timeline/versioning yet.
- Next slice should address manager hierarchy and subordinate rollups tied to assignments.
