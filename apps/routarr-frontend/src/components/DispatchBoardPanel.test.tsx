import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { DispatchBoardPanel } from './DispatchBoardPanel'

vi.mock('../api/client', () => ({
  getDispatchBoard: vi.fn().mockResolvedValue({
    scope: 'daily',
    windowStart: '2026-05-27T00:00:00Z',
    windowEnd: '2026-05-28T00:00:00Z',
    trips: {
      plannedCount: 1,
      assignedCount: 0,
      dispatchedCount: 1,
      inProgressCount: 0,
      completedCount: 0,
      cancelledCount: 0,
      totalCount: 2,
      lateCount: 0,
      atRiskCount: 1,
    },
    routes: {
      draftCount: 0,
      plannedCount: 1,
      activeCount: 0,
      completedCount: 0,
      cancelledCount: 0,
      totalCount: 1,
    },
    stops: {
      pendingCount: 2,
      arrivedCount: 0,
      completedCount: 0,
      skippedCount: 0,
      totalCount: 2,
    },
    workQueue: {
      unassignedDriverTripCount: 1,
      unlinkedRouteCount: 0,
      pendingStopCount: 2,
      missingProofTripCount: 1,
    },
    assignedTrips: [
      {
        tripId: '11111111-1111-1111-1111-111111111111',
        tripNumber: 'TR-20260527-AB12CD34',
        title: 'North yard delivery',
        dispatchStatus: 'dispatched',
        assignedDriverPersonId: 'driver-1',
        vehicleRefKey: 'VEH-1',
        scheduledStartAt: '2026-05-27T10:00:00Z',
        scheduledEndAt: '2026-05-27T13:00:00Z',
        isLate: false,
        isAtRisk: true,
        routeCount: 1,
        pendingStopCount: 2,
        missingRequiredProofCount: 1,
      },
    ],
    activeTrips: [
      {
        tripId: '11111111-1111-1111-1111-111111111111',
        tripNumber: 'TR-20260527-AB12CD34',
        title: 'North yard delivery',
        dispatchStatus: 'dispatched',
        assignedDriverPersonId: 'driver-1',
        vehicleRefKey: 'VEH-1',
        scheduledStartAt: '2026-05-27T10:00:00Z',
        scheduledEndAt: '2026-05-27T13:00:00Z',
        isLate: false,
        isAtRisk: true,
        routeCount: 1,
        pendingStopCount: 2,
        missingRequiredProofCount: 1,
      },
    ],
    generatedAt: '2026-05-27T12:05:00Z',
  }),
}))

describe('DispatchBoardPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders dispatch board counts and trip rows from the API', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const onScopeChange = vi.fn()

    render(
      <QueryClientProvider client={client}>
        <DispatchBoardPanel accessToken="token" scope="daily" onScopeChange={onScopeChange} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/Dispatch board/)).toBeInTheDocument()
    expect(screen.getByText('Unassigned trips')).toBeInTheDocument()
    expect(screen.getAllByText('North yard delivery').length).toBeGreaterThan(0)
    expect(screen.getAllByText('At risk').length).toBeGreaterThan(0)
  })

  it('switches to weekly scope when requested', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const onScopeChange = vi.fn()

    render(
      <QueryClientProvider client={client}>
        <DispatchBoardPanel accessToken="token" scope="daily" onScopeChange={onScopeChange} />
      </QueryClientProvider>,
    )

    await screen.findByText(/Dispatch board/)
    fireEvent.click(screen.getByRole('button', { name: 'Weekly' }))
    expect(onScopeChange).toHaveBeenCalledWith('weekly')
  })

  it('shows retry callout when board fails', async () => {
    const { getDispatchBoard } = await import('../api/client')
    vi.mocked(getDispatchBoard).mockRejectedValueOnce(new Error('board down'))
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const onScopeChange = vi.fn()

    render(
      <QueryClientProvider client={client}>
        <DispatchBoardPanel accessToken="token" scope="daily" onScopeChange={onScopeChange} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Dispatch board unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry board' })).toBeInTheDocument()
  })
})
