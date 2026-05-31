import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { deleteDriverAvailability, updateDriverAvailability } from '../api/client'
import { DriverAvailabilityPanel } from './DriverAvailabilityPanel'

vi.mock('../api/client', () => ({
  getDriverAvailabilityPanel: vi.fn().mockResolvedValue({
    scope: 'daily',
    windowStart: '2026-05-27T00:00:00Z',
    windowEnd: '2026-05-28T00:00:00Z',
    summary: {
      recordCount: 1,
      unavailableCount: 1,
      limitedCount: 0,
      availableCount: 0,
      conflictCount: 1,
    },
    records: [
      {
        availabilityId: '44444444-4444-4444-4444-444444444444',
        personId: 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
        availabilityStatus: 'unavailable',
        startsAt: '2026-05-27T08:00:00Z',
        endsAt: '2026-05-27T18:00:00Z',
        reason: 'PTO',
        hasConflict: true,
        conflictingTripCount: 1,
        conflictingTrips: [
          {
            tripId: '11111111-1111-1111-1111-111111111111',
            tripNumber: 'TR-001',
            title: 'North route',
            dispatchStatus: 'assigned',
            scheduledStartAt: '2026-05-27T09:00:00Z',
            scheduledEndAt: '2026-05-27T11:00:00Z',
          },
        ],
      },
    ],
    generatedAt: '2026-05-27T12:00:00Z',
  }),
  listDrivers: vi.fn().mockResolvedValue({
    items: [
      {
        personId: 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
        displayName: 'Alex Driver',
        mirroredAt: '2026-05-27T08:00:00Z',
      },
    ],
  }),
  createDriverAvailability: vi.fn(),
  updateDriverAvailability: vi.fn().mockResolvedValue({}),
  deleteDriverAvailability: vi.fn().mockResolvedValue(undefined),
}))

describe('DriverAvailabilityPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders availability records and conflict summary', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={client}>
        <DriverAvailabilityPanel
          accessToken="token"
          scope="daily"
          onScopeChange={vi.fn()}
          canManage={false}
          sessionPersonId="aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"
        />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/Driver availability/)).toBeInTheDocument()
    expect(screen.getByText(/PTO/)).toBeInTheDocument()
    expect(screen.getByText(/1 trip conflict/)).toBeInTheDocument()
    expect(screen.getByText(/TR-001/)).toBeInTheDocument()
  })

  it('toggles weekly scope', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const onScopeChange = vi.fn()

    render(
      <QueryClientProvider client={client}>
        <DriverAvailabilityPanel
          accessToken="token"
          scope="daily"
          onScopeChange={onScopeChange}
          canManage={true}
          sessionPersonId="aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"
        />
      </QueryClientProvider>,
    )

    await screen.findByText(/Driver availability/)
    fireEvent.click(screen.getByRole('button', { name: 'Weekly' }))
    expect(onScopeChange).toHaveBeenCalledWith('weekly')
  })

  it('edits and deletes availability when manager', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true)
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={client}>
        <DriverAvailabilityPanel
          accessToken="token"
          scope="daily"
          onScopeChange={vi.fn()}
          canManage={true}
          sessionPersonId="aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"
        />
      </QueryClientProvider>,
    )

    await screen.findByText(/PTO/)
    fireEvent.click(screen.getByRole('button', { name: 'Edit' }))
    expect(screen.getByRole('button', { name: 'Save changes' })).toBeInTheDocument()

    fireEvent.change(screen.getByDisplayValue('PTO'), { target: { value: 'Vacation' } })
    fireEvent.click(screen.getByRole('button', { name: 'Save changes' }))

    await waitFor(() => {
      expect(updateDriverAvailability).toHaveBeenCalledWith(
        'token',
        '44444444-4444-4444-4444-444444444444',
        expect.objectContaining({
          availabilityStatus: 'unavailable',
          reason: 'Vacation',
        }),
      )
    })

    fireEvent.click(screen.getByRole('button', { name: 'Delete' }))
    await waitFor(() => {
      expect(deleteDriverAvailability).toHaveBeenCalledWith(
        'token',
        '44444444-4444-4444-4444-444444444444',
      )
    })
  })

  it('renders retryable error callout when panel query fails', async () => {
    vi.mocked(client.getDriverAvailabilityPanel).mockRejectedValueOnce(
      new Error('driver availability unavailable'),
    )
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <DriverAvailabilityPanel
          accessToken="token"
          scope="daily"
          onScopeChange={vi.fn()}
          canManage={false}
          sessionPersonId="aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"
        />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('driver availability unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry driver availability' })).toBeInTheDocument()
  })
})
