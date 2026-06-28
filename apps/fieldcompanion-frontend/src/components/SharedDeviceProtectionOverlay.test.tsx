import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import {
  OFFLINE_ACTION_FIELD_INBOX_ACKNOWLEDGE,
  type QueuedOfflineAction,
} from '../lib/offlineQueue'
import { SharedDeviceProtectionOverlay } from './SharedDeviceProtectionOverlay'

const queuedAction = {
  idempotencyKey: 'ack-1',
  actionKind: OFFLINE_ACTION_FIELD_INBOX_ACKNOWLEDGE,
  taskKey: 'trainarr:1',
  productKey: 'trainarr',
  clientCreatedAt: '2026-06-26T00:00:00.000Z',
  title: 'Acknowledge training',
} satisfies QueuedOfflineAction

describe('SharedDeviceProtectionOverlay', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders the warning state and sign-out action', () => {
    const onReauthenticate = vi.fn()

    render(
      <SharedDeviceProtectionOverlay
        isVisible
        isWarning
        userDisplayName="Alex Worker"
        tenantDisplayName="Acme Logistics"
        pendingActions={[]}
        onOpenOfflineQueue={vi.fn()}
        onReauthenticate={onReauthenticate}
        onDiscardQueuedWorkAndSignOut={vi.fn()}
        onStaySignedIn={vi.fn()}
      />,
    )

    expect(screen.getByText('Shared device warning')).toBeInTheDocument()
    expect(screen.getByText('Current session: Alex Worker · Acme Logistics')).toBeInTheDocument()
    expect(screen.queryByText(/acme-logistics/)).not.toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Sign out now' }))
    expect(onReauthenticate).toHaveBeenCalledTimes(1)
  })

  it('renders the locked queue review state without offering a direct sign-in exit when work is pending', () => {
    render(
      <SharedDeviceProtectionOverlay
        isVisible
        userDisplayName="Alex Worker"
        tenantDisplayName="Acme Logistics"
        pendingActions={[queuedAction]}
        onOpenOfflineQueue={vi.fn()}
        onReauthenticate={vi.fn()}
        onDiscardQueuedWorkAndSignOut={vi.fn()}
        onStaySignedIn={vi.fn()}
      />,
    )

    expect(screen.getByText('Session locked')).toBeInTheDocument()
    expect(screen.getByText('Queued work still needs attention')).toBeInTheDocument()
    expect(screen.getByText('TrainArr task')).toBeInTheDocument()
    expect(screen.queryByText('trainarr:1')).not.toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Review offline queue' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Discard queued work and sign out' })).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Return to sign in' })).not.toBeInTheDocument()
  })
})
