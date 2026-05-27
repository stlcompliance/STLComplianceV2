# Worker 6 — NexArr platform-admin APIs + suite platform-admin UI

## Slice name

M2/M3 Platform administration — `/api/platform-admin/*` and suite-frontend control-plane surfaces

## Products touched

- **NexArr** (primary): platform-admin read APIs, audit on sensitive reads
- **Suite Frontend** (`apps/suite-frontend`): platform-admin routes under `/app/platform-admin`

## Schema

None (aggregates existing M2 tables).

## APIs

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/platform-admin/dashboard` | JWT; `stl_platform_admin` |
| GET | `/api/platform-admin/launch-diagnostics?tenantId=&productKey=&page=&pageSize=` | JWT; platform admin |
| GET | `/api/platform-admin/overview/tenants?page=&pageSize=` | JWT; platform admin |
| GET | `/api/platform-admin/overview/products` | JWT; platform admin |

## Permissions

- All routes require platform administrator (`stl_platform_admin=true` and active user).
- Tenant admins receive **403 Forbidden** (covered by integration tests).

## Audit

Sensitive reads write `platform_audit_events`:

- `platform_admin.dashboard.read`
- `platform_admin.launch_diagnostics.read`
- `platform_admin.overview.tenants.read`
- `platform_admin.overview.products.read`

## UI (suite-frontend)

| Route | Page |
|-------|------|
| `/app/platform-admin` | Dashboard summary cards (live API) |
| `/app/platform-admin/launch` | Launch diagnostics table + issues |
| `/app/platform-admin/tenants` | Tenant overview table |
| `/app/platform-admin/products` | Product overview table |

- Sidebar **Platform admin** link visible only when `me.isPlatformAdmin` (`PermissionGate`).
- `RequirePlatformAdmin` guards nested routes.
- API client: `getPlatformAdminDashboard`, `getPlatformAdminLaunchDiagnostics`, `getPlatformAdminTenantOverview`, `getPlatformAdminProductOverview`.

## Tests

- `tests/STLCompliance.NexArr.Auth.Tests/NexArrPlatformAdminApiTests.cs` — auth required, platform admin happy path, tenant admin denied (InMemory DB).
- Existing suite-frontend Vitest (`permissions.test.ts`, `authStorage.test.ts`).

## Local dev

```powershell
dotnet run --project apps/nexarr-api/NexArr.Api/NexArr.Api.csproj
cd apps/suite-frontend
npm run dev
# Sign in as admin@demo.stl — open Platform admin in sidebar
```

## Gaps / next (Worker 7+)

- Unified suite home dashboard widgets (M3)
- Platform audit search/export UI
- CRUD surfaces in suite for tenants/products/entitlements (today: NexArr APIs only; admin via API or future UI)
- Shared `packages/ui` design system extraction
- Playwright E2E against docker-compose stack
- First product shell (e.g. StaffArr) consuming handoff redeem
