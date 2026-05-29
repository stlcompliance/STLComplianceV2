import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { DriverPortalPanel } from './DriverPortalPanel'

vi.mock('../api/client', () => ({
  getDriverPortalSchedule: vi.fn(),
  getDriverPortalTripExecution: vi.fn(),
  getDriverPortalCaptureReadiness: vi.fn(),
  createDriverPortalTripProof: vi.fn(),
  submitDriverPortalTripDvir: vi.fn(),
  uploadDriverPortalCaptureAttachment: vi.fn(),
  readFileAsDataUrl: vi.fn(),
  dispatchDriverPortalTrip: vi.fn(),
  startDriverPortalTrip: vi.fn(),
  completeDriverPortalTrip: vi.fn(),
  closeDriverPortalTrip: vi.fn(),
  reportDriverPortalTripException: vi.fn(),
}))

import * as client from '../api/client'

function renderPanel() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={qc}>
      <DriverPortalPanel accessToken="token" />
    </QueryClientProvider>,
  )
}

describe('DriverPortalPanel', () => {
  afterEach(() => cleanup())

  it('renders today trip and starts via driver portal API', async () => {
    vi.mocked(client.getDriverPortalSchedule).mockResolvedValue({
      todayStart: new Date().toISOString(),
      todayEnd: new Date(Date.now() + 86400000).toISOString(),
      upcomingEnd: new Date(Date.now() + 7 * 86400000).toISOString(),
      generatedAt: new Date().toISOString(),
      todayTrips: [
        {
          tripId: 'trip-1',
          tripNumber: 'TR-DRIVER',
          title: 'Morning haul',
          dispatchStatus: 'dispatched',
          vehicleRefKey: 'VEH-1',
          scheduledStartAt: new Date().toISOString(),
          scheduledEndAt: new Date(Date.now() + 3600000).toISOString(),
          dispatchedAt: new Date().toISOString(),
          startedAt: null,
          completedAt: null,
          closedAt: null,
          canDispatch: false,
          canStart: true,
          canComplete: false,
          canClose: false,
          proofCount: 0,
          hasPreTripDvir: false,
          hasPostTripDvir: false,
          captureStartReady: true,
          captureCompleteReady: true,
        },
      ],
      upcomingTrips: [],
    })
    vi.mocked(client.getDriverPortalCaptureReadiness).mockResolvedValue({
      tripId: 'trip-1',
      dispatchStatus: 'dispatched',
      canStartTrip: true,
      canCompleteTrip: true,
      items: [],
    })
    vi.mocked(client.getDriverPortalTripExecution).mockResolvedValue({
      tripId: 'trip-1',
      tripNumber: 'TR-DRIVER',
      dispatchStatus: 'dispatched',
      assignedDriverPersonId: 'person-1',
      closedAt: null,
      proofs: [],
      dvirInspections: [],
      hasPreTripDvir: false,
      hasPostTripDvir: false,
    })
    vi.mocked(client.startDriverPortalTrip).mockResolvedValue({
      tripId: 'trip-1',
      tripNumber: 'TR-DRIVER',
      title: 'Morning haul',
      dispatchStatus: 'in_progress',
    } as never)

    renderPanel()
    expect(await screen.findByText('Driver portal')).toBeTruthy()
    expect(screen.getByTestId('driver-portal-trip-trip-1')).toBeTruthy()

    fireEvent.click(screen.getByRole('button', { name: 'Start trip' }))
    await waitFor(() => expect(client.startDriverPortalTrip).toHaveBeenCalledWith('token', 'trip-1'))
  })

  it('submits driver-reported exception via driver portal API', async () => {
    vi.mocked(client.getDriverPortalSchedule).mockResolvedValue({
      todayStart: new Date().toISOString(),
      todayEnd: new Date(Date.now() + 86400000).toISOString(),
      upcomingEnd: new Date(Date.now() + 7 * 86400000).toISOString(),
      generatedAt: new Date().toISOString(),
      todayTrips: [
        {
          tripId: 'trip-2',
          tripNumber: 'TR-EX',
          title: 'Afternoon run',
          dispatchStatus: 'in_progress',
          vehicleRefKey: null,
          scheduledStartAt: new Date().toISOString(),
          scheduledEndAt: null,
          dispatchedAt: new Date().toISOString(),
          startedAt: new Date().toISOString(),
          completedAt: null,
          closedAt: null,
          canDispatch: false,
          canStart: false,
          canComplete: true,
          canClose: false,
          proofCount: 0,
          hasPreTripDvir: true,
          hasPostTripDvir: false,
          captureStartReady: true,
          captureCompleteReady: true,
        },
      ],
      upcomingTrips: [],
    })
    vi.mocked(client.reportDriverPortalTripException).mockResolvedValue({
      exceptionId: 'ex-1',
      exceptionKey: 'DEX-TEST',
      title: 'Delay',
      description: '[Driver-reported] jam',
      category: 'delay',
      status: 'open',
      tripId: 'trip-2',
      tripNumber: 'TR-EX',
      tripTitle: 'Afternoon run',
      assignedToUserId: null,
      slaDueAt: new Date().toISOString(),
      isSlaBreached: false,
      resolutionTemplateKey: '',
      resolutionNotes: '',
      createdByUserId: 'user-1',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      assignedAt: null,
      resolvedAt: null,
    })

    renderPanel()
    expect(await screen.findByText('Afternoon run')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Report exception' }))
    fireEvent.change(screen.getByPlaceholderText('Short title (required)'), {
      target: { value: 'Traffic delay' },
    })
    fireEvent.change(screen.getByPlaceholderText('What happened?'), {
      target: { value: 'Stopped on highway' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Submit to dispatch' }))

    await waitFor(() =>
      expect(client.reportDriverPortalTripException).toHaveBeenCalledWith('token', 'trip-2', {
        title: 'Traffic delay',
        description: 'Stopped on highway',
        exceptionType: 'traffic_delay',
      }),
    )
    expect(await screen.findByText(/Exception DEX-TEST reported/)).toBeTruthy()
  })
})
