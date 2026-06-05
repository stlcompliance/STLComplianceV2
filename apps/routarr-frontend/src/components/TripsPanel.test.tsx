import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { TripsPanel } from './TripsPanel'

vi.mock('../api/client', () => ({
  listDrivers: vi.fn().mockResolvedValue({ items: [] }),
  listVehicleRefs: vi.fn().mockResolvedValue({ items: [] }),
}))

function renderPanel(overrides: Partial<Parameters<typeof TripsPanel>[0]> = {}) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <TripsPanel
        mode="create"
        accessToken="token"
        canCreate
        canAssign
        canPerform
        canManage
        viewAllTrips
        sessionPersonId="bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"
        trips={[
          {
            tripId: '11111111-1111-1111-1111-111111111111',
            tripNumber: 'TR-20260527-AB12CD34',
            title: 'North yard delivery',
            dispatchStatus: 'assigned',
            assignedDriverPersonId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
            vehicleRefKey: 'VEH-FL-100',
            scheduledStartAt: null,
            scheduledEndAt: null,
            loadCount: 1,
            createdByUserId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
            createdAt: '2026-05-27T12:00:00Z',
            updatedAt: '2026-05-27T12:00:00Z',
            assignedAt: '2026-05-27T12:05:00Z',
            dispatchedAt: null,
            startedAt: null,
            completedAt: null,
            cancelledAt: null,
            closedAt: null,
          },
        ]}
        selectedTrip={null}
        selectedTripId=""
        tripTitle=""
        tripDescription=""
        vehicleRefKey=""
        driverPersonId=""
        loadKey=""
        loadOrigin=""
        loadDestination=""
        statusFilter=""
        isLoading={false}
        isDetailLoading={false}
        isCreating={false}
        isAssigning={false}
        isUpdatingStatus={false}
        onSelectedTripIdChange={vi.fn()}
        onTripTitleChange={vi.fn()}
        onTripDescriptionChange={vi.fn()}
        onVehicleRefKeyChange={vi.fn()}
        onDriverPersonIdChange={vi.fn()}
        onLoadKeyChange={vi.fn()}
        onLoadOriginChange={vi.fn()}
        onLoadDestinationChange={vi.fn()}
        onStatusFilterChange={vi.fn()}
        onCreateTrip={vi.fn()}
        onAssignDriver={vi.fn()}
        onUpdateStatus={vi.fn()}
        {...overrides}
      />
    </QueryClientProvider>,
  )
}

describe('TripsPanel', () => {
  it('renders trip list and create form', () => {
    renderPanel()

    expect(screen.getByText('North yard delivery')).toBeInTheDocument()
    expect(screen.getByText('assigned')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Create trip' })).toBeInTheDocument()
    expect(screen.getByTestId('trip-create-vehicle-picker')).toBeInTheDocument()
  })
})
