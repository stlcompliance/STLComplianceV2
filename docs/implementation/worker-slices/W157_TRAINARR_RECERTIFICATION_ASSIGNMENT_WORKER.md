# Worker 157 — TrainArr recertification assignment worker (M12)

## Slice name

M12 recertification assignment worker — tenant recertification settings, scheduled shared-worker scan for expiring qualifications, automatic recertification training assignments with StaffArr blocker publish, JWT admin settings UI, integration and frontend tests

## Products touched

- **TrainArr API** (`apps/trainarr-api`): recertification settings/run tables, `SourceQualificationIssueId` on assignments, `RecertificationAssignmentService`, internal + JWT endpoints
- **shared-worker** (`workers/shared-worker`): `TrainArrRecertificationAssignmentJob`, client, options
- **StaffArr API** (integration consumer): training blocker ingest on recertification assignment create
- **TrainArr Frontend** (`apps/trainarr-frontend`): `RecertificationSettingsPanel`, Settings workspace wiring

## Schema

Migration `TrainArrRecertificationAssignment`:

- `trainarr_tenant_recertification_settings` — per-tenant enable flag + lead days (default 30)
- `trainarr_recertification_assignment_runs` — worker outcome audit (assigned/skipped)
- `trainarr_training_assignments.SourceQualificationIssueId` — links recertification assignment to source qualification

## API + auth changes

### TrainArr JWT (trainarr admin)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/recertification-settings` | `RequireRecertificationSettingsManage` |
| PUT | `/api/recertification-settings` | Same |
| GET | `/api/recertification-settings/runs` | Same |

### TrainArr internal (shared-worker)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/internal/recertification/pending` | source `shared-worker`, scope `trainarr.recertification.assign` |
| POST | `/api/internal/recertification/process-batch` | Same |

Assignment responses include `sourceQualificationIssueId` when reason is `recertification`.

## Permission keys

- JWT: trainarr admin / tenant_admin via `RequireRecertificationSettingsManage`
- Worker scope: `trainarr.recertification.assign`

## Worker behavior

`TrainArrRecertificationAssignmentJob` runs on a configurable interval (default 30 min), calls `POST /api/internal/recertification/process-batch` with a NexArr service token. For each tenant with recertification enabled, issued/suspended qualifications expiring within `LeadDays` are candidates. Each candidate gets a `recertification` assignment from the original training definition, StaffArr missing-assignment blocker, notification enqueue, and run audit. Duplicate active recertification assignments and prior successful runs are skipped.

## Frontend changes

- **RecertificationSettingsPanel** on TrainArr Settings workspace — enable toggle, lead days, recent worker runs from real API

## Tests

### Backend integration (`StaffArrTrainArrRecertificationAssignmentWorkerTests`)

- Service token auth rejection
- Pending list before processing
- Process batch creates recertification assignment + StaffArr blocker

### Unit (`RecertificationAssignmentRulesTests`)

- Lead window + status guards for assignment eligibility

### Frontend unit

- `RecertificationSettingsPanel.test.tsx`

## Verification commands

```powershell
dotnet build -c Release
dotnet test tests/STLCompliance.Shared.Worker.Tests/STLCompliance.Shared.Worker.Tests.csproj -c Release --filter "FullyQualifiedName~Recertification"
dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~Recertification"
cd apps/trainarr-frontend
npm run test -- --run RecertificationSettingsPanel
```

## Remaining gaps

- No post-expiry recertification (lead window requires future expiry only)
- No per-definition recertification lead override
- Expired qualifications rely on W44 expiration worker, not auto-recert after expiry

## Next recommended slice

**TrainArr qualification recalculation worker** or next open M12 worker backlog row from `02_PRODUCT_IMPLEMENTATION_BACKLOG.md`.
