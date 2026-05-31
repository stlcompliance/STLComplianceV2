import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { NotificationSettingsPanel } from './NotificationSettingsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getTrainingNotificationSettings: vi.fn(),
    getTrainingNotificationDispatches: vi.fn(),
    upsertTrainingNotificationSettings: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <NotificationSettingsPanel accessToken="token" canManage />
    </QueryClientProvider>,
  )
}

describe('NotificationSettingsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders settings and empty dispatch list', async () => {
    vi.mocked(client.getTrainingNotificationSettings).mockResolvedValue({
      isEnabled: true,
      notificationWebhookUrl: 'https://hooks.example.com/trainarr',
      notifyOnAssignmentCreated: true,
      notifyOnAssignmentCompleted: true,
      notifyOnQualificationExpiring: true,
      notifyOnQualificationIssued: true,
      notifyOnQualificationSuspended: true,
      notifyOnQualificationRevoked: true,
      notifyOnQualificationExpired: true,
      notifyOnAssignmentDueReminder: true,
      notifyOnAssignmentOverdueEscalation: true,
      expiringLeadDays: 30,
      maxAttempts: 10,
      retryIntervalMinutes: 5,
      updatedAt: null,
    })
    vi.mocked(client.getTrainingNotificationDispatches).mockResolvedValue({ items: [] })

    renderPanel()

    await waitFor(() => {
      expect(screen.getByTestId('notification-settings-enabled')).toBeChecked()
    })
    expect(screen.getByTestId('notification-dispatches-empty')).toBeInTheDocument()
  })

  it('shows retry callout when settings fail', async () => {
    vi.mocked(client.getTrainingNotificationSettings).mockRejectedValue(new Error('settings down'))
    vi.mocked(client.getTrainingNotificationDispatches).mockResolvedValue({ items: [] })
    renderPanel()

    expect(await screen.findByText('Notification settings unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry settings' })).toBeInTheDocument()
  })
})
