# Worker 165 — TrainArr training audit package (M12)

## Slice name

M12 training audit package — tenant training compliance proof bundle (audit events, definitions, programs, requirements, assignments, evidence metadata, evaluations, signoffs, qualifications, StaffArr publications, person training history) with ZIP/JSON export, auth, TrainArr UI, and integration tests

## Products touched

- **TrainArr API** (`apps/trainarr-api`): `AuditPackageService`, `/api/audit-packages` endpoints
- **TrainArr Frontend** (`apps/trainarr-frontend`): `AuditPackageExportPanel` on settings workspace
- **Integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): `TrainArrAuditPackageTests`

## Schema

No new tables — exports from existing TrainArr tenant-scoped tables:

| Section | Source |
|---------|--------|
| `audit_events.json` | `trainarr_audit_events` (filtered by `OccurredAt`) |
| `training_definitions.json` | `trainarr_training_definitions` (filtered by `UpdatedAt`) |
| `training_programs.json` | `trainarr_training_programs` (filtered by `UpdatedAt`) |
| `training_program_definitions.json` | `trainarr_training_program_definitions` |
| `training_rule_pack_requirements.json` | `trainarr_training_rule_pack_requirements` (filtered by `UpdatedAt`) |
| `training_assignments.json` | `trainarr_training_assignments` (filtered by `UpdatedAt`) |
| `training_evidence.json` | `trainarr_training_evidence` metadata only (filtered by `CreatedAt`) |
| `training_evaluations.json` | `trainarr_training_evaluations` (filtered by `EvaluatedAt`) |
| `training_signoffs.json` | `trainarr_training_signoffs` (filtered by `SignedAt`) |
| `qualification_issues.json` | `trainarr_qualification_issues` (filtered by `UpdatedAt`) |
| `certification_publications.json` | `trainarr_certification_publications` (filtered by `PublishedAt`) |
| `person_training_history.json` | `trainarr_person_training_history_entries` (filtered by `OccurredAt`) |
| `manifest.json` | Package id, tenant, generated time, counts (ZIP only) |

## API + auth changes

### TrainArr user APIs (JWT + TrainArr entitlement)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/audit-packages/manifest` | read: `tenant_admin`, `trainarr_admin`, `trainarr_trainer`, platform admin |
| GET | `/api/audit-packages/export?format=zip` | export: `tenant_admin`, `trainarr_admin`, platform admin |
| GET | `/api/audit-packages/export?format=json` | export: same — structured JSON body |
| GET | `/api/audit-packages/export?from=&to=` | optional `DateTimeOffset` filters per section |

Successful exports write `audit_package.export` audit events with package id as target.

## Permission keys

- **Read manifest:** `trainarr` admin/trainer role gate (via `RequireAuditPackageRead`)
- **Export:** `trainarr.audit.export` role gate — `tenant_admin`, `trainarr_admin`, platform admin

## Frontend changes

- **AuditPackageExportPanel** on TrainArr settings — manifest sections, optional from/to dates, ZIP download, JSON preview with counts
- `canExportAuditPackage` — tenant admin, trainarr admin, platform admin
- `canReadAuditPackage` — also trainarr trainer (manifest/sections only)

## Worker / events

None (async audit package generation worker deferred to M12 follow-up).

## Tests

### Backend integration (`TrainArrAuditPackageTests`)

- `Audit_package_manifest_lists_sections`
- `Audit_package_export_zip_contains_json_files`
- `Audit_package_export_json_returns_structured_package`
- `Audit_package_export_writes_audit_event`
- `Audit_package_export_denies_trainer`
- `Audit_package_export_rejects_invalid_date_range`
- `Audit_package_export_date_filter_limits_assignments`

### Frontend unit

- `AuditPackageExportPanel.test.tsx`

## Verification commands

```powershell
dotnet build -c Release apps/trainarr-api/TrainArr.Api/TrainArr.Api.csproj
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~TrainArrAuditPackage"
cd apps/trainarr-frontend
npm run test
npm run build
```

## Remaining gaps

- Async audit package generation worker not started
- Audit timeline browse API not included (StaffArr W126 pattern deferred)
- Evidence file bytes not embedded in package (metadata only by design)
- Cross-product joint audit bundles (StaffArr + TrainArr) remain separate exports

## Next recommended slice

Per `02_PRODUCT_IMPLEMENTATION_BACKLOG.md`, next open **M12** items include TrainArr integration settings or other product M12 backlog rows.
