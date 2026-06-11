import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { AdminSection } from './AdminSection'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

vi.mock('../../components/PersonExportDeliverySettingsPanel', () => ({
  PersonExportDeliverySettingsPanel: () => <div data-testid="person-export-delivery-settings-panel" />,
}))

vi.mock('../../components/StaffArrScheduledWorkerSettingsPanel', () => ({
  StaffArrScheduledWorkerSettingsPanel: ({ config }: { config: { panelTestId: string } }) => (
    <div data-testid={config.panelTestId} />
  ),
}))

vi.mock('../../components/DataExportsPanel', () => ({
  DataExportsPanel: () => <div data-testid="data-exports-panel" />,
}))

function buildState(canManagePeopleProfiles: boolean): StaffArrWorkspaceState {
  return {
    accessToken: 'token',
    canManagePeopleProfiles,
  } as StaffArrWorkspaceState
}

describe('AdminSection', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders admin workspace with export and worker panels for staffarr_admin', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <AdminSection state={buildState(true)} />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('staffarr-settings-admin-workspace')).toBeTruthy()
    expect(screen.getByTestId('person-export-delivery-settings-panel')).toBeTruthy()
    expect(screen.getByTestId('certification-expiration-settings-panel')).toBeTruthy()
    expect(screen.getByTestId('readiness-rollup-settings-panel')).toBeTruthy()
    expect(screen.getByTestId('permission-projection-settings-panel')).toBeTruthy()
    expect(screen.getByTestId('personnel-history-rollup-settings-panel')).toBeTruthy()
    expect(screen.getByTestId('data-exports-panel')).toBeTruthy()
  })

  it('omits admin workspace when user cannot manage worker settings', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <AdminSection state={buildState(false)} />
      </QueryClientProvider>,
    )

    expect(screen.queryByTestId('staffarr-settings-admin-workspace')).toBeNull()
    expect(screen.queryByTestId('data-exports-panel')).toBeNull()
  })
})
