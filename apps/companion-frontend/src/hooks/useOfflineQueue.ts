import { useCallback, useEffect, useState } from 'react'

import { syncCompanionOfflineActions, validateCompanionFieldTask } from '../api/client'
import { companionPlainReason } from '../lib/companionPlainReason'
import {
  enqueueFieldInboxAcknowledge,
  getOfflineQueueSnapshot,
  markSyncFailure,
  markSyncSuccess,
  type OfflineQueueSnapshot,
  type QueuedOfflineAction,
} from '../lib/offlineQueue'
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
      markSyncSuccess(syncedKeys)

      for (const item of response.synced) {
        setLocalSubmission({
          taskKey: item.taskKey,
          kind: 'acknowledge',
          phase: 'synced',
          message: 'Acknowledgment synced to NexArr.',
        })
      }

      const syncedCount = response.accepted + response.duplicates
      if (syncedCount > 0) {
        pushSubmissionToast({
          tone: 'success',
          message:
            syncedCount === 1
              ? 'Field acknowledgment synced.'
              : `${syncedCount} field acknowledgments synced.`,
        })
      }

      refresh()
      options?.onSyncComplete?.()
    } catch (error) {
      const message = companionPlainReason(error, 'Offline sync failed')
      markSyncFailure(message)

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
          const message =
            validation.reasonMessage ?? 'This acknowledgment cannot be submitted right now.'
          pushSubmissionToast({ tone: 'error', message })
          throw new Error(message)
        }
      }

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
