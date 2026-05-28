# Worker 126 — StaffArr audit package export + timeline (M12)

## Scope

Completes the W105/W106 backlog recommendation for tenant-admin audit packages:

- **Export** (from Worker 106, `2778bde`): `GET /api/audit-packages/export` ZIP/JSON with optional `from`/`to` filters; manifest; workforce proof sections; export audit logging.
- **Timeline** (this slice): `GET /api/audit-packages/timeline` — paginated tenant audit event browse for admins before export; StaffArr UI preview panel.

## API

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/audit-packages/manifest` | people-read roles |
| GET | `/api/audit-packages/timeline?from=&to=&page=&pageSize=` | people-read roles |
| GET | `/api/audit-packages/export` | `tenant_admin`, `staffarr_admin`, `hr_admin`, platform admin |

## Frontend

`AuditPackageExportPanel` — manifest sections, date filters, **audit timeline preview**, ZIP download, JSON preview with counts.

## Tests

- `StaffArrAuditPackageTests` — export + timeline pagination and date filters
- `AuditPackageExportPanel.test.tsx` — timeline render smoke

## Related

- Worker 106 (`2778bde`) — initial audit package export foundations
- Deferred: async `staffarr-worker` audit package generation job
