import { describe, expect, it } from 'vitest'

import {
  formatOfflineQueueAge,
  summarizeOfflineQueueFreshness,
} from './offlineQueueFreshness'

describe('offlineQueueFreshness', () => {
  it('formats relative queue ages', () => {
    const now = new Date('2026-06-26T12:00:00.000Z')

    expect(formatOfflineQueueAge('2026-06-26T11:58:00.000Z', now)).toBe('2m')
    expect(formatOfflineQueueAge('2026-06-26T09:30:00.000Z', now)).toBe('2h 30m')
    expect(formatOfflineQueueAge('2026-06-24T12:00:00.000Z', now)).toBe('2d 0h')
  })

  it('flags stale queues after 24 hours', () => {
    const snapshot = summarizeOfflineQueueFreshness({
      now: new Date('2026-06-26T12:00:00.000Z'),
      pending: [
        { clientCreatedAt: '2026-06-25T11:30:00.000Z' },
        { clientCreatedAt: '2026-06-26T11:30:00.000Z' },
      ],
      lastSyncedAt: '2026-06-26T10:00:00.000Z',
    })

    expect(snapshot.oldestPendingAt).toBe('2026-06-25T11:30:00.000Z')
    expect(snapshot.oldestPendingAgeLabel).toBe('1d 0h')
    expect(snapshot.lastSyncedAgeLabel).toBe('2h 0m')
    expect(snapshot.isStale).toBe(true)
  })
})
