# Worker 164 — TrainArr orphan reference detection worker (M12)

## Slice name

M12 orphan reference detection worker — tenant scan settings, cross-product reference validation via StaffArr person lookup and Compliance Core citation/rule-pack lookups, materialized findings, shared-worker scheduled job, JWT admin settings UI, integration and frontend tests

## Products touched

- **TrainArr API** (`apps/trainarr-api`): settings/findings/run tables, `OrphanReferenceWorkerService`, `OrphanReferenceSettingsService`, `StaffArrPersonLookupClient`, internal + JWT endpoints
- **shared-worker** (`workers/shared-worker`): `TrainArrOrphanReferenceJob`, client, options
- **TrainArr Frontend** (`apps/trainarr-frontend`): `OrphanReferenceSettingsPanel`, Settings workspace wiring
- **STLCompliance.Shared**: integration token catalog updates (`staffarr.person.lookup` on trainarr-staffarr profile, worker token profile)

## Schema

Migration `TrainArrOrphanReferenceWorker`:

- `trainarr_tenant_orphan_reference_settings` — per-tenant enable flag, scan staleness hours (default 24)
- `trainarr_orphan_reference_findings` — materialized orphan references (StaffArr person, Compliance Core citation, Compliance Core rule pack)
- `trainarr_orphan_reference_runs` — worker outcome audit per tenant scan

## API + auth changes

### TrainArr JWT (trainarr admin)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/orphan-reference-settings` | `RequireOrphanReferenceSettingsManage` |
| PUT | `/api/orphan-reference-settings` | Same |
| GET | `/api/orphan-reference-settings/findings` | Same |
| GET | `/api/orphan-reference-settings/runs` | Same |

### TrainArr internal (shared-worker)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/internal/orphan-references/pending` | source `shared-worker`, scope `trainarr.orphan_references.scan` |
| POST | `/api/internal/orphan-references/process-batch` | Same |

`process-batch` body: optional `tenantId`, optional `asOfUtc`, `batchSize` (1–50, default 10), `stalenessHours` (1–168, default 24).

## Permission keys

- JWT: trainarr admin / tenant_admin via `RequireOrphanReferenceSettingsManage`
- Worker scope: `trainarr.orphan_references.scan`
- Cross-product validation uses existing TrainArr service tokens with `staffarr.person.lookup`, `compliancecore.citations.read`, `compliancecore.rulepacks.read`

## Worker behavior

`TrainArrOrphanReferenceJob` runs on a configurable interval (default 120 min), calls `POST /api/internal/orphan-references/process-batch`. For each enabled tenant whose last scan is stale, the service collects references from assignments, qualifications, publications, citations, rule-pack requirements, and related tables; validates via owner APIs; upserts active findings; resolves findings when references become valid again; records tenant run audit.

## Frontend changes

- **OrphanReferenceSettingsPanel** on TrainArr Settings workspace — enable toggle, staleness hours, active findings list, recent scan runs from real APIs

## Tests

### Backend integration (`StaffArrTrainArrOrphanReferenceWorkerTests`)

- Service token auth rejection
- Pending list before processing
- Process batch detects orphan StaffArr person and Compliance Core citation references

### Unit (`OrphanReferenceRulesTests`)

- Staleness boundary, batch/staleness normalization, reference key builders

### Frontend unit

- `OrphanReferenceSettingsPanel.test.tsx`

## Verification commands

```powershell
dotnet build -c Release
dotnet test tests/STLCompliance.Shared.Worker.Tests/STLCompliance.Shared.Worker.Tests.csproj -c Release --filter "FullyQualifiedName~OrphanReference"
dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~OrphanReferenceWorker"
cd apps/trainarr-frontend
npm run test -- --run OrphanReferenceSettingsPanel
```

## Remaining gaps

- No automatic remediation/cleanup of orphan references (detection-only V1)
- StaffArr person validation is per-ID HTTP lookup (no batch integration endpoint yet)
- No webhook/notification when new orphans are detected

## Next recommended slice

Per `02_PRODUCT_IMPLEMENTATION_BACKLOG.md`, next open **M12** items include TrainArr **training audit package** or other product M12 backlog rows.
