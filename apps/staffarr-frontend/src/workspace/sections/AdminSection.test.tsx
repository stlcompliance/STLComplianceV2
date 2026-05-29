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

vi.mock('../../components/AuditPackageExportPanel', () => ({
  AuditPackageExportPanel: () => <div data-testid="staffarr-audit-export-panel" />,
}))

function buildState(canManagePeopleProfiles: boolean, canExportAudit = false): StaffArrWorkspaceState {
  return {
    accessToken: 'token',
    canManagePeopleProfiles,
    canExportAudit,
  } as StaffArrWorkspaceState
}

describe('AdminSection', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders admin workspace with all six product-admin panels for staffarr_admin', () => {
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
    expect(screen.getByTestId('audit-package-generation-settings-panel')).toBeTruthy()
  })

  it('renders audit export panel outside admin workspace when user can export audit packages', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <AdminSection state={buildState(true, true)} />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('staffarr-settings-admin-workspace')).toBeTruthy()
    expect(screen.getByTestId('staffarr-audit-export-panel')).toBeTruthy()
  })

  it('omits admin workspace when user cannot manage worker settings', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <AdminSection state={buildState(false, true)} />
      </QueryClientProvider>,
    )

    expect(screen.queryByTestId('staffarr-settings-admin-workspace')).toBeNull()
    expect(screen.getByTestId('staffarr-audit-export-panel')).toBeTruthy()
  })
})
