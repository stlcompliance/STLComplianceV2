import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ReportsSection } from './ReportsSection'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

vi.mock('../../components/ComplianceReportsPanel', () => ({
  ComplianceReportsPanel: () => <div data-testid="compliance-reports-panel" />,
}))

vi.mock('../../components/ExecutiveReportsPanel', () => ({
  ExecutiveReportsPanel: () => <div data-testid="executive-reports-panel" />,
}))

vi.mock('../../components/MaintenanceReportsPanel', () => ({
  MaintenanceReportsPanel: () => <div data-testid="maintenance-reports-panel" />,
}))

vi.mock('../../components/DataExportsPanel', () => ({
  DataExportsPanel: () => <div data-testid="data-exports-panel" />,
}))

function buildState(roleKey: string, isPlatformAdmin = false): MaintainArrWorkspaceState {
  return {
    accessToken: 'token',
    me: { tenantRoleKey: roleKey, isPlatformAdmin },
  } as MaintainArrWorkspaceState
}

describe('ReportsSection', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders reports workspace with all four M12 panels for maintainarr_admin', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <ReportsSection state={buildState('maintainarr_admin')} />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('maintainarr-reports-workspace')).toBeInTheDocument()
    expect(screen.getByTestId('compliance-reports-panel')).toBeInTheDocument()
    expect(screen.getByTestId('executive-reports-panel')).toBeInTheDocument()
    expect(screen.getByTestId('maintenance-reports-panel')).toBeInTheDocument()
    expect(screen.getByTestId('data-exports-panel')).toBeInTheDocument()
  })

  it('omits reports workspace for unauthorized tenant members', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <ReportsSection state={buildState('supplyarr_admin')} />
      </QueryClientProvider>,
    )

    expect(screen.queryByTestId('maintainarr-reports-workspace')).not.toBeInTheDocument()
    expect(screen.queryByTestId('maintenance-reports-panel')).not.toBeInTheDocument()
  })
})
