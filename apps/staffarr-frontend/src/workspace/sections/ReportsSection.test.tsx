import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ReportsSection } from './ReportsSection'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

vi.mock('../../components/PersonnelReportsPanel', () => ({
  PersonnelReportsPanel: () => <div data-testid="personnel-reports-panel" />,
}))

vi.mock('../../components/ReadinessReportsPanel', () => ({
  ReadinessReportsPanel: () => <div data-testid="readiness-reports-panel" />,
}))

vi.mock('../../components/IncidentReportsPanel', () => ({
  IncidentReportsPanel: () => <div data-testid="incident-reports-panel" />,
}))

vi.mock('../../components/DataExportsPanel', () => ({
  DataExportsPanel: () => <div data-testid="data-exports-panel" />,
}))

function buildState(roleKey: string, isPlatformAdmin = false): StaffArrWorkspaceState {
  return {
    accessToken: 'token',
    me: { tenantRoleKey: roleKey, isPlatformAdmin },
  } as StaffArrWorkspaceState
}

describe('ReportsSection', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders reports workspace for authorized admin', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <ReportsSection state={buildState('staffarr_admin')} />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('staffarr-reports-workspace')).toBeTruthy()
    expect(screen.getByTestId('personnel-reports-panel')).toBeTruthy()
    expect(screen.getByTestId('readiness-reports-panel')).toBeTruthy()
    expect(screen.getByTestId('incident-reports-panel')).toBeTruthy()
    expect(screen.getByTestId('data-exports-panel')).toBeTruthy()
  })

  it('omits reports workspace for unauthorized roles', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <ReportsSection state={buildState('tenant_member')} />
      </QueryClientProvider>,
    )

    expect(screen.queryByTestId('staffarr-reports-workspace')).toBeNull()
    expect(screen.queryByTestId('personnel-reports-panel')).toBeNull()
  })
})
