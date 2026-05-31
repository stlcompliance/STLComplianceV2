import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../api/nexarrClient'
import { ProductHubPage } from './ProductHubPage'

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
  })

  it('shows entitlement callout when user cannot access product', async () => {
    renderPage('/products/unknownproduct')

    expect(await screen.findByText('You are not entitled to this product.')).toBeTruthy()
  })

  it('shows retryable launch-context error callout', async () => {
    vi.mocked(nexarr.getLaunchContext).mockRejectedValueOnce(new Error('launch context unavailable'))

    renderPage('/products/staffarr')

    expect(await screen.findByText('launch context unavailable')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry launch context' })).toBeTruthy()
  })
})
