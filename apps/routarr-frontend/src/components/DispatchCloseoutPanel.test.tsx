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
      openTrips: 2,
      openRoutes: 1,
      openStops: 3,
      totalInScopeTrips: 4,
      totalInScopeRoutes: 2,
    },
    trips: {
      planned: 2,
      assigned: 0,
      dispatched: 0,
      inProgress: 0,
      completed: 0,
      cancelled: 0,
    },
    routes: {
      draft: 0,
      planned: 1,
      active: 0,
      completed: 0,
      cancelled: 0,
    },
    stops: {
      pending: 3,
      arrived: 0,
      completed: 0,
      skipped: 0,
    },
    openTrips: [],
    openRoutes: [],
  }),
  previewDispatchCloseout: vi.fn().mockResolvedValue({
    scope: 'daily',
    windowStart: '2026-05-27T00:00:00Z',
    windowEnd: '2026-05-28T00:00:00Z',
    remainingTripDisposition: 'cancel',
    openStopDisposition: 'skip',
    summary: {
      tripCount: 2,
      tripsCanApply: 2,
      tripsBlocked: 0,
      stopCount: 3,
      stopsCanApply: 3,
      stopsBlocked: 0,
      routeCount: 1,
      routesCanApply: 1,
      routesBlocked: 0,
    },
    tripActions: [
      {
        tripId: '11111111-1111-1111-1111-111111111111',
        tripNumber: 'TR-1',
        currentDispatchStatus: 'planned',
        targetDispatchStatus: 'cancelled',
        canApply: true,
        blockCode: null,
        blockMessage: null,
        transitionSteps: ['cancelled'],
      },
    ],
    stopActions: [],
    routeActions: [],
  }),
  applyDispatchCloseout: vi.fn().mockResolvedValue({
    scope: 'daily',
    windowStart: '2026-05-27T00:00:00Z',
    windowEnd: '2026-05-28T00:00:00Z',
    summary: {
      tripCount: 2,
      tripsCanApply: 2,
      tripsBlocked: 0,
      stopCount: 3,
      stopsCanApply: 3,
      stopsBlocked: 0,
      routeCount: 1,
      routesCanApply: 1,
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

  it('shows open work counts and runs preview', async () => {
    renderPanel()

    expect(await screen.findByText(/Open: 2 trips/)).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Preview closeout' }))

    await waitFor(() => {
      expect(screen.getByText(/Preview: 2\/2 trips/)).toBeInTheDocument()
    })
  })
})
