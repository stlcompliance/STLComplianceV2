import { QueryClient, QueryClientProvider } from '@tanstack/react-query'

import { cleanup, render, screen, waitFor } from '@testing-library/react'

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'



import type { MeResponse } from '../../api/types'

import * as nexarr from '../../api/nexarrClient'

import { IdentityProfilePanel } from './IdentityProfilePanel'



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

    getMyTenants: vi.fn(),

    getMyEntitlements: vi.fn(),

  }

})



function renderPanel() {

  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

  return render(

    <QueryClientProvider client={queryClient}>

      <IdentityProfilePanel />

    </QueryClientProvider>,

  )

}



describe('IdentityProfilePanel', () => {

  beforeEach(() => {

    mockMe = baseMe

  })



  afterEach(() => {

    cleanup()

    vi.clearAllMocks()

  })



  it('renders profile summary and tenant memberships from NexArr APIs', async () => {

    vi.mocked(nexarr.getMyTenants).mockResolvedValue([

      {

        tenantId: 'tenant-a',

        slug: 'alpha',

        displayName: 'Alpha Corp',

        status: 'active',

        roleKey: 'tenant_admin',

      },

      {

        tenantId: 'tenant-b',

        slug: 'beta',

        displayName: 'Beta LLC',

        status: 'active',

        roleKey: 'member',

      },

    ])

    vi.mocked(nexarr.getMyEntitlements).mockResolvedValue([

      { productKey: 'nexarr', displayName: 'NexArr', status: 'active' },

      { productKey: 'staffarr', displayName: 'StaffArr', status: 'active' },

    ])



    renderPanel()



    expect(screen.getByText('Alex Operator')).toBeInTheDocument()

    expect(screen.getByText('alex@example.com')).toBeInTheDocument()



    await waitFor(() => {

      expect(screen.getByText('Beta LLC')).toBeInTheDocument()

    })



    expect(screen.getByText('Current workspace')).toBeInTheDocument()

    expect(screen.getByText('tenant_admin')).toBeInTheDocument()

    expect(screen.getByText('member')).toBeInTheDocument()

  })



  it('shows product entitlements for the active tenant workspace', async () => {

    vi.mocked(nexarr.getMyTenants).mockResolvedValue([])

    vi.mocked(nexarr.getMyEntitlements).mockResolvedValue([

      { productKey: 'nexarr', displayName: 'NexArr', status: 'active' },

      { productKey: 'staffarr', displayName: 'StaffArr', status: 'active' },

    ])



    renderPanel()



    await waitFor(() => {
      expect(screen.getByText('NexArr')).toBeInTheDocument()
    })

    const entitlementsSection = screen.getByTestId('profile-entitlements-section')
    expect(entitlementsSection).toHaveTextContent('StaffArr')
    expect(entitlementsSection).toHaveTextContent('(alpha)')
    expect(nexarr.getMyEntitlements).toHaveBeenCalledTimes(1)

  })



  it('shows platform administrator badge when applicable', async () => {

    mockMe = { ...baseMe, isPlatformAdmin: true, displayName: 'Platform Admin' }

    vi.mocked(nexarr.getMyTenants).mockResolvedValue([])

    vi.mocked(nexarr.getMyEntitlements).mockResolvedValue([])



    renderPanel()



    await waitFor(() => {

      expect(screen.getByText('Platform administrator')).toBeInTheDocument()

    })

  })

  it('shows retryable API callouts for entitlements and tenant memberships', async () => {
    vi.mocked(nexarr.getMyTenants).mockRejectedValueOnce(new Error('tenant memberships unavailable'))
    vi.mocked(nexarr.getMyEntitlements).mockRejectedValueOnce(new Error('entitlements unavailable'))

    renderPanel()

    expect(await screen.findByText('tenant memberships unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry tenant memberships' })).toBeInTheDocument()
    expect(screen.getByText('entitlements unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry entitlements' })).toBeInTheDocument()
  })

})


