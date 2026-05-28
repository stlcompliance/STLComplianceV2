# Worker 154 — StaffArr personnel notes + documents foundations (M4)

## Slice name

M4 workforce spine — `staffarr.notes.manage` / `staffarr.documents.manage`, personnel note persistence with visibility controls, personnel document metadata + local file storage, person-scoped APIs, person timeline integration, authorized StaffArr UI, integration and frontend tests

## Products touched

- **StaffArr API** (`apps/staffarr-api`): `staffarr_personnel_notes`, `staffarr_personnel_documents`, note/document services, storage service, `/api/people/{personId}/notes` and `/api/people/{personId}/documents` endpoints, audit events, timeline aggregation
- **StaffArr Frontend** (`apps/staffarr-frontend`): `PersonnelNotesPanel`, `PersonnelDocumentsPanel` on People workspace, API client/types, workspace state wiring
- **StaffArr integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): note CRUD/auth/visibility, document upload/download/auth, timeline aggregation

## Schema

Migration `StaffArrPersonnelNotesDocumentsFoundations`:

- `staffarr_personnel_notes` — tenant-scoped notes with `personId`, category, visibility, subject, body, status, author/timestamps
- `staffarr_personnel_documents` — tenant-scoped document metadata with `personId`, type, title, file metadata, `storageKey`, optional expiration, uploader/timestamps

## API + auth changes

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/people/{personId}/notes` | read scope; visibility-filtered results |
| POST | `/api/people/{personId}/notes` | `staffarr.notes.manage` |
| GET | `/api/people/{personId}/notes/{noteId}` | read scope + note visibility |
| GET | `/api/people/{personId}/documents` | read scope (people.read roles or self) |
| POST | `/api/people/{personId}/documents` | `staffarr.documents.manage` |
| GET | `/api/people/{personId}/documents/{documentId}` | read scope |
| GET | `/api/people/{personId}/documents/{documentId}/content` | read scope; file download |

### Note categories

`general`, `performance`, `coaching`, `disciplinary`, `medical`, `other`

### Note visibility

`hr_only`, `management`, `personnel_visible`

### Document types

`id_verification`, `employment_contract`, `certification_copy`, `medical_form`, `policy_acknowledgment`, `other`

## Permission keys

- `staffarr.notes.manage` — HR roles create notes
- `staffarr.documents.manage` — HR roles upload documents
- Read aligned with people read + self; note visibility enforced server-side

## Frontend changes

- **Personnel notes panel** on People workspace — list, detail, intake with category/visibility
- **Personnel documents panel** — list, detail, authenticated download, file upload form
- Person timeline shows `personnel_note_created` and `personnel_document_uploaded` events

## Tests

### Backend integration

- `Personnel_note_create_list_and_detail_with_visibility_filtering`
- `Personnel_note_hr_only_hidden_from_tenant_member_self`
- `Personnel_note_create_denies_supervisor_role`
- `Personnel_document_upload_list_download_and_timeline`
- `Personnel_document_upload_denies_supervisor_role`

### Frontend unit

- `PersonnelNotesPanel.test.tsx`
- `PersonnelDocumentsPanel.test.tsx`

## Remaining gaps

- Note edit/archive workflows not implemented
- Document versioning and expiration worker not implemented
- Audit package export does not yet include note/document payloads
- No dedicated personnel history rollup worker beyond timeline aggregation

## Next recommended slice

M4 product-facing person lookup API or M12 personnel history rollup worker per backlog priority; alternatively next open M12 worker backlog row from `00_SLICE_STATE.md`.
