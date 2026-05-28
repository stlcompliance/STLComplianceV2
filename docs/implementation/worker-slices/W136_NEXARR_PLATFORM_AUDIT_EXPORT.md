# Worker 136 — NexArr platform audit export (M12)

## Scope

Control-plane audit packaging for platform administrators (W128 pattern on NexArr):

- **Sync APIs** — `GET /api/platform-admin/audit-packages/manifest`, `/timeline`, `/export` (ZIP/JSON)
- **Async jobs** — `nexarr_platform_audit_package_generation_jobs`, POST/GET job + download
- **Internal worker** — `GET/POST /api/internal/platform-audit-package-jobs/*`, `nexarr.platform_audit_packages.generate` service token
- **shared-worker** — `NexArrPlatformAuditPackageGenerationJob`
- **Suite UI** — `/app/platform-admin/audit-export` (`PlatformAuditPackageExportPanel`)
- **Tests** — `NexArrPlatformAuditPackageTests`, `PlatformAuditPackageGenerationRulesTests`

## Package sections

Platform audit events, tenants, entitlements, product catalog, platform users (no passwords), service clients, service token metadata (no hashes), launch profiles, callback allowlist.

Optional `tenantId` scopes audit events and tenant-scoped tables.

## Verification

```powershell
dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~PlatformAudit"
cd apps/suite-frontend
npm test
```

## Out of scope

- Per-tenant product operational audit packages (owned by StaffArr/MaintainArr/etc.)
- Automated scheduled platform audit delivery to external storage
