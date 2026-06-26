import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { afterEach, describe, expect, it, vi } from 'vitest'

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
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

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
    expect(screen.getByText(/product destination status changes enqueue integration events/i)).toBeInTheDocument()
    expect(await screen.findByText('tenant.created')).toBeInTheDocument()
    expect(screen.getByText('2')).toBeInTheDocument()
  })

  it('renders callout when settings fail to load', async () => {
    vi.mocked(nexarr.getPlatformOutboxPublisherSettings).mockRejectedValueOnce(
      new Error('outbox settings unavailable'),
    )
    vi.mocked(nexarr.getPlatformOutboxPublisherStatus).mockResolvedValue({
      asOfUtc: new Date().toISOString(),
      isEnabled: true,
      pendingCount: 0,
      deadLetterCount: 0,
      latestRun: null,
    })
    vi.mocked(nexarr.getPlatformOutboxPublisherRuns).mockResolvedValue({ items: [] })
    vi.mocked(nexarr.getPlatformOutboxEvents).mockResolvedValue({ items: [] })

    renderPanel()

    expect(await screen.findByText('outbox settings unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry settings' })).toBeInTheDocument()
  })

  it('renders retryable callouts when events/runs fail', async () => {
    vi.mocked(nexarr.getPlatformOutboxPublisherSettings).mockResolvedValue({
      isEnabled: true,
      maxRetryAttempts: 5,
      retryIntervalMinutes: 5,
      updatedAt: null,
    })
    vi.mocked(nexarr.getPlatformOutboxPublisherStatus).mockResolvedValue({
      asOfUtc: new Date().toISOString(),
      isEnabled: true,
      pendingCount: 0,
      deadLetterCount: 0,
      latestRun: null,
    })
    vi.mocked(nexarr.getPlatformOutboxPublisherRuns).mockRejectedValueOnce(
      new Error('runs unavailable'),
    )
    vi.mocked(nexarr.getPlatformOutboxEvents).mockRejectedValueOnce(
      new Error('events unavailable'),
    )

    renderPanel()

    expect(await screen.findByText('events unavailable')).toBeInTheDocument()
    expect(await screen.findByText('runs unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry events' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry runs' })).toBeInTheDocument()
  })

  it('surfaces action error when trigger fails', async () => {
    vi.mocked(nexarr.getPlatformOutboxPublisherSettings).mockResolvedValue({
      isEnabled: true,
      maxRetryAttempts: 5,
      retryIntervalMinutes: 5,
      updatedAt: null,
    })
    vi.mocked(nexarr.getPlatformOutboxPublisherStatus).mockResolvedValue({
      asOfUtc: new Date().toISOString(),
      isEnabled: true,
      pendingCount: 0,
      deadLetterCount: 0,
      latestRun: null,
    })
    vi.mocked(nexarr.getPlatformOutboxPublisherRuns).mockResolvedValue({ items: [] })
    vi.mocked(nexarr.getPlatformOutboxEvents).mockResolvedValue({ items: [] })
    vi.mocked(nexarr.triggerPlatformOutboxPublisher).mockRejectedValueOnce(
      new Error('trigger failed'),
    )

    renderPanel()

    fireEvent.click(await screen.findByTestId('platform-outbox-trigger'))

    await waitFor(() => {
      expect(screen.getByText('trigger failed')).toBeInTheDocument()
    })
  })
})
