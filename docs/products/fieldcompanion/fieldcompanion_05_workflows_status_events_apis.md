# Field Companion — Workflows, Status Logic, Events, and APIs

## Major workflow: authenticated mobile task execution

```text
1. User signs in through NexArr.
2. Field Companion opens MobileSession.
3. Field Companion loads StaffArr person/readiness/permission context.
4. Field Companion loads product surfaces based on assigned work, relevance, and permissions.
5. Field Companion loads assigned MobileTasks.
6. User opens task.
7. Field Companion renders MobileActionSchema.
8. User completes fields and evidence capture.
9. Field Companion submits MobileActionSubmission to source product.
10. Source product validates action.
11. Source product accepts, rejects, blocks, or returns conflict.
12. Field Companion displays result.
13. Events are published for reporting/audit.
```

## Major workflow: no-login secure upload

```text
1. Source product requests secure upload session.
2. Field Companion/RecordArr creates scoped token.
3. QR code or link is shown.
4. External user opens link.
5. Only the specific upload/signature/capture action is shown.
6. User uploads or captures evidence.
7. RecordArr stores file/record.
8. Source product receives record reference.
9. Secure upload session expires or is marked used.
```

## Major workflow: offline work order task

```text
1. Technician opens assigned MaintainArr work order task while online.
2. Task schema and limited object context are cached.
3. Technician loses signal.
4. Technician completes task, captures photos, records labor notes.
5. Field Companion creates OfflineAction.
6. Connection returns.
7. Evidence uploads to RecordArr.
8. Action submits to MaintainArr.
9. MaintainArr accepts or returns conflict.
10. Field Companion marks synced or displays conflict.
```

## Major workflow: route stop proof

```text
1. Driver opens RoutArr trip.
2. Driver arrives at stop.
3. Field Companion records arrive action.
4. Driver captures signature/photos/POD/BOL as required.
5. RecordArr stores evidence.
6. RoutArr completes stop.
7. OrdArr receives fulfillment update if tied to order.
8. CustomArr receives customer activity if applicable.
```

## Major workflow: receiving document capture

```text
1. LoadArr receiving task requires BOL or packing slip.
2. Receiver displays QR to driver or captures document directly.
3. Field Companion performs document scan.
4. RecordArr generates PDF/OCR record.
5. LoadArr links document to Receipt.
6. Discrepancy may trigger AssurArr workflow.
```

## Major workflow: training signoff

```text
1. TrainArr assignment includes step requiring signoff.
2. Field Companion shows trainee/trainer/evaluator action.
3. Required signature/evidence is captured.
4. TrainArr validates role and assignment state.
5. Step is marked complete or failed.
6. Qualification progress updates.
```

## Major workflow: incident report

```text
1. Worker opens Report Incident.
2. Field Companion shows controlled incident fields.
3. User captures photos/voice notes/location if permitted.
4. StaffArr receives incident.
5. RecordArr stores evidence.
6. StaffArr routes to TrainArr/AssurArr/MaintainArr/etc. if needed.
```

## Field Companion emitted events

```text
fieldcompanion.mobile_session.started
fieldcompanion.mobile_session.ended
fieldcompanion.product_surface.opened

fieldcompanion.mobile_task.created
fieldcompanion.mobile_task.assigned
fieldcompanion.mobile_task.viewed
fieldcompanion.mobile_task.accepted
fieldcompanion.mobile_task.started
fieldcompanion.mobile_task.submitted
fieldcompanion.mobile_task.synced
fieldcompanion.mobile_task.failed_sync
fieldcompanion.mobile_task.completed
fieldcompanion.mobile_task.expired

fieldcompanion.action.submitted
fieldcompanion.action.accepted
fieldcompanion.action.rejected
fieldcompanion.action.conflict

fieldcompanion.secure_upload.created
fieldcompanion.secure_upload.opened
fieldcompanion.secure_upload.completed
fieldcompanion.secure_upload.expired
fieldcompanion.secure_upload.revoked

fieldcompanion.capture.photo_captured
fieldcompanion.capture.signature_captured
fieldcompanion.capture.document_scanned
fieldcompanion.capture.voice_note_captured
fieldcompanion.capture.uploaded_to_recordarr

fieldcompanion.offline_action.created
fieldcompanion.offline_action.synced
fieldcompanion.offline_action.failed
fieldcompanion.offline_action.conflict

fieldcompanion.device.registered
fieldcompanion.device.revoked
fieldcompanion.notification.sent
```

## APIs Field Companion should expose

```text
GET /api/v1/mobile/me
GET /api/v1/mobile/session
POST /api/v1/mobile/session/start
POST /api/v1/mobile/session/end

GET /api/v1/mobile/product-surfaces
GET /api/v1/mobile/tasks
GET /api/v1/mobile/tasks/{mobileTaskId}
POST /api/v1/mobile/tasks/{mobileTaskId}/accept
POST /api/v1/mobile/tasks/{mobileTaskId}/start

GET /api/v1/mobile/action-schemas/{sourceProduct}/{actionKey}
POST /api/v1/mobile/actions
POST /api/v1/mobile/actions/{submissionId}/retry

POST /api/v1/mobile/offline-actions
POST /api/v1/mobile/offline-actions/sync
GET /api/v1/mobile/offline-actions/status

POST /api/v1/mobile/upload-sessions
GET /api/v1/mobile/upload-sessions/{token}
POST /api/v1/mobile/upload-sessions/{token}/complete
POST /api/v1/mobile/upload-sessions/{token}/revoke

POST /api/v1/mobile/captures/photos
POST /api/v1/mobile/captures/signatures
POST /api/v1/mobile/captures/document-scans
POST /api/v1/mobile/captures/voice-notes

POST /api/v1/mobile/scans
POST /api/v1/mobile/devices/register
POST /api/v1/mobile/devices/{deviceId}/revoke
GET /api/v1/mobile/notifications
POST /api/v1/mobile/notifications/{notificationId}/read
```

