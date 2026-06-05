import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { NotificationSettingsPanel } from './NotificationSettingsPanel'

vi.mock('../api/client', () => ({
  getFieldCompanionNotificationSettings: vi.fn().mockResolvedValue({
    isEnabled: false,
    notificationWebhookUrl: null,
    notifyOnHandoffRedeemed: true,
    notifyOnFieldInboxRefreshed: true,
    updatedAt: null,
  }),
  getFieldCompanionNotificationDispatches: vi.fn().mockResolvedValue({ items: [] }),
  upsertFieldCompanionNotificationSettings: vi.fn(),
}))

describe('NotificationSettingsPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders settings for administrators', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <NotificationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/Operational notifications/)).toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-push-readiness-label')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Save notification settings/i })).toBeInTheDocument()
  })

  it('hides panel for non-administrators', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { container } = render(
      <QueryClientProvider client={client}>
        <NotificationSettingsPanel accessToken="token" canManage={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })

  it('shows retryable settings error callout when settings load fails', async () => {
    vi.mocked(client.getFieldCompanionNotificationSettings).mockRejectedValueOnce(
      new Error('settings unavailable'),
    )
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <NotificationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('settings unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry settings' })).toBeInTheDocument()
  })
})
