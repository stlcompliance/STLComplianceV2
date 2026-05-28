import { useCallback, useEffect, useState } from 'react'

import { syncCompanionOfflineActions } from '../api/client'
import {
  enqueueFieldInboxAcknowledge,
  getOfflineQueueSnapshot,
  markSyncFailure,
  markSyncSuccess,
  type OfflineQueueSnapshot,
  type QueuedOfflineAction,
} from '../lib/offlineQueue'

export function useOfflineQueue(accessToken: string) {
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
      refresh()
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Offline sync failed'
      markSyncFailure(message)
      refresh()
      throw error
    } finally {
      setIsSyncing(false)
    }
  }, [accessToken, refresh, snapshot.pending])

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
    (input: { taskKey: string; productKey: string; title: string }) => {
      const action = enqueueFieldInboxAcknowledge(input)
      refresh()
      if (isOnline) {
        void syncPending().catch(() => undefined)
      }
      return action
    },
    [isOnline, refresh, syncPending],
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
