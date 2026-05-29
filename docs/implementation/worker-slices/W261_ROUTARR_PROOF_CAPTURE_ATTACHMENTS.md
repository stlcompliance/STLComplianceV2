# W261 — RoutArr M9 proof photo/document/signature attachments (W257 extension)

Builds on **W257** (trip execution capture policy, readiness gates) and **W217** (proof/DVIR persistence).

## Slice name

M9 trip capture attachments — photo, document, and signature files on proof records and DVIR inspections, tenant attachment policy, driver upload + dispatcher download.

## Products touched

- **RoutArr API** (`apps/routarr-api`): migration, `TripCaptureAttachment`, storage service, attachment APIs, extended capture settings + readiness rules, execution summary nesting.
- **RoutArr Frontend** (`apps/routarr-frontend`): `TripCaptureAttachmentPanel`, enhanced `DriverPortalPanel`, `TripProofDvirReadPanel`, `TripExecutionSettingsPanel` attachment toggles.
- **Tests**: `RoutArrTripCaptureAttachmentTests`, extended `TripExecutionCaptureRulesTests` / capture settings tests, Vitest for attachment panel + updated panel mocks.

## Database

| Table | Purpose |
|-------|---------|
| `routarr_trip_capture_attachments` | File metadata + storage key linked to proof or DVIR subject within trip |
| `routarr_tenant_trip_execution_settings` | +5 attachment requirement flags |

Migration: `RoutArrTripCaptureAttachments`.

Attachment kinds: `photo`, `document`, `signature`. Subject types: `proof`, `dvir` (subject id references existing RoutArr rows; no cross-DB FKs).

File storage: `CaptureAttachmentStorage:RootPath` (default `data/routarr-capture-attachments`).

## API (JWT)

### Trip / driver-portal attachment routes (assigned driver write; dispatcher read)

| Method | Route | Auth | Behavior |
|--------|-------|------|----------|
| `GET` | `/api/trips/{tripId}/proofs/{proofId}/attachments` | proof read | List attachments for proof |
| `POST` | `/api/trips/{tripId}/proofs/{proofId}/attachments` | proof write | Upload base64 attachment |
| `GET` | `/api/trips/{tripId}/proofs/{proofId}/attachments/{attachmentId}/content` | proof read | Download file bytes |
| `GET/POST/GET content` | `/api/trips/{tripId}/dvir/{dvirId}/attachments[...]` | dvir read/write | Same for DVIR subjects |
| Mirror routes | `/api/driver-portal/trips/{tripId}/...` | driver portal | Same semantics for assigned driver |

Request body (`UploadTripCaptureAttachmentRequest`): `attachmentKind`, `fileName`, `contentType`, `contentBase64`, optional `notes`.

### Extended trip execution settings

Five new booleans on `GET/PUT /api/trip-execution-settings`:

- `requirePickupProofPhotoBeforeStart`
- `requirePreTripDvirPhotoBeforeStart`
- `requireDeliveryProofPhotoBeforeComplete`
- `requireDeliverySignatureBeforeComplete`
- `requirePostTripDvirPhotoBeforeComplete`

### Capture readiness enrichment

Readiness checklist adds keys: `pickup_proof_photo`, `pre_trip_dvir_photo`, `delivery_proof_photo`, `delivery_signature`, `post_trip_dvir_photo` when corresponding policy flags are enabled and parent proof/DVIR exists.

### Execution summary

`TripProofRecordResponse` and `TripDvirInspectionResponse` now include nested `attachments[]`.

### Audit actions

- `trip_capture_attachment.upload`
- `trip_capture_attachment.list`
- `trip_capture_attachment.download`

## Frontend

- **Driver portal**: per proof/DVIR `TripCaptureAttachmentPanel` — photo/document file pickers + canvas signature pad (`signature-pad` test id).
- **Dispatch read**: `TripProofDvirReadPanel` download buttons per attachment (authenticated blob download).
- **Settings** (`/settings`): five attachment requirement toggles under “Attachment requirements”.

## Tests

| Suite | Coverage |
|-------|----------|
| `RoutArrTripCaptureAttachmentTests` | Upload photo, download, readiness satisfied, start blocked without photo |
| `TripExecutionCaptureRulesTests` | Pickup photo readiness blocker |
| `RoutArrTripExecutionCaptureTests` | Settings upsert with attachment flags |
| `RoutArrTripProofDvirTests` | Regression |
| `TripCaptureAttachmentPanel.test.tsx` | Panel render |
| Updated panel Vitest mocks | `attachments: []` on proof/DVIR DTOs |

## Verification commands

```powershell
dotnet build apps/routarr-api/RoutArr.Api/RoutArr.Api.csproj -c Release
dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~TripCaptureAttachment|FullyQualifiedName~TripExecutionCapture|FullyQualifiedName~TripProofDvir"
cd apps/routarr-frontend
npm run test -- --run TripCaptureAttachmentPanel TripExecutionSettingsPanel DriverPortalPanel TripProofDvirReadPanel
npm run build
```

## Out of scope

- ~~M13 Playwright attachment upload smoke~~ → **W264 complete**
- Attachment retention/purge worker
- OCR / image validation beyond size/type checks

## Next slice

- ~~**M13 Playwright** — RoutArr dispatch proof/DVIR read attachment download smoke (dispatcher download path; builds on W261/W248)~~ → **W265 complete**
- ~~**M13 Playwright** — RoutArr driver-portal attachment upload smoke (photo/signature path; builds on W261)~~ → **W264 complete**
