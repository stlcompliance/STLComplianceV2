# Worker 36 — TrainArr batch qualification checks

## Slice name

M10 batch qualification authorization checks — `POST /api/qualification-checks/batch` orchestrating per-subject local qualification state with rate-limited parallel Compliance Core `/api/internal/evaluate` calls, supervisor JWT auth, batch audit event, trainarr-frontend batch panel, cross-product tests

## Products touched

- **TrainArr API** (`apps/trainarr-api`): batch endpoint, `QualificationCheckService.CheckBatchAsync`, `RequireBatchQualificationChecks`
- **Compliance Core API** (`apps/compliancecore-api`): existing `POST /api/internal/evaluate` (no rule logic duplicated in TrainArr)
- **TrainArr Frontend** (`apps/trainarr-frontend`): `BatchQualificationCheckPanel` on home page for supervisors/trainers
- **Integration tests**: `StaffArrTrainArrQualificationBatchCheckTests`

## API + auth changes

### TrainArr user API (JWT + TrainArr entitlement)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/qualification-checks/batch` | `RequireBatchQualificationChecks` — `tenant_admin`, `trainarr_admin`, `trainarr_trainer`, or platform admin |

Request: shared `qualificationKey`, optional `rulePackKey`, `subjects[]` with `staffarrPersonId` and optional per-subject `context` for Compliance Core fact resolution.

Response: `batchId`, `qualificationKey`, `results[]` (same shape as single check), `summary` (`total`, `allowCount`, `warnCount`, `blockCount`).

TrainArr merges local `trainarr_qualification_issues` per person with Compliance Core evaluation (strictest outcome wins). Compliance Core evaluations run in parallel with `ComplianceCore:MaxConcurrentEvaluations` (default 4). Max 100 subjects per batch.

Audit: `qualification_check.batch_run` on `qualification_check_batch` target with aggregate counts in `result`.

### Configuration

`apps/trainarr-api/TrainArr.Api/appsettings.json`:

```json
"ComplianceCore": {
  "BaseUrl": "http://localhost:5107",
  "ServiceToken": "",
  "MaxConcurrentEvaluations": 4
}
```

## Frontend changes

- **BatchQualificationCheckPanel** — qualification key, rule pack key, remediation multi-select, paste person ID list, batch summary and per-person outcomes
- Shown when `canRunBatchQualificationChecks` (admin/trainer roles)

## Tests

### Cross-product (`StaffArrTrainArrQualificationBatchCheckTests`)

- `Batch_qualification_check_returns_per_subject_results_and_summary`
- `Batch_qualification_check_blocks_subject_when_compliance_rules_fail`
- `Batch_qualification_check_writes_batch_audit_event`
- `Batch_qualification_check_denies_tenant_member`
- `Batch_qualification_check_rejects_empty_subjects`

### Frontend unit

- `BatchQualificationCheckPanel.test.tsx`

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~QualificationBatch"
cd apps/trainarr-frontend
npm install
npm run test -- --run
npm run build
```

## Remaining gaps

- Compliance Core batch evaluate API not added (TrainArr parallelizes single evaluate calls)
- Per-subject context from StaffArr person attributes not auto-populated
- Training definition → rule pack key linkage still caller-provided

## Next recommended slice

**Compliance Core audit package export** (M5/M12) or **TrainArr citation attachment** (M10) per milestone priority.
