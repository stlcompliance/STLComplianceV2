# Worker 22 — TrainArr training assignment engine

## Slice name

M6 qualification spine — training definitions, assignments entity, create/list/complete APIs, remediation linkage, StaffArr blocker publish/clear on assignment lifecycle, TrainArr user auth, assignments UI, cross-product tests

## Products touched

- **TrainArr API** (`apps/trainarr-api`): `trainarr_training_definitions`, `trainarr_training_assignments`, user auth spine, assignment/remediation/definition endpoints
- **TrainArr Frontend** (`apps/trainarr-frontend`): handoff shell, assignments list/detail, remediation→assignment workflow
- **StaffArr API** (integration consumer): training blocker ingest/clear via existing integration APIs
- **StaffArr integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): end-to-end remediation→assignment→completion→blocker clear

## Schema

### TrainArr migration `TrainArrTrainingAssignmentEngine`

- `trainarr_training_definitions` — tenant-scoped training catalog (definition key, name, description, qualification key/name, status)
- `trainarr_training_assignments` — tenant-scoped assignments referencing `staffarr_person_id` (opaque), internal FK to `trainarr_training_definitions`, optional internal FK to `trainarr_staffarr_incident_remediations`
  - assignment reason (`manual`, `incident_remediation`), status (`assigned`, `in_progress`, `completed`, `cancelled`), due/completed metadata, `blocker_publication_id` linking to `trainarr_certification_publications`

## API + auth changes

### TrainArr user APIs (JWT + TrainArr entitlement)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/auth/handoff/redeem` | Anonymous |
| GET | `/api/session`, `/api/me` | JWT + TrainArr entitlement |
| GET/POST | `/api/training-definitions` | read: entitled users; create: `tenant_admin`, `trainarr_admin` |
| GET/POST | `/api/training-assignments` | list/read: admin/trainer or self; create: `tenant_admin`, `trainarr_admin` |
| GET | `/api/training-assignments/{id}` | same as list/read |
| POST | `/api/training-assignments/{id}/complete` | admin/trainer or assignment subject (`tenant_member` self) |
| GET | `/api/incident-remediations` | `tenant_admin`, `trainarr_admin`, `trainarr_trainer` |

Role keys map to permission keys `trainarr.assignments.create` and `trainarr.assignments.complete` per `21_PERMISSION_KEYS_AND_DEFAULT_ROLES.md`.

### Assignment lifecycle integration

- **On create:** publishes `missing_assignment` training blocker to StaffArr via existing `StaffArrTrainingBlockerClient` + local `trainarr_certification_publications` record; updates linked remediation status to `assignment_created`
- **On complete:** clears StaffArr training blocker for stored publication id; updates linked remediation status to `completed`

Existing service-token integration endpoints unchanged (`/api/integrations/incident-remediations`, `/api/certification-publications`).

## Frontend changes

- New **TrainArr frontend** app on port 5176 with NexArr handoff redeem
- **Assignments panel** — list, detail, complete action with role-aware authorization
- **Remediation → assignment panel** — select pending remediation + definition, create assignment via real API

## Tests

### Backend integration (`StaffArrTrainArrTrainingAssignmentTests`)

- `Remediation_to_assignment_completion_clears_staffarr_training_blocker`
- `Training_assignment_create_denies_member_role`
- `Training_assignment_list_allows_member_self_scope`
- `Training_assignment_rejects_duplicate_active_remediation_assignment`

### Frontend unit

- `AssignmentsPanel.test.tsx` — empty state, assignment row rendering, complete button
- `RemediationAssignmentPanel.test.tsx` — manage gate, pending remediation workflow

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release
dotnet test "tests/STLCompliance.Health.Tests/STLCompliance.Health.Tests.csproj" -c Release --filter "FullyQualifiedName~TrainArr"
cd apps/trainarr-frontend
npm install
npm run test -- --run
npm run build
```

## Remaining gaps

- Program builder, evidence upload, signoffs, evaluations, and qualification issue remain future M6 slices
- Positive qualification grant to StaffArr person certifications not wired (blocker clear only)
- TrainArr dedicated auth test project not added (covered via cross-product tests)
- Full TrainArr shell navigation beyond assignments dashboard

## Next recommended slice

**TrainArr program builder / training evidence capture** (M6 continuation) or **Compliance Core vocabulary spine** (M5) per milestone priority.
