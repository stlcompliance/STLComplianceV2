import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { TripExecutionWorkspacePanel } from './TripExecutionWorkspacePanel'

vi.mock('../api/client', () => ({
  getTrip: vi.fn(),
  getRoutes: vi.fn(),
  getRoute: vi.fn(),
  getTripCaptureReadiness: vi.fn(),
  getTripExecutionSummary: vi.fn(),
  listDispatchExceptions: vi.fn(),
  overrideTripSupplierReadiness: vi.fn(),
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
          canDispatch={true}
          canPerform={true}
          canManage={true}
          canOverrideSupplierReadiness={true}
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
    vi.mocked(client.listDispatchExceptions).mockResolvedValue({
      totalCount: 1,
      openCount: 1,
      overdueCount: 0,
      items: [
        {
          exceptionId: 'exception-1',
          exceptionKey: 'EX-1',
          title: 'Delay at stop',
          description: 'Carrier delay reported',
          category: 'delay',
          status: 'open',
          tripId: 'trip-1',
          tripNumber: 'TR-001',
          tripTitle: 'Morning run',
          assignedToUserId: null,
          slaDueAt: null,
          isSlaBreached: false,
          resolutionTemplateKey: 'manual_follow_up',
          resolutionNotes: '',
          createdByUserId: 'user-1',
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
          assignedAt: null,
          resolvedAt: null,
        },
      ],
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

    expect(screen.getByRole('heading', { name: 'Morning run' })).toBeInTheDocument()
    expect(screen.getByTestId('trip-workspace-readiness')).toBeInTheDocument()
    expect(screen.getByTestId('trip-workspace-dvir-capture')).toBeInTheDocument()
    expect(screen.getByTestId('trip-workspace-route-route-1')).toBeInTheDocument()
    expect(screen.getByTestId('trip-workspace-audit-trail')).toBeInTheDocument()
    expect(screen.getByText('trip.status')).toBeInTheDocument()
    expect(screen.getByText('At risk')).toBeInTheDocument()
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
    vi.mocked(client.listDispatchExceptions).mockResolvedValue({
      totalCount: 0,
      openCount: 0,
      overdueCount: 0,
      items: [],
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

  it('shows retryable callout when trip query fails', async () => {
    vi.mocked(client.getTrip).mockRejectedValueOnce(new Error('trip workspace unavailable'))

    renderPanel()

    expect(await screen.findByText('trip workspace unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry trip workspace' })).toBeInTheDocument()
  })

  it('allows authorized users to override an active supplier-readiness block', async () => {
    vi.mocked(client.getTrip).mockResolvedValue({
      tripId: 'trip-1',
      tripNumber: 'TR-001',
      title: 'Morning run',
      description: 'Test trip',
      dispatchStatus: 'assigned',
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
      supplierOrderId: 'supplier-order-1',
      brokerOrderId: null,
      dispatchBlockReason: 'supplier_order_not_complete',
      supplierReadinessStatusSnapshot: 'partially_ready',
      supplierQuantityReadySnapshot: 200,
      supplierOrderedQuantitySnapshot: 520,
      supplierExpectedReadyAtSnapshot: null,
      supplierConfirmedReadyAtSnapshot: null,
      releasedForDispatchAt: null,
      releasedForDispatchByEventId: null,
      dispatchOverrideAt: null,
      dispatchOverrideByPersonId: null,
      dispatchOverrideReason: null,
      dispatchBlocks: [
        {
          dispatchBlockId: 'block-1',
          blockType: 'supplier_readiness',
          blockReason: 'supplier_order_partially_ready',
          blockingEntityType: 'supplier_order',
          blockingEntityId: 'supplier-order-1',
          status: 'active',
          createdAt: new Date().toISOString(),
          resolvedAt: null,
          resolvedByEventId: null,
          resolvedByPersonId: null,
          overrideReason: null,
        },
      ],
      dispatchReleaseSnapshot: null,
    })
    vi.mocked(client.getRoutes).mockResolvedValue([])
    vi.mocked(client.getTripCaptureReadiness).mockResolvedValue({
      tripId: 'trip-1',
      dispatchStatus: 'assigned',
      canStartTrip: true,
      canCompleteTrip: false,
      items: [],
    })
    vi.mocked(client.getTripExecutionSummary).mockResolvedValue({
      tripId: 'trip-1',
      tripNumber: 'TR-001',
      dispatchStatus: 'assigned',
      assignedDriverPersonId: 'person-1',
      closedAt: null,
      proofs: [],
      dvirInspections: [],
      hasPreTripDvir: true,
      hasPostTripDvir: false,
    })
    vi.mocked(client.listDispatchExceptions).mockResolvedValue({
      totalCount: 0,
      openCount: 0,
      overdueCount: 0,
      items: [],
    })
    vi.mocked(client.getTripAuditTrail).mockResolvedValue({
      tripId: 'trip-1',
      entries: [],
    })
    vi.mocked(client.overrideTripSupplierReadiness).mockResolvedValue({
      tripId: 'trip-1',
      tripNumber: 'TR-001',
      title: 'Morning run',
      description: 'Test trip',
      dispatchStatus: 'assigned',
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
      supplierOrderId: 'supplier-order-1',
      brokerOrderId: null,
      dispatchBlockReason: null,
      supplierReadinessStatusSnapshot: 'partially_ready',
      supplierQuantityReadySnapshot: 200,
      supplierOrderedQuantitySnapshot: 520,
      supplierExpectedReadyAtSnapshot: null,
      supplierConfirmedReadyAtSnapshot: null,
      releasedForDispatchAt: null,
      releasedForDispatchByEventId: null,
      dispatchOverrideAt: new Date().toISOString(),
      dispatchOverrideByPersonId: 'person-99',
      dispatchOverrideReason: 'Approved after phone confirmation',
      dispatchBlocks: [],
      dispatchReleaseSnapshot: null,
    })

    renderPanel()

    expect(await screen.findByText('Supplier-readiness block is active.')).toBeInTheDocument()

    fireEvent.change(screen.getByLabelText('Override reason'), {
      target: { value: 'Approved after phone confirmation' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Override supplier-readiness block' }))

    await waitFor(() => {
      expect(client.overrideTripSupplierReadiness).toHaveBeenCalledWith('token', 'trip-1', {
        reason: 'Approved after phone confirmation',
      })
    })
  })
})
