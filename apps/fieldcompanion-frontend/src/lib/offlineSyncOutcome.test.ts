import { describe, expect, it } from 'vitest'

import {
  isRetryableOfflineRejection,
  partitionRejectedItems,
  summarizeOfflineSyncOutcome,
} from './offlineSyncOutcome'

describe('offlineSyncOutcome', () => {
  it('summarizes mixed sync results', () => {
    expect(
      summarizeOfflineSyncOutcome({ accepted: 1, duplicates: 0, rejected: 1 }),
    ).toBe('1 offline action synced; 1 could not sync.')
  })

  it('partitions retryable inbox failures', () => {
    const { retryableKeys, permanentKeys } = partitionRejectedItems([
      {
        idempotencyKey: 'a',
        reasonCode: 'fieldcompanion.field_task.inbox_unavailable',
        reasonMessage: 'Try again later.',
      },
      {
        idempotencyKey: 'b',
        reasonCode: 'fieldcompanion.field_task.not_in_inbox',
        reasonMessage: 'Not in inbox.',
      },
    ])

    expect(retryableKeys.has('a')).toBe(true)
    expect(permanentKeys.has('b')).toBe(true)
    expect(isRetryableOfflineRejection('fieldcompanion.field_task.inbox_unavailable')).toBe(true)
  })
})
