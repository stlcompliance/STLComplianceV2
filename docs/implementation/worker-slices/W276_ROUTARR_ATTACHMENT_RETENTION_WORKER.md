# Worker 276 — RoutArr attachment retention worker (M12)

## Slice name

M12 attachment retention worker — tenant retention settings, expired trip capture attachment purge via shared-worker scheduled job, storage file deletion + DB record removal, JWT admin settings UI, integration and frontend tests

## Products touched

- **RoutArr API** (`apps/routarr-api`): retention settings/run tables, `AttachmentRetentionWorkerService`, `AttachmentRetentionSettingsService`, internal + JWT endpoints, `RoutArrCaptureAttachmentStorageService.TryDelete`
- **shared-worker** (`workers/shared-worker`): `RoutArrAttachmentRetentionJob`, client, options
- **RoutArr Frontend** (`apps/routarr-frontend`): `AttachmentRetentionSettingsPanel`, Settings workspace wiring

## Schema

Migration `RoutArrAttachmentRetentionWorker`:

- `routarr_tenant_attachment_retention_settings` — per-tenant enable flag, retention days after trip close (default 365)
- `routarr_attachment_retention_runs` — worker outcome audit (purged/none/skipped, counts, bytes reclaimed)

## API + auth changes

### RoutArr JWT (routarr admin)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/attachment-retention-settings` | `RequireAttachmentRetentionSettingsManage` |
| PUT | `/api/attachment-retention-settings` | Same |
| GET | `/api/attachment-retention-settings/runs` | Same |

### RoutArr internal (shared-worker)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/internal/attachment-retention/pending` | source `shared-worker`, scope `routarr.attachments.retention.purge` |
| POST | `/api/internal/attachment-retention/process-batch` | Same |

`process-batch` body: optional `tenantId`, optional `asOfUtc`, `batchSize` (1–200, default 50). Response includes purged attachment IDs, bytes reclaimed, and per-item skip reasons.

## Permission keys

- JWT: routarr admin / tenant_admin via `RequireAttachmentRetentionSettingsManage`
- Worker scope: `routarr.attachments.retention.purge`

## Worker behavior

`RoutArrAttachmentRetentionJob` runs on a configurable interval (default 60 min), calls `POST /api/internal/attachment-retention/process-batch` with a NexArr service token. For each tenant with retention enabled, capture attachments on completed/cancelled trips whose close date is older than `RetentionDaysAfterTripClose` are purge candidates. Completed trips use `ClosedAt ?? CompletedAt ?? UpdatedAt`; cancelled trips use `CancelledAt ?? UpdatedAt`. Each candidate deletes the on-disk storage file, removes the DB record, records tenant run audit, and writes batch audit when scoped to a tenant.

## Frontend changes

- **AttachmentRetentionSettingsPanel** on RoutArr Settings workspace — enable toggle, retention days, recent worker runs from real APIs

## Tests

### Backend integration (`RoutArrAttachmentRetentionWorkerTests`)

- Service token auth rejection
- Pending list before processing
- Process batch purges expired attachment files and DB records

### Unit (`AttachmentRetentionRulesTests`)

- Closed trip detection, retention boundary, normalization guards

### Frontend unit

- `AttachmentRetentionSettingsPanel.test.tsx`

## Verification commands

```powershell
dotnet build -c Release
dotnet test tests/STLCompliance.Shared.Worker.Tests/STLCompliance.Shared.Worker.Tests.csproj -c Release --filter "FullyQualifiedName~AttachmentRetention"
dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~AttachmentRetentionWorker"
cd apps/routarr-frontend
npm run test -- --run AttachmentRetentionSettingsPanel
```

## Remaining gaps

- No pre-purge export/audit package snapshot before deletion
- Completed trips without driver Close still purge based on `CompletedAt` when `ClosedAt` is null
- No webhook/notification when purges occur

## Next recommended slice

**M13 Playwright — RoutArr settings trip completion rollup panel smoke** (builds on W176/W263/W277).
