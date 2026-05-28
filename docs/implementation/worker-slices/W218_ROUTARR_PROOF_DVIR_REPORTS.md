# Worker 218 — RoutArr M12 proof/DVIR reporting

## Slice name

M12 proof/DVIR reporting — tenant-scoped rollups on `routarr_trip_proof_records` and `routarr_trip_dvir_inspections` (W217), summary + trip/proof/DVIR detail + CSV export, Reports workspace panel, auth and audit aligned with W214 dispatch reports.

## Products touched

- **RoutArr API** (`apps/routarr-api`): `ProofDvirReportService`, `/api/reports/proof-dvir/*`.
- **RoutArr Frontend** (`apps/routarr-frontend`): `ProofDvirReportsPanel` on Reports workspace.
- **Tests**: `RoutArrProofDvirReportTests`, `ProofDvirReportsPanel.test.tsx`.

## Backend (RoutArr)

### APIs (JWT)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/reports/proof-dvir/summary?scope=daily\|weekly` | Proof/DVIR rollups for reporting window |
| GET | `/api/reports/proof-dvir/summary/export?scope=…` | CSV of all scoped proof + DVIR rows |
| GET | `/api/reports/proof-dvir/trips/{tripId}` | Trip detail with proof and DVIR lists |
| GET | `/api/reports/proof-dvir/proofs/{proofId}` | Proof record detail with trip context |
| GET | `/api/reports/proof-dvir/dvir/{dvirId}` | DVIR inspection detail with trip context |

### Data sources (owned tables only)

- `routarr_trip_proof_records` — scoped by `captured_at`
- `routarr_trip_dvir_inspections` — scoped by `submitted_at`
- `routarr` trips — trip metadata for summary/detail joins

No new migration.

### Authorization

- read: `RequireDispatchReportRead` → `RequireTripsAssign` (same as W214)
- export: `RequireDispatchReportExport` → `RequireTripsManage`

Drivers cannot read fleet-wide proof/DVIR reports.

### Audit actions

- `routarr.reports.proof_dvir.summary`
- `routarr.reports.proof_dvir.export`
- `routarr.reports.proof_dvir.trip.detail`
- `routarr.reports.proof_dvir.proof.detail`
- `routarr.reports.proof_dvir.dvir.detail`

### Entity export manifest

`GET /api/exports/manifest` `reportExports` includes proof/DVIR report CSV route (Worker 218).

## Frontend

- `ProofDvirReportsPanel` below route reports on `/reports`
- Metrics: proof count, DVIR count, trips with activity, fail/conditional DVIR
- Selectable trips, recent proof, recent DVIR with detail panes
- Export CSV when `canExportDispatchReports`

## Tests

| Suite | Coverage |
|-------|----------|
| `RoutArrProofDvirReportTests` | Summary rollups; trip/proof/DVIR detail; CSV export; driver denied read/export |
| `ProofDvirReportsPanel.test.tsx` | Panel render, export button, permission gate |

## Verification commands

```powershell
dotnet test "tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~RoutArrProofDvirReport"
cd apps/routarr-frontend
npm run test -- --run ProofDvirReportsPanel
```

## Relationship to W217

W217 owns capture workflow and persistence; W218 adds **read-only reporting** on the same tables without cross-product DB access.

## Next recommended backlog

Any product per suite priority — e.g. RoutArr audit package export, TrainArr M12 reporting, Compliance Core M12, SupplyArr integration depth.
