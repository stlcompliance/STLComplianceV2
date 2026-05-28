# Worker 166 — TrainArr integration settings (M6)

## Slice name

M6 integration settings — tenant cross-product integration toggles (StaffArr, Compliance Core, RoutArr), connectivity probes, server-side enforcement on inbound/outbound integration paths, TrainArr admin settings UI, integration and unit tests

## Products touched

- **TrainArr API** (`apps/trainarr-api`): `trainarr_tenant_integration_settings`, `IntegrationSettingsService`, `IntegrationProbeService`, `/api/integration-settings` endpoints, enforcement gates
- **TrainArr Frontend** (`apps/trainarr-frontend`): `IntegrationSettingsPanel` on settings workspace
- **Integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): `TrainArrIntegrationSettingsTests`
- **Unit tests** (`tests/STLCompliance.Shared.Worker.Tests`): `IntegrationSettingsRulesTests`

## Schema

Migration `TrainArrIntegrationSettings`:

- `trainarr_tenant_integration_settings` — per-tenant integration master toggles and feature-level switches (unique on `TenantId`)

| Column | Purpose |
|--------|---------|
| `StaffArrIntegrationEnabled` | Master StaffArr toggle |
| `StaffArrIncidentIntakeEnabled` | `/api/integrations/incident-remediations` |
| `StaffArrPublicationDeliveryEnabled` | StaffArr publication outbox enqueue |
| `ComplianceCoreIntegrationEnabled` | Master Compliance Core toggle |
| `ComplianceCoreQualificationChecksEnabled` | Compliance Core evaluate during qualification checks |
| `RoutarrIntegrationEnabled` | Master RoutArr toggle |
| `RoutarrQualificationDispatchEnabled` | `/api/integrations/routarr-qualification-check` |

Defaults when no row exists: all enabled.

## API + auth changes

### TrainArr user APIs (JWT + TrainArr entitlement)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/integration-settings` | `RequireIntegrationSettingsManage` |
| PUT | `/api/integration-settings` | Same |
| GET | `/api/integration-settings/probes` | Same — live `/health` probes for StaffArr + Compliance Core base URLs |

Successful upserts write `integration_settings.upsert` audit events.

## Permission keys

- JWT: `tenant_admin`, `trainarr_admin`, platform admin via `RequireIntegrationSettingsManage`

## Enforcement

- **StaffArr incident intake** — `StaffarrIncidentRemediationService.IngestAsync` rejects when disabled (403)
- **StaffArr publication delivery** — `StaffarrPublicationRetryService.EnqueueAndAttemptAsync` no-ops when disabled
- **Compliance Core qualification checks** — `QualificationCheckService.EvaluateComplianceCoreAsync` rejects when disabled (403)
- **RoutArr qualification dispatch** — integration endpoint rejects when disabled (403)

## Frontend changes

- **IntegrationSettingsPanel** on TrainArr settings — master/feature toggles per product, connectivity probe list, save to real APIs

## Worker / events

None (settings-only slice; no scheduled worker).

## Tests

### Backend integration (`TrainArrIntegrationSettingsTests`)

- `Integration_settings_defaults_when_missing`
- `Integration_settings_upsert_persists_and_writes_audit`
- `Incident_intake_rejects_when_disabled`
- `Integration_settings_denies_trainer`
- `Integration_probes_returns_items`

### Unit (`IntegrationSettingsRulesTests`)

- Default-enabled resolution when snapshot missing
- Master toggle disables child features
- Feature-level disable respected

### Frontend unit

- `IntegrationSettingsPanel.test.tsx`

## Verification commands

```powershell
dotnet build -c Release apps/trainarr-api/TrainArr.Api/TrainArr.Api.csproj
dotnet test tests/STLCompliance.Shared.Worker.Tests/STLCompliance.Shared.Worker.Tests.csproj -c Release --filter "FullyQualifiedName~IntegrationSettings"
dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~TrainArrIntegrationSettings"
cd apps/trainarr-frontend
npm run test -- --run IntegrationSettingsPanel
npm run build
```

## Remaining gaps

- RoutArr connectivity probe not included (no configured RoutArr client options in TrainArr API yet)
- Orphan reference / person lookup paths do not yet consult integration settings
- No webhook/notification when integrations are disabled and calls are rejected

## Next recommended slice

Per `02_PRODUCT_IMPLEMENTATION_BACKLOG.md`, next open items include other product M12 backlog rows (NexArr service-token cleanup worker, MaintainArr async audit package generation, etc.).
