# Worker 132 — MaintainArr async audit package generation (M6/M12)

## Scope

Vertical slice building on sync audit export foundations:

- `maintainarr_audit_package_generation_jobs` — outbox with pending/processing/completed/failed status and stored ZIP/JSON artifacts
- User APIs: `POST /api/audit-packages/jobs`, `GET /api/audit-packages/jobs/{id}`, `GET .../download`
- Sync export: `GET /api/audit-packages/manifest`, `GET /api/audit-packages/export` (ZIP/JSON)
- Internal APIs: `GET /api/internal/audit-package-jobs/pending`, `POST .../process-batch` (service token `maintainarr.audit_packages.generate`)
- `shared-worker` `MaintainArrAuditPackageGenerationJob` + `StlIntegrationTokenCatalog` profile `worker-maintainarr-audit-packages`
- `maintainarr-frontend` `AuditPackageExportPanel` background ZIP export with job status polling and auto-download
- Tests: `MaintainArrAuditPackageGenerationTests`, `MaintainArrAuditPackageGenerationRulesTests`

## Package sections

| Section | File |
|---------|------|
| Audit events | `audit_events.json` |
| Assets | `assets.json` |
| Work orders | `work_orders.json` |
| Defects | `defects.json` |
| Inspection runs | `inspection_runs.json` |
| PM schedules | `pm_schedules.json` |

## Job lifecycle

| Status | Meaning |
|--------|---------|
| `pending` | Queued by user; awaiting worker |
| `processing` | Worker claimed job |
| `completed` | Artifact stored (`ArtifactZip` or `ArtifactJson`) |
| `failed` | Generation error recorded in `ErrorMessage` |

## Configuration

- API: export roles (`tenant_admin`, `maintainarr_admin`, `maintainarr_manager`) for job create/status/download; `shared-worker` bearer for internal batch
- Worker: `MaintainArrAuditPackageGeneration__MaintainArrBaseUrl`, `MaintainArrAuditPackageGeneration__ServiceToken`
- Render: env on `shared-worker` service in `render.yaml`

## Sync export

`GET /api/audit-packages/export` remains for immediate ZIP/JSON downloads; background jobs target large tenant packages without blocking the browser request.
