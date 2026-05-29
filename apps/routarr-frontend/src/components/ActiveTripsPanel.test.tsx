import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'

import { cleanup, fireEvent, render, screen } from '@testing-library/react'

import { afterEach, describe, expect, it, vi } from 'vitest'

import { ActiveTripsPanel } from './ActiveTripsPanel'



vi.mock('../api/client', () => ({

  getActiveTrips: vi.fn(),

}))



import * as client from '../api/client'



function renderPanel() {

  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })

  render(
    <QueryClientProvider client={qc}>
      <MemoryRouter>
        <ActiveTripsPanel accessToken="token" scope="daily" />
      </MemoryRouter>
    </QueryClientProvider>,
  )

}



const baseTrip = {

  tripId: 'trip-1',

  tripNumber: 'TR-ACTIVE',

  title: 'Highway run',

  dispatchStatus: 'dispatched',

  assignedDriverPersonId: 'person-1',

  assignedDriverDisplayName: 'Alex Driver',

  vehicleRefKey: 'VEH-1',

  scheduledStartAt: new Date().toISOString(),

  scheduledEndAt: new Date(Date.now() + 3600000).toISOString(),

  dispatchedAt: new Date().toISOString(),

  startedAt: null,

  isLate: true,

  isAtRisk: false,

  routeCount: 1,

  pendingStopCount: 2,

  completedStopCount: 1,

  totalStopCount: 3,

  stopProgressPercent: 33,

  openExceptionCount: 2,

  timelineOffsetPercent: 10,

  timelineWidthPercent: 20,

}



describe('ActiveTripsPanel', () => {

  afterEach(() => cleanup())



  it('renders list and map views with late trip, progress, and filters', async () => {

    vi.mocked(client.getActiveTrips).mockResolvedValue({

      scope: 'daily',

      windowStart: new Date().toISOString(),

      windowEnd: new Date(Date.now() + 86400000).toISOString(),

      generatedAt: new Date().toISOString(),

      summary: {

        totalCount: 1,

        lateCount: 1,

        atRiskCount: 0,

        dispatchedCount: 1,

        inProgressCount: 0,

        unassignedCount: 0,

        openExceptionCount: 2,

      },

      items: [baseTrip],

    })



    renderPanel()

    expect(await screen.findByText('Active trips')).toBeTruthy()

    expect(screen.getByTestId('active-trip-row-trip-1')).toBeTruthy()

    expect(screen.getByText('Late')).toBeTruthy()

    expect(screen.getByText(/Alex Driver/)).toBeTruthy()

    expect(screen.getByTestId('active-trip-progress-trip-1')).toBeTruthy()

    expect(screen.getByTestId('active-trip-exceptions-trip-1')).toBeTruthy()

    expect(screen.getByTestId('active-trips-attention-filter')).toBeTruthy()

    expect(screen.getByTestId('active-trips-status-filter')).toBeTruthy()



    fireEvent.click(screen.getByRole('button', { name: 'map' }))

    expect(screen.getByTestId('active-trips-map')).toBeTruthy()

    expect(screen.getByTestId('active-trip-map-trip-1')).toBeTruthy()

  })



  it('passes attention filter to API', async () => {

    vi.mocked(client.getActiveTrips).mockResolvedValue({

      scope: 'daily',

      windowStart: new Date().toISOString(),

      windowEnd: new Date(Date.now() + 86400000).toISOString(),

      generatedAt: new Date().toISOString(),

      summary: {

        totalCount: 0,

        lateCount: 0,

        atRiskCount: 0,

        dispatchedCount: 0,

        inProgressCount: 0,

        unassignedCount: 0,

        openExceptionCount: 0,

      },

      items: [],

    })



    renderPanel()

    await screen.findByText('Active trips')

    fireEvent.click(screen.getByTestId('active-trips-attention-filter'))



    expect(client.getActiveTrips).toHaveBeenCalledWith('token', 'daily', {

      attentionOnly: true,

      statusFilter: 'all',

    })

  })

})


