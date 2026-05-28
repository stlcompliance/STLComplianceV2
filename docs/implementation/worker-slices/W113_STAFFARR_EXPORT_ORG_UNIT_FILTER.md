# StaffArr person export org-unit filter UI

## Slice name

M4 person export org-unit filter — wire existing `orgUnitId` API filter into `PersonExportPanel`, active org-unit dropdown, integration + frontend tests

## Products touched

- **StaffArr Frontend** (`apps/staffarr-frontend`): `PersonExportPanel` org-unit filter
- **Integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): `People_export_filters_by_org_unit`

## API + auth changes

No API changes — `GET /api/people/export?orgUnitId=` already filters by `PrimaryOrgUnitId` (Worker 111).

## UI changes

- Loads org units via `GET /api/org-units`
- Optional **Primary org unit filter** dropdown (active units only)
- Passes `orgUnitId` to CSV, JSON, and ZIP export calls (filters passed at mutate time to avoid stale closure)

## Tests

### Backend integration

- `People_export_filters_by_org_unit` — two sites, export scoped to one returns single person

### Frontend (`PersonExportPanel.test.tsx`)

- `renders export controls for writers` — includes org unit filter
- `passes org unit filter to JSON export` — verifies client call args

## Verification commands

```powershell
dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~StaffArrPersonExportTests"
cd apps/staffarr-frontend; npm test -- PersonExportPanel.test.tsx
```

## Next recommended slice

Scheduled weekly staging load soak in CI, or StaffArr export combined employment + org-unit filter presets.
