# Worker 13 - StaffArr role templates + permission templates/assignment foundations

## Slice name

M4 workforce spine - tenant-scoped role template and permission template foundations, person role assignment write/read/status APIs, server-side authorization enforcement, and real StaffArr UI integration for template + assignment workflows.

## Products touched

- **StaffArr API** (`apps/staffarr-api`): new role/permission template entities, role assignment foundations, API endpoints, service-level validation, tenant-scope checks, and audit events.
- **StaffArr Frontend** (`apps/staffarr-frontend`): role + permission template panel, person role assignment workflows, and real API integration for create/read/status updates.

## Schema

Migration: `20260527102256_StaffArrRoleTemplatePermissionFoundations`

Changes:

- `staffarr_permission_templates`
  - columns: `tenant_id`, `permission_key`, `name`, `description`, `status`, timestamps
  - unique constraint on `(tenant_id, permission_key)`
- `staffarr_role_templates`
  - columns: `tenant_id`, `role_key`, `name`, `description`, `status`, timestamps
  - unique constraint on `(tenant_id, role_key)`
- `staffarr_role_template_permissions`
  - columns: `tenant_id`, `role_template_id`, `permission_template_id`, `scope_type`, `scope_value`, timestamps
  - unique constraint on `(tenant_id, role_template_id, permission_template_id, scope_type, scope_value)`
- `staffarr_person_role_assignments`
  - columns: `tenant_id`, `person_id`, `role_template_id`, `scope_type`, `scope_value`, `status`, timestamps
  - unique constraint on `(tenant_id, person_id, role_template_id, scope_type, scope_value)`
  - index on `(tenant_id, person_id, status)` for read/status queries

Notes:

- All tables are tenant-scoped and stay within StaffArr ownership boundaries.
- No cross-product foreign keys were introduced.

## API + auth changes

### StaffArr API endpoints

- `GET /api/roles` - list tenant role templates with mapped permission templates.
- `POST /api/roles` - create role template with permission mappings.
- `PUT /api/roles/{roleTemplateId}` - update role template metadata/status/mappings.
- `GET /api/permissions` - list tenant permission templates.
- `POST /api/permissions` - upsert permission template by permission key.
- `GET /api/people/{personId}/role-assignments` - list person role assignments.
- `POST /api/people/{personId}/role-assignments` - create person role assignment.
- `PATCH /api/people/{personId}/role-assignments/{assignmentId}/status` - activate/deactivate assignment.

### Role and entitlement enforcement

All endpoints require authenticated + entitled StaffArr users.

- template/assignment reads require `RequireRoleTemplateRead`
- template/assignment writes require `RequireRoleTemplateWrite`
- allowed writer roles: `platform_admin`, `tenant_admin`, `staffarr_admin`, `hr_admin`
- denied write example: `supervisor`

### Validation and scope constraints

- role key and permission key normalization/format validation.
- non-tenant scopes must reference in-tenant org units of matching scope type (`site|department|team|position`).
- role assignment create requires active role template.
- duplicate template mappings and duplicate person role assignments are rejected.
- assignment status constrained to `active|inactive`.

### Audit logging

Successful write actions emit:

- `permission_template.upsert`
- `role_template.create`
- `role_template.update`
- `person_role_assignment.create`
- `person_role_assignment.status_update`

## Frontend changes

- Added **Role and permission templates** panel to StaffArr home.
- Real API-backed flows for:
  - permission template upsert
  - role template create (with permission mapping input)
  - role template activate/deactivate
  - person role assignment create
  - person role assignment activate/deactivate
- Permission-aware behavior:
  - writer roles can mutate templates and assignments
  - non-writers receive read-only fallback copy
- Mutation errors are surfaced with status-aware classification (`403/409/400`).

## Tests

### Backend integration (`tests/STLCompliance.StaffArr.Auth.Tests`)

Added coverage for:

- permission template + role template + person role assignment happy path including status mutation.
- denied template writes for non-writer role.
- assignment rejection when role template is inactive.
- audit event persistence for template/assignment write actions.

### Frontend unit (`apps/staffarr-frontend/src/components`)

Added coverage for:

- role/permission panel read-only fallback for non-writers.
- permission template upsert submit dispatch.
- role/permission mutation error classification copy mapping.

## Verification commands

```powershell
dotnet ef migrations add StaffArrRoleTemplatePermissionFoundations --project "apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj" --startup-project "apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj" --output-dir Migrations
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj"
dotnet build "apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj"
cd apps/staffarr-frontend
npm run test -- --run
npm run build
```

## Remaining gaps

- No historical timeline endpoint yet for permission/role assignment changes beyond audit events.
- No computed "effective permission projection" endpoint yet (direct + inherited + scoped merge).
- Next slice should implement scoped permission projection reads and permission history timeline surfaces.
