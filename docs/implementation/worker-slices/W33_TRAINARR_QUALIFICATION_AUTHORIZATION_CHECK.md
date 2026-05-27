# Worker 33 — TrainArr qualification authorization check API

## Slice name

M6 qualification authorization check — `POST /api/qualification-checks` orchestrating TrainArr local qualification state with Compliance Core internal rule evaluation, JWT auth for TrainArr users, service tokens for Compliance Core, remediation assignment UI gate, cross-product tests

## Products touched

- **Compliance Core API** (`apps/compliancecore-api`): `POST /api/internal/evaluate` (service token scope `compliancecore.rules.evaluate`)
- **TrainArr API** (`apps/trainarr-api`): qualification check orchestration, Compliance Core HTTP client
- **TrainArr Frontend** (`apps/trainarr-frontend`): authorization check panel before remediation assignment create
- **Integration tests**: `ComplianceCoreInternalRuleEvaluationTests`, `StaffArrTrainArrQualificationCheckTests`

## API + auth changes

### TrainArr user API (JWT + TrainArr entitlement)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/qualification-checks` | `RequireQualificationChecks` — same read scope as training assignments (admin/trainer or self person) |

Request: `staffarrPersonId`, `qualificationKey`, optional `rulePackKey`, optional `context` for Compliance Core fact resolution.

Response: `outcome` (`allow` \| `warn` \| `block`), `reasonCode`, `message`, `localQualification`, `complianceCore` summary.

TrainArr merges local `trainarr_qualification_issues` status with Compliance Core evaluation (strictest outcome wins). No rule evaluation logic duplicated in TrainArr.

### Compliance Core internal API (NexArr service token → Compliance Core)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/internal/evaluate` | service token → `compliancecore`, scope `compliancecore.rules.evaluate` |

Resolves facts from rule pack content via fact source registry, evaluates rules, maps `pass` → `allow`, `fail` → `block`, unresolved facts → `warn`.

### Configuration

`apps/trainarr-api/TrainArr.Api/appsettings.json`:

```json
"ComplianceCore": {
  "BaseUrl": "http://localhost:5107",
  "ServiceToken": ""
}
```

## Frontend changes

- **QualificationCheckPanel** — run check, show allow/warn/block outcome with local + Compliance Core detail
- **RemediationAssignmentPanel** — check before assignment create; blocks create when outcome is `block`

## Tests

### Compliance Core (`ComplianceCoreInternalRuleEvaluationTests`)

- `Internal_evaluate_resolves_facts_and_returns_allow`
- `Internal_evaluate_rejects_missing_service_token`

### Cross-product (`StaffArrTrainArrQualificationCheckTests`)

- `Qualification_check_warns_without_local_qualification_when_rules_pass`
- `Qualification_check_blocks_when_compliance_rules_fail`
- `Qualification_check_blocks_suspended_local_qualification`
- `Qualification_check_denies_unrelated_member_for_other_person`

### Frontend unit

- `QualificationCheckPanel.test.tsx`
- `RemediationAssignmentPanel.test.tsx` (updated props)

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj" -c Release
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~QualificationCheck"
cd apps/trainarr-frontend
npm install
npm run test -- --run
npm run build
```

## Remaining gaps

- Training definition → rule pack key linkage not persisted (callers pass `rulePackKey` on check)
- Compliance Core findings / workflow gates deferred
- Product API fact fetch adapters still context-only

## Next recommended slice

**Compliance Core findings + workflow gate API** (M5) or **TrainArr batch qualification checks** (M10) per milestone priority.
