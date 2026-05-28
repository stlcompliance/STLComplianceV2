import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi } from 'vitest'

import { DispatchReportsPanel } from './DispatchReportsPanel'

vi.mock('../api/client', () => ({
  getDispatchReportSummary: vi.fn().mockResolvedValue({
    generatedAt: new Date().toISOString(),
    scope: 'daily',
    windowStart: new Date().toISOString(),
    windowEnd: new Date().toISOString(),
    totalTripCount: 1,
    lateTripCount: 0,
    atRiskTripCount: 1,
    unassignedTripCount: 0,
    openExceptionCount: 1,
    delayExceptionCount: 1,
    tripStatusCounts: [{ key: 'dispatched', count: 1 }],
    exceptionStatusCounts: [{ key: 'open', count: 1 }],
    exceptionCategoryCounts: [{ key: 'delay', count: 1 }],
    trips: [
      {
        tripId: 'trip-1',
        tripNumber: 'TR-RPT',
        title: 'Report test haul',
        dispatchStatus: 'dispatched',
        assignedDriverPersonId: 'driver-1',
        vehicleRefKey: 'VEH-1',
        scheduledStartAt: new Date().toISOString(),
        scheduledEndAt: new Date().toISOString(),
        isLate: false,
        isAtRisk: true,
        isUnassigned: false,
        routeCount: 1,
        openExceptionCount: 0,
      },
    ],
    recentExceptions: [
      {
        exceptionId: 'ex-1',
        exceptionKey: 'DEL-001',
        title: 'Highway delay',
        category: 'delay',
        status: 'open',
        tripId: 'trip-1',
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      },
    ],
  }),
  getDispatchReportTripDetail: vi.fn(),
  getDispatchReportExceptionDetail: vi.fn(),
  exportDispatchReportSummaryCsv: vi.fn(),
}))

describe('DispatchReportsPanel', () => {
  it('renders dispatch report summary rows', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <DispatchReportsPanel accessToken="token" canRead={true} canExport={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('dispatch-reports-panel')).toBeTruthy()
    expect(await screen.findByText(/TR-RPT — Report test haul/i)).toBeTruthy()
    expect(screen.getByRole('button', { name: /Export CSV/i })).toBeTruthy()
  })

  it('returns null when user cannot read reports', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <DispatchReportsPanel accessToken="token" canRead={false} canExport={false} />
      </QueryClientProvider>,
    )

    expect(container.firstChild).toBeNull()
  })
})
