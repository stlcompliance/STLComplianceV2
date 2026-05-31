import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { TenantCatalogAdminPanel } from './TenantCatalogAdminPanel'

vi.mock('../../api/nexarrClient', () => ({
  listTenants: vi.fn(),
  createTenant: vi.fn(),
  updateTenant: vi.fn(),
  updateTenantStatus: vi.fn(),
}))

function renderPanel() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <TenantCatalogAdminPanel />
    </QueryClientProvider>,
  )
}

describe('TenantCatalogAdminPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows retryable callout when tenants query fails', async () => {
    vi.mocked(nexarr.listTenants).mockRejectedValueOnce(new Error('tenants unavailable'))

    renderPanel()

    expect(await screen.findByText('tenants unavailable')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry tenants' })).toBeTruthy()
  })
})
