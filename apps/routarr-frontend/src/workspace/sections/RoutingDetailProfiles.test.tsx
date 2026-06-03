import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import { fireEvent } from '@testing-library/react'

import { RouteProfile, TripProfile } from './RoutingDetailProfiles'

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

  it('renders route optimization preview and triggers optimize action', () => {
    const optimizeRouteMutation = {
      isPending: false,
      mutate: vi.fn(),
    }
    const state = {
      selectedRouteId: 'route-1',
      routesQuery: {
        data: [
          {
            routeId: 'route-1',
            routeNumber: 'RT-100',
            title: 'Scheduled route',
            routeStatus: 'draft',
            tripId: null,
            stopCount: 2,
            createdByUserId: 'user-1',
            createdAt: '2026-05-26T10:00:00Z',
            updatedAt: '2026-05-27T10:00:00Z',
            activatedAt: null,
            completedAt: null,
            cancelledAt: null,
          },
        ],
      },
      routeDetailQuery: {
        data: {
          routeId: 'route-1',
          routeNumber: 'RT-100',
          title: 'Scheduled route',
          description: 'Route with stops out of scheduled order.',
          routeStatus: 'draft',
          tripId: null,
          stops: [
            {
              stopId: 'stop-1',
              stopKey: 'stop-a',
              label: 'Late pickup',
              addressLabel: 'Site A',
              staffarrSiteOrgUnitId: null,
              staffarrSiteNameSnapshot: '',
              stopType: 'pickup',
              stopStatus: 'pending',
              sequenceNumber: 1,
              scheduledArrivalAt: '2026-05-27T11:00:00Z',
              arrivedAt: null,
              completedAt: null,
              createdAt: '2026-05-26T10:00:00Z',
              updatedAt: '2026-05-26T10:00:00Z',
            },
            {
              stopId: 'stop-2',
              stopKey: 'stop-b',
              label: 'Early delivery',
              addressLabel: 'Site B',
              staffarrSiteOrgUnitId: null,
              staffarrSiteNameSnapshot: '',
              stopType: 'delivery',
              stopStatus: 'pending',
              sequenceNumber: 2,
              scheduledArrivalAt: '2026-05-27T09:00:00Z',
              arrivedAt: null,
              completedAt: null,
              createdAt: '2026-05-26T10:00:00Z',
              updatedAt: '2026-05-26T10:00:00Z',
            },
          ],
          createdByUserId: 'user-1',
          createdAt: '2026-05-26T10:00:00Z',
          updatedAt: '2026-05-27T10:00:00Z',
          activatedAt: null,
          completedAt: null,
          cancelledAt: null,
        },
      },
      optimizeRouteMutation,
      tripDetailQuery: { data: null },
      tripAssetDispatchabilityQuery: { isLoading: false, data: null },
      selectedTripId: '',
      statusFilter: '',
    } as any

    render(
      <MemoryRouter>
        <RouteProfile state={state} />
      </MemoryRouter>,
    )

    expect(screen.getByText('Route optimization')).toBeInTheDocument()
    expect(screen.getByText(/re-ordered to better match scheduled arrivals/i)).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Optimize stop order' }))
    expect(optimizeRouteMutation.mutate).toHaveBeenCalledTimes(1)
  })

  it('runs a geofence check for a selected stop', () => {
    const checkRouteStopGeofenceMutation = {
      isPending: false,
      mutate: vi.fn(),
    }
    const state = {
      selectedRouteId: 'route-1',
      routesQuery: {
        data: [
          {
            routeId: 'route-1',
            routeNumber: 'RT-100',
            title: 'Geofence route',
            routeStatus: 'draft',
            tripId: null,
            stopCount: 1,
            createdByUserId: 'user-1',
            createdAt: '2026-05-26T10:00:00Z',
            updatedAt: '2026-05-27T10:00:00Z',
            activatedAt: null,
            completedAt: null,
            cancelledAt: null,
          },
        ],
      },
      routeDetailQuery: {
        data: {
          routeId: 'route-1',
          routeNumber: 'RT-100',
          title: 'Geofence route',
          description: 'Route with a geofence anchor.',
          routeStatus: 'draft',
          tripId: null,
          stops: [
            {
              stopId: 'stop-1',
              stopKey: 'stop-a',
              label: 'Warehouse gate',
              addressLabel: 'Site A',
              staffarrSiteOrgUnitId: null,
              staffarrSiteNameSnapshot: '',
              stopType: 'pickup',
              stopStatus: 'pending',
              sequenceNumber: 1,
              geofenceAnchorLatitude: 40.0001,
              geofenceAnchorLongitude: -105.0002,
              geofenceRadiusMeters: 250,
              lastGeofenceCheckAt: '2026-05-27T12:00:00Z',
              lastGeofenceResult: 'inside',
              lastGeofenceDistanceMeters: 12.3,
              lastGeofenceReportedLatitude: 40.0002,
              lastGeofenceReportedLongitude: -105.0001,
              scheduledArrivalAt: null,
              arrivedAt: null,
              completedAt: null,
              createdAt: '2026-05-26T10:00:00Z',
              updatedAt: '2026-05-27T12:00:00Z',
            },
          ],
          createdByUserId: 'user-1',
          createdAt: '2026-05-26T10:00:00Z',
          updatedAt: '2026-05-27T10:00:00Z',
          activatedAt: null,
          completedAt: null,
          cancelledAt: null,
        },
      },
      optimizeRouteMutation: { isPending: false, mutate: vi.fn() },
      checkRouteStopGeofenceMutation,
      tripDetailQuery: { data: null },
      tripAssetDispatchabilityQuery: { isLoading: false, data: null },
      selectedTripId: '',
      statusFilter: '',
    } as any

    render(
      <MemoryRouter>
        <RouteProfile state={state} />
      </MemoryRouter>,
    )

    expect(screen.getByRole('button', { name: 'Check geofence' })).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Check geofence' }))
    expect(screen.getByLabelText('Reported latitude')).toHaveValue('40.0001')
    expect(screen.getByLabelText('Reported longitude')).toHaveValue('-105.0002')
    fireEvent.click(screen.getByRole('button', { name: 'Run geofence check' }))
    expect(checkRouteStopGeofenceMutation.mutate).toHaveBeenCalledWith({
      stopId: 'stop-1',
      reportedLatitude: 40.0001,
      reportedLongitude: -105.0002,
    })
  })
})
