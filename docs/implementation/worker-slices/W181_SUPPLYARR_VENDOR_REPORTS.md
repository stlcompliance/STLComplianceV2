# Worker 181 — SupplyArr vendor reports (M12)

**Products:** SupplyArr API, supplyarr-frontend  
**Milestone:** M12  
**Backlog:** SupplyArr vendor reports (first M12 reporting slice)

## Summary

Tenant-scoped vendor procurement rollup reports with summary, per-vendor detail, CSV export, audit logging, and a Reports workspace panel.

## Backend (SupplyArr)

### APIs (JWT)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/reports/vendors/summary` | Vendor rollup summary (`approvalStatus`, `activeOnly` filters) |
| GET | `/api/reports/vendors/{vendorPartyId}` | Per-vendor detail with recent PR/PO rows and catalog links |
| GET | `/api/reports/vendors/summary/export` | CSV export of summary rows |

### Authorization

- read: `RequireVendorReportRead` (party read roles)
- export: `RequireVendorReportExport` (purchase request read roles)

### Audit actions

- `supplyarr.reports.vendor.summary`
- `supplyarr.reports.vendor.detail`
- `supplyarr.reports.vendor.export`

## Frontend (supplyarr-frontend)

- Workspace section `reports` with `VendorReportsPanel`
- Nav item Reports (`/reports`)
- API client: `getVendorReportSummary`, `getVendorReportDetail`, `exportVendorReportSummaryCsv`

## Tests

- `SupplyArrVendorReportTests` — summary, detail, CSV export, unauthorized
- `VendorReportsPanel.test.tsx` — panel render

## Next slice

Per backlog: SupplyArr parts/inventory reports, purchasing reports, compliance reports, forgiving search, or audit history.
