# Worker 163 — TrainArr evidence retention worker (M12)

## Slice name

M12 evidence retention worker — tenant retention settings, expired evidence purge via shared-worker scheduled job, storage file deletion + DB record removal, JWT admin settings UI, integration and frontend tests

## Products touched

- **TrainArr API** (`apps/trainarr-api`): retention settings/run tables, `EvidenceRetentionWorkerService`, `EvidenceRetentionSettingsService`, internal + JWT endpoints, `TrainArrEvidenceStorageService.TryDelete`
- **shared-worker** (`workers/shared-worker`): `TrainArrEvidenceRetentionJob`, client, options
- **TrainArr Frontend** (`apps/trainarr-frontend`): `EvidenceRetentionSettingsPanel`, Settings workspace wiring

## Schema

Migration `TrainArrEvidenceRetentionWorker`:

- `trainarr_tenant_evidence_retention_settings` — per-tenant enable flag, retention days after assignment close (default 365)
- `trainarr_evidence_retention_runs` — worker outcome audit (purged/none/skipped, counts, bytes reclaimed)

## API + auth changes

### TrainArr JWT (trainarr admin)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/evidence-retention-settings` | `RequireEvidenceRetentionSettingsManage` |
| PUT | `/api/evidence-retention-settings` | Same |
| GET | `/api/evidence-retention-settings/runs` | Same |

### TrainArr internal (shared-worker)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/internal/evidence-retention/pending` | source `shared-worker`, scope `trainarr.evidence.retention.purge` |
| POST | `/api/internal/evidence-retention/process-batch` | Same |

`process-batch` body: optional `tenantId`, optional `asOfUtc`, `batchSize` (1–200, default 50). Response includes purged evidence IDs, bytes reclaimed, and per-item skip reasons.

## Permission keys

- JWT: trainarr admin / tenant_admin via `RequireEvidenceRetentionSettingsManage`
- Worker scope: `trainarr.evidence.retention.purge`

## Worker behavior

`TrainArrEvidenceRetentionJob` runs on a configurable interval (default 60 min), calls `POST /api/internal/evidence-retention/process-batch` with a NexArr service token. For each tenant with retention enabled, evidence on completed/cancelled assignments whose close date is older than `RetentionDaysAfterAssignmentClose` is a purge candidate. Each candidate deletes the on-disk storage file, removes the DB record, records tenant run audit, and writes batch audit when scoped to a tenant.

## Frontend changes

- **EvidenceRetentionSettingsPanel** on TrainArr Settings workspace — enable toggle, retention days, recent worker runs from real APIs

## Tests

### Backend integration (`StaffArrTrainArrEvidenceRetentionWorkerTests`)

- Service token auth rejection
- Pending list before processing
- Process batch purges expired evidence files and DB records

### Unit (`EvidenceRetentionRulesTests`)

- Closed assignment detection, retention boundary, normalization guards

### Frontend unit

- `EvidenceRetentionSettingsPanel.test.tsx`

## Verification commands

```powershell
dotnet build -c Release
dotnet test tests/STLCompliance.Shared.Worker.Tests/STLCompliance.Shared.Worker.Tests.csproj -c Release --filter "FullyQualifiedName~EvidenceRetention"
dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~EvidenceRetentionWorker"
cd apps/trainarr-frontend
npm run test -- --run EvidenceRetentionSettingsPanel
```

## Remaining gaps

- No pre-purge export/audit package snapshot before deletion
- Cancelled assignments use `UpdatedAt` as close date (no dedicated `CancelledAt` field yet)
- No webhook/notification when purges occur

## Next recommended slice

**M12 TrainArr orphan reference detection worker** per `02_PRODUCT_IMPLEMENTATION_BACKLOG.md`.
