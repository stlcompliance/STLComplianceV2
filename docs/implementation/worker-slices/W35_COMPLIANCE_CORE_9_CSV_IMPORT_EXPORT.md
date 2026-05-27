# Worker 35 — Compliance Core 9-CSV import/export

## Slice name

M5 9-CSV bundle lifecycle — export nine CSV files from tenant vocabulary/keys/rule packs/citations/fact requirements/mappings/SDS references; validate and upsert imports with transaction + audit; read/manage APIs; compliancecore-frontend CSV bundle tab; integration and frontend tests

## Products touched

- **Compliance Core API** (`apps/compliancecore-api`): `CsvImportExportService`, SDS reference entity, CSV bundle endpoints
- **Compliance Core Frontend** (`apps/compliancecore-frontend`): CSV bundle tab with export ZIP and import panel
- **Integration tests** (`tests/STLCompliance.ComplianceCore.Auth.Tests`): `ComplianceCoreCsvBundleTests`

## Schema

### Compliance Core migration `ComplianceCoreCsvBundleSdsReferences`

- `compliancecore_sds_references` — tenant-scoped SDS/HazCom reference rows keyed by `sds_key`, optional `material_key` FK

## 9-CSV bundle (per `docs/22_CONTROLLED_VOCABULARY_AND_COMPLIANCE_KEYS.md`)

| File | Entity coverage |
|------|-----------------|
| `controlled_vocabulary.csv` | Vocabulary terms |
| `vocabulary_aliases.csv` | Term aliases |
| `compliance_keys.csv` | Compliance keys |
| `material_keys.csv` | Material keys |
| `rule_packs.csv` | Rule packs (references existing `program_key`) |
| `rule_requirements.csv` | Regulatory citations |
| `rule_fact_requirements.csv` | Fact requirements |
| `regulatory_mappings.csv` | Regulatory mappings |
| `sds_references.csv` | SDS references |

Regulatory programs/jurisdictions/governing bodies are **not** in the 9-CSV bundle; imports reference existing `program_key` values seeded via regulatory registry APIs.

## API + auth changes

### Compliance Core user APIs (JWT + Compliance Core entitlement)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/csv-bundle/manifest` | read: entitled users |
| GET | `/api/csv-bundle/export` | read: entitled users — ZIP of all nine files |
| GET | `/api/csv-bundle/files/{fileName}` | read: entitled users — single CSV |
| POST | `/api/csv-bundle/import?dryRun=` | manage: `tenant_admin`, `compliance_admin` — multipart CSV or ZIP |

Import applies files in dependency order inside a relational DB transaction (in-memory test DB skips transaction wrapper). Successful imports write `csv_bundle.import` audit events.

## Frontend changes

- Home page eighth tab: **CSV bundle**
- **CsvImportExportPanel** — manifest list, ZIP export download, dry-run/apply import with issue summary

## Tests

### Backend integration (`ComplianceCoreCsvBundleTests`)

- `Csv_bundle_manifest_lists_nine_files`
- `Csv_bundle_export_zip_contains_nine_csv_files`
- `Csv_bundle_import_round_trip_upserts_keys`
- `Csv_bundle_import_denies_tenant_member`
- `Csv_bundle_import_dry_run_reports_validation_without_apply`

### Frontend unit

- `CsvImportExportPanel.test.tsx`

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

- Dedicated `/api/sds` and `/api/hazcom` CRUD APIs deferred (SDS rows importable via 9-CSV)
- Fact definitions not included in 9-CSV (referenced by key in `rule_fact_requirements.csv`)
- Audit package export and batch workflow gate checks still deferred

## Next recommended slice

**TrainArr batch qualification checks** (M10) or **Compliance Core audit package export** (M5/M12) per milestone priority.
