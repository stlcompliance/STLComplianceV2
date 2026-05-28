# Worker 158 — TrainArr qualification recalculation worker (M12)

## Slice name

M12 qualification recalculation worker — tenant recalculation settings, materialized authorization outcomes for active qualifications, scheduled shared-worker refresh via Compliance Core rule-pack evaluation, optional auto-suspend on compliance block, JWT admin settings UI, integration and frontend tests

## Products touched

- **TrainArr API** (`apps/trainarr-api`): recalculation settings/state/run tables, `QualificationRecalculationService`, `QualificationCheckService.EvaluateIssueAsync`, internal + JWT endpoints
- **shared-worker** (`workers/shared-worker`): `TrainArrQualificationRecalculationJob`, client, options
- **TrainArr Frontend** (`apps/trainarr-frontend`): `QualificationRecalculationSettingsPanel`, Settings workspace wiring

## Schema

Migration `TrainArrQualificationRecalculation`:

- `trainarr_tenant_qualification_recalculation_settings` — per-tenant enable flag, staleness hours (default 24), auto-suspend toggle
- `trainarr_qualification_recalculation_states` — materialized per-qualification check outcome (`Outcome`, `ReasonCode`, `RulePackKey`, `ComputedAt`)
- `trainarr_qualification_recalculation_runs` — worker outcome audit (recalculated/suspended/skipped)

## API + auth changes

### TrainArr JWT (trainarr admin)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/qualification-recalculation-settings` | `RequireQualificationRecalculationSettingsManage` |
| PUT | `/api/qualification-recalculation-settings` | Same |
| GET | `/api/qualification-recalculation-settings/states` | Same |
| GET | `/api/qualification-recalculation-settings/runs` | Same |

### TrainArr internal (shared-worker)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/internal/qualification-recalculation/pending` | source `shared-worker`, scope `trainarr.qualifications.recalculate` |
| POST | `/api/internal/qualification-recalculation/process-batch` | Same |

`process-batch` body: optional `tenantId`, optional `asOfUtc`, `batchSize` (1–500, default 100), `stalenessHours` (1–168, default 24). Response includes recalculated/suspended IDs and per-item skip reasons.

## Permission keys

- JWT: trainarr admin / tenant_admin via `RequireQualificationRecalculationSettingsManage`
- Worker scope: `trainarr.qualifications.recalculate`

## Worker behavior

`TrainArrQualificationRecalculationJob` runs on a configurable interval (default 30 min), calls `POST /api/internal/qualification-recalculation/process-batch` with a NexArr service token. For each tenant with recalculation enabled, issued/suspended qualifications whose materialized state is missing or older than `StalenessHours` are candidates. Each candidate reuses qualification-check merge logic (local TrainArr state + optional Compliance Core rule-pack evaluation), upserts `trainarr_qualification_recalculation_states`, records run audit, and optionally suspends issued qualifications when `AutoSuspendOnBlock` is enabled and compliance evaluation blocks.

## Frontend changes

- **QualificationRecalculationSettingsPanel** on TrainArr Settings workspace — enable toggle, staleness hours, auto-suspend toggle, recent materialized states and worker runs from real APIs

## Tests

### Backend integration (`StaffArrTrainArrQualificationRecalculationWorkerTests`)

- Service token auth rejection
- Pending list before processing
- Process batch persists materialized allow outcome for issued qualification

### Unit (`QualificationRecalculationRulesTests`)

- Staleness boundary + auto-suspend guards

### Frontend unit

- `QualificationRecalculationSettingsPanel.test.tsx`

## Verification commands

```powershell
dotnet build -c Release
dotnet test tests/STLCompliance.Shared.Worker.Tests/STLCompliance.Shared.Worker.Tests.csproj -c Release --filter "FullyQualifiedName~QualificationRecalculation"
dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~QualificationRecalculation"
cd apps/trainarr-frontend
npm run test -- --run QualificationRecalculationSettingsPanel
```

## Remaining gaps

- No immediate invalidation on rule-pack change (staleness window driven; use W42 impact assessment for manual triage)
- Auto-suspend only when compliance evaluation blocks (local-only blocks unchanged)
- No RoutArr/MaintainArr consumer of materialized recalculation states yet

## Next recommended slice

**TrainArr StaffArr publish retry worker** or next open M12 worker backlog row from `02_PRODUCT_IMPLEMENTATION_BACKLOG.md`.
