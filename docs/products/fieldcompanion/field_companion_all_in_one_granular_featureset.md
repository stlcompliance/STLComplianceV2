# Field Companion — Scope, Ownership, and Boundaries

## Product purpose

Field Companion is the mobile human execution layer for the STL Compliance / ARR suite. It gives workers, drivers, technicians, receivers, trainers, supervisors, vendors, customers, and temporary external users a simple way to perform permitted actions against the correct product APIs.

Field Companion answers:

- What do I need to do?
- What am I allowed to do?
- What is blocked?
- Why is it blocked?
- What can I complete from my phone?
- What evidence do I need to capture?
- What product owns this action?
- Can this be done offline?
- Did my action sync successfully?

Field Companion is intentionally not a source-of-truth business product. It is a controlled mobile interface that routes actions to source products.

## Field Companion owns

```text
- Mobile task inbox presentation
- Product switcher presentation
- My work view
- Mobile session context
- Device profile
- Secure upload session presentation
- QR/barcode scan UX
- Photo capture UX
- Signature capture UX
- Voice note capture UX
- Document scan UX
- Offline action queue
- Offline sync status
- Conflict presentation
- Mobile action schema rendering
- Field-friendly error messages
- Push notification presentation
- Worker-first task grouping
```

## Field Companion does not own

```text
- Platform login
- Tenant entitlement
- Person master
- Permission assignment truth
- Training assignment truth
- Qualification truth
- Asset truth
- Work order truth
- Defect truth
- Inspection truth
- Inventory balance
- Stock ledger
- Receiving truth
- Procurement truth
- Route/trip truth
- Customer truth
- Order lifecycle truth
- Document/file storage truth
- Quality hold/release truth
- Regulatory meaning
- Reporting read models
- Accounting execution
```

## External product dependencies

```text
NexArr
- Login/handoff
- Tenant/product entitlement
- Product switcher availability
- Session security

StaffArr
- Person context
- Permission context
- Readiness
- Incident reporting
- Person/location references

TrainArr
- Training assignments
- Training steps
- Trainee acknowledgement
- Trainer signoff
- Evaluator signoff
- Qualification status

Compliance Core
- Field-facing compliance prompts
- Evidence requirements
- Controlled situation/evidence fields
- Compliance warnings when products expose them

MaintainArr
- Work orders
- Work order tasks
- Inspections
- Defects
- Meter readings
- Labor entries
- Part requests
- Part usage
- Asset status

LoadArr
- Receiving tasks
- Putaway tasks
- Pick tasks
- Issue tasks
- Transfer tasks
- Count tasks
- Barcode scan validation
- Discrepancy reporting

SupplyArr
- Supplier document upload
- Purchase receiving context where delegated
- Supplier-facing upload links if allowed

RoutArr
- Trips
- Stops
- Arrive/depart actions
- Proof of pickup
- Proof of delivery
- Route exceptions
- BOL/POD capture

CustomArr
- Customer contact/location context
- Customer-facing secure upload or signature flows where allowed
- Customer issue intake where allowed

OrdArr
- Order task context
- Fulfillment task presentation
- Order completion evidence capture

RecordArr
- Actual file/document/photo/signature storage
- Secure upload sessions
- OCR/scanning/PDF processing
- Evidence package references

AssurArr
- Nonconformance evidence capture
- Containment task completion
- CAPA action completion
- Quality audit checklist execution
- Hold/release evidence capture

ReportArr
- Mobile activity reporting facts
- Operational task completion metrics
```

## Core source-of-truth rules

```text
1. Field Companion owns mobile UX, not business truth.
2. Every business action must route to the owning product API.
3. Offline actions are pending until the owning product accepts them.
4. Field Companion may cache task/action schemas but cannot become authoritative.
5. Field Companion may show status snapshots but must refresh from source products.
6. No-login secure links must be narrow, scoped, expiring, and auditable.
7. RecordArr owns uploaded file records.
8. StaffArr owns person identity and permissions.
9. NexArr owns login and product entitlement.
10. Product APIs own validation and final acceptance of actions.
```

## Standard Field Companion object envelope

