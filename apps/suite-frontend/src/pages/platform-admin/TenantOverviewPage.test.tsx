import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { TenantOverviewPage } from './TenantOverviewPage'

vi.mock('../../api/nexarrClient', () => ({
  getPlatformAdminTenantOverview: vi.fn(),
}))

vi.mock('../../components/platform-admin/TenantCatalogAdminPanel', () => ({
  TenantCatalogAdminPanel: () => <div>Tenant catalog admin</div>,
}))

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <TenantOverviewPage />
    </QueryClientProvider>,
  )
}

describe('TenantOverviewPage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows retryable error callout when tenants query fails', async () => {
    vi.mocked(nexarr.getPlatformAdminTenantOverview).mockRejectedValueOnce(
      new Error('tenants unavailable'),
    )

    renderPage()

    expect(await screen.findByText('tenants unavailable')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry tenants' })).toBeTruthy()
  })
})
