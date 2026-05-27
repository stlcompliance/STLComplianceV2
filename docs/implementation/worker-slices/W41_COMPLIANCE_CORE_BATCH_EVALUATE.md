# Worker 41 — Compliance Core cross-product batch evaluate API

## Slice name

M5/M10 batch internal rule evaluation — `POST /api/internal/evaluate/batch` evaluating multiple rule packs with shared or per-item context using existing W32/W33 fact resolution and `RuleEvaluator`, service token auth, TrainArr batch qualification check refactored to single HTTP call, integration tests

## Products touched

- **Compliance Core API** (`apps/compliancecore-api`): batch contracts, `InternalRuleEvaluationService.EvaluateBatchAsync`, internal endpoint
- **TrainArr API** (`apps/trainarr-api`): `ComplianceCoreRuleEvaluationClient.EvaluateRulePackBatchAsync`, `QualificationCheckService.CheckBatchAsync` uses batch endpoint
- **Integration tests** (`tests/STLCompliance.ComplianceCore.Auth.Tests`): batch cases in `ComplianceCoreInternalRuleEvaluationTests`
- **Cross-product tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): existing batch qualification tests unchanged behavior

## API + auth changes

### Compliance Core internal API (NexArr service token → Compliance Core)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/internal/evaluate/batch` | service token → `compliancecore`, scope `compliancecore.rules.evaluate` |

Request: `tenantId`, `items[]` with `rulePackKey` and optional per-item `context`; request-level shared `context`; optional `emitFindings` (same as single evaluate).

Response: `batchId`, `results[]` (same shape as single `InternalEvaluateRulePackResponse`), `summary` (`total`, `allowCount`, `warnCount`, `blockCount`).

Each item runs full fact resolution and rule evaluation (reuses `EvaluateAsync`). Max 100 items per batch. Audit: `rules.internal_evaluate_batch` on `rule_pack_evaluation_batch` target.

## TrainArr integration

- Batch qualification checks (`POST /api/qualification-checks/batch`) call Compliance Core once via `api/internal/evaluate/batch` instead of N parallel single-evaluate HTTP calls
- Single qualification checks still use `POST /api/internal/evaluate`

## Tests

### Compliance Core (`ComplianceCoreInternalRuleEvaluationTests`)

- `Internal_evaluate_batch_returns_per_item_results_and_summary`
- `Internal_evaluate_batch_rejects_empty_items`
- `Internal_evaluate_batch_rejects_missing_service_token`

### Cross-product (existing, no new files)

- `StaffArrTrainArrQualificationBatchCheckTests` — batch warn/block/validation cases

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~Internal_evaluate"
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~QualificationBatch"
```

## Remaining gaps

- No concurrency limit within Compliance Core batch (evaluations run in parallel via `Task.WhenAll`)
- No per-item correlation id in batch response (callers map by item order)
- StaffArr and other products not yet wired to batch evaluate directly

## Next recommended slice

**TrainArr rule change impact** (M6/M10) — completed in W42. **Compliance Core admin batch evaluate UI** — completed in W43 (`POST /api/rule-packs/evaluate/batch`).
