# Worker 230 — MaintainArr M12 audit export filter/summary parity

Builds on **W132** (async audit package generation). Mirrors **W226** (NexArr), **W227** (RoutArr), **W228** (StaffArr) filter/CSV/manifest v2 pattern.

## Scope

- **Richer audit-event filters** — `action`, `result`, `targetType`, `actorUserId` plus `from`/`to` on timeline, summary, sync export, and async jobs
- **CSV export** — `GET /api/audit-packages/export?format=csv` and `audit_events.csv` inside ZIP (manifest v2)
- **Discovery APIs** — `GET /filter-options`, `GET /summary`, `GET /timeline` with maintenance bundle counts
- **Job persistence** — `FilterJson` on `maintainarr_audit_package_generation_jobs`
- **maintainarr-frontend** — `AuditPackageExportPanel` on Settings (W226-style filters, summary, timeline preview, CSV/JSON/ZIP/background job)
- **Tests** — `MaintainArrAuditPackageTests`, extended generation tests, Vitest panel test

## Package sections (manifest v2)

Audit events (JSON + CSV), assets, work orders, defects, inspection runs, PM schedules (7 sections + `manifest.json` → 8 ZIP entries).

## Authorization

| Action | Roles |
|--------|-------|
| Read | `RequireAuditPackageRead` (manager+ with maintainarr entitlement) |
| Export / jobs | tenant admin, maintainarr admin, maintainarr manager |

Worker: existing W132 `MaintainArrAuditPackageGenerationJob` + `maintainarr.audit_packages.generate` (no new worker).

## Migration

`MaintainArrAuditPackageExportEnhancements` — `FilterJson` column on generation jobs.

## Verification

```powershell
dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~AuditPackage"
cd apps/maintainarr-frontend
npm test -- AuditPackageExportPanel
```

## Out of scope

- NexArr platform audit export (W226)
- RoutArr / StaffArr audit export (W227/W228)
- New async worker service (W132 unchanged)
