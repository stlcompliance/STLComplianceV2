# Worker 60 — MaintainArr asset readiness endpoint

## Slice name

M7 maintenance spine — real-time asset readiness from open critical/high defects, active work orders, PM due/overdue, and latest failed inspections; JWT read API with single-asset and fleet list; maintainarr-frontend readiness column on asset registry; integration and frontend tests.

## Products touched

- **MaintainArr API** (`apps/maintainarr-api`): `AssetReadinessService`, `AssetReadinessRules`, `GET /api/asset-readiness`, JWT asset-read auth.
- **maintainarr-frontend**: readiness badge/column on `AssetRegistryPanel`, fleet readiness query on home workspace.

## Schema

No new tables. Readiness is computed at request time from:

- `maintainarr_defects`
- `maintainarr_work_orders`
- `maintainarr_pm_schedules`
- `maintainarr_inspection_runs` (+ template name join)

## API + auth changes

### MaintainArr user API (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/asset-readiness?assetId={guid}` | Asset readiness read (asset read / MaintainArr entitlement) |
| GET | `/api/asset-readiness` | Fleet readiness summaries for all tenant assets |

`MaintainArrAuthorizationService.RequireAssetReadinessRead` reuses asset read rules.

### Readiness rules

- **not_ready** when any blocker exists; otherwise **ready**
- Blockers (priority order):
  - Open **critical** or **high** defects (`open`, `acknowledged`, `in_repair`)
  - Latest **completed** inspection with result `failed`
  - Active PM schedule (`status=active`) with `due_status` `due` or `overdue`
  - Active work orders (`open`, `in_progress`)

Single-asset response: `AssetReadinessResponse` (`readinessStatus`, `readinessBasis`, `blockers[]`, `signals` counts).

Fleet response: `AssetReadinessSummaryResponse[]` (`readinessStatus`, `blockerCount`, `primaryBlockerMessage`).

## Frontend changes

- `AssetRegistryPanel` — readiness badge per asset with blocker count and primary blocker tooltip/message
- Home workspace loads fleet readiness via `getAssetReadinessFleet`
- API client: `getAssetReadiness`, `getAssetReadinessFleet`

## Tests

### Backend unit (`AssetReadinessRulesTests`)

- Open defect status, blocking severities, active work order status, blocking PM due status
- Ready / not_ready status resolution

### Backend integration (`MaintainArrAssetReadinessTests`)

- `Asset_readiness_ready_when_no_blockers`
- `Asset_readiness_not_ready_for_open_critical_defect`
- `Asset_readiness_not_ready_for_active_work_order_and_pm_overdue`
- `Asset_readiness_fleet_list_returns_all_assets_with_status`
- `Asset_readiness_missing_asset_returns_not_found`
- `Asset_readiness_requires_maintainarr_entitlement`

### Frontend unit

- `AssetRegistryPanel.test.tsx` — readiness badge and blocker message
- `client.test.ts` — single asset and fleet API paths

## Verification commands

```powershell
dotnet build "apps/maintainarr-api/MaintainArr.Api/MaintainArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~AssetReadiness"
cd apps/maintainarr-frontend
npm run test
npm run build
```

## Remaining gaps

- Asset readiness gate API for RoutArr/dispatch (M10)
- `maintainarr.asset.readinessChanged` event publication
- Materialized readiness snapshots for large fleets
- Manual readiness override (if product requires parity with StaffArr)

## Next recommended slice

**MaintainArr labor/evidence capture** or **SupplyArr parts demand from work orders** per M7 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
