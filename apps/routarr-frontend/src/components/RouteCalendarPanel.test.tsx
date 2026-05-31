import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { RouteCalendarPanel } from './RouteCalendarPanel'

vi.mock('../api/client', () => ({
  getRouteCalendar: vi.fn().mockResolvedValue({
    scope: 'daily',
    windowStart: '2026-05-27T00:00:00Z',
    windowEnd: '2026-05-28T00:00:00Z',
    summary: {
      tripCount: 1,
      routeCount: 1,
      stopCount: 1,
      lateTripCount: 0,
      atRiskTripCount: 1,
    },
    days: [
      {
        date: '2026-05-27T00:00:00Z',
        events: [
          {
            eventType: 'trip',
            entityId: '11111111-1111-1111-1111-111111111111',
            label: 'North yard delivery',
            status: 'dispatched',
            scheduledAt: '2026-05-27T10:00:00Z',
            scheduledEndAt: '2026-05-27T13:00:00Z',
            tripId: '11111111-1111-1111-1111-111111111111',
            routeId: null,
            tripNumber: 'TR-20260527-AB12CD34',
            routeNumber: null,
            assignedDriverPersonId: 'driver-1',
            isLate: false,
            isAtRisk: true,
          },
          {
            eventType: 'stop',
            entityId: '22222222-2222-2222-2222-222222222222',
            label: 'Pickup yard',
            status: 'pending',
            scheduledAt: '2026-05-27T11:00:00Z',
            scheduledEndAt: null,
            tripId: '11111111-1111-1111-1111-111111111111',
            routeId: '33333333-3333-3333-3333-333333333333',
            tripNumber: 'TR-20260527-AB12CD34',
            routeNumber: 'RT-001',
            assignedDriverPersonId: 'driver-1',
            isLate: false,
            isAtRisk: false,
          },
        ],
      },
    ],
    generatedAt: '2026-05-27T12:05:00Z',
  }),
}))

describe('RouteCalendarPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders calendar day columns and events from the API', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const onScopeChange = vi.fn()

    render(
      <QueryClientProvider client={client}>
        <RouteCalendarPanel accessToken="token" scope="daily" onScopeChange={onScopeChange} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/Route calendar/)).toBeInTheDocument()
    expect(screen.getByText('North yard delivery')).toBeInTheDocument()
    expect(screen.getByText('Pickup yard')).toBeInTheDocument()
    expect(screen.getByText(/1 at-risk trip/)).toBeInTheDocument()
  })

  it('switches to weekly scope when requested', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const onScopeChange = vi.fn()

    render(
      <QueryClientProvider client={client}>
        <RouteCalendarPanel accessToken="token" scope="daily" onScopeChange={onScopeChange} />
      </QueryClientProvider>,
    )

    await screen.findByText(/Route calendar/)
    fireEvent.click(screen.getByRole('button', { name: 'Weekly' }))
    expect(onScopeChange).toHaveBeenCalledWith('weekly')
  })

  it('shows retry callout when calendar fails', async () => {
    const { getRouteCalendar } = await import('../api/client')
    vi.mocked(getRouteCalendar).mockRejectedValueOnce(new Error('calendar down'))
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const onScopeChange = vi.fn()

    render(
      <QueryClientProvider client={client}>
        <RouteCalendarPanel accessToken="token" scope="daily" onScopeChange={onScopeChange} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Route calendar unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry calendar' })).toBeInTheDocument()
  })
})
