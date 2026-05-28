# Worker 167 — TrainArr async audit package generation (M12)

## Slice name

M12 async audit package generation — background ZIP/JSON job queue, internal process-batch worker API, shared-worker scheduled scan, TrainArr admin export UI with job status polling, integration and unit tests (builds on W165 sync audit package)

## Products touched

- **TrainArr API** (`apps/trainarr-api`): `trainarr_audit_package_generation_jobs`, `AuditPackageGenerationService`, job + internal endpoints
- **shared-worker** (`workers/shared-worker`): `TrainArrAuditPackageGenerationJob`, client, options
- **TrainArr Frontend** (`apps/trainarr-frontend`): `AuditPackageExportPanel` background ZIP export + job status
- **Shared** (`packages/shared-dotnet`): `StlIntegrationTokenCatalog` profile for `trainarr.audit_packages.generate`
- **Render** (`render.yaml`): `TrainArrAuditPackageGeneration__TrainArrBaseUrl`
- **Tests**: `TrainArrAuditPackageGenerationTests`, `TrainArrAuditPackageGenerationRulesTests`, frontend vitest, OpenAPI snapshot

## Schema

Migration `TrainArrAuditPackageGenerationJobs`:

- `trainarr_audit_package_generation_jobs` — async export job queue with stored ZIP/JSON artifacts

| Column | Purpose |
|--------|---------|
| `Status` | pending / processing / completed / failed |
| `Format` | zip or json |
| `FromUtc` / `ToUtc` | Optional date filters |
| `PackageId` | Generated package identifier |
| `ArtifactZip` / `ArtifactJson` | Stored export artifact |
| `ErrorMessage` | Failure detail when status is failed |

Indexes: `(TenantId, Status, CreatedAt)`, `CreatedAt`.

## API + auth changes

### TrainArr user APIs (JWT + `trainarr.audit.export`)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/audit-packages/jobs` | `RequireAuditPackageExport` — enqueue background job |
| GET | `/api/audit-packages/jobs/{jobId}` | Same — poll job status |
| GET | `/api/audit-packages/jobs/{jobId}/download` | Same — download completed artifact |

Sync manifest/export from W165 unchanged.

### TrainArr internal APIs (service token)

| Method | Route | Scope |
|--------|-------|-------|
| GET | `/api/internal/audit-package-jobs/pending` | `trainarr.audit_packages.generate` |
| POST | `/api/internal/audit-package-jobs/process-batch` | Same |

Source product must be `shared-worker`; target product `TrainArr`.

## Worker behavior

- `TrainArrAuditPackageGenerationJob` runs on configurable interval (default 2 min)
- Calls internal `process-batch` with service token
- Processes pending jobs via `AuditPackageService.MaterializeExportAsync` + ZIP/JSON artifact storage
- Writes audit events: `audit_package.generation.enqueued`, `.completed`, `.failed`, `.batch`

## Frontend

- `AuditPackageExportPanel`: "Background ZIP export" button, job status chip with polling, auto-download on completion
- API client: `createAuditPackageGenerationJob`, `getAuditPackageGenerationJob`, `downloadAuditPackageGenerationJob`

## Tests

- Integration: create job → pending → process-batch → completed → download ZIP
- Rules unit tests: format normalization, download readiness
- Frontend vitest: background export button visibility
- OpenAPI snapshot updated for new job endpoints

## Remaining gaps

- NexArr M12 service-token cleanup worker (next backlog row)
- Background JSON job UI (API supports json format; UI only queues ZIP background jobs, matching other products)

## Next recommended slice

**Worker 168 — NexArr service-token cleanup worker (M12)**: expired/revoked service token purge, internal batch API, shared-worker job, platform admin visibility, tests.
