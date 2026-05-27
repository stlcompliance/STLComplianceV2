# Worker 38 — TrainArr citation attachment

## Slice name

M10 citation attachment — local mirror of Compliance Core citation keys on training definitions/programs/assignments, attach/list/remove APIs with JWT auth, optional Compliance Core metadata enrichment via service token, trainarr-frontend citation panels on definition/program detail, cross-product tests

## Products touched

- **TrainArr API** (`apps/trainarr-api`): `trainarr_training_citation_attachments`, `TrainingCitationService`, nested `/api/{entity}/citations` routes
- **Compliance Core API** (`apps/compliancecore-api`): `POST /api/internal/citations/lookup` with `compliancecore.citations.read` service scope
- **TrainArr Frontend** (`apps/trainarr-frontend`): `CitationAttachmentPanel` on definition/program selection
- **Integration tests**: `StaffArrTrainArrCitationAttachmentTests`

## API + auth changes

### TrainArr user API (JWT + TrainArr entitlement)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/training-definitions/{id}/citations?includeMetadata=` | `RequireCitationsRead` (definition read) |
| POST | `/api/training-definitions/{id}/citations?validateWithComplianceCore=` | `RequireCitationsManage` (definition manage) |
| DELETE | `/api/training-definitions/{id}/citations/{attachmentId}` | `RequireCitationsManage` |
| GET | `/api/training-programs/{id}/citations` | program read |
| POST | `/api/training-programs/{id}/citations` | program manage |
| DELETE | `/api/training-programs/{id}/citations/{attachmentId}` | program manage |
| GET/POST/DELETE | `/api/training-assignments/{id}/citations` | assignment read / create scope |

Request body: `complianceCoreCitationId`, `citationKey`, optional `citationVersion`.

Response: opaque reference fields plus optional `metadata` (label, source reference, program/rule pack keys) when Compliance Core lookup succeeds.

Audit: `citation.attach` / `citation.detach` on entity type target.

### Compliance Core internal API (service token)

| Method | Route | Scope |
|--------|-------|-------|
| POST | `/api/internal/citations/lookup` | `compliancecore.citations.read` |

Body: `{ tenantId, citationIds[] }` (max 200 ids).

### Configuration

`ComplianceCore:ServiceToken` on TrainArr must include `compliancecore.citations.read` (in addition to evaluate scope) for metadata validation/enrichment.

## Frontend changes

- **ProgramBuilderPanel** — select definition/program for citation management
- **CitationAttachmentPanel** — attach by Compliance Core citation id + key, optional validate checkbox, list with metadata, remove

## Tests

### Cross-product (`StaffArrTrainArrCitationAttachmentTests`)

- `Training_definition_citation_attach_list_remove_with_metadata`
- `Training_program_citation_attachment_persists_reference_only`
- `Training_definition_citation_attach_denies_member_role`
- `Training_definition_citation_attach_rejects_duplicate`
- `Citation_attach_writes_audit_event`

### Frontend unit

- `CitationAttachmentPanel.test.tsx`

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~CitationAttachment"
cd apps/trainarr-frontend
npm install
npm run test -- --run
npm run build
```

## Remaining gaps

- Assignment-level citation UI not surfaced on assignment detail panel (API available)
- No automatic citation sync when Compliance Core supersedes a citation version
- Citation picker/search against Compliance Core catalog not in TrainArr UI (manual id/key entry)

## Next recommended slice

**Compliance Core batch workflow gate checks** (M5/M10) or **TrainArr rule-pack requirement intake** per milestone priority.
