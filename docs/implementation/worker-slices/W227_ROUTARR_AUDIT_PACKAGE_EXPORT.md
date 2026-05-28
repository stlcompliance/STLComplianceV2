# Worker 227 — RoutArr M12 audit package export

Mirrors **NexArr W136/W226** (filters, CSV/JSON/ZIP) and **MaintainArr W132** (async generation jobs) for tenant-scoped RoutArr audit events only.

## Scope

- **Sync APIs** — `GET /api/audit-packages/manifest`, `/filter-options`, `/summary`, `/timeline`, `/export` (`zip` default, `json`, `csv`)
- **Async jobs** — `routarr_audit_package_generation_jobs`, POST/GET job + download; `FilterJson` stores full filter set
- **Internal worker** — `GET/POST /api/internal/audit-package-jobs/*`, service token `routarr.audit_packages.generate`
- **shared-worker** — `RoutArrAuditPackageGenerationJob` + `StlIntegrationTokenCatalog` profile `worker-routarr-audit-packages`
- **routarr-frontend** — `AuditPackageExportPanel` on Reports workspace (read: dispatcher+; export: manager/admin)
- **Tests** — `RoutArrAuditPackageTests`, `RoutArrAuditPackageGenerationRulesTests`, Vitest panel test

## Package sections

| Section | File |
|---------|------|
| Audit events (JSON) | `audit_events.json` |
| Audit events (CSV) | `audit_events.csv` |

ZIP also includes `manifest.json` with package metadata and applied filters.

## Authorization

| Action | Roles |
|--------|-------|
| Read (manifest, filters, summary, timeline) | `routarr_dispatcher` and above (same as dispatch reports read) |
| Export / jobs | `routarr_manager`, `routarr_admin`, `tenant_admin` (dispatch report export) |

## Configuration

- Worker: `RoutArrAuditPackageGeneration__RoutArrBaseUrl`, `RoutArrAuditPackageGeneration__ServiceToken`
- Render: env on `shared-worker` in `render.yaml`

## Verification

```powershell
dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~AuditPackage"
cd apps/routarr-frontend
npm test -- AuditPackageExportPanel
```

## Out of scope

- Full entity bundles (trips/routes/exceptions CSV exports remain under `/api/exports/*`)
- Cross-tenant platform audit export (NexArr W226)
