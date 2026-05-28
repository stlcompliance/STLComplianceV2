# Worker 208 — NexArr M12 platform lifecycle workers (umbrella)

## Slice name

M12 platform lifecycle cluster — unified platform-admin overview for service-token cleanup, entitlement reconciliation, and tenant lifecycle workers (implements backlog gap closure atop Workers 168–170)

## Products touched

- **NexArr API** (`apps/nexarr-api`): `PlatformLifecycleOverviewService`, `GET /api/platform-admin/platform-lifecycle/overview`
- **shared-worker** (existing): `NexArrServiceTokenCleanupJob`, `NexArrEntitlementReconciliationJob`, `NexArrTenantLifecycleJob`
- **Suite Frontend** (`apps/suite-frontend`): `PlatformLifecycleOverviewPanel`, `/app/platform-admin/lifecycle`
- **Tests** (`tests/STLCompliance.NexArr.Auth.Tests`): `NexArrPlatformLifecycleOverviewTests`

## Prior worker slices (foundation — not reimplemented in W208)

| Worker | Backlog item | Scope |
|--------|--------------|-------|
| 168 | Service-token cleanup | `nexarr.service_tokens.cleanup.purge`, settings/runs UI |
| 169 | Entitlement reconciliation | `nexarr.entitlements.reconcile`, licenses + drift UI |
| 170 | Tenant lifecycle | `nexarr.tenants.lifecycle.process`, suspend/reactivate UI |

W208 adds a **single overview surface** so operators see all three workers without re-opening each settings page.

## Schema

No new tables in W208.

## API + auth

| Method | Path | Auth |
|--------|------|------|
| GET | `/api/platform-admin/platform-lifecycle/overview` | Platform admin JWT |

Response aggregates per worker: enabled flag, pending sample count, latest run summary, service-token scope, deep-link paths.

### Audit

- `platform_lifecycle.overview.read`

## Shared worker (unchanged, verified wired)

| Job | Config section | Service token scope |
|-----|----------------|---------------------|
| `NexArrServiceTokenCleanupJob` | `NexArrServiceTokenCleanup` | `nexarr.service_tokens.cleanup.purge` |
| `NexArrEntitlementReconciliationJob` | `NexArrEntitlementReconciliation` | `nexarr.entitlements.reconcile` |
| `NexArrTenantLifecycleJob` | `NexArrTenantLifecycle` | `nexarr.tenants.lifecycle.process` |

Internal batch routes remain on `/api/internal/*` per worker slice docs (W168–170).

## Frontend

- Route: `/app/platform-admin/lifecycle`
- Nav: **Lifecycle workers** in platform-admin shell
- Detail pages unchanged: `/service-tokens`, `/entitlements`, `/tenant-lifecycle`

## Tests

- `NexArrPlatformLifecycleOverviewTests` — forbidden for tenant admin, overview lists three workers + scopes
- `PlatformLifecycleOverviewPanel.test.tsx` — renders worker card

## Next slice

Per suite backlog scan after NexArr M12 lifecycle cluster:

- **RoutArr M9/M12** — dispatch command center and reporting (greenfield)
- **TrainArr M12** — person training history, notification settings, remaining workers
- **Compliance Core M12** — source ingestion, rule change monitoring
