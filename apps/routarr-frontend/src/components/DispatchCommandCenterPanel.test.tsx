import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { DispatchCommandCenterPanel } from './DispatchCommandCenterPanel'

vi.mock('../api/client', () => ({
  getDispatchCommandCenter: vi.fn(),
  upsertDispatchBoardState: vi.fn(),
  assignTripDriver: vi.fn(),
  updateTripStatus: vi.fn(),
}))

import * as client from '../api/client'

function renderPanel(canAssign: boolean) {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={qc}>
      <DispatchCommandCenterPanel
        accessToken="token"
        scope="daily"
        onScopeChange={vi.fn()}
        canAssign={canAssign}
      />
    </QueryClientProvider>,
  )
}

describe('DispatchCommandCenterPanel', () => {
  afterEach(() => cleanup())

  it('renders status columns from command center', async () => {
    vi.mocked(client.getDispatchCommandCenter).mockResolvedValue({
      generatedAt: new Date().toISOString(),
      scope: 'daily',
      boardState: { defaultScope: 'daily', updatedAt: new Date().toISOString(), updatedByUserId: null },
      board: {
        scope: 'daily',
        windowStart: new Date().toISOString(),
        windowEnd: new Date().toISOString(),
        trips: {
          plannedCount: 1,
          assignedCount: 0,
          dispatchedCount: 0,
          inProgressCount: 0,
          completedCount: 0,
          cancelledCount: 0,
          totalCount: 1,
          lateCount: 0,
          atRiskCount: 0,
        },
        routes: {
          draftCount: 0,
          plannedCount: 0,
          activeCount: 0,
          completedCount: 0,
          cancelledCount: 0,
          totalCount: 0,
        },
        stops: {
          pendingCount: 0,
          arrivedCount: 0,
          completedCount: 0,
          skippedCount: 0,
          totalCount: 0,
        },
        workQueue: {
          unassignedDriverTripCount: 1,
          unlinkedRouteCount: 0,
          pendingStopCount: 0,
        },
        assignedTrips: [],
        activeTrips: [],
        generatedAt: new Date().toISOString(),
      },
      tripColumns: [
        {
          dispatchStatus: 'planned',
          label: 'Planned',
          count: 1,
          trips: [
            {
              tripId: 't1',
              tripNumber: 'TR-1',
              title: 'Morning run',
              dispatchStatus: 'planned',
              assignedDriverPersonId: null,
              vehicleRefKey: null,
              scheduledStartAt: null,
              scheduledEndAt: null,
              loadCount: 0,
              createdByUserId: 'u1',
              createdAt: new Date().toISOString(),
              updatedAt: new Date().toISOString(),
              assignedAt: null,
              dispatchedAt: null,
              startedAt: null,
              completedAt: null,
              cancelledAt: null,
            },
          ],
        },
      ],
      driverRefs: { items: [{ personId: 'person-12345678', displayName: 'Alex Driver', mirroredAt: new Date().toISOString() }] },
      actions: [],
    })

    renderPanel(true)
    expect(await screen.findByText('Dispatch command center')).toBeTruthy()
    expect(screen.getByTestId('trip-column-planned')).toBeTruthy()
    expect(screen.getByText('Morning run')).toBeTruthy()
  })
})
