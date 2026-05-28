# StaffArr export scheduled delivery foundations

## Slice name

M4/M12 StaffArr person export scheduled delivery — tenant schedule config, internal worker batch API, delivery run audit, shared-worker job, UI schedule panel, tests

## Products touched

- **StaffArr API** — `TenantPersonExportSchedule`, `PersonExportDeliveryRun`, schedule GET/PUT, internal delivery endpoints, `PersonExportDeliveryService`
- **shared-worker** — `StaffArrPersonExportDeliveryJob`, HTTP client, configuration
- **StaffArr Frontend** — scheduled delivery section in `PersonExportPanel`
- **Tests** — `StaffArrPersonExportDeliveryWorkerTests`, `PersonExportDeliveryRulesTests`

## Schema

### Migration `StaffArrPersonExportScheduledDelivery`

- `staffarr_tenant_person_export_schedules` — one row per tenant (`IsEnabled`, `IntervalHours`, `LastDeliveredAt`)
- `staffarr_person_export_delivery_runs` — audit of each scheduled export run (`ExportId`, `PersonCount`, filters used)

## API + auth changes

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/people/export/schedule` | `RequirePeopleWrite` |
| PUT | `/api/people/export/schedule` | `RequirePeopleWrite` |
| GET | `/api/internal/person-export-deliveries/pending` | Service token: source `shared-worker`, target `staffarr`, scope `staffarr.people.export.scheduled` |
| POST | `/api/internal/person-export-deliveries/process-batch` | Same |

Scheduled delivery uses tenant export preset filters when configured, runs `PeopleExportService.BuildExportAsync`, records delivery run + audit `person.export.scheduled_delivery`.

## shared-worker configuration

`StaffArrPersonExportDelivery` section — `Enabled`, `StaffArrBaseUrl`, `ServiceToken`, `ScanIntervalMinutes` (default 60), `BatchSize` (default 10), optional `TenantId`.

## Tests

- `PersonExportDeliveryRulesTests` — due interval + normalization
- `StaffArrPersonExportDeliveryWorkerTests` — auth, pending, deliver, skip recent, schedule GET/PUT
- `PersonExportPanel.test.tsx` — save schedule

## Verification commands

```powershell
dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~StaffArrPersonExportDelivery"
dotnet test tests/STLCompliance.Shared.Worker.Tests/STLCompliance.Shared.Worker.Tests.csproj -c Release --filter "FullyQualifiedName~PersonExportDeliveryRules"
cd apps/staffarr-frontend; npm test -- PersonExportPanel.test.tsx
```

## Next recommended slice

RoutArr staging trip mirror seed for dispatch gate k6 journeys, or StaffArr export delivery notification hooks.