```text
FieldCompanionObject
- id
- tenantId
- objectNumber
- objectType
- status
- title
- summary
- personId
- deviceId
- sourceProduct
- sourceObjectRef
- actionType
- createdAt
- createdByPersonId
- updatedAt
- expiresAt
- syncedAt
- auditTrail
- eventLog
```

## Object prefixes

```text
MTASK  Mobile task
MSESS  Mobile session
DEV    Device profile
ACT    Mobile action
OFF    Offline action
SYNC   Sync batch
UPL    Secure upload session
CAP    Capture artifact
SCAN   Scan event
PUSH   Push notification
CONF   Conflict
FORM   Mobile form schema
VIEW   Mobile view definition
```

## Standard source action reference

```text
SourceActionRef
- sourceProduct
- sourceObjectType
- sourceObjectId
- sourceObjectNumber
- actionKey
- actionLabel
- statusSnapshot
- versionSnapshot
- lastResolvedAt
```

## Standard mobile display principle

Field Companion should show:

```text
- what the user needs to do
- what object it is for
- why it matters
- what evidence is required
- what is blocking it
- what happens after submission
- whether it is synced
```

Field Companion should avoid:

```text
- raw JSON
- admin-heavy screens
- unclear product boundaries
- hidden sync failures
- freetyped compliance answers where controlled options are possible
- broad no-login access
```


---


# Field Companion — Mobile Task and Session Model

## Mobile task

A MobileTask is a presentation wrapper around a source product task/action. The source product owns the actual task. Field Companion owns how it appears and how the action is collected.

```text
MobileTask
- mobileTaskId
- tenantId
- mobileTaskNumber
- personId
- sourceProduct
  - staffarr
  - trainarr
  - maintainarr
  - loadarr
  - supplyarr
  - routarr
  - customarr
  - ordarr
  - recordarr
  - assurarr
  - compliancecore
- sourceObjectRef
- sourceTaskRef
- taskType
  - work_order_task
  - inspection
  - defect_report
  - meter_reading
  - labor_entry
  - training_step
  - trainer_signoff
  - evaluator_signoff
  - route_stop
  - proof_of_delivery
  - proof_of_pickup
  - receiving
  - putaway
  - pick
  - issue
  - transfer
  - count
  - incident_report
  - document_upload
  - quality_containment
  - capa_action
  - audit_checklist
  - approval
  - signature
  - custom_form
- title
- summary
- instructions
- priority
  - low
  - normal
  - high
  - urgent
  - emergency
- status
  - available
  - assigned
  - accepted
  - in_progress
  - blocked
  - submitted
  - synced
  - failed_sync
  - completed
  - canceled
  - expired
- dueAt
- availableFrom
- expiresAt
- staffarrSiteId
- staffarrLocationId
- objectDisplaySnapshot
- requiredCapabilities
- requiredPermissions
- requiredQualifications
- requiredEvidence
- blockerRefs
- warningRefs
- offlineAllowed
- offlineExpiresAt
- actionSchemaRef
- displayHints
- deepLink
- createdAt
- updatedAt
- lastSyncedAt
```

## Mobile task status definitions

```text
available
- Task can be opened by the user.

assigned
- Task is assigned to the user.

accepted
- User acknowledged/accepted the task.

in_progress
- User started the task.

blocked
- Task cannot proceed due to source product blocker.

submitted
- User submitted action locally or online and it is awaiting source confirmation.

synced
- Source product accepted the action.

failed_sync
- Action failed to sync or source product rejected it.

completed
- Source product considers the task complete.

canceled
- Task is canceled by source product.

expired
- Task or secure action window expired.
```

## Mobile task grouping

```text
MobileTaskGroup
- groupId
- personId
- groupType
  - today
  - overdue
  - high_priority
  - route
  - work_order
  - training
  - warehouse
  - approvals
  - evidence_needed
  - blocked
- title
- taskRefs
- sortOrder
```

## My work view

```text
MyWorkView
- personId
- currentShiftSnapshot
- taskGroups
- urgentTasks
- blockedTasks
- offlineQueueSummary
- syncStatusSummary
- productAccessSummary
- readinessWarnings
```

## Mobile action schema

