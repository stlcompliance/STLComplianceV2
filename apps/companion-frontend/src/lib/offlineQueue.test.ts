import { afterEach, describe, expect, it } from 'vitest'

import { MAX_OFFLINE_QUEUE_SIZE } from './offlineSyncOutcome'
import {
  clearOfflineQueueForTests,
  enqueueFieldInboxAcknowledge,
  getOfflineQueueSnapshot,
  markSyncPartial,
  markSyncSuccess,
  OfflineQueueCapacityError,
  OFFLINE_QUEUE_STORAGE_KEY,
} from './offlineQueue'

describe('offlineQueue', () => {
  afterEach(() => {
    clearOfflineQueueForTests()
  })

  it('enqueues acknowledge actions with idempotency keys', () => {
    const action = enqueueFieldInboxAcknowledge({
      taskKey: 'trainarr:1',
      productKey: 'trainarr',
      title: 'Safety training',
    })

    expect(action.idempotencyKey).toBeTruthy()
    expect(getOfflineQueueSnapshot().pending).toHaveLength(1)
  })

  it('deduplicates pending acknowledges for the same task', () => {
    enqueueFieldInboxAcknowledge({
      taskKey: 'trainarr:1',
      productKey: 'trainarr',
      title: 'Safety training',
    })
    enqueueFieldInboxAcknowledge({
      taskKey: 'trainarr:1',
      productKey: 'trainarr',
      title: 'Safety training',
    })

    expect(getOfflineQueueSnapshot().pending).toHaveLength(1)
  })

  it('drops permanently rejected items but keeps retryable ones', () => {
    const retryable = enqueueFieldInboxAcknowledge({
      taskKey: 'trainarr:retry',
      productKey: 'trainarr',
      title: 'Retry me',
    })
    const permanent = enqueueFieldInboxAcknowledge({
      taskKey: 'trainarr:drop',
      productKey: 'trainarr',
      title: 'Drop me',
    })

    markSyncPartial({
      syncedKeys: new Set(),
      permanentRejectedKeys: new Set([permanent.idempotencyKey]),
      lastSyncError: 'Inbox unavailable',
    })

    const snapshot = getOfflineQueueSnapshot()
    expect(snapshot.pending.map((item) => item.idempotencyKey)).toEqual([retryable.idempotencyKey])
    expect(snapshot.lastSyncError).toBe('Inbox unavailable')
  })

  it('enforces max queue size', () => {
    for (let index = 0; index < MAX_OFFLINE_QUEUE_SIZE; index += 1) {
      enqueueFieldInboxAcknowledge({
        taskKey: `trainarr:${index}`,
        productKey: 'trainarr',
        title: `Task ${index}`,
      })
    }

    expect(() =>
      enqueueFieldInboxAcknowledge({
        taskKey: 'trainarr:overflow',
        productKey: 'trainarr',
        title: 'Overflow',
      }),
    ).toThrow(OfflineQueueCapacityError)
  })

  it('clears synced pending items', () => {
    const action = enqueueFieldInboxAcknowledge({
      taskKey: 'trainarr:2',
      productKey: 'trainarr',
      title: 'DVIR review',
    })

    markSyncSuccess(new Set([action.idempotencyKey]))
    const snapshot = getOfflineQueueSnapshot()
    expect(snapshot.pending).toHaveLength(0)
    expect(snapshot.lastSyncedAt).toBeTruthy()
    expect(window.localStorage.getItem(OFFLINE_QUEUE_STORAGE_KEY)).toContain('lastSyncedAt')
  })
})
