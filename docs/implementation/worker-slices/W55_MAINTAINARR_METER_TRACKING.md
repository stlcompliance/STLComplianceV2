# Worker 55 — MaintainArr meter tracking

## Slice name

M7 — asset meter definitions, meter readings (usage baselines), PM usage forecast linkage, JWT APIs, maintainarr-frontend capture UI, unit and integration tests

## Products touched

- **MaintainArr API** (`apps/maintainarr-api`): `maintainarr_asset_meters`, `maintainarr_meter_readings`, meter PM forecast on `maintainarr_pm_schedules`, user meter/reading APIs
- **maintainarr-frontend**: `MeterReadingsPanel` on home workspace, meter API client
- **Tests**: `MeterPmForecastRulesTests` (shared worker test project), `MaintainArrMeterTrackingTests` (integration)

## Schema

### Migration `MaintainArrMeterTracking`

- `maintainarr_asset_meters` — per-asset meter definitions (`meterKey`, `unit`, `baselineReading`, `currentReading`, `lastReadingAt`, `status`)
- `maintainarr_meter_readings` — reading history (`readingValue`, `deltaFromPrevious`, `readAt`, `recordedByUserId`, `isCorrection`, `notes`)
- `maintainarr_pm_schedules` extended — `scheduleMode` (`calendar` | `meter`), `asset_meter_id`, `interval_usage`, `next_due_at_usage`, `last_completed_usage`

## API + auth changes

### MaintainArr user API (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/assets/{assetId}/meters` | Meter read (technician+) |
| POST | `/api/assets/{assetId}/meters` | Meter manage (manager+) |
| GET | `/api/meters/{assetMeterId}` | Meter read |
| GET | `/api/meters/{assetMeterId}/readings` | Meter read |
| POST | `/api/meters/{assetMeterId}/readings` | Meter record (technician+) |
| GET | `/api/meters/{assetMeterId}/pm-forecast` | PM read — linked meter-based schedules |

PM schedule create/update accepts optional `scheduleMode`, `assetMeterId`, `intervalUsage`, `nextDueAtUsage` for meter-based PM.

Recording a reading updates meter `currentReading`, audits `meter_reading.record` / `meter_reading.correction`, and when usage crosses `nextDueAtUsage` marks linked active meter PM schedules `due` (`pm_schedule.meter_forecast.due` audit).

## Tests

### Unit (`MeterPmForecastRulesTests`)

- `ShouldMarkDueFromUsage` threshold and status guards
- `ComputeUsageUntilDue` remaining usage
- `ComputeInitialNextDueAtUsage` baseline + interval

### Integration (`MaintainArrMeterTrackingTests`)

- `Create_meter_record_reading_and_list_history`
- `Meter_reading_marks_linked_pm_schedule_due_from_usage`
- `Record_reading_rejects_regression_without_correction`
- `Record_reading_requires_authentication`

### Frontend

- `MeterReadingsPanel.test.tsx` — capture UI, forecast table, empty history
- `client.test.ts` — asset meters list, record reading, readings list

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.Shared.Worker.Tests/STLCompliance.Shared.Worker.Tests.csproj" -c Release --filter "FullyQualifiedName~MeterPm"
dotnet test "tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~Meter"
cd apps/maintainarr-frontend
npm run test
npm run build
```

## Remaining gaps

- Meter correction workflow UI (API supports `isCorrection`; dedicated correction panel deferred)
- PM completion does not yet roll `lastCompletedUsage` / advance `nextDueAtUsage` on meter schedules
- Calendar PM due scan unchanged; meter due is reading-driven only
- No rollover / max-reading wrap handling

## Next recommended slice

**MaintainArr work-order lifecycle** per M7 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
