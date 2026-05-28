# StaffArr tenant person export preset persistence

## Slice name

M4 tenant person export preset persistence — per-tenant saved export filter defaults, GET/PUT APIs, audit, UI save/apply, integration + frontend tests

## Products touched

- **StaffArr API** (`apps/staffarr-api`): `TenantPersonExportPreset` entity, migration, `PersonExportPresetService`, `/api/people/export/preset`
- **StaffArr Frontend** (`apps/staffarr-frontend`): tenant default load/save/apply in `PersonExportPanel`, preset helper extensions
- **Integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): `StaffArrPersonExportPresetTests`

## API + auth changes

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/api/people/export/preset` | `RequirePeopleWrite` | Returns tenant saved preset or 404 when unset |
| PUT | `/api/people/export/preset` | `RequirePeopleWrite` | Upserts tenant preset (employment status, org unit, optional preset key) |

Validation:

- Employment status must be `active`, `inactive`, `terminated`, or omitted
- Org unit must exist and be active in tenant when provided
- Preset key must match known quick presets when provided
- `active-at-org-unit` preset requires org unit

Audit action: `person.export_preset.update`

## UI changes

- Loads tenant default on panel mount and applies filters when configured
- **Save tenant default** persists current filter state (infers preset key when it matches a quick preset)
- **Apply tenant default** re-applies saved filters
- Tenant default summary with last saved timestamp

## Tests

### Backend integration

- `StaffArrPersonExportPresetTests` — GET 404, PUT/GET round trip, validation, audit, auth denial

### Frontend

- `personExportFilterPresets.test.ts` — infer preset key + response-to-state helpers
- `PersonExportPanel.test.tsx` — load preset, save preset, existing export flows

## Verification commands

```powershell
dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~StaffArrPersonExportPresetTests"
cd apps/staffarr-frontend; npm test -- personExportFilterPresets.test.ts PersonExportPanel.test.tsx
```

## Next recommended slice

TrainArr staging qualification mirror seed for k6 journeys, or StaffArr export scheduled delivery foundations.