A MobileActionSchema tells Field Companion what fields, controls, validations, and evidence capture steps to show for a source product action.

```text
MobileActionSchema
- actionSchemaId
- sourceProduct
- actionKey
- title
- description
- schemaVersion
- status
  - draft
  - active
  - deprecated
- fields
- validationRules
- evidenceRequirements
- offlineBehavior
- submissionEndpoint
- conflictRules
- successMessage
- failureMessage
```

## Mobile action field

```text
MobileActionField
- fieldId
- actionSchemaId
- fieldKey
- label
- helpText
- fieldType
  - text
  - textarea
  - number
  - date
  - datetime
  - select
  - multi_select
  - checkbox
  - pass_fail
  - yes_no
  - photo
  - video
  - audio
  - signature
  - barcode_scan
  - qr_scan
  - meter_reading
  - location_confirm
  - document_scan
  - hidden
- required
- options
- defaultValue
- validationRules
- conditionalDisplayRules
- mapsToSourceField
```

## Mobile session

```text
MobileSession
- mobileSessionId
- tenantId
- personId
- deviceId
- nexarrSessionRef
- status
  - active
  - expired
  - revoked
  - offline
- productContext
- startedAt
- endedAt
- expiresAt
- lastSyncAt
- lastOnlineAt
- offlineMode
- appVersion
- deviceSnapshot
- sourceIp
- userAgent
```

## Product surface

A ProductSurface is a mobile-friendly product area shown only when the user is entitled and permitted.

```text
ProductSurface
- productSurfaceId
- productKey
- title
- subtitle
- iconKey
- status
  - available
  - hidden
  - disabled
- requiredEntitlement
- requiredPermissionRefs
- primaryActions
- taskTypes
- launchPath
- displayOrder
```

## Product switcher

```text
ProductSwitcher
- personId
- tenantId
- productSurfaces
- defaultSurface
- lastUsedSurface
- entitlementSnapshot
- permissionSnapshot
```

## Mobile notification

```text
MobileNotification
- notificationId
- tenantId
- personId
- notificationType
  - task_assigned
  - task_due
  - task_overdue
  - route_update
  - work_order_update
  - training_due
  - approval_needed
  - sync_failed
  - secure_upload_completed
  - incident_update
- title
- body
- sourceProduct
- sourceObjectRef
- priority
- status
  - queued
  - sent
  - delivered
  - read
  - dismissed
  - failed
- createdAt
- sentAt
- readAt
```

## Session workflow

```text
1. User logs in through NexArr or receives scoped secure link.
2. Field Companion starts MobileSession.
3. Field Companion loads StaffArr person context.
4. Field Companion loads entitled product surfaces.
5. Field Companion loads mobile tasks from source products.
6. User performs action.
7. Field Companion submits action online or queues it offline.
8. Source product accepts/rejects action.
9. Task status updates.
```

## Mobile task events

```text
fieldcompanion.mobile_task.created
fieldcompanion.mobile_task.assigned
fieldcompanion.mobile_task.viewed
fieldcompanion.mobile_task.accepted
fieldcompanion.mobile_task.started
fieldcompanion.mobile_task.blocked
fieldcompanion.mobile_task.submitted
fieldcompanion.mobile_task.synced
fieldcompanion.mobile_task.failed_sync
fieldcompanion.mobile_task.completed
fieldcompanion.mobile_task.expired
fieldcompanion.mobile_session.started
fieldcompanion.mobile_session.ended
fieldcompanion.product_surface.opened
fieldcompanion.notification.sent
fieldcompanion.notification.read
```


---


# Field Companion — Secure Upload and Capture Model

## Secure upload session

A SecureUploadSession allows a narrow, temporary, scoped upload or signature flow. It is useful for drivers, vendors, customers, suppliers, visitors, or other people who should not receive full product access.

The session must not grant broad tenant access.

