# Worker 175 — SupplyArr lead-time snapshot worker (M12)

**Products:** SupplyArr, shared-worker, supplyarr-frontend  
**Milestone:** M12  
**Backlog:** SupplyArr `[M12] lead-time snapshot worker`

## Summary

Scheduled worker captures vendor catalog lead times on part vendor links into lead-time snapshot history when catalog lead times drift from the current effective snapshot. Tenant admins configure enablement and staleness; procurement admins maintain catalog reference lead times on vendor links.

## Backend (SupplyArr)

### Schema

Migration: `SupplyArrLeadTimeSnapshotWorker`

- `supplyarr_tenant_lead_time_snapshot_settings` — tenant worker policy
- `supplyarr_part_vendor_lead_time_capture_states` — last captured catalog lead time per vendor link
- `supplyarr_lead_time_snapshot_runs` — batch run audit
- `supplyarr_part_vendor_links` column: `CatalogLeadTimeDays`

### Tenant admin APIs (JWT + SupplyArr admin)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/lead-time-snapshot-settings` | Read worker settings |
| PUT | `/api/lead-time-snapshot-settings` | Upsert worker settings |
| GET | `/api/lead-time-snapshot-settings/pending` | Preview pending catalog captures |
| GET | `/api/lead-time-snapshot-settings/runs` | Recent worker runs |

### Catalog lead time API (JWT + parts manage)

| Method | Path | Purpose |
|--------|------|---------|
| PUT | `/api/parts/{partId}/vendor-links/{linkId}/catalog-lead-time` | Upsert vendor link catalog reference lead time |

### Internal APIs (service token)

| Method | Path | Scope |
|--------|------|-------|
| GET | `/api/internal/lead-time-snapshots/pending` | `supplyarr.leadtime.snapshots.capture` |
| POST | `/api/internal/lead-time-snapshots/process-batch` | same |

## Shared worker

- `SupplyArrLeadTimeSnapshotJob` — default 60 min interval, batch 100, staleness 24h
- Config: `SupplyArrLeadTimeSnapshot__SupplyArrBaseUrl`, `SupplyArrLeadTimeSnapshot__ServiceToken`

## Frontend (supplyarr-frontend)

- Settings → `LeadTimeSnapshotSettingsPanel` — enable toggle, staleness, pending/runs preview

## Tests

- `LeadTimeSnapshotCaptureRulesTests` — drift detection, snapshot key format
- `SupplyArrLeadTimeSnapshotWorkerTests` — auth, pending preview, batch capture, capture state
- `LeadTimeSnapshotSettingsPanel.test.tsx` — panel render

## Next slice

Per backlog: RoutArr `[M12] trip completion rollup worker` or SupplyArr `[M12] procurement coordination worker`.
