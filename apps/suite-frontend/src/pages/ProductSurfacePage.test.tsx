import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../api/nexarrClient'
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
  })

  it('shows launch mutation errors in callout', async () => {
    vi.mocked(nexarr.getNavigation).mockResolvedValue({
      products: [
        {
          productKey: 'staffarr',
          displayName: 'StaffArr',
          surfaces: [
            {
              surfaceKey: 'launch',
              label: 'Launch',
              isEnabled: true,
              launchPath: '/launch',
              relativePath: 'launch',
            },
          ],
        },
      ],
    })
    vi.mocked(nexarr.getLaunchContext).mockResolvedValue({
      productKey: 'staffarr',
      productDisplayName: 'StaffArr',
      canLaunch: true,
      launchUrl: 'https://example.test/launch',
      reasonCode: null,
      reasonDetail: null,
    })

    renderPage()

    expect(await screen.findByText('launch failed')).toBeTruthy()
  })
})
