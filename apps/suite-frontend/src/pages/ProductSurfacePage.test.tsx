import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../api/nexarrClient'
import { NexarrApiError } from '../api/types'
import { ProductSurfacePage } from './ProductSurfacePage'

vi.mock('../auth/AuthProvider', () => ({
  useAuth: () => ({
    me: {
      tenantId: 'tenant-1',
      entitlements: ['staffarr'],
    },
  }),
}))

vi.mock('../hooks/useLaunchContextGate', () => ({
  useLaunchContextGate: () => ({ data: true }),
}))

vi.mock('../hooks/useProductLaunch', () => ({
  useProductLaunch: () => ({
    isPending: false,
    mutate: vi.fn(),
    isError: true,
    error: new Error('launch failed'),
  }),
}))

vi.mock('../api/nexarrClient', () => ({
  getNavigation: vi.fn(),
  getLaunchContext: vi.fn(),
}))

function renderPage(path = '/products/staffarr/surfaces/launch') {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <MemoryRouter initialEntries={[path]}>
        <Routes>
          <Route path="/products/:productKey/surfaces/:surfaceKey" element={<ProductSurfacePage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('ProductSurfacePage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
    vi.unstubAllGlobals()
  })

  it('shows launch mutation errors in callout', async () => {
    vi.mocked(nexarr.getNavigation).mockResolvedValue({
      tenantId: 'tenant-1',
      products: [
        {
          productKey: 'staffarr',
          displayName: 'StaffArr',
          routePath: '/products/staffarr',
          sortOrder: 1,
          surfaces: [
            {
              surfaceKey: 'launch',
              label: 'Launch',
              relativePath: 'launch',
              iconKey: 'rocket',
              sortOrder: 1,
              isEnabled: true,
              permissionHint: null,
            },
          ],
        },
      ],
    })
    vi.mocked(nexarr.getLaunchContext).mockResolvedValue({
      tenantId: 'tenant-1',
      tenantSlug: 'tenant-1',
      tenantDisplayName: 'Tenant 1',
      userId: 'user-1',
      userEmail: 'user1@example.test',
      productKey: 'staffarr',
      productDisplayName: 'StaffArr',
      baseLaunchUrl: 'https://example.test',
      canLaunch: true,
      launchUrl: 'https://example.test/launch',
      denialReasonCode: null,
    })

    renderPage()

    expect(await screen.findByText('launch failed')).toBeTruthy()
  })

  it('redirects to login when the launch-context session has expired', async () => {
    const assign = vi.fn()
    vi.stubGlobal('location', {
      href: 'https://suite.example.com/app/products/staffarr/surfaces/launch',
      assign,
    })
    vi.mocked(nexarr.getNavigation).mockResolvedValue({
      tenantId: 'tenant-1',
      products: [
        {
          productKey: 'staffarr',
          displayName: 'StaffArr',
          routePath: '/products/staffarr',
          sortOrder: 1,
          surfaces: [
            {
              surfaceKey: 'launch',
              label: 'Launch',
              relativePath: 'launch',
              iconKey: 'rocket',
              sortOrder: 1,
              isEnabled: true,
              permissionHint: null,
            },
          ],
        },
      ],
    })
    vi.mocked(nexarr.getLaunchContext).mockRejectedValueOnce(
      new NexarrApiError(401, 'Unauthorized'),
    )

    renderPage()

    await waitFor(() => {
      expect(assign).toHaveBeenCalledWith(
        'https://suite.example.com/login?productKey=staffarr&callbackUrl=https%3A%2F%2Fsuite.example.com%2Fapp%2Fproducts%2Fstaffarr%2Fsurfaces%2Flaunch',
      )
    })
  })
})
