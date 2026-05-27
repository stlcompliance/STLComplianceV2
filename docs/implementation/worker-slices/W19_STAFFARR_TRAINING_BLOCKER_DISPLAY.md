# Worker 19 — StaffArr training blocker display + TrainArr publication spine

## Slice name

M4 workforce spine — TrainArr certification publication to StaffArr training blocker mirror, readiness integration, service-token integration APIs, and readiness UI training blocker display

## Products touched

- **STLCompliance.Shared** (`packages/shared-dotnet`): `StlServiceTokenValidator` for cross-product JWT service token validation
- **StaffArr API** (`apps/staffarr-api`): `staffarr_person_training_blockers` mirror, integration ingest/clear APIs, readiness calculation merge
- **TrainArr API** (`apps/trainarr-api`): `trainarr_certification_publications`, `POST /api/certification-publications`, StaffArr publish client
- **StaffArr Frontend** (`apps/staffarr-frontend`): readiness panel training blocker rendering
- **StaffArr integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): cross-product publication + ingest tests

## Schema

### StaffArr migration `StaffArrTrainingBlockerDisplay`

- `staffarr_person_training_blockers` — tenant-scoped mirror of TrainArr training blockers
  - `person_id` (StaffArr-local FK only)
  - `trainarr_publication_id` (external id, unique per tenant)
  - qualification key/name, blocker type, message, status, published/expires/cleared timestamps

### TrainArr migration `TrainArrCertificationPublicationFoundations`

- `trainarr_certification_publications` — TrainArr-owned publication records referencing `staffarr_person_id` (opaque GUID, no cross-DB FK)

## API + auth changes

### StaffArr integration (service token)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/integrations/training-blockers` | NexArr service token: source `trainarr`, allowed `staffarr`, scope `staffarr.training_blockers.write`, tenant scope |
| POST | `/api/integrations/training-blockers/clear` | same |

### TrainArr publication (service token)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/certification-publications` | Service token: source `trainarr`, allowed `trainarr`, scope `trainarr.certification_publications.write`, tenant scope |

Publishes a training blocker to StaffArr via configured `StaffArr:ServiceToken`.

### Readiness API (extended)

- `GET /api/people/{personId}/readiness` and `GET /api/readiness?personId=` now return:
  - `blockers[]` with `blockerSource` (`certification` | `training`)
  - `readinessBasis` may be `training_blockers` when active training blockers exist
  - Training blockers make a person `not_ready` unless a manual override is active

## Frontend changes

- **Readiness panel** distinguishes certification vs training blockers (violet training styling + label)
- Types updated for extended blocker shape and `training_blockers` readiness basis

## Tests

### Backend integration (`StaffArrTrainArrTrainingBlockerTests`)

- `Training_blocker_ingest_shows_on_person_readiness`
- `Trainarr_publication_publishes_training_blocker_to_staffarr_readiness`
- `Training_blocker_ingest_rejects_missing_service_token`
- `Training_blocker_clear_removes_active_blocker_from_readiness`

### Frontend unit

- `ReadinessPanel.test.tsx` — training blocker label rendering

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release
dotnet test "tests/STLCompliance.Health.Tests/STLCompliance.Health.Tests.csproj" -c Release --filter "FullyQualifiedName~TrainArr"
cd apps/staffarr-frontend
npm run test -- --run
npm run build
```

## Remaining gaps

- Service token validation is cryptographic/claims-based only (no NexArr registry revocation check on product APIs yet)
- TrainArr publication endpoint uses service tokens (TrainArr user auth spine remains M6)
- Positive qualification grants via `trainarr_publication` source on person certifications not wired yet
- TrainArr dedicated auth test project not added (covered via StaffArr cross-product tests)

## Next recommended slice

StaffArr person timeline foundations or TrainArr training assignment engine per M4/M6 backlog order (incident routing completed in Worker 20).
