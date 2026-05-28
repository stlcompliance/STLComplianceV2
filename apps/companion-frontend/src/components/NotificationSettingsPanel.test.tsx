import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { NotificationSettingsPanel } from './NotificationSettingsPanel'

vi.mock('../api/client', () => ({
  getCompanionNotificationSettings: vi.fn().mockResolvedValue({
    isEnabled: false,
    notificationWebhookUrl: null,
    notifyOnHandoffRedeemed: true,
    notifyOnFieldInboxRefreshed: true,
    updatedAt: null,
  }),
  getCompanionNotificationDispatches: vi.fn().mockResolvedValue({ items: [] }),
  upsertCompanionNotificationSettings: vi.fn(),
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
})
