import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { ToastProvider } from '../../feedback'
import { PlatformJourneySeedsPage } from './PlatformJourneySeedsPage'

vi.mock('../../api/nexarrClient', () => ({
  getPlatformJourneySeedTargets: vi.fn(),
  seedPlatformJourney: vi.fn(),
}))

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={client}>
      <ToastProvider>
        <PlatformJourneySeedsPage />
      </ToastProvider>
    </QueryClientProvider>,
  )
}

describe('PlatformJourneySeedsPage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders journey seed inputs and runs a seed', async () => {
    vi.mocked(nexarr.getPlatformJourneySeedTargets).mockResolvedValue([
      {
        productKey: 'trainarr',
        displayName: 'TrainArr',
        description: 'Seed the load-test qualification, assignment, and publication inputs.',
        seedPath: '/api/load-test-journey/seed',
        baseUrl: 'https://api.example.com/trainarr',
        isConfigured: true,
      },
    ])
    vi.mocked(nexarr.seedPlatformJourney).mockResolvedValue({
      productKey: 'trainarr',
      displayName: 'TrainArr',
      description: 'Seed the load-test qualification, assignment, and publication inputs.',
      seedPath: '/api/load-test-journey/seed',
      baseUrl: 'https://api.example.com/trainarr',
      isConfigured: true,
      succeeded: true,
      statusCode: 200,
      responseBody: '{"ok":true}',
      requestedAt: '2026-06-08T15:00:00Z',
    })

    renderPage()

    expect(await screen.findByText('TrainArr')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Seed journey' }))

    await waitFor(() => {
      expect(nexarr.seedPlatformJourney).toHaveBeenCalledWith('trainarr')
    })

    expect(await screen.findByText(/Last run succeeded/)).toBeInTheDocument()
    expect(screen.getByText('HTTP 200')).toBeInTheDocument()
    expect(screen.getByText('{"ok":true}')).toBeInTheDocument()
  })
})
