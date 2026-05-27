# Worker 37 — Compliance Core audit package export

## Slice name

M5/M12 audit package export — tenant audit events, findings, evaluation runs, and rule pack metadata in ZIP or JSON; optional date range filters; read/export auth; compliancecore-frontend export panel; audit event on generation; integration and frontend tests

## Products touched

- **Compliance Core API** (`apps/compliancecore-api`): `AuditPackageService`, `/api/audit-packages` endpoints
- **Compliance Core Frontend** (`apps/compliancecore-frontend`): Audit package tab with date range + ZIP/JSON download
- **Integration tests** (`tests/STLCompliance.ComplianceCore.Auth.Tests`): `ComplianceCoreAuditPackageTests`

## Schema

No new tables — exports from existing `compliancecore_audit_events`, `compliancecore_findings`, `compliancecore_rule_evaluation_runs`, `compliancecore_rule_packs` (+ regulatory program join for `program_key`).

## Audit package contents (per featureset)

| Section | Source |
|---------|--------|
| `audit_events.json` | `compliancecore_audit_events` (filtered by `OccurredAt` when date range set) |
| `findings.json` | `compliancecore_findings` (filtered by `CreatedAt`) |
| `evaluation_runs.json` | `compliancecore_rule_evaluation_runs` (filtered by `CreatedAt`) |
| `rule_packs.json` | All tenant rule packs with program metadata (full snapshot) |
| `manifest.json` | Package id, tenant, generated time, counts (ZIP only) |

## API + auth changes

### Compliance Core user APIs (JWT + Compliance Core entitlement)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/audit-packages/manifest` | read: entitled users with findings read (`compliance_reviewer`, `compliance_admin`, `tenant_admin`, `tenant_member`) |
| GET | `/api/audit-packages/export?format=zip` | export: `tenant_admin`, `compliance_admin`, `compliance_reviewer` (default ZIP) |
| GET | `/api/audit-packages/export?format=json` | export: same — structured JSON body |
| GET | `/api/audit-packages/export?from=&to=` | optional `DateTimeOffset` filters on audit events / findings / evaluations |

Successful exports write `audit_package.export` audit events with package id as target.

## Frontend changes

- Home page ninth tab: **Audit package**
- **AuditPackageExportPanel** — manifest sections, optional from/to dates, ZIP download, JSON preview with counts
- `canExportAuditPackage` — tenant admin, compliance admin, compliance reviewer

## Tests

### Backend integration (`ComplianceCoreAuditPackageTests`)

- `Audit_package_manifest_lists_sections`
- `Audit_package_export_zip_contains_json_files`
- `Audit_package_export_json_returns_structured_package`
- `Audit_package_export_writes_audit_event`
- `Audit_package_export_denies_tenant_member`
- `Audit_package_export_rejects_invalid_date_range`
- `Audit_package_export_date_filter_limits_audit_events`

### Frontend unit

- `AuditPackageExportPanel.test.tsx`

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj" -c Release
dotnet test "tests/STLCompliance.Health.Tests/STLCompliance.Health.Tests.csproj" -c Release --filter "FullyQualifiedName~ComplianceCore"
cd apps/compliancecore-frontend
npm install
npm run test -- --run
npm run build
```

## Remaining gaps

- Cross-product audit packages (StaffArr + TrainArr joint export) remain M12 / separate products
- Async audit package generation worker deferred
- Waivers and workflow gate check results not yet included in package sections

## Next recommended slice

**TrainArr citation attachment** (M10) or **Compliance Core batch workflow gate checks** per milestone priority.
