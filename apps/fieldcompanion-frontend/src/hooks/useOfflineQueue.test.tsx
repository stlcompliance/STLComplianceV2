import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { useState } from 'react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { validateFieldCompanionFieldTask, syncFieldCompanionOfflineActions } from '../api/client'
import { clearSubmissionStateForTests, getLocalSubmission, getSubmissionToasts } from '../lib/submissionState'
import { clearOfflineQueueForTests, OFFLINE_ACTION_FIELD_INBOX_ACKNOWLEDGE } from '../lib/offlineQueue'
import { useOfflineQueue } from './useOfflineQueue'

vi.mock('../api/client', () => ({
  syncFieldCompanionOfflineActions: vi.fn(),
  validateFieldCompanionFieldTask: vi.fn(),
}))

function setNavigatorOnline(value: boolean): void {
  Object.defineProperty(navigator, 'onLine', {
    configurable: true,
    value,
  })
}

function Harness({ onSyncComplete }: { onSyncComplete: () => void }) {
  const queue = useOfflineQueue('access-token', { onSyncComplete })
  const [error, setError] = useState<string | null>(null)

  return (
    <div>
      <button
        type="button"
        onClick={() => {
          setError(null)
          void queue
            .queueAcknowledge({
              taskKey: 'task-1',
              productKey: 'maintainarr',
              title: 'Safety training',
              deepLinkPath: '/tasks/task-1',
            })
            .catch((queueError: unknown) => {
              setError(queueError instanceof Error ? queueError.message : String(queueError))
            })
        }}
      >
        Queue acknowledge
      </button>
      <button
        type="button"
        onClick={() => {
          void queue.syncPending()
        }}
      >
        Sync now
      </button>
      <div data-testid="online-state">{queue.isOnline ? 'online' : 'offline'}</div>
      <div data-testid="sync-state">{queue.isSyncing ? 'syncing' : 'idle'}</div>
      <div data-testid="pending-count">{queue.pendingCount}</div>
      <div data-testid="submission-phase">{getLocalSubmission('task-1', 'acknowledge')?.phase ?? 'none'}</div>
      <div data-testid="toast-messages">
        {getSubmissionToasts().map((toast) => toast.message).join(' | ') || 'none'}
      </div>
      {error ? <div data-testid="error-message">{error}</div> : null}
    </div>
  )
}

describe('useOfflineQueue', () => {
  beforeEach(() => {
    setNavigatorOnline(true)
    vi.clearAllMocks()
  })

  afterEach(() => {
    cleanup()
    clearOfflineQueueForTests()
    clearSubmissionStateForTests()
  })

  it('queues acknowledgments offline and syncs them after reconnect', async () => {
    setNavigatorOnline(false)

    vi.mocked(validateFieldCompanionFieldTask).mockResolvedValue({
      allowed: true,
      reasonCode: null,
      reasonMessage: null,
      taskKey: 'task-1',
      productKey: 'maintainarr',
      title: 'Safety training',
      blockedReason: null,
    })
    vi.mocked(syncFieldCompanionOfflineActions).mockImplementation(async (_accessToken, body) => {
      const action = body.actions[0]

      return {
        accepted: 1,
        duplicates: 0,
        rejected: 0,
        synced: [
          {
            idempotencyKey: action.idempotencyKey,
            actionKind: action.actionKind,
            taskKey: action.taskKey,
            productKey: action.productKey,
            syncedAt: '2026-06-26T12:00:00.000Z',
          },
        ],
        rejectedItems: [],
      } as never
    })

    const onSyncComplete = vi.fn()
    render(<Harness onSyncComplete={onSyncComplete} />)

    fireEvent.click(screen.getByRole('button', { name: 'Queue acknowledge' }))

    await waitFor(() => {
      expect(screen.getByTestId('pending-count')).toHaveTextContent('1')
    })

    expect(screen.getByTestId('online-state')).toHaveTextContent('offline')
    expect(screen.getByTestId('submission-phase')).toHaveTextContent('queued')
    expect(screen.getByTestId('toast-messages')).toHaveTextContent('Queued acknowledgment for “Safety training”.')
    expect(syncFieldCompanionOfflineActions).not.toHaveBeenCalled()

    window.dispatchEvent(new Event('online'))

    await waitFor(() => {
      expect(screen.getByTestId('online-state')).toHaveTextContent('online')
      expect(screen.getByTestId('pending-count')).toHaveTextContent('0')
      expect(screen.getByTestId('submission-phase')).toHaveTextContent('synced')
    })

    expect(validateFieldCompanionFieldTask).toHaveBeenCalledWith('access-token', {
      taskKey: 'task-1',
      submissionKind: 'acknowledge',
      productKey: 'maintainarr',
    })
    expect(syncFieldCompanionOfflineActions).toHaveBeenCalledWith('access-token', {
      actions: [
        expect.objectContaining({
          actionKind: OFFLINE_ACTION_FIELD_INBOX_ACKNOWLEDGE,
          taskKey: 'task-1',
          productKey: 'maintainarr',
        }),
      ],
    })
    expect(screen.getByTestId('toast-messages')).toHaveTextContent('1 offline action synced.')
    expect(screen.getByTestId('toast-messages')).toHaveTextContent('Queued acknowledgment for “Safety training”.')
    expect(onSyncComplete).toHaveBeenCalledTimes(1)
  })

  it('rejects denied acknowledgments without queuing or syncing', async () => {
    vi.mocked(validateFieldCompanionFieldTask).mockResolvedValue({
      allowed: false,
      reasonCode: 'fieldcompanion.field_task.access_unavailable',
      reasonMessage: 'This task is closed.',
      taskKey: 'task-1',
      productKey: 'maintainarr',
      title: 'Safety training',
      blockedReason: null,
    })

    render(<Harness onSyncComplete={vi.fn()} />)

    fireEvent.click(screen.getByRole('button', { name: 'Queue acknowledge' }))

    await waitFor(() => {
      expect(screen.getByTestId('error-message')).toHaveTextContent('This task is closed.')
    })

    expect(screen.getByTestId('pending-count')).toHaveTextContent('0')
    expect(screen.getByTestId('submission-phase')).toHaveTextContent('none')
    expect(screen.getByTestId('toast-messages')).toHaveTextContent('This task is closed.')
    expect(syncFieldCompanionOfflineActions).not.toHaveBeenCalled()
  })
})
