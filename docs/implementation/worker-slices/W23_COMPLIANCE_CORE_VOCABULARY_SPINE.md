# Worker 23 — Compliance Core controlled vocabulary spine

## Slice name

M5 vocabulary and key spine — 14 controlled vocabulary type keys, vocabulary terms, alias mapping, compliance keys, material keys, read/manage APIs, JWT user auth, vocabulary admin UI, integration tests

## Products touched

- **Compliance Core API** (`apps/compliancecore-api`): vocabulary types/terms/aliases, compliance keys, material keys, audit events, user auth spine
- **Compliance Core Frontend** (`apps/compliancecore-frontend`): handoff shell, vocabulary registry panel with term/key lists and admin seed action
- **NexArr** (launch profile): Compliance Core frontend launch URL on port 5177
- **Compliance Core integration tests** (`tests/STLCompliance.ComplianceCore.Auth.Tests`): vocabulary spine auth and CRUD tests

## Schema

### Compliance Core migration `ComplianceCoreVocabularySpine`

- `compliancecore_vocabulary_types` — system registry of 14 controlled vocabulary type keys (global, seeded)
- `compliancecore_vocabulary_terms` — tenant-scoped controlled vocabulary terms
- `compliancecore_vocabulary_aliases` — tenant-scoped alias mappings to vocabulary terms
- `compliancecore_compliance_keys` — tenant-scoped compliance keys
- `compliancecore_material_keys` — tenant-scoped material keys
- `compliancecore_audit_events` — tenant-scoped audit trail for vocabulary/key mutations

### 14 controlled vocabulary type keys (seeded)

`material_hazard`, `physical_state`, `compliance_domain`, `certification_category`, `readiness_blocker`, `incident_reason`, `training_requirement`, `inspection_category`, `defect_severity`, `dispatch_category`, `dvir_reason`, `route_exception`, `vendor_compliance`, `evidence_type`

## API + auth changes

### Compliance Core user APIs (JWT + Compliance Core entitlement)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/auth/handoff/redeem` | Anonymous |
| GET | `/api/session`, `/api/me` | JWT + Compliance Core entitlement |
| GET | `/api/vocabulary/types` | read: entitled users |
| GET/POST | `/api/vocabulary` | read: entitled; create: `tenant_admin`, `compliance_admin` |
| POST | `/api/vocabulary/aliases` | `tenant_admin`, `compliance_admin` |
| GET/POST | `/api/compliance-keys` | read: entitled; create: `tenant_admin`, `compliance_admin` |
| GET/POST | `/api/material-keys` | read: entitled; create: `tenant_admin`, `compliance_admin` |

Role keys map to permission keys `compliancecore.vocabulary.manage` and `compliancecore.keys.manage` per `21_PERMISSION_KEYS_AND_DEFAULT_ROLES.md`.

## Frontend changes

- New **Compliance Core frontend** app on port 5177 with NexArr handoff redeem
- **Vocabulary panel** — lists 14 vocabulary types, filtered terms, compliance keys, material keys
- Admin seed action creates sample term plus baseline compliance/material keys when empty

## Tests

### Backend integration (`ComplianceCoreVocabularySpineTests`)

- `Vocabulary_types_returns_fourteen_controlled_keys`
- `Vocabulary_term_create_and_list_with_alias`
- `Compliance_key_create_denies_member_role`
- `Material_key_create_and_list_for_compliance_admin`
- `Vocabulary_read_requires_compliancecore_entitlement`

### Frontend unit

- `VocabularyPanel.test.tsx` — empty state, term/key row rendering, admin button

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

- Governing body, jurisdiction, and regulatory program registries remain future M5 slices
- Rule packs, citations, fact catalog, 9-CSV import/export not implemented
- Internal resolve/validate APIs not wired
- Cross-product vocabulary consumption (StaffArr reason codes from Compliance Core) not integrated

## Next recommended slice

**Compliance Core regulatory registries + rule pack foundations** (M5 continuation) or **TrainArr program builder / evidence capture** (M6) per dependency priority.
