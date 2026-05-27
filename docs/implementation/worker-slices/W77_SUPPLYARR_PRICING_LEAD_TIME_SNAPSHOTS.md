# Worker 77 — SupplyArr pricing/lead-time snapshots

## Slice name

M8 supply spine — vendor part link pricing and lead-time history with effective dates, `/api/pricing-snapshots` and `/api/lead-time-snapshots`, supplyarr-frontend panel, tests.

## Products touched

- **SupplyArr API** (`apps/supplyarr-api`): `supplyarr_part_vendor_pricing_snapshots`, `supplyarr_part_vendor_lead_time_snapshots`, `PricingSnapshotService`, `LeadTimeSnapshotService`, CRUD/list endpoints, audit events.
- **SupplyArr Frontend** (`apps/supplyarr-frontend`): `PricingLeadTimePanel`, API client methods, `HomePage` integration.

## Schema

Migration: `SupplyArrPricingLeadTimeSnapshots`

Added SupplyArr tables:

- `supplyarr_part_vendor_pricing_snapshots` — tenant-scoped unit price history per `supplyarr_part_vendor_links` row (`snapshotKey`, `unitPrice`, `currencyCode`, optional `minimumOrderQuantity`, `effectiveFrom`/`effectiveTo`, `source`, `notes`)
- `supplyarr_part_vendor_lead_time_snapshots` — tenant-scoped lead-time history per vendor link (`snapshotKey`, `leadTimeDays`, `effectiveFrom`/`effectiveTo`, `source`, `notes`)

Indexes: unique `(tenantId, snapshotKey)` per table, `(tenantId, partVendorLinkId, effectiveFrom)`, `(tenantId, partVendorLinkId, effectiveTo)`.

Notes:

- Creating a snapshot with a later `effectiveFrom` closes prior open rows (`effectiveTo` null) for the same vendor link.
- Snapshot keys are unique per tenant within each table (pricing vs lead-time keys are separate namespaces).

## API + auth changes

### SupplyArr API endpoints

- `GET /api/pricing-snapshots` — list with optional `partVendorLinkId`, `partId`, `vendorPartyId`, `asOf`
- `GET /api/pricing-snapshots/{pricingSnapshotId}` — detail
- `POST /api/pricing-snapshots` — record new pricing snapshot (closes prior open row on same link)
- `GET /api/lead-time-snapshots` — list with same filters
- `GET /api/lead-time-snapshots/{leadTimeSnapshotId}` — detail
- `POST /api/lead-time-snapshots` — record new lead-time snapshot

### Authorization

- read: `RequirePartsRead` (tenant member and supply roles)
- manage: `RequirePartsManage` (admin/manager)

## Frontend changes

- `PricingLeadTimePanel` — pricing and lead-time history lists, current-only filter, record forms when `canManageParts`
- API client: `getPricingSnapshots`, `createPricingSnapshot`, `getLeadTimeSnapshots`, `createLeadTimeSnapshot`
- `HomePage` — queries, mutations, panel after returns

## Tests

### Backend integration (`tests/STLCompliance.SupplyArr.Auth.Tests`)

- `Pricing_and_lead_time_snapshots_happy_path`
- `Pricing_snapshot_create_denied_without_manage_role`

### Frontend unit

- `PricingLeadTimePanel.test.tsx` — renders current pricing and lead-time rows
- `client.test.ts` — pricing/lead-time list parsing

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

- Automated price/lead-time snapshot workers (M12)
- Availability snapshots
- PO line unit price from current pricing snapshot
- MaintainArr demand intake cost/lead-time read-through (M10)
- Bulk import from vendor catalogs

## Next slice (Worker 78)

Recommended: **RoutArr drag-and-drop assignment** or **SupplyArr availability snapshots** per M8/M9 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
