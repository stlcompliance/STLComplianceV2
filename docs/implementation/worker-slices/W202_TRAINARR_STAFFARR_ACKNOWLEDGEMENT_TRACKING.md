# Worker 202 — TrainArr M10 StaffArr acknowledgement tracking

**Milestone:** M10  
**Backlog:** TrainArr `[M10] StaffArr acknowledgement tracking`

## Summary

TrainArr publishes training assignment acknowledgement requests to StaffArr on assignment create/recertification. StaffArr stores durable acknowledgement state per person. TrainArr mirrors StaffArr status on assignments and blocks evidence upload until the person acknowledges in StaffArr.

## StaffArr

- Table `staffarr_person_training_acknowledgements` (`PersonTrainingAcknowledgement`)
- Integration ingest/supersede/status:
  - `POST /api/integrations/training-acknowledgements`
  - `POST /api/integrations/training-acknowledgements/supersede`
  - `GET /api/integrations/training-acknowledgements/status`
- JWT user APIs:
  - `GET /api/training-acknowledgements`
  - `POST /api/training-acknowledgements/{id}/acknowledge`
- Service token scopes: `staffarr.training_acknowledgements.write`, `staffarr.training_acknowledgements.read`
- Field inbox tasks (`training_acknowledgement`) for pending items when `personId` filter is set
- Frontend: `/training-acknowledgements` workspace + `TrainingAcknowledgementsPanel`

## TrainArr

- Mirror columns on `trainarr_training_assignments`:
  - `StaffarrAcknowledgementRequestId` (uses assignment id)
  - `StaffarrAcknowledgementStatus`
  - `StaffarrAcknowledgementAt`
- `StaffArrTrainingAcknowledgementClient` + `TrainingAcknowledgementPublicationService`
- Publish on assignment create/recertification; sync on assignment GET; supersede open requests on completion
- Evidence gate: `evidence.acknowledgement_required` until StaffArr status is `acknowledged`
- Assignment detail API/UI exposes acknowledgement fields
- Token catalog: `trainarr-staffarr` profile extended with acknowledgement scopes

## Tests

- `StaffArrTrainArrTrainingAcknowledgementTests` (ingest/status, assignment publish, evidence gate, acknowledge flow)
- Updated TrainArr cross-product tests for acknowledgement service token + HTTP client registration
- `TrainArrAcknowledgementTestHelper` for evidence tests

## Verification

```bash
dotnet build apps/staffarr-api/StaffArr.Api/StaffArr.Api.csproj
dotnet build apps/trainarr-api/TrainArr.Api/TrainArr.Api.csproj
dotnet test tests/STLCompliance.StaffArr.Auth.Tests --filter FullyQualifiedName~TrainingAcknowledgement
```

## Next backlog

Per suite backlog after M10 acknowledgement: **MaintainArr M12 maintenance reports** / executive reports, or remaining **NexArr M12** platform workers — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
