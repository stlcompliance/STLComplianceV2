# Worker 204 — MaintainArr executive reports (M12)

**Products:** MaintainArr API, maintainarr-frontend  
**Milestone:** M12  
**Backlog:** MaintainArr executive reports (suite-level maintenance KPI rollups)  
**Reference pattern:** Worker 203 maintenance reports (read-only rollups + CSV + audit)

## Summary

Tenant-scoped executive dashboard aggregating fleet readiness, operational KPIs, scope-level readiness rollups, and SupplyArr parts-demand mirror stats from owned MaintainArr tables. No cross-product database access — SupplyArr linkage uses local `WorkOrderPartsDemandLine` reference fields only.

## Backend (MaintainArr)

### APIs (JWT)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/reports/executive/summary` | Executive KPI rollup |
| GET | `/api/reports/executive/summary/export` | CSV export (fleet KPIs + scope rows) |

### Data sources (owned tables)

- `AssetStatusScopeRollups` / `AssetStatusRollups` — fleet and scope readiness
- `Assets`, `WorkOrders`, `Defects`, `WorkOrderLaborEntries`, `PmSchedules` — operational totals
- `WorkOrderPartsDemandLines` — SupplyArr demand/procurement mirror refs (`SupplyarrDemandRefId`, procurement status)

### Authorization

- read: `RequireExecutiveReportRead` — tenant_admin, maintainarr_admin, maintainarr_manager (not technician)
- export: `RequireExecutiveReportExport` — audit package export roles

### Audit actions

- `maintainarr.reports.executive.summary`
- `maintainarr.reports.executive.export`

## Frontend (maintainarr-frontend)

- `ExecutiveReportsPanel` on Reports workspace (above maintenance reports panel)
- Session gates: `canReadExecutiveReports`, `canExportExecutiveReports`
- API client: `getExecutiveReportSummary`, `exportExecutiveReportSummaryCsv`

## Tests

- `MaintainArrExecutiveReportTests` — summary, CSV export, unauthorized
- `ExecutiveReportsPanel.test.tsx` — panel render and permission gate

## Next slice

Per suite backlog scan: **MaintainArr M12 compliance reports**, **imports/exports**, or **NexArr M12** platform lifecycle workers — see `00_SLICE_STATE.md`.
