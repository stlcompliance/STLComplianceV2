import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { deleteEquipmentAvailability, updateEquipmentAvailability } from '../api/client'
import { EquipmentAvailabilityPanel } from './EquipmentAvailabilityPanel'

vi.mock('../api/client', () => ({
  getEquipmentAvailabilityPanel: vi.fn().mockResolvedValue({
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
        availabilityId: '55555555-5555-5555-5555-555555555555',
        vehicleRefKey: 'truck-42',
        availabilityStatus: 'unavailable',
        startsAt: '2026-05-27T08:00:00Z',
        endsAt: '2026-05-27T18:00:00Z',
        reason: 'PM service',
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
  listVehicleRefs: vi.fn().mockResolvedValue({
    items: [
      {
        vehicleRefKey: 'truck-42',
        displayLabel: 'Truck 42',
        assetTag: 'TRK-42',
        mirroredAt: '2026-05-27T08:00:00Z',
        fromMirror: true,
      },
    ],
  }),
  createEquipmentAvailability: vi.fn(),
  updateEquipmentAvailability: vi.fn().mockResolvedValue({}),
  deleteEquipmentAvailability: vi.fn().mockResolvedValue(undefined),
}))

describe('EquipmentAvailabilityPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders availability records and conflict summary', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={client}>
        <EquipmentAvailabilityPanel
          accessToken="token"
          scope="daily"
          onScopeChange={vi.fn()}
          canManage={false}
        />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/Equipment availability/)).toBeInTheDocument()
    expect(screen.getByText(/PM service/)).toBeInTheDocument()
    expect(screen.getByText(/truck-42/)).toBeInTheDocument()
    expect(screen.getByText(/1 trip conflict/)).toBeInTheDocument()
    expect(screen.getByText(/TR-001/)).toBeInTheDocument()
  })

  it('toggles weekly scope', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const onScopeChange = vi.fn()

    render(
      <QueryClientProvider client={client}>
        <EquipmentAvailabilityPanel
          accessToken="token"
          scope="daily"
          onScopeChange={onScopeChange}
          canManage={true}
        />
      </QueryClientProvider>,
    )

    await screen.findByText(/Equipment availability/)
    fireEvent.click(screen.getByRole('button', { name: 'Weekly' }))
    expect(onScopeChange).toHaveBeenCalledWith('weekly')
  })

  it('edits and deletes availability when manager', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true)
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={client}>
        <EquipmentAvailabilityPanel
          accessToken="token"
          scope="daily"
          onScopeChange={vi.fn()}
          canManage={true}
        />
      </QueryClientProvider>,
    )

    await screen.findByText(/PM service/)
    fireEvent.click(screen.getByRole('button', { name: 'Edit' }))
    fireEvent.change(screen.getByDisplayValue('PM service'), { target: { value: 'Shop repair' } })
    fireEvent.click(screen.getByRole('button', { name: 'Save changes' }))

    await waitFor(() => {
      expect(updateEquipmentAvailability).toHaveBeenCalledWith(
        'token',
        '55555555-5555-5555-5555-555555555555',
        expect.objectContaining({
          availabilityStatus: 'unavailable',
          reason: 'Shop repair',
        }),
      )
    })

    fireEvent.click(screen.getByRole('button', { name: 'Delete' }))
    await waitFor(() => {
      expect(deleteEquipmentAvailability).toHaveBeenCalledWith(
        'token',
        '55555555-5555-5555-5555-555555555555',
      )
    })
  })
})
