# Worker 58 — MaintainArr PM program builder

## Slice name

M7 maintenance spine — PM programs grouping PM schedules with asset-type or asset scope, JWT CRUD APIs, schedule assignment, maintainarr-frontend program builder panel, integration and frontend tests.

## Products touched

- **MaintainArr API** (`apps/maintainarr-api`): `maintainarr_pm_programs`, `maintainarr_pm_program_schedules`, `PmProgramService`, `/api/preventive-maintenance/programs` endpoints, audit events, EF migration.
- **maintainarr-frontend**: `PmProgramBuilderPanel` on home workspace, PM program and schedule API client methods.

## Schema

Migration: `MaintainArrPmPrograms`

Added MaintainArr tables:

- `maintainarr_pm_programs` — tenant-scoped PM program catalog (`programKey`, `name`, `scopeType` asset_type/asset, optional `assetTypeId` or `assetId`, `status` draft/active/inactive)
- `maintainarr_pm_program_schedules` — junction linking programs to `maintainarr_pm_schedules` with `sortOrder` (unique per schedule — one program per schedule)

Notes:

- Asset type scope groups schedules for all assets of that type.
- Asset scope groups schedules for a single asset.
- Schedule assignment validates scope match and rejects schedules already assigned elsewhere.

## API + auth changes

### MaintainArr user API (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/preventive-maintenance/programs` | PM read (technician+) |
| GET | `/api/preventive-maintenance/programs/{id}` | PM read — detail with schedules |
| POST | `/api/preventive-maintenance/programs` | PM manage (`maintainarr.pm.manage`) |
| PUT | `/api/preventive-maintenance/programs/{id}` | PM manage |
| PATCH | `/api/preventive-maintenance/programs/{id}/status` | PM manage — draft/active/inactive |
| PUT | `/api/preventive-maintenance/programs/{id}/schedules` | PM manage — replace assigned schedules |

Reuses existing `RequirePmRead` / `RequirePmManage` authorization from PM schedule endpoints.

## Frontend changes

- `PmProgramBuilderPanel` — list programs, create with scope, assign schedules, activate draft programs
- Home workspace integrates program builder below PM due panel
- API client: list/detail/create/replace-schedules/activate helpers; `getPmSchedules` for schedule picker

## Tests

### Backend integration (`STLCompliance.MaintainArr.Auth.Tests`)

- `Pm_program_builder_crud_happy_path`
- `Activate_program_without_schedules_returns_bad_request`
- `Pm_program_manage_denied_for_technician`
- `Assign_schedule_outside_scope_returns_bad_request`

### Frontend unit

- `PmProgramBuilderPanel.test.tsx` — list/detail rendering and empty state
- `client.test.ts` — PM program list and PM schedule list success paths

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~PmProgram"
cd apps/maintainarr-frontend
npm run test
npm run build
```

## Remaining gaps

- Bulk program rollout to assets (auto-create schedules from program template)
- Program-level due rollup dashboard
- PM program linkage to inspection templates / work-order templates

## Next recommended slice

**MaintainArr maintenance history** or **asset readiness endpoint** per M7 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
