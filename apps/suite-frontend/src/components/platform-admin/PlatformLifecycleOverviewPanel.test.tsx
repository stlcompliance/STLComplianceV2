import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { PlatformLifecycleOverviewPanel } from './PlatformLifecycleOverviewPanel'

vi.mock('../../api/nexarrClient', () => ({
  getPlatformLifecycleOverview: vi.fn(),
}))

import * as nexarr from '../../api/nexarrClient'

function renderPanel() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <MemoryRouter>
        <PlatformLifecycleOverviewPanel />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('PlatformLifecycleOverviewPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders worker cards from overview', async () => {
    vi.mocked(nexarr.getPlatformLifecycleOverview).mockResolvedValue({
      generatedAt: new Date().toISOString(),
      workers: [
        {
          workerKey: 'service_token_cleanup',
          label: 'Service token cleanup',
          description: 'Purges tokens',
          isEnabled: true,
          pendingCount: 2,
          latestRun: null,
          serviceTokenScope: 'nexarr.service_tokens.cleanup.purge',
          platformSettingsPath: '/api/platform-admin/service-token-cleanup/settings',
          suiteAdminPath: '/app/platform-admin/service-tokens',
        },
      ],
    })

    renderPanel()
    expect(await screen.findByText('Service token cleanup')).toBeTruthy()
    expect(screen.getByText('Pending')).toBeTruthy()
    expect(screen.getByRole('link', { name: /Open settings/i })).toBeTruthy()
  })

  it('renders callout when overview fails to load', async () => {
    vi.mocked(nexarr.getPlatformLifecycleOverview).mockRejectedValueOnce(new Error('overview offline'))

    renderPanel()

    expect(await screen.findByText('overview offline')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry overview' })).toBeTruthy()
  })
})
