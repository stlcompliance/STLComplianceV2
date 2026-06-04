import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ReportsSection } from './ReportsSection'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

vi.mock('../../components/ComplianceReportsPanel', () => ({
  ComplianceReportsPanel: () => <div data-testid="compliance-reports-panel" />,
}))

vi.mock('../../components/CitationReviewReportsPanel', () => ({
  CitationReviewReportsPanel: () => <div data-testid="citation-review-reports-panel" />,
}))

vi.mock('../../components/EvidenceCompletenessReportsPanel', () => ({
  EvidenceCompletenessReportsPanel: () => <div data-testid="evidence-completeness-reports-panel" />,
}))

vi.mock('../../components/OperatorReportsPanel', () => ({
  OperatorReportsPanel: () => <div data-testid="operator-reports-panel" />,
}))

vi.mock('../../components/WaiverReportsPanel', () => ({
  WaiverReportsPanel: () => <div data-testid="waiver-reports-panel" />,
}))

vi.mock('../../components/ExceptionExemptionReportsPanel', () => ({
  ExceptionExemptionReportsPanel: () => <div data-testid="exception-exemption-reports-panel" />,
}))

vi.mock('../../components/AuditReadinessReportsPanel', () => ({
  AuditReadinessReportsPanel: () => <div data-testid="audit-readiness-reports-panel" />,
}))

vi.mock('../../components/RemediationQueueReportsPanel', () => ({
  RemediationQueueReportsPanel: () => <div data-testid="remediation-queue-reports-panel" />,
}))

vi.mock('../../components/ProductIntegrationHealthReportsPanel', () => ({
  ProductIntegrationHealthReportsPanel: () => <div data-testid="product-integration-health-reports-panel" />,
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
    expect(screen.getByTestId('citation-review-reports-panel')).toBeTruthy()
    expect(screen.getByTestId('evidence-completeness-reports-panel')).toBeTruthy()
    expect(screen.getByTestId('operator-reports-panel')).toBeTruthy()
    expect(screen.getByTestId('waiver-reports-panel')).toBeTruthy()
    expect(screen.getByTestId('exception-exemption-reports-panel')).toBeTruthy()
    expect(screen.getByTestId('remediation-queue-reports-panel')).toBeTruthy()
    expect(screen.getByTestId('audit-readiness-reports-panel')).toBeTruthy()
    expect(screen.getByTestId('product-integration-health-reports-panel')).toBeTruthy()
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