```text
SecureUploadSession
- uploadSessionId
- tenantId
- uploadSessionNumber
- sourceProduct
  - loadarr
  - routarr
  - recordarr
  - supplyarr
  - customarr
  - ordarr
  - assurarr
  - maintainarr
  - staffarr
- sourceObjectRef
- uploadPurpose
  - bol
  - pod
  - packing_slip
  - invoice_reference
  - supplier_document
  - customer_document
  - delivery_photo
  - damage_photo
  - signature
  - incident_photo
  - training_evidence
  - maintenance_evidence
  - quality_evidence
  - audit_evidence
  - other_document
- status
  - created
  - active
  - used
  - partially_used
  - expired
  - revoked
  - failed
- tokenHash
- publicLabel
- noLoginUserLabel
- allowedMimeTypes
- maxUploads
- maxFileSizeBytes
- requiresSignature
- requiresPhoto
- requiresDocumentScan
- allowRetake
- allowManualCrop
- recordarrUploadTargetRef
- uploadedRecordRefs
- createdAt
- createdByPersonId
- activatedAt
- expiresAt
- revokedAt
- revokedByPersonId
- revokeReason
- completedAt
- sourceIp
- userAgent
- auditTrail
```

## Secure upload access rules

```text
1. Token must be random, unguessable, scoped, and expiring.
2. Token must map to one purpose and one source context.
3. Token must not expose product navigation.
4. Token must not expose unrelated tenant data.
5. Upload limits must be enforced.
6. Session should support revoke.
7. Completed uploads become RecordArr records.
8. Source product receives only record refs and metadata.
```

## Capture artifact

CaptureArtifact is Field Companion’s local/mobile representation of evidence before or during RecordArr persistence.

```text
CaptureArtifact
- captureArtifactId
- tenantId
- mobileSessionId
- uploadSessionId
- sourceProduct
- sourceObjectRef
- captureType
  - photo
  - document_scan
  - signature
  - video
  - audio
  - barcode
  - qr_code
  - geolocation
  - form_response
- status
  - captured
  - processing
  - uploaded
  - accepted
  - rejected
  - failed
- localUri
- recordarrRecordId
- filename
- mimeType
- sizeBytes
- checksum
- capturedAt
- capturedByPersonId
- noLoginUserLabel
- metadata
- rejectionReason
```

## Document scan

Document scanning is the workflow where an image is corrected into a document-like PDF/evidence record.

```text
DocumentScan
- documentScanId
- tenantId
- captureArtifactId
- uploadSessionId
- sourceProduct
- sourceObjectRef
- scanPurpose
  - bol
  - pod
  - packing_slip
  - certificate
  - inspection_form
  - maintenance_record
  - supplier_document
  - customer_document
  - other
- status
  - image_captured
  - edge_detection_pending
  - edge_detected
  - manual_crop_required
  - manually_corrected
  - enhanced
  - pdf_generation_pending
  - pdf_generated
  - ocr_pending
  - ocr_completed
  - failed
- originalImageRef
- correctedImageRef
- generatedPdfRecordRef
- edgeCoordinates
- manualEdgeCoordinates
- enhancementSettings
- ocrSummary
- confidenceScore
- processedAt
- correctedByPersonId
```

## Signature capture

```text
SignatureCapture
- signatureCaptureId
- tenantId
- sourceProduct
- sourceObjectRef
- signaturePurpose
  - proof_of_delivery
  - proof_of_pickup
  - training_acknowledgement
  - trainer_signoff
  - evaluator_signoff
  - work_order_closeout
  - inspection_attestation
  - quality_release
  - customer_acceptance
  - other
- signerName
- signerPersonId
- signerExternalLabel
- signerTitle
- signatureImageRecordRef
- signedAt
- capturedByPersonId
- geoCoordinates
- deviceSnapshot
- attestationText
```

## Photo capture

```text
PhotoCapture
- photoCaptureId
- tenantId
- sourceProduct
- sourceObjectRef
- photoPurpose
  - defect
  - damage
  - completion
  - before
  - after
  - receipt
  - delivery
  - quality
  - incident
  - audit
  - training
  - other
- recordarrRecordId
- capturedAt
- capturedByPersonId
- geoCoordinates
- notes
- required
- acceptedBySourceProduct
```

## Barcode/QR scan

