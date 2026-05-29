import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi } from 'vitest'

import { PlatformOutboxPublisherPanel } from './PlatformOutboxPublisherPanel'

vi.mock('../../api/nexarrClient', () => ({
  getPlatformOutboxPublisherSettings: vi.fn(),
  getPlatformOutboxPublisherStatus: vi.fn(),
  getPlatformOutboxPublisherRuns: vi.fn(),
  getPlatformOutboxEvents: vi.fn(),
  upsertPlatformOutboxPublisherSettings: vi.fn(),
  triggerPlatformOutboxPublisher: vi.fn(),
}))

import * as nexarr from '../../api/nexarrClient'

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <PlatformOutboxPublisherPanel />
    </QueryClientProvider>,
  )
}

describe('PlatformOutboxPublisherPanel', () => {
  it('renders outbox status and recent events', async () => {
    vi.mocked(nexarr.getPlatformOutboxPublisherSettings).mockResolvedValue({
      isEnabled: true,
      maxRetryAttempts: 5,
      retryIntervalMinutes: 5,
      updatedAt: null,
    })
    vi.mocked(nexarr.getPlatformOutboxPublisherStatus).mockResolvedValue({
      asOfUtc: new Date().toISOString(),
      isEnabled: true,
      pendingCount: 2,
      deadLetterCount: 0,
      latestRun: null,
    })
    vi.mocked(nexarr.getPlatformOutboxPublisherRuns).mockResolvedValue({ items: [] })
    vi.mocked(nexarr.getPlatformOutboxEvents).mockResolvedValue({
      items: [
        {
          eventId: 'evt-1',
          eventType: 'tenant.created',
          tenantId: 'tenant-1',
          processingStatus: 'published',
          attemptCount: 1,
          errorMessage: null,
          occurredAt: new Date().toISOString(),
          publishedAt: new Date().toISOString(),
        },
      ],
    })

    renderPanel()

    expect(await screen.findByTestId('platform-outbox-publisher-panel')).toBeInTheDocument()
    expect(await screen.findByText('tenant.created')).toBeInTheDocument()
    expect(screen.getByText('2')).toBeInTheDocument()
  })
})
