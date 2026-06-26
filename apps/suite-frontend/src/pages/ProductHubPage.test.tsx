import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../api/nexarrClient'
import { NexarrApiError } from '../api/types'
import { ProductHubPage } from './ProductHubPage'

vi.mock('../auth/AuthProvider', () => ({
  useAuth: () => ({
    me: {
      tenantId: 'tenant-1',
      launchableProductKeys: ['nexarr', 'staffarr'],
    },
  }),
}))

vi.mock('../hooks/useLaunchContextGate', () => ({
  useLaunchContextGate: () => ({ data: true }),
}))

vi.mock('../hooks/useProductLaunch', () => ({
  useProductLaunch: () => ({ isPending: false, mutate: vi.fn() }),
}))

vi.mock('../api/nexarrClient', () => ({
  getLaunchContext: vi.fn(),
}))

function renderPage(path = '/products/staffarr') {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <MemoryRouter initialEntries={[path]}>
        <Routes>
          <Route path="/products/:productKey" element={<ProductHubPage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('ProductHubPage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
    vi.unstubAllGlobals()
  })

  it('shows an availability callout when the product is unknown', async () => {
    renderPage('/products/unknownproduct')

    expect(await screen.findByText('This product is unavailable in the current workspace.')).toBeTruthy()
  })

  it('shows retryable launch-context error callout', async () => {
    vi.mocked(nexarr.getLaunchContext).mockRejectedValueOnce(new Error('launch context unavailable'))

    renderPage('/products/staffarr')

    expect(await screen.findByText('launch context unavailable')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry launch details' })).toBeTruthy()
  })

  it('redirects to login when the launch context session has expired', async () => {
    const assign = vi.fn()
    vi.stubGlobal('location', {
      href: 'https://suite.example.com/app/products/staffarr',
      assign,
    })
    vi.mocked(nexarr.getLaunchContext).mockRejectedValueOnce(
      new NexarrApiError(401, 'Unauthorized'),
    )

    renderPage('/products/staffarr')

    await waitFor(() => {
      expect(assign).toHaveBeenCalledWith(
        'https://suite.example.com/login?productKey=staffarr&callbackUrl=https%3A%2F%2Fsuite.example.com%2Fapp%2Fproducts%2Fstaffarr',
      )
    })
  })

  it('shows friendly launch denial guidance without raw codes', async () => {
    vi.mocked(nexarr.getLaunchContext).mockResolvedValue({
      tenantId: 'tenant-1',
      tenantSlug: 'tenant-1',
      tenantDisplayName: 'Tenant 1',
      userId: 'user-1',
      userEmail: 'user1@example.test',
      productKey: 'staffarr',
      productDisplayName: 'StaffArr',
      baseLaunchUrl: 'https://example.test',
      canLaunch: false,
      launchUrl: 'https://example.test/launch',
      denialReasonCode: 'launch.product_unavailable',
    })

    renderPage('/products/staffarr')

    expect(await screen.findByText('Product unavailable')).toBeTruthy()
    expect(
      screen.getByText(
        'Confirm your tenant membership, product status, and permissions, then try again.',
      ),
    ).toBeTruthy()
    expect(screen.queryByText(/launch\.product_unavailable/i)).toBeNull()
  })
})
