# Worker 184 — SupplyArr compliance reports

## Slice name

M12 reporting — supplier compliance document and expiration rollups.

## Products touched

- **SupplyArr API** (`apps/supplyarr-api`): `supplyarr_party_compliance_documents` table, `ComplianceReportService`, `/api/reports/compliance/*`, audit events.
- **SupplyArr Frontend** (`apps/supplyarr-frontend`): `ComplianceReportsPanel` on Reports workspace.
- **Tests** (`tests/STLCompliance.SupplyArr.Auth.Tests`): `SupplyArrComplianceReportTests`.

## Schema

Migration: `SupplyArrPartyComplianceDocuments`

- `supplyarr_party_compliance_documents` — tenant-scoped supplier compliance metadata linked to `supplyarr_external_parties` (`documentKey`, `documentTypeKey`, `version`, `reviewStatus`, `expiresAt`, file metadata).

Notes:

- Reports slice adds persistence required for honest compliance rollups (M8 supplier compliance documents backlog item remains for upload/review workflows).
- No cross-product database coupling.

## API + auth changes

### Endpoints

- `GET /api/reports/compliance/summary` — party/document rollups; filters: `attentionOnly`, `partyType`, `externalPartyId`, `reviewStatus`
- `GET /api/reports/compliance/summary/export` — CSV export (same filters)
- `GET /api/reports/compliance/parties/{externalPartyId}` — party compliance detail

### Authorization

- Read: `RequireComplianceReportRead` → party read roles (same as vendor reports)
- Export: `RequireComplianceReportExport` → party read roles

### Audit

- `supplyarr.reports.compliance.summary`
- `supplyarr.reports.compliance.export`
- `supplyarr.reports.compliance.party_detail`

## Frontend changes

- `ComplianceReportsPanel` on Reports workspace (`reports` route)
- Permission gates: `canReadComplianceReports`, `canExportComplianceReports` in `sessionStorage.ts`
- API client: summary, party detail, CSV export

## Tests

### Backend integration

- summary rollups (expired + pending counts)
- attention-only filter
- party detail
- CSV export
- unauthorized without JWT

### Frontend unit

- `ComplianceReportsPanel.test.tsx` — renders summary; hidden without read permission

## Verification commands

```powershell
dotnet build "apps/supplyarr-api/SupplyArr.Api/SupplyArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~SupplyArrComplianceReportTests"
cd apps/supplyarr-frontend
npm run test
npm run build
```

## Remaining gaps

- M8 supplier compliance document upload, versioning, and review workflow APIs/UI
- Forgiving search and audit history (M12 backlog)

## Next slice (Worker 185)

Recommended: **SupplyArr forgiving search** or **audit history** per `02_PRODUCT_IMPLEMENTATION_BACKLOG.md` and `00_SLICE_STATE.md`.
