# Worker 200 — SupplyArr M8 supply readiness dashboard

**Product:** SupplyArr  
**Milestone:** M8  
**Backlog:** `[M8] supply readiness dashboard` (`02_PRODUCT_IMPLEMENTATION_BACKLOG.md`)

## Delivered

- **API:** `GET /api/supply-readiness/dashboard` — tenant-scoped aggregates for active parts, stock totals, below-reorder count, open backorders, open/issued PR/PO counts, open demand refs by source (MaintainArr/RoutArr/TrainArr/StaffArr), compliance attention, active vendor restrictions, active procurement exceptions, and capped attention items.
- **Auth:** `RequireSupplyReadinessRead` (admin, manager, clerk, buyer, tenant_member, platform admin).
- **Audit:** `supplyarr.supply_readiness.dashboard` on each dashboard read.
- **Frontend:** `/readiness` workspace route, `SupplyReadinessDashboardPanel`, nav item, `getSupplyReadinessDashboard` client, `canReadSupplyReadiness` session helper.
- **Tests:** `SupplyArrSupplyReadinessDashboardTests` (aggregation + forbidden role), `SupplyReadinessDashboardPanel.test.tsx`.

## Design notes

- Read-only aggregation over existing SupplyArr tables; no new migration.
- Attention list prioritizes stock below reorder, open procurement documents, backorders, compliance docs, restrictions, and exceptions (max 25 items).

## Verification

```bash
dotnet build apps/supplyarr-api/SupplyArr.Api/SupplyArr.Api.csproj
dotnet test tests/STLCompliance.SupplyArr.Auth.Tests --filter SupplyArrSupplyReadinessDashboardTests
cd apps/supplyarr-frontend && npm run build && npm test -- SupplyReadinessDashboardPanel
```

## Next slice

Worker **201** — SupplyArr M8 warranty claims (complete). See `00_SLICE_STATE.md` for Worker 202 suite-wide next slice.
