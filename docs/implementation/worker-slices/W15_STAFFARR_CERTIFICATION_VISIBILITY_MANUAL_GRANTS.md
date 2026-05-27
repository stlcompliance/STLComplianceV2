# Worker 15 - StaffArr certification visibility + manual certification grant foundations

## Slice name

M4 workforce spine - tenant-scoped certification definitions, person certification read/grant/update APIs, readiness baseline seed data, server-enforced authorization, and real StaffArr UI integration on selected profile.

## Products touched

- **StaffArr API** (`apps/staffarr-api`): certification definition and person certification schema, service logic, endpoints, readiness baseline seed, and authorization enforcement.
- **StaffArr Frontend** (`apps/staffarr-frontend`): certification panel with catalog visibility, manual grant form, revoke action, and real API integration.
- **StaffArr integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): certification happy path, auth denial, and self-read coverage.

## Schema

Migration: `20260527104423_StaffArrCertificationFoundations`

Changes:

- Added `staffarr_certification_definitions`
  - columns: `tenant_id`, `certification_key`, `name`, `description`, `category`, `default_validity_days`, `status`, timestamps
  - unique constraint on `(tenant_id, certification_key)`
- Added `staffarr_person_certifications`
  - columns: `tenant_id`, `person_id`, `certification_definition_id`, `source_type`, `status`, `granted_at`, `expires_at`, `notes`, `granted_by_user_id`, `external_publication_id`, timestamps
  - indexes on `(tenant_id, person_id, status)` and `(tenant_id, person_id, certification_definition_id, status)`
  - foreign keys remain within StaffArr-owned tables only

## API + auth changes

### StaffArr API endpoints

- `GET /api/certifications` - list tenant certification definitions; auto-seeds readiness baseline definitions on first read.
- `POST /api/certifications` - upsert certification definition.
- `GET /api/people/{personId}/certifications` - list person certification records with computed effective status.
- `POST /api/people/{personId}/certifications` - grant manual certification record.
- `PATCH /api/people/{personId}/certifications/{personCertificationId}` - update status/expiry/notes.

### Readiness baseline seed

On first definition list for a tenant, StaffArr upserts:

- `readiness.safety_orientation`
- `readiness.hazmat_awareness`
- `readiness.equipment_operator`

### Role and entitlement enforcement

- Catalog/person certification reads require authenticated + StaffArr-entitled users.
- `platform_admin`, `tenant_admin`, `staffarr_admin`, `hr_admin`, `supervisor` can read any person certifications in tenant.
- `tenant_member` can read own certifications only.
- Definition upsert, manual grant, and update require `staffarr.certifications.manage` equivalent writer roles (`platform_admin`, `tenant_admin`, `staffarr_admin`, `hr_admin`).

## Frontend changes

- Added **Certifications** panel on StaffArr home for selected person.
- Panel renders:
  - person certification records with effective status, source, grant/expiry dates, and notes
  - readiness catalog definitions
  - manual grant form and revoke action for authorized roles
- Real API integration with certification definition and person certification endpoints.
- Query invalidation refreshes person certifications after grant/update mutations.

## Tests

### Backend integration

Extended `StaffArrHandoffApiTests` to cover:

- readiness baseline seed + manual grant + revoke happy path with audit events
- forbidden grant for non-writer role (`supervisor`)
- tenant member self-read allowed and unrelated person read forbidden

### Frontend unit

Added `CertificationPanel.test.tsx` coverage for certification record and readiness catalog rendering.

## Verification commands

```powershell
dotnet ef migrations add StaffArrCertificationFoundations --project "apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj" --startup-project "apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj" --output-dir Migrations
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj"
dotnet build "apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj"
cd apps/staffarr-frontend
npm run test -- --run
npm run build
```

## Remaining gaps

- TrainArr publication ingestion (`trainarr_publication` source type) is modeled but not wired yet.
- Effective expiration is computed at read time; certification expiration worker remains M12 scope.
- Readiness calculation engine does not yet consume certification records (next M4 slice).
- No dedicated export/reporting endpoint for certification history yet.

## Next recommended slice

Implement StaffArr readiness calculation foundations (person readiness read API, plain-English blockers from missing/expired certifications, and real UI readiness summary on selected profile).
