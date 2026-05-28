# Worker 179 — SupplyArr demand processing worker (M12)

**Products:** SupplyArr, shared-worker, supplyarr-frontend  
**Milestone:** M12  
**Backlog:** SupplyArr `[M12] demand processing worker`

## Summary

Scheduled worker evaluates stock availability for received MaintainArr demand references, materializes processing outcomes and recommendations, optionally auto-creates purchase request drafts when stock is short, and enqueues webhook notifications.

## Backend (SupplyArr)

### Schema

Migration: `SupplyArrDemandProcessingWorker`

- `supplyarr_tenant_demand_processing_settings` — tenant worker policy
- `supplyarr_demand_processing_states` — per demand-ref processing summary
- `supplyarr_demand_processing_runs` — batch run audit

### Tenant admin APIs (JWT + SupplyArr admin)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/demand-processing-settings` | Read worker settings |
| PUT | `/api/demand-processing-settings` | Upsert worker settings |
| GET | `/api/demand-processing-settings/pending` | Preview pending demand refs |
| GET | `/api/demand-processing-settings/runs` | Recent worker runs |

### Read APIs (JWT + demand ref read)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/demand-processing` | Dashboard of processing states |
| GET | `/api/demand-processing/{demandRefId}` | Detail with line stock summaries |

### Internal APIs (service token)

| Method | Path | Scope |
|--------|------|-------|
| GET | `/api/internal/demand-processing/pending` | `supplyarr.demand.process` |
| POST | `/api/internal/demand-processing/process-batch` | same |

## Shared worker

- `SupplyArrDemandProcessingJob` — default 30 min interval, batch 50, staleness 4h
- Config: `SupplyArrDemandProcessing__SupplyArrBaseUrl`, `SupplyArrDemandProcessing__ServiceToken`

## Frontend (supplyarr-frontend)

- Settings → `DemandProcessingSettingsPanel` — enable toggle, auto PR draft, staleness, pending/runs preview
- Purchasing → `DemandProcessingPanel` — processing outcome dashboard

## Tests

- `DemandProcessingRulesTests` — staleness, min hours, outcome resolution
- `SupplyArrDemandProcessingWorkerTests` — auth, pending preview, batch stock check + auto PR draft, dashboard read
- `DemandProcessingSettingsPanel.test.tsx` — panel render
- `DemandProcessingPanel.test.tsx` — dashboard render

## Next slice

Per backlog: SupplyArr M12 remaining items or next product milestone per `00_SLICE_STATE.md`.
