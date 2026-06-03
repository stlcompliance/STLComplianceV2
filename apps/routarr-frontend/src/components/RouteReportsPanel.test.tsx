import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi, afterEach } from 'vitest'
import { cleanup, fireEvent } from '@testing-library/react'

import { RouteReportsPanel } from './RouteReportsPanel'

vi.mock('../api/client', () => ({
  getRouteReportSummary: vi.fn().mockResolvedValue({
    generatedAt: new Date().toISOString(),
    scope: 'daily',
    windowStart: new Date().toISOString(),
    windowEnd: new Date().toISOString(),
    totalRouteCount: 1,
    totalStopCount: 2,
    pendingStopCount: 1,
    arrivedStopCount: 0,
    completedStopCount: 1,
    skippedStopCount: 0,
    routeStatusCounts: [{ key: 'draft', count: 1 }],
    stopStatusCounts: [
      { key: 'pending', count: 1 },
      { key: 'completed', count: 1 },
    ],
    stopTypeCounts: [{ key: 'pickup', count: 1 }],
    routes: [
      {
        routeId: 'route-1',
        routeNumber: 'RT-RPT',
        title: 'Report test route',
        routeStatus: 'draft',
        tripId: null,
        tripNumber: null,
        totalStopCount: 2,
        pendingStopCount: 1,
        arrivedStopCount: 0,
        completedStopCount: 1,
        skippedStopCount: 0,
        completionPercent: 50,
      },
    ],
    recentStops: [
      {
        stopId: 'stop-1',
        routeId: 'route-1',
        routeNumber: 'RT-RPT',
        stopKey: 'stop-a',
        label: 'Pickup',
        stopType: 'pickup',
        stopStatus: 'completed',
        sequenceNumber: 1,
        scheduledArrivalAt: null,
        updatedAt: new Date().toISOString(),
      },
    ],
  }),
  getRouteReportRouteDetail: vi.fn(),
  getRouteReportStopDetail: vi.fn(),
  exportRouteReportSummaryCsv: vi.fn(),
}))

describe('RouteReportsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders route report summary rows', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <RouteReportsPanel accessToken="token" canRead={true} canExport={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('route-reports-panel')).toBeTruthy()
    expect(await screen.findByText(/RT-RPT — Report test route/i)).toBeTruthy()
    expect(screen.getByRole('button', { name: /Export CSV/i })).toBeTruthy()
  })

  it('renders route detail history when a route is selected', async () => {
    const { getRouteReportRouteDetail } = await import('../api/client')
    vi.mocked(getRouteReportRouteDetail).mockResolvedValueOnce({
      routeId: 'route-1',
      routeNumber: 'RT-RPT',
      title: 'Report test route',
      description: 'A route used to verify history rendering.',
      routeStatus: 'draft',
      tripId: null,
      tripNumber: null,
      tripTitle: null,
      totalStopCount: 2,
      pendingStopCount: 1,
      completedStopCount: 1,
      skippedStopCount: 0,
      completionPercent: 50,
      createdAt: '2026-06-01T00:00:00Z',
      updatedAt: '2026-06-02T00:00:00Z',
      activatedAt: '2026-06-01T12:00:00Z',
      completedAt: null,
      stops: [],
      history: [
        {
          occurredAt: '2026-06-02T09:00:00Z',
          action: 'route.updated',
          result: 'success',
          reasonCode: null,
          actorUserId: 'actor-1',
        },
      ],
    })

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <RouteReportsPanel accessToken="token" canRead={true} canExport={true} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('route-reports-panel')
    await screen.findByText(/RT-RPT — Report test route/i)
    fireEvent.click(screen.getByText(/RT-RPT — Report test route/i))
    expect(await screen.findByText('Route history')).toBeInTheDocument()
    expect(await screen.findByText('route.updated')).toBeInTheDocument()
  })

  it('returns null when user cannot read reports', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <RouteReportsPanel accessToken="token" canRead={false} canExport={false} />
      </QueryClientProvider>,
    )

    expect(container.firstChild).toBeNull()
  })

  it('shows export failure callout', async () => {
    const { exportRouteReportSummaryCsv } = await import('../api/client')
    vi.mocked(exportRouteReportSummaryCsv).mockRejectedValueOnce(new Error('export down'))
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <RouteReportsPanel accessToken="token" canRead={true} canExport={true} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('route-reports-panel')
    fireEvent.click(screen.getByRole('button', { name: /Export CSV/i }))
    expect(await screen.findByText('CSV export failed')).toBeInTheDocument()
  })
})
