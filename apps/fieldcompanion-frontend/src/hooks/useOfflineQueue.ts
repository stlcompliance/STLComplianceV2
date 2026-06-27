import { useCallback, useEffect, useState } from 'react'

import { syncFieldCompanionOfflineActions, validateFieldCompanionFieldTask } from '../api/client'
import { resolveDeniedReason } from '../lib/FieldCompanionDeniedReasonCatalog'
import { FieldCompanionPlainReason } from '../lib/FieldCompanionPlainReason'
import {
  CLOCK_QUEUE_TASK_KEY,
  enqueueFieldInboxAcknowledge,
  enqueueClockPunch,
  discardOfflineQueueConflict,
  getOfflineQueueSnapshot,
  getQueuedOfflineActionPayload,
  markSyncPartial,
  OFFLINE_ACTION_STAFFARR_CLOCK_PUNCH,
  OfflineQueueCapacityError,
  retryOfflineQueueConflict,
  type OfflineQueueSnapshot,
  type QueuedOfflineAction,
} from '../lib/offlineQueue'
import {
  partitionRejectedItems,
  summarizeOfflineSyncOutcome,
} from '../lib/offlineSyncOutcome'
import { pushSubmissionToast, setLocalSubmission } from '../lib/submissionState'

export function useOfflineQueue(
  accessToken: string,
  options?: { onSyncComplete?: () => void },
) {
  const [snapshot, setSnapshot] = useState<OfflineQueueSnapshot>(() => getOfflineQueueSnapshot())
  const [isOnline, setIsOnline] = useState(
    () => typeof navigator === 'undefined' || navigator.onLine,
  )
  const [isSyncing, setIsSyncing] = useState(false)

  const getSubmissionKind = useCallback((item: QueuedOfflineAction) => {
    return item.actionKind === OFFLINE_ACTION_STAFFARR_CLOCK_PUNCH ? 'clock' : 'acknowledge'
  }, [])

  const getSyncSuccessMessage = useCallback((item: QueuedOfflineAction) => {
    return item.actionKind === OFFLINE_ACTION_STAFFARR_CLOCK_PUNCH
      ? `${item.title} synced to StaffArr.`
      : 'Acknowledgment synced to NexArr.'
  }, [])

  const getDefaultFailureMessage = useCallback((item: QueuedOfflineAction) => {
    return item.actionKind === OFFLINE_ACTION_STAFFARR_CLOCK_PUNCH
      ? `${item.title} could not sync.`
      : 'Acknowledgment could not sync.'
  }, [])

  const refresh = useCallback(() => {
    setSnapshot(getOfflineQueueSnapshot())
  }, [])

  const syncPending = useCallback(async () => {
    const currentSnapshot = getOfflineQueueSnapshot()
    if (!accessToken || currentSnapshot.pending.length === 0) {
      return
    }

    for (const item of currentSnapshot.pending) {
      setLocalSubmission({
        taskKey: item.taskKey,
        kind: getSubmissionKind(item),
        phase: 'syncing',
      })
    }

    setIsSyncing(true)
    try {
      const response = await syncFieldCompanionOfflineActions(accessToken, {
        actions: currentSnapshot.pending.map((item) => ({
          idempotencyKey: item.idempotencyKey,
          actionKind: item.actionKind,
          taskKey: item.taskKey,
          productKey: item.productKey,
          clientCreatedAt: item.clientCreatedAt,
          payload: getQueuedOfflineActionPayload(item),
        })),
      })

      const syncedKeys = new Set(response.synced.map((item) => item.idempotencyKey))
      const { retryableKeys, permanentKeys } = partitionRejectedItems(response.rejectedItems)
      const summary = summarizeOfflineSyncOutcome(response)
      const lastSyncError =
        response.rejected > 0 && retryableKeys.size > 0
          ? resolveDeniedReason(
            response.rejectedItems.find((item) => retryableKeys.has(item.idempotencyKey)) ?? {},
              'Some acknowledgments could not sync yet. They will retry automatically when the product inbox is available.',
            )
          : response.rejected > 0
            ? resolveDeniedReason(
                response.rejectedItems[0] ?? {},
                'Some acknowledgments could not sync.',
              )
            : null

      markSyncPartial({
        syncedKeys,
        permanentRejectedItems: response.rejectedItems.filter((item) => permanentKeys.has(item.idempotencyKey)),
        lastSyncError,
      })

      for (const item of response.synced) {
        const pendingItem = currentSnapshot.pending.find((pending) => pending.idempotencyKey === item.idempotencyKey)
        setLocalSubmission({
          taskKey: pendingItem?.taskKey ?? item.taskKey,
          kind: pendingItem ? getSubmissionKind(pendingItem) : 'acknowledge',
          phase: 'synced',
          message: pendingItem ? getSyncSuccessMessage(pendingItem) : 'Offline action synced.',
        })
      }

      for (const item of response.rejectedItems) {
        const pendingItem = currentSnapshot.pending.find((pending) => pending.idempotencyKey === item.idempotencyKey)
        setLocalSubmission({
          taskKey: pendingItem?.taskKey ?? '',
          kind: pendingItem ? getSubmissionKind(pendingItem) : 'acknowledge',
          phase: retryableKeys.has(item.idempotencyKey) ? 'queued' : 'failed',
          message: resolveDeniedReason(
            item,
            pendingItem ? getDefaultFailureMessage(pendingItem) : 'Offline action could not sync.',
          ),
        })
      }

      if (summary) {
        pushSubmissionToast({
          tone: response.rejected > 0 && syncedKeys.size === 0 ? 'error' : 'success',
          message: summary,
        })
      }

      for (const item of response.rejectedItems) {
        if (!retryableKeys.has(item.idempotencyKey)) {
          const pendingItem = currentSnapshot.pending.find((pending) => pending.idempotencyKey === item.idempotencyKey)
          pushSubmissionToast({
            tone: 'error',
            message: resolveDeniedReason(
              item,
              pendingItem ? getDefaultFailureMessage(pendingItem) : 'Offline action could not sync.',
            ),
          })
        }
      }

      refresh()
      options?.onSyncComplete?.()
    } catch (error) {
      const message = FieldCompanionPlainReason(error, 'Offline sync failed')
      markSyncPartial({
        syncedKeys: new Set(),
        permanentRejectedItems: [],
        lastSyncError: message,
      })

      for (const item of currentSnapshot.pending) {
        setLocalSubmission({
          taskKey: item.taskKey,
          kind: getSubmissionKind(item),
          phase: 'failed',
          message,
        })
      }

      pushSubmissionToast({ tone: 'error', message })
      refresh()
      throw error
    } finally {
      setIsSyncing(false)
    }
  }, [accessToken, getDefaultFailureMessage, getSubmissionKind, getSyncSuccessMessage, options, refresh])

  useEffect(() => {
    const handleOnline = () => {
      setIsOnline(true)
      void syncPending().catch(() => undefined)
    }
    const handleOffline = () => setIsOnline(false)

    window.addEventListener('online', handleOnline)
    window.addEventListener('offline', handleOffline)
    return () => {
      window.removeEventListener('online', handleOnline)
      window.removeEventListener('offline', handleOffline)
    }
  }, [syncPending])

  const queueAcknowledge = useCallback(
    async (input: { taskKey: string; productKey: string; title: string; deepLinkPath?: string | null }) => {
      if (accessToken) {
        const validation = await validateFieldCompanionFieldTask(accessToken, {
          taskKey: input.taskKey,
          submissionKind: 'acknowledge',
          productKey: input.productKey,
        })
        if (!validation.allowed) {
          const message = resolveDeniedReason(
            validation,
            'This acknowledgment cannot be submitted right now.',
          )
          pushSubmissionToast({ tone: 'error', message })
          throw new Error(message)
        }
      }

      try {
        const action = enqueueFieldInboxAcknowledge(input)
        setLocalSubmission({
          taskKey: input.taskKey,
          kind: 'acknowledge',
          phase: isOnline ? 'syncing' : 'queued',
          message: isOnline ? undefined : 'Queued for sync when back online.',
        })
        pushSubmissionToast({
          tone: 'info',
          message: isOnline
            ? `Syncing acknowledgment for “${input.title}”.`
            : `Queued acknowledgment for “${input.title}”.`,
        })
        refresh()
        if (isOnline) {
          void syncPending().catch(() => undefined)
        }
        return action
      } catch (error) {
        if (error instanceof OfflineQueueCapacityError) {
          pushSubmissionToast({ tone: 'error', message: error.message })
        }

        throw error
      }
    },
    [accessToken, isOnline, refresh, syncPending],
  )

  const queueClockAction = useCallback(
    async (input: {
      eventType: 'clock_in' | 'clock_out'
      eventTimestamp: string
      capturedAt: string | null
      timezone: string
    }) => {
      try {
        const action = enqueueClockPunch(input)
        const actionLabel = input.eventType === 'clock_in' ? 'Clock in' : 'Clock out'
        setLocalSubmission({
          taskKey: CLOCK_QUEUE_TASK_KEY,
          kind: 'clock',
          phase: isOnline ? 'syncing' : 'queued',
          message: isOnline
            ? `${actionLabel} will sync to StaffArr now.`
            : `${actionLabel} queued for sync when back online.`,
        })
        pushSubmissionToast({
          tone: 'info',
          message: isOnline ? `Syncing ${actionLabel.toLowerCase()}.` : `Queued ${actionLabel.toLowerCase()}.`,
        })
        refresh()
        if (isOnline) {
          void syncPending().catch(() => undefined)
        }
        return action
      } catch (error) {
        if (error instanceof OfflineQueueCapacityError) {
          pushSubmissionToast({ tone: 'error', message: error.message })
        }

        throw error
      }
    },
    [isOnline, refresh, syncPending],
  )

  const retryConflict = useCallback(
    async (idempotencyKey: string) => {
      const moved = retryOfflineQueueConflict(idempotencyKey)
      refresh()
      if (moved && isOnline && accessToken) {
        await syncPending()
      }
      return moved
    },
    [accessToken, isOnline, refresh, syncPending],
  )

  const discardConflict = useCallback(
    (idempotencyKey: string) => {
      const removed = discardOfflineQueueConflict(idempotencyKey)
      refresh()
      return removed
    },
    [refresh],
  )

  return {
    pending: snapshot.pending,
    conflicts: snapshot.conflicts,
    pendingCount: snapshot.pending.length,
    conflictCount: snapshot.conflicts.length,
    lastSyncedAt: snapshot.lastSyncedAt,
    lastSyncError: snapshot.lastSyncError,
    isOnline,
    isSyncing,
    queueAcknowledge,
    queueClockAction,
    retryConflict,
    discardConflict,
    syncPending,
    refresh,
  }
}

export type { QueuedOfflineAction }
