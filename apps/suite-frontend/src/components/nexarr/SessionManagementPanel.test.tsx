import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { ToastProvider } from '../../feedback'
import { SessionManagementPanel } from './SessionManagementPanel'

const logout = vi.fn()

vi.mock('../../auth/AuthProvider', () => ({
  useAuth: () => ({
    logout,
    session: { sessionId: 'session-current' },
  }),
}))

vi.mock('../../api/nexarrClient', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../../api/nexarrClient')>()
  return {
    ...actual,
    getMySessions: vi.fn(),
    revokeMySession: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <SessionManagementPanel />
      </ToastProvider>
    </QueryClientProvider>,
  )
}

describe('SessionManagementPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders active sessions from NexArr API', async () => {
    vi.mocked(nexarr.getMySessions).mockResolvedValue({
      sessions: [
        {
          sessionId: 'session-current',
          createdAt: '2026-05-29T10:00:00Z',
          expiresAt: '2026-06-05T10:00:00Z',
          revokedAt: null,
          userAgent: 'Vitest browser',
          ipAddress: '127.0.0.1',
          activeTenantId: '11111111-1111-1111-1111-111111111101',
          isCurrent: true,
          isActive: true,
          isRemembered: true,
        },
      ],
    })

    renderPanel()

    await waitFor(() => {
      expect(screen.getByText(/Current remembered session/)).toBeInTheDocument()
    })
    expect(screen.getByText('Vitest browser')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Sign out this device' })).toBeInTheDocument()
  })

  it('requires confirmation before revoking a remote session', async () => {
    const user = userEvent.setup()
    vi.mocked(nexarr.getMySessions).mockResolvedValue({
      sessions: [
        {
          sessionId: 'session-remote',
          createdAt: '2026-05-29T10:00:00Z',
          expiresAt: '2026-06-05T10:00:00Z',
          revokedAt: null,
          userAgent: 'Other browser',
          ipAddress: '10.0.0.2',
          activeTenantId: '11111111-1111-1111-1111-111111111101',
          isCurrent: false,
          isActive: true,
          isRemembered: false,
        },
      ],
    })
    vi.mocked(nexarr.revokeMySession).mockResolvedValue(undefined)

    renderPanel()

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Revoke' })).toBeInTheDocument()
    })

    await user.click(screen.getByRole('button', { name: 'Revoke' }))

    expect(screen.getByRole('alertdialog')).toBeInTheDocument()
    expect(nexarr.revokeMySession).not.toHaveBeenCalled()

    await user.click(screen.getByRole('button', { name: 'Revoke session' }))

    await waitFor(() => {
      expect(nexarr.revokeMySession).toHaveBeenCalledWith('session-remote')
    })

    await waitFor(() => {
      expect(screen.getByRole('status')).toHaveTextContent('Session revoked.')
    })
  })

  it('confirms current-device sign out and logs out after revoke', async () => {
    const user = userEvent.setup()
    vi.mocked(nexarr.getMySessions).mockResolvedValue({
      sessions: [
        {
          sessionId: 'session-current',
          createdAt: '2026-05-29T10:00:00Z',
          expiresAt: '2026-06-05T10:00:00Z',
          revokedAt: null,
          userAgent: 'Vitest browser',
          ipAddress: '127.0.0.1',
          activeTenantId: '11111111-1111-1111-1111-111111111101',
          isCurrent: true,
          isActive: true,
          isRemembered: false,
        },
      ],
    })
    vi.mocked(nexarr.revokeMySession).mockResolvedValue(undefined)

    renderPanel()

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Sign out this device' })).toBeInTheDocument()
    })

    await user.click(screen.getByRole('button', { name: 'Sign out this device' }))
    expect(screen.getByRole('alertdialog')).toHaveTextContent('Sign out this device?')

    await user.click(screen.getByRole('button', { name: 'Sign out' }))

    await waitFor(() => {
      expect(logout).toHaveBeenCalledTimes(1)
    })
  })

  it('shows error callout when session list fails to load', async () => {
    vi.mocked(nexarr.getMySessions).mockRejectedValueOnce(new Error('sessions unavailable'))

    renderPanel()

    expect(await screen.findByText('sessions unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry sessions' })).toBeInTheDocument()
  })
})
