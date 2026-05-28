import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi } from 'vitest'

import { MaintenanceReportsPanel } from './MaintenanceReportsPanel'

vi.mock('../api/client', () => ({
  getMaintenanceReportSummary: vi.fn().mockResolvedValue({
    generatedAt: new Date().toISOString(),
    totalAssetCount: 1,
    activeAssetCount: 1,
    workOrderStatusCounts: [{ key: 'open', count: 1 }],
    defectStatusCounts: [],
    inspectionRunStatusCounts: [],
    pmScheduleStatusCounts: [],
    assets: [
      {
        assetId: 'asset-1',
        assetTag: 'FLT-01',
        assetName: 'Forklift 01',
        lifecycleStatus: 'active',
        readinessStatus: 'ready',
        openWorkOrderCount: 1,
        openDefectCount: 0,
        overduePmScheduleCount: 0,
        duePmScheduleCount: 0,
      },
    ],
  }),
  getMaintenanceReportAssetDetail: vi.fn(),
  getMaintenanceReportWorkOrderDetail: vi.fn(),
  exportMaintenanceReportSummaryCsv: vi.fn(),
}))

describe('MaintenanceReportsPanel', () => {
  it('renders maintenance report summary rows', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <MaintenanceReportsPanel accessToken="token" canRead={true} canExport={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('maintenance-reports-panel')).toBeInTheDocument()
    expect(await screen.findByText(/FLT-01 — Forklift 01/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Export CSV/i })).toBeInTheDocument()
  })

  it('returns null when user cannot read reports', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <MaintenanceReportsPanel accessToken="token" canRead={false} canExport={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })
})
