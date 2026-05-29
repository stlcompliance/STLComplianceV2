import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { EntitlementAdminPanel } from './EntitlementAdminPanel'

vi.mock('../../api/nexarrClient', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../../api/nexarrClient')>()
  return {
    ...actual,
    getPlatformAdminTenantOverview: vi.fn(),
    getPlatformAdminProductOverview: vi.fn(),
    listEntitlements: vi.fn(),
    grantEntitlement: vi.fn(),
    revokeEntitlement: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <EntitlementAdminPanel />
    </QueryClientProvider>,
  )
}

const tenantId = '11111111-1111-1111-1111-111111111101'

describe('EntitlementAdminPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('loads entitlements when tenant is selected', async () => {
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
    vi.mocked(nexarr.listEntitlements).mockResolvedValue({
      items: [
        {
          entitlementId: 'ent-1',
          tenantId,
          productKey: 'staffarr',
          productDisplayName: 'StaffArr',
          status: 'Active',
          grantedAt: '2026-05-01T00:00:00Z',
          revokedAt: null,
        },
      ],
      page: 1,
      pageSize: 50,
      totalCount: 1,
      hasNextPage: false,
    })

    renderPanel()

    await waitFor(() => {
      expect(screen.getByRole('option', { name: /Demo Tenant/ })).toBeInTheDocument()
    })

    await user.selectOptions(screen.getByTestId('entitlement-admin-tenant'), tenantId)

    await waitFor(() => {
      expect(nexarr.listEntitlements).toHaveBeenCalledWith(tenantId)
    })

    expect(screen.getByTestId('entitlement-admin-list')).toBeInTheDocument()
    expect(screen.getByTestId('entitlement-revoke-ent-1')).toBeInTheDocument()
  })
})