```text
ScanEvent
- scanEventId
- tenantId
- mobileSessionId
- sourceProduct
- sourceObjectRef
- scanType
  - barcode
  - qr_code
  - data_matrix
  - text
- scannedValue
- parsedValue
- scanPurpose
  - identify_asset
  - identify_location
  - identify_item
  - identify_order
  - identify_trip
  - secure_upload
  - login_handoff
  - count
  - pick
  - putaway
  - issue
  - receiving
- validationStatus
  - valid
  - invalid
  - unknown
  - duplicate
- validatedByProduct
- validationMessage
- scannedAt
- scannedByPersonId
```

## Voice note capture

```text
VoiceNoteCapture
- voiceNoteId
- tenantId
- sourceProduct
- sourceObjectRef
- purpose
  - work_note
  - defect_note
  - incident_note
  - route_exception
  - quality_note
  - training_note
- audioRecordRef
- transcriptRecordRef
- transcriptionStatus
  - pending
  - completed
  - failed
  - skipped
- capturedAt
- capturedByPersonId
```

## BOL upload workflow

```text
1. LoadArr or RoutArr creates secure upload session for BOL.
2. Field Companion displays QR code or link.
3. Driver opens link without login.
4. Driver captures or uploads image.
5. Field Companion starts document scan.
6. Edge detection runs.
7. Driver/receiver can manually crop if needed.
8. Image is enhanced and sent to RecordArr.
9. RecordArr creates PDF/OCR record.
10. Source product receives RecordRef.
11. Upload session is marked used/completed.
```

## Proof of delivery workflow

```text
1. Driver opens route stop in Field Companion.
2. RoutArr requires proof fields.
3. Driver captures signature, photos, notes, and optional document scan.
4. Field Companion submits proof action to RoutArr.
5. RecordArr stores evidence artifacts.
6. RoutArr marks stop completed.
7. OrdArr receives fulfillment update.
```

## Damage photo workflow

```text
1. User reports damage in receiving, delivery, maintenance, or quality flow.
2. Field Companion captures required photos.
3. Photo artifacts upload to RecordArr.
4. Source product receives RecordRefs.
5. AssurArr may create nonconformance/hold if quality impact exists.
```

## Capture events

```text
fieldcompanion.secure_upload.created
fieldcompanion.secure_upload.opened
fieldcompanion.secure_upload.completed
fieldcompanion.secure_upload.expired
fieldcompanion.secure_upload.revoked
fieldcompanion.capture.photo_captured
fieldcompanion.capture.signature_captured
fieldcompanion.capture.document_scanned
fieldcompanion.capture.voice_note_captured
fieldcompanion.scan.completed
fieldcompanion.capture.uploaded_to_recordarr
fieldcompanion.capture.rejected_by_source
```


---


# Field Companion — Offline, Sync, Device, and Conflict Model

## Offline action

An OfflineAction represents a user action captured while offline or during unreliable connectivity. It is not final until accepted by the owning product.

```text
OfflineAction
- offlineActionId
- tenantId
- mobileSessionId
- personId
- deviceId
- sourceProduct
- sourceObjectRef
- sourceTaskRef
- actionType
- actionKey
- payload
- payloadSchemaVersion
- localSequenceNumber
- capturedAt
- deviceTimestamp
- timezone
- locationSnapshot
- evidenceArtifactRefs
- syncStatus
  - queued
  - ready_to_sync
  - syncing
  - synced
  - conflict
  - rejected
  - failed
  - canceled
- serverReceivedAt
- syncedAt
- sourceProductResponse
- rejectionReason
- retryCount
- lastRetryAt
- idempotencyKey
```

## Sync batch

```text
SyncBatch
- syncBatchId
- tenantId
- mobileSessionId
- personId
- deviceId
- status
  - created
  - uploading
  - partially_synced
  - synced
  - conflict
  - failed
- actionRefs
- startedAt
- completedAt
- failedAt
- failureReason
- networkSnapshot
```

## Sync status summary

```text
SyncStatusSummary
- personId
- deviceId
- queuedActionCount
- failedActionCount
- conflictCount
- lastSuccessfulSyncAt
- lastFailedSyncAt
- offlineMode
- warningMessage
```

## Conflict

A Conflict occurs when an offline action cannot safely apply because source product state changed.

