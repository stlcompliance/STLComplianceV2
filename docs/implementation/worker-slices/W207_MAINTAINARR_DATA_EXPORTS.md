# MaintainArr M12 data exports

## Slice name

M12 data/report export surfaces — bulk entity CSV downloads (assets, work orders, inspection runs), export manifest coordinating report CSV endpoints (W203–205) and existing audit package export (W132), auth, audit, Reports workspace UI, integration + frontend tests

## Products touched

- **MaintainArr API** (`apps/maintainarr-api`): `EntityBulkExportService`, `/api/exports/*`
- **MaintainArr Frontend** (`apps/maintainarr-frontend`): `DataExportsPanel` on Reports workspace
- **Integration tests** (`tests/STLCompliance.MaintainArr.Auth.Tests`): `MaintainArrEntityBulkExportTests`

## Schema

No new tables — direct CSV materialization from existing MaintainArr-owned tables.

## API + auth changes

### MaintainArr user APIs (JWT + MaintainArr entitlement)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/exports/manifest` | `RequireEntityExport` |
| GET | `/api/exports/assets` | CSV asset registry (`lifecycleStatus` filter) |
| GET | `/api/exports/work-orders` | CSV work orders (`status`, `assetId` filters) |
| GET | `/api/exports/inspection-runs` | CSV inspection runs (`status`, `assetId` filters) |

`RequireEntityExport` aliases `RequireAuditPackageExport` (tenant admin, maintainarr admin, manager, platform admin).

### Related export surfaces (not reimplemented in W207)

| Surface | Route | Worker |
|---------|-------|--------|
| Maintenance report CSV | `GET /api/reports/maintenance/summary/export` | 203 |
| Executive report CSV | `GET /api/reports/executive/summary/export` | 204 |
| Compliance report CSV | `GET /api/reports/compliance/summary/export` | 205 |
| Audit package ZIP/JSON | `GET /api/audit-packages/export` | 132 |
| Async audit jobs | `POST /api/audit-packages/jobs` | 132 |

Manifest `reportExports` and `auditPackageFormats` document these for UI coordination.

### Audit events

- `maintainarr.exports.assets` — `reasonCode` = row count
- `maintainarr.exports.work_orders`
- `maintainarr.exports.inspection_runs`

## CSV headers

**Assets** (import-compatible keys plus ids):  
`assetClassKey,assetTypeKey,assetTag,name,description,siteRef,lifecycleStatus,assetId,createdAt,updatedAt`

**Work orders:**  
`workOrderNumber,assetTag,title,description,priority,status,source,assignedTechnicianPersonId,createdAt,updatedAt,startedAt,completedAt,cancelledAt,workOrderId,assetId`

**Inspection runs:**  
`assetTag,templateKey,templateVersion,status,result,startedAt,completedAt,inspectionRunId,assetId`

## Frontend changes

- **DataExportsPanel** — manifest-driven entity CSV download buttons on Reports workspace
- Report CSV exports remain on Maintenance / Executive / Compliance panels above
- Full audit package export remains on Settings (`AuditPackageExportPanel`)

## Worker / events

None (sync direct download; async audit package jobs unchanged).

## Tests

### Backend integration (`MaintainArrEntityBulkExportTests`)

- `Entity_export_manifest_lists_entities_and_reports`
- `Entity_export_assets_csv_includes_seeded_asset`
- `Entity_export_work_orders_csv_includes_seeded_work_order`
- `Entity_export_inspection_runs_csv_returns_header_when_empty`
- `Entity_export_denies_unauthenticated`

### Frontend (`DataExportsPanel.test.tsx`)

- Read-only notice for non-exporters
- Entity export controls for exporters

## Next slice

After MaintainArr M12 imports/exports cluster (Workers 206–207), recommended backlog per suite scan:

- **NexArr M12** — platform lifecycle workers (service-token cleanup, entitlement reconciliation, tenant lifecycle)
- **RoutArr M9/M12** — dispatch and reporting depth (large unstarted product area)
- **TrainArr M12** — remaining notification/history workers
- **Cross-product M10** — integration slices per product matrices
