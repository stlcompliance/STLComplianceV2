# Worker 170 — NexArr tenant lifecycle worker (M12)

**Products:** NexArr, shared-worker, suite-frontend  
**Milestone:** M12  
**Backlog:** NexArr `[M12] tenant lifecycle worker`

## Summary

Automates tenant suspension when all product licenses have lapsed beyond a configurable grace period, and reactivation when a valid license returns. Optionally revokes active user sessions on suspend. Platform admins configure behavior and review pending actions and run history from the suite platform-admin UI. The shared worker calls NexArr internal batch APIs with scope `nexarr.tenants.lifecycle.process`.

## Backend (NexArr)

### Schema

- `nexarr_platform_tenant_lifecycle_settings` — singleton platform settings
- `nexarr_tenant_lifecycle_runs` — batch run audit

### Platform admin APIs (JWT + platform admin)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/platform-admin/tenant-lifecycle/settings` | Read lifecycle settings |
| PUT | `/api/platform-admin/tenant-lifecycle/settings` | Upsert lifecycle settings |
| GET | `/api/platform-admin/tenant-lifecycle/runs` | Recent lifecycle runs |
| GET | `/api/platform-admin/tenant-lifecycle/pending` | Preview pending suspend/reactivate actions |

### Internal APIs (service token)

| Method | Path | Scope |
|--------|------|-------|
| GET | `/api/internal/tenant-lifecycle/pending` | `nexarr.tenants.lifecycle.process` |
| POST | `/api/internal/tenant-lifecycle/process-batch` | same |

## Shared worker

- `NexArrTenantLifecycleJob` — default 60 min interval, batch 25
- Config: `NexArrTenantLifecycle__NexArrBaseUrl`, `NexArrTenantLifecycle__ServiceToken`

## Frontend (suite-frontend)

- Route: `/app/platform-admin/tenant-lifecycle`
- `TenantLifecycleSettingsPanel` — enable toggle, auto-suspend/reactivate, grace days, session revoke, pending preview, recent runs

## Tests

- `NexArrTenantLifecycleTests` — auth, pending suspend, batch suspend + session revoke, reactivate
- `TenantLifecycleRulesTests` — license validity and action kind rules
- `TenantLifecycleSettingsPanel.test.tsx` — frontend panel

## Next slice

Per backlog: remaining M12 items across products or next milestone rows from `02_PRODUCT_IMPLEMENTATION_BACKLOG.md`.
