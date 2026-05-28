# Worker 203 — MaintainArr maintenance reports (M12)

**Products:** MaintainArr API, maintainarr-frontend  
**Milestone:** M12  
**Backlog:** MaintainArr maintenance reports (first MaintainArr M12 reporting slice)  
**Reference pattern:** Worker 181 SupplyArr vendor reports (read-only rollups + CSV + audit)

## Summary

Tenant-scoped maintenance rollup reports across owned MaintainArr tables (assets, work orders, defects, inspection runs, PM schedules) with summary, entity detail routes, CSV export, audit logging, and a Reports workspace panel.

## Backend (MaintainArr)

### APIs (JWT)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/reports/maintenance/summary` | Fleet rollup (`lifecycleStatus` filter) |
| GET | `/api/reports/maintenance/summary/export` | CSV export of summary asset rows |
| GET | `/api/reports/maintenance/assets/{assetId}` | Per-asset detail with recent work orders, defects, inspections, PM |
| GET | `/api/reports/maintenance/work-orders/{workOrderId}` | Work order detail with labor/evidence totals |
| GET | `/api/reports/maintenance/defects/{defectId}` | Defect detail |
| GET | `/api/reports/maintenance/inspection-runs/{inspectionRunId}` | Inspection run detail |
| GET | `/api/reports/maintenance/pm-schedules/{pmScheduleId}` | PM schedule detail |

### Authorization

- read: `RequireMaintenanceReportRead` → assets read permission
- export: `RequireMaintenanceReportExport` → audit package export permission

### Audit actions

- `maintainarr.reports.maintenance.summary`
- `maintainarr.reports.maintenance.export`
- `maintainarr.reports.maintenance.asset.detail`
- `maintainarr.reports.maintenance.work_order.detail`
- `maintainarr.reports.maintenance.defect.detail`
- `maintainarr.reports.maintenance.inspection_run.detail`
- `maintainarr.reports.maintenance.pm_schedule.detail`

### Implementation

- `MaintenanceReportContracts.cs` — summary and detail DTOs
- `MaintenanceReportService.cs` — aggregations from owned EF tables; CSV builder
- `MaintenanceReportEndpoints.cs` — route mapping + audit on each handler

## Frontend (maintainarr-frontend)

- Workspace section `reports` with `MaintenanceReportsPanel`
- Nav item Reports (`/reports`)
- Session gates: `canReadMaintenanceReports`, `canExportMaintenanceReports`
- API client: `getMaintenanceReportSummary`, asset/work-order detail helpers, `exportMaintenanceReportSummaryCsv`

## Tests

- `MaintainArrMaintenanceReportTests` — summary, asset detail, work order detail, CSV export, unauthorized
- `MaintenanceReportsPanel.test.tsx` — panel render and permission gate

## Next slice

Per suite backlog: **Worker 204 executive reports** (cross-product / suite-level reporting), or additional MaintainArr M12 slices as prioritized.
