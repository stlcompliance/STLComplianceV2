# Worker 47 — Compliance Core scheduled evaluation worker

## Slice name

M12 cross-product scheduled worker — `shared-worker` scans Compliance Core for published rule packs due for scheduled evaluation, invokes internal batch API with service token scope `compliancecore.rules.evaluate.scheduled`, persists scheduled run audit records, real `RuleEvaluator` evaluation via `InternalRuleEvaluationService`

## Products touched

- **Compliance Core API** (`apps/compliancecore-api`): internal `GET/POST /api/internal/scheduled-evaluations/*`, `ScheduledRuleEvaluationService`, `LastScheduledEvaluationAt` on rule packs, `compliancecore_scheduled_rule_evaluation_runs` audit table, migration
- **shared-worker** (`workers/shared-worker`): `ComplianceCoreScheduledEvaluationJob`, HTTP client, `ComplianceCoreScheduledEvaluation` configuration
- **Tests**: `ScheduledRuleEvaluationRulesTests`; `ComplianceCoreScheduledEvaluationWorkerTests` (5 cases)

## Schema

### Migration `ComplianceCoreScheduledRuleEvaluation`

- `compliancecore_rule_packs.LastScheduledEvaluationAt` (nullable `timestamptz`)
- Index `(tenant_id, status, last_scheduled_evaluation_at)` on rule packs
- Table `compliancecore_scheduled_rule_evaluation_runs` — batch audit (counts, status, interval hours, tenant scope)

## API + auth changes

### Compliance Core internal (service token)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/internal/scheduled-evaluations/pending` | NexArr service token: source `shared-worker`, target `compliancecore`, scope `compliancecore.rules.evaluate.scheduled` (or `compliancecore.rules.evaluate`) |
| POST | `/api/internal/scheduled-evaluations/process-batch` | Same |

`process-batch` body: optional `tenantId`, optional `asOfUtc`, `batchSize` (1–500, default 100), `intervalHours` (1–720, default 24), `emitFindings` (default true). Response includes `scheduledRunId`, outcome counts, `evaluationRunIds`, and per-pack skip reasons.

Due packs: latest **published** active version per `packKey` with rule content, where `LastScheduledEvaluationAt` is null or older than `intervalHours` before `asOfUtc`. Each match runs `InternalRuleEvaluationService.EvaluateAsync` (fact resolve + `RuleEvaluator`), updates `LastScheduledEvaluationAt`, and optionally emits findings.

## shared-worker configuration

`ComplianceCoreScheduledEvaluation` section:

| Key | Default | Purpose |
|-----|---------|---------|
| `Enabled` | `true` | Toggle job |
| `ComplianceCoreBaseUrl` | `http://localhost:5107` | Compliance Core API base |
| `ServiceToken` | `""` | Bearer for internal scheduled evaluation API |
| `ScanIntervalMinutes` | `60` | Periodic scan interval |
| `BatchSize` | `50` | Max rule packs per run |
| `IntervalHours` | `24` | Due-if-not-evaluated window |
| `TenantId` | `null` | Optional tenant filter |
| `EmitFindings` | `true` | Pass through to process-batch |

## Tests

### Unit (`ScheduledRuleEvaluationRulesTests`)

- Published status + content eligibility
- Due interval boundary (`IsDue`)
- Interval/batch normalization clamps

### Integration (`ComplianceCoreScheduledEvaluationWorkerTests`)

- `Process_batch_rejects_missing_service_token`
- `Process_batch_rejects_trainarr_source_token`
- `List_pending_returns_published_rule_pack_before_processing`
- `Process_batch_evaluates_due_published_rule_pack_and_records_run`
- `Process_batch_skips_recently_evaluated_packs_until_interval_elapses`

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.Shared.Worker.Tests/STLCompliance.Shared.Worker.Tests.csproj" -c Release
dotnet test "tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~ScheduledEvaluation"
```

## Remaining gaps

- No multi-tenant discovery loop in worker (optional `TenantId` only)
- Per-pack evaluation interval overrides not modeled (global `intervalHours` only)
- `compliancecore-worker` remains vocabulary/maintenance-focused; cross-product scheduled orchestration stays in `shared-worker`

## Next recommended slice

**StaffArr permission projection / readiness rollup workers** or **MaintainArr/RoutArr scheduled workers** per M12 milestone priority — see `docs/08_EVENTS_WORKERS_AND_INTEGRATION.md`.
