# Worker 52 — MaintainArr inspection template builder

## Slice name

M7 maintenance spine — inspection templates with categories, checklist items, asset-type linkage, JWT CRUD APIs, maintainarr-frontend template builder UI, integration and frontend tests.

## Products touched

- **MaintainArr API** (`apps/maintainarr-api`): inspection template domain tables, `InspectionTemplateService`, `/api/inspection-templates` endpoints, audit events, EF migration.
- **MaintainArr Frontend** (`apps/maintainarr-frontend`): `InspectionTemplateBuilderPanel` on home workspace, API client methods.

## Schema

Migration: `MaintainArrInspectionTemplates`

Added MaintainArr tables:

- `maintainarr_inspection_templates` — tenant-scoped template catalog (`templateKey`, `name`, `version`, `status` draft/active/inactive)
- `maintainarr_inspection_template_categories` — ordered sections within a template (`categoryKey`, `name`, `sortOrder`)
- `maintainarr_inspection_checklist_items` — checklist prompts (`itemKey`, `prompt`, `itemType` pass_fail/numeric/text, `isRequired`, optional `categoryId`)
- `maintainarr_inspection_template_asset_types` — many-to-many link from templates to `maintainarr_asset_types`

Notes:

- Template `version` increments when categories, items, or asset-type links change (builder edits).
- Activating a template requires at least one checklist item.
- Inspection runs and defect capture deferred to a later slice.

## API + auth changes

### MaintainArr user API (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/inspection-templates` | Inspections read (technician+) |
| GET | `/api/inspection-templates/{id}` | Inspections read — full detail |
| POST | `/api/inspection-templates` | Inspections manage (`maintainarr.inspections.manage`) |
| PUT | `/api/inspection-templates/{id}` | Inspections manage |
| PATCH | `/api/inspection-templates/{id}/status` | Inspections manage — draft/active/inactive |
| POST | `/api/inspection-templates/{id}/categories` | Inspections manage |
| PUT | `/api/inspection-templates/{id}/categories/{categoryId}` | Inspections manage |
| DELETE | `/api/inspection-templates/{id}/categories/{categoryId}` | Inspections manage |
| POST | `/api/inspection-templates/{id}/checklist-items` | Inspections manage |
| PUT | `/api/inspection-templates/{id}/checklist-items/{itemId}` | Inspections manage |
| DELETE | `/api/inspection-templates/{id}/checklist-items/{itemId}` | Inspections manage |
| PUT | `/api/inspection-templates/{id}/asset-types` | Inspections manage — replace linked asset types |

`MaintainArrAuthorizationService` adds `RequireInspectionsRead` and `RequireInspectionsManage` (mirrors asset/PM role gates).

## Frontend changes

- `InspectionTemplateBuilderPanel` — list templates, create template, add categories/items, link asset types, activate draft templates
- Home workspace integrates template builder above asset registry
- API client: list/detail/create/category/item/asset-type/activate helpers

## Tests

### Backend integration (`STLCompliance.MaintainArr.Auth.Tests`)

- `Inspection_template_builder_crud_happy_path`
- `Activate_template_without_checklist_items_returns_bad_request`
- `Inspection_template_manage_denied_for_technician`

### Frontend unit

- `InspectionTemplateBuilderPanel.test.tsx` — list/detail rendering and empty state
- `client.test.ts` — inspection template list success path

## Verification commands

```powershell
dotnet build "apps/maintainarr-api/MaintainArr.Api/MaintainArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj" -c Release
cd apps/maintainarr-frontend
npm run test
npm run build
```

## Remaining gaps

- Inspection runner, runs, answers, evidence, signatures (later M7 slice)
- Inspection due scan worker (after runner foundations)
- Versioned template publish workflow (immutable published revisions)
- Dynamic inspection rules and Compliance Core vocabulary binding for `inspection_category`

## Next recommended slice

**MaintainArr meter tracking** or **inspection runner foundations** per M7 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
