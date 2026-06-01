import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { RoutesPanel } from './RoutesPanel'

describe('RoutesPanel', () => {
  it('renders route list and create form', () => {
    render(
      <RoutesPanel
        mode="create"
        canCreate
        canPerform
        viewAllRoutes
        routes={[
          {
            routeId: '11111111-1111-1111-1111-111111111111',
            routeNumber: 'RT-20260527-AB12CD34',
            title: 'North quarry loop',
            routeStatus: 'planned',
            tripId: '22222222-2222-2222-2222-222222222222',
            stopCount: 2,
            createdByUserId: '33333333-3333-3333-3333-333333333333',
            createdAt: '2026-05-27T12:00:00Z',
            updatedAt: '2026-05-27T12:00:00Z',
            activatedAt: null,
            completedAt: null,
            cancelledAt: null,
          },
        ]}
        selectedRoute={null}
        selectedRouteId=""
        selectedTripId=""
        routeTitle=""
        routeDescription=""
        stopKey=""
        stopLabel=""
        stopAddress=""
        stopType="pickup"
        isLoading={false}
        isDetailLoading={false}
        isCreating={false}
        isLinking={false}
        isUpdatingStop={false}
        onSelectedRouteIdChange={vi.fn()}
        onRouteTitleChange={vi.fn()}
        onRouteDescriptionChange={vi.fn()}
        onStopKeyChange={vi.fn()}
        onStopLabelChange={vi.fn()}
        onStopAddressChange={vi.fn()}
        onStopTypeChange={vi.fn()}
        onCreateRoute={vi.fn()}
        onLinkTrip={vi.fn()}
        onUpdateStopStatus={vi.fn()}
      />,
    )

    expect(screen.getByText('North quarry loop')).toBeInTheDocument()
    expect(screen.getByText('planned')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Create route' })).toBeInTheDocument()
  })
})
