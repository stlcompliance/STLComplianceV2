# MaintainArr bulk asset import

## Slice name

M12 bulk asset import — validate/commit batch import with persisted import batches, CSV or JSON upload, audit logging, Settings workspace import panel, integration + frontend tests

## Products touched

- **MaintainArr API** (`apps/maintainarr-api`): `AssetBulkImportService`, `AssetImportCsvParser`, `MaintainArrImportBatch` entity, `/api/imports/assets/*`
- **MaintainArr Frontend** (`apps/maintainarr-frontend`): `AssetBulkImportPanel` on Settings workspace
- **Integration tests** (`tests/STLCompliance.MaintainArr.Auth.Tests`): `MaintainArrAssetBulkImportTests`

## Schema

### `maintainarr_import_batches`

| Column | Type | Notes |
|--------|------|-------|
| id | uuid | PK |
| tenant_id | uuid | |
| import_type | text | `assets` |
| phase | text | `validate` or `commit` |
| status | text | `completed` / `failed` |
| total_rows | int | |
| success_count | int | |
| error_count | int | |
| created_at | timestamptz | |
| created_by_user_id | uuid | nullable |

Migration: `MaintainArrImportBatches` (EF).

## API + auth changes

### MaintainArr user APIs (JWT + MaintainArr entitlement)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/imports/assets/validate` | `RequireAssetImportManage` → `RequireAssetsManage` (tenant admin, maintainarr admin, manager) |
| POST | `/api/imports/assets/commit` | same |

### Request formats

**JSON**

```json
{
  "assets": [
    {
      "assetClassKey": "vehicles",
      "assetTypeKey": "forklift",
      "assetTag": "FLT-101",
      "name": "Forklift 101",
      "description": "",
      "siteRef": "yard-a",
      "lifecycleStatus": "active"
    }
  ]
}
```

**Multipart** — form field `file` (CSV with header row)

### CSV columns

`assetClassKey`, `assetTypeKey`, `assetTag`, `name`, `description`, `siteRef`, `lifecycleStatus`

### Validation rules

- Max 100 rows per request
- Resolves `assetClassKey` + `assetTypeKey` to active class/type within tenant
- Duplicate `assetTag` within batch → row error (`assets.duplicate_tag`)
- Existing tenant `assetTag` → row error
- Validate phase: dry-run, persists batch record only
- Commit phase: creates assets, per-row `asset.create` audit

### Audit events

- `maintainarr.imports.assets.validate` — validate phase summary
- `maintainarr.imports.assets.commit` — commit phase summary
- `asset.create` per created asset (`reasonCode: bulk_import`)

## Permission keys

- **Manage:** `maintainarr.assets.manage` via `RequireAssetImportManage`

## Frontend changes

- **AssetBulkImportPanel** — CSV textarea with template, Validate + Commit import buttons, per-row results
- Wired in **Settings** workspace section with `canManage`; refetches assets query after successful commit

## Worker / events

None.

## Tests

### Backend integration (`MaintainArrAssetBulkImportTests`)

- `Asset_import_validate_does_not_persist`
- `Asset_import_commit_creates_assets`
- `Asset_import_reports_duplicate_tag_in_batch`
- `Asset_import_denies_unauthenticated`

### Frontend (`AssetBulkImportPanel.test.tsx`)

- Read-only notice for non-writers
- Import controls for writers
- CSV header parse error before API call

## Next slice

**Worker 207** — MaintainArr M12 exports (report/data export surfaces per backlog).
