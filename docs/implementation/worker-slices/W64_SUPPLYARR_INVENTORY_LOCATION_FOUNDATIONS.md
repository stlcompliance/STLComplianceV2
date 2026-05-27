# Worker 64 — SupplyArr inventory location foundations

## Slice name

M8 supply spine — inventory warehouses/sites, bins, part stock levels per bin, `/api/inventory` CRUD, supplyarr-frontend workspace, tests.

## Products touched

- **SupplyArr API** (`apps/supplyarr-api`): inventory location domain tables, location/bin/stock endpoints, audit events.
- **SupplyArr Frontend** (`apps/supplyarr-frontend`): inventory panel on home workspace with location/bin create and stock upsert flows.

## Schema

Migration: `SupplyArrInventoryLocationFoundations`

Added SupplyArr tables:

- `supplyarr_inventory_locations` — tenant-scoped warehouses/sites (`locationKey`, `name`, `locationType`, `addressLine`, `status`)
- `supplyarr_inventory_bins` — bins within a location (`binKey`, `name`, `status`)
- `supplyarr_part_stock_levels` — quantity on hand (and reserved placeholder) per part per bin

Notes:

- Stock levels reference `supplyarr_parts` and `supplyarr_inventory_bins`.
- `QuantityReserved` is stored for future reservation slices; this slice only sets `QuantityOnHand`.
- Separate SupplyArr PostgreSQL database; no cross-database foreign keys.

## API + auth changes

### SupplyArr API endpoints

- `GET/POST/PUT/PATCH /api/inventory/locations` — warehouse/site list, create, update, status
- `GET/POST /api/inventory/locations/{locationId}/bins` — bin list and create
- `PUT/PATCH /api/inventory/bins/{binId}` — bin update and status
- `GET/POST /api/inventory/stock` — stock level list (optional `locationId`, `binId`, `partId` filters) and upsert on-hand quantity

### Authorization

`SupplyArrAuthorizationService` enforces:

- product entitlement (`supplyarr`) required for all protected routes
- read: platform admin, tenant admin, supplyarr admin/manager/clerk, tenant member (`RequireInventoryRead`)
- manage: platform admin, tenant admin, supplyarr admin/manager (`supplyarr.inventory.manage` via `RequireInventoryManage`)

## Frontend changes

- `InventoryPanel` on home workspace — lists locations, bins at selected location, and stock levels
- Manage forms for location create, bin create, and stock upsert (when role can manage inventory)
- API client helpers: `getInventoryLocations`, `getInventoryBins`, `getPartStockLevels`, `createInventoryLocation`, `createInventoryBin`, `upsertPartStockLevel`

## Tests

### Backend integration (`tests/STLCompliance.SupplyArr.Auth.Tests`)

- inventory location + bin + stock upsert happy path
- location create denied for clerk role
- (existing) handoff, party registry, part catalog, entitlement tests unchanged

### Frontend unit

- `src/api/client.test.ts` — inventory locations list success parsing
- `src/components/InventoryPanel.test.tsx` — location/bin/stock list rendering

## Verification commands

```powershell
dotnet build "apps/supplyarr-api/SupplyArr.Api/SupplyArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj" -c Release
cd apps/supplyarr-frontend
npm install
npm run test
npm run build
```

## Remaining gaps

- Stock reservations, transfers, cycle counts, reorder evaluation (later M8 slices)
- Purchase requests/orders, receiving, pricing/lead-time snapshots
- MaintainArr parts demand intake (M10)
- Multi-location bin picker for stock upsert (all bins across tenant)

## Next slice (Worker 65)

Recommended: **SupplyArr purchase request foundations** or **RoutArr trip/dispatch foundations** per M8/M9 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
