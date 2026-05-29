import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { AdminSection } from './AdminSection'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

vi.mock('../../components/AuditDeliveryOrchestrationPanel', () => ({
  AuditDeliveryOrchestrationPanel: () => (
    <div data-testid="compliancecore-audit-delivery-orchestration-panel" />
  ),
}))

vi.mock('../../components/M12AnalyticsWorkerSettingsPanel', () => ({
  M12AnalyticsWorkerSettingsPanel: () => (
    <div data-testid="compliancecore-m12-analytics-worker-settings-panel" />
  ),
}))

vi.mock('../../components/ReadinessForecastPanel', () => ({
  ReadinessForecastPanel: () => <div data-testid="readiness-forecast-panel" />,
}))

vi.mock('../../components/ControlEffectivenessPanel', () => ({
  ControlEffectivenessPanel: () => <div data-testid="control-effectiveness-panel" />,
}))

vi.mock('../../components/MissingEvidenceWarningsPanel', () => ({
  MissingEvidenceWarningsPanel: () => <div data-testid="missing-evidence-warnings-panel" />,
}))

vi.mock('../../components/RiskScoringPanel', () => ({
  RiskScoringPanel: () => <div data-testid="risk-scoring-panel" />,
}))

vi.mock('../../components/RuleChangeMonitoringPanel', () => ({
  RuleChangeMonitoringPanel: () => <div data-testid="rule-change-monitoring-panel" />,
}))

vi.mock('../../components/SourceIngestionPanel', () => ({
  SourceIngestionPanel: () => <div data-testid="source-ingestion-panel" />,
}))

vi.mock('../../components/CsvImportExportPanel', () => ({
  CsvImportExportPanel: () => <div data-testid="csv-import-export-panel" />,
}))

vi.mock('../../components/AuditPackageExportPanel', () => ({
  AuditPackageExportPanel: () => <div data-testid="compliancecore-audit-export-panel" />,
}))

function buildState(
  overrides: Partial<ComplianceCoreWorkspaceState> = {},
): ComplianceCoreWorkspaceState {
  return {
    accessToken: 'token',
    canManage: false,
    canExportAudit: false,
    canReadOrchestration: false,
    canEvaluateRisk: false,
    canEvaluateMissingEvidence: false,
    canEvaluateControlEffectiveness: false,
    canEvaluateReadinessForecast: false,
    ...overrides,
  } as ComplianceCoreWorkspaceState
}

describe('AdminSection', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders admin workspace with all nine product-admin panels for compliance admin', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <AdminSection
          state={buildState({
            canManage: true,
            canReadOrchestration: true,
            canEvaluateRisk: true,
            canEvaluateMissingEvidence: true,
            canEvaluateControlEffectiveness: true,
            canEvaluateReadinessForecast: true,
          })}
        />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('compliancecore-settings-admin-workspace')).toBeTruthy()
    expect(screen.getByTestId('compliancecore-audit-delivery-orchestration-panel')).toBeTruthy()
    expect(screen.getByTestId('compliancecore-m12-analytics-worker-settings-panel')).toBeTruthy()
    expect(screen.getByTestId('readiness-forecast-panel')).toBeTruthy()
    expect(screen.getByTestId('control-effectiveness-panel')).toBeTruthy()
    expect(screen.getByTestId('missing-evidence-warnings-panel')).toBeTruthy()
    expect(screen.getByTestId('risk-scoring-panel')).toBeTruthy()
    expect(screen.getByTestId('rule-change-monitoring-panel')).toBeTruthy()
    expect(screen.getByTestId('source-ingestion-panel')).toBeTruthy()
    expect(screen.getByTestId('csv-import-export-panel')).toBeTruthy()
  })

  it('renders audit export panel outside admin workspace when user can export audit packages', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <AdminSection
          state={buildState({
            canManage: true,
            canReadOrchestration: true,
            canExportAudit: true,
          })}
        />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('compliancecore-settings-admin-workspace')).toBeTruthy()
    expect(screen.getByTestId('compliancecore-audit-export-panel')).toBeTruthy()
  })

  it('omits admin workspace when user lacks admin permissions but shows audit export', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <AdminSection state={buildState({ canExportAudit: true })} />
      </QueryClientProvider>,
    )

    expect(screen.queryByTestId('compliancecore-settings-admin-workspace')).toBeNull()
    expect(screen.getByTestId('compliancecore-audit-export-panel')).toBeTruthy()
  })
})
