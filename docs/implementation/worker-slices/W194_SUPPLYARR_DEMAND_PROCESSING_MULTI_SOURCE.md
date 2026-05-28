# Worker 194 — SupplyArr demand processing multi-source (M8/M10/M12)

**Products:** SupplyArr, shared-worker, supplyarr-frontend  
**Milestone:** M8 (procurement automation), extends W179  
**Depends on:** W179 (demand processing worker), W181–193 (demand intake + status callbacks)

## Summary

Extends the W179 demand processing worker so pending `received` demand references from **MaintainArr, RoutArr, TrainArr, and StaffArr** are evaluated in one batch. Auto PR draft creation uses the correct per-source intake service. Each source is tenant-configurable; MaintainArr remains enabled by default for backward compatibility.

## Backend (SupplyArr)

### Schema

Migration: `SupplyArrDemandProcessingMultiSource`

- `supplyarr_tenant_demand_processing_settings` — `ProcessMaintainarrDemandRefs` (default true), `ProcessRoutarrDemandRefs`, `ProcessTrainarrDemandRefs`, `ProcessStaffarrDemandRefs`
- `supplyarr_demand_processing_states` — `DemandRefSource`; FK to `supplyarr_maintainarr_demand_refs` removed (states may reference any source table)
- Index `(TenantId, DemandRefSource)` on processing states

### Worker behavior (`DemandProcessingWorkerService`)

- Pending scan: all four `*DemandRefs` tables where `status == received`, `PurchaseRequestId == null`, tenant `IsEnabled`, per-source flag, existing min-hours / staleness rules
- Batch merges candidates across enabled sources, orders by `LastProcessedAt` / `ReceivedAt`, applies `batchSize`
- Auto PR: `MaintainArrDemandIntakeService`, `RoutArrDemandIntakeService`, `TrainArrDemandIntakeService`, `StaffArrDemandIntakeService` by `DemandRefSource`
- MaintainArr-only procurement webhook on auto PR (`NotifyOnPrDraftCreated`); other sources use W193 status callbacks via intake
- Idempotency: unique `(tenantId, demandRefId)` on `supplyarr_demand_processing_states`

### APIs (unchanged paths)

Internal batch endpoints unchanged — shared-worker `SupplyArrDemandProcessingJob` still calls:

| Method | Path | Scope |
|--------|------|-------|
| GET | `/api/internal/demand-processing/pending` | `supplyarr.demand.process` |
| POST | `/api/internal/demand-processing/process-batch` | same |

Contract additions: `demandRefSource`, `sourceRefKey` on pending/results/dashboard; source toggles on settings GET/PUT.

### Dashboard (`DemandProcessingService`)

Multi-source status lookup and detail line loading by `DemandRefSource`.

## Shared worker

**No code changes.** Job and client from W179:

- `SupplyArrDemandProcessingJob` — default 30 min interval
- Config: `SupplyArrDemandProcessing__SupplyArrBaseUrl`, `SupplyArrDemandProcessing__ServiceToken`
- Optional: `SupplyArrDemandProcessing__BatchSize`, `SupplyArrDemandProcessing__StalenessHours`

## Frontend (supplyarr-frontend)

- `DemandProcessingSettingsPanel` — four source checkboxes + updated copy
- `DemandProcessingPanel` — shows `demandRefSource` and `sourceRefKey`
- API types updated for new settings and summary fields

## Tests

- `DemandProcessingRulesTests` — `IsSourceEnabled` per source flags
- `SupplyArrDemandProcessingWorkerTests` — MaintainArr regression + **RoutArr auto PR** with MaintainArr disabled
- `DemandProcessingSettingsPanel.test.tsx`, `DemandProcessingPanel.test.tsx` — updated mocks

## Tenant defaults

| Setting | Default |
|---------|---------|
| `ProcessMaintainarrDemandRefs` | `true` |
| `ProcessRoutarrDemandRefs` | `false` |
| `ProcessTrainarrDemandRefs` | `false` |
| `ProcessStaffarrDemandRefs` | `false` |

Enable additional sources in SupplyArr Settings → Demand processing worker.

## Next slice

Per `00_SLICE_STATE.md`: remaining **M8** procurement automation (vendor restrictions, PO coordination depth) or next milestone backlog item.