```text
Conflict
- conflictId
- tenantId
- offlineActionId
- sourceProduct
- sourceObjectRef
- conflictType
  - object_changed
  - object_closed
  - assignment_removed
  - permission_revoked
  - qualification_expired
  - hold_placed
  - duplicate_submission
  - stale_schema
  - validation_failed
  - evidence_missing
  - sequence_error
- severity
  - warning
  - blocking
  - critical
- status
  - open
  - resolved
  - discarded
  - force_submitted
- sourceCurrentStateSnapshot
- offlinePayloadSnapshot
- resolutionOptions
- resolvedAt
- resolvedByPersonId
- resolutionNotes
```

## Conflict resolution option

```text
ConflictResolutionOption
- optionKey
- label
- description
- allowed
- requiresPermission
- result
  - discard_local
  - retry
  - submit_as_new
  - overwrite_if_allowed
  - manual_review
  - open_source_product
```

## Device profile

```text
DeviceProfile
- deviceId
- tenantId
- personId
- deviceName
- platform
  - ios
  - android
  - web
  - windows
  - other
- appVersion
- osVersion
- browser
- status
  - trusted
  - untrusted
  - revoked
  - expired
- registeredAt
- registeredByPersonId
- lastSeenAt
- lastIp
- pushTokenRef
- biometricEnabledSnapshot
- offlineStorageEnabled
- revokedAt
- revokedByPersonId
- revokeReason
```

## Local cache entry

```text
LocalCacheEntry
- cacheEntryId
- tenantId
- deviceId
- personId
- sourceProduct
- sourceObjectRef
- cacheType
  - task
  - action_schema
  - lookup
  - location
  - asset_summary
  - item_summary
  - route_summary
  - training_step
- status
  - fresh
  - stale
  - expired
  - invalidated
- cachedAt
- expiresAt
- versionSnapshot
- dataClassification
  - low
  - internal
  - sensitive
  - restricted
```

## Offline policy

```text
OfflinePolicy
- policyId
- tenantId
- productKey
- actionKey
- offlineAllowed
- maxOfflineDurationMinutes
- requiresPriorCache
- requiresCurrentAssignment
- allowEvidenceCaptureOffline
- allowSubmitAfterExpiration
- conflictBehavior
  - reject
  - manual_review
  - accept_if_idempotent
  - accept_with_warning
- sensitiveDataCacheAllowed
```

## Network snapshot

```text
NetworkSnapshot
- connectionType
  - wifi
  - cellular
  - offline
  - unknown
- effectiveType
  - slow
  - moderate
  - fast
  - unknown
- online
- capturedAt
```

## Sync workflow

```text
1. User opens task while online.
2. Field Companion caches task and action schema if offline is allowed.
3. User loses connection.
4. User completes action offline.
5. Field Companion creates OfflineAction with idempotency key.
6. Evidence artifacts are stored locally until upload.
7. Connection returns.
8. Field Companion uploads evidence to RecordArr.
9. Field Companion submits action to owning product.
10. Owning product validates state, permission, assignment, and schema version.
11. Action is accepted, rejected, or marked conflict.
12. User sees clear result.
```

## Offline safety rules

```text
1. Offline actions are never final until accepted by source product.
2. Dangerous actions may require online validation.
3. Permission-sensitive actions should have short offline windows.
4. Source product must validate idempotency.
5. Source product must validate stale object state.
6. Sensitive cached data should expire.
7. User must see unsynced/failed/conflict state.
8. Field Companion must not hide sync failure.
```

## Device registration workflow

```text
1. User signs in through NexArr.
2. Field Companion captures device profile.
3. User/device trust policy is evaluated.
4. Device profile is created or updated.
5. Push token is registered if available.
6. Device can be revoked by policy or admin.
```

## Conflict workflow

```text
1. Offline action sync fails due to state mismatch.
2. Conflict is created.
3. User sees human-readable explanation.
4. Allowed resolution options are shown.
5. User or supervisor resolves.
6. Source product receives final action or local action is discarded.
```

## Offline/sync/device events

