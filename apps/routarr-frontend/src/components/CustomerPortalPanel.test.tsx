import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { CustomerPortalPanel } from './CustomerPortalPanel'

const {
  getTripByNumber,
  getRoute,
  getRoutes,
  getTripExecutionSummary,
  listDispatchExceptions,
} = vi.hoisted(() => ({
  getTripByNumber: vi.fn(),
  getRoute: vi.fn(),
  getRoutes: vi.fn(),
  getTripExecutionSummary: vi.fn(),
  listDispatchExceptions: vi.fn(),
}))

vi.mock('../api/client', () => ({
  getTripByNumber,
  getRoute,
  getRoutes,
  getTripExecutionSummary,
  listDispatchExceptions,
}))

describe('CustomerPortalPanel', () => {
  it('searches by trip number and renders trip visibility', async () => {
    const queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
      },
    })

    getTripByNumber.mockResolvedValue({
      tripId: '11111111-1111-1111-1111-111111111111',
      tripNumber: 'TR-20260603-0001',
      title: 'Customer shipment',
      description: 'Shipment to customer',
      dispatchStatus: 'dispatched',
      assignedDriverPersonId: 'driver-1',
      vehicleRefKey: 'TRUCK-17',
      scheduledStartAt: '2026-06-03T10:00:00Z',
      scheduledEndAt: '2026-06-03T16:00:00Z',
      loads: [
        {
          loadId: 'load-1',
          loadKey: 'LOAD-1',
          description: 'Load one',
          loadType: 'general',
          status: 'planned',
          sequenceNumber: 1,
          originLabel: 'Warehouse',
          destinationLabel: 'Customer site',
          createdAt: '2026-06-03T09:00:00Z',
          updatedAt: '2026-06-03T09:00:00Z',
        },
      ],
      createdByUserId: 'creator-1',
      createdAt: '2026-06-03T08:30:00Z',
      updatedAt: '2026-06-03T09:30:00Z',
      assignedAt: '2026-06-03T09:45:00Z',
      acceptedAt: null,
      dispatchedAt: '2026-06-03T10:15:00Z',
      startedAt: '2026-06-03T10:20:00Z',
      completedAt: null,
      closedAt: null,
      cancelledAt: null,
    })
    getRoutes.mockResolvedValue([
      {
        routeId: 'route-1',
        routeNumber: 'RT-0001',
        title: 'Customer route',
        routeStatus: 'active',
        tripId: '11111111-1111-1111-1111-111111111111',
        stopCount: 1,
        createdByUserId: 'creator-1',
        createdAt: '2026-06-03T08:30:00Z',
        updatedAt: '2026-06-03T09:30:00Z',
        activatedAt: null,
        completedAt: null,
        cancelledAt: null,
      },
    ])
    getRoute.mockResolvedValue({
      routeId: 'route-1',
      routeNumber: 'RT-0001',
      title: 'Customer route',
      description: 'Primary delivery route',
      routeStatus: 'active',
      tripId: '11111111-1111-1111-1111-111111111111',
      stops: [
        {
          stopId: 'stop-1',
          stopKey: 'stop-1',
          label: 'Customer site',
          addressLabel: '123 Main',
          stopType: 'delivery',
          stopStatus: 'pending',
          sequenceNumber: 1,
          geofenceAnchorLatitude: null,
          geofenceAnchorLongitude: null,
          geofenceRadiusMeters: null,
          lastGeofenceCheckAt: null,
          lastGeofenceResult: null,
          lastGeofenceDistanceMeters: null,
          lastGeofenceReportedLatitude: null,
          lastGeofenceReportedLongitude: null,
          scheduledArrivalAt: null,
          arrivedAt: null,
          completedAt: null,
          createdAt: '2026-06-03T08:30:00Z',
          updatedAt: '2026-06-03T09:30:00Z',
        },
      ],
      createdByUserId: 'creator-1',
      createdAt: '2026-06-03T08:30:00Z',
      updatedAt: '2026-06-03T09:30:00Z',
      activatedAt: null,
      completedAt: null,
      cancelledAt: null,
    })
    listDispatchExceptions.mockResolvedValue({
      totalCount: 1,
      openCount: 1,
      overdueCount: 0,
      items: [
        {
          exceptionId: 'exception-1',
          exceptionKey: 'EX-1',
          title: 'Customer delay',
          description: 'Waiting for dock',
          category: 'delay',
          status: 'open',
          tripId: '11111111-1111-1111-1111-111111111111',
          tripNumber: 'TR-20260603-0001',
          tripTitle: 'Customer shipment',
          assignedToUserId: null,
          slaDueAt: null,
          isSlaBreached: false,
          resolutionTemplateKey: 'manual_follow_up',
          resolutionNotes: '',
          createdByUserId: 'creator-1',
          createdAt: '2026-06-03T10:25:00Z',
          updatedAt: '2026-06-03T10:25:00Z',
          assignedAt: null,
          resolvedAt: null,
        },
      ],
    })
    getTripExecutionSummary.mockResolvedValue({
      tripId: '11111111-1111-1111-1111-111111111111',
      tripNumber: 'TR-20260603-0001',
      dispatchStatus: 'dispatched',
      assignedDriverPersonId: 'driver-1',
      closedAt: null,
      hasPreTripDvir: true,
      hasPostTripDvir: false,
      proofs: [
        {
          proofId: 'proof-1',
          tripId: '11111111-1111-1111-1111-111111111111',
          proofType: 'pickup_photo',
          capturedByPersonId: 'driver-1',
          vehicleRefKey: 'TRUCK-17',
          referenceKey: 'dock-1',
          notes: null,
          reviewStatus: 'pending_review',
          reviewedByPersonId: null,
          reviewedAt: null,
          reviewNotes: null,
          capturedAt: '2026-06-03T10:30:00Z',
          createdAt: '2026-06-03T10:30:00Z',
          attachments: [],
        },
      ],
      dvirInspections: [
        {
          dvirId: 'dvir-1',
          tripId: '11111111-1111-1111-1111-111111111111',
          phase: 'pre_trip',
          result: 'pass',
          vehicleRefKey: 'TRUCK-17',
          odometerReading: null,
          defectNotes: null,
          submittedByPersonId: 'driver-1',
          submittedAt: '2026-06-03T09:50:00Z',
          attachments: [],
        },
      ],
    })

    render(
      <QueryClientProvider client={queryClient}>
        <CustomerPortalPanel accessToken="token" canRead />
      </QueryClientProvider>,
    )

    fireEvent.change(screen.getByLabelText('Trip number'), {
      target: { value: 'TR-20260603-0001' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Search trip' }))

    expect(await screen.findByText('Customer portal')).toBeTruthy()
    await waitFor(() => {
      expect(getTripByNumber).toHaveBeenCalledWith('token', 'TR-20260603-0001')
      expect(getRoutes).toHaveBeenCalledWith('token', '11111111-1111-1111-1111-111111111111')
      expect(getTripExecutionSummary).toHaveBeenCalledWith(
        'token',
        '11111111-1111-1111-1111-111111111111',
      )
      expect(listDispatchExceptions).toHaveBeenCalledWith('token')
    })
    expect(screen.getByText('TR-20260603-0001 — Customer shipment')).toBeTruthy()
    expect(screen.getByText('Dispatch status')).toBeTruthy()
    expect(screen.getByRole('heading', { name: 'Loads' })).toBeTruthy()
    expect(screen.getByText('Proof archive')).toBeTruthy()
    await waitFor(() => {
      expect(
        screen.getAllByText((_, element) =>
          element?.textContent?.replace(/\s+/g, ' ').trim() ===
          '1 route(s) · 1 pending stop(s) · 1 open exception(s)',
        ).length,
      ).toBeGreaterThan(0)
    })
    await waitFor(() => {
      expect(
        screen.getAllByText((_, element) =>
          element?.textContent?.replace(/\s+/g, ' ').trim() ===
          'pickup_photo · dock-1 · pending review',
        ).length,
      ).toBeGreaterThan(0)
      expect(
        screen.getAllByText((_, element) =>
          element?.textContent?.replace(/\s+/g, ' ').trim() ===
          'pre_trip · pass · 6/3/2026, 4:50:00 AM',
        ).length,
      ).toBeGreaterThan(0)
    })
  })
})
