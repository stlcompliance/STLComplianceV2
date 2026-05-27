# Worker 40 — TrainArr rule-pack requirement intake

## Slice name

M6/M10 rule-pack requirement intake — local reference table linking training definitions/programs to Compliance Core rule pack keys, CRUD APIs with JWT auth, optional Compliance Core validation via service token, auto-resolve rulePackKey in qualification-check/batch, trainarr-frontend requirement panels, cross-product tests

## Products touched

- **TrainArr API** (`apps/trainarr-api`): `trainarr_training_rule_pack_requirements`, `TrainingRulePackRequirementService`, nested `/api/{entity}/rule-pack-requirements` routes
- **Compliance Core API** (`apps/compliancecore-api`): `POST /api/internal/rule-packs/lookup` with `compliancecore.rulepacks.read` service scope
- **TrainArr Frontend** (`apps/trainarr-frontend`): `RulePackRequirementPanel` on definition/program builder selection
- **Integration tests**: `StaffArrTrainArrRulePackRequirementTests`

## API + auth changes

### TrainArr user API (JWT + TrainArr entitlement)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/training-definitions/{id}/rule-pack-requirements?includeMetadata=` | definition read |
| PUT | `/api/training-definitions/{id}/rule-pack-requirements?validateWithComplianceCore=` | definition manage |
| DELETE | `/api/training-definitions/{id}/rule-pack-requirements/{requirementId}` | definition manage |
| GET/PUT/DELETE | `/api/training-programs/{id}/rule-pack-requirements` | program read / manage |

Request body: `rulePackKey` (opaque Compliance Core key).

Response: requirement id, entity reference, rule pack key, optional `metadata` (label, program, version, status) when Compliance Core lookup succeeds.

Audit: `rule_pack_requirement.create` / `update` / `remove`.

### Qualification check auto-resolve

`POST /api/qualification-checks` and `POST /api/qualification-checks/batch` accept optional `trainingDefinitionId` / `trainingProgramId`. When `rulePackKey` is omitted, TrainArr resolves from linked requirements (definition → program → qualification-key match).

### Compliance Core internal API (service token)

| Method | Route | Scope |
|--------|-------|-------|
| POST | `/api/internal/rule-packs/lookup` | `compliancecore.rulepacks.read` |

Body: `{ tenantId, rulePackKeys[] }` (max 200 keys).

### Configuration

`ComplianceCore:ServiceToken` on TrainArr must include `compliancecore.rulepacks.read` (in addition to `compliancecore.rules.evaluate`) for validation/enrichment.

## Frontend changes

- **ProgramBuilderPanel** — existing definition/program selection drives requirement panels
- **RulePackRequirementPanel** — save rule pack key, optional validate checkbox, list with metadata, remove
- **RemediationAssignmentPanel / batch check** — pass `trainingDefinitionId`; rule pack override remains optional

## Tests

### Cross-product (`StaffArrTrainArrRulePackRequirementTests`)

- `Training_definition_rule_pack_upsert_list_remove_with_metadata`
- `Training_program_rule_pack_requirement_persists_reference_only`
- `Training_definition_rule_pack_denies_member_role`
- `Training_definition_rule_pack_rejects_unknown_pack_when_validating`
- `Rule_pack_requirement_writes_audit_event`
- `Qualification_check_resolves_rule_pack_from_definition_requirement`

### Frontend unit

- `RulePackRequirementPanel.test.tsx`

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~RulePackRequirement"
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~QualificationCheck"
cd apps/trainarr-frontend
npm install
npm run test -- --run
npm run build
```

## Remaining gaps

- No rule pack picker/search against Compliance Core catalog in TrainArr UI (manual key entry)
- Multiple requirements per entity: qualification check uses first linked key only
- Program-level inheritance to member definitions not automatic for checks without explicit ids

## Next recommended slice

**Compliance Core cross-product batch evaluate API** (M5/M10) or **TrainArr rule change impact** per milestone priority.
