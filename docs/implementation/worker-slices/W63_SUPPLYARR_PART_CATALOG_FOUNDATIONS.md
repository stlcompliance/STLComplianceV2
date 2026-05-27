# Worker 63 — SupplyArr part catalog foundations

## Slice name

M8 supply spine — part catalogs, part SKUs, manufacturer aliases, vendor links, CRUD APIs, supplyarr-frontend workspace, tests.

## Products touched

- **SupplyArr API** (`apps/supplyarr-api`): part catalog domain tables, CRUD endpoints, vendor linkage to external parties, audit events.
- **SupplyArr Frontend** (`apps/supplyarr-frontend`): part catalog panel on home workspace with catalog/part create and vendor link flows.

## Schema

Migration: `SupplyArrPartCatalogFoundations`

Added SupplyArr tables:

- `supplyarr_part_catalogs` — tenant-scoped catalog groupings (`catalogKey`, `name`, `description`, `status`)
- `supplyarr_parts` — tenant-scoped part SKUs (`partKey`, optional `partCatalogId`, `categoryKey`, `unitOfMeasure`, manufacturer fields, `status`)
- `supplyarr_part_manufacturer_aliases` — alternate manufacturer identifiers per part (`aliasKey`, `manufacturerName`, `manufacturerPartNumber`)
- `supplyarr_part_vendor_links` — cross-reference parts to vendor/supplier external parties (`vendorPartNumber`, `isPreferred`)

Notes:

- Vendor links reference `supplyarr_external_parties` (vendor or supplier party types only).
- Separate SupplyArr PostgreSQL database; no cross-database foreign keys.

## API + auth changes

### SupplyArr API endpoints

- `GET/POST/PUT/PATCH /api/catalogs` — part catalog list, create, update, status
- `GET/POST/PUT/PATCH /api/parts` — part SKU list (optional `catalogId` filter), create, update, status
- `POST /api/parts/{partId}/manufacturer-aliases` — add manufacturer alias
- `POST /api/parts/{partId}/vendor-links` — link part to vendor/supplier party

### Authorization

`SupplyArrAuthorizationService` enforces:

- product entitlement (`supplyarr`) required for all protected routes
- read: platform admin, tenant admin, supplyarr admin/manager/clerk, tenant member (`RequirePartsRead`)
- manage: platform admin, tenant admin, supplyarr admin/manager (`supplyarr.parts.manage` via `RequirePartsManage`)

## Frontend changes

- `PartCatalogPanel` on home workspace — lists catalogs and parts from real APIs
- Manage forms for catalog create, part SKU create, and vendor link (when role can manage parts)
- API client helpers: `getPartCatalogs`, `getParts`, `createPartCatalog`, `createPart`, `createPartVendorLink`

## Tests

### Backend integration (`tests/STLCompliance.SupplyArr.Auth.Tests`)

- part catalog + part CRUD with vendor link happy path
- part create denied for clerk role
- (existing) handoff, party registry, entitlement tests unchanged

### Frontend unit

- `src/api/client.test.ts` — parts list success parsing
- `src/components/PartCatalogPanel.test.tsx` — catalog/part list rendering with vendor link display

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

- Inventory locations, stock/reservation, reorder evaluation (later M8 slices)
- Purchase requests/orders, receiving, pricing/lead-time snapshots
- MaintainArr parts demand intake (M10)
- Manufacturer alias UI (API only in this slice)

## Next slice (Worker 64)

Recommended: **SupplyArr inventory location foundations** or **RoutArr trip/dispatch foundations** per M8/M9 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
