import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi } from 'vitest'

import { ExecutiveReportsPanel } from './ExecutiveReportsPanel'

vi.mock('../api/client', () => ({
  getExecutiveReportSummary: vi.fn().mockResolvedValue({
    generatedAt: new Date().toISOString(),
    fleetReadiness: {
      totalAssets: 10,
      readyCount: 8,
      notReadyCount: 2,
      readyPercent: 80,
      computedAt: new Date().toISOString(),
      fromScopeRollup: true,
    },
    operationalTotals: {
      totalAssetCount: 10,
      activeAssetCount: 9,
      openWorkOrderCount: 3,
      openCriticalDefectCount: 1,
      openHighDefectCount: 2,
      overduePmScheduleCount: 1,
      failedInspectionCount: 0,
      laborHoursLast30Days: 42.5,
      workOrdersCompletedLast30Days: 5,
      activeTechnicianAssignments: 2,
    },
    supplyDemand: {
      sourceProduct: 'supplyarr',
      totalDemandLines: 4,
      publishedDemandLines: 3,
      openProcurementLines: 1,
      fulfilledLines: 1,
      procurementStatusCounts: [{ key: 'awaiting_procurement', count: 1 }],
    },
    scopeReadiness: [
      {
        scopeType: 'site',
        scopeEntityId: 'site-1',
        scopeLabel: 'Main yard',
        totalAssets: 5,
        readyCount: 4,
        notReadyCount: 1,
        readyPercent: 80,
        computedAt: new Date().toISOString(),
      },
    ],
    workOrderStatusCounts: [{ key: 'open', count: 3 }],
    defectSeverityCounts: [{ key: 'high', count: 2 }],
  }),
  exportExecutiveReportSummaryCsv: vi.fn(),
}))

describe('ExecutiveReportsPanel', () => {
  it('renders executive summary metrics', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <ExecutiveReportsPanel accessToken="token" canRead={true} canExport={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('executive-reports-panel')).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: /Executive summary/i })).toBeInTheDocument()
    expect(await screen.findByText(/Main yard/i)).toBeInTheDocument()
    expect(await screen.findByText(/SupplyArr demand lines:/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Export CSV/i })).toBeInTheDocument()
  })

  it('returns null when user cannot read executive reports', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <ExecutiveReportsPanel accessToken="token" canRead={false} canExport={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })
})
