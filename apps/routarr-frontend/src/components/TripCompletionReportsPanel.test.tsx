import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { TripCompletionReportsPanel } from './TripCompletionReportsPanel'

vi.mock('../api/client', () => ({
  getTripCompletions: vi.fn().mockResolvedValue({
    items: [
      {
        tripId: 'trip-1',
        tripNumber: 'T-100',
        title: 'North route',
        dispatchStatus: 'completed',
        assignedDriverPersonId: 'person-1',
        vehicleRefKey: 'VEH-1',
        scheduledStartAt: null,
        scheduledEndAt: null,
        startedAt: '2026-05-29T10:00:00Z',
        completedAt: '2026-05-29T12:00:00Z',
        cancelledAt: null,
        durationMinutes: 120,
        routeCount: 1,
        completedRouteCount: 1,
        stopCount: 2,
        completedStopCount: 2,
        skippedStopCount: 0,
        pendingStopCount: 0,
        loadCount: 1,
        deliveredLoadCount: 1,
        pendingLoadCount: 0,
        sourceUpdatedAt: '2026-05-29T12:00:00Z',
        computedAt: '2026-05-29T12:05:00Z',
        isMaterialized: true,
      },
    ],
  }),
  getTripCompletionDetail: vi.fn().mockResolvedValue({
    summary: {
      tripId: 'trip-1',
      tripNumber: 'T-100',
      title: 'North route',
      dispatchStatus: 'completed',
      assignedDriverPersonId: 'person-1',
      vehicleRefKey: 'VEH-1',
      scheduledStartAt: null,
      scheduledEndAt: null,
      startedAt: '2026-05-29T10:00:00Z',
      completedAt: '2026-05-29T12:00:00Z',
      cancelledAt: null,
      durationMinutes: 120,
      routeCount: 1,
      completedRouteCount: 1,
      stopCount: 2,
      completedStopCount: 2,
      skippedStopCount: 0,
      pendingStopCount: 0,
      loadCount: 1,
      deliveredLoadCount: 1,
      pendingLoadCount: 0,
      sourceUpdatedAt: '2026-05-29T12:00:00Z',
      computedAt: '2026-05-29T12:05:00Z',
      isMaterialized: true,
    },
    events: [
      {
        eventKind: 'trip_started',
        title: 'Trip started',
        detail: 'North route',
        occurredAt: '2026-05-29T10:00:00Z',
        sequenceNumber: 1,
        sourceEntityType: 'trip',
        sourceEntityId: 'trip-1',
      },
    ],
  }),
  getRouteCompletions: vi.fn().mockResolvedValue({
    items: [
      {
        routeId: 'route-1',
        routeNumber: 'R-10',
        title: 'North route',
        routeStatus: 'completed',
        tripId: 'trip-1',
        tripNumber: 'T-100',
        tripDispatchStatus: 'completed',
        stopCount: 2,
        completedStopCount: 2,
        skippedStopCount: 0,
        completedAt: '2026-05-29T12:00:00Z',
        computedAt: '2026-05-29T12:05:00Z',
        isMaterialized: true,
      },
    ],
  }),
}))

describe('TripCompletionReportsPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders trip completion summaries and export control', async () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <TripCompletionReportsPanel accessToken="token" canRead canExport />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('trip-completion-reports-panel')).toBeInTheDocument()
    expect(await screen.findByText('T-100 — North route')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Export CSV' })).toBeInTheDocument()
    expect(await screen.findByText('R-10 — North route')).toBeInTheDocument()
  })

  it('returns null when read access is denied', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <TripCompletionReportsPanel accessToken="token" canRead={false} canExport={false} />
      </QueryClientProvider>,
    )

    expect(screen.queryByTestId('trip-completion-reports-panel')).not.toBeInTheDocument()
  })
})
