import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { TripExecutionWorkspacePanel } from './TripExecutionWorkspacePanel'

vi.mock('../api/client', () => ({
  getTrip: vi.fn(),
  getDispatchReportTripDetail: vi.fn(),
  getRoutes: vi.fn(),
  getRoute: vi.fn(),
  getTripCaptureReadiness: vi.fn(),
  getTripExecutionSummary: vi.fn(),
  getTripAuditTrail: vi.fn(),
  updateTripStatus: vi.fn(),
  submitTripDvir: vi.fn(),
}))

import * as client from '../api/client'

function renderPanel() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={qc}>
      <MemoryRouter>
        <TripExecutionWorkspacePanel
          accessToken="token"
          tripId="trip-1"
          canPerform={true}
          canManage={true}
        />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

afterEach(() => {
  cleanup()
  vi.clearAllMocks()
})

describe('TripExecutionWorkspacePanel', () => {
  it('renders trip execution sections with readiness and audit trail', async () => {
    vi.mocked(client.getTrip).mockResolvedValue({
      tripId: 'trip-1',
      tripNumber: 'TR-001',
      title: 'Morning run',
      description: 'Test trip',
      dispatchStatus: 'dispatched',
      assignedDriverPersonId: 'person-1',
      vehicleRefKey: 'VEH-1',
      scheduledStartAt: new Date().toISOString(),
      scheduledEndAt: new Date().toISOString(),
      loads: [],
      createdByUserId: 'user-1',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      assignedAt: null,
      dispatchedAt: null,
      startedAt: null,
      completedAt: null,
      cancelledAt: null,
      closedAt: null,
    })
    vi.mocked(client.getDispatchReportTripDetail).mockResolvedValue({
      tripId: 'trip-1',
      tripNumber: 'TR-001',
      title: 'Morning run',
      description: 'Test trip',
      dispatchStatus: 'dispatched',
      assignedDriverPersonId: 'person-1',
      vehicleRefKey: 'VEH-1',
      scheduledStartAt: new Date().toISOString(),
      scheduledEndAt: new Date().toISOString(),
      dispatchedAt: null,
      startedAt: null,
      completedAt: null,
      cancelledAt: null,
      isLate: false,
      isAtRisk: true,
      routeCount: 1,
      pendingStopCount: 2,
      linkedExceptionCount: 0,
      delayExceptionCount: 0,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    })
    vi.mocked(client.getRoutes).mockResolvedValue([
      {
        routeId: 'route-1',
        routeNumber: 'RT-001',
        title: 'Main route',
        routeStatus: 'active',
        tripId: 'trip-1',
        stopCount: 2,
        createdByUserId: 'user-1',
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        activatedAt: null,
        completedAt: null,
        cancelledAt: null,
      },
    ])
    vi.mocked(client.getRoute).mockResolvedValue({
      routeId: 'route-1',
      routeNumber: 'RT-001',
      title: 'Main route',
      description: '',
      routeStatus: 'active',
      tripId: 'trip-1',
      stops: [
        {
          stopId: 'stop-1',
          stopKey: 'pickup',
          label: 'Pickup',
          addressLabel: '123 Main',
          stopType: 'pickup',
          stopStatus: 'pending',
          sequenceNumber: 1,
          scheduledArrivalAt: null,
          arrivedAt: null,
          completedAt: null,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        },
      ],
      createdByUserId: 'user-1',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      activatedAt: null,
      completedAt: null,
      cancelledAt: null,
    })
    vi.mocked(client.getTripCaptureReadiness).mockResolvedValue({
      tripId: 'trip-1',
      dispatchStatus: 'dispatched',
      canStartTrip: false,
      canCompleteTrip: false,
      items: [
        {
          key: 'pre_trip_dvir',
          label: 'Pre-trip DVIR',
          satisfied: false,
          required: true,
          message: 'Submit pre-trip DVIR',
        },
      ],
    })
    vi.mocked(client.getTripExecutionSummary).mockResolvedValue({
      tripId: 'trip-1',
      tripNumber: 'TR-001',
      dispatchStatus: 'dispatched',
      assignedDriverPersonId: 'person-1',
      closedAt: null,
      proofs: [],
      dvirInspections: [],
      hasPreTripDvir: false,
      hasPostTripDvir: false,
    })
    vi.mocked(client.getTripAuditTrail).mockResolvedValue({
      tripId: 'trip-1',
      entries: [
        {
          auditEventId: 'audit-1',
          actorUserId: 'user-1',
          action: 'trip.status',
          targetType: 'trip',
          targetId: 'trip-1',
          result: 'dispatched',
          reasonCode: null,
          correlationId: 'corr-1',
          occurredAt: new Date().toISOString(),
        },
      ],
    })

    renderPanel()

    await waitFor(() => {
      expect(screen.getByTestId('trip-execution-workspace-panel')).toBeInTheDocument()
      expect(screen.getByTestId('trip-workspace-readiness')).toBeInTheDocument()
    })

    expect(screen.getByText('Morning run')).toBeInTheDocument()
    expect(screen.getByTestId('trip-workspace-readiness')).toBeInTheDocument()
    expect(screen.getByTestId('trip-workspace-dvir-capture')).toBeInTheDocument()
    expect(screen.getByTestId('trip-workspace-route-route-1')).toBeInTheDocument()
    expect(screen.getByTestId('trip-workspace-audit-trail')).toBeInTheDocument()
    expect(screen.getByText('trip.status')).toBeInTheDocument()
    expect(screen.getByText(/Trip is at risk/)).toBeInTheDocument()
  })

  it('shows operator DVIR capture forms and submits via trip API', async () => {
    vi.mocked(client.getTrip).mockResolvedValue({
      tripId: 'trip-1',
      tripNumber: 'TR-001',
      title: 'Morning run',
      description: 'Test trip',
      dispatchStatus: 'dispatched',
      assignedDriverPersonId: 'person-1',
      vehicleRefKey: 'VEH-1',
      scheduledStartAt: new Date().toISOString(),
      scheduledEndAt: new Date().toISOString(),
      loads: [],
      createdByUserId: 'user-1',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      assignedAt: null,
      dispatchedAt: null,
      startedAt: null,
      completedAt: null,
      cancelledAt: null,
      closedAt: null,
    })
    vi.mocked(client.getDispatchReportTripDetail).mockResolvedValue({
      tripId: 'trip-1',
      tripNumber: 'TR-001',
      title: 'Morning run',
      description: 'Test trip',
      dispatchStatus: 'dispatched',
      assignedDriverPersonId: 'person-1',
      vehicleRefKey: 'VEH-1',
      scheduledStartAt: new Date().toISOString(),
      scheduledEndAt: new Date().toISOString(),
      dispatchedAt: null,
      startedAt: null,
      completedAt: null,
      cancelledAt: null,
      isLate: false,
      isAtRisk: false,
      routeCount: 0,
      pendingStopCount: 0,
      linkedExceptionCount: 0,
      delayExceptionCount: 0,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    })
    vi.mocked(client.getRoutes).mockResolvedValue([])
    vi.mocked(client.getTripCaptureReadiness).mockResolvedValue({
      tripId: 'trip-1',
      dispatchStatus: 'dispatched',
      canStartTrip: false,
      canCompleteTrip: false,
      items: [],
    })
    vi.mocked(client.getTripExecutionSummary).mockResolvedValue({
      tripId: 'trip-1',
      tripNumber: 'TR-001',
      dispatchStatus: 'dispatched',
      assignedDriverPersonId: 'person-1',
      closedAt: null,
      proofs: [],
      dvirInspections: [],
      hasPreTripDvir: false,
      hasPostTripDvir: false,
    })
    vi.mocked(client.getTripAuditTrail).mockResolvedValue({
      tripId: 'trip-1',
      entries: [],
    })
    vi.mocked(client.submitTripDvir).mockResolvedValue({
      dvirId: 'dvir-1',
      tripId: 'trip-1',
      phase: 'pre_trip',
      vehicleRefKey: 'VEH-1',
      result: 'pass',
      odometerReading: 12000,
      defectNotes: '',
      submittedByPersonId: 'dispatcher-1',
      submittedAt: new Date().toISOString(),
      attachments: [],
    })

    renderPanel()

    await waitFor(() => {
      expect(screen.getByTestId('trip-workspace-dvir-capture')).toBeInTheDocument()
    })

    fireEvent.click(screen.getByRole('button', { name: /Submit pre-trip DVIR/i }))

    await waitFor(() => {
      expect(client.submitTripDvir).toHaveBeenCalledWith('token', 'trip-1', {
        phase: 'pre_trip',
        result: 'pass',
        vehicleRefKey: 'VEH-1',
        odometerReading: undefined,
        defectNotes: undefined,
      })
    })
  })
})
