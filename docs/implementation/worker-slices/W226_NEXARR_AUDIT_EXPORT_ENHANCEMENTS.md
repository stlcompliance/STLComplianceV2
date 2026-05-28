# Worker 226 — NexArr M12 audit export enhancements

Builds on **Worker 136** (`W136_NEXARR_PLATFORM_AUDIT_EXPORT.md`).

## Scope

- **Richer filters** — `action`, `result`, `targetType`, `actorUserId`, `productKey` (plus existing `tenantId`, `from`, `to`) on timeline, summary, sync export, and async jobs
- **CSV export** — `GET /api/platform-admin/audit-packages/export?format=csv` and `platform_audit_events.csv` inside ZIP packages
- **JSON packages** — manifest v2, `appliedFilters` on export payload, filter persistence on generation jobs (`FilterJson`)
- **Discovery APIs** — `GET /filter-options`, `GET /summary` with counts and by-result/by-action breakdown
- **Suite UI** — filter dropdowns, export summary, Download audit CSV, Download JSON package, manifest v2 display
- **Tests** — extended `NexArrPlatformAuditPackageTests`, `PlatformAuditPackageExportPanel.test.tsx`, E2E summary assertion

## API surface

| Method | Path | Notes |
|--------|------|-------|
| GET | `/api/platform-admin/audit-packages/manifest` | Package version `2`, CSV section |
| GET | `/api/platform-admin/audit-packages/filter-options` | Distinct actions/results/target types + product keys |
| GET | `/api/platform-admin/audit-packages/summary` | Scoped counts + breakdown |
| GET | `/api/platform-admin/audit-packages/timeline` | All filter query params |
| GET | `/api/platform-admin/audit-packages/export` | `format=zip` (default), `json`, `csv` |
| POST | `/api/platform-admin/audit-packages/jobs` | Body includes all filter fields; stored as `FilterJson` |

Platform admin JWT required on all routes.

## Migration

`20260528141152_NexArrPlatformAuditExportEnhancements` — `FilterJson` on `nexarr_platform_audit_package_generation_jobs`.

## Verification

```powershell
dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~PlatformAudit"
cd apps/suite-frontend
npm test -- PlatformAuditPackageExportPanel
```

Live E2E (optional):

```powershell
# E2E_LIVE=1 with stack up
npx playwright test tests/e2e-playwright/tests/platform-admin-audit-export-smoke.spec.ts
```

## Out of scope

- Per-tenant product operational audit packages (StaffArr, MaintainArr, etc.)
- Scheduled delivery to external object storage
- RoutArr audit bundle export (separate worker backlog)
