# StaffArr person export bundle

## Slice name

M4 person export bundle — workforce directory CSV/JSON/ZIP export compatible with bulk import, managerEmail resolution, optional employment status filter, audit logging, StaffArr UI, integration + frontend tests

## Products touched

- **StaffArr API** (`apps/staffarr-api`): `PeopleExportService`, `/api/people/export` endpoints
- **StaffArr Frontend** (`apps/staffarr-frontend`): `PersonExportPanel` on home workspace
- **Integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): `StaffArrPersonExportTests`

## Schema

No new tables — reads from `staffarr_people`, writes `staffarr_audit_events`.

## API + auth changes

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/people/export/manifest` | write: `tenant_admin`, `staffarr_admin`, `hr_admin`, platform admin |
| GET | `/api/people/export?format=csv` | write: same |
| GET | `/api/people/export?format=json` | write: same |
| GET | `/api/people/export` | write: same — ZIP default |
| GET | `/api/people/export?employmentStatus=` | optional filter |

### CSV columns (import-compatible + reference fields)

`givenName,familyName,primaryEmail,employmentStatus,jobTitle,managerEmail,primaryOrgUnitId,personId`

### Audit events

- `person.export` with export id and person count reason code

## Frontend changes

- **PersonExportPanel** — employment status filter, CSV/ZIP download, JSON preview
- Reuses `canManagePeople` gate (same as bulk import)

## Tests

### Backend integration (`StaffArrPersonExportTests`)

- `People_export_manifest_lists_formats`
- `People_export_json_includes_manager_email`
- `People_export_csv_matches_import_header`
- `People_export_zip_contains_csv_and_manifest`
- `People_export_filters_by_employment_status`
- `People_export_denied_for_non_writer_role`
- `People_export_writes_audit_event`

### Frontend unit

- `PersonExportPanel.test.tsx`

## Verification commands

```powershell
dotnet build -c Release apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~PersonExport"
cd apps/staffarr-frontend
npm run test
npm run build
```

## Remaining gaps

- Org-unit filter UI not wired (API supports `orgUnitId`)
- NexArr external user linkage not included in export
- Scheduled/automated export worker deferred

## Next recommended slice

Render staging load soak against PO SLOs, or StaffArr org-unit filter on person export UI.
