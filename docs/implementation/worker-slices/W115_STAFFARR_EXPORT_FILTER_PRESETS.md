# StaffArr person export filter presets

## Slice name

M4 person export filter presets — quick HR export presets combining employment status and org-unit filters, preset helper module, integration + frontend tests

## Products touched

- **StaffArr Frontend** (`apps/staffarr-frontend`): `PersonExportPanel` preset buttons, `personExportFilterPresets` helper
- **Integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): combined employment + org-unit export filter

## API + auth changes

No API changes — `GET /api/people/export?employmentStatus=&orgUnitId=` already supported (Workers 111/113).

## UI changes

Quick filter presets:

| Preset | Filters applied |
|--------|-----------------|
| All people | Clears employment status and org unit |
| Active workforce | `employmentStatus=active` |
| Inactive records | `employmentStatus=inactive` |
| Terminated records | `employmentStatus=terminated` |
| Active at org unit | `employmentStatus=active` + selected `orgUnitId` (disabled until org unit chosen) |

Manual dropdowns remain for fine-tuning. Active filter summary shown below presets.

## Tests

### Backend integration

- `People_export_filters_by_employment_status_and_org_unit`

### Frontend

- `personExportFilterPresets.test.ts` — preset resolution helpers (6 tests)
- `PersonExportPanel.test.tsx` — preset buttons + combined export call

## Verification commands

```powershell
dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~StaffArrPersonExportTests"
cd apps/staffarr-frontend; npm test -- personExportFilterPresets.test.ts PersonExportPanel.test.tsx
```

## Next recommended slice

Compliance Core staging seeds for journey k6 scenarios, or StaffArr export preset persistence per tenant.
