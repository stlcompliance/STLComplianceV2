# Worker 31 — TrainArr qualification suspend / revoke / expire

## Slice name

M6 qualification lifecycle — TrainArr suspend/revoke/expire on issued qualifications, StaffArr certification lifecycle ingest, training blocker re-publish on suspend, cross-product service token calls, frontends, integration tests

## Products touched

- **TrainArr API** (`apps/trainarr-api`): lifecycle columns on `trainarr_qualification_issues`, `QualificationIssueService` suspend/revoke/expire, `CertificationPublicationService` lifecycle publications, `StaffArrCertificationLifecycleClient`, `/api/qualification-issues/*` endpoints, audit events
- **StaffArr API** (`apps/staffarr-api`): `POST /api/integrations/certification-lifecycle`, `CertificationLifecycleIngestionService`, `LastExternalLifecyclePublicationId` on person certifications for idempotency
- **TrainArr Frontend** (`apps/trainarr-frontend`): lifecycle status display + suspend/revoke/expire actions on assignment detail
- **StaffArr Frontend** (`apps/staffarr-frontend`): TrainArr lifecycle status label on certification panel
- **StaffArr integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): `StaffArrTrainArrQualificationLifecycleTests`

## Schema

### TrainArr migration `TrainArrQualificationLifecycle`

- `trainarr_qualification_issues` additions:
  - `StatusChangedAt`, `LifecycleReason`, `LifecyclePublicationId`
  - unique filtered index on `(tenant_id, lifecycle_publication_id)`

### StaffArr migration `StaffArrCertificationLifecycleIngest`

- `staffarr_person_certifications.LastExternalLifecyclePublicationId` with unique filtered index for idempotent lifecycle replay

## API + auth changes

### TrainArr user APIs (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/qualification-issues/{id}` | `trainarr_admin` / `tenant_admin` |
| POST | `/api/qualification-issues/{id}/suspend` | `trainarr_admin` / `tenant_admin` |
| POST | `/api/qualification-issues/{id}/revoke` | `trainarr_admin` / `tenant_admin` |
| POST | `/api/qualification-issues/{id}/expire` | `trainarr_admin` / `tenant_admin` |

Status transitions: `issued`/`suspended` → `suspended`/`revoked`/`expired`; terminal statuses reject further changes (409).

Each action publishes a lifecycle certification publication and calls StaffArr via service token.

### StaffArr integration (service token)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/integrations/certification-lifecycle` | NexArr service token: source `trainarr`, allowed `staffarr`, scope `staffarr.certification_lifecycle.write`, tenant scope |

Lifecycle ingest behavior:

- **suspend** — re-publishes TrainArr training blocker (`suspended`) via existing blocker ingest; certification remains active
- **revoke** — sets linked `PersonCertification` (by grant publication id) to `revoked`
- **expire** — sets linked certification to `expired`

Idempotent by `TrainarrLifecyclePublicationId`.

## Frontend changes

- **TrainArr HomePage** — qualification status badge (issued/suspended/revoked/expired), lifecycle reason/publication ids, admin suspend/revoke/expire controls
- **StaffArr CertificationPanel** — `TrainArr lifecycle: Revoked/Expired` label for non-active TrainArr publication grants

## Tests

### Backend integration (`StaffArrTrainArrQualificationLifecycleTests`)

- `Qualification_suspend_publishes_staffarr_training_blocker`
- `Qualification_revoke_updates_staffarr_certification_status`
- `Qualification_expire_updates_staffarr_certification_status`
- `Qualification_lifecycle_rejects_terminal_status_transition`
- `Certification_lifecycle_ingest_rejects_missing_service_token`
- `Certification_lifecycle_ingest_is_idempotent_by_lifecycle_publication_id`

### Frontend unit

- `CertificationPanel.test.tsx` — TrainArr lifecycle revoked status rendering

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~TrainArr"
cd apps/trainarr-frontend
npm run test -- --run
npm run build
cd ../staffarr-frontend
npm run test -- --run
npm run build
```

## Remaining gaps

- Qualification reinstate / unsuspend workflow not implemented
- No NexArr service-token registry revocation check on product APIs yet
- TrainArr dedicated auth test project not added (covered via StaffArr cross-product tests)

## Next recommended slice

**Compliance Core fact source registry + internal resolve API** (M5) or **TrainArr qualification authorization check API** (M6) per milestone priority.
