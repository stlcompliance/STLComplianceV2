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
FieldCompanion.offline_action.created
FieldCompanion.offline_action.queued
FieldCompanion.offline_action.sync_started
FieldCompanion.offline_action.synced
FieldCompanion.offline_action.rejected
FieldCompanion.offline_action.failed
FieldCompanion.offline_action.conflict

FieldCompanion.sync_batch.created
FieldCompanion.sync_batch.completed
FieldCompanion.sync_batch.failed

FieldCompanion.conflict.created
FieldCompanion.conflict.resolved
FieldCompanion.conflict.discarded

FieldCompanion.device.registered
FieldCompanion.device.seen
FieldCompanion.device.revoked

FieldCompanion.cache.created
FieldCompanion.cache.invalidated
FieldCompanion.cache.expired
```
