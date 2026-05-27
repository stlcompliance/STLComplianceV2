# StaffArr person update/deactivate workflows

## Slice name

M4 person profile update and employment status workflows — `PUT /api/people/{personId}`, `PATCH /api/people/{personId}/employment-status`, write auth, validation, audit logging, StaffArr UI, and integration tests

## Products touched

- **StaffArr API** (`apps/staffarr-api`): `PeopleService` update/status methods, `PeopleEndpoints`, contracts
- **StaffArr Frontend** (`apps/staffarr-frontend`): `PersonProfileEditorPanel`, API client/types, `HomePage` wiring
- **Integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): `StaffArrPersonUpdateWorkflowTests`

## Schema

No new tables — updates existing `staffarr_people` fields and writes `staffarr_audit_events`.

## API + auth changes

### StaffArr user APIs (JWT + StaffArr entitlement)

| Method | Route | Auth |
|--------|-------|------|
| PUT | `/api/people/{personId}` | write: `tenant_admin`, `staffarr_admin`, `hr_admin`, platform admin |
| PATCH | `/api/people/{personId}/employment-status` | write: same |

### Validation rules

- Email uniqueness per tenant (409 on conflict)
- Org unit must exist in tenant when provided
- Manager must exist in tenant; cannot self-manage; cycle detection on manager chain
- Employment statuses: `active`, `inactive`, `terminated`
- Deactivate/terminate blocked when person has active direct reports (409)

### Audit events

- `person.update` on profile field changes
- `person.employment_status_update` on status changes
- `person.create` on create (actor user id now recorded)

## Permission keys

- **Write:** `staffarr.people.write` role gate via `RequirePeopleWrite`

## Frontend changes

- **PersonProfileEditorPanel** — edit given/family name, email, org unit, manager, job title; employment status actions (reactivate, mark inactive, terminate)
- `canManagePeople` — tenant admin, staffarr admin, hr admin, platform admin
- TanStack Query mutations invalidate people, profile, manager chain, subordinates, and timeline queries

## Worker / events

None.

## Tests

### Backend integration (`StaffArrPersonUpdateWorkflowTests`)

- `Person_update_happy_path_updates_profile_fields`
- `Person_update_denied_for_non_writer_role`
- `Person_update_rejects_email_conflict`
- `Person_employment_status_deactivate_and_reactivate`
- `Person_deactivate_rejects_active_subordinates`
- `Person_employment_status_update_writes_audit_event`

### Frontend unit

- `PersonProfileEditorPanel.test.tsx`

## Verification commands

```powershell
dotnet build -c Release apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~PersonUpdate"
cd apps/staffarr-frontend
npm run test
npm run build
```

## Remaining gaps

- Bulk person import/export not started
- NexArr external user linkage sync on email change deferred
- Offboarding checklist workflow (incidents, certifications, assignments) not automated on terminate

## Next recommended slice

Product-owner SLO adoption for M13 load tests, or StaffArr bulk person onboarding import.
