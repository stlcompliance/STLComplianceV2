import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { SettingsSection } from './SettingsSection'
import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'

vi.mock('../../components/NotificationSettingsPanel', () => ({
  NotificationSettingsPanel: () => <div data-testid="notification-settings-panel" />,
}))

vi.mock('../../components/TripExecutionSettingsPanel', () => ({
  TripExecutionSettingsPanel: () => <div data-testid="trip-execution-settings-panel" />,
}))

vi.mock('../../components/TripCompletionRollupSettingsPanel', () => ({
  TripCompletionRollupSettingsPanel: () => (
    <div data-testid="trip-completion-rollup-settings-panel" />
  ),
}))

vi.mock('../../components/AttachmentRetentionSettingsPanel', () => ({
  AttachmentRetentionSettingsPanel: () => (
    <div data-testid="attachment-retention-settings-panel" />
  ),
}))

function buildState(roleKey: string, isPlatformAdmin = false): RoutArrWorkspaceState {
  return {
    session: { accessToken: 'token' },
    roleKey,
    isPlatformAdmin,
  } as RoutArrWorkspaceState
}

describe('SettingsSection', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders admin workspace with all settings panels for routarr_admin', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <SettingsSection state={buildState('routarr_admin')} />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('routarr-settings-admin-workspace')).toBeInTheDocument()
    expect(screen.getByTestId('notification-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('trip-execution-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('trip-completion-rollup-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('attachment-retention-settings-panel')).toBeInTheDocument()
  })

  it('renders nothing for unauthorized tenant members', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    const { container } = render(
      <QueryClientProvider client={queryClient}>
        <SettingsSection state={buildState('tenant_member')} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })
})
