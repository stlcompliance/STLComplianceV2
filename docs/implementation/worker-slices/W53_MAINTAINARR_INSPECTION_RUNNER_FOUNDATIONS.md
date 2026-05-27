# Worker 53 — MaintainArr inspection runner foundations

## Slice name

M7 maintenance spine — inspection runs with checklist answers, JWT execute/list APIs, maintainarr-frontend inspection runner UI, integration and frontend tests.

## Products touched

- **MaintainArr API** (`apps/maintainarr-api`): `maintainarr_inspection_runs`, `maintainarr_inspection_run_answers`, `InspectionRunService`, `/api/inspections` endpoints, audit events, EF migration `MaintainArrInspectionRuns`.
- **MaintainArr Frontend** (`apps/maintainarr-frontend`): `InspectionRunnerPanel` on home workspace, API client methods.

## Schema

Migration: `MaintainArrInspectionRuns`

Added MaintainArr tables:

- `maintainarr_inspection_runs` — tenant-scoped execution records (`assetId`, `inspectionTemplateId`, `templateVersion` snapshot, `status` in_progress/completed, `result` passed/failed, `startedByUserId`, timestamps)
- `maintainarr_inspection_run_answers` — answers linked to run + checklist item (`passFailValue`, `numericValue`, `textValue`, `answeredByUserId`, `answeredAt`); unique per run/item

Notes:

- Runs require an **active** template with checklist items; asset must be **active**.
- When a template has asset-type links, the asset type must match.
- Only one in-progress run per asset+template at a time.
- Completing validates required answers and sets `passed` unless any pass/fail answer is `fail`.
- Defect capture, evidence, and signatures deferred to a later slice.

## API + auth changes

### MaintainArr user API (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/inspections` | Inspections read — managers see all runs; technicians see runs they started |
| GET | `/api/inspections/{id}` | Inspections read — same visibility as list |
| POST | `/api/inspections` | Inspections execute (technician+) — start run for asset + template |
| PUT | `/api/inspections/{id}/answers` | Inspections execute — upsert answers (batch) |
| POST | `/api/inspections/{id}/complete` | Inspections execute — complete run, compute passed/failed |

`MaintainArrAuthorizationService` adds `RequireInspectionsExecute`, `CanViewAllInspectionRuns`, and `RequireInspectionRunAccess`.

## Frontend changes

- `InspectionRunnerPanel` — select asset/template, start run, answer checklist (pass/fail, numeric, text), save answers, complete run, browse run history
- Home workspace integrates runner above template builder
- API client: list/get/start/submit/complete helpers

## Tests

### Backend integration (`STLCompliance.MaintainArr.Auth.Tests`)

- `Inspection_run_happy_path_passes`
- `Inspection_run_fail_answer_marks_run_failed`
- `Technician_cannot_view_other_users_inspection_run`
- `Start_run_requires_active_template`

### Frontend unit

- `InspectionRunnerPanel.test.tsx` — active run UI and empty history
- `client.test.ts` — inspection runs list success path

## Verification commands

```powershell
dotnet build "apps/maintainarr-api/MaintainArr.Api/MaintainArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj" -c Release
cd apps/maintainarr-frontend
npm run test
npm run build
```

## Remaining gaps

- Defect capture from failed items (next M7 slice)
- Inspection evidence, signatures, offline/mobile capture
- Inspection due scan worker
- Dynamic inspection rules and Compliance Core vocabulary binding

## Next recommended slice

**MaintainArr defect capture** (failed inspection → defect record) or **meter tracking** per M7 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
