import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi, beforeEach } from 'vitest'

import * as client from '../api/client'
import { EventProcessingSettingsPanel } from './EventProcessingSettingsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getEventProcessingSettings: vi.fn(),
    upsertEventProcessingSettings: vi.fn(),
    getTrainingDomainEvents: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <EventProcessingSettingsPanel accessToken="token" canManage />
    </QueryClientProvider>,
  )
}

describe('EventProcessingSettingsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  beforeEach(() => {
    vi.mocked(client.getEventProcessingSettings).mockResolvedValue({
      isEnabled: true,
      maxAttempts: 10,
      retryIntervalMinutes: 5,
      updatedAt: null,
    })
    vi.mocked(client.getTrainingDomainEvents).mockResolvedValue({ items: [] })
  })

  it('renders settings and empty events state', async () => {
    renderPanel()
    await waitFor(() => {
      expect(screen.getByTestId('event-processing-enabled')).toBeChecked()
    })
    expect(screen.getByTestId('event-processing-max-attempts')).toHaveValue('10')
    await waitFor(() => {
      expect(screen.getByTestId('event-processing-events-empty')).toBeInTheDocument()
    })
  })

  it('shows retry callout when settings fail to load', async () => {
    vi.mocked(client.getEventProcessingSettings).mockRejectedValue(new Error('settings down'))
    renderPanel()

    expect(await screen.findByText('Event processing settings unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry settings' })).toBeInTheDocument()
  })
})
