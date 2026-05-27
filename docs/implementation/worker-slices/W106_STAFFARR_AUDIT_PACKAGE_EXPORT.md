# StaffArr audit package export foundations

## Slice name

M12 workforce audit package export — tenant workforce proof bundle (audit events, people, permission history, certifications, incidents, readiness overrides, training blockers) with ZIP/JSON export, auth, StaffArr UI, and integration tests

## Products touched

- **StaffArr API** (`apps/staffarr-api`): `AuditPackageService`, `/api/audit-packages` endpoints
- **StaffArr Frontend** (`apps/staffarr-frontend`): `AuditPackageExportPanel` on home workspace
- **Integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): `StaffArrAuditPackageTests`

## Schema

No new tables — exports from existing StaffArr tenant-scoped tables:

| Section | Source |
|---------|--------|
| `audit_events.json` | `staffarr_audit_events` (filtered by `OccurredAt`) |
| `people.json` | `staffarr_people` (filtered by `UpdatedAt`) |
| `permission_history.json` | `staffarr_permission_history_events` (filtered by `OccurredAt`) |
| `person_certifications.json` | `staffarr_person_certifications` (filtered by `UpdatedAt`) |
| `personnel_incidents.json` | `staffarr_personnel_incidents` (filtered by `CreatedAt`) |
| `readiness_overrides.json` | `staffarr_person_readiness_overrides` (filtered by `CreatedAt`) |
| `training_blockers.json` | `staffarr_person_training_blockers` (filtered by `PublishedAt`) |
| `manifest.json` | Package id, tenant, generated time, counts (ZIP only) |

## API + auth changes

### StaffArr user APIs (JWT + StaffArr entitlement)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/audit-packages/manifest` | read: people-read roles (`tenant_admin`, `staffarr_admin`, `hr_admin`, `supervisor`, platform admin) |
| GET | `/api/audit-packages/export?format=zip` | export: `tenant_admin`, `staffarr_admin`, `hr_admin`, platform admin |
| GET | `/api/audit-packages/export?format=json` | export: same — structured JSON body |
| GET | `/api/audit-packages/export?from=&to=` | optional `DateTimeOffset` filters per section |

Successful exports write `audit_package.export` audit events with package id as target.

## Permission keys

- **Read manifest:** `staffarr.people.read` role gate (via `RequireAuditPackageRead`)
- **Export:** `staffarr.audit.export` role gate — `tenant_admin`, `staffarr_admin`, `hr_admin`, platform admin

## Frontend changes

- **AuditPackageExportPanel** on StaffArr home — manifest sections, optional from/to dates, ZIP download, JSON preview with counts
- `canExportAuditPackage` — tenant admin, staffarr admin, hr admin, platform admin

## Worker / events

None (async audit package generation worker deferred to M12 follow-up).

## Tests

### Backend integration (`StaffArrAuditPackageTests`)

- `Audit_package_manifest_lists_sections`
- `Audit_package_export_zip_contains_json_files`
- `Audit_package_export_json_returns_structured_package`
- `Audit_package_export_writes_audit_event`
- `Audit_package_export_denies_supervisor`
- `Audit_package_export_rejects_invalid_date_range`
- `Audit_package_export_date_filter_limits_audit_events`

### Frontend unit

- `AuditPackageExportPanel.test.tsx`

## Verification commands

```powershell
dotnet build -c Release apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~AuditPackage"
cd apps/staffarr-frontend
npm run test
npm run build
```

## Remaining gaps

- Async audit package generation worker (`staffarr-worker`) not started
- Cross-product joint audit bundles (StaffArr + TrainArr) remain separate product exports
- Org-unit assignments and readiness rollups not yet included as dedicated sections

## Next recommended slice

StaffArr person update/deactivate workflows or product-owner SLO adoption for M13 load tests.
