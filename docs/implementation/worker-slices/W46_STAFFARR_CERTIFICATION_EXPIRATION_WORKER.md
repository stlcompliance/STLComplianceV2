# Worker 46 — StaffArr certification expiration worker

## Slice name

M12 cross-product scheduled worker — `shared-worker` scans StaffArr for person certifications past `expiresAt`, transitions active records to `expired` via internal service-token API, configuration for scan interval and batch size, unit and integration tests

## Products touched

- **shared-worker** (`workers/shared-worker`): `StaffArrCertificationExpirationJob`, HTTP client to StaffArr internal API, `appsettings` for interval/batch/token
- **StaffArr API** (`apps/staffarr-api`): `CertificationExpirationService` + rules, internal endpoints, `ExpireByWorkerAsync`, index on `(tenant_id, status, expires_at)` for scan queries
- **Tests**: `STLCompliance.Shared.Worker.Tests` (expiration rules), `StaffArrCertificationExpirationWorkerTests` (cross-product)

## Schema

### StaffArr migration `StaffArrCertificationExpirationIndex`

- Index `(tenant_id, status, expires_at)` on `staffarr_person_certifications`
- `ExpiresAt` column already present from certification foundations (W15); no column change

## API + auth changes

### StaffArr internal (service token)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/internal/certifications/pending-expiration` | NexArr service token: source `shared-worker`, target `staffarr`, scope `staffarr.certifications.expire` |
| POST | `/api/internal/certifications/process-expirations` | Same |

`process-expirations` body: optional `tenantId`, optional `asOfUtc`, `batchSize` (1–500, default 100). Response includes expired IDs and per-item skip reasons.

Scanner selects `active` person certifications where `expiresAt` is on or before `asOfUtc`. Each match sets status `expired`, appends an auto-expire note, and audits `person_certification.expire.auto`.

TrainArr-driven expirations continue via lifecycle ingest (W31/W44); this worker covers StaffArr-owned records and any active certification past expiry not yet persisted as `expired`.

## shared-worker configuration

`StaffArrCertificationExpiration` section:

| Key | Default | Purpose |
|-----|---------|---------|
| `Enabled` | `true` | Toggle job |
| `StaffArrBaseUrl` | `http://localhost:5102` | StaffArr API base |
| `ServiceToken` | `""` | Bearer for internal expire API |
| `ScanIntervalMinutes` | `15` | Periodic scan interval |
| `BatchSize` | `100` | Max certifications per run |
| `TenantId` | `null` | Optional tenant filter |

## Tests

### Unit (`CertificationExpirationRulesTests`)

- Expirable status guard (`active` only)
- `ShouldExpire` boundary and missing-expiry cases

### Integration (`StaffArrCertificationExpirationWorkerTests`)

- `Process_expirations_rejects_missing_service_token`
- `Process_expirations_rejects_trainarr_source_token`
- `Process_expirations_expires_past_due_manual_certification`
- `List_pending_expiration_returns_candidates_before_processing`

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.Shared.Worker.Tests/STLCompliance.Shared.Worker.Tests.csproj" -c Release
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~CertificationExpiration"
```

## Remaining gaps

- No multi-tenant discovery loop in worker (optional `TenantId` only)
- `staffarr-worker` remains heartbeat-only; cross-product expiration jobs live in `shared-worker` (same as W44)

## Next recommended slice

**Compliance Core scheduled evaluation worker** or **StaffArr permission projection / readiness rollup workers** per M12 milestone priority — see `docs/08_EVENTS_WORKERS_AND_INTEGRATION.md`.
