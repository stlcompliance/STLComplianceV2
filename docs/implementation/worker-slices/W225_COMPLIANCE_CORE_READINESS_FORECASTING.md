# Worker 225 — Compliance Core M12 readiness forecasting

## Slice name

M12 readiness forecasting — combine risk scores, missing-evidence warnings, and control effectiveness at scope into readiness forecasts; persisted runs and per-pack forecasts, evaluate/list/summary APIs, Admin `ReadinessForecastPanel`, integration + Vitest tests

## Products touched

- **Compliance Core API** (`apps/compliancecore-api`): `ReadinessForecastService`, `ReadinessForecastRules`, `compliancecore_readiness_forecast_runs`, `compliancecore_readiness_forecasts`
- **Compliance Core Frontend** (`apps/compliancecore-frontend`): `ReadinessForecastPanel` on Admin workspace
- **Integration tests** (`tests/STLCompliance.ComplianceCore.Auth.Tests`): `ComplianceCoreReadinessForecastTests`

## Schema

### Migration `ComplianceCoreReadinessForecasting`

**`compliancecore_readiness_forecast_runs`**

| Column | Notes |
|--------|-------|
| id | PK |
| tenant_id, scope_key | |
| packs_forecast_count | |
| readiness_score, readiness_level | aggregate |
| lowest/average_readiness_score | |
| highest_risk_score, missing_evidence_warning_count, average_effectiveness_score | from source runs |
| risk_score_run_id, missing_evidence_warning_run_id, control_effectiveness_run_id | traceability (no cross-DB FK) |
| forecasted_at | |

**`compliancecore_readiness_forecasts`**

| Column | Notes |
|--------|-------|
| id | PK |
| run_id | FK cascade |
| scope_key, pack_key, rule_pack_id | |
| readiness_score, readiness_level | ready / caution / not_ready / unknown |
| risk_score, risk_level | |
| effectiveness_score, effectiveness_level | |
| missing_evidence_warning_count, highest_missing_evidence_severity | |
| summary | |

## Forecast model

On evaluate:

1. Run `RiskScoringService`, `MissingEvidenceWarningService`, and `ControlEffectivenessService` at the same scope/context.
2. Merge per `pack_key` and compute readiness score (0–100) from inverted risk, effectiveness, and missing-evidence penalties.
3. Level: ≥75 ready, ≥50 caution, ≥25 not_ready, else unknown.
4. Persist forecast run with source run IDs for audit traceability.

## API + auth

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/readiness-forecasts/summary` | entitled read |
| GET | `/api/readiness-forecasts` | read — `scopeKey`, `rulePackKey`, `readinessLevel`, `runId`, `limit` |
| POST | `/api/readiness-forecasts/evaluate` | admin / compliance_admin / compliance_reviewer |

## Audit

- `readiness_forecasts.evaluate`

## Frontend

- **ReadinessForecastPanel** — forecast/ready/caution/not-ready/weakest tiles, evaluate form, per-pack forecast list
- `canEvaluateReadinessForecast` (same roles as risk scoring)

## Tests

### Backend (`ComplianceCoreReadinessForecastTests`)

- `Readiness_forecast_evaluate_list_and_summary_for_effective_pack`
- `Readiness_forecast_evaluate_without_fact_source_yields_lower_score`
- `Readiness_forecast_evaluate_denies_tenant_member`
- `Readiness_forecast_list_allowed_for_tenant_member`

### Frontend (`ReadinessForecastPanel.test.tsx`)

- Evaluator controls and forecast list
- Read-only message for non-evaluators

## Verification

```powershell
dotnet build -c Release apps/compliancecore-api/ComplianceCore.Api/ComplianceCore.Api.csproj
dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~ReadinessForecast"
cd apps/compliancecore-frontend
npm run test -- --run ReadinessForecast
npm run build
```

## Remaining gaps

- Evaluate triggers three upstream runs (no read-only aggregate-from-latest mode)
- No scheduled forecast worker or trend analytics across runs
- NexArr/RoutArr M12 audit export slices remain separate backlog

## Next recommended slice

**NexArr M12** audit export enhancements, **RoutArr M12** audit package export, or Compliance Core vocabulary/regulatory workers per `00_SLICE_STATE.md`.
