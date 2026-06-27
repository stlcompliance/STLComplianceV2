import { afterEach, describe, expect, it } from 'vitest'

import { MAX_OFFLINE_QUEUE_SIZE } from './offlineSyncOutcome'
import {
  CLOCK_QUEUE_PRODUCT_KEY,
  CLOCK_QUEUE_TASK_KEY,
  clearOfflineQueueForTests,
  discardOfflineQueueConflict,
  enqueueFieldInboxAcknowledge,
  enqueueClockPunch,
  getOfflineQueueSnapshot,
  OFFLINE_ACTION_STAFFARR_CLOCK_PUNCH,
  markSyncPartial,
  markSyncSuccess,
  OfflineQueueCapacityError,
  OFFLINE_QUEUE_STORAGE_KEY,
  retryOfflineQueueConflict,
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

  it('stores clock punches without sensitive details in session storage', () => {
    const action = enqueueClockPunch({
      eventType: 'clock_in',
      eventTimestamp: '2026-06-23T12:00:00.000Z',
      capturedAt: '2026-06-23T12:00:00.000Z',
      timezone: 'America/Chicago',
    })

    expect(action.payload).toMatchObject({
      eventType: 'clock_in',
      eventTimestamp: '2026-06-23T12:00:00.000Z',
      capturedAt: '2026-06-23T12:00:00.000Z',
      timezone: 'America/Chicago',
      idempotencyKey: action.idempotencyKey,
    })

    const stored = window.sessionStorage.getItem(OFFLINE_QUEUE_STORAGE_KEY) ?? ''
    expect(stored).toContain('"eventType":"clock_in"')
    expect(stored).not.toContain('sourceDeviceId')
    expect(stored).not.toContain('geoPoint')
    expect(stored).not.toContain('siteRef')
    expect(stored).not.toContain('locationRef')
    expect(stored).not.toContain('notes')
  })

  it('redacts sensitive clock punch data from legacy queue snapshots', () => {
    window.sessionStorage.setItem(
      OFFLINE_QUEUE_STORAGE_KEY,
      JSON.stringify({
        pending: [
          {
            idempotencyKey: 'clock-legacy',
            actionKind: OFFLINE_ACTION_STAFFARR_CLOCK_PUNCH,
            taskKey: CLOCK_QUEUE_TASK_KEY,
            productKey: CLOCK_QUEUE_PRODUCT_KEY,
            clientCreatedAt: '2026-06-23T12:00:00.000Z',
            title: 'Clock in',
            payload: {
              eventType: 'clock_in',
              eventTimestamp: '2026-06-23T12:00:00.000Z',
              capturedAt: '2026-06-23T12:00:00.000Z',
              timezone: 'America/Chicago',
              idempotencyKey: 'clock-legacy',
              sourceDeviceId: 'Mozilla/5.0 test device',
              geoPoint: '39.7817,-89.6501',
              siteRef: 'site-123',
              locationRef: 'location-456',
              notes: 'private note',
            },
          },
        ],
        lastSyncedAt: null,
        lastSyncError: null,
      }),
    )

    const snapshot = getOfflineQueueSnapshot()
    const first = snapshot.pending[0] as any

    expect(first.payload).toMatchObject({
      eventType: 'clock_in',
      eventTimestamp: '2026-06-23T12:00:00.000Z',
      capturedAt: '2026-06-23T12:00:00.000Z',
      timezone: 'America/Chicago',
      idempotencyKey: 'clock-legacy',
    })

    const stored = window.sessionStorage.getItem(OFFLINE_QUEUE_STORAGE_KEY) ?? ''
    expect(stored).not.toContain('sourceDeviceId')
    expect(stored).not.toContain('geoPoint')
    expect(stored).not.toContain('siteRef')
    expect(stored).not.toContain('locationRef')
    expect(stored).not.toContain('notes')
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

  it('moves permanent rejections into reviewable conflicts and keeps retryable ones pending', () => {
    const retryable = enqueueFieldInboxAcknowledge({
      taskKey: 'trainarr:retry',
      productKey: 'trainarr',
      title: 'Retry me',
    })
    const permanent = enqueueFieldInboxAcknowledge({
      taskKey: 'trainarr:drop',
      productKey: 'trainarr',
      title: 'Drop me',
      deepLinkPath: '/assignments/trainarr-drop',
    })

    markSyncPartial({
      syncedKeys: new Set(),
      permanentRejectedItems: [
        {
          idempotencyKey: permanent.idempotencyKey,
          reasonCode: 'fieldcompanion.offline_actions.record_changed',
          reasonMessage: 'The task changed while you were offline.',
        },
      ],
      lastSyncError: 'Inbox unavailable',
    })

    const snapshot = getOfflineQueueSnapshot()
    expect(snapshot.pending.map((item) => item.idempotencyKey)).toEqual([retryable.idempotencyKey])
    expect(snapshot.conflicts).toHaveLength(1)
    expect(snapshot.conflicts[0]?.action.idempotencyKey).toBe(permanent.idempotencyKey)
    expect(snapshot.conflicts[0]?.action.deepLinkPath).toBe('/assignments/trainarr-drop')
    expect(snapshot.lastSyncError).toBe('Inbox unavailable')
  })

  it('retries and discards conflicts without affecting other pending actions', () => {
    const action = enqueueFieldInboxAcknowledge({
      taskKey: 'trainarr:conflict',
      productKey: 'trainarr',
      title: 'Retry me later',
    })

    markSyncPartial({
      syncedKeys: new Set(),
      permanentRejectedItems: [
        {
          idempotencyKey: action.idempotencyKey,
          reasonCode: 'fieldcompanion.offline_actions.record_changed',
          reasonMessage: 'The task changed while you were offline.',
        },
      ],
      lastSyncError: 'Validation failed',
    })

    expect(getOfflineQueueSnapshot().conflicts).toHaveLength(1)
    expect(retryOfflineQueueConflict(action.idempotencyKey)).toBe(true)
    expect(getOfflineQueueSnapshot().pending).toHaveLength(1)
    expect(getOfflineQueueSnapshot().conflicts).toHaveLength(0)

    expect(discardOfflineQueueConflict(action.idempotencyKey)).toBe(true)
    expect(getOfflineQueueSnapshot().pending).toHaveLength(0)
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
    expect(snapshot.conflicts).toHaveLength(0)
    expect(snapshot.lastSyncedAt).toBeTruthy()
    expect(window.sessionStorage.getItem(OFFLINE_QUEUE_STORAGE_KEY)).toContain('lastSyncedAt')
  })
})
