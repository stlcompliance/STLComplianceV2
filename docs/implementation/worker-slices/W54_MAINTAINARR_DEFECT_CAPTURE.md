# Worker 54 — MaintainArr defect capture

## Slice name

M7 maintenance spine — defect records linked to inspection runs and assets, auto-create on failed inspection completion, manual capture APIs, JWT auth, maintainarr-frontend defects panel, integration and frontend tests.

## Products touched

- **MaintainArr API** (`apps/maintainarr-api`): `maintainarr_defects`, `DefectService`, `/api/defects` and `/api/inspections/{id}/defects`, audit events, EF migration `MaintainArrDefects`.
- **MaintainArr Frontend** (`apps/maintainarr-frontend`): `DefectsPanel`, inspection runner capture action, API client methods.

## Schema

Migration: `MaintainArrDefects`

Added MaintainArr table:

- `maintainarr_defects` — tenant-scoped defect records (`assetId`, optional `inspectionRunId`, optional `checklistItemId`, `title`, `description`, `severity`, `status`, `source`, `reportedByUserId`, timestamps); unique per run+checklist item when checklist item is set

Notes:

- Completing an inspection run with fail answers auto-creates defects (`source` = `inspection_auto`).
- Manual capture via `POST /api/defects` or `POST /api/inspections/{id}/defects` (`inspection_manual`).
- Status lifecycle: `open`, `acknowledged`, `in_repair`, `resolved`, `closed`.
- Work orders, readiness restriction, and escalation deferred to later slices.

## API + auth changes

### MaintainArr user API (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/defects` | Defects read — managers see all; technicians see defects they reported |
| GET | `/api/defects/{id}` | Defects read — same visibility as list |
| POST | `/api/defects` | Defects create (technician+) — manual defect on active asset |
| PATCH | `/api/defects/{id}/status` | Defects status manage (manager+) |
| POST | `/api/inspections/{id}/defects` | Defects create + inspection run access — idempotent capture from failed items |

`MaintainArrAuthorizationService` adds `RequireDefectsRead`, `RequireDefectsCreate`, `RequireDefectsStatusManage`, `CanViewAllDefects`, and `RequireDefectAccess`.

`InspectionRunService.CompleteAsync` auto-creates defects when the run result is `failed`.

## Frontend changes

- `DefectsPanel` — list/filter defects, manual report form, manager status updates
- `InspectionRunnerPanel` — capture defects action on failed completed runs (idempotent with auto-create)
- Home workspace integrates defects panel above inspection runner
- API client: list/get/create/update status + create from inspection run

## Tests

### Backend integration (`STLCompliance.MaintainArr.Auth.Tests`)

- `Failed_inspection_completion_auto_creates_defect`
- `Manual_defect_create_and_status_update`
- `Manual_create_from_inspection_is_idempotent`
- `Technician_cannot_view_other_users_defect`
- `Technician_cannot_update_defect_status`

### Frontend unit

- `DefectsPanel.test.tsx` — list + empty state
- `InspectionRunnerPanel.test.tsx` — capture defects button on failed runs
- `client.test.ts` — defects list success path

## Verification commands

```powershell
dotnet build "apps/maintainarr-api/MaintainArr.Api/MaintainArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj" -c Release
cd apps/maintainarr-frontend
npm run test
npm run build
```

## Remaining gaps

- Work-order generation from defects
- Critical/safety defect asset readiness restriction
- Defect escalation worker
- Meter tracking (alternate M7 slice)

## Next recommended slice

**MaintainArr meter tracking** or **work-order lifecycle** per M7 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
