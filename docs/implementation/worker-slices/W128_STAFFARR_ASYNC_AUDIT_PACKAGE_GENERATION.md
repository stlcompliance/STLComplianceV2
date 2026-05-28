# Worker 128 — StaffArr async audit package generation worker (M4/M12)

## Scope

Vertical slice building on Workers 106/126 sync audit export:

- `staffarr_audit_package_generation_jobs` — outbox with pending/processing/completed/failed status and stored ZIP/JSON artifacts
- User APIs: `POST /api/audit-packages/jobs`, `GET /api/audit-packages/jobs/{id}`, `GET .../download`
- Internal APIs: `GET /api/internal/audit-package-jobs/pending`, `POST .../process-batch` (service token `staffarr.audit_packages.generate`)
- `shared-worker` `StaffArrAuditPackageGenerationJob` + `StlIntegrationTokenCatalog` profile `worker-staffarr-audit-packages`
- `staffarr-frontend` `AuditPackageExportPanel` background ZIP export with job status polling and auto-download
- Tests: `StaffArrAuditPackageGenerationTests`, `AuditPackageGenerationRulesTests`

## Job lifecycle

| Status | Meaning |
|--------|---------|
| `pending` | Queued by user; awaiting worker |
| `processing` | Worker claimed job |
| `completed` | Artifact stored (`ArtifactZip` or `ArtifactJson`) |
| `failed` | Generation error recorded in `ErrorMessage` |

## Configuration

- API: export roles for job create/status/download; `shared-worker` bearer for internal batch
- Worker: `StaffArrAuditPackageGeneration__StaffArrBaseUrl`, `StaffArrAuditPackageGeneration__ServiceToken`
- Render: env on `shared-worker` service in `render.yaml`

## Sync export

`GET /api/audit-packages/export` remains for immediate ZIP/JSON downloads; background jobs target large tenant packages without blocking the browser request.
