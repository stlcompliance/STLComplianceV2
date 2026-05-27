# Worker 27 — TrainArr program builder / evidence capture foundations

## Slice name

M6 program builder and evidence capture — training programs linked to definitions, assignment evidence with local file storage, program CRUD/list, evidence attach/list APIs, JWT auth, trainarr-frontend panels, completion/blocker integration, cross-product and frontend tests

## Products touched

- **TrainArr API** (`apps/trainarr-api`): `trainarr_training_programs`, `trainarr_training_program_definitions`, `trainarr_training_evidence`, program and evidence endpoints, local evidence storage
- **TrainArr Frontend** (`apps/trainarr-frontend`): program builder panel, evidence capture on assignment detail
- **StaffArr integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): program/evidence auth and completion→blocker clear

## Schema

### TrainArr migration `TrainArrProgramBuilderEvidence`

- `trainarr_training_programs` — tenant-scoped programs (program key, name, description, status `draft` | `published`)
- `trainarr_training_program_definitions` — junction linking programs to `trainarr_training_definitions` with sort order
- `trainarr_training_evidence` — tenant-scoped evidence rows per assignment (type key, file metadata, storage key, notes, uploader)

Evidence bytes stored on disk under `EvidenceStorage:RootPath` (default `data/trainarr-evidence`).

## API + auth changes

### TrainArr user APIs (JWT + TrainArr entitlement)

| Method | Route | Auth |
|--------|-------|------|
| GET/POST | `/api/training-programs` | read: entitled users; create: `tenant_admin`, `trainarr_admin` |
| GET/PUT | `/api/training-programs/{id}` | read: entitled; update/publish: `tenant_admin`, `trainarr_admin` |
| GET/POST | `/api/training-assignments/{id}/evidence` | read: assignment read scope; upload: `trainarr.evidence.upload` (admin/trainer or assignment subject) |

Evidence upload accepts JSON with base64 content, writes file to tenant-scoped storage, and moves `assigned` assignments to `in_progress`.

Assignment detail includes `evidenceCount`. Completion flow unchanged: `POST .../complete` still clears StaffArr training blockers.

## Frontend changes

- **Program builder panel** — create programs from active definitions; list existing programs
- **Evidence capture panel** — list/upload evidence on selected assignment detail; shows evidence count
- Home workspace title updated to qualification workspace

## Tests

### Backend integration (`StaffArrTrainArrProgramEvidenceTests`)

- `Training_program_create_list_and_publish`
- `Training_program_create_denies_member_role`
- `Training_evidence_upload_list_and_complete_clears_blocker`
- `Training_evidence_upload_allows_member_self`

### Frontend unit

- `ProgramBuilderPanel.test.tsx` — admin form and member gate
- `EvidenceCapturePanel.test.tsx` — empty state, evidence list, upload button

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

- Program versioning, requirements, steps, and guided builder UX deferred
- Evidence download endpoint and virus scanning deferred
- Signoffs, evaluations, qualification issue, and positive StaffArr certification grant not wired
- Program-to-assignment applicability rules not enforced on assignment create

## Next recommended slice

**TrainArr signoffs / evaluations** (M6 continuation) or **Compliance Core rule version content + evaluation foundations** (M5) per milestone priority.
