import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { IntegrationEventSettingsPanel } from './IntegrationEventSettingsPanel'

const upsertIntegrationEventSettings = vi.fn()

vi.mock('../api/client', () => ({
  getIntegrationEventSettings: vi.fn().mockResolvedValue({
    isEnabled: true,
    maxAttempts: 5,
    retryIntervalMinutes: 15,
    updatedAt: null,
  }),
  listIntegrationOutboxEvents: vi.fn().mockResolvedValue({ items: [] }),
  upsertIntegrationEventSettings: (...args: unknown[]) => upsertIntegrationEventSettings(...args),
  RoutArrApiError: class RoutArrApiError extends Error {
    constructor(
      message: string,
      readonly status: number,
      readonly body: string,
    ) {
      super(message)
      this.name = 'RoutArrApiError'
    }
  },
}))

describe('IntegrationEventSettingsPanel', () => {
  afterEach(() => {
    cleanup()
    upsertIntegrationEventSettings.mockReset()
  })

  it('renders settings for authorized users', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <IntegrationEventSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('integration-event-settings-panel')).toBeTruthy()
    expect(screen.getByTestId('integration-event-settings-enabled')).toBeTruthy()
    expect(screen.getByTestId('integration-event-max-attempts')).toBeTruthy()
    expect(screen.getByTestId('integration-event-retry-interval')).toBeTruthy()
    expect(screen.getByRole('button', { name: /Save integration event settings/i })).toBeTruthy()
  })

  it('returns null when user cannot manage settings', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { container } = render(
      <QueryClientProvider client={client}>
        <IntegrationEventSettingsPanel accessToken="token" canManage={false} />
      </QueryClientProvider>,
    )

    expect(container.firstChild).toBeNull()
  })

  it('renders recent outbox rows when events exist', async () => {
    const outboxEventId = '33333333-3333-3333-3333-333333333301'
    const relatedEntityId = '44444444-4444-4444-4444-444444444401'
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { listIntegrationOutboxEvents } = await import('../api/client')
    vi.mocked(listIntegrationOutboxEvents).mockResolvedValueOnce({
      items: [
        {
          outboxEventId,
          eventKind: 'trip.dispatched',
          processingStatus: 'processed',
          relatedEntityType: 'trip',
          relatedEntityId,
          attemptCount: 1,
          errorMessage: null,
          createdAt: '2026-05-29T00:00:00Z',
          processedAt: '2026-05-29T00:01:00Z',
        },
      ],
    })

    render(
      <QueryClientProvider client={client}>
        <IntegrationEventSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('integration-outbox-list')).toBeTruthy()
    expect(screen.getByTestId(`integration-outbox-row-${outboxEventId}`)).toBeTruthy()
    expect(screen.getByText('trip.dispatched')).toBeTruthy()
    expect(screen.getByText(/trip 44444444-4444-4444-4444-444444444401/i)).toBeTruthy()
  })

  it('saves integration event settings', async () => {
    upsertIntegrationEventSettings.mockResolvedValue({
      isEnabled: false,
      maxAttempts: 3,
      retryIntervalMinutes: 30,
      updatedAt: '2026-05-29T00:00:00Z',
    })

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <IntegrationEventSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    const enabled = await screen.findByTestId('integration-event-settings-enabled')
    await waitFor(() => expect((enabled as HTMLInputElement).checked).toBe(true))
    fireEvent.click(enabled)
    fireEvent.change(screen.getByTestId('integration-event-max-attempts'), { target: { value: '3' } })
    fireEvent.change(screen.getByTestId('integration-event-retry-interval'), { target: { value: '30' } })
    fireEvent.click(screen.getByTestId('integration-event-settings-save'))

    await waitFor(() =>
      expect(upsertIntegrationEventSettings).toHaveBeenCalledWith('token', {
        isEnabled: false,
        maxAttempts: 3,
        retryIntervalMinutes: 30,
      }),
    )
  })

  it('shows validation error for invalid max attempts', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <IntegrationEventSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    const maxAttempts = await screen.findByTestId('integration-event-max-attempts')
    await waitFor(() => expect((maxAttempts as HTMLInputElement).value).toBe('5'))
    fireEvent.change(maxAttempts, { target: { value: '99' } })
    fireEvent.click(screen.getByTestId('integration-event-settings-save'))

    expect(await screen.findByTestId('integration-event-settings-validation-error')).toBeTruthy()
    expect(upsertIntegrationEventSettings).not.toHaveBeenCalled()
  })
})
