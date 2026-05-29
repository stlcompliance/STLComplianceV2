import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ReportsSection } from './ReportsSection'
import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'

vi.mock('../../components/DispatchReportsPanel', () => ({
  DispatchReportsPanel: () => <div data-testid="dispatch-reports-panel" />,
}))

vi.mock('../../components/RouteReportsPanel', () => ({
  RouteReportsPanel: () => <div data-testid="route-reports-panel" />,
}))

vi.mock('../../components/ProofDvirReportsPanel', () => ({
  ProofDvirReportsPanel: () => <div data-testid="proof-dvir-reports-panel" />,
}))

vi.mock('../../components/DataExportsPanel', () => ({
  DataExportsPanel: () => <div data-testid="data-exports-panel" />,
}))

vi.mock('../../components/AuditPackageExportPanel', () => ({
  AuditPackageExportPanel: () => <div data-testid="routarr-audit-export-panel" />,
}))

function buildState(roleKey: string, isPlatformAdmin = false): RoutArrWorkspaceState {
  return {
    session: { accessToken: 'token' },
    me: { tenantRoleKey: roleKey, isPlatformAdmin },
  } as RoutArrWorkspaceState
}

describe('ReportsSection', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders reports workspace with all four M12 panels for routarr_admin', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <ReportsSection state={buildState('routarr_admin')} />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('routarr-reports-workspace')).toBeInTheDocument()
    expect(screen.getByTestId('dispatch-reports-panel')).toBeInTheDocument()
    expect(screen.getByTestId('route-reports-panel')).toBeInTheDocument()
    expect(screen.getByTestId('proof-dvir-reports-panel')).toBeInTheDocument()
    expect(screen.getByTestId('data-exports-panel')).toBeInTheDocument()
    expect(screen.getByTestId('routarr-audit-export-panel')).toBeInTheDocument()
  })

  it('omits reports workspace for unauthorized tenant members', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <ReportsSection state={buildState('tenant_member')} />
      </QueryClientProvider>,
    )

    expect(screen.queryByTestId('routarr-reports-workspace')).not.toBeInTheDocument()
    expect(screen.queryByTestId('dispatch-reports-panel')).not.toBeInTheDocument()
  })
})
