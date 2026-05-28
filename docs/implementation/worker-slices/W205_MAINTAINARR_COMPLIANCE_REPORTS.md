# Worker 205 — MaintainArr compliance reports (M12)

**Products:** MaintainArr API, maintainarr-frontend  
**Milestone:** M12  
**Backlog:** MaintainArr compliance reports  
**Reference pattern:** Workers 203–204 maintenance/executive reports

## Summary

Tenant-scoped compliance rollups across inspection pass/fail rates, open defect severity (including inspection-sourced), PM adherence, and local Compliance Core regulatory key mirrors. No cross-product database access.

## Backend (MaintainArr)

### Persistence

- `maintainarr_compliance_regulatory_key_mirrors` — rebuildable mirrors linking `complianceKey` / `materialKey` / `regulatoryCitationKey` to MaintainArr subjects (`inspection_template`, `asset_type`, `pm_program`)

### APIs (JWT)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/reports/compliance/summary` | Compliance rollup (`attentionOnly`, `siteRef` filters) |
| GET | `/api/reports/compliance/inspection-templates/{inspectionTemplateId}` | Per-template compliance detail |
| GET | `/api/reports/compliance/summary/export` | CSV export |

### Authorization

- read: `RequireComplianceReportRead` — manager/admin roles (same as executive reports)
- export: `RequireComplianceReportExport` — audit package export roles

### Audit actions

- `maintainarr.reports.compliance.summary`
- `maintainarr.reports.compliance.template.detail`
- `maintainarr.reports.compliance.export`

## Frontend (maintainarr-frontend)

- `ComplianceReportsPanel` on Reports workspace (above executive/maintenance panels)
- Session gates: `canReadComplianceReports`, `canExportComplianceReports`
- API client: `getComplianceReportSummary`, `exportComplianceReportSummaryCsv`

## Tests

- `MaintainArrComplianceReportTests` — summary, template detail, CSV export, unauthorized
- `ComplianceReportsPanel.test.tsx` — panel render and permission gate

## Next slice

Per backlog: **Worker 206 imports** or **Worker 207 exports** — see `00_SLICE_STATE.md`.