## APIs Field Companion should consume

```text
NexArr
- POST /api/v1/platform/handoff/redeem
- GET /platform/me
- GET /api/v1/platform/session/context
- POST /api/v1/platform/service-tokens/introspect

StaffArr
- GET /persons/{personId}
- GET /persons/{personId}/readiness
- GET /persons/{personId}/permissions
- GET /locations/{locationId}
- POST /incidents
- POST /permission-checks

TrainArr
- GET /mobile/assignments
- GET /mobile/assignments/{assignmentId}
- POST /mobile/assignments/{assignmentId}/steps/{stepId}/complete
- POST /mobile/signoffs

MaintainArr
- GET /mobile/work-orders
- GET /mobile/work-orders/{workOrderId}
- POST /mobile/work-orders/{workOrderId}/actions
- GET /mobile/inspections
- POST /mobile/inspections/{inspectionId}/answers
- POST /mobile/defects
- POST /mobile/meter-readings

LoadArr
- GET /mobile/receiving-tasks
- POST /mobile/receiving-tasks/{taskId}/actions
- GET /mobile/pick-tasks
- POST /mobile/pick-tasks/{taskId}/actions
- GET /mobile/count-tasks
- POST /mobile/count-tasks/{taskId}/counts

RoutArr
- GET /mobile/trips
- POST /mobile/trips/{tripId}/actions
- POST /mobile/stops/{stopId}/arrive
- POST /mobile/stops/{stopId}/depart
- POST /mobile/proof-events
- POST /mobile/exceptions

RecordArr
- POST /upload-sessions
- POST /records
- POST /document-scans
- GET /records/{recordId}

AssurArr
- GET /mobile/containment-actions
- POST /mobile/containment-actions/{actionId}/complete
- GET /mobile/capa-actions
- POST /mobile/capa-actions/{actionId}/complete
- POST /mobile/nonconformances

OrdArr
- GET /mobile/order-tasks
- POST /mobile/order-tasks/{taskId}/complete

CustomArr
- GET /mobile/customer-locations/{locationId}
- POST /mobile/customer-issues
```

## Permission examples

```text
fieldcompanion.mobile.use
fieldcompanion.mobile.offline_use
fieldcompanion.mobile.scan
fieldcompanion.mobile.capture_photo
fieldcompanion.mobile.capture_signature
fieldcompanion.mobile.capture_document
fieldcompanion.mobile.secure_upload.create
fieldcompanion.mobile.secure_upload.revoke
fieldcompanion.mobile.device.manage
fieldcompanion.mobile.sync_conflicts.resolve
fieldcompanion.mobile.admin
```

Most product actions should still require source-product permissions, such as:

```text
maintainarr.work_orders.execute
trainarr.trainer.signoff
routarr.trips.execute
loadarr.pick.execute
assurarr.containment.complete
staffarr.incidents.create
```

## Default role examples

```text
Mobile User
- Use Field Companion.
- View assigned tasks.
- Submit allowed source-product actions.

Mobile Offline User
- Mobile User permissions.
- Offline queue use where product policy allows.

Mobile Supervisor
- View team/direct-report tasks where allowed.
- Resolve selected sync conflicts.
- Approve selected mobile actions where product permissions allow.

External Upload User
- No login.
- Only scoped secure upload/signature action.

Field Companion Admin
- Manage mobile settings, device trust, secure upload configuration, and action schema availability.
```

## Field Companion UI surfaces

```text
/app/field-companion
- My Work
- Scan
- Product Switcher
- Offline Queue
- Notifications
- Profile / Readiness
- Secure Upload
- Incident Report
- Settings

Mobile surfaces by product:
- MaintainArr
- TrainArr
- RoutArr
- LoadArr
- StaffArr
- AssurArr
- RecordArr
- OrdArr
- CustomArr
- SupplyArr
```

## My Work UI

```text
MyWorkPage
- urgent tasks
- overdue tasks
- today
- assigned work orders
- assigned route stops
- assigned training
- assigned warehouse tasks
- approvals
- evidence needed
- blocked tasks
- offline/sync warning
```

## Task detail UI

```text
TaskDetailPage
- source object header
- task title
- instructions
- due date
- priority
- blockers
- warnings
- required evidence
- action fields
- capture buttons
- submit button
- offline availability indicator
- sync status
```

## Secure upload UI

```text
SecureUploadPage
- upload purpose
- source display snapshot
- expiration warning
- capture/upload controls
- retake option
- manual crop if document scan
- submit button
- completion confirmation
```

## Offline queue UI

```text
OfflineQueuePage
- queued actions
- uploading actions
- failed actions
- conflicts
- retry controls
- discard controls where allowed
- plain-language reason
```

## Mobile design rules

```text
1. Big buttons.
2. Minimal typing.
3. Controlled fields wherever possible.
4. Camera/scan first where useful.
5. Plain language blockers.
6. Clear sync status.
7. Clear source object identity.
8. Avoid raw product jargon for frontline users.
9. Still preserve product ownership behind the scenes.
10. Never let no-login links expose broader data.
```
