import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi, afterEach } from 'vitest'
import { cleanup, fireEvent, waitFor } from '@testing-library/react'

import { ExecutiveReportsPanel } from './ExecutiveReportsPanel'
import * as client from '../api/client'

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
    downtimeTrend: {
      periodDays: 30,
      currentPeriod: {
        periodStart: new Date(Date.now() - 30 * 86400000).toISOString(),
        periodEnd: new Date().toISOString(),
        downtimeHours: 48,
        availabilityPercent: 92.5,
        plannedDowntimeHours: 12,
        unplannedDowntimeHours: 36,
        activeDowntimeEventCount: 2,
        fromMaterializedSnapshot: true,
      },
      previousPeriod: {
        periodStart: new Date(Date.now() - 60 * 86400000).toISOString(),
        periodEnd: new Date(Date.now() - 30 * 86400000).toISOString(),
        downtimeHours: 32,
        availabilityPercent: 95,
        plannedDowntimeHours: 8,
        unplannedDowntimeHours: 24,
        activeDowntimeEventCount: 0,
        fromMaterializedSnapshot: false,
      },
      downtimeHoursDelta: 16,
      availabilityPercentDelta: -2.5,
      fleetSnapshotComputedAt: new Date().toISOString(),
    },
    supplyDemand: {
      sourceProduct: 'supplyarr',
      totalDemandLines: 4,
      publishedDemandLines: 3,
      openProcurementLines: 1,
      fulfilledLines: 1,
      procurementStatusCounts: [{ key: 'awaiting_procurement', count: 1 }],
    },
    partsDemandForecast: {
      openLineCount: 3,
      distinctPartCount: 2,
      forecastQuantity: 5,
      topParts: [
        {
          supplyarrPartId: 'part-1',
          partNumber: 'BRK-001',
          description: 'Brake pads',
          unitOfMeasure: 'each',
          forecastQuantity: 3,
          openLineCount: 2,
          openWorkOrderCount: 2,
          pmWorkOrderCount: 1,
          defectWorkOrderCount: 0,
          manualWorkOrderCount: 1,
          oldestCreatedAt: new Date(Date.now() - 86400000).toISOString(),
          newestCreatedAt: new Date().toISOString(),
        },
      ],
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
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

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
    expect(await screen.findByTestId('executive-downtime-trend')).toBeInTheDocument()
    expect(await screen.findByText(/Downtime trend/i)).toBeInTheDocument()
    expect(await screen.findByText(/\+16\.0h/i)).toBeInTheDocument()
    expect(await screen.findByText(/SupplyArr demand lines:/i)).toBeInTheDocument()
    expect(await screen.findByTestId('parts-demand-forecast-panel')).toBeInTheDocument()
    expect(await screen.findByRole('heading', { name: /Parts-demand forecast/i })).toBeInTheDocument()
    expect(await screen.findByText(/BRK-001/i)).toBeInTheDocument()
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

  it('shows retry callout when summary fails', async () => {
    vi.mocked(client.getExecutiveReportSummary).mockRejectedValue(new Error('summary down'))

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <ExecutiveReportsPanel accessToken="token" canRead={true} canExport={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Executive summary unavailable')).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Retry summary' }))

    await waitFor(() => {
      expect(client.getExecutiveReportSummary).toHaveBeenCalledTimes(2)
    })
  })
})
