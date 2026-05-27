# Worker 32 — Compliance Core fact source registry + internal resolve API

## Slice name

M5 fact source registry + internal resolve/validate — tenant-scoped fact sources linked to catalog definitions, static_config and product_api source types, service-token internal APIs, admin CRUD, admin UI tab, audit events, integration and frontend tests

## Products touched

- **Compliance Core API** (`apps/compliancecore-api`): `compliancecore_fact_sources`, fact resolve/validate services, internal endpoints
- **Compliance Core Frontend** (`apps/compliancecore-frontend`): Fact sources tab with seed workflow
- **Compliance Core integration tests** (`tests/STLCompliance.ComplianceCore.Auth.Tests`): fact source registry and internal API tests with NexArr-issued service tokens

## Schema

### Compliance Core migration `ComplianceCoreFactSourceRegistry`

- `compliancecore_fact_sources` — tenant-scoped sources linking fact definitions to source type, optional product key/reference, JSONB config, priority ordering

Source types:

- `static_config` — resolves typed values from config JSON (`booleanValue`, `stringValue`, `numberValue`, `dateValue`)
- `product_api` — extension point; resolves from caller `context` map until product fetch adapters are added

## API + auth changes

### Compliance Core user APIs (JWT + Compliance Core entitlement)

| Method | Route | Auth |
|--------|-------|------|
| GET/POST | `/api/fact-sources` | read: entitled users; create: `tenant_admin`, `compliance_admin` |

### Internal service APIs (NexArr service token → Compliance Core)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/internal/resolve` | service token with target `compliancecore`, scopes `compliancecore.facts.resolve` |
| POST | `/api/internal/validate` | service token with target `compliancecore`, scopes `compliancecore.facts.validate` |

Resolve walks active sources by priority. Validate reports per-key resolvability without caller context (product_api sources require context at resolve time).

## Frontend changes

- Home page sixth tab: **Fact sources**
- **FactSourcesPanel** — lists catalog facts and registered sources; seed creates static_config + product_api sample sources
- Header rollup includes fact source count

## Tests

### Backend integration (`ComplianceCoreFactSourceRegistryTests`)

- `Fact_source_create_list_and_internal_resolve_static_config`
- `Internal_resolve_uses_context_for_product_api_source`
- `Internal_validate_reports_missing_catalog_and_sources`
- `Internal_resolve_rejects_missing_service_token`
- `Fact_source_create_denies_member_role`

### Frontend unit

- `FactSourcesPanel.test.tsx` — empty state, source row rendering, seed button

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

- Product API fetch adapters (StaffArr/TrainArr HTTP clients) not implemented — context overrides only
- Rule evaluation does not yet call internal resolve for missing facts
- 9-CSV import/export, findings, audit packages deferred
- Additional source types (computed, event snapshot) deferred

## Next recommended slice

**TrainArr qualification authorization check API** (M6) or **Compliance Core findings + workflow gate API** (M5) per milestone priority.
