import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { NotificationSettingsPanel } from './NotificationSettingsPanel'

vi.mock('../api/client', () => ({
  getDispatchNotificationSettings: vi.fn().mockResolvedValue({
    isEnabled: false,
    notificationWebhookUrl: null,
    notifyOnTripAssigned: true,
    notifyOnTripDispatched: true,
    notifyOnTripInProgress: true,
    notifyOnTripCompleted: true,
    notifyOnTripCancelled: true,
    updatedAt: null,
  }),
  getDispatchNotificationDispatches: vi.fn().mockResolvedValue({ items: [] }),
  upsertDispatchNotificationSettings: vi.fn(),
}))

describe('NotificationSettingsPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders settings for authorized users', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <NotificationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/Dispatch notifications/)).toBeTruthy()
    expect(screen.getByRole('button', { name: /Save notification settings/i })).toBeTruthy()
  })

  it('renders nothing for unauthorized users', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { container } = render(
      <QueryClientProvider client={client}>
        <NotificationSettingsPanel accessToken="token" canManage={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })
})
