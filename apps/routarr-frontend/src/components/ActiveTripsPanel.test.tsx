import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
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
      <ActiveTripsPanel accessToken="token" scope="daily" />
    </QueryClientProvider>,
  )
}

describe('ActiveTripsPanel', () => {
  afterEach(() => cleanup())

  it('renders list and map views with late trip', async () => {
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
      },
      items: [
        {
          tripId: 'trip-1',
          tripNumber: 'TR-ACTIVE',
          title: 'Highway run',
          dispatchStatus: 'dispatched',
          assignedDriverPersonId: 'person-1',
          vehicleRefKey: 'VEH-1',
          scheduledStartAt: new Date().toISOString(),
          scheduledEndAt: new Date(Date.now() + 3600000).toISOString(),
          dispatchedAt: new Date().toISOString(),
          startedAt: null,
          isLate: true,
          isAtRisk: false,
          routeCount: 1,
          pendingStopCount: 2,
          timelineOffsetPercent: 10,
          timelineWidthPercent: 20,
        },
      ],
    })

    renderPanel()
    expect(await screen.findByText('Active trips')).toBeTruthy()
    expect(screen.getByTestId('active-trip-row-trip-1')).toBeTruthy()
    expect(screen.getByText('Late')).toBeTruthy()

    fireEvent.click(screen.getByRole('button', { name: 'map' }))
    expect(screen.getByTestId('active-trips-map')).toBeTruthy()
    expect(screen.getByTestId('active-trip-map-trip-1')).toBeTruthy()
  })
})
