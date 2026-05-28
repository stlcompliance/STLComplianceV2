# Worker 222 — Compliance Core M12 risk scoring

## Slice name

M12 risk scoring — score published rule packs per tenant scope using product fact mirrors and internal rule evaluation; persisted runs and scores, evaluate/list/summary APIs, Admin `RiskScoringPanel`, integration + Vitest tests

## Products touched

- **Compliance Core API** (`apps/compliancecore-api`): `RiskScoringService`, `RiskScoringRules`, `compliancecore_risk_score_runs`, `compliancecore_risk_scores`
- **Compliance Core Frontend** (`apps/compliancecore-frontend`): `RiskScoringPanel` on Admin workspace
- **Integration tests** (`tests/STLCompliance.ComplianceCore.Auth.Tests`): `ComplianceCoreRiskScoringTests`

## Schema

### Migration `ComplianceCoreRiskScoring`

**`compliancecore_risk_score_runs`**

| Column | Notes |
|--------|-------|
| id | PK |
| tenant_id | |
| scope_key | evaluated scope (e.g. `tenant`, `purchase_request:{guid}`) |
| packs_evaluated_count | |
| highest_risk_score | 0–100 |
| highest_risk_level | low / medium / high / critical |
| evaluated_at | |

**`compliancecore_risk_scores`**

| Column | Notes |
|--------|-------|
| id | PK |
| run_id | FK cascade |
| scope_key, pack_key, rule_pack_id | |
| risk_score_value | 0–100 |
| risk_level | derived from score |
| rule_outcome, evaluation_result | from internal evaluate |
| unresolved_fact_count, failed_rule_count, resolved_fact_count | |
| mirror_fact_count | mirrors at scope when evaluated |
| summary | human-readable |

## Scoring model

For each published rule pack (latest version per `pack_key`, max 25 per request):

1. `InternalRuleEvaluationService.EvaluateAsync` resolves facts (including `product_mirror` rows) and evaluates rule content.
2. Numeric score (0–100) from outcome, unresolved facts, failed rules, and missing mirrors at scope.
3. Risk level: ≤25 low, ≤50 medium, ≤75 high, else critical.

## API + auth

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/risk-scores/summary` | entitled read (`RequireRiskScoringRead`) |
| GET | `/api/risk-scores` | read — filters: `scopeKey`, `rulePackKey`, `runId`, `limit` (defaults to latest run) |
| POST | `/api/risk-scores/evaluate` | `tenant_admin`, `compliance_admin`, `compliance_reviewer` |

Evaluate body: `{ "scopeKey?", "rulePackKey?", "context?" }` — `context` drives scope resolution (e.g. `purchase_request_id`) and fact resolve.

## Audit

- `risk_scores.evaluate` — per evaluation run

## Frontend

- **RiskScoringPanel** — summary tiles, scope/pack/context inputs, evaluate button, latest scores list
- `canEvaluateRisk` from session role (same as audit export reviewers)

## Tests

### Backend (`ComplianceCoreRiskScoringTests`)

- `Risk_score_evaluate_list_and_summary_for_published_pack`
- `Risk_score_evaluate_without_fact_source_yields_higher_risk`
- `Risk_score_evaluate_denies_tenant_member`
- `Risk_score_list_allowed_for_tenant_member`

### Frontend (`RiskScoringPanel.test.tsx`)

- Evaluator controls and score list
- Read-only message for non-evaluators

## Verification

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~RiskScoring"
cd apps/compliancecore-frontend
npm run test -- --run RiskScoring
npm run build
```

## Remaining gaps

- Non-boolean product mirror facts contribute via unresolved/warn paths only (rule evaluator is boolean-centric)
- No scheduled risk scoring worker (on-demand evaluate only)
- Predictive missing-evidence warnings remain a separate M12 item

## Next recommended slice

**NexArr M12** audit export enhancements, **RoutArr M12** audit package export, or **Compliance Core M12** predictive missing-evidence warnings per `00_SLICE_STATE.md`.
