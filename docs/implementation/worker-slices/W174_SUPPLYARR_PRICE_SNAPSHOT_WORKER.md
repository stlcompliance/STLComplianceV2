# Worker 174 — SupplyArr price snapshot worker (M12)

**Products:** SupplyArr, shared-worker, supplyarr-frontend  
**Milestone:** M12  
**Backlog:** SupplyArr `[M12] price snapshot worker`

## Summary

Scheduled worker captures vendor catalog prices on part vendor links into pricing snapshot history when catalog prices drift from the current effective snapshot. Tenant admins configure enablement and staleness; procurement admins maintain catalog reference prices on vendor links.

## Backend (SupplyArr)

### Schema

Migration: `SupplyArrPriceSnapshotWorker`

- `supplyarr_tenant_price_snapshot_settings` — tenant worker policy
- `supplyarr_part_vendor_price_capture_states` — last captured catalog price per vendor link
- `supplyarr_price_snapshot_runs` — batch run audit
- `supplyarr_part_vendor_links` columns: `catalog_unit_price`, `catalog_currency_code`, `catalog_minimum_order_quantity`

### Tenant admin APIs (JWT + SupplyArr admin)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/price-snapshot-settings` | Read worker settings |
| PUT | `/api/price-snapshot-settings` | Upsert worker settings |
| GET | `/api/price-snapshot-settings/pending` | Preview pending catalog captures |
| GET | `/api/price-snapshot-settings/runs` | Recent worker runs |

### Catalog price API (JWT + parts manage)

| Method | Path | Purpose |
|--------|------|---------|
| PUT | `/api/parts/{partId}/vendor-links/{linkId}/catalog-price` | Upsert vendor link catalog reference price |

### Internal APIs (service token)

| Method | Path | Scope |
|--------|------|-------|
| GET | `/api/internal/price-snapshots/pending` | `supplyarr.pricing.snapshots.capture` |
| POST | `/api/internal/price-snapshots/process-batch` | same |

## Shared worker

- `SupplyArrPriceSnapshotJob` — default 60 min interval, batch 100, staleness 24h
- Config: `SupplyArrPriceSnapshot__SupplyArrBaseUrl`, `SupplyArrPriceSnapshot__ServiceToken`

## Frontend (supplyarr-frontend)

- Settings → `PriceSnapshotSettingsPanel` — enable toggle, staleness, pending/runs preview

## Tests

- `PriceSnapshotCaptureRulesTests` — drift detection, snapshot key format
- `SupplyArrPriceSnapshotWorkerTests` — auth, pending preview, batch capture, capture state
- `PriceSnapshotSettingsPanel.test.tsx` — panel render

## Next slice

Per backlog: SupplyArr `[M12] lead-time snapshot worker` or RoutArr `[M12] trip completion rollup worker`.
