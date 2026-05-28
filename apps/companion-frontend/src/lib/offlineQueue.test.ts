import { afterEach, describe, expect, it } from 'vitest'

import {
  clearOfflineQueueForTests,
  enqueueFieldInboxAcknowledge,
  getOfflineQueueSnapshot,
  markSyncSuccess,
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
