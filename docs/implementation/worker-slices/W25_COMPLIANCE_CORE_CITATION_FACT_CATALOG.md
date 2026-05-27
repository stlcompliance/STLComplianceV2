# Worker 25 — Compliance Core citation registry + fact catalog foundations

## Slice name

M5 citation registry and fact catalog foundations — regulatory citations linked to programs/rule packs with versioning and supersession; fact definitions and requirements linked to rule packs or citations; read/manage APIs with JWT auth; admin UI tab; audit events; integration and frontend tests

## Products touched

- **Compliance Core API** (`apps/compliancecore-api`): regulatory citations, fact definitions, fact requirements, audit events
- **Compliance Core Frontend** (`apps/compliancecore-frontend`): Citations & facts tab with seed workflow
- **Compliance Core integration tests** (`tests/STLCompliance.ComplianceCore.Auth.Tests`): citation/fact catalog auth and CRUD tests

## Schema

### Compliance Core migration `ComplianceCoreCitationFactCatalog`

- `compliancecore_regulatory_citations` — tenant-scoped citations linked to regulatory programs and optional rule packs; version number per citation key; optional supersession FK
- `compliancecore_fact_definitions` — tenant-scoped fact catalog entries with value type (`string`, `boolean`, `number`, `date`)
- `compliancecore_fact_requirements` — tenant-scoped requirements linking facts to rule packs and/or citations

## API + auth changes

### Compliance Core user APIs (JWT + Compliance Core entitlement)

| Method | Route | Auth |
|--------|-------|------|
| GET/POST | `/api/citations` | read: entitled users; create: `tenant_admin`, `compliance_admin` |
| GET/POST | `/api/fact-definitions` | read: entitled users; create: `tenant_admin`, `compliance_admin` |
| GET/POST | `/api/fact-requirements` | read: entitled users; create: `tenant_admin`, `compliance_admin` |

Query filters: `regulatoryProgramId`, `rulePackId` on citations; `rulePackId`, `citationId` on fact requirements.

## Frontend changes

- Home page third tab: **Citations & facts**
- **CitationFactCatalogPanel** — lists citations, fact definitions, and fact requirements from real APIs
- Admin seed action creates sample CFR citation, driver license fact, and rule-pack-linked requirement (seeds regulatory chain when empty)

## Tests

### Backend integration (`ComplianceCoreCitationFactCatalogTests`)

- `Citation_create_list_and_versioning`
- `Fact_catalog_create_and_link_to_rule_pack`
- `Citation_create_denies_member_role`
- `Fact_requirement_create_requires_rule_pack_or_citation`
- `Citation_read_requires_compliancecore_entitlement`

### Frontend unit

- `CitationFactCatalogPanel.test.tsx` — empty state, catalog row rendering, seed button

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

- Regulatory mappings, rule version content, and rule evaluation not implemented
- Fact source registry and full citation supersession workflows deferred
- 9-CSV import/export and internal resolve/validate APIs deferred
- Cross-product citation/fact consumption not integrated

## Next recommended slice

**Compliance Core regulatory mappings** (M5 continuation) or **TrainArr program builder / evidence capture** (M6) per dependency priority.
