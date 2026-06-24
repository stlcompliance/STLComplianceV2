import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, within } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { DispatchAssignmentPanel } from './DispatchAssignmentPanel'

const { previewDispatchAssignment, assignTripDriver, assignTripVehicle } = vi.hoisted(() => ({
  previewDispatchAssignment: vi.fn(),
  assignTripDriver: vi.fn(),
  assignTripVehicle: vi.fn(),
}))

vi.mock('../api/client', () => ({
  getTrips: vi.fn().mockResolvedValue([
    {
      tripId: '11111111-1111-1111-1111-111111111111',
      tripNumber: 'TR-1',
      title: 'North run',
      dispatchStatus: 'planned',
      assignedDriverPersonId: null,
      vehicleRefKey: null,
      scheduledStartAt: '2026-05-27T10:00:00Z',
      scheduledEndAt: '2026-05-27T13:00:00Z',
      loadCount: 0,
      createdByUserId: 'user-1',
      createdAt: '2026-05-27T08:00:00Z',
      updatedAt: '2026-05-27T08:00:00Z',
      assignedAt: null,
      dispatchedAt: null,
      startedAt: null,
      completedAt: null,
      cancelledAt: null,
    },
  ]),
  getDriverAvailabilityPanel: vi.fn().mockResolvedValue({
    scope: 'daily',
    windowStart: '2026-05-27T00:00:00Z',
    windowEnd: '2026-05-28T00:00:00Z',
    summary: {
      recordCount: 1,
      unavailableCount: 0,
      limitedCount: 0,
      availableCount: 1,
      conflictCount: 0,
    },
    records: [
      {
        availabilityId: 'aaaa',
        personId: 'driver-person-1',
        availabilityStatus: 'available',
        startsAt: '2026-05-27T08:00:00Z',
        endsAt: '2026-05-27T18:00:00Z',
        reason: '',
        hasConflict: false,
        conflictingTripCount: 0,
        conflictingTrips: [],
      },
    ],
    generatedAt: '2026-05-27T12:00:00Z',
  }),
  getEquipmentAvailabilityPanel: vi.fn().mockResolvedValue({
    scope: 'daily',
    windowStart: '2026-05-27T00:00:00Z',
    windowEnd: '2026-05-28T00:00:00Z',
    summary: {
      recordCount: 1,
      unavailableCount: 0,
      limitedCount: 0,
      availableCount: 1,
      conflictCount: 0,
    },
    records: [
      {
        availabilityId: 'bbbb',
        vehicleRefKey: 'vehicle-1',
        availabilityStatus: 'available',
        startsAt: '2026-05-27T08:00:00Z',
        endsAt: '2026-05-27T18:00:00Z',
        reason: '',
        hasConflict: false,
        conflictingTripCount: 0,
        conflictingTrips: [],
      },
    ],
    generatedAt: '2026-05-27T12:00:00Z',
  }),
  previewDispatchAssignment,
  assignTripDriver,
  assignTripVehicle,
}))

describe('DispatchAssignmentPanel', () => {
  afterEach(() => {
    cleanup()
    vi.restoreAllMocks()
  })

  it('renders driver and trip drop targets', async () => {
    previewDispatchAssignment.mockResolvedValue({
      tripId: '11111111-1111-1111-1111-111111111111',
      assignmentKind: 'driver',
      canAssign: true,
      hasBlockingConflicts: true,
      blockingDriverAvailability: [
        {
          availabilityId: 'availability-1',
          personId: 'driver-person-1',
          availabilityStatus: 'unavailable',
          startsAt: '2026-05-27T08:00:00Z',
          endsAt: '2026-05-27T18:00:00Z',
          reason: 'PTO',
          hasConflict: true,
          conflictingTripCount: 1,
          conflictingTrips: [],
        },
      ],
      blockingEquipmentAvailability: [],
      overlappingTrips: [],
      driverEligibility: null,
      assetDispatchability: null,
      workflowGates: null,
    })
    assignTripDriver.mockResolvedValue({})

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <DispatchAssignmentPanel accessToken="token" scope="daily" canAssign />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Drag-and-drop assignment')).toBeTruthy()
    expect(screen.getByTestId('driver-chip-driver-person-1')).toBeTruthy()
    expect(screen.getByTestId('trip-drop-11111111-1111-1111-1111-111111111111')).toBeTruthy()

    const dropTarget = screen.getByTestId('trip-drop-11111111-1111-1111-1111-111111111111')
    fireEvent.drop(dropTarget, {
      dataTransfer: {
        getData: () => JSON.stringify({ kind: 'driver', personId: 'driver-person-1' }),
      },
    })

    expect(await screen.findByRole('alertdialog')).toBeTruthy()
    fireEvent.click(within(screen.getByRole('alertdialog')).getByRole('button', { name: /assign/i }))

    await vi.waitFor(() => {
      expect(previewDispatchAssignment).toHaveBeenCalled()
      expect(assignTripDriver).toHaveBeenCalled()
    })
  })

  it('shows callout when assignment preview fails', async () => {
    previewDispatchAssignment.mockRejectedValueOnce(new Error('assignment preview failed'))

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <DispatchAssignmentPanel accessToken="token" scope="daily" canAssign />
      </QueryClientProvider>,
    )

    const dropTarget = await screen.findByTestId('trip-drop-11111111-1111-1111-1111-111111111111')
    fireEvent.drop(dropTarget, {
      dataTransfer: {
        getData: () => JSON.stringify({ kind: 'driver', personId: 'driver-person-1' }),
      },
    })

    expect(await screen.findByText('assignment preview failed')).toBeTruthy()
    expect(screen.getByTestId('dispatch-assignment-error')).toBeTruthy()
  })
})
