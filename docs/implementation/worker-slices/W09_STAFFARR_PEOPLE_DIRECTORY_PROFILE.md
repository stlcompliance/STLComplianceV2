# Worker 9 - StaffArr people directory + person profile core

## Slice name

M4 workforce spine - people directory, person profile, org-unit read context, and tenant-role-aware people API authorization.

## Products touched

- **StaffArr API** (`apps/staffarr-api`): people/org domain tables, provisioning, people/org endpoints, role-aware authorization.
- **StaffArr Frontend** (`apps/staffarr-frontend`): real `/api/people` and `/api/org-units` powered directory/profile workspace.
- **NexArr API** (`apps/nexarr-api`): handoff redeem payload enriched with tenant role and platform-admin flags for downstream authorization context.
- **Shared auth library** (`packages/shared-dotnet/STLCompliance.Shared`): added tenant-role claim type/helper.

## Schema

Migration: `20260527093953_StaffArrPeopleDirectorySlice`

Added StaffArr tables:

- `staffarr_people`
  - tenant-scoped person records (`personId` as PK)
  - optional `external_user_id` linkage to NexArr user identity (no cross-db FK)
  - manager self-reference and optional org-unit reference
- `staffarr_org_units`
  - tenant-scoped org structure nodes with parent self-reference

Notes:

- No cross-database foreign keys introduced.
- Human references use `personId` in API contracts and JWT context.

## API + auth changes

### StaffArr API endpoints

- `GET /api/people` - list people directory (requires StaffArr entitlement + read-capable tenant role)
- `GET /api/people/{personId}` - profile detail (self-access allowed with entitlement; others require read role)
- `POST /api/people` - create person (requires write-capable tenant role)
- `GET /api/org-units` - org-unit listing (requires people read role)

### Existing endpoint enhancements

- `POST /api/auth/handoff/redeem`
  - now provisions/links `personId` in StaffArr on first launch
  - embeds tenant-role and platform-admin claims in StaffArr JWT
- `GET /api/session` and `GET /api/me`
  - now return tenant role/platform-admin context
  - resolve canonical StaffArr `personId` from DB provisioning

### Tenant/product role awareness

Role checks are enforced server-side via `StaffArrAuthorizationService`:

- product entitlement (`staffarr`) required for all protected people/org reads
- role key gates:
  - read: `platform_admin`, `tenant_admin`, `staffarr_admin`, `hr_admin`, `supervisor`
  - write: `platform_admin`, `tenant_admin`, `staffarr_admin`, `hr_admin`

NexArr handoff redeem now returns `tenantRoleKey` + `isPlatformAdmin` so StaffArr can enforce role-aware permissions without crossing product ownership boundaries.

## Frontend changes

- Home workspace now renders:
  - authenticated session context (role/org/job)
  - real people directory (`/api/people`)
  - selected profile panel (`/api/people/{personId}`)
  - org-unit panel (`/api/org-units`)
- Error handling:
  - clears stale session and prompts relaunch on `401/403` from people/org API calls
  - explicit user-facing fallback for directory/profile load failures
- No mocked people/org data introduced.

## Validation and error handling

`POST /api/people` enforces:

- required name/email/status constraints
- RFC-valid email format
- tenant-scoped uniqueness for email
- tenant-scoped existence checks for org-unit and manager references

Structured API errors return domain codes for client handling (`people.validation`, `people.email_conflict`, `org_unit.not_found`, etc.).

## Tests

### Backend integration (`tests/STLCompliance.StaffArr.Auth.Tests`)

Added/updated coverage for:

- handoff redeem and `/api/me` with provisioned `personId`
- role context propagation (`tenantRoleKey`)
- people directory denied for member role
- people directory allowed for tenant admin role
- self profile access allowed without broad people-read role
- person create endpoint permission and validation failures

### Frontend unit

- `src/api/client.test.ts`
  - verifies `/api/people` success parsing
  - verifies forbidden response surfaces `StaffArrApiError`

## Verification commands

```powershell
dotnet ef migrations add StaffArrPeopleDirectorySlice --project "apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj" --startup-project "apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj" --output-dir Migrations
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj"
cd apps/staffarr-frontend
npm run test
npm run build
```

## Remaining gaps

- Org-unit create/update/delete flows and org-tree editor UI are not implemented yet.
- People update/deactivate workflows and assignment management are pending.
- Dedicated StaffArr API project test suite split (people/org vs auth) remains to be carved out.
