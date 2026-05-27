# Worker 28 — TrainArr signoffs / evaluations foundations

## Slice name

M6 signoffs and evaluations — evaluation and signoff records linked to training assignments, submit/list APIs, JWT role scopes, completion gate before StaffArr blocker clear, trainarr-frontend panels, cross-product and frontend tests

## Products touched

- **TrainArr API** (`apps/trainarr-api`): `trainarr_training_evaluations`, `trainarr_training_signoffs`, evaluation/signoff endpoints, completion gate on assignment complete
- **TrainArr Frontend** (`apps/trainarr-frontend`): evaluation and signoff panel on assignment detail
- **StaffArr integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): signoff/evaluation auth, completion gate, blocker clear after requirements met

## Schema

### TrainArr migration `TrainArrSignoffsEvaluations`

- `trainarr_training_evaluations` — one evaluation per assignment (unique tenant + assignment), result `pass` | `fail` | `incomplete`, optional score, notes, evaluator user
- `trainarr_training_signoffs` — trainee and trainer signoffs per assignment (unique tenant + assignment + role)

## API + auth changes

### TrainArr user APIs (JWT + TrainArr entitlement)

| Method | Route | Auth |
|--------|-------|------|
| GET/POST | `/api/evaluations` | read: assignment read scope; submit: `trainarr_trainer`, `trainarr_admin`, `tenant_admin` |
| GET/POST | `/api/signoffs` | read: assignment read scope; trainee signoff: assignment subject (`tenant_member` self); trainer signoff: trainer/admin scope |
| GET/POST | `/api/training-assignments/{id}/evaluations` | same as `/api/evaluations` |
| GET/POST | `/api/training-assignments/{id}/signoffs` | same as `/api/signoffs` |

Assignment detail includes `evaluation`, `signoffs`, and `completionRequirementsMet`.

### Completion gate

`POST /api/training-assignments/{id}/complete` requires:

1. Passing evaluation (`result` = `pass`)
2. Trainee signoff
3. Trainer signoff

Only then does the flow clear the StaffArr training blocker publication.

## Frontend changes

- **SignoffEvaluationPanel** — submit evaluation (trainer/admin), trainee and trainer signoffs, completion gate status on assignment detail
- Complete button enabled only when `completionRequirementsMet` is true for the selected assignment

## Tests

### Backend integration (`StaffArrTrainArrSignoffsEvaluationsTests`)

- `Evaluation_submit_and_signoffs_list_on_assignment_detail`
- `Complete_denied_until_evaluation_and_signoffs_recorded`
- `Evaluation_submit_denies_member_role`
- `Trainee_signoff_denies_trainer_for_other_person`
- `Fail_evaluation_blocks_completion_gate`

Updated existing TrainArr completion tests to satisfy the gate via `TrainArrCompletionTestHelper`.

### Frontend unit

- `SignoffEvaluationPanel.test.tsx` — empty state, evaluation form, requirements met display

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~TrainArr"
cd apps/trainarr-frontend
npm install
npm run test -- --run
npm run build
```

## Remaining gaps

- Evaluator role distinct from trainer, practical step builder, and re-evaluation workflows deferred
- Qualification issue and positive StaffArr certification grant not wired
- Top-level list without `trainingAssignmentId` returns tenant-wide rows (admin tooling only)

## Next recommended slice

**TrainArr qualification issue + StaffArr certification grant** (M6 continuation) or **Compliance Core rule version content + evaluation foundations** (M5) per milestone priority.
