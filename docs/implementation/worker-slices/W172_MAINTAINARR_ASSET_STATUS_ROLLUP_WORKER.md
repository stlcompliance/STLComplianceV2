# Worker 172 — MaintainArr asset status rollup worker (M12)

**Products:** MaintainArr, shared-worker, maintainarr-frontend  
**Milestone:** M12  
**Backlog:** MaintainArr `[M12] asset status rollup worker`

## Summary

Scheduled worker materializes per-asset readiness status and fleet/type/class/site scope rollups. Tenant admins configure staleness and enable the worker; asset readiness reads prefer fresh materialized rows when available.

## Backend (MaintainArr)

### Schema

- `maintainarr_tenant_asset_status_rollup_settings` — tenant rollup policy
- `maintainarr_asset_status_rollups` — per-asset materialized readiness
- `maintainarr_asset_status_scope_rollups` — fleet, asset type, asset class, site aggregates
- `maintainarr_asset_status_rollup_runs` — batch run audit

### Tenant admin APIs (JWT + maintainarr admin)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/asset-status-rollup-settings` | Read rollup settings |
| PUT | `/api/asset-status-rollup-settings` | Upsert rollup settings |
| GET | `/api/asset-status-rollup-settings/pending` | Preview pending asset refreshes |
| GET | `/api/asset-status-rollup-settings/runs` | Recent worker runs |

### Public rollup read APIs (JWT + asset read)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/asset-status-rollups/fleet` | Fleet rollup summary |
| GET | `/api/asset-status-rollups/types` | Asset type rollups |
| GET | `/api/asset-status-rollups/types/{assetTypeId}` | Single type rollup |
| GET | `/api/asset-status-rollups/classes` | Asset class rollups |
| GET | `/api/asset-status-rollups/sites` | Site rollups |
| GET | `/api/asset-status-rollups/assets` | Per-asset rollups |
| GET | `/api/asset-status-rollups/assets/{assetId}` | Single asset rollup |

### Internal APIs (service token)

| Method | Path | Scope |
|--------|------|-------|
| GET | `/api/internal/asset-status-rollups/pending` | `maintainarr.asset_status.rollup` |
| POST | `/api/internal/asset-status-rollups/process-batch` | same |

## Shared worker

- `MaintainArrAssetStatusRollupJob` — default 30 min interval, batch 50, staleness 1h
- Config: `MaintainArrAssetStatusRollup__MaintainArrBaseUrl`, `MaintainArrAssetStatusRollup__ServiceToken`

## Frontend (maintainarr-frontend)

- Settings → `AssetStatusRollupSettingsPanel` — enable toggle, staleness, pending/runs preview, fleet snapshot

## Tests

- `AssetStatusRollupRulesTests` — staleness, aggregation, site key normalization
- `MaintainArrAssetStatusRollupWorkerTests` — auth, pending preview, batch refresh, fleet rollup
- `AssetStatusRollupSettingsPanel.test.tsx` — panel render

## Next slice

Per backlog: MaintainArr `[M12] maintenance history rollup worker` or next open M12 row from `02_PRODUCT_IMPLEMENTATION_BACKLOG.md`.
