# Worker 26 — Compliance Core regulatory mappings

## Slice name

M5 regulatory mappings — tenant-scoped mappings linking compliance keys and material keys to regulatory programs, rule packs, citations, and fact definitions; read/manage APIs with JWT auth; admin UI tab; audit events; integration and frontend tests

## Products touched

- **Compliance Core API** (`apps/compliancecore-api`): regulatory mappings, audit events
- **Compliance Core Frontend** (`apps/compliancecore-frontend`): Regulatory mappings tab with seed workflow
- **Compliance Core integration tests** (`tests/STLCompliance.ComplianceCore.Auth.Tests`): regulatory mapping auth and CRUD tests

## Schema

### Compliance Core migration `ComplianceCoreRegulatoryMappings`

- `compliancecore_regulatory_mappings` — tenant-scoped mappings with `target_kind` (`compliance_key` | `material_key`), required regulatory program FK, optional rule pack / citation / fact definition FKs, and exactly one compliance key or material key FK

## API + auth changes

### Compliance Core user APIs (JWT + Compliance Core entitlement)

| Method | Route | Auth |
|--------|-------|------|
| GET/POST | `/api/regulatory-mappings` | read: entitled users; create: `tenant_admin`, `compliance_admin` |

Query filters: `regulatoryProgramId`, `rulePackId`, `citationId`, `complianceKeyId`, `materialKeyId`.

Create validation: target kind must match the provided key FK; rule pack and citation must belong to the selected program.

## Frontend changes

- Home page fourth tab: **Regulatory mappings**
- **RegulatoryMappingsPanel** — lists mappings from real APIs
- Admin seed action creates sample `vehicle_inspection` compliance key mapping to FMCSA driver qualification rule pack (seeds regulatory chain and key when empty)

## Tests

### Backend integration (`ComplianceCoreRegulatoryMappingsTests`)

- `Regulatory_mapping_create_list_and_filter`
- `Regulatory_mapping_material_key_target`
- `Regulatory_mapping_create_denies_member_role`
- `Regulatory_mapping_requires_target_key`
- `Regulatory_mapping_read_requires_compliancecore_entitlement`

### Frontend unit

- `RegulatoryMappingsPanel.test.tsx` — empty state, mapping row rendering, seed button

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

- Rule version content, any/all/none rule builder, and evaluation APIs not wired
- Fact source registry and 9-CSV import/export deferred
- Internal resolve/validate APIs and regulatory mapping validation worker deferred
- Cross-product mapping consumption not integrated

## Next recommended slice

**TrainArr program builder / evidence capture** (M6) or **Compliance Core rule version content + evaluation foundations** (M5) per dependency priority.
