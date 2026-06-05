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
FieldCompanion.mobile_task.created
FieldCompanion.mobile_task.assigned
FieldCompanion.mobile_task.viewed
FieldCompanion.mobile_task.accepted
FieldCompanion.mobile_task.started
FieldCompanion.mobile_task.blocked
FieldCompanion.mobile_task.submitted
FieldCompanion.mobile_task.synced
FieldCompanion.mobile_task.failed_sync
FieldCompanion.mobile_task.completed
FieldCompanion.mobile_task.expired
FieldCompanion.mobile_session.started
FieldCompanion.mobile_session.ended
FieldCompanion.product_surface.opened
FieldCompanion.notification.sent
FieldCompanion.notification.read
```
