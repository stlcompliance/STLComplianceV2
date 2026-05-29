import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { SettingsSection } from './SettingsSection'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

vi.mock('../../components/NotificationSettingsPanel', () => ({
  NotificationSettingsPanel: () => <div data-testid="notification-settings-panel" />,
}))

vi.mock('../../components/PriceSnapshotSettingsPanel', () => ({
  PriceSnapshotSettingsPanel: () => <div data-testid="price-snapshot-settings-panel" />,
}))

vi.mock('../../components/LeadTimeSnapshotSettingsPanel', () => ({
  LeadTimeSnapshotSettingsPanel: () => <div data-testid="lead-time-snapshot-settings-panel" />,
}))

vi.mock('../../components/AvailabilitySnapshotSettingsPanel', () => ({
  AvailabilitySnapshotSettingsPanel: () => (
    <div data-testid="availability-snapshot-settings-panel" />
  ),
}))

vi.mock('../../components/ProcurementCoordinationSettingsPanel', () => ({
  ProcurementCoordinationSettingsPanel: () => (
    <div data-testid="procurement-coordination-settings-panel" />
  ),
}))

vi.mock('../../components/ApprovalReminderSettingsPanel', () => ({
  ApprovalReminderSettingsPanel: () => <div data-testid="approval-reminder-settings-panel" />,
}))

vi.mock('../../components/ProcurementExceptionEscalationSettingsPanel', () => ({
  ProcurementExceptionEscalationSettingsPanel: () => (
    <div data-testid="procurement-exception-escalation-settings-panel" />
  ),
}))

vi.mock('../../components/DemandProcessingSettingsPanel', () => ({
  DemandProcessingSettingsPanel: () => <div data-testid="demand-processing-settings-panel" />,
}))

vi.mock('../../components/IntegrationEventSettingsPanel', () => ({
  IntegrationEventSettingsPanel: () => <div data-testid="integration-event-settings-panel" />,
}))

function buildState(canManageNotifications: boolean): SupplyArrWorkspaceState {
  return {
    accessToken: 'token',
    canManageNotifications,
  } as SupplyArrWorkspaceState
}

describe('SettingsSection', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders admin workspace with all settings panels for supplyarr_admin', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <SettingsSection state={buildState(true)} />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('supplyarr-settings-admin-workspace')).toBeInTheDocument()
    expect(screen.getByTestId('notification-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('price-snapshot-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('lead-time-snapshot-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('availability-snapshot-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('procurement-coordination-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('approval-reminder-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('procurement-exception-escalation-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('demand-processing-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('integration-event-settings-panel')).toBeInTheDocument()
  })

  it('renders permission message for unauthorized tenant members', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <SettingsSection state={buildState(false)} />
      </QueryClientProvider>,
    )

    expect(screen.queryByTestId('supplyarr-settings-admin-workspace')).not.toBeInTheDocument()
    expect(
      screen.getByText('You do not have permission to manage notification settings.'),
    ).toBeInTheDocument()
  })
})
