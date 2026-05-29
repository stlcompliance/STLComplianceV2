import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { ToastProvider } from '../../feedback'
import { IdentityAccessPanel } from './IdentityAccessPanel'

vi.mock('../../auth/AuthProvider', () => ({
  useAuth: () => ({
    me: {
      userId: 'user-1',
      email: 'alex@example.com',
      displayName: 'Alex Operator',
      isPlatformAdmin: false,
      tenantId: 'tenant-a',
      tenantSlug: 'alpha',
      tenantDisplayName: 'Alpha Corp',
      entitlements: ['nexarr'],
    },
    logout: vi.fn(),
    session: { sessionId: 'session-current' },
  }),
}))

vi.mock('../../api/nexarrClient', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../../api/nexarrClient')>()
  return {
    ...actual,
    getMyTenants: vi.fn(),
    getMySessions: vi.fn(),
    getMyEntitlements: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <IdentityAccessPanel />
      </ToastProvider>
    </QueryClientProvider>,
  )
}

describe('IdentityAccessPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders profile above session management', async () => {
    vi.mocked(nexarr.getMyTenants).mockResolvedValue([
      {
        tenantId: 'tenant-a',
        slug: 'alpha',
        displayName: 'Alpha Corp',
        status: 'active',
        roleKey: 'tenant_admin',
      },
    ])
    vi.mocked(nexarr.getMySessions).mockResolvedValue({ sessions: [] })
    vi.mocked(nexarr.getMyEntitlements).mockResolvedValue([])

    renderPanel()

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Active sessions' })).toBeInTheDocument()
    })

    const profileHeading = screen.getByRole('heading', { name: 'Profile' })
    const sessionsHeading = screen.getByRole('heading', { name: 'Active sessions' })

    expect(
      profileHeading.compareDocumentPosition(sessionsHeading) & Node.DOCUMENT_POSITION_FOLLOWING,
    ).toBeTruthy()
    expect(screen.getByText('Alex Operator')).toBeInTheDocument()
    expect(screen.getByText('No sessions found.')).toBeInTheDocument()
  })
})
