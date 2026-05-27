# Worker 81 — SupplyArr reorder evaluation

## Slice name

M8 inventory/procurement spine — part reorder points on `supplyarr_parts`, stock vs reorder point evaluation, suggested PR lines, `/api/reorder-evaluation` user API, `shared-worker` optional draft PR job, supplyarr-frontend panel, tests.

## Products touched

- **SupplyArr API** (`apps/supplyarr-api`): `ReorderPoint` / `ReorderQuantity` on parts, `ReorderEvaluationService` + rules, user + internal endpoints, audit events.
- **shared-worker** (`workers/shared-worker`): `SupplyArrReorderEvaluationJob`, HTTP client to SupplyArr internal API.
- **SupplyArr Frontend** (`apps/supplyarr-frontend`): `ReorderEvaluationPanel`, API client methods, `HomePage` integration.
- **Tests**: `STLCompliance.SupplyArr.Auth.Tests`, `STLCompliance.Shared.Worker.Tests` (`ReorderEvaluationRulesTests`), `SupplyArrReorderEvaluationWorkerTests`.

## Schema

Migration: `SupplyArrReorderEvaluation`

Added columns on `supplyarr_parts`:

- `reorder_point` — nullable decimal; when set, part participates in reorder evaluation
- `reorder_quantity` — optional fixed order quantity when below reorder point

Evaluation aggregates `quantity_on_hand - quantity_reserved` across all bins per part (tenant-scoped).

## API + auth changes

### SupplyArr user API (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/reorder-evaluation` | Inventory read — returns suggestions for parts at/below reorder point |
| GET | `/api/reorder-evaluation/parts/{partId}/policy` | Inventory read |
| PUT | `/api/reorder-evaluation/parts/{partId}/policy` | Inventory manage (`supplyarr.inventory.manage`) |
| POST | `/api/reorder-evaluation/create-purchase-request` | PR create — draft PR from selected suggestion part ids |

`POST create-purchase-request` skips parts with open draft/submitted purchase request lines. Suggested quantity uses `reorder_quantity` when set, otherwise deficit to reorder point (minimum 1).

### SupplyArr internal (service token)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/internal/reorder/pending` | NexArr service token: source `shared-worker`, target `supplyarr`, scope `supplyarr.reorder.evaluate` |
| POST | `/api/internal/reorder/process-evaluation` | Same |

`process-evaluation` body: optional `tenantId`, `batchSize` (1–500), `createDraftPurchaseRequests` (groups actionable suggestions by preferred vendor into draft PRs).

## shared-worker configuration

`SupplyArrReorderEvaluation` section:

| Key | Default | Purpose |
|-----|---------|---------|
| `Enabled` | `true` | Toggle job |
| `SupplyArrBaseUrl` | `http://localhost:5106` | SupplyArr API base |
| `ServiceToken` | `""` | Bearer for internal reorder API |
| `ScanIntervalMinutes` | `60` | Periodic evaluation interval |
| `BatchSize` | `100` | Max parts evaluated per run |
| `CreateDraftPurchaseRequests` | `true` | Auto-create draft PRs when suggestions exist |
| `TenantId` | `null` | Optional tenant filter |

## Frontend changes

- `ReorderEvaluationPanel` — policy editor, suggestion table, draft PR creation from selected lines
- API client: `getReorderEvaluation`, `upsertPartReorderPolicy`, `createPurchaseRequestFromReorder`
- `PartResponse` includes `reorderPoint` / `reorderQuantity`

## Tests

### Backend integration (`tests/STLCompliance.SupplyArr.Auth.Tests`)

- `Reorder_evaluation_suggests_low_stock_and_creates_draft_purchase_request`
- `Reorder_policy_upsert_denied_for_clerk_role`
- `SupplyArrReorderEvaluationWorkerTests` — service token, pending list, worker draft PR creation

### Unit (`ReorderEvaluationRulesTests`)

- `NeedsReorder` boundary at reorder point
- `ResolveSuggestedQuantity` with/without reorder quantity
- Open PR status detection

### Frontend unit

- `ReorderEvaluationPanel.test.tsx` — suggestion rendering
- `client.test.ts` — reorder evaluation list parsing

## Verification commands

```powershell
dotnet build "apps/supplyarr-api/SupplyArr.Api/SupplyArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~Reorder"
dotnet test "tests/STLCompliance.Shared.Worker.Tests/STLCompliance.Shared.Worker.Tests.csproj" -c Release --filter "FullyQualifiedName~Reorder"
cd apps/supplyarr-frontend
npm install
npm run test
npm run build
```

## Remaining gaps

- Per-location reorder points (tenant-wide aggregate only)
- Reservation-aware allocation rules beyond simple on-hand minus reserved
- Auto-submit PRs or PO generation from evaluation (manual/worker draft only)
- MaintainArr demand intake → reorder policy automation (M10)
- Dedicated `supplyarr-worker` process (cross-product job lives in `shared-worker`)

## Next slice (Worker 82)

Recommended: **RoutArr dispatch closeout** or **SupplyArr demand intake** per M8/M9/M10 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
