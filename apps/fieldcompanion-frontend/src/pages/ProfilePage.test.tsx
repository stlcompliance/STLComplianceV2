import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { clearSession } from '../auth/sessionStorage'
import { ProfilePage } from './ProfilePage'

vi.mock('../api/client', () => ({
  renewFieldCompanionSession: vi.fn(() => Promise.resolve(null)),
}))

vi.mock('../hooks/useFieldCompanionWorkspace', () => ({
  useFieldCompanionWorkspace: vi.fn(() => ({
    session: {
      accessToken: 'token',
      accessTokenExpiresAt: '2026-07-23T18:00:00Z',
      displayName: 'Alex Worker',
      email: 'alex.worker@example.com',
      personId: 'person-123',
      tenantDisplayName: 'Acme Logistics',
      tenantId: 'tenant-789',
      tenantRoleKey: 'worker',
      tenantSlug: 'acme-logistics',
      userId: 'user-456',
    },
    accessToken: 'token',
    meQuery: { data: { ok: true } },
  })),
}))

vi.mock('../hooks/useOfflineQueue', () => ({
  useOfflineQueue: vi.fn(() => ({
    lastSyncError: null,
    lastSyncedAt: null,
    pendingCount: 0,
  })),
}))

vi.mock('../lib/pushNotifications', () => ({
  getPushPermissionState: vi.fn(() => 'granted'),
  isWebPushSupported: vi.fn(() => true),
  pushReadinessLabel: vi.fn(() => 'Ready'),
}))

vi.mock('../lib/fieldInbox', () => ({
  formatWhen: vi.fn(() => 'formatted'),
}))

const originalLocation = window.location

vi.mock('../auth/sessionStorage', () => ({
  clearSession: vi.fn(),
}))

describe('ProfilePage', () => {
  beforeEach(() => {
    Object.defineProperty(window, 'location', {
      configurable: true,
      value: {
        ...originalLocation,
        assign: vi.fn(),
      },
    })
    vi.mocked(clearSession).mockClear()
  })

  afterEach(() => {
    cleanup()
    vi.mocked(clearSession).mockClear()
    Object.defineProperty(window, 'location', {
      configurable: true,
      value: originalLocation,
    })
  })

  it('keeps raw person and tenant identifiers out of the profile UI', () => {
    render(<ProfilePage />)

    expect(screen.getByText('Alex Worker')).toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-device-capability-panel')).toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-session-status')).toHaveTextContent('Session active')
    expect(screen.queryByText('person-123')).not.toBeInTheDocument()
    expect(screen.queryByText('user-456')).not.toBeInTheDocument()
    expect(screen.queryByText('tenant-789')).not.toBeInTheDocument()
    expect(screen.queryByText('acme-logistics')).not.toBeInTheDocument()

    fireEvent.click(screen.getByText('Advanced session details'))

    expect(screen.getByText('Worker profile')).toBeInTheDocument()
    expect(screen.getByText('Linked to StaffArr')).toBeInTheDocument()
    expect(screen.getByText('Session scope')).toBeInTheDocument()
    expect(screen.queryByText('person-123')).not.toBeInTheDocument()
    expect(screen.queryByText('user-456')).not.toBeInTheDocument()
    expect(screen.queryByText('tenant-789')).not.toBeInTheDocument()
    expect(screen.queryByText('acme-logistics')).not.toBeInTheDocument()
  })

  it('confirms before clearing this device', () => {
    render(<ProfilePage />)

    expect(screen.getByText('Device cleanup')).toBeInTheDocument()
    expect(screen.getByText('Cleanup state')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Clear this device' }))
    expect(screen.getByText('Confirmation pending')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Confirm clear' }))

    expect(clearSession).toHaveBeenCalledTimes(1)
    expect(window.location.assign).toHaveBeenCalledWith('/')
  })

  it('refreshes the current session from the profile summary', async () => {
    const { renewFieldCompanionSession } = await import('../api/client')

    render(<ProfilePage />)

    fireEvent.click(screen.getByRole('button', { name: 'Refresh session' }))

    await waitFor(() => {
      expect(renewFieldCompanionSession).toHaveBeenCalledTimes(1)
    })
  })
})
