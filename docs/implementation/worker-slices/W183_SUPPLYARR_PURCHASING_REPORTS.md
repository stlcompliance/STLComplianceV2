# Worker 183 — SupplyArr purchasing reports (M12)

**Products:** SupplyArr API, supplyarr-frontend  
**Milestone:** M12  
**Backlog:** SupplyArr purchasing reports

## Summary

Tenant-scoped procurement pipeline rollup reports across purchase requests, purchase orders, receiving receipts, and backorders with summary, document drill-down, CSV export, audit logging, and Reports workspace panel.

## Backend (SupplyArr)

### APIs (JWT)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/reports/purchasing/summary` | Pipeline totals, status counts, PR/PO document list (`openDocumentsOnly`, `vendorPartyId`) |
| GET | `/api/reports/purchasing/summary/export` | CSV export of document rows |
| GET | `/api/reports/purchasing/purchase-requests/{purchaseRequestId}` | PR lines + linked PO |
| GET | `/api/reports/purchasing/purchase-orders/{purchaseOrderId}` | PO lines, receiving receipts, backorders |

### Authorization

- read: `RequirePurchasingReportRead` (purchase request read roles)
- export: `RequirePurchasingReportExport` (purchase request read roles)

### Audit actions

- `supplyarr.reports.purchasing.summary`
- `supplyarr.reports.purchasing.export`
- `supplyarr.reports.purchasing.purchase_request_detail`
- `supplyarr.reports.purchasing.purchase_order_detail`

## Frontend (supplyarr-frontend)

- `PurchasingReportsPanel` on Reports workspace
- API client: `getPurchasingReportSummary`, `getPurchasingPurchaseRequestDetail`, `getPurchasingPurchaseOrderDetail`, `exportPurchasingReportSummaryCsv`

## Tests

- `SupplyArrPurchasingReportTests` — summary, PR detail, PO detail, CSV export, unauthorized
- `PurchasingReportsPanel.test.tsx` — panel render

## Next slice

Per backlog: SupplyArr compliance reports, forgiving search, or audit history.
