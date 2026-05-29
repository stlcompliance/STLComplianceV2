import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import type { MeResponse } from '../../api/types'
import * as nexarr from '../../api/nexarrClient'
import { NexArrTenantsPanel } from './NexArrTenantsPanel'

const baseMe: MeResponse = {
  userId: 'user-1',
  email: 'admin@example.com',
  displayName: 'Platform Admin',
  isPlatformAdmin: true,
  tenantId: 'tenant-a',
  tenantSlug: 'alpha',
  tenantDisplayName: 'Alpha Corp',
  entitlements: ['nexarr'],
}

let mockMe: MeResponse | undefined = baseMe

vi.mock('../../auth/AuthProvider', () => ({
  useAuth: () => ({ me: mockMe }),
}))

vi.mock('../../api/nexarrClient', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../../api/nexarrClient')>()
  return {
    ...actual,
    getPlatformAdminTenantOverview: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <NexArrTenantsPanel />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('NexArrTenantsPanel', () => {
  beforeEach(() => {
    mockMe = baseMe
    vi.mocked(nexarr.getPlatformAdminTenantOverview).mockResolvedValue({
      items: [
        {
          tenantId: 'tenant-a',
          slug: 'alpha',
          displayName: 'Alpha Corp',
          status: 'active',
          activeEntitlementCount: 3,
          membershipCount: 12,
          createdAt: '2026-01-15T00:00:00Z',
        },
        {
          tenantId: 'tenant-b',
          slug: 'beta',
          displayName: 'Beta Industries',
          status: 'suspended',
          activeEntitlementCount: 0,
          membershipCount: 4,
          createdAt: '2026-02-20T00:00:00Z',
        },
      ],
      page: 1,
      pageSize: 100,
      totalCount: 2,
      hasNextPage: false,
    })
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders tenant registry from platform admin overview API', async () => {
    renderPanel()

    await waitFor(() => {
      expect(screen.getByTestId('nexarr-tenants-panel')).toBeInTheDocument()
    })

    expect(screen.getByText('Alpha Corp')).toBeInTheDocument()
    expect(screen.getByText('Beta Industries')).toBeInTheDocument()
    expect(screen.getByText('2')).toBeInTheDocument()
    expect(screen.getByText('1')).toBeInTheDocument()
    expect(screen.getByText('3')).toBeInTheDocument()
    expect(screen.getByText('suspended')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /Manage tenants/i })).toHaveAttribute(
      'href',
      '/app/platform-admin/tenants',
    )
    expect(nexarr.getPlatformAdminTenantOverview).toHaveBeenCalledWith(1, 100)
  })

  it('shows empty state when no tenants exist', async () => {
    vi.mocked(nexarr.getPlatformAdminTenantOverview).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 100,
      totalCount: 0,
      hasNextPage: false,
    })

    renderPanel()

    await waitFor(() => {
      expect(screen.getByText('No tenants registered on this platform.')).toBeInTheDocument()
    })
  })

  it('shows API error state', async () => {
    vi.mocked(nexarr.getPlatformAdminTenantOverview).mockRejectedValue(
      new Error('Platform admin access required'),
    )

    renderPanel()

    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveTextContent('Platform admin access required')
    })
  })
})
