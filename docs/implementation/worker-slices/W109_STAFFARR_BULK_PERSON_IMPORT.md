# StaffArr bulk person onboarding import

## Slice name

M4 bulk person onboarding import — `POST /api/people/import` batch create with row-level validation, managerEmail resolution, dry-run preview, audit logging, StaffArr CSV import UI, integration + frontend tests

## Products touched

- **StaffArr API** (`apps/staffarr-api`): `PeopleBulkImportService`, `/api/people/import`
- **StaffArr Frontend** (`apps/staffarr-frontend`): `PersonBulkImportPanel` on home workspace
- **Integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): `StaffArrPersonBulkImportTests`

## Schema

No new tables — creates rows in `staffarr_people`, writes `staffarr_audit_events`.

## API + auth changes

### StaffArr user APIs (JWT + StaffArr entitlement)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/people/import` | write: `tenant_admin`, `staffarr_admin`, `hr_admin`, platform admin |

### Request body

```json
{
  "dryRun": false,
  "people": [
    {
      "givenName": "Jane",
      "familyName": "Doe",
      "primaryEmail": "jane.doe@example.com",
      "employmentStatus": "active",
      "jobTitle": "Technician",
      "managerEmail": null
    }
  ]
}
```

### Validation rules

- Max 100 rows per request
- Per-row validation (same rules as single create)
- Duplicate emails within batch → row error
- Existing tenant email → row error
- `managerEmail` resolves to earlier batch row or existing tenant person
- Partial success: successful rows persist; failed rows reported in `results`

### Audit events

- `person.create` per created person (`reasonCode: bulk_import`)
- `person.import.batch` summary when at least one person created

## Permission keys

- **Write:** `staffarr.people.write` via `RequirePeopleWrite`

## Frontend changes

- **PersonBulkImportPanel** — CSV textarea with template, dry-run toggle, per-row results
- Reuses `canManagePeople` from profile editor panel
- Invalidates people directory query after successful apply

## Worker / events

None.

## Tests

### Backend integration (`StaffArrPersonBulkImportTests`)

- `Bulk_import_creates_multiple_people`
- `Bulk_import_denied_for_non_writer_role`
- `Bulk_import_reports_duplicate_email_within_batch`
- `Bulk_import_reports_existing_tenant_email_conflict`
- `Bulk_import_dry_run_validates_without_persisting`
- `Bulk_import_writes_batch_audit_event`

### Frontend unit

- `PersonBulkImportPanel.test.tsx`

## Verification commands

```powershell
dotnet build -c Release apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~BulkImport"
cd apps/staffarr-frontend
npm run test
npm run build
```

## Remaining gaps

- CSV/JSON file upload and export not started
- NexArr external user provisioning on import deferred
- Role template assignment during import deferred

## Next recommended slice

Extend nightly live k6 to all seven PO scenarios, or StaffArr person export bundle.
