import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'

import { TripProfile } from './RoutingDetailProfiles'

describe('TripProfile', () => {
  it('renders MaintainArr dispatch readiness in the trip profile', () => {
    const state = {
      selectedTripId: 'trip-1',
      tripsQuery: {
        data: [
          {
            tripId: 'trip-1',
            tripNumber: 'TRIP-100',
            title: 'Regional delivery',
            dispatchStatus: 'planned',
            assignedDriverPersonId: 'person-1',
            vehicleRefKey: 'veh-1',
            loadCount: 1,
            scheduledStartAt: '2026-05-27T10:00:00Z',
            scheduledEndAt: '2026-05-27T18:00:00Z',
            createdAt: '2026-05-26T10:00:00Z',
            completedAt: null,
            startedAt: null,
          },
        ],
      },
      tripDetailQuery: {
        data: {
          tripId: 'trip-1',
          tripNumber: 'TRIP-100',
          title: 'Regional delivery',
          description: 'Deliver regional load',
          dispatchStatus: 'planned',
          assignedDriverPersonId: 'person-1',
          vehicleRefKey: 'veh-1',
          loads: [
            {
              loadId: 'load-1',
              loadKey: 'LD-1',
              description: 'Load 1',
              originLabel: 'Warehouse A',
              destinationLabel: 'Customer B',
              status: 'loaded',
            },
          ],
          scheduledStartAt: '2026-05-27T10:00:00Z',
          scheduledEndAt: '2026-05-27T18:00:00Z',
          createdAt: '2026-05-26T10:00:00Z',
          completedAt: null,
          startedAt: null,
        },
      },
      tripAssetDispatchabilityQuery: {
        isLoading: false,
        data: {
          vehicleRefKey: 'veh-1',
          assetTag: 'TRK-01',
          outcome: 'warn',
          reasonCode: 'asset_dispatchability_warn',
          message: 'Asset dispatchability check returned warnings.',
          isBlocking: false,
          maintainArr: {
            assetId: 'asset-1',
            assetTag: 'TRK-01',
            readinessStatus: 'not_ready',
            readinessBasis: 'certifications',
            blockerCount: 1,
            primaryBlockerMessage: 'Inspection is overdue.',
          },
        },
      },
      routesQuery: { data: [] },
      routeDetailQuery: { data: null },
      selectedRouteId: '',
      statusFilter: '',
    } as any

    render(
      <MemoryRouter>
        <TripProfile state={state} />
      </MemoryRouter>,
    )

    expect(screen.getByText('Dispatch readiness')).toBeInTheDocument()
    expect(screen.getByText('Dispatch warning')).toBeInTheDocument()
    expect(screen.getByText(/MaintainArr asset TRK-01 is Not ready/i)).toBeInTheDocument()
    expect(screen.getByText(/Inspection is overdue/i)).toBeInTheDocument()
  })
})
