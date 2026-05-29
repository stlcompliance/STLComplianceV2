import { useCallback, useEffect, useState } from 'react'

import { syncCompanionOfflineActions, validateCompanionFieldTask } from '../api/client'
import { resolveDeniedReason } from '../lib/companionDeniedReasonCatalog'
import { companionPlainReason } from '../lib/companionPlainReason'
import {
  enqueueFieldInboxAcknowledge,
  getOfflineQueueSnapshot,
  markSyncPartial,
  OfflineQueueCapacityError,
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

  const refresh = useCallback(() => {
    setSnapshot(getOfflineQueueSnapshot())
  }, [])

  const syncPending = useCallback(async () => {
    if (!accessToken || snapshot.pending.length === 0) {
      return
    }

    for (const item of snapshot.pending) {
      setLocalSubmission({
        taskKey: item.taskKey,
        kind: 'acknowledge',
        phase: 'syncing',
      })
    }

    setIsSyncing(true)
    try {
      const response = await syncCompanionOfflineActions(accessToken, {
        actions: snapshot.pending.map((item) => ({
          idempotencyKey: item.idempotencyKey,
          actionKind: item.actionKind,
          taskKey: item.taskKey,
          productKey: item.productKey,
          clientCreatedAt: item.clientCreatedAt,
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
        permanentRejectedKeys: permanentKeys,
        lastSyncError,
      })

      for (const item of response.synced) {
        setLocalSubmission({
          taskKey: item.taskKey,
          kind: 'acknowledge',
          phase: 'synced',
          message: 'Acknowledgment synced to NexArr.',
        })
      }

      for (const item of response.rejectedItems) {
        setLocalSubmission({
          taskKey:
            snapshot.pending.find((pending) => pending.idempotencyKey === item.idempotencyKey)
              ?.taskKey ?? '',
          kind: 'acknowledge',
          phase: retryableKeys.has(item.idempotencyKey) ? 'queued' : 'failed',
          message: resolveDeniedReason(item, 'Acknowledgment could not sync.'),
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
          pushSubmissionToast({
            tone: 'error',
            message: resolveDeniedReason(item, 'Acknowledgment could not sync.'),
          })
        }
      }

      refresh()
      options?.onSyncComplete?.()
    } catch (error) {
      const message = companionPlainReason(error, 'Offline sync failed')
      markSyncPartial({
        syncedKeys: new Set(),
        permanentRejectedKeys: new Set(),
        lastSyncError: message,
      })

      for (const item of snapshot.pending) {
        setLocalSubmission({
          taskKey: item.taskKey,
          kind: 'acknowledge',
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
  }, [accessToken, options, refresh, snapshot.pending])

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
    async (input: { taskKey: string; productKey: string; title: string }) => {
      if (accessToken) {
        const validation = await validateCompanionFieldTask(accessToken, {
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

  return {
    pending: snapshot.pending,
    pendingCount: snapshot.pending.length,
    lastSyncedAt: snapshot.lastSyncedAt,
    lastSyncError: snapshot.lastSyncError,
    isOnline,
    isSyncing,
    queueAcknowledge,
    syncPending,
    refresh,
  }
}

export type { QueuedOfflineAction }
