# Worker 62 — SupplyArr vendor/procurement foundations

## Slice name

M8 supply spine — external party registry (vendors, dealers, suppliers), NexArr handoff auth, supplyarr-frontend shell.

## Products touched

- **SupplyArr API** (`apps/supplyarr-api`): party registry domain tables, auth spine, CRUD endpoints, audit events.
- **SupplyArr Frontend** (`apps/supplyarr-frontend`): handoff launch, session storage, party registry workspace.
- **NexArr API** (`apps/nexarr-api`): SupplyArr launch profile updated to frontend port 5179.

## Schema

Migration: `SupplyArrVendorRegistryFoundations`

Added SupplyArr tables:

- `supplyarr_external_parties` — tenant-scoped vendor/dealer/supplier records (`partyKey`, `partyType`, `approvalStatus`, `status`)
- `supplyarr_party_contacts` — tenant-scoped contacts linked to external parties
- `supplyarr_audit_events` — write audit trail for registry mutations

Notes:

- Separate SupplyArr PostgreSQL database; no cross-database foreign keys.
- Party types: `vendor`, `dealer`, `supplier`.

## API + auth changes

### SupplyArr API endpoints

- `POST /api/auth/handoff/redeem` — NexArr handoff redeem (anonymous)
- `GET /api/session`, `GET /api/me` — session bootstrap and profile
- `GET/POST/PUT/PATCH /api/parties` — full party registry
- `GET/POST/PUT/PATCH /api/vendors` — vendor-scoped registry
- `GET/POST/PUT/PATCH /api/dealers` — dealer-scoped registry
- `GET/POST/PUT/PATCH /api/suppliers` — supplier-scoped registry
- `POST /api/{parties|vendors|dealers|suppliers}/{partyId}/contacts` — contact create

### Authorization

`SupplyArrAuthorizationService` enforces:

- product entitlement (`supplyarr`) required for all protected routes
- read: platform admin, tenant admin, supplyarr admin/manager/clerk, tenant member
- manage: platform admin, tenant admin, supplyarr admin/manager (`supplyarr.parties.manage` scope)

## Frontend changes

- New `apps/supplyarr-frontend` on port **5179** with Vite proxy to SupplyArr API (5106)
- `/launch?handoff=` redeem flow and `stl.supplyarr.session` storage
- Home workspace renders vendors, suppliers, and dealers from real APIs
- Vendor create form shown only when role can manage parties

## Tests

### Backend integration (`tests/STLCompliance.SupplyArr.Auth.Tests`)

- handoff redeem + `/api/me` happy path
- party/vendor registry CRUD happy path with contact + approval status
- vendor create denied for clerk role
- `/api/me` forbidden without supplyarr entitlement

### Frontend unit

- `src/api/client.test.ts` — vendor list success/forbidden parsing
- `src/components/PartyRegistryPanel.test.tsx` — registry list rendering

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

- Part catalog, inventory locations, purchase requests/orders (later M8 slices)
- MaintainArr parts demand intake (M10)
- Supplier compliance documents and onboarding workflows

## Next slice (Worker 63)

Recommended: **SupplyArr part catalog foundations** or **RoutArr trip/dispatch foundations** per M8/M9 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
