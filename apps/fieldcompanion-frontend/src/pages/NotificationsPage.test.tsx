import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { NotificationsPage } from './NotificationsPage'

vi.mock('../hooks/useFieldCompanionWorkspace', () => ({
  useFieldCompanionWorkspace: vi.fn(() => ({
    session: {
      accessToken: 'token',
    },
    accessToken: 'token',
    meQuery: {
      data: {
        isPlatformAdmin: true,
        tenantRoleKey: 'tenant_admin',
      },
    },
  })),
}))

vi.mock('../components/NotificationSettingsPanel', () => ({
  NotificationSettingsPanel: ({
    accessToken,
    canManage,
  }: {
    accessToken: string
    canManage: boolean
  }) => (
    <div data-testid="fieldcompanion-notification-settings-panel">
      {accessToken} {canManage ? 'manage' : 'view'}
    </div>
  ),
}))

describe('NotificationsPage', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders the notification settings entry point for administrators', () => {
    render(<NotificationsPage />)

    expect(screen.getByText('Notifications')).toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-notification-settings-panel')).toHaveTextContent(
      'token manage',
    )
    expect(screen.getByTestId('fieldcompanion-device-capability-panel')).toBeInTheDocument()
  })
})
