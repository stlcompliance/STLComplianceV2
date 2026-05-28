# Worker 168 — NexArr service-token cleanup worker (M12)

**Products:** NexArr, shared-worker, suite-frontend  
**Milestone:** M12  
**Backlog:** NexArr `[M12] service-token cleanup worker`

## Summary

Purges expired and revoked NexArr service token records after configurable grace periods. Platform admins enable cleanup and review run history from the suite platform-admin UI. The shared worker calls NexArr internal batch APIs with scope `nexarr.service_tokens.cleanup.purge`.

## Backend (NexArr)

### Schema

- `nexarr_platform_service_token_cleanup_settings` — singleton platform settings (`IsEnabled`, grace days)
- `nexarr_service_token_cleanup_runs` — batch run audit
- Index on `service_tokens.revoked_at`

### Platform admin APIs (JWT + platform admin)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/platform-admin/service-token-cleanup/settings` | Read cleanup settings |
| PUT | `/api/platform-admin/service-token-cleanup/settings` | Upsert cleanup settings |
| GET | `/api/platform-admin/service-token-cleanup/runs` | Recent cleanup runs |

### Internal APIs (service token)

| Method | Path | Scope |
|--------|------|-------|
| GET | `/api/internal/service-token-cleanup/pending` | `nexarr.service_tokens.cleanup.purge` |
| POST | `/api/internal/service-token-cleanup/process-batch` | same |

## Shared worker

- `NexArrServiceTokenCleanupJob` — default 60 min interval, batch 100
- Config: `NexArrServiceTokenCleanup__NexArrBaseUrl`, `NexArrServiceTokenCleanup__ServiceToken`

## Frontend (suite-frontend)

- Route: `/app/platform-admin/service-tokens`
- `ServiceTokenCleanupSettingsPanel` — enable toggle, grace days, recent runs

## Tests

- `NexArrServiceTokenCleanupTests` — auth, pending list, batch purge, run history
- `ServiceTokenCleanupRulesTests` — grace period rules
- `ServiceTokenCleanupSettingsPanel.test.tsx` — frontend panel

## Next slice

Per backlog: NexArr entitlement reconciliation worker or remaining M12 items.
