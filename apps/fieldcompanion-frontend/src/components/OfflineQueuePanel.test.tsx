import { cleanup, fireEvent, render, screen, within } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import {
  OFFLINE_ACTION_FIELD_INBOX_ACKNOWLEDGE,
  type QueuedOfflineAction,
} from '../lib/offlineQueue'
import { OfflineQueuePanel } from './OfflineQueuePanel'

describe('OfflineQueuePanel', () => {
  beforeEach(() => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-06-26T02:00:00.000Z'))
    vi.stubEnv('VITE_TRAINARR_FRONTEND_BASE', 'https://trainarr.example.com')
  })

  afterEach(() => {
    cleanup()
    vi.useRealTimers()
    vi.unstubAllEnvs()
  })

  it('renders conflict review actions', () => {
    const retryConflict = vi.fn()
    const discardConflict = vi.fn()
    const action = {
      idempotencyKey: 'ack-1',
      actionKind: OFFLINE_ACTION_FIELD_INBOX_ACKNOWLEDGE,
      taskKey: 'trainarr:1',
      productKey: 'trainarr',
      clientCreatedAt: '2026-06-26T00:00:00.000Z',
      title: 'Acknowledge training',
      deepLinkPath: '/assignments/00000000-0000-0000-0000-000000000111',
    } satisfies QueuedOfflineAction

    render(
      <OfflineQueuePanel
        isOnline={false}
        pendingCount={1}
        pending={[action]}
        conflicts={[
          {
            action,
            reasonCode: 'fieldcompanion.offline_actions.record_changed',
            reasonMessage: 'The task changed while you were offline.',
            rejectedAt: '2026-06-26T01:00:00.000Z',
          },
        ]}
        lastSyncedAt="2026-06-26T00:00:00.000Z"
        lastSyncError="Sync unavailable"
        isSyncing={false}
        onSyncNow={vi.fn()}
        onRetryConflict={retryConflict}
        onDiscardConflict={discardConflict}
      />,
    )

    expect(screen.getByText('Sync conflicts need review')).toBeInTheDocument()
    expect(within(screen.getByTestId('fieldcompanion-offline-conflict-item')).getByText('Acknowledge training')).toBeInTheDocument()
    expect(screen.getByText('The task changed while you were offline.')).toBeInTheDocument()
    expect(screen.getByText('Recommended next step')).toBeInTheDocument()
    expect(screen.getByText('Review needed')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Open current task' })).toHaveAttribute(
      'href',
      'https://trainarr.example.com/assignments/00000000-0000-0000-0000-000000000111',
    )
    expect(screen.getByTestId('fieldcompanion-offline-freshness')).toBeInTheDocument()
    expect(screen.getByText('Last sync age: 2h 0m')).toBeInTheDocument()

    fireEvent.click(screen.getByTestId('fieldcompanion-offline-conflict-retry'))
    fireEvent.click(screen.getByTestId('fieldcompanion-offline-conflict-discard'))

    expect(retryConflict).toHaveBeenCalledWith('ack-1')
    expect(discardConflict).toHaveBeenCalledWith('ack-1')
  })

  it('flags stale queues with a review warning', () => {
    const action = {
      idempotencyKey: 'ack-stale',
      actionKind: OFFLINE_ACTION_FIELD_INBOX_ACKNOWLEDGE,
      taskKey: 'trainarr:stale',
      productKey: 'trainarr',
      clientCreatedAt: '2026-06-24T12:00:00.000Z',
      title: 'Review stale action',
    } satisfies QueuedOfflineAction

    render(
      <OfflineQueuePanel
        isOnline={false}
        pendingCount={1}
        pending={[action]}
        conflicts={[]}
        lastSyncedAt="2026-06-25T12:00:00.000Z"
        lastSyncError={null}
        isSyncing={false}
        onSyncNow={vi.fn()}
      />,
    )

    expect(screen.getByTestId('fieldcompanion-offline-freshness-stale')).toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-offline-freshness-warning')).toHaveTextContent(
      'This queue is older than 24h. Review the items before syncing so you do not submit stale work.',
    )
    expect(screen.getByText('queued 1d 14h ago')).toBeInTheDocument()
  })
})
