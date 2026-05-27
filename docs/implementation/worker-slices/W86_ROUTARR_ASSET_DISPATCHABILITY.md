# Worker 86 — RoutArr asset dispatchability checks

## Slice name

M10 asset dispatchability — check vehicle reference or asset tag against MaintainArr asset readiness before trip vehicle assign; `/api/asset-dispatchability` API, MaintainArr integration endpoint, assign/preview/bulk gates, routarr-frontend assignment warnings, cross-product tests.

## Products touched

- **MaintainArr API** (`apps/maintainarr-api`): `AssetReadinessService.GetByDispatchRefAsync`, `GET /api/integrations/routarr-asset-readiness` (service token scope `maintainarr.asset_readiness.dispatch_gate`).
- **RoutArr API** (`apps/routarr-api`): `AssetDispatchabilityService`, `AssetDispatchabilityRules`, MaintainArr HTTP client, `POST /api/asset-dispatchability/check`, enhanced assignment preview + assign-vehicle with dispatchability gates, audit events.
- **RoutArr Frontend** (`apps/routarr-frontend`): assignment panel dispatchability warnings and override flags on drag-and-drop vehicle assign.
- **Tests**: `AssetDispatchabilityRulesTests`, `RoutArrAssetDispatchabilityTests`, updated `DispatchAssignmentPanel.test.tsx`.

## Schema

No new migration — dispatchability is computed at request time via MaintainArr asset readiness.

## API + auth changes

### RoutArr user API (JWT + RoutArr entitlement)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/asset-dispatchability/check` | `RequireTripsAssign` (`routarr.dispatch.assign`) |

Request: `vehicleRefKey` and/or `assetTag` (at least one required).

Response: `outcome` (`allow` \| `warn` \| `block`), `reasonCode`, `message`, `isBlocking`, optional `maintainArr` summary.

Assignment integration:

- `POST /api/dispatch/assignments/preview` — vehicle previews include `assetDispatchability` summary; blocking dispatchability sets `hasBlockingConflicts`.
- `PATCH /api/trips/{tripId}/assign-vehicle` — blocks on dispatchability `block` unless `ignoreDispatchabilityBlocks: true` (409).
- `POST /api/dispatch/bulk/preview|apply` — bulk vehicle items include dispatchability; apply supports `ignoreDispatchabilityBlocks`.

### MaintainArr integration API (NexArr service token → MaintainArr)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/integrations/routarr-asset-readiness?tenantId=&vehicleRefKey=&assetTag=` | source `routarr`, target `maintainarr`, scope `maintainarr.asset_readiness.dispatch_gate` |

Resolves asset by explicit `assetTag`, or `vehicleRefKey` as asset id (GUID) or asset tag. Returns `AssetReadinessResponse`.

### Configuration

`apps/routarr-api/RoutArr.Api/appsettings.json`:

```json
"AssetDispatchability": {
  "CheckMaintainArrReadiness": true
},
"MaintainArr": {
  "BaseUrl": "http://localhost:5104",
  "ServiceToken": ""
}
```

### Dispatchability merge rules

- MaintainArr `not_ready` → `block`
- Asset not found in MaintainArr (integration 404) → `warn` (`maintainarr_asset_not_found`)
- Integration unconfigured → `warn` (`dispatchability_check_unavailable`)
- Ready asset → `allow`

Audit: `asset_dispatchability.check`.

## Frontend changes

- `DispatchAssignmentPanel` — shows dispatchability warnings in conflict messaging; confirms on warn; passes `ignoreDispatchabilityBlocks` on override for vehicle assign.
- API client: `checkAssetDispatchability`, extended preview/assign types.

## Tests

### Backend unit (`AssetDispatchabilityRulesTests`)

- MaintainArr not_ready merges to block
- Asset not found merges to warn
- Ready asset merges to allow
- ApplyDispatchability marks preview blocked

### Cross-product (`RoutArrAssetDispatchabilityTests`)

- `Asset_dispatchability_check_reports_maintainarr_not_ready`
- `Assign_vehicle_blocked_when_maintainarr_not_ready_and_override_succeeds`

### Frontend unit

- `DispatchAssignmentPanel.test.tsx` — updated preview shape with `assetDispatchability`

## Verification commands

```powershell
dotnet build "apps/routarr-api/RoutArr.Api/RoutArr.Api.csproj" -c Release
dotnet build "apps/maintainarr-api/MaintainArr.Api/MaintainArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~Dispatchability"
cd apps/routarr-frontend
npm install
npm run test
npm run build
```

## Remaining gaps

- Compliance Core dispatch workflow gates
- Materialized asset dispatchability cache / worker
- Bulk dispatch UI surfacing `ignoreDispatchabilityBlocks` explicitly

## Next slice (Worker 87)

Recommended: **Compliance Core dispatch workflow gates** per M10 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
