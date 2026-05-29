import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { NotificationSettingsPanel } from './NotificationSettingsPanel'

const upsertDispatchNotificationSettings = vi.fn()

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
  upsertDispatchNotificationSettings: (...args: unknown[]) =>
    upsertDispatchNotificationSettings(...args),
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

describe('NotificationSettingsPanel', () => {
  afterEach(() => {
    cleanup()
    upsertDispatchNotificationSettings.mockReset()
  })

  it('renders settings for authorized users', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <NotificationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('notification-settings-panel')).toBeTruthy()
    expect(screen.getByTestId('notification-settings-enabled')).toBeTruthy()
    expect(screen.getByTestId('notification-settings-webhook')).toBeTruthy()
    expect(screen.getByRole('button', { name: /Save notification settings/i })).toBeTruthy()
  })

  it('renders recent dispatch rows when dispatches exist', async () => {
    const tripId = '11111111-1111-1111-1111-111111111101'
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { getDispatchNotificationDispatches } = await import('../api/client')
    vi.mocked(getDispatchNotificationDispatches).mockResolvedValueOnce({
      items: [
        {
          notificationId: '22222222-2222-2222-2222-222222222201',
          eventKind: 'trip_dispatched',
          dispatchStatus: 'pending',
          tripId,
          driverPersonId: null,
          relatedEntityType: 'trip',
          relatedEntityId: tripId,
          webhookHost: 'hooks.example.com',
          httpStatusCode: null,
          errorMessage: null,
          createdAt: '2026-05-28T00:00:00Z',
          dispatchedAt: null,
        },
      ],
    })

    render(
      <QueryClientProvider client={client}>
        <NotificationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('notification-dispatches-list')).toBeTruthy()
    expect(screen.getByTestId(`notification-dispatch-row-${tripId}`)).toBeTruthy()
    expect(screen.getByText('trip_dispatched')).toBeTruthy()
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

  it('shows required webhook validation when enabled with empty URL', async () => {
    const { getDispatchNotificationSettings } = await import('../api/client')
    vi.mocked(getDispatchNotificationSettings).mockResolvedValueOnce({
      isEnabled: true,
      notificationWebhookUrl: '',
      notifyOnTripAssigned: true,
      notifyOnTripDispatched: true,
      notifyOnTripInProgress: true,
      notifyOnTripCompleted: true,
      notifyOnTripCancelled: true,
      updatedAt: null,
    })

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <NotificationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('notification-settings-panel')
    await waitFor(() => {
      expect(screen.getByTestId('notification-settings-enabled')).toBeChecked()
    })
    fireEvent.click(screen.getByTestId('notification-settings-save'))

    expect(await screen.findByTestId('notification-settings-webhook-error')).toHaveTextContent(
      /required when dispatch notifications are enabled/i,
    )
    expect(upsertDispatchNotificationSettings).not.toHaveBeenCalled()
  })

  it('shows invalid webhook validation for malformed URL', async () => {
    const { getDispatchNotificationSettings } = await import('../api/client')
    vi.mocked(getDispatchNotificationSettings).mockResolvedValueOnce({
      isEnabled: true,
      notificationWebhookUrl: '',
      notifyOnTripAssigned: true,
      notifyOnTripDispatched: true,
      notifyOnTripInProgress: true,
      notifyOnTripCompleted: true,
      notifyOnTripCancelled: true,
      updatedAt: null,
    })

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <NotificationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('notification-settings-panel')
    await waitFor(() => {
      expect(screen.getByTestId('notification-settings-enabled')).toBeChecked()
    })
    fireEvent.input(screen.getByTestId('notification-settings-webhook'), {
      target: { value: 'not-a-valid-url' },
    })
    fireEvent.click(screen.getByTestId('notification-settings-save'))

    expect(await screen.findByTestId('notification-settings-webhook-error')).toHaveTextContent(
      /absolute URL/i,
    )
    expect(upsertDispatchNotificationSettings).not.toHaveBeenCalled()
  })

  it('clears webhook validation when disabling notifications', async () => {
    const { getDispatchNotificationSettings } = await import('../api/client')
    vi.mocked(getDispatchNotificationSettings).mockResolvedValueOnce({
      isEnabled: false,
      notificationWebhookUrl: null,
      notifyOnTripAssigned: true,
      notifyOnTripDispatched: true,
      notifyOnTripInProgress: true,
      notifyOnTripCompleted: true,
      notifyOnTripCancelled: true,
      updatedAt: null,
    })

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <NotificationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('notification-settings-panel')
    await waitFor(() => {
      expect(screen.getByTestId('notification-settings-enabled')).not.toBeChecked()
    })

    fireEvent.click(screen.getByTestId('notification-settings-enabled'))
    fireEvent.click(screen.getByTestId('notification-settings-save'))
    expect(await screen.findByTestId('notification-settings-webhook-error')).toBeTruthy()

    fireEvent.click(screen.getByTestId('notification-settings-enabled'))

    await waitFor(() => {
      expect(screen.queryByTestId('notification-settings-webhook-error')).toBeNull()
    })
    expect(upsertDispatchNotificationSettings).not.toHaveBeenCalled()
  })

  it('persists saved webhook when disabling and saving with empty field', async () => {
    const savedWebhook = 'https://hooks.example.com/routarr-e2e-w293'
    const { getDispatchNotificationSettings } = await import('../api/client')
    vi.mocked(getDispatchNotificationSettings).mockResolvedValueOnce({
      isEnabled: true,
      notificationWebhookUrl: savedWebhook,
      notifyOnTripAssigned: true,
      notifyOnTripDispatched: true,
      notifyOnTripInProgress: true,
      notifyOnTripCompleted: true,
      notifyOnTripCancelled: true,
      updatedAt: null,
    })
    upsertDispatchNotificationSettings.mockResolvedValueOnce({
      isEnabled: false,
      notificationWebhookUrl: savedWebhook,
      notifyOnTripAssigned: true,
      notifyOnTripDispatched: true,
      notifyOnTripInProgress: true,
      notifyOnTripCompleted: true,
      notifyOnTripCancelled: true,
      updatedAt: '2026-05-28T00:00:00Z',
    })

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <NotificationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('notification-settings-panel')
    await waitFor(() => {
      expect(screen.getByTestId('notification-settings-webhook')).toHaveValue(savedWebhook)
    })

    fireEvent.input(screen.getByTestId('notification-settings-webhook'), {
      target: { value: '' },
    })
    fireEvent.click(screen.getByTestId('notification-settings-enabled'))
    fireEvent.click(screen.getByTestId('notification-settings-save'))

    await waitFor(() => {
      expect(upsertDispatchNotificationSettings).toHaveBeenCalledWith('token', {
        isEnabled: false,
        notificationWebhookUrl: null,
        notifyOnTripAssigned: true,
        notifyOnTripDispatched: true,
        notifyOnTripInProgress: true,
        notifyOnTripCompleted: true,
        notifyOnTripCancelled: true,
      })
    })
  })

  it('restores saved webhook from API when re-enabling notifications', async () => {
    const savedWebhook = 'https://hooks.example.com/routarr-e2e-w292'
    const { getDispatchNotificationSettings } = await import('../api/client')
    vi.mocked(getDispatchNotificationSettings).mockResolvedValueOnce({
      isEnabled: true,
      notificationWebhookUrl: savedWebhook,
      notifyOnTripAssigned: true,
      notifyOnTripDispatched: true,
      notifyOnTripInProgress: true,
      notifyOnTripCompleted: true,
      notifyOnTripCancelled: true,
      updatedAt: null,
    })

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <NotificationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('notification-settings-panel')
    await waitFor(() => {
      expect(screen.getByTestId('notification-settings-webhook')).toHaveValue(savedWebhook)
    })

    fireEvent.input(screen.getByTestId('notification-settings-webhook'), {
      target: { value: '' },
    })
    fireEvent.click(screen.getByTestId('notification-settings-save'))
    expect(await screen.findByTestId('notification-settings-webhook-error')).toBeTruthy()

    fireEvent.click(screen.getByTestId('notification-settings-enabled'))
    await waitFor(() => {
      expect(screen.queryByTestId('notification-settings-webhook-error')).toBeNull()
    })

    fireEvent.click(screen.getByTestId('notification-settings-enabled'))

    await waitFor(() => {
      expect(screen.getByTestId('notification-settings-enabled')).toBeChecked()
      expect(screen.getByTestId('notification-settings-webhook')).toHaveValue(savedWebhook)
    })
    expect(screen.queryByTestId('notification-settings-webhook-error')).toBeNull()
    expect(upsertDispatchNotificationSettings).not.toHaveBeenCalled()
  })

  it('requires new webhook URL when re-enabling after explicit clear removed saved URL', async () => {
    const { getDispatchNotificationSettings } = await import('../api/client')
    vi.mocked(getDispatchNotificationSettings).mockResolvedValueOnce({
      isEnabled: false,
      notificationWebhookUrl: null,
      notifyOnTripAssigned: true,
      notifyOnTripDispatched: true,
      notifyOnTripInProgress: true,
      notifyOnTripCompleted: true,
      notifyOnTripCancelled: true,
      updatedAt: null,
    })
    upsertDispatchNotificationSettings.mockResolvedValueOnce({
      isEnabled: true,
      notificationWebhookUrl: 'https://hooks.example.com/routarr-e2e-w300-new',
      notifyOnTripAssigned: true,
      notifyOnTripDispatched: true,
      notifyOnTripInProgress: true,
      notifyOnTripCompleted: true,
      notifyOnTripCancelled: true,
      updatedAt: '2026-05-28T00:00:00Z',
    })

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <NotificationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('notification-settings-panel')
    await waitFor(() => {
      expect(screen.getByTestId('notification-settings-enabled')).not.toBeChecked()
      expect(screen.getByTestId('notification-settings-webhook')).toHaveValue('')
    })

    fireEvent.click(screen.getByTestId('notification-settings-enabled'))
    await waitFor(() => {
      expect(screen.getByTestId('notification-settings-enabled')).toBeChecked()
      expect(screen.getByTestId('notification-settings-webhook')).toHaveValue('')
    })

    fireEvent.click(screen.getByTestId('notification-settings-save'))
    expect(await screen.findByTestId('notification-settings-webhook-error')).toBeTruthy()

    fireEvent.input(screen.getByTestId('notification-settings-webhook'), {
      target: { value: 'https://hooks.example.com/routarr-e2e-w300-new' },
    })
    fireEvent.click(screen.getByTestId('notification-settings-save'))

    await waitFor(() => {
      expect(upsertDispatchNotificationSettings).toHaveBeenCalledWith('token', {
        isEnabled: true,
        notificationWebhookUrl: 'https://hooks.example.com/routarr-e2e-w300-new',
        notifyOnTripAssigned: true,
        notifyOnTripDispatched: true,
        notifyOnTripInProgress: true,
        notifyOnTripCompleted: true,
        notifyOnTripCancelled: true,
      })
    })
  })

  it('restores saved webhook from API after disable-save, re-enable save, and toggle off/on', async () => {
    const preservedWebhook = 'https://hooks.example.com/routarr-e2e-w307'
    const { getDispatchNotificationSettings } = await import('../api/client')
    vi.mocked(getDispatchNotificationSettings).mockResolvedValue({
      isEnabled: true,
      notificationWebhookUrl: preservedWebhook,
      notifyOnTripAssigned: true,
      notifyOnTripDispatched: true,
      notifyOnTripInProgress: true,
      notifyOnTripCompleted: true,
      notifyOnTripCancelled: true,
      updatedAt: '2026-05-28T00:00:00Z',
    })

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <NotificationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('notification-settings-panel')
    await waitFor(() => {
      expect(screen.getByTestId('notification-settings-enabled')).toBeChecked()
      expect(screen.getByTestId('notification-settings-webhook')).toHaveValue(preservedWebhook)
    })

    fireEvent.click(screen.getByTestId('notification-settings-enabled'))
    await waitFor(() => {
      expect(screen.getByTestId('notification-settings-enabled')).not.toBeChecked()
    })

    fireEvent.click(screen.getByTestId('notification-settings-enabled'))

    await waitFor(() => {
      expect(screen.getByTestId('notification-settings-enabled')).toBeChecked()
      expect(screen.getByTestId('notification-settings-webhook')).toHaveValue(preservedWebhook)
    })
    expect(upsertDispatchNotificationSettings).not.toHaveBeenCalled()
  })

  it('restores new saved webhook from API after toggle off/on post explicit clear', async () => {
    const originalWebhook = 'https://hooks.example.com/routarr-e2e-w300-original'
    const newWebhook = 'https://hooks.example.com/routarr-e2e-w305-new'
    const { getDispatchNotificationSettings } = await import('../api/client')
    vi.mocked(getDispatchNotificationSettings).mockResolvedValue({
      isEnabled: true,
      notificationWebhookUrl: newWebhook,
      notifyOnTripAssigned: true,
      notifyOnTripDispatched: true,
      notifyOnTripInProgress: true,
      notifyOnTripCompleted: true,
      notifyOnTripCancelled: true,
      updatedAt: '2026-05-28T00:00:00Z',
    })

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <NotificationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('notification-settings-panel')
    await waitFor(() => {
      expect(screen.getByTestId('notification-settings-enabled')).toBeChecked()
      expect(screen.getByTestId('notification-settings-webhook')).toHaveValue(newWebhook)
      expect(screen.getByTestId('notification-settings-webhook')).not.toHaveValue(originalWebhook)
    })

    fireEvent.click(screen.getByTestId('notification-settings-enabled'))
    await waitFor(() => {
      expect(screen.getByTestId('notification-settings-enabled')).not.toBeChecked()
    })

    fireEvent.click(screen.getByTestId('notification-settings-enabled'))

    await waitFor(() => {
      expect(screen.getByTestId('notification-settings-enabled')).toBeChecked()
      expect(screen.getByTestId('notification-settings-webhook')).toHaveValue(newWebhook)
      expect(screen.getByTestId('notification-settings-webhook')).not.toHaveValue(originalWebhook)
    })
    expect(upsertDispatchNotificationSettings).not.toHaveBeenCalled()
  })

  it('clears saved webhook in API when disabling with explicit clear intent', async () => {
    const savedWebhook = 'https://hooks.example.com/routarr-e2e-w298'
    const { getDispatchNotificationSettings } = await import('../api/client')
    vi.mocked(getDispatchNotificationSettings).mockResolvedValueOnce({
      isEnabled: true,
      notificationWebhookUrl: savedWebhook,
      notifyOnTripAssigned: true,
      notifyOnTripDispatched: true,
      notifyOnTripInProgress: true,
      notifyOnTripCompleted: true,
      notifyOnTripCancelled: true,
      updatedAt: null,
    })
    upsertDispatchNotificationSettings.mockResolvedValueOnce({
      isEnabled: false,
      notificationWebhookUrl: null,
      notifyOnTripAssigned: true,
      notifyOnTripDispatched: true,
      notifyOnTripInProgress: true,
      notifyOnTripCompleted: true,
      notifyOnTripCancelled: true,
      updatedAt: '2026-05-28T00:00:00Z',
    })

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <NotificationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('notification-settings-panel')
    await waitFor(() => {
      expect(screen.getByTestId('notification-settings-webhook')).toHaveValue(savedWebhook)
    })

    fireEvent.click(screen.getByTestId('notification-settings-enabled'))
    expect(await screen.findByTestId('notification-settings-clear-webhook-on-disable')).toBeTruthy()
    fireEvent.click(screen.getByTestId('notification-settings-clear-webhook-on-disable'))
    await waitFor(() => {
      expect(screen.getByTestId('notification-settings-webhook')).toHaveValue('')
    })
    fireEvent.click(screen.getByTestId('notification-settings-save'))

    await waitFor(() => {
      expect(upsertDispatchNotificationSettings).toHaveBeenCalledWith('token', {
        isEnabled: false,
        notificationWebhookUrl: null,
        notifyOnTripAssigned: true,
        notifyOnTripDispatched: true,
        notifyOnTripInProgress: true,
        notifyOnTripCompleted: true,
        notifyOnTripCancelled: true,
        clearNotificationWebhookOnDisable: true,
      })
    })
  })

  it('clears webhook validation after editing the field', async () => {
    const { getDispatchNotificationSettings } = await import('../api/client')
    vi.mocked(getDispatchNotificationSettings).mockResolvedValueOnce({
      isEnabled: true,
      notificationWebhookUrl: '',
      notifyOnTripAssigned: true,
      notifyOnTripDispatched: true,
      notifyOnTripInProgress: true,
      notifyOnTripCompleted: true,
      notifyOnTripCancelled: true,
      updatedAt: null,
    })

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <NotificationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('notification-settings-panel')
    await waitFor(() => {
      expect(screen.getByTestId('notification-settings-enabled')).toBeChecked()
    })
    fireEvent.click(screen.getByTestId('notification-settings-save'))
    expect(await screen.findByTestId('notification-settings-webhook-error')).toBeTruthy()

    fireEvent.input(screen.getByTestId('notification-settings-webhook'), {
      target: { value: 'https://hooks.example.com/routarr' },
    })

    await waitFor(() => {
      expect(screen.queryByTestId('notification-settings-webhook-error')).toBeNull()
    })
  })
})
