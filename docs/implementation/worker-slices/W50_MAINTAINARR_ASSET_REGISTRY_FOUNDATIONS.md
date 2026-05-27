# Worker 50 — MaintainArr asset registry foundations

## Slice name

M7 maintenance spine — asset classes, asset types, asset registry CRUD, NexArr handoff auth, maintainarr-frontend shell.

## Products touched

- **MaintainArr API** (`apps/maintainarr-api`): asset registry domain tables, auth spine, CRUD endpoints, audit events.
- **MaintainArr Frontend** (`apps/maintainarr-frontend`): handoff launch, session storage, asset registry workspace.
- **NexArr API** (`apps/nexarr-api`): MaintainArr launch profile updated to frontend port 5178.

## Schema

Migration: `MaintainArrAssetRegistryFoundations`

Added MaintainArr tables:

- `maintainarr_asset_classes` — tenant-scoped classification catalog (`classKey`, `name`, `status`)
- `maintainarr_asset_types` — tenant-scoped type catalog linked to class (`typeKey`, `assetClassId`)
- `maintainarr_assets` — tenant-scoped asset instances (`assetTag`, `name`, `lifecycleStatus`, optional `siteRef`)
- `maintainarr_audit_events` — write audit trail for registry mutations

Notes:

- Separate MaintainArr PostgreSQL database; no cross-database foreign keys.
- Human/technician references deferred to later slices (`maintainarr_staff_person_refs`).

## API + auth changes

### MaintainArr API endpoints

- `POST /api/auth/handoff/redeem` — NexArr handoff redeem (anonymous)
- `GET /api/session`, `GET /api/me` — session bootstrap and profile
- `GET/POST/PUT/PATCH /api/asset-classes` — class catalog CRUD + status
- `GET/POST/PUT/PATCH /api/asset-types` — type catalog CRUD + status
- `GET/POST/PUT/PATCH /api/assets` — asset registry CRUD + lifecycle status

### Authorization

`MaintainArrAuthorizationService` enforces:

- product entitlement (`maintainarr`) required for all protected routes
- read: platform admin, tenant admin, maintainarr admin/manager/technician, tenant member
- manage: platform admin, tenant admin, maintainarr admin/manager (`maintainarr.assets.create` scope)

## Frontend changes

- New `apps/maintainarr-frontend` on port **5178** with Vite proxy to MaintainArr API (5104)
- `/launch?handoff=` redeem flow and `stl.maintainarr.session` storage
- Home workspace renders asset classes, types, and assets from real APIs
- Create forms shown only when role can manage assets

## Tests

### Backend integration (`tests/STLCompliance.MaintainArr.Auth.Tests`)

- handoff redeem + `/api/me` happy path
- asset class/type/asset CRUD happy path
- asset create denied for technician role
- `/api/me` forbidden without maintainarr entitlement

### Frontend unit

- `src/api/client.test.ts` — asset list success/forbidden parsing
- `src/components/AssetRegistryPanel.test.tsx` — registry list rendering

## Verification commands

```powershell
dotnet build "apps/maintainarr-api/MaintainArr.Api/MaintainArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj" -c Release
cd apps/maintainarr-frontend
npm install
npm run test
npm run build
```

## Remaining gaps

- Meter tracking, inspections, defects, work orders, PM schedules (later M7 slices)
- MaintainArr scheduled worker (PM due scan / inspection due) — next recommended slice
- Technician person refs and asset assignment linkage
