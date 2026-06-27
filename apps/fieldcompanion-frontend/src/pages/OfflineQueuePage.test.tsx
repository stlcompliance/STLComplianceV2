import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { OfflineQueuePage } from './OfflineQueuePage'

vi.mock('../hooks/useFieldCompanionWorkspace', () => ({
  useFieldCompanionWorkspace: vi.fn(() => ({
    accessToken: 'token',
  })),
}))

vi.mock('../hooks/useOfflineQueue', () => ({
  useOfflineQueue: vi.fn(() => ({
    isOnline: false,
    pendingCount: 2,
    pending: [
      { idempotencyKey: 'ack-1', title: 'Acknowledge work' },
      { idempotencyKey: 'ack-2', title: 'Acknowledge more work' },
    ],
    lastSyncedAt: '2026-06-26T00:00:00.000Z',
    lastSyncError: 'Sync unavailable',
    isSyncing: false,
    syncPending: vi.fn(),
  })),
}))

vi.mock('../hooks/useFieldTaskSubmissionState', () => ({
  useFieldTaskSubmissionState: vi.fn(() => ({
    isLoadingServer: false,
  })),
}))

vi.mock('../components/OfflineQueuePanel', () => ({
  OfflineQueuePanel: ({
    isOnline,
    pendingCount,
  }: {
    isOnline: boolean
    pendingCount: number
  }) => (
    <div data-testid="fieldcompanion-offline-queue-panel">
      {isOnline ? 'online' : 'offline'} {pendingCount}
    </div>
  ),
}))

describe('OfflineQueuePage', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders the queue summary and sync guidance', () => {
    render(<OfflineQueuePage />)

    expect(screen.getByText('Offline queue')).toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-offline-queue-panel')).toHaveTextContent('offline 2')
    expect(screen.getByText('How sync behaves')).toBeInTheDocument()
    expect(
      screen.getByText(/Queued items older than 24 hours are flagged as stale so you can review them before syncing\./i),
    ).toBeInTheDocument()
  })
})
