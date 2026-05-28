# Worker 182 — SupplyArr parts/inventory reports (M12)

**Products:** SupplyArr API, supplyarr-frontend  
**Milestone:** M12  
**Backlog:** SupplyArr parts/inventory reports

## Summary

Tenant-scoped parts catalog and inventory rollup reports with summary, per-part and per-location detail, CSV export, audit logging, and Reports workspace panel.

## Backend (SupplyArr)

### APIs (JWT)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/reports/parts-inventory/summary` | Parts + location rollups (`activePartsOnly`, `belowReorderOnly`, `inventoryLocationId`) |
| GET | `/api/reports/parts-inventory/parts/{partId}` | Part stock-by-bin and vendor links |
| GET | `/api/reports/parts-inventory/locations/{inventoryLocationId}` | Location bins and stocked parts |
| GET | `/api/reports/parts-inventory/summary/export` | CSV export of filtered part rows |

### Authorization

- read: `RequirePartsInventoryReportRead` (inventory read roles)
- export: `RequirePartsInventoryReportExport` (inventory read roles)

### Audit actions

- `supplyarr.reports.parts_inventory.summary`
- `supplyarr.reports.parts_inventory.part_detail`
- `supplyarr.reports.parts_inventory.location_detail`
- `supplyarr.reports.parts_inventory.export`

## Frontend (supplyarr-frontend)

- `PartsInventoryReportsPanel` on Reports workspace (with vendor reports)
- API client: `getPartsInventoryReportSummary`, `getPartsInventoryPartDetail`, `getPartsInventoryLocationDetail`, `exportPartsInventoryReportSummaryCsv`

## Tests

- `SupplyArrPartsInventoryReportTests` — summary, part detail, location detail, CSV export, unauthorized
- `PartsInventoryReportsPanel.test.tsx` — panel render

## Next slice

Per backlog: SupplyArr purchasing reports, compliance reports, forgiving search, or audit history.
