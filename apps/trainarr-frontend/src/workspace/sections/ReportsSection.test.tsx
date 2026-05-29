import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ReportsSection } from './ReportsSection'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

vi.mock('../../components/AssignmentReportsPanel', () => ({
  AssignmentReportsPanel: () => <div data-testid="assignment-reports-panel" />,
}))

vi.mock('../../components/QualificationReportsPanel', () => ({
  QualificationReportsPanel: () => <div data-testid="qualification-reports-panel" />,
}))

vi.mock('../../components/ComplianceReportsPanel', () => ({
  ComplianceReportsPanel: () => <div data-testid="compliance-reports-panel" />,
}))

vi.mock('../../components/DataExportsPanel', () => ({
  DataExportsPanel: () => <div data-testid="data-exports-panel" />,
}))

function buildState(roleKey: string, isPlatformAdmin = false): TrainArrWorkspaceState {
  return {
    accessToken: 'token',
    me: { tenantRoleKey: roleKey, isPlatformAdmin },
  } as TrainArrWorkspaceState
}

describe('ReportsSection', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders reports workspace with all four M12 panels for trainarr_admin', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <ReportsSection state={buildState('trainarr_admin')} />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('trainarr-reports-workspace')).toBeInTheDocument()
    expect(screen.getByTestId('assignment-reports-panel')).toBeInTheDocument()
    expect(screen.getByTestId('qualification-reports-panel')).toBeInTheDocument()
    expect(screen.getByTestId('compliance-reports-panel')).toBeInTheDocument()
    expect(screen.getByTestId('data-exports-panel')).toBeInTheDocument()
  })

  it('omits reports workspace for unauthorized tenant members', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <ReportsSection state={buildState('supplyarr_admin')} />
      </QueryClientProvider>,
    )

    expect(screen.queryByTestId('trainarr-reports-workspace')).not.toBeInTheDocument()
    expect(screen.queryByTestId('assignment-reports-panel')).not.toBeInTheDocument()
  })
})
