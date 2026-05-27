# Worker 14 - StaffArr scoped effective-permission projection + permission history timeline

## Slice name

M4 workforce spine - scoped effective permission projection read API + permission assignment timeline history API, server-enforced read authorization, and real StaffArr UI visibility for projection and timeline.

## Products touched

- **StaffArr API** (`apps/staffarr-api`): permission history event schema, effective projection query logic, timeline query logic, endpoint additions, and authorization enforcement.
- **StaffArr Frontend** (`apps/staffarr-frontend`): projection/timeline API client integration and real home-page panel rendering for selected people.
- **StaffArr integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): permission projection/timeline behavior and auth coverage.

## Schema

Migration: `20260527103114_StaffArrPermissionProjectionHistoryTimeline`

Changes:

- Added `staffarr_permission_history_events`
  - columns: `tenant_id`, `person_id`, `assignment_id`, `role_template_id`, `permission_template_id`, `actor_user_id`, `event_type`, `assignment_status`, role/permission key+name snapshots, scoped values, `occurred_at`
  - indexes:
    - `(tenant_id, person_id, occurred_at)`
    - `(tenant_id, assignment_id, occurred_at)`
  - foreign keys remain within StaffArr-owned tables only

## API + auth changes

### StaffArr API endpoints

- `GET /api/people/{personId}/permissions/effective` - computes active effective permission projection for person using active role assignments + active role template mappings.
- `GET /api/people/{personId}/permissions/history?limit=100` - returns permission timeline events ordered by `occurredAt` desc.

### Event behavior

Permission history events are persisted on:

- person role assignment create (`assignment_created`)
- person role assignment status update (`assignment_status_updated`)
- role template permission mapping update for active assignments (`role_template_permissions_updated`)

### Role and entitlement enforcement

All projection and history reads require authenticated + StaffArr-entitled users, and are enforced server-side:

- `platform_admin`, `tenant_admin`, `staffarr_admin`, `hr_admin`, `supervisor` can read any person projection/timeline in tenant.
- `tenant_member` is limited to self (`personId` claim match).
- unrelated tenant-member reads are forbidden.

## Frontend changes

- Added **Scoped effective permissions and history** panel on StaffArr home.
- Panel renders:
  - effective permission projection (permission key/name, resolved scope, source-count visibility)
  - permission history timeline entries (event type, role/permission context, scope, timestamp)
- Real API integration with:
  - `GET /api/people/{personId}/permissions/effective`
  - `GET /api/people/{personId}/permissions/history`
- Query invalidation now refreshes projection/timeline after role-template and assignment mutations.

## Tests

### Backend integration

Extended `StaffArrHandoffApiTests` to cover:

- projection and timeline happy-path with role assignment create + status transition
- effective permission removal after assignment deactivation
- timeline event capture for both create and status update event types
- forbidden response for unrelated `tenant_member` permission projection read

### Frontend unit

Added `PermissionProjectionTimelinePanel.test.tsx` coverage for:

- rendering of effective permission rows
- rendering of timeline event row and mapped event label copy

## Verification commands

```powershell
dotnet ef migrations add StaffArrPermissionProjectionHistoryTimeline --project "apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj" --startup-project "apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj" --output-dir Migrations
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj"
dotnet build "apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj"
cd apps/staffarr-frontend
npm run test -- --run
npm run build
```

## Remaining gaps

- Permission projection is computed at read time; no dedicated materialized projection worker exists yet.
- Timeline currently tracks role-assignment and role-template mapping lifecycle impacts; certification/readiness history integration is still pending.
- No dedicated export/reporting endpoint for permission history yet.

## Next recommended slice

Implement StaffArr certification visibility + manual certification grant foundations (tenant-scoped certification records, person certification read/write APIs with auth, and real UI integration on selected profile).
