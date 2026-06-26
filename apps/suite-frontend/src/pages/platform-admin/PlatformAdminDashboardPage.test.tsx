import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { PlatformAdminDashboardPage } from './PlatformAdminDashboardPage'

vi.mock('../../api/nexarrClient', () => ({
  getPlatformAdminDashboard: vi.fn(),
}))

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <MemoryRouter>
        <PlatformAdminDashboardPage />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('PlatformAdminDashboardPage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders dashboard stats', async () => {
    vi.mocked(nexarr.getPlatformAdminDashboard).mockResolvedValue({
      generatedAt: new Date().toISOString(),
      activeTenantCount: 4,
      activeProductCount: 6,
      activeLaunchableDestinationCount: 17,
      serviceClientCount: 3,
      activeServiceTokenCount: 11,
      launchProfileCount: 5,
      pendingHandoffCount: 2,
      auditEventsLast24Hours: 20,
      tenantCount: 5,
      productCount: 7,
      totalLaunchableDestinationCount: 21,
      expiredUnredeemedHandoffCount: 1,
    })

    renderPage()

    expect(await screen.findByText('Active launch contexts')).toBeTruthy()
    expect(screen.getByText('4')).toBeTruthy()
    expect(screen.getByText('17')).toBeTruthy()
  })

  it('shows retryable error callout when dashboard query fails', async () => {
    vi.mocked(nexarr.getPlatformAdminDashboard).mockRejectedValueOnce(
      new Error('dashboard unavailable'),
    )

    renderPage()

    expect(await screen.findByText('dashboard unavailable')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry dashboard' })).toBeTruthy()
  })
})