```text
fieldcompanion.offline_action.created
fieldcompanion.offline_action.queued
fieldcompanion.offline_action.sync_started
fieldcompanion.offline_action.synced
fieldcompanion.offline_action.rejected
fieldcompanion.offline_action.failed
fieldcompanion.offline_action.conflict

fieldcompanion.sync_batch.created
fieldcompanion.sync_batch.completed
fieldcompanion.sync_batch.failed

fieldcompanion.conflict.created
fieldcompanion.conflict.resolved
fieldcompanion.conflict.discarded

fieldcompanion.device.registered
fieldcompanion.device.seen
fieldcompanion.device.revoked

fieldcompanion.cache.created
fieldcompanion.cache.invalidated
fieldcompanion.cache.expired
```


---


# Field Companion — Product Surfaces and Action Model

## Product surface rule

Field Companion should not simply mirror every product admin screen. It should expose field-appropriate actions.

The default mobile layout should be:

```text
- My Work
- Scan
- Report
- Product surfaces
- Offline queue
- Profile/readiness
```

## MaintainArr mobile surface

```text
MaintainArrSurface
- assigned work orders
- assigned work order tasks
- asset lookup by scan/search
- inspection execution
- defect reporting
- meter reading entry
- labor start/stop
- part request
- part usage/install confirmation
- photo evidence
- return-to-service checklist where permitted
```

### MaintainArr mobile actions

```text
maintainarr.work_order.start
maintainarr.work_order.pause
maintainarr.work_order.resume
maintainarr.work_order.complete_task
maintainarr.work_order.add_note
maintainarr.work_order.record_labor
maintainarr.work_order.request_part
maintainarr.work_order.record_part_usage
maintainarr.work_order.upload_evidence
maintainarr.inspection.start
maintainarr.inspection.answer_item
maintainarr.inspection.pause
maintainarr.inspection.complete
maintainarr.defect.report
maintainarr.meter_reading.record
maintainarr.asset.scan_lookup
```

## TrainArr mobile surface

```text
TrainArrSurface
- my training assignments
- training steps
- trainee acknowledgement
- trainer signoff
- evaluator signoff
- practical evaluation checklist
- evidence upload
- remediation tasks
```

### TrainArr mobile actions

```text
trainarr.assignment.start
trainarr.step.complete
trainarr.step.upload_evidence
trainarr.trainee.acknowledge
trainarr.trainer.signoff
trainarr.evaluator.signoff
trainarr.evaluation.pass
trainarr.evaluation.fail
```

## RoutArr mobile surface

```text
RoutArrSurface
- assigned trips
- route stops
- stop instructions
- arrive/depart
- pickup proof
- delivery proof
- route exceptions
- BOL/POD upload
- customer signature
- damage photos
```

### RoutArr mobile actions

```text
routarr.trip.start
routarr.trip.complete
routarr.stop.arrive
routarr.stop.depart
routarr.proof.pickup_capture
routarr.proof.delivery_capture
routarr.exception.report
routarr.document.upload_bol
routarr.document.upload_pod
```

## LoadArr mobile surface

```text
LoadArrSurface
- receiving tasks
- putaway tasks
- pick tasks
- issue tasks
- transfer tasks
- cycle counts
- item/location scans
- discrepancy reporting
- quarantine/hold visibility
```

### LoadArr mobile actions

```text
loadarr.receiving.start
loadarr.receiving.scan_item
loadarr.receiving.confirm_line
loadarr.receiving.report_discrepancy
loadarr.putaway.start
loadarr.putaway.confirm
loadarr.pick.start
loadarr.pick.confirm
loadarr.issue.confirm
loadarr.transfer.confirm
loadarr.count.record
loadarr.location.scan
loadarr.item.scan
```

## StaffArr mobile surface

```text
StaffArrSurface
- my profile
- my readiness
- my permissions snapshot
- my team/direct reports if supervisor
- incident reporting
- approval tasks
- restriction/blocker visibility
```

### StaffArr mobile actions

```text
staffarr.incident.report
staffarr.approval.decide
staffarr.readiness.view
staffarr.person.scan_lookup
staffarr.location.scan_lookup
```

## AssurArr mobile surface

```text
AssurArrSurface
- assigned containment actions
- assigned CAPA actions
- quality audit checklist
- nonconformance evidence capture
- hold/release evidence
- quality photos
```

