# Worker 79 — SupplyArr availability snapshots

## Slice name

M8 supply spine — vendor part link availability quantity/status history with effective dates, `/api/availability-snapshots`, supplyarr-frontend panel, tests.

## Products touched

- **SupplyArr API** (`apps/supplyarr-api`): `supplyarr_part_vendor_availability_snapshots`, `AvailabilitySnapshotService`, CRUD/list endpoints, audit events.
- **SupplyArr Frontend** (`apps/supplyarr-frontend`): `AvailabilitySnapshotsPanel`, API client methods, `HomePage` integration.

## Schema

Migration: `SupplyArrAvailabilitySnapshots`

Added SupplyArr table:

- `supplyarr_part_vendor_availability_snapshots` — tenant-scoped availability history per `supplyarr_part_vendor_links` row (`snapshotKey`, optional `quantityAvailable`, `availabilityStatus`, `effectiveFrom`/`effectiveTo`, `source`, `notes`)

Indexes: unique `(tenantId, snapshotKey)`, `(tenantId, partVendorLinkId, effectiveFrom)`, `(tenantId, partVendorLinkId, effectiveTo)`.

Notes:

- Creating a snapshot with a later `effectiveFrom` closes prior open rows (`effectiveTo` null) for the same vendor link.
- `availabilityStatus` values: `in_stock`, `limited`, `backorder`, `out_of_stock`, `discontinued`.
- `source` accepts `manual`, `quote`, `contract`, or `vendor_feed`.

## API + auth changes

### SupplyArr API endpoints

- `GET /api/availability-snapshots` — list with optional `partVendorLinkId`, `partId`, `vendorPartyId`, `asOf`
- `GET /api/availability-snapshots/{availabilitySnapshotId}` — detail
- `POST /api/availability-snapshots` — record new availability snapshot (closes prior open row on same link)

### Authorization

- read: `RequirePartsRead` (tenant member and supply roles)
- manage: `RequirePartsManage` (admin/manager)

## Frontend changes

- `AvailabilitySnapshotsPanel` — availability history list, current-only filter, record form when `canManageParts`
- API client: `getAvailabilitySnapshots`, `createAvailabilitySnapshot`
- `HomePage` — queries, mutations, panel after pricing/lead-time

## Tests

### Backend integration (`tests/STLCompliance.SupplyArr.Auth.Tests`)

- `Availability_snapshots_happy_path`
- `Availability_snapshot_create_denied_without_manage_role`

### Frontend unit

- `AvailabilitySnapshotsPanel.test.tsx` — renders current availability rows
- `client.test.ts` — availability list parsing

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

- Automated availability snapshot workers (M12)
- PO line availability check from current snapshot
- MaintainArr demand intake availability read-through (M10)
- Bulk import from vendor catalogs

## Next slice (Worker 80)

Recommended: **RoutArr bulk dispatch actions** or **SupplyArr reorder evaluation** per M8/M9 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
