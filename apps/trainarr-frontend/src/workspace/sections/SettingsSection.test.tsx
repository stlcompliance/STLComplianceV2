import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { SettingsSection } from './SettingsSection'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

vi.mock('../../components/IntegrationSettingsPanel', () => ({
  IntegrationSettingsPanel: () => <div data-testid="integration-settings-panel" />,
}))

vi.mock('../../components/NotificationSettingsPanel', () => ({
  NotificationSettingsPanel: () => <div data-testid="notification-settings-panel" />,
}))

vi.mock('../../components/AssignmentReminderEscalationSettingsPanel', () => ({
  AssignmentReminderEscalationSettingsPanel: () => (
    <div data-testid="assignment-reminder-escalation-settings-panel" />
  ),
}))

vi.mock('../../components/RecertificationSettingsPanel', () => ({
  RecertificationSettingsPanel: () => <div data-testid="recertification-settings-panel" />,
}))

vi.mock('../../components/QualificationRecalculationSettingsPanel', () => ({
  QualificationRecalculationSettingsPanel: () => (
    <div data-testid="qualification-recalculation-settings-panel" />
  ),
}))

vi.mock('../../components/RulePackImpactSettingsPanel', () => ({
  RulePackImpactSettingsPanel: () => <div data-testid="rule-pack-impact-settings-panel" />,
}))

vi.mock('../../components/EvidenceRetentionSettingsPanel', () => ({
  EvidenceRetentionSettingsPanel: () => <div data-testid="evidence-retention-settings-panel" />,
}))

vi.mock('../../components/OrphanReferenceSettingsPanel', () => ({
  OrphanReferenceSettingsPanel: () => <div data-testid="orphan-reference-settings-panel" />,
}))

vi.mock('../../components/StaffarrPublicationSettingsPanel', () => ({
  StaffarrPublicationSettingsPanel: () => <div data-testid="staffarr-publication-settings-panel" />,
}))

vi.mock('../../components/EventProcessingSettingsPanel', () => ({
  EventProcessingSettingsPanel: () => <div data-testid="event-processing-settings-panel" />,
}))

vi.mock('../../components/AuditPackageExportPanel', () => ({
  AuditPackageExportPanel: () => <div data-testid="trainarr-audit-package-export-panel" />,
}))

function buildState(
  canNotifications: boolean,
  canReadAudit = false,
  canExportAudit = false,
): TrainArrWorkspaceState {
  return {
    accessToken: 'token',
    canNotifications,
    canReadAudit,
    canExportAudit,
  } as TrainArrWorkspaceState
}

describe('SettingsSection', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders admin workspace with all settings panels for trainarr_admin', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <SettingsSection state={buildState(true)} />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('trainarr-settings-admin-workspace')).toBeInTheDocument()
    expect(screen.getByTestId('integration-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('notification-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('assignment-reminder-escalation-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('recertification-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('qualification-recalculation-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('rule-pack-impact-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('evidence-retention-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('orphan-reference-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('staffarr-publication-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('event-processing-settings-panel')).toBeInTheDocument()
  })

  it('renders audit export panel outside admin workspace when user can read audit packages', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <SettingsSection state={buildState(true, true, true)} />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('trainarr-settings-admin-workspace')).toBeInTheDocument()
    expect(screen.getByTestId('trainarr-audit-package-export-panel')).toBeInTheDocument()
  })

  it('renders permission message for unauthorized tenant members', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <SettingsSection state={buildState(false)} />
      </QueryClientProvider>,
    )

    expect(screen.queryByTestId('trainarr-settings-admin-workspace')).not.toBeInTheDocument()
    expect(screen.getByText('You do not have permission to manage settings.')).toBeInTheDocument()
  })
})
