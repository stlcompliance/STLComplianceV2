import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { NotificationSettingsPanel } from './NotificationSettingsPanel'

vi.mock('../api/client', () => ({
  getProcurementNotificationSettings: vi.fn().mockResolvedValue({
    isEnabled: false,
    notificationWebhookUrl: null,
    notifyOnPurchaseRequestSubmitted: true,
    notifyOnPurchaseRequestApproved: true,
    notifyOnPurchaseOrderIssued: true,
    notifyOnReceivingReceiptPosted: true,
    updatedAt: null,
  }),
  getProcurementNotificationDispatches: vi.fn().mockResolvedValue({ items: [] }),
  upsertProcurementNotificationSettings: vi.fn(),
}))

describe('NotificationSettingsPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders procurement notification settings for admins', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <NotificationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/Procurement notifications/)).toBeTruthy()
    expect(screen.getByRole('button', { name: /Save notification settings/i })).toBeTruthy()
  })

  it('hides panel when user cannot manage settings', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { container } = render(
      <QueryClientProvider client={client}>
        <NotificationSettingsPanel accessToken="token" canManage={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })
})
