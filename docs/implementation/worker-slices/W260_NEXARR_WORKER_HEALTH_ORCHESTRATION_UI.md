# W260 — NexArr M12 platform-admin service token / worker health orchestration UI

## Slice name

M12 platform operations — unified suite platform-admin orchestration for product health probes, service token inventory, and NexArr lifecycle workers (builds on W93 platform health, W168–170 workers, W208 lifecycle overview).

## Products touched

- **NexArr API** (`apps/nexarr-api`): `PlatformWorkerHealthOrchestrationService`, `GET/POST /api/platform-admin/worker-health-orchestration/*`
- **Suite Frontend** (`apps/suite-frontend`): `PlatformWorkerHealthOrchestrationPanel`, `/app/platform-admin/orchestration`
- **Tests**: `NexArrPlatformWorkerHealthOrchestrationTests`, `PlatformWorkerHealthOrchestrationPanel.test.tsx`

## Schema

No new tables (read-only aggregation + manual triggers call existing worker services).

## API + auth

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| GET | `/api/platform-admin/worker-health-orchestration` | Platform admin JWT | Product `/health/ready` probes, token inventory counts, lifecycle worker status |
| POST | `/api/platform-admin/worker-health-orchestration/trigger-service-token-cleanup` | Platform admin JWT | Manual `ProcessBatchAsync` when cleanup enabled |
| POST | `/api/platform-admin/worker-health-orchestration/trigger-entitlement-reconciliation` | Platform admin JWT | Manual reconciliation batch when enabled |
| POST | `/api/platform-admin/worker-health-orchestration/trigger-tenant-lifecycle` | Platform admin JWT | Manual tenant lifecycle batch when enabled |

### Audit

- `platform_worker_health.orchestration.read`
- `platform_worker_health.trigger_service_token_cleanup`
- `platform_worker_health.trigger_entitlement_reconciliation`
- `platform_worker_health.trigger_tenant_lifecycle`

Manual triggers return **409** when the corresponding worker is disabled (operator must enable on settings page first).

## Frontend

- Route: `/app/platform-admin/orchestration`
- Nav: **Worker health** in platform-admin shell
- `data-testid`: `platform-worker-health-orchestration-panel`, product health / token inventory / per-worker trigger buttons

## Tests

```powershell
dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~PlatformWorkerHealthOrchestration"
cd apps/suite-frontend
npm test -- PlatformWorkerHealthOrchestrationPanel
```

## Out of scope

- ~~M13 Playwright smoke for orchestration panel (optional follow-up)~~ → **W262**
- Service token issue/revoke UI (existing admin APIs)
- Expanding orchestration to companion notification worker

## Next slice

- **M13 Playwright** — NexArr platform-admin worker health orchestration smoke → **W262 complete**
- **RoutArr M9** — proof photo/document/signature attachments → **W261 complete**
- ~~**M13 Playwright** — RoutArr trip execution settings panel smoke (optional `/settings` companion to W259)~~ → **W263 complete**
- **M13 Playwright** — RoutArr driver-portal attachment upload smoke (photo/signature path; builds on W261)
