# Worker 45 — Compliance Core operator dashboards

## Slice name

M5/M12 Compliance Core operator dashboards — aggregated tenant metrics from real DB queries, JWT read auth for compliance roles, compliancecore-frontend landing tab with summary cards

## Products touched

- **Compliance Core API** (`apps/compliancecore-api`): `GET /api/dashboards/operator`, `OperatorDashboardService`, contracts, authorization
- **Compliance Core Frontend** (`apps/compliancecore-frontend`): `OperatorDashboardPanel`, default Dashboard tab on home
- **Tests**: `ComplianceCoreOperatorDashboardTests`, `OperatorDashboardPanel.test.tsx`

## API + auth

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/dashboards/operator` | JWT; Compliance Core entitlement; roles: `tenant_admin`, `compliance_admin`, `compliance_reviewer`, `tenant_member` (same as findings read) |

### Response aggregates (tenant-scoped, live queries)

- **Findings**: open / block-severity open / warn-severity open / acknowledged / resolved / total
- **Rule packs**: counts by status (`draft`, `review`, `published`, `archived`)
- **Evaluations**: total, last 24h, pass, fail; up to 8 recent runs with pack label/key
- **Workflow gates**: definition count; check result totals; block/warn/allow outcome counts (block = gate check failures)
- **Audit events**: total, last 24h, success vs non-success
- **generatedAt**: UTC timestamp when the snapshot was built

Reads are audited as `operator_dashboard.read`.

## Frontend

- Home tab **Dashboard** (default landing) renders `OperatorDashboardPanel` with TanStack Query calling `getOperatorDashboard`.
- Summary cards only; no static placeholder counts.

## Tests

### Integration (`ComplianceCoreOperatorDashboardTests`)

- Empty tenant returns zero counts
- After evaluate + gate check, counts reflect real findings/evaluations/gate blocks
- Requires authentication and `compliancecore` entitlement
- `tenant_member` can read

### Frontend (`OperatorDashboardPanel.test.tsx`)

- Renders API-driven counts and recent evaluation row

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~OperatorDashboard"
cd apps/compliancecore-frontend
npm test -- --run OperatorDashboardPanel
npm run build
```

## Remaining gaps

- No time-range filters on dashboard (fixed 24h windows for evaluations/gates/audit only)
- No drill-down navigation from cards to detail tabs
- Charts/graphs deferred; counts-only UI is intentional for this slice

## Next recommended slice

**StaffArr certification expiration worker** (M12) — scheduled scan for person certifications past expiry, mirror TrainArr W44 pattern with StaffArr internal API and `staffarr-worker` or `shared-worker`.
