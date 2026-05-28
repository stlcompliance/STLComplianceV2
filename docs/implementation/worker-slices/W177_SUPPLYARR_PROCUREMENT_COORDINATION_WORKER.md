# Worker 177 — SupplyArr procurement coordination worker (M12)

**Products:** SupplyArr, shared-worker, supplyarr-frontend  
**Milestone:** M12  
**Backlog:** SupplyArr `[M12] procurement coordination worker`

## Summary

Scheduled worker materializes PR/PO pipeline coordination state for active procurement workflows, including coordination stage, next required action, receiving progress, and milestone events. Tenant admins configure enablement and staleness; purchasing users read materialized-first coordination dashboard APIs.

## Backend (SupplyArr)

### Schema

Migration: `SupplyArrProcurementCoordinationWorker`

- `supplyarr_tenant_procurement_coordination_settings` — tenant worker policy
- `supplyarr_procurement_coordination_records` — materialized coordination summary per PR or PO subject
- `supplyarr_procurement_coordination_events` — milestone events (submit/approve/issue/receipt)
- `supplyarr_procurement_coordination_runs` — batch run audit

### Tenant admin APIs (JWT + SupplyArr admin)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/procurement-coordination-settings` | Read worker settings |
| PUT | `/api/procurement-coordination-settings` | Upsert worker settings |
| GET | `/api/procurement-coordination-settings/pending` | Preview pending coordination refreshes |
| GET | `/api/procurement-coordination-settings/runs` | Recent worker runs |

### Coordination read APIs (JWT + purchase read)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/procurement-coordination` | Dashboard with stage counts and active items |
| GET | `/api/procurement-coordination/{subjectType}/{subjectId}` | Detail + events |

### Internal APIs (service token)

| Method | Path | Scope |
|--------|------|-------|
| GET | `/api/internal/procurement-coordination/pending` | `supplyarr.procurement.coordination` |
| POST | `/api/internal/procurement-coordination/process-batch` | same |

## Shared worker

- `SupplyArrProcurementCoordinationJob` — default 30 min interval, batch 50, staleness 2h
- Config: `SupplyArrProcurementCoordination__SupplyArrBaseUrl`, `SupplyArrProcurementCoordination__ServiceToken`

## Frontend (supplyarr-frontend)

- Settings → `ProcurementCoordinationSettingsPanel` — enable toggle, staleness, pending/runs preview
- Purchasing → `ProcurementCoordinationPanel` — active coordination dashboard

## Tests

- `ProcurementCoordinationRulesTests` — staleness, pending, receipt progress, terminal stages
- `SupplyArrProcurementCoordinationWorkerTests` — auth, pending preview, batch materialize, read APIs
- `ProcurementCoordinationSettingsPanel.test.tsx` — panel render
- `ProcurementCoordinationPanel.test.tsx` — dashboard render

## Next slice

Per backlog: SupplyArr `[M12] approval reminder worker`.
