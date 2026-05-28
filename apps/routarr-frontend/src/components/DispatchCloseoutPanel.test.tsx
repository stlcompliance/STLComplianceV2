import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { DispatchCloseoutPanel } from './DispatchCloseoutPanel'

vi.mock('../api/client', () => ({
  getDispatchCloseoutSummary: vi.fn().mockResolvedValue({
    scope: 'daily',
    windowStart: '2026-05-27T00:00:00Z',
    windowEnd: '2026-05-28T00:00:00Z',
    counts: {
      openTrips: 1,
      openRoutes: 0,
      openStops: 0,
      totalInScopeTrips: 1,
      totalInScopeRoutes: 0,
    },
    trips: {
      planned: 1,
      assigned: 0,
      dispatched: 0,
      inProgress: 0,
      completed: 0,
      cancelled: 0,
    },
    routes: {
      draft: 0,
      planned: 0,
      active: 0,
      completed: 0,
      cancelled: 0,
    },
    stops: {
      pending: 0,
      arrived: 0,
      completed: 0,
      skipped: 0,
    },
    openTrips: [
      {
        tripId: '11111111-1111-1111-1111-111111111111',
        tripNumber: 'TR-1',
        title: 'Test',
        dispatchStatus: 'planned',
        assignedDriverPersonId: null,
      },
    ],
    openRoutes: [],
  }),
  getDispatchCloseoutChecklists: vi.fn().mockResolvedValue({
    scope: 'daily',
    windowStart: '2026-05-27T00:00:00Z',
    windowEnd: '2026-05-28T00:00:00Z',
    remainingTripDisposition: 'cancel',
    trips: [
      {
        tripId: '11111111-1111-1111-1111-111111111111',
        tripNumber: 'TR-1',
        dispatchStatus: 'planned',
        readyForCloseout: true,
        items: [
          {
            key: 'trip_disposition_ready',
            label: 'Trip status can close',
            satisfied: true,
            required: true,
            detail: null,
          },
        ],
      },
    ],
  }),
  getDispatchCloseoutAudit: vi.fn().mockResolvedValue({
    entries: [
      {
        id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
        actorUserId: null,
        action: 'dispatch_closeout.summary',
        targetType: 'dispatch_closeout',
        targetId: 'daily',
        result: '1 trips',
        occurredAt: '2026-05-27T12:00:00Z',
      },
    ],
  }),
  previewDispatchCloseout: vi.fn().mockResolvedValue({
    scope: 'daily',
    windowStart: '2026-05-27T00:00:00Z',
    windowEnd: '2026-05-28T00:00:00Z',
    remainingTripDisposition: 'cancel',
    openStopDisposition: 'skip',
    summary: {
      tripCount: 1,
      tripsCanApply: 1,
      tripsBlocked: 0,
      stopCount: 0,
      stopsCanApply: 0,
      stopsBlocked: 0,
      routeCount: 0,
      routesCanApply: 0,
      routesBlocked: 0,
    },
    tripActions: [],
    stopActions: [],
    routeActions: [],
  }),
  applyDispatchCloseout: vi.fn().mockResolvedValue({
    scope: 'daily',
    windowStart: '2026-05-27T00:00:00Z',
    windowEnd: '2026-05-28T00:00:00Z',
    summary: {
      tripCount: 1,
      tripsCanApply: 1,
      tripsBlocked: 0,
      stopCount: 0,
      stopsCanApply: 0,
      stopsBlocked: 0,
      routeCount: 0,
      routesCanApply: 0,
      routesBlocked: 0,
    },
    tripResults: [],
    stopResults: [],
    routeResults: [],
  }),
}))

function renderPanel() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={client}>
      <DispatchCloseoutPanel accessToken="token" scope="daily" canAssign />
    </QueryClientProvider>,
  )
}

describe('DispatchCloseoutPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows checklist, audit, and runs preview', async () => {
    renderPanel()

    expect(await screen.findByTestId('dispatch-closeout-panel')).toBeInTheDocument()
    expect(screen.getByText(/Trip closeout checklist/)).toBeInTheDocument()
    expect(screen.getByText(/Recent closeout audit/)).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Preview closeout' }))

    await waitFor(() => {
      expect(screen.getByText(/Preview \(all open\)/)).toBeInTheDocument()
    })
  })

  it('shows bulk mode when a trip is selected', async () => {
    renderPanel()

    const checkbox = await screen.findByRole('checkbox', { name: 'Select TR-1' })
    fireEvent.click(checkbox)

    expect(screen.getByText(/Bulk closeout: 1 trip\(s\) selected/)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Apply bulk closeout' })).toBeInTheDocument()
  })
})
