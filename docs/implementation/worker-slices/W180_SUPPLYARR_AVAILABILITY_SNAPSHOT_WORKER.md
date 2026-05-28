# Worker 180 — SupplyArr availability snapshot worker (M12)

**Products:** SupplyArr, shared-worker, supplyarr-frontend  
**Milestone:** M12  
**Backlog:** SupplyArr automated availability snapshot worker (completes W79 gap)

## Summary

Scheduled worker captures vendor catalog availability on part vendor links into availability snapshot history when catalog quantity or status drifts from the current effective snapshot. Tenant admins configure enablement and staleness; procurement admins maintain catalog reference availability on vendor links.

## Backend (SupplyArr)

### Schema

Migration: `SupplyArrAvailabilitySnapshotWorker`

- `supplyarr_tenant_availability_snapshot_settings` — tenant worker policy
- `supplyarr_part_vendor_availability_capture_states` — last captured catalog availability per vendor link
- `supplyarr_availability_snapshot_runs` — batch run audit
- `supplyarr_part_vendor_links` columns: `CatalogQuantityAvailable`, `CatalogAvailabilityStatus`

### Tenant admin APIs (JWT + SupplyArr admin)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/availability-snapshot-settings` | Read worker settings |
| PUT | `/api/availability-snapshot-settings` | Upsert worker settings |
| GET | `/api/availability-snapshot-settings/pending` | Preview pending catalog captures |
| GET | `/api/availability-snapshot-settings/runs` | Recent worker runs |

### Catalog availability API (JWT + parts manage)

| Method | Path | Purpose |
|--------|------|---------|
| PUT | `/api/parts/{partId}/vendor-links/{linkId}/catalog-availability` | Upsert vendor link catalog reference availability |

### Internal APIs (service token)

| Method | Path | Scope |
|--------|------|-------|
| GET | `/api/internal/availability-snapshots/pending` | `supplyarr.availability.snapshots.capture` |
| POST | `/api/internal/availability-snapshots/process-batch` | same |

## Shared worker

- `SupplyArrAvailabilitySnapshotJob` — default 60 min interval, batch 100, staleness 24h
- Config: `SupplyArrAvailabilitySnapshot__SupplyArrBaseUrl`, `SupplyArrAvailabilitySnapshot__ServiceToken`

## Frontend (supplyarr-frontend)

- Settings → `AvailabilitySnapshotSettingsPanel` — enable toggle, staleness, pending/runs preview

## Tests

- `AvailabilitySnapshotCaptureRulesTests` — drift detection, snapshot key format
- `SupplyArrAvailabilitySnapshotWorkerTests` — auth, pending preview, batch capture, capture state
- `AvailabilitySnapshotSettingsPanel.test.tsx` — panel render

## Next slice

Per backlog: SupplyArr M12 remaining items (vendor reports, parts/inventory reports, purchasing reports, compliance reports, forgiving search, audit history) or next product milestone per `00_SLICE_STATE.md`.
