# StaffArr export delivery notification hooks

## Slice name

M4/M12 StaffArr scheduled export delivery notification hooks — tenant webhook config, HTTPS dispatch on success/failure, delivery notification audit, UI + tests

## Products touched

- **StaffArr API** — schedule webhook fields, `PersonExportDeliveryNotificationService`, notification list API, delivery hook integration
- **StaffArr Frontend** — webhook + notify toggles and recent notification status in `PersonExportPanel`
- **Tests** — `StaffArrPersonExportDeliveryNotificationTests`, `PersonExportDeliveryNotificationRulesTests`

## Schema

### Migration `StaffArrPersonExportDeliveryNotifications`

- `staffarr_tenant_person_export_schedules` — `NotificationWebhookUrl`, `NotifyOnSuccess`, `NotifyOnFailure`
- `staffarr_person_export_delivery_notifications` — per-attempt audit (`sent` / `failed` / `skipped`)

## API + auth changes

| Method | Route | Auth |
|--------|-------|------|
| PUT | `/api/people/export/schedule` | `RequirePeopleWrite` — extended body with webhook + notify flags |
| GET | `/api/people/export/schedule` | `RequirePeopleWrite` — returns webhook + notify flags |
| GET | `/api/people/export/delivery-notifications` | `RequirePeopleWrite` — recent notification attempts |

Webhook POST payloads:

- `person.export.scheduled_delivery.success` — `{ tenantId, exportId, personCount, deliveredAt }`
- `person.export.scheduled_delivery.failure` — `{ tenantId, reason, attemptedAt }`

Scheduled delivery batch (`shared-worker`) triggers hooks after each success/failure.

## Permission keys

No new keys — uses existing `staffarr.people.write` for schedule/notification configuration.

## Worker / events

`PersonExportDeliveryService` invokes notification hooks after successful delivery and on per-tenant batch failures (failed delivery run + audit `person.export.scheduled_delivery.failed`).

## Tests

- `StaffArrPersonExportDeliveryNotificationTests` — webhook capture on success, invalid URL rejection, list API
- `PersonExportDeliveryNotificationRulesTests` — URL normalization
- `PersonExportPanel.test.tsx` — schedule save includes webhook fields

## Verification commands

```powershell
dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~PersonExportDeliveryNotification"
cd apps/staffarr-frontend; npm test -- PersonExportPanel.test.tsx
```

## Next recommended slice

k6 optional `STL_LOAD_JOURNEY_TRIP_ID` from RoutArr seed, or TrainArr notification settings foundations.
