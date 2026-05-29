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

    expect(await screen.findByTestId('notification-settings-panel')).toBeTruthy()
    expect(screen.getByTestId('notification-settings-enabled')).toBeTruthy()
    expect(screen.getByTestId('notification-settings-webhook')).toBeTruthy()
    expect(screen.getByTestId('notification-settings-save')).toBeTruthy()
  })

  it('shows dispatch list test ids when dispatches exist', async () => {
    const { getProcurementNotificationDispatches } = await import('../api/client')
    vi.mocked(getProcurementNotificationDispatches).mockResolvedValueOnce({
      items: [
        {
          notificationId: 'n1',
          eventKind: 'procurement_exception_sla_escalation',
          dispatchStatus: 'pending',
          vendorPartyId: null,
          relatedEntityType: 'procurement_exception',
          relatedEntityId: 'exc-1',
          webhookHost: 'hooks.example.com',
          httpStatusCode: null,
          errorMessage: null,
          createdAt: '2026-05-28T00:00:00Z',
          dispatchedAt: null,
        },
      ],
    })

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <NotificationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('notification-dispatches-list')).toBeTruthy()
    expect(screen.getByTestId('notification-dispatch-row-exc-1')).toBeTruthy()
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
