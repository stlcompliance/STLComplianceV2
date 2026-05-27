# Worker 10 - StaffArr org hierarchy management write flows

## Slice name

M4 workforce spine - org hierarchy management write flows (`/api/org-units` create/update/status) with permission-aware UI editor and hierarchy validation.

## Products touched

- **StaffArr API** (`apps/staffarr-api`): org-unit write endpoints, hierarchy validation, audit logging, migration updates.
- **StaffArr Frontend** (`apps/staffarr-frontend`): org hierarchy management UI (create/edit/status), permission-aware write behavior, API wiring.

## Schema

Migration: `20260527095140_StaffArrOrgHierarchyWriteFlows`

Changes:

- `staffarr_org_units`
  - added `status` (`active|inactive`, max 32, defaults to `active`)
- `staffarr_audit_events`
  - new tenant-scoped audit table for write-sensitive StaffArr actions

Notes:

- Tenant scoping preserved throughout reads/writes.
- No cross-database FKs introduced.

## API + auth changes

### StaffArr API endpoints

- `POST /api/org-units` - create org unit (write roles only)
- `PUT /api/org-units/{orgUnitId}` - edit org unit type/name/parent (write roles only)
- `PATCH /api/org-units/{orgUnitId}/status` - activate/deactivate org unit (write roles only)
- `GET /api/org-units` now returns `status` in payload

### Role enforcement

Write operations remain server-side permission-gated via `StaffArrAuthorizationService.RequirePeopleWrite`:

- allowed: `platform_admin`, `tenant_admin`, `staffarr_admin`, `hr_admin`
- denied: non-writer roles (e.g., `supervisor`, `tenant_member`)

### Hierarchy and validation rules

- tenant-scoped parent existence checks
- parent must be active
- no self-parent and no parent cycle creation
- duplicate type/name prevention per tenant (case-insensitive)
- status constrained to `active|inactive`
- cannot deactivate unit with active children

### Audit logging

Sensitive org-write actions emit tenant-scoped audit records:

- `org_unit.create`
- `org_unit.update`
- `org_unit.status_update`

## Frontend changes

- Added real **Org hierarchy management** panel on StaffArr home:
  - hierarchy tree rendering from `/api/org-units`
  - create org unit form (name/type/parent)
  - edit selected unit (name/type/parent)
  - activate/deactivate toggle
- Permission-aware UI behavior:
  - writer roles see mutation controls
  - non-writers see read-only tree + explicit permission notice
- Mutation errors surface API conflict/validation/forbidden responses directly in UI.

## Tests

### Backend integration (`tests/STLCompliance.StaffArr.Auth.Tests`)

Added coverage for:

- org-unit write happy path (create -> update -> status change)
- write denied for non-writer role
- duplicate-name conflict, hierarchy cycle conflict, and parent-deactivation conflict
- audit records persisted for successful writes

### Frontend unit (`apps/staffarr-frontend/src/components`)

Added coverage for:

- org hierarchy write-role gating helper
- read-only rendering for non-writers
- status toggle action dispatch from UI

## Verification commands

```powershell
dotnet ef migrations add StaffArrOrgHierarchyWriteFlows --project "apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj" --startup-project "apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj" --output-dir Migrations
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj"
cd apps/staffarr-frontend
npm run test
npm run build
```

## Remaining gaps

- Org-unit delete/archive flow is not implemented.
- Dedicated StaffArr audit query/reporting surface is not implemented.
- Org-linked assignment workflows (sites/departments/teams/positions) remain pending.
