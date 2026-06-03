import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ReportsSection } from './ReportsSection'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

vi.mock('../../components/ComplianceReportsPanel', () => ({
  ComplianceReportsPanel: () => <div data-testid="compliance-reports-panel" />,
}))

vi.mock('../../components/OperatorReportsPanel', () => ({
  OperatorReportsPanel: () => <div data-testid="operator-reports-panel" />,
}))

vi.mock('../../components/DataExportsPanel', () => ({
  DataExportsPanel: () => <div data-testid="data-exports-panel" />,
}))

function buildState(roleKey: string): ComplianceCoreWorkspaceState {
  const canReadReports = roleKey !== 'unknown_role'
  return {
    accessToken: 'token',
    me: {
      userId: 'user-1',
      personId: 'person-1',
      email: 'admin@demo.stl',
      displayName: 'Demo Admin',
      tenantId: 'tenant-1',
      tenantRoleKey: roleKey,
      isPlatformAdmin: false,
      productKey: 'compliancecore',
      hasComplianceCoreEntitlement: true,
      entitlements: ['compliancecore'],
      canManageVocabulary: false,
      canExportAuditPackage: false,
      canEvaluateRiskScores: false,
      canEvaluateMissingEvidenceWarnings: false,
      canEvaluateControlEffectiveness: false,
      canEvaluateReadinessForecast: false,
      canReadReports,
      canExportReports: canReadReports,
    },
  } as ComplianceCoreWorkspaceState
}

describe('ReportsSection', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders reports workspace with all panels for authorized admin', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <ReportsSection state={buildState('compliance_admin')} />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('compliancecore-reports-workspace')).toBeTruthy()
    expect(screen.getByTestId('compliance-reports-panel')).toBeTruthy()
    expect(screen.getByTestId('operator-reports-panel')).toBeTruthy()
    expect(screen.getByTestId('data-exports-panel')).toBeTruthy()
  })

  it('omits reports workspace for unauthorized roles', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <ReportsSection state={buildState('unknown_role')} />
      </QueryClientProvider>,
    )

    expect(screen.queryByTestId('compliancecore-reports-workspace')).toBeNull()
    expect(screen.queryByTestId('compliance-reports-panel')).toBeNull()
  })
})
