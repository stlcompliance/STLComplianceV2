# Worker 20 — StaffArr incident routing to TrainArr

## Slice name

M4 workforce spine — `training_compliance` incident forward to TrainArr remediation intake, local routing mirror, service-token integration, authorized UI, audit on both products

## Products touched

- **TrainArr API** (`apps/trainarr-api`): `trainarr_staffarr_incident_remediations`, `trainarr_audit_events`, `POST /api/integrations/incident-remediations`
- **StaffArr API** (`apps/staffarr-api`): `staffarr_incident_trainarr_routings`, `POST /api/incidents/{incidentId}/route-to-trainarr`, `TrainArrIncidentRemediationClient`
- **StaffArr Frontend** (`apps/staffarr-frontend`): route button/status on IncidentsPanel for eligible incidents
- **StaffArr integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): cross-product routing tests

## Schema

### StaffArr migration `StaffArrIncidentTrainarrRouting`

- `staffarr_incident_trainarr_routings` — tenant-scoped mirror of TrainArr remediation reference (local FK to `staffarr_personnel_incidents` only; `trainarr_remediation_id` is opaque external id)
  - `routing_status` (`routed`), `routed_at`, `routed_by_user_id`
  - unique per `(tenant_id, incident_id)` and `(tenant_id, trainarr_remediation_id)`

### TrainArr migration `TrainArrStaffarrIncidentRemediation`

- `trainarr_staffarr_incident_remediations` — TrainArr-owned remediation intake referencing `staffarr_incident_id` and `staffarr_person_id` (opaque GUIDs, no cross-DB FK)
- `trainarr_audit_events` — product audit trail (first TrainArr audit table)

## API + auth changes

### TrainArr integration (service token)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/integrations/incident-remediations` | NexArr service token: source `staffarr`, allowed `trainarr`, scope `trainarr.incident_remediations.write`, tenant scope |

Idempotent on `(tenant_id, staffarr_incident_id)`.

### StaffArr user API

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/incidents/{incidentId}/route-to-trainarr` | `staffarr.incidents.manage` (same roles as intake) |

Eligible `reason_category_key`: `training_compliance` only.

Incident list/detail responses include optional `trainarrRouting` (`routingStatus`, `trainarrRemediationId`, `routedAt`, `routedByUserId`).

StaffArr calls TrainArr via configured `TrainArr:BaseUrl` + `TrainArr:ServiceToken`.

## Audit events

| Product | Action | Target |
|---------|--------|--------|
| StaffArr | `incident.route_trainarr` | `personnel_incident` |
| TrainArr | `incident_remediation.intake` | `staffarr_incident_remediation` |

## Frontend changes

- **Incidents panel** shows “routed to TrainArr” in list when `trainarrRouting` is present
- Detail shows routing metadata and **Route to TrainArr for remediation** for `training_compliance` incidents without routing (authorized roles only)
- `routePersonnelIncidentToTrainarr` API client + mutation on HomePage

## Tests

### Backend integration (`StaffArrTrainArrIncidentRoutingTests`)

- `Training_compliance_incident_routes_to_trainarr_with_mirror_and_audit`
- `Safety_incident_route_to_trainarr_rejected`
- `Incident_remediation_ingest_rejects_missing_service_token`
- `Incident_route_to_trainarr_is_idempotent`

### Frontend unit

- `IncidentsPanel.test.tsx` — route button for training compliance incidents

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

- TrainArr remediation workflow beyond intake (assignments, completion, publication back) remains M6
- Compliance Core incident evaluation trigger not wired (Workflow 6 step 3)
- Incident close/update and non-`training_compliance` routing policies remain future slices
- Service token validation is cryptographic/claims-based only (no NexArr registry revocation check on product APIs yet)

## Next recommended slice

StaffArr person timeline foundations (M4) or TrainArr training assignment engine (M6) per backlog priority.
