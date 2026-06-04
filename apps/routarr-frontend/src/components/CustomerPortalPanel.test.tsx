import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { CustomerPortalPanel } from './CustomerPortalPanel'

const {
  getTripByNumber,
  getDispatchReportTripDetail,
  getProofDvirReportTripDetail,
} = vi.hoisted(() => ({
  getTripByNumber: vi.fn(),
  getDispatchReportTripDetail: vi.fn(),
  getProofDvirReportTripDetail: vi.fn(),
}))

vi.mock('../api/client', () => ({
  getTripByNumber,
  getDispatchReportTripDetail,
  getProofDvirReportTripDetail,
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
    getDispatchReportTripDetail.mockResolvedValue({
      tripId: '11111111-1111-1111-1111-111111111111',
      tripNumber: 'TR-20260603-0001',
      title: 'Customer shipment',
      description: 'Shipment to customer',
      dispatchStatus: 'dispatched',
      assignedDriverPersonId: 'driver-1',
      vehicleRefKey: 'TRUCK-17',
      scheduledStartAt: '2026-06-03T10:00:00Z',
      scheduledEndAt: '2026-06-03T16:00:00Z',
      dispatchedAt: '2026-06-03T10:15:00Z',
      startedAt: '2026-06-03T10:20:00Z',
      completedAt: null,
      cancelledAt: null,
      isLate: false,
      isAtRisk: true,
      routeCount: 1,
      pendingStopCount: 1,
      missingRequiredProofCount: 0,
      linkedExceptionCount: 1,
      delayExceptionCount: 1,
      createdAt: '2026-06-03T08:30:00Z',
      updatedAt: '2026-06-03T09:30:00Z',
      dispatchReleaseSnapshot: null,
    })
    getProofDvirReportTripDetail.mockResolvedValue({
      tripId: '11111111-1111-1111-1111-111111111111',
      tripNumber: 'TR-20260603-0001',
      title: 'Customer shipment',
      dispatchStatus: 'dispatched',
      assignedDriverPersonId: 'driver-1',
      vehicleRefKey: 'TRUCK-17',
      scheduledStartAt: '2026-06-03T10:00:00Z',
      scheduledEndAt: '2026-06-03T16:00:00Z',
      proofCount: 1,
      hasPreTripDvir: true,
      hasPostTripDvir: false,
      missingRequiredProofCount: 0,
      failOrConditionalDvirCount: 0,
      proofs: [
        {
          proofId: 'proof-1',
          tripId: '11111111-1111-1111-1111-111111111111',
          tripNumber: 'TR-20260603-0001',
          proofType: 'pickup_photo',
          capturedByPersonId: 'driver-1',
          vehicleRefKey: 'TRUCK-17',
          referenceKey: 'dock-1',
          reviewStatus: 'pending_review',
          capturedAt: '2026-06-03T10:30:00Z',
        },
      ],
      dvirInspections: [
        {
          dvirId: 'dvir-1',
          tripId: '11111111-1111-1111-1111-111111111111',
          tripNumber: 'TR-20260603-0001',
          phase: 'pre_trip',
          result: 'pass',
          vehicleRefKey: 'TRUCK-17',
          submittedByPersonId: 'driver-1',
          submittedAt: '2026-06-03T09:50:00Z',
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
    })
    expect(screen.getByText('TR-20260603-0001 — Customer shipment')).toBeTruthy()
    expect(screen.getByText('Dispatch status')).toBeTruthy()
    expect(screen.getByRole('heading', { name: 'Loads' })).toBeTruthy()
    expect(screen.getByText('Proof archive')).toBeTruthy()
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
