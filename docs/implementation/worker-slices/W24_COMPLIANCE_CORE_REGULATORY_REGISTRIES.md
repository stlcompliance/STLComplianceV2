# Worker 24 — Compliance Core regulatory registries + rule pack foundations

## Slice name

M5 regulatory registries and rule pack foundations — governing body, jurisdiction, and regulatory program registries; rule pack entity with version/status and regulatory program linkage; read/manage APIs with JWT auth; regulatory admin UI; audit events; integration and frontend tests

## Products touched

- **Compliance Core API** (`apps/compliancecore-api`): governing bodies, jurisdictions, regulatory programs, rule packs, audit events, authorization extensions
- **Compliance Core Frontend** (`apps/compliancecore-frontend`): regulatory registry panel with seed workflow and rule pack status actions; tabbed home page
- **Compliance Core integration tests** (`tests/STLCompliance.ComplianceCore.Auth.Tests`): regulatory registry and rule pack auth/CRUD tests

## Schema

### Compliance Core migration `ComplianceCoreRegulatoryRegistries`

- `compliancecore_governing_bodies` — tenant-scoped governing body registry
- `compliancecore_jurisdictions` — tenant-scoped jurisdictions linked to governing bodies
- `compliancecore_regulatory_programs` — tenant-scoped regulatory programs linked to jurisdictions
- `compliancecore_rule_packs` — tenant-scoped rule packs linked to regulatory programs with version number and lifecycle status

### Rule pack lifecycle statuses

`draft`, `review`, `published`, `archived`

New rule packs start at version 1 (or next version for the same pack key) in `draft` status.

## API + auth changes

### Compliance Core user APIs (JWT + Compliance Core entitlement)

| Method | Route | Auth |
|--------|-------|------|
| GET/POST | `/api/governing-bodies` | read: entitled users; create: `tenant_admin`, `compliance_admin` |
| GET/POST | `/api/jurisdictions` | read: entitled users; create: `tenant_admin`, `compliance_admin` |
| GET/POST | `/api/regulatory-programs` | read: entitled users; create: `tenant_admin`, `compliance_admin` |
| GET/POST | `/api/rule-packs` | read: entitled users; create: `tenant_admin`, `compliance_admin` (`compliancecore.rulepacks.create`) |
| PATCH | `/api/rule-packs/{id}/status` | review: `compliancecore.rulepacks.create`; publish: `compliancecore.rulepacks.publish`; archive: registry manage |

Role keys map to permission keys per `21_PERMISSION_KEYS_AND_DEFAULT_ROLES.md`.

## Frontend changes

- Home page tab navigation: **Vocabulary & keys** and **Regulatory & rule packs**
- **Regulatory registry panel** — lists governing bodies, jurisdictions, programs, and rule packs from real APIs
- Admin seed action creates sample DOT → US Federal → FMCSA → driver qualification rule pack chain when empty
- Rule pack draft → review → publish actions wired to status PATCH API

## Tests

### Backend integration (`ComplianceCoreRegulatoryRegistriesTests`)

- `Regulatory_registry_chain_create_and_list`
- `Rule_pack_create_list_and_publish_lifecycle`
- `Rule_pack_create_denies_member_role`
- `Governing_body_create_denies_member_role`
- `Regulatory_read_requires_compliancecore_entitlement`

### Frontend unit

- `RegulatoryRegistryPanel.test.tsx` — empty state, registry row rendering, seed and status action buttons

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

- Citation registry, fact catalog, and regulatory mappings not implemented
- Rule version content, any/all/none rule builder, and evaluation APIs not wired
- 9-CSV import/export and internal resolve/validate APIs deferred
- Cross-product rule pack consumption not integrated

## Next recommended slice

**Compliance Core citation registry + fact catalog foundations** (M5 continuation) or **TrainArr program builder / evidence capture** (M6) per dependency priority.
