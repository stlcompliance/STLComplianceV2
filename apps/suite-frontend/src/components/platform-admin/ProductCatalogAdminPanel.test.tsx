import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { ProductCatalogAdminPanel } from './ProductCatalogAdminPanel'

vi.mock('../../api/nexarrClient', () => ({
  listProducts: vi.fn(),
  createProduct: vi.fn(),
  updateProduct: vi.fn(),
}))

function renderPanel() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <ProductCatalogAdminPanel />
    </QueryClientProvider>,
  )
}

describe('ProductCatalogAdminPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows retryable callout when products query fails', async () => {
    vi.mocked(nexarr.listProducts).mockRejectedValueOnce(new Error('products unavailable'))

    renderPanel()

    expect(await screen.findByText('products unavailable')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry products' })).toBeTruthy()
  })
})
