import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { SettingsSection } from './SettingsSection'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

vi.mock('../../components/TenantSettingsPanel', () => ({
  TenantSettingsPanel: ({
    canManage,
    canRead,
  }: {
    canManage: boolean
    canRead: boolean
  }) => (
    <div
      data-can-manage={String(canManage)}
      data-can-read={String(canRead)}
      data-testid="trainarr-tenant-settings-panel"
    />
  ),
}))

vi.mock('../../components/AuditPackageExportPanel', () => ({
  AuditPackageExportPanel: () => <div data-testid="trainarr-audit-package-export-panel" />,
}))

function buildState(
  canReadSettings: boolean,
  canReadAudit = false,
  canExportAudit = false,
  canManageSettings = canReadSettings,
): TrainArrWorkspaceState {
  return {
    accessToken: 'token',
    canReadSettings,
    canManageSettings,
    canReadAudit,
    canExportAudit,
  } as TrainArrWorkspaceState
}

describe('SettingsSection', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders canonical tenant settings panel for trainarr_admin', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <SettingsSection state={buildState(true)} />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('trainarr-settings-admin-workspace')).toBeInTheDocument()
    expect(screen.getByTestId('trainarr-tenant-settings-panel')).toHaveAttribute('data-can-manage', 'true')
  })

  it('renders read-only tenant settings for trainarr_manager', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <SettingsSection state={buildState(true, false, false, false)} />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('trainarr-settings-admin-workspace')).toBeInTheDocument()
    expect(screen.getByTestId('trainarr-tenant-settings-panel')).toHaveAttribute('data-can-read', 'true')
    expect(screen.getByTestId('trainarr-tenant-settings-panel')).toHaveAttribute('data-can-manage', 'false')
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
