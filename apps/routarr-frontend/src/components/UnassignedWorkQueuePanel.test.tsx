import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { UnassignedWorkQueuePanel } from './UnassignedWorkQueuePanel'

vi.mock('../api/client', () => ({
  getUnassignedWorkQueue: vi.fn(),
  assignTripDriver: vi.fn(),
  applyBulkDispatch: vi.fn(),
}))

import * as client from '../api/client'

function renderPanel(canAssign = true) {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={qc}>
      <UnassignedWorkQueuePanel accessToken="token" scope="daily" canAssign={canAssign} />
    </QueryClientProvider>,
  )
}

describe('UnassignedWorkQueuePanel', () => {
  afterEach(() => cleanup())

  it('renders unassigned trips with assign controls', async () => {
    vi.mocked(client.getUnassignedWorkQueue).mockResolvedValue({
      scope: 'daily',
      windowStart: new Date().toISOString(),
      windowEnd: new Date(Date.now() + 86400000).toISOString(),
      generatedAt: new Date().toISOString(),
      unassignedCount: 1,
      items: [
        {
          tripId: 'trip-u1',
          tripNumber: 'TR-U1',
          title: 'Needs driver',
          dispatchStatus: 'planned',
          scheduledStartAt: new Date().toISOString(),
          scheduledEndAt: null,
          isLate: false,
          isAtRisk: true,
          routeCount: 0,
          pendingStopCount: 0,
        },
      ],
      driverRefs: {
        items: [{ personId: 'person-1', displayName: 'Alex', mirroredAt: new Date().toISOString() }],
      },
    })

    renderPanel(true)
    expect(await screen.findByText('Unassigned work queue')).toBeTruthy()
    expect(screen.getByTestId('unassigned-trip-trip-u1')).toBeTruthy()
    expect(screen.getByTestId('bulk-assign-unassigned')).toBeTruthy()
  })
})
