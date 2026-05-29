import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { SettingsSection } from './SettingsSection'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

vi.mock('../../components/AssetBulkImportPanel', () => ({
  AssetBulkImportPanel: () => <div data-testid="asset-bulk-import-panel" />,
}))

vi.mock('../../components/PmDueScanSettingsPanel', () => ({
  PmDueScanSettingsPanel: () => <div data-testid="pm-due-scan-settings-panel" />,
}))

vi.mock('../../components/MaintenanceHistoryRollupSettingsPanel', () => ({
  MaintenanceHistoryRollupSettingsPanel: () => (
    <div data-testid="maintenance-history-rollup-settings-panel" />
  ),
}))

vi.mock('../../components/AssetStatusRollupSettingsPanel', () => ({
  AssetStatusRollupSettingsPanel: () => <div data-testid="asset-status-rollup-settings-panel" />,
}))

vi.mock('../../components/DefectEscalationSettingsPanel', () => ({
  DefectEscalationSettingsPanel: () => <div data-testid="defect-escalation-settings-panel" />,
}))

vi.mock('../../components/NotificationSettingsPanel', () => ({
  NotificationSettingsPanel: () => <div data-testid="notification-settings-panel" />,
}))

vi.mock('../../components/AuditPackageExportPanel', () => ({
  AuditPackageExportPanel: () => <div data-testid="maintainarr-audit-export-panel" />,
}))

function buildState(
  canManage: boolean,
  canManageNotifications: boolean,
  canExportAudit = false,
): MaintainArrWorkspaceState {
  return {
    accessToken: 'token',
    canManage,
    canManageNotifications,
    canExportAudit,
    assetsQuery: { refetch: vi.fn() },
  } as unknown as MaintainArrWorkspaceState
}

describe('SettingsSection', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders admin workspace with all settings panels for maintainarr_admin', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <SettingsSection state={buildState(true, true)} />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('maintainarr-settings-admin-workspace')).toBeInTheDocument()
    expect(screen.getByTestId('pm-due-scan-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('maintenance-history-rollup-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('asset-status-rollup-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('defect-escalation-settings-panel')).toBeInTheDocument()
    expect(screen.getByTestId('notification-settings-panel')).toBeInTheDocument()
  })

  it('renders audit export panel outside admin workspace when user can export audit packages', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <SettingsSection state={buildState(true, true, true)} />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('maintainarr-settings-admin-workspace')).toBeInTheDocument()
    expect(screen.getByTestId('maintainarr-audit-export-panel')).toBeInTheDocument()
  })

  it('omits admin workspace when user cannot manage notification settings', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <SettingsSection state={buildState(true, false)} />
      </QueryClientProvider>,
    )

    expect(screen.queryByTestId('maintainarr-settings-admin-workspace')).not.toBeInTheDocument()
    expect(screen.getByTestId('asset-bulk-import-panel')).toBeInTheDocument()
  })
})
