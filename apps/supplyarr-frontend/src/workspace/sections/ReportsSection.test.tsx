import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ReportsSection } from './ReportsSection'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

vi.mock('../../components/VendorReportsPanel', () => ({
  VendorReportsPanel: () => <div data-testid="vendor-reports-panel" />,
}))

vi.mock('../../components/PartsInventoryReportsPanel', () => ({
  PartsInventoryReportsPanel: () => <div data-testid="parts-inventory-reports-panel" />,
}))

vi.mock('../../components/PurchasingReportsPanel', () => ({
  PurchasingReportsPanel: () => <div data-testid="purchasing-reports-panel" />,
}))

vi.mock('../../components/ErpExportsPanel', () => ({
  ErpExportsPanel: () => <div data-testid="erp-exports-panel" />,
}))

vi.mock('../../components/ComplianceReportsPanel', () => ({
  ComplianceReportsPanel: () => <div data-testid="compliance-reports-panel" />,
}))

vi.mock('../../components/AuditHistoryPanel', () => ({
  AuditHistoryPanel: () => <div data-testid="audit-history-panel" />,
}))

function buildState(permissions: Partial<SupplyArrWorkspaceState>): SupplyArrWorkspaceState {
  return {
    accessToken: 'token',
    canReadVendorReports: false,
    canExportVendorReports: false,
    canReadPartsInventoryReports: false,
    canExportPartsInventoryReports: false,
    canReadPurchasingReports: false,
    canExportPurchasingReports: false,
    canReadComplianceReports: false,
    canExportComplianceReports: false,
    canReadAuditHistory: false,
    ...permissions,
  } as SupplyArrWorkspaceState
}

describe('ReportsSection', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders reports workspace with all current panels for supplyarr_admin', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <ReportsSection
          state={buildState({
            canReadVendorReports: true,
            canExportVendorReports: true,
            canReadPartsInventoryReports: true,
            canExportPartsInventoryReports: true,
            canReadPurchasingReports: true,
            canExportPurchasingReports: true,
            canReadComplianceReports: true,
            canExportComplianceReports: true,
            canReadAuditHistory: true,
          })}
        />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('supplyarr-reports-workspace')).toBeInTheDocument()
    expect(screen.getByTestId('vendor-reports-panel')).toBeInTheDocument()
    expect(screen.getByTestId('parts-inventory-reports-panel')).toBeInTheDocument()
    expect(screen.getByTestId('purchasing-reports-panel')).toBeInTheDocument()
    expect(screen.getByTestId('erp-exports-panel')).toBeInTheDocument()
    expect(screen.getByTestId('compliance-reports-panel')).toBeInTheDocument()
    expect(screen.getByTestId('audit-history-panel')).toBeInTheDocument()
  })

  it('omits reports workspace for unauthorized tenant members', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <ReportsSection state={buildState({})} />
      </QueryClientProvider>,
    )

    expect(screen.queryByTestId('supplyarr-reports-workspace')).not.toBeInTheDocument()
    expect(screen.queryByTestId('vendor-reports-panel')).not.toBeInTheDocument()
  })
})
