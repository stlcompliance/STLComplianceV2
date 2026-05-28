# Worker 228 — StaffArr M12 personnel audit package export enhancements

Builds on **W106** (export), **W126** (timeline), **W128** (async jobs). Mirrors **W226/W227** filter and CSV/JSON/ZIP enhancements.

## Scope

- **Richer audit-event filters** — `action`, `result`, `targetType`, `actorUserId` plus existing `from`/`to` on timeline, summary, sync export, and async jobs
- **CSV export** — `GET /api/audit-packages/export?format=csv` and `audit_events.csv` inside ZIP (manifest v2)
- **Discovery APIs** — `GET /filter-options`, `GET /summary` with workforce section counts and audit-event breakdown
- **Job persistence** — `FilterJson` on `staffarr_audit_package_generation_jobs`
- **staffarr-frontend** — enhanced `AuditPackageExportPanel` on Admin workspace (existing placement)
- **Tests** — extended `StaffArrAuditPackageTests`, Vitest panel test

## Package sections (unchanged workforce bundle)

Audit events (JSON + CSV), people, permission history, certifications, incidents, readiness overrides, training blockers.

## Authorization

| Action | Roles |
|--------|-------|
| Read | supervisor+ with audit read (existing `RequireAuditPackageRead`) |
| Export / jobs | tenant admin, staffarr admin, HR admin (`RequireAuditPackageExport`) |

Worker: existing `staffarr.audit_packages.generate` + `StaffArrAuditPackageGenerationJob`.

## Migration

`StaffArrAuditPackageExportEnhancements` — `FilterJson` column on generation jobs.

## Verification

```powershell
dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~AuditPackage"
cd apps/staffarr-frontend
npm test -- AuditPackageExportPanel
```

## Out of scope

- NexArr platform audit export (W226)
- RoutArr dispatch audit export (W227)
