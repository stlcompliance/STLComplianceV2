import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { ProductOverviewPage } from './ProductOverviewPage'

vi.mock('../../api/nexarrClient', () => ({
  getPlatformAdminProductOverview: vi.fn(),
}))

vi.mock('../../components/platform-admin/ProductCatalogAdminPanel', () => ({
  ProductCatalogAdminPanel: () => <div>Product catalog admin</div>,
}))

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <ProductOverviewPage />
    </QueryClientProvider>,
  )
}

describe('ProductOverviewPage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows retryable error callout when products query fails', async () => {
    vi.mocked(nexarr.getPlatformAdminProductOverview).mockRejectedValueOnce(
      new Error('products unavailable'),
    )

    renderPage()

    expect(await screen.findByText('products unavailable')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry products' })).toBeTruthy()
  })
})
