# Worker 224 — Compliance Core M12 control effectiveness tracking

## Slice name

M12 control effectiveness tracking — measure published rule packs as compliance controls at scope via internal rule evaluation; persisted runs and records, evaluate/list/summary APIs, Admin `ControlEffectivenessPanel`, integration + Vitest tests

## Products touched

- **Compliance Core API** (`apps/compliancecore-api`): `ControlEffectivenessService`, `ControlEffectivenessRules`, `compliancecore_control_effectiveness_runs`, `compliancecore_control_effectiveness_records`
- **Compliance Core Frontend** (`apps/compliancecore-frontend`): `ControlEffectivenessPanel` on Admin workspace
- **Integration tests** (`tests/STLCompliance.ComplianceCore.Auth.Tests`): `ComplianceCoreControlEffectivenessTests`

## Schema

### Migration `ComplianceCoreControlEffectiveness`

**`compliancecore_control_effectiveness_runs`**

| Column | Notes |
|--------|-------|
| id | PK |
| tenant_id | |
| scope_key | |
| packs_evaluated_count | |
| lowest_effectiveness_score | weakest control (0–100) |
| lowest_effectiveness_level | |
| average_effectiveness_score | |
| evaluated_at | |

**`compliancecore_control_effectiveness_records`**

| Column | Notes |
|--------|-------|
| id | PK |
| run_id | FK cascade |
| scope_key, pack_key, rule_pack_id | |
| effectiveness_score | 0–100 (higher = more effective) |
| effectiveness_level | effective / partially_effective / ineffective / unknown |
| control_status | operating / degraded / failing / unknown |
| rule_outcome, evaluation_result | |
| total/passed/failed rule counts | |
| unresolved/resolved fact counts | |
| summary | |

## Effectiveness model

For each published rule pack (latest per `pack_key`, max 25):

1. `InternalRuleEvaluationService.EvaluateAsync` at scope.
2. Score 0–100 from outcome, pass/fail, unresolved facts, and rule pass rate.
3. Level: ≥80 effective, ≥55 partially_effective, ≥30 ineffective, else unknown.
4. Control status: operating / degraded / failing mapped from level.

## API + auth

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/control-effectiveness/summary` | entitled read |
| GET | `/api/control-effectiveness` | read — `scopeKey`, `rulePackKey`, `effectivenessLevel`, `runId`, `limit` |
| POST | `/api/control-effectiveness/evaluate` | admin / compliance_admin / compliance_reviewer |

## Audit

- `control_effectiveness.evaluate`

## Frontend

- **ControlEffectivenessPanel** — average/effective/partial/ineffective/weakest tiles, evaluate form, records list
- `canEvaluateControlEffectiveness` (same roles as risk scoring)

## Tests

### Backend (`ComplianceCoreControlEffectivenessTests`)

- `Control_effectiveness_evaluate_list_and_summary_for_effective_pack`
- `Control_effectiveness_evaluate_without_fact_source_yields_low_score`
- `Control_effectiveness_evaluate_denies_tenant_member`
- `Control_effectiveness_list_allowed_for_tenant_member`

### Frontend (`ControlEffectivenessPanel.test.tsx`)

- Evaluator controls and records list
- Read-only message for non-evaluators

## Verification

```powershell
dotnet build -c Release apps/compliancecore-api/ComplianceCore.Api/ComplianceCore.Api.csproj
dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~ControlEffectiveness"
cd apps/compliancecore-frontend
npm run test -- --run ControlEffectiveness
npm run build
```

## Remaining gaps

- No scheduled effectiveness scan worker (on-demand evaluate only)
- No historical trend aggregation across runs (latest run list/summary only)
- Readiness forecasting remains a separate M12 item

## Next recommended slice

**Compliance Core M12** readiness forecasting; **NexArr M12** audit export enhancements; **RoutArr M12** audit package export per `00_SLICE_STATE.md`.
