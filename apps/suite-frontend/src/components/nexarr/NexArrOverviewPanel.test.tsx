import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import type { MeResponse } from '../../api/types'
import * as nexarr from '../../api/nexarrClient'
import { NexArrOverviewPanel } from './NexArrOverviewPanel'

const baseMe: MeResponse = {
  userId: 'user-1',
  email: 'alex@example.com',
  displayName: 'Alex Operator',
  isPlatformAdmin: false,
  tenantId: 'tenant-a',
  tenantSlug: 'alpha',
  tenantDisplayName: 'Alpha Corp',
  entitlements: ['nexarr', 'staffarr'],
}

let mockMe: MeResponse | undefined = baseMe

vi.mock('../../auth/AuthProvider', () => ({
  useAuth: () => ({ me: mockMe }),
}))

vi.mock('../../api/nexarrClient', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../../api/nexarrClient')>()
  return {
    ...actual,
    getMyEntitlements: vi.fn(),
    getMyTenants: vi.fn(),
    getNavigation: vi.fn(),
    getMySessions: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <NexArrOverviewPanel />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('NexArrOverviewPanel', () => {
  beforeEach(() => {
    mockMe = baseMe
    vi.mocked(nexarr.getMyEntitlements).mockResolvedValue([
      { productKey: 'nexarr', displayName: 'NexArr', status: 'active' },
      { productKey: 'staffarr', displayName: 'StaffArr', status: 'active' },
    ])
    vi.mocked(nexarr.getMyTenants).mockResolvedValue([
      {
        tenantId: 'tenant-a',
        slug: 'alpha',
        displayName: 'Alpha Corp',
        status: 'active',
        roleKey: 'tenant_admin',
      },
    ])
    vi.mocked(nexarr.getNavigation).mockResolvedValue({
      tenantId: 'tenant-a',
      products: [
        {
          productKey: 'nexarr',
          displayName: 'NexArr',
          routePath: '/app/nexarr',
          sortOrder: 0,
          surfaces: [
            {
              surfaceKey: 'overview',
              label: 'Overview',
              relativePath: '',
              iconKey: 'dashboard',
              sortOrder: 0,
              isEnabled: true,
              permissionHint: null,
            },
            {
              surfaceKey: 'identity',
              label: 'Identity & access',
              relativePath: 'identity',
              iconKey: 'auth',
              sortOrder: 10,
              isEnabled: true,
              permissionHint: null,
            },
          ],
        },
      ],
    })
    vi.mocked(nexarr.getMySessions).mockResolvedValue({
      sessions: [
        {
          sessionId: 'session-current',
          createdAt: '2026-05-01T00:00:00Z',
          expiresAt: '2026-06-01T00:00:00Z',
          revokedAt: null,
          userAgent: 'Test browser',
          ipAddress: '127.0.0.1',
          activeTenantId: 'tenant-a',
          isCurrent: true,
          isActive: true,
        },
      ],
    })
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders platform overview from NexArr APIs', async () => {
    renderPanel()

    await waitFor(() => {
      expect(screen.getByTestId('nexarr-overview-panel')).toBeInTheDocument()
    })

    expect(screen.getByText('Alex Operator')).toBeInTheDocument()
    expect(screen.getByText('Alpha Corp')).toBeInTheDocument()
    expect(screen.getByText('StaffArr')).toBeInTheDocument()
    expect(screen.getByText('1')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Identity & access' })).toHaveAttribute(
      'href',
      '/app/nexarr/identity',
    )
    expect(screen.getByRole('link', { name: 'Manage identity & sessions' })).toHaveAttribute(
      'href',
      '/app/nexarr/identity',
    )
  })

  it('shows platform admin shortcut when user is platform administrator', async () => {
    mockMe = { ...baseMe, isPlatformAdmin: true }

    renderPanel()

    await waitFor(() => {
      expect(screen.getByRole('link', { name: 'Open platform admin' })).toHaveAttribute(
        'href',
        '/app/platform-admin',
      )
    })
  })

  it('shows empty entitlements state', async () => {
    vi.mocked(nexarr.getMyEntitlements).mockResolvedValue([])

    renderPanel()

    await waitFor(() => {
      expect(
        screen.getByText('No active product entitlements for this workspace.'),
      ).toBeInTheDocument()
    })
  })

  it('shows API callout when overview queries fail', async () => {
    vi.mocked(nexarr.getMyEntitlements).mockRejectedValueOnce(new Error('overview unavailable'))

    renderPanel()

    expect(await screen.findByText('overview unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry overview' })).toBeInTheDocument()
  })
})
