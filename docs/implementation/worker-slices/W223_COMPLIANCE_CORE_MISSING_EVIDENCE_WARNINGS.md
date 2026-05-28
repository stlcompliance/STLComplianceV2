# Worker 223 — Compliance Core M12 predictive missing-evidence warnings

## Slice name

M12 predictive missing-evidence warnings — analyze published rule packs and fact mirrors at scope to predict missing evidence; persisted runs and warnings, evaluate/list/summary APIs, Admin `MissingEvidenceWarningsPanel`, integration + Vitest tests

## Products touched

- **Compliance Core API** (`apps/compliancecore-api`): `MissingEvidenceWarningService`, `MissingEvidenceWarningRules`, `compliancecore_missing_evidence_warning_runs`, `compliancecore_missing_evidence_warnings`
- **Compliance Core Frontend** (`apps/compliancecore-frontend`): `MissingEvidenceWarningsPanel` on Admin workspace
- **Integration tests** (`tests/STLCompliance.ComplianceCore.Auth.Tests`): `ComplianceCoreMissingEvidenceWarningTests`

## Schema

### Migration `ComplianceCoreMissingEvidenceWarnings`

**`compliancecore_missing_evidence_warning_runs`**

| Column | Notes |
|--------|-------|
| id | PK |
| tenant_id | |
| scope_key | analyzed scope |
| packs_analyzed_count | |
| warnings_emitted_count | |
| highest_severity | low / medium / high / critical |
| evaluated_at | |

**`compliancecore_missing_evidence_warnings`**

| Column | Notes |
|--------|-------|
| id | PK |
| run_id | FK cascade |
| scope_key, pack_key, rule_pack_id, fact_key | |
| fact_definition_id | nullable |
| warning_type | `rule_pack_fact` / `catalog_requirement` |
| severity, reason_code | |
| has_mirror_at_scope | |
| is_required_in_rule, is_required_in_catalog | |
| summary | |

## Prediction model

For each published rule pack (latest per `pack_key`, max 25):

1. Collect fact keys from rule content and active catalog `fact_requirements` for the pack.
2. Resolve facts via `FactResolveService` and compare to `product_fact_mirrors` at scope.
3. Emit warnings when rule facts are unresolved or lack mirrors, or required catalog facts lack mirrors.
4. Severity from rule/catalog requirement, mirror presence, definition, and resolve outcome.

Reason codes: `missing_mirror`, `unresolved_fact`, `no_fact_definition`.

## API + auth

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/missing-evidence-warnings/summary` | entitled read |
| GET | `/api/missing-evidence-warnings` | read — `scopeKey`, `rulePackKey`, `severity`, `runId`, `limit` |
| POST | `/api/missing-evidence-warnings/evaluate` | `tenant_admin`, `compliance_admin`, `compliance_reviewer` |

## Audit

- `missing_evidence_warnings.evaluate`

## Frontend

- **MissingEvidenceWarningsPanel** — summary tiles, scope/pack/context, evaluate, latest warnings list
- `canEvaluateMissingEvidence` (same roles as risk scoring / audit export)

## Tests

### Backend (`ComplianceCoreMissingEvidenceWarningTests`)

- `Missing_evidence_evaluate_list_and_summary_for_pack_without_mirror`
- `Missing_evidence_catalog_requirement_without_mirror_emits_warning`
- `Missing_evidence_evaluate_denies_tenant_member`
- `Missing_evidence_list_allowed_for_tenant_member`

### Frontend (`MissingEvidenceWarningsPanel.test.tsx`)

- Evaluator controls and warnings list
- Read-only message for non-evaluators

## Verification

```powershell
dotnet build -c Release apps/compliancecore-api/ComplianceCore.Api/ComplianceCore.Api.csproj
dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~MissingEvidence"
cd apps/compliancecore-frontend
npm run test -- --run MissingEvidence
npm run build
```

## Remaining gaps

- No scheduled warning scan worker (on-demand evaluate only)
- Does not ingest mirrors; relies on existing product fact mirror + source ingestion paths
- Control effectiveness tracking and readiness forecasting remain separate M12 items

## Next recommended slice

**Compliance Core M12** control effectiveness tracking or readiness forecasting; **NexArr M12** audit export enhancements; **RoutArr M12** audit package export per `00_SLICE_STATE.md`.
