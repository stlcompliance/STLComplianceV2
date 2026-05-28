# Worker 162 — TrainArr rule-pack impact worker (M12)

## Slice name

M12 rule-pack impact worker — tenant impact scan settings, materialized per-rule-pack drift outcomes, scheduled shared-worker scans via Compliance Core lookup + W42 impact assessment, optional auto-baseline sync, JWT admin settings UI, integration and frontend tests

## Products touched

- **TrainArr API** (`apps/trainarr-api`): impact settings/state/run tables, `RulePackImpactWorkerService`, `RulePackImpactSettingsService`, internal + JWT endpoints
- **shared-worker** (`workers/shared-worker`): `TrainArrRulePackImpactJob`, client, options
- **TrainArr Frontend** (`apps/trainarr-frontend`): `RulePackImpactSettingsPanel`, Settings workspace wiring

## Schema

Migration `TrainArrRulePackImpactWorker`:

- `trainarr_tenant_rule_pack_impact_settings` — per-tenant enable flag, staleness hours (default 24), auto-update baselines toggle
- `trainarr_rule_pack_impact_states` — materialized per-rule-pack assessment outcome (`RequiresAttention`, `HasDrift`, triggers, version/status drift, affected counts)
- `trainarr_rule_pack_impact_runs` — worker outcome audit (assessed/attention_required/skipped)

## API + auth changes

### TrainArr JWT (trainarr admin)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/rule-pack-impact-settings` | `RequireRulePackImpactSettingsManage` |
| PUT | `/api/rule-pack-impact-settings` | Same |
| GET | `/api/rule-pack-impact-settings/states` | Same |
| GET | `/api/rule-pack-impact-settings/runs` | Same |

### TrainArr internal (shared-worker)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/internal/rule-pack-impact/pending` | source `shared-worker`, scope `trainarr.rulepack_impact.scan` |
| POST | `/api/internal/rule-pack-impact/process-batch` | Same |

`process-batch` body: optional `tenantId`, optional `asOfUtc`, `batchSize` (1–200, default 50), `stalenessHours` (1–168, default 24). Response includes assessed/attention-required rule pack keys and per-item skip reasons.

## Permission keys

- JWT: trainarr admin / tenant_admin via `RequireRulePackImpactSettingsManage`
- Worker scope: `trainarr.rulepack_impact.scan`

## Worker behavior

`TrainArrRulePackImpactJob` runs on a configurable interval (default 30 min), calls `POST /api/internal/rule-pack-impact/process-batch` with a NexArr service token. For each tenant with impact scanning enabled, distinct rule pack keys linked via training requirements whose materialized state is missing or older than `StalenessHours` are candidates. Each candidate reuses `RulePackImpactService.AssessAsync` (Compliance Core lookup + affected entity rollups), upserts `trainarr_rule_pack_impact_states`, records run audit, and optionally updates requirement baselines when `AutoUpdateRequirementBaselines` is enabled and the scan requires no attention.

## Frontend changes

- **RulePackImpactSettingsPanel** on TrainArr Settings workspace — enable toggle, staleness hours, auto-baseline toggle, recent materialized states and worker runs from real APIs

## Tests

### Backend integration (`StaffArrTrainArrRulePackImpactWorkerTests`)

- Service token auth rejection
- Pending list before processing
- Process batch persists materialized attention-required state on version drift

### Unit (`RulePackImpactRulesTests`)

- Staleness boundary + auto-baseline guards

### Frontend unit

- `RulePackImpactSettingsPanel.test.tsx`

## Verification commands

```powershell
dotnet build -c Release
dotnet test tests/STLCompliance.Shared.Worker.Tests/STLCompliance.Shared.Worker.Tests.csproj -c Release --filter "FullyQualifiedName~RulePackImpact"
dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~RulePackImpactWorker"
cd apps/trainarr-frontend
npm run test -- --run RulePackImpactSettingsPanel
```

## Remaining gaps

- No webhook/notification fan-out when impact scans require attention (use notification dispatch settings separately)
- Auto-baseline sync only when scan is clean; drift still requires manual review via W42 impact panel
- No cross-product consumer of materialized impact states yet

## Next recommended slice

**M12 TrainArr evidence retention worker** or orphan reference detection worker per `02_PRODUCT_IMPLEMENTATION_BACKLOG.md`.
