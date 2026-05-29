# W257 — RoutArr M9 proof/DVIR capture depth (W217 extension)

Builds on **W217** (`TripProofRecord`, `TripDvirInspection`, `TripProofDvirService`, driver portal + dispatch read panels).

## Slice name

M9 trip proof/DVIR capture depth — tenant capture policy, readiness checklist, driver-portal gates, full DVIR form, pickup/delivery quick capture.

## Products touched

- **RoutArr API** (`apps/routarr-api`): migration, `TripExecutionCaptureRules`, `TripExecutionCaptureService`, settings + capture-readiness endpoints, `DriverPortalService` gates.
- **RoutArr Frontend** (`apps/routarr-frontend`): `TripExecutionSettingsPanel`, enhanced `DriverPortalPanel` capture UX.
- **Tests**: `TripExecutionCaptureRulesTests`, `RoutArrTripExecutionCaptureTests`, `TripExecutionSettingsPanel.test.tsx`, `DriverPortalPanel.test.tsx` (updated).

## Database

| Table | Purpose |
|-------|---------|
| `routarr_tenant_trip_execution_settings` | Per-tenant flags for required proof/DVIR and DVIR-fail blocks on start/complete |

Migration: `RoutArrTripExecutionCaptureDepth`.

Default policy when no row (code defaults): require pre-trip DVIR before start; block start on pre-trip fail; optional post-trip/delivery proof before complete.

## API (JWT)

### Trip execution settings (dispatcher/admin)

| Method | Route | Auth | Behavior |
|--------|-------|------|----------|
| `GET` | `/api/trip-execution-settings` | `RequireNotificationSettingsManage` | Read tenant capture policy |
| `PUT` | `/api/trip-execution-settings` | `RequireNotificationSettingsManage` | Upsert tenant capture policy |

### Driver portal capture readiness

| Method | Route | Auth | Behavior |
|--------|-------|------|----------|
| `GET` | `/api/driver-portal/trips/{tripId}/capture-readiness` | Assigned driver or dispatcher read | Checklist items + `canStartTrip` / `canCompleteTrip` |

### Driver portal gates (existing routes)

- `POST /api/driver-portal/trips/{tripId}/start` — `409` when capture policy not satisfied (`driver_portal.capture_not_ready`)
- `POST /api/driver-portal/trips/{tripId}/complete` — same when post-trip/delivery policy not satisfied

### DVIR validation (existing submit routes)

- Fail/conditional DVIR requires defect notes (min 3 chars) — `trip_dvir.defect_notes_required`

### Schedule enrichment

`GET /api/driver-portal/schedule` trip rows add `captureStartReady`, `captureCompleteReady`; `canStart` / `canComplete` respect readiness.

### Audit actions

- `trip_execution_settings.update`
- `trip_capture_readiness.read`

## Frontend

- **Settings** (`/settings`): `TripExecutionSettingsPanel` (`trip-execution-settings-panel`) — six policy toggles.
- **Driver portal**: full pre/post DVIR forms (result, odometer, defect notes), quick pickup/delivery proof buttons, capture-readiness blocker list, start/complete disabled when not ready.

## Tests

| Suite | Coverage |
|-------|----------|
| `TripExecutionCaptureRulesTests` | DVIR notes validation, readiness start blocks |
| `RoutArrTripExecutionCaptureTests` | Settings upsert, start blocked until pre-trip DVIR, readiness API, fail without notes |
| `RoutArrTripProofDvirTests` | Existing W217 flows (regression) |
| `TripExecutionSettingsPanel.test.tsx` | Panel render |
| `DriverPortalPanel.test.tsx` | Schedule + start with readiness mocks |

## Verification commands

```powershell
dotnet build apps/routarr-api/RoutArr.Api/RoutArr.Api.csproj -c Release
dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~TripExecutionCapture|FullyQualifiedName~TripProofDvir"
cd apps/routarr-frontend
npm run test -- --run DriverPortalPanel TripExecutionSettingsPanel
npm run build
```

## Out of scope

- Photo/document/signature attachments on proof (M9 matrix item deferred)
- Hard block on dispatch closeout apply (W251 remains checklist-only)
- M13 Playwright driver-portal capture smoke → **W259 complete**

## Next slice

- **M13 Playwright** — unassigned queue preview-before-assign depth smoke (W255 extension) → **W258 complete**
- **NexArr M12** — platform-admin service token / worker health orchestration UI
- **RoutArr M9** — photos/documents/signatures on proof capture (if scoped)
