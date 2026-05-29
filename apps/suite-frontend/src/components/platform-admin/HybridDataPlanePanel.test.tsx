import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { HybridDataPlanePanel } from './HybridDataPlanePanel'

vi.mock('../../api/nexarrClient', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../../api/nexarrClient')>()
  return {
    ...actual,
    getPlatformAdminTenantOverview: vi.fn(),
    getPlatformAdminProductOverview: vi.fn(),
    listDataPlaneProfiles: vi.fn(),
    listEffectiveDataPlaneProfiles: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <HybridDataPlanePanel />
    </QueryClientProvider>,
  )
}

const tenantId = '11111111-1111-1111-1111-111111111101'

describe('HybridDataPlanePanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
    vi.useRealTimers()
  })

  it('shows effective deployment map for selected tenant', async () => {
    const user = userEvent.setup()
    vi.mocked(nexarr.getPlatformAdminTenantOverview).mockResolvedValue({
      items: [
        {
          tenantId,
          slug: 'demo',
          displayName: 'Demo Tenant',
          status: 'Active',
          activeEntitlementCount: 1,
          membershipCount: 2,
          createdAt: '2026-01-01T00:00:00Z',
        },
      ],
      page: 1,
      pageSize: 100,
      totalCount: 1,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getPlatformAdminProductOverview).mockResolvedValue([
      {
        productKey: 'staffarr',
        displayName: 'StaffArr',
        isActive: true,
        activeEntitlementCount: 1,
        hasLaunchProfile: true,
        launchProfileActive: true,
        baseUrl: 'http://localhost:5175',
      },
    ])
    vi.mocked(nexarr.listDataPlaneProfiles).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 100,
      totalCount: 0,
      hasNextPage: false,
    })
    vi.mocked(nexarr.listEffectiveDataPlaneProfiles).mockResolvedValue([
      {
        tenantId,
        productKey: 'staffarr',
        productDisplayName: 'StaffArr',
        deploymentMode: 'hosted',
        trustStatus: 'trusted',
      },
    ])

    renderPanel()

    await waitFor(() => {
      expect(screen.getByRole('option', { name: /Demo Tenant/ })).toBeInTheDocument()
    })

    await user.selectOptions(screen.getByTestId('data-plane-tenant'), tenantId)

    await waitFor(() => {
      expect(nexarr.listEffectiveDataPlaneProfiles).toHaveBeenCalledWith(tenantId)
    })

    expect(screen.getByTestId('data-plane-effective-section')).toBeInTheDocument()
    expect(screen.getByText('hosted')).toBeInTheDocument()
  })
})
