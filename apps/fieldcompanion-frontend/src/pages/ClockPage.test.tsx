import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ClockPage } from './ClockPage'

const {
  queueClockActionMock,
  setOfflineQueueState,
  submitClockEventMock,
  useOfflineQueueMock,
} = vi.hoisted(() => {
  const queueClockActionMock = vi.fn().mockResolvedValue(undefined)
  const submitClockEventMock = vi.fn()
  const defaultOfflineQueueState = {
    isOnline: false,
    pendingCount: 3,
    queueClockAction: queueClockActionMock,
    syncPending: vi.fn(),
  }
  let offlineQueueState = defaultOfflineQueueState

  return {
    queueClockActionMock,
    submitClockEventMock,
    setOfflineQueueState: (state: typeof defaultOfflineQueueState) => {
      offlineQueueState = state
    },
    resetOfflineQueueState: () => {
      offlineQueueState = defaultOfflineQueueState
    },
    useOfflineQueueMock: vi.fn(() => offlineQueueState),
  }
})

vi.mock('../api/client', () => ({
  getFieldCompanionClockStatus: vi.fn().mockResolvedValue({
    currentState: 'not_clocked_in',
    recentEvents: [
      {
        eventType: 'clock_in',
        eventTimestamp: '2026-06-26T12:00:00.000Z',
        capturedTimestamp: '2026-06-26T12:00:00.000Z',
        timezone: 'America/Chicago',
        id: 'clock-1',
        anomalyFlags: [],
        siteRef: null,
        locationRef: null,
      },
    ],
  }),
  submitFieldCompanionClockEvent: submitClockEventMock,
}))

vi.mock('../hooks/useFieldCompanionWorkspace', () => ({
  useFieldCompanionWorkspace: vi.fn(() => ({
    accessToken: 'token',
    meQuery: {
      data: {
        displayName: 'Alex Worker',
      },
    },
  })),
}))

vi.mock('../hooks/useOfflineQueue', () => ({
  useOfflineQueue: useOfflineQueueMock,
}))

describe('ClockPage', () => {
  afterEach(() => {
    cleanup()
    queueClockActionMock.mockClear()
    submitClockEventMock.mockReset()
    setOfflineQueueState({
      isOnline: false,
      pendingCount: 3,
      queueClockAction: queueClockActionMock,
      syncPending: vi.fn(),
    })
    Reflect.deleteProperty(navigator, 'geolocation')
  })

  it('renders clock status, worker context, and offline punch feedback', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={client}>
        <ClockPage />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Clock')).toBeInTheDocument()
    expect(screen.getByText('Alex Worker')).toBeInTheDocument()
    expect(screen.getByText('Offline now. New punches will queue on this device and replay automatically. 3 pending.')).toBeInTheDocument()
    expect(screen.getByText('Location is unavailable on this device. Punches still record without GPS.')).toBeInTheDocument()
    expect(screen.getByText('No punches yet')).toBeInTheDocument()
    expect(screen.getByText('Clock in')).toBeInTheDocument()
    expect(screen.getByText('Clock out')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Clock in' }))

    expect(await screen.findByText('clock in queued. It will sync to StaffArr when you are back online.')).toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-clock-feedback')).toHaveAttribute('aria-live', 'polite')
    expect(queueClockActionMock).toHaveBeenCalled()
  })

  it('records punches with geolocation when available and explains the privacy fallback', async () => {
    setOfflineQueueState({
      isOnline: true,
      pendingCount: 0,
      queueClockAction: queueClockActionMock,
      syncPending: vi.fn(),
    })

    Object.defineProperty(navigator, 'geolocation', {
      configurable: true,
      value: {
        getCurrentPosition: vi.fn((success: PositionCallback) =>
          success({
            coords: {
              accuracy: 25,
              altitude: null,
              altitudeAccuracy: null,
              heading: null,
              latitude: 38.123456,
              longitude: -90.654321,
              speed: null,
            },
            timestamp: Date.now(),
          } as GeolocationPosition),
        ),
      },
    })

    submitClockEventMock.mockResolvedValueOnce({
      clockEventId: 'clock-2',
      created: true,
      conflictDetected: false,
      status: 'success',
      currentState: 'clocked_in',
      event: {
        eventType: 'clock_in',
        eventTimestamp: '2026-06-26T12:15:00.000Z',
        capturedTimestamp: '2026-06-26T12:15:00.000Z',
        timezone: 'America/Chicago',
        id: 'clock-2',
        anomalyFlags: [],
        siteRef: null,
        locationRef: '38.123456,-90.654321',
      },
    })

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={client}>
        <ClockPage />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Location is optional. If you approve the browser prompt, Field Companion attaches a coarse GPS point to the punch. If you decline, the punch still records without location.')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Clock in' }))

    expect(await screen.findByText('Recorded clock in with device location.')).toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-clock-feedback')).toHaveAttribute('aria-live', 'polite')
    expect(submitClockEventMock).toHaveBeenCalledWith(
      'token',
      expect.objectContaining({
        eventType: 'clock_in',
        geoPoint: '38.123456,-90.654321',
        sourceDeviceId: expect.stringMatching(/ on /),
      }),
    )
    const submittedPayload = submitClockEventMock.mock.calls[0]?.[1]
    expect(submittedPayload.sourceDeviceId).not.toContain('Mozilla')
    expect(submittedPayload.sourceDeviceId).not.toContain('AppleWebKit')
  })
})
