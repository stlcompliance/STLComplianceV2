# Worker 29 — TrainArr qualification issue + StaffArr certification grant

## Slice name

M6 qualification issue — TrainArr qualification issue record on assignment completion, positive certification grant publication to StaffArr, integration ingest API with service token auth, trainarr/staffarr frontend visibility, cross-product tests

## Products touched

- **TrainArr API** (`apps/trainarr-api`): `trainarr_qualification_issues`, qualification grant publication (`qualification_grant`), `QualificationIssueService`, `StaffArrCertificationGrantClient`, completion flow extension
- **StaffArr API** (`apps/staffarr-api`): `POST /api/integrations/certification-grants`, `CertificationGrantIngestionService`, person certification `trainarr_publication` source, unique index on external publication id
- **TrainArr Frontend** (`apps/trainarr-frontend`): qualification issued display on completed assignment detail
- **StaffArr Frontend** (`apps/staffarr-frontend`): TrainArr source label and publication id on certification panel
- **StaffArr integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): `StaffArrTrainArrQualificationGrantTests` + updated TrainArr completion tests

## Schema

### TrainArr migration `TrainArrQualificationIssue`

- `trainarr_qualification_issues` — one issue per completed assignment (unique tenant + assignment)
  - links to `trainarr_certification_publications` via `grant_publication_id` (opaque id, no cross-DB FK)
  - qualification key/name, status `issued`, issued timestamp

### StaffArr migration `StaffArrCertificationGrantIngest`

- Unique filtered index on `staffarr_person_certifications (tenant_id, external_publication_id)` for idempotent TrainArr grant replay

## API + auth changes

### StaffArr integration (service token)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/integrations/certification-grants` | NexArr service token: source `trainarr`, allowed `staffarr`, scope `staffarr.certification_grants.write`, tenant scope |

Ingest payload: tenant, person, TrainArr publication/assignment ids, qualification key/name, training definition name, granted/expires timestamps, notes.

Resolves certification definition by qualification key (existing readiness key or auto-upsert `trainarr.{qualificationKey}`). Grants `PersonCertification` with `source_type` = `trainarr_publication`.

### TrainArr completion flow

`POST /api/training-assignments/{id}/complete` (after existing completion gate):

1. Clears StaffArr training blocker publication (unchanged)
2. Publishes `qualification_grant` certification publication
3. Calls StaffArr certification grant ingest
4. Persists `trainarr_qualification_issues` record

Assignment detail and complete response include `qualificationIssue` when issued.

## Frontend changes

- **TrainArr HomePage** — emerald “Qualification issued” block on completed assignments with grant publication id
- **StaffArr CertificationPanel** — human-readable TrainArr source label and external publication id for grants

## Tests

### Backend integration (`StaffArrTrainArrQualificationGrantTests`)

- `Assignment_completion_issues_qualification_and_grants_staffarr_certification`
- `Readiness_qualification_grant_satisfies_readiness_requirement`
- `Certification_grant_ingest_rejects_missing_service_token`
- `Certification_grant_ingest_is_idempotent_by_publication_id`

Updated `StaffArrTrainArrTrainingAssignmentTests` remediation completion path to assert qualification issue + StaffArr certification.

### Frontend unit

- `CertificationPanel.test.tsx` — TrainArr publication source rendering

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~TrainArr"
cd apps/trainarr-frontend
npm run test -- --run
npm run build
cd ../staffarr-frontend
npm run test -- --run
npm run build
```

## Remaining gaps

- Qualification reinstate / unsuspend workflow deferred (see Worker 31)
- No NexArr service-token registry revocation check on product APIs yet
- TrainArr dedicated auth test project not added (covered via StaffArr cross-product tests)

## Next recommended slice

**Compliance Core rule version content + evaluation foundations** (M5) or **TrainArr qualification suspend/revoke/expire** (M6) per milestone priority.
