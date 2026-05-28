# Worker 219 — StaffArr read of TrainArr person training history (M12)

## Slice name

M12 person training history — StaffArr integration read of TrainArr materialized `trainarr_person_training_history_entries` (completes W160 TrainArr gap: workforce profile shows TrainArr-owned training timeline via API read-through).

## Products touched

- **TrainArr API**: integration `GET /api/integrations/person-training-history`, scope `trainarr.person_training_history.read`
- **StaffArr API**: `TrainarrPersonTrainingHistoryService`, `GET /api/people/{personId}/trainarr-training-history`
- **STLCompliance.Shared**: `staffarr-trainarr` token profile scope widened
- **staffarr-frontend**: `PersonTrainarrTrainingHistoryPanel` on People workspace
- **Tests**: `StaffArrTrainArrPersonTrainingHistoryTests`, `PersonTrainarrTrainingHistoryPanel.test.tsx`

## TrainArr

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/integrations/person-training-history?tenantId=&staffarrPersonId=&limit=` | Service token: source `staffarr`, scope `trainarr.person_training_history.read` |

Returns existing `PersonTrainingHistoryResponse` from `PersonTrainingHistoryService`.

## StaffArr

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/people/{personId}/trainarr-training-history?limit=` | `RequirePersonHistoryRead` (same as person timeline) |

Response includes `sourceProduct: trainarr` and `sourceNote` clarifying read-through (not StaffArr-owned truth).

### Audit

- `staffarr.trainarr_training_history.read`

## Frontend

- People profile shows **TrainArr training history** panel below person timeline when a person is selected.

## Tests

| Suite | Coverage |
|-------|----------|
| `StaffArrTrainArrPersonTrainingHistoryTests` | StaffArr reads history via TrainArr integration; JWT rejected on integration endpoint |
| `PersonTrainarrTrainingHistoryPanel.test.tsx` | Renders entries and empty state |

## Verification

```powershell
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~StaffArrTrainArrPersonTrainingHistory"
cd apps/staffarr-frontend
npm run test -- --run PersonTrainarrTrainingHistoryPanel
```

## Relationship to W160

W160 materializes history in TrainArr and exposes TrainArr-native read APIs. W219 wires **StaffArr person profile** to that data without cross-product DB access.

## Next recommended slice

Per `02_PRODUCT_IMPLEMENTATION_BACKLOG.md` — e.g. **Compliance Core M12** (source ingestion, rule change monitoring), **NexArr M12** audit export enhancements, **RoutArr** audit package export, or **TrainArr** notification settings UX depth.
