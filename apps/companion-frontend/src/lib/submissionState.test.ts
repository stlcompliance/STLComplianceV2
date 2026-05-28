import { afterEach, describe, expect, it } from 'vitest'

import {
  clearSubmissionStateForTests,
  getLocalSubmission,
  getSubmissionToasts,
  mergeSubmissionChips,
  pushSubmissionToast,
  setLocalSubmission,
} from './submissionState'

describe('submissionState', () => {
  afterEach(() => {
    clearSubmissionStateForTests()
  })

  it('stores local acknowledge phases', () => {
    setLocalSubmission({
      taskKey: 'trainarr:assignment:1',
      kind: 'acknowledge',
      phase: 'queued',
      message: 'Queued for sync',
    })

    const entry = getLocalSubmission('trainarr:assignment:1', 'acknowledge')
    expect(entry?.phase).toBe('queued')
    expect(entry?.message).toBe('Queued for sync')
  })

  it('prefers in-flight local state over server synced', () => {
    const chips = mergeSubmissionChips({
      taskKey: 'trainarr:assignment:1',
      acknowledgeLocal: {
        taskKey: 'trainarr:assignment:1',
        kind: 'acknowledge',
        phase: 'syncing',
        updatedAt: new Date().toISOString(),
      },
      serverItems: [
        {
          submissionKind: 'acknowledge',
          status: 'synced',
          detailMessage: 'Older sync',
        },
      ],
    })

    expect(chips).toHaveLength(1)
    expect(chips[0]?.label).toContain('syncing')
    expect(chips[0]?.tone).toBe('progress')
  })

  it('shows server synced when no local in-flight entry', () => {
    const chips = mergeSubmissionChips({
      taskKey: 'trainarr:assignment:1',
      serverItems: [
        {
          submissionKind: 'evidence',
          status: 'synced',
          detailMessage: 'Uploaded photo evidence',
        },
      ],
    })

    expect(chips[0]?.tone).toBe('success')
    expect(chips[0]?.detail).toBe('Uploaded photo evidence')
  })

  it('queues submission toasts', () => {
    pushSubmissionToast({ tone: 'success', message: 'Synced.' })
    pushSubmissionToast({ tone: 'error', message: 'Failed.' })

    expect(getSubmissionToasts()[0]?.message).toBe('Failed.')
  })
})
