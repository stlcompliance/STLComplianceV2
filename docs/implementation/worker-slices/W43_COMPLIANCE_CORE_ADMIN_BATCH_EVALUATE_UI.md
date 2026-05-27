# Worker 43 — Compliance Core admin batch evaluate UI

## Slice name

M5/M10 admin batch rule evaluation — `POST /api/rule-packs/evaluate/batch` (JWT) orchestrating parallel per-pack evaluations via existing `RuleEvaluationService` / `RuleEvaluator`, `BatchRuleEvaluationPanel` on the Rule evaluation tab, integration and frontend tests

## Products touched

- **Compliance Core API** (`apps/compliancecore-api`): batch contracts, `RulePackBatchEvaluationService`, user endpoint
- **Compliance Core Frontend** (`apps/compliancecore-frontend`): `BatchRuleEvaluationPanel` on Rule evaluation tab
- **Integration tests** (`tests/STLCompliance.ComplianceCore.Auth.Tests`): batch cases in `ComplianceCoreRuleEvaluationTests`

## API + auth changes

### Compliance Core user API (JWT + Compliance Core entitlement)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/rule-packs/evaluate/batch` | same as single evaluate (`RequireRuleEvaluation`) |

Request: `items[]` with `rulePackKey` and optional per-item `facts` overrides; request-level shared `facts`, `emitFindings`. Duplicate pack keys deduplicated (last wins). Max 100 items per batch (same limit as W41 internal batch).

Response: `batchId`, `results[]` (`rulePackKey`, `outcome`, `overallResult`, `evaluationRunId`, `ruleResults`, …), `summary` (`total`, `allowCount`, `warnCount`, `blockCount`).

Each item resolves the latest active rule pack by key, runs `RuleEvaluationService.EvaluateAsync` (explicit boolean facts — admin UI path), maps pass/fail to allow/block via `RuleEvaluationOutcomeMapper`. Audit: `rules.evaluate_batch` on `rule_pack_evaluation_batch` target.

### Relation to W41 internal batch

- W41 `POST /api/internal/evaluate/batch` remains the cross-product path (service token, fact-source resolution via context).
- W43 JWT batch reuses the same orchestration pattern (parallel items, summary, audit) and `RuleEvaluator` engine, with explicit facts for operator tooling.

## Frontend changes

- **BatchRuleEvaluationPanel** — multi-select rule packs (by `packKey`), shared fact checkboxes, emit-findings toggle, batch summary and per-pack outcomes
- Wired from **RuleEvaluationPanel** / Home page Rule evaluation tab

## Tests

### Backend integration (`ComplianceCoreRuleEvaluationTests`)

- `Rule_pack_batch_evaluate_returns_per_item_results_and_summary`
- `Rule_pack_batch_evaluate_blocks_when_shared_facts_fail`
- `Rule_pack_batch_evaluate_rejects_empty_items`
- `Rule_pack_batch_evaluate_requires_compliancecore_entitlement`
- `Rule_pack_batch_evaluate_member_can_run`

### Frontend unit

- `BatchRuleEvaluationPanel.test.tsx`

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~Rule_pack_batch"
cd apps/compliancecore-frontend
npm install
npm run test -- --run
npm run build
```

## Remaining gaps

- JWT batch does not use fact-source resolution (context-only); operators must supply boolean facts (same as single JWT evaluate)
- No per-item fact override UI (API supports per-item `facts`; panel sends shared facts only)
- StaffArr / other products not wired to JWT batch (use W41 internal batch)

## Next recommended slice

**TrainArr expiration scanning worker** (M12) or **Compliance Core operator dashboards** per milestone priority.
