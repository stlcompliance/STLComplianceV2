# Worker 44 — TrainArr expiration scanning worker

## Slice name

M12 cross-product scheduled worker — `shared-worker` scans TrainArr for qualifications past `expiresAt`, invokes W31 expire lifecycle via service token internal API, optional `ExpiresAt` on qualification issues, configuration for scan interval and batch size, unit and integration tests

## Products touched

- **shared-worker** (`workers/shared-worker`): `TrainArrQualificationExpirationJob`, HTTP client to TrainArr internal API, `appsettings` for interval/batch/token
- **TrainArr API** (`apps/trainarr-api`): `ExpiresAt` on `trainarr_qualification_issues`, `QualificationExpirationService` + rules, internal endpoints, `ExpireByWorkerAsync`, migration backfill from grant publications
- **Tests**: `STLCompliance.Shared.Worker.Tests` (scanner rules), `StaffArrTrainArrQualificationExpirationWorkerTests` (cross-product)

## Schema

### TrainArr migration `TrainArrQualificationExpiresAt`

- `trainarr_qualification_issues.ExpiresAt` (nullable)
- Index `(tenant_id, status, expires_at)`
- SQL backfill from `trainarr_certification_publications.ExpiresAt` via `GrantPublicationId`

## API + auth changes

### TrainArr internal (service token)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/internal/qualifications/pending-expiration` | NexArr service token: source `shared-worker`, target `trainarr`, scope `trainarr.qualifications.expire` |
| POST | `/api/internal/qualifications/process-expirations` | Same |

`process-expirations` body: optional `tenantId`, optional `asOfUtc`, `batchSize` (1–500, default 100). Response includes expired IDs and per-item skip reasons.

Scanner selects `issued` / `suspended` qualifications where effective expiry (`issue.ExpiresAt` or grant publication `ExpiresAt`) is on or before `asOfUtc`. Each match runs existing `QualificationIssueService` expire flow (StaffArr lifecycle ingest, audit `qualification_issue.expire.auto`).

### TrainArr user API

`QualificationIssueResponse` includes `expiresAt`. Issue-on-completion copies grant expiry onto the qualification issue row.

## shared-worker configuration

`TrainArrQualificationExpiration` section:

| Key | Default | Purpose |
|-----|---------|---------|
| `Enabled` | `true` | Toggle job |
| `TrainArrBaseUrl` | `http://localhost:5103` | TrainArr API base |
| `ServiceToken` | `""` | Bearer for internal expire API |
| `ScanIntervalMinutes` | `15` | Periodic scan interval |
| `BatchSize` | `100` | Max qualifications per run |
| `TenantId` | `null` | Optional tenant filter |

## Tests

### Unit (`QualificationExpirationRulesTests`)

- Expirable status guard
- Effective expiry resolution (issue vs grant fallback)
- `ShouldExpire` boundary and missing-expiry cases

### Integration (`StaffArrTrainArrQualificationExpirationWorkerTests`)

- `Process_expirations_rejects_missing_service_token`
- `Process_expirations_expires_past_due_qualification_and_updates_staffarr`
- `List_pending_expiration_returns_candidates_before_processing`

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.Shared.Worker.Tests/STLCompliance.Shared.Worker.Tests.csproj" -c Release
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~QualificationExpiration"
```

## Remaining gaps

- No multi-tenant discovery loop in worker (optional `TenantId` only; platform-wide scan requires tenant-unscoped service token)
- StaffArr certification expiration worker completed in W46 (`shared-worker` + StaffArr internal API)
- Product-specific `trainarr-worker` still heartbeat-only; cross-product jobs live in `shared-worker`

## Next recommended slice

**Compliance Core operator dashboards** (M5/M12) or **StaffArr certification expiration worker** per milestone priority.