### AssurArr mobile actions

```text
assurarr.nonconformance.create
assurarr.containment.start
assurarr.containment.complete
assurarr.capa_action.complete
assurarr.audit.answer_item
assurarr.quality_evidence.upload
assurarr.hold_release.request
```

## RecordArr mobile surface

```text
RecordArrSurface
- document upload
- document scan
- photo capture
- signature capture
- evidence package contribution
- upload status
```

### RecordArr mobile actions

```text
recordarr.record.upload
recordarr.document.scan
recordarr.photo.capture
recordarr.signature.capture
recordarr.evidence.submit
```

## SupplyArr mobile surface

```text
SupplyArrSurface
- supplier document upload
- supplier receiving evidence where allowed
- purchase request evidence
- vendor work evidence
```

### SupplyArr mobile actions

```text
supplyarr.supplier_document.upload
supplyarr.purchase_request.evidence_upload
supplyarr.vendor_contact.capture
```

## OrdArr mobile surface

```text
OrdArrSurface
- assigned order tasks
- fulfillment evidence
- customer acceptance where permitted
- order blocker visibility
```

### OrdArr mobile actions

```text
ordarr.order_task.complete
ordarr.fulfillment_evidence.capture
ordarr.customer_acceptance.capture
ordarr.blocker.view
```

## CustomArr mobile surface

```text
CustomArrSurface
- customer location details
- customer contact details
- customer issue capture
- customer signature/upload where permitted
```

### CustomArr mobile actions

```text
customarr.customer_location.view
customarr.customer_issue.create
customarr.customer_signature.capture
customarr.customer_document.upload
```

## Compliance Core mobile surface

Compliance Core should not become a general legal research UI in Field Companion. It may expose controlled prompts where needed.

```text
ComplianceCoreSurface
- controlled compliance prompts
- evidence requirement prompts
- field-safe TSE input where permitted
- requirement explanation snippets
```

### Compliance Core mobile actions

```text
compliancecore.evidence_requirement.view
compliancecore.controlled_prompt.answer
compliancecore.tse.mobile_evaluate_limited
```

## Mobile action submission

```text
MobileActionSubmission
- submissionId
- tenantId
- mobileTaskId
- mobileSessionId
- personId
- deviceId
- sourceProduct
- sourceObjectRef
- actionKey
- payload
- evidenceArtifactRefs
- status
  - draft
  - submitted
  - accepted
  - rejected
  - conflict
  - failed
- submittedAt
- acceptedAt
- rejectedAt
- rejectionReason
- sourceProductResponse
- idempotencyKey
```

## Mobile validation result

```text
MobileValidationResult
- validationResultId
- submissionId
- sourceProduct
- status
  - valid
  - warning
  - invalid
  - blocked
- messages
- fieldErrors
- blockers
- requiredActions
```

## Human-friendly blocker

```text
MobileBlockerDisplay
- blockerId
- sourceProduct
- sourceObjectRef
- blockerType
  - permission
  - qualification
  - assignment
  - safety
  - quality
  - inventory
  - compliance
  - document
  - sync
  - expired
  - unknown
- title
- plainLanguageMessage
- canResolveHere
- resolutionAction
- escalationTarget
```

## Mobile UX rules

```text
1. Default to task-based UI, not product admin UI.
2. A worker should not need to know which product owns the backend object.
3. Product ownership should remain clear in metadata and audit.
4. Blockers must be plain language.
5. Critical actions need confirmation.
6. Evidence requirements should be shown before submission.
7. Offline eligibility should be obvious.
8. Failed sync should be impossible to miss.
9. No-login users should see only the requested upload/signature action.
```


---


# Field Companion — Workflows, Status Logic, Events, and APIs

## Major workflow: authenticated mobile task execution

```text
1. User signs in through NexArr.
2. Field Companion opens MobileSession.
3. Field Companion loads StaffArr person/readiness/permission context.
4. Field Companion loads product surfaces based on entitlement and permissions.
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
- POST /handoff/redeem
- GET /platform/me
- GET /entitlements
- POST /service-tokens/introspect

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
