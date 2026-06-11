import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { ExceptionsSection } from './ExceptionsSection'
import { ProofReviewSection } from './ProofReviewSection'
import { StopsSection } from './StopsSection'

vi.mock('../../components/DispatchExceptionQueuePanel', () => ({
  DispatchExceptionQueuePanel: ({ canTriage }: { canTriage: boolean }) => (
    <div data-testid="dispatch-exception-queue-panel">{canTriage ? 'triage-enabled' : 'triage-disabled'}</div>
  ),
}))

vi.mock('../../components/TripProofDvirReadPanel', () => ({
  TripProofDvirReadPanel: () => <div data-testid="trip-proof-dvir-read-panel" />,
}))

vi.mock('../../components/RoutesPanel', () => ({
  RoutesPanel: ({ mode, canCreate }: { mode: string; canCreate: boolean }) => (
    <div data-testid="routes-panel">{`${mode}:${String(canCreate)}`}</div>
  ),
}))

function buildState(roleKey = 'routarr_dispatcher', isPlatformAdmin = false) {
  return {
    session: {
      accessToken: 'token',
      userId: 'user-1',
    },
    roleKey,
    isPlatformAdmin,
    routesQuery: { data: [], isLoading: false },
    routeDetailQuery: { data: null, isLoading: false },
    selectedRouteId: '',
    selectedTripId: '',
    routeTitle: '',
    routeDescription: '',
    stopKey: '',
    stopLabel: '',
    stopAddress: '',
    stopType: 'pickup',
    stopScheduledArrivalAt: '',
    stopGeofenceAnchorLatitude: '',
    stopGeofenceAnchorLongitude: '',
    stopGeofenceRadiusMeters: '',
    linkRouteMutation: { isPending: false, mutate: vi.fn() },
    updateStopStatusMutation: { isPending: false, mutate: vi.fn() },
    setSelectedRouteId: vi.fn(),
    setRouteTitle: vi.fn(),
    setRouteDescription: vi.fn(),
    setStopKey: vi.fn(),
    setStopLabel: vi.fn(),
    setStopAddress: vi.fn(),
    setStopType: vi.fn(),
    setStopScheduledArrivalAt: vi.fn(),
    setStopGeofenceAnchorLatitude: vi.fn(),
    setStopGeofenceAnchorLongitude: vi.fn(),
    setStopGeofenceRadiusMeters: vi.fn(),
  } as never
}

describe('Operational RoutArr sections', () => {
  it('renders the exception queue without embedding report panels', () => {
    render(<ExceptionsSection state={buildState()} />)

    expect(screen.getByTestId('dispatch-exception-queue-panel')).toHaveTextContent('triage-enabled')
    expect(screen.queryByTestId('dispatch-reports-panel')).not.toBeInTheDocument()
  })

  it('renders the proof review read panel without report rollups', () => {
    render(<ProofReviewSection state={buildState()} />)

    expect(screen.getByTestId('trip-proof-dvir-read-panel')).toBeInTheDocument()
    expect(screen.queryByTestId('proof-dvir-reports-panel')).not.toBeInTheDocument()
  })

  it('renders the operational routes panel for stops workspace', () => {
    render(<StopsSection state={buildState()} />)

    expect(screen.getByTestId('routes-panel')).toHaveTextContent('details:false')
    expect(screen.queryByTestId('route-reports-panel')).not.toBeInTheDocument()
  })
})
