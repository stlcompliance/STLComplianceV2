import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, within } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { UnassignedWorkQueuePanel } from './UnassignedWorkQueuePanel'

vi.mock('@stl/shared-ui', () => ({
  ApiErrorCallout: ({
    title,
    message,
    retryLabel,
    onRetry,
  }: {
    title: string
    message: string
    retryLabel?: string
    onRetry?: () => void
  }) => (
    <div>
      <h3>{title}</h3>
      <p>{message}</p>
      {retryLabel && onRetry ? <button type="button" onClick={onRetry}>{retryLabel}</button> : null}
    </div>
  ),
  StaticSearchPicker: ({
    label,
    value,
    options,
    onChange,
    placeholder,
    testId,
    disabled,
  }: {
    label?: string
    value: string
    options: Array<{ value: string; label: string }>
    onChange: (value: string) => void
    placeholder?: string
    testId?: string
    disabled?: boolean
  }) => {
    const query = value.trim().toLowerCase()
    const visibleOptions = query
      ? options.filter((option) =>
          option.label.toLowerCase().includes(query) || option.value.toLowerCase().includes(query),
        )
      : options

    return (
      <div data-testid={testId}>
        <label>
          {label}
          <input
            aria-label={label ?? placeholder ?? 'Static search picker'}
            value={value}
            onChange={(event) => onChange(event.target.value)}
            disabled={disabled}
          />
        </label>
        {visibleOptions.length > 0 ? (
          <div>
            {visibleOptions.map((option) => (
              <button key={option.value} type="button" onClick={() => onChange(option.value)}>
                {option.label}
              </button>
            ))}
          </div>
        ) : null}
      </div>
    )
  },
  getErrorMessage: (error: unknown, fallback: string) => (error instanceof Error ? error.message : fallback),
}))

vi.mock('../api/client', () => ({
  getUnassignedWorkQueue: vi.fn(),
  assignTripDriver: vi.fn(),
  applyBulkDispatch: vi.fn(),
  previewBulkDispatch: vi.fn(),
  previewDispatchAssignment: vi.fn(),
}))

import * as client from '../api/client'

function renderPanel(canAssign = true) {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={qc}>
      <UnassignedWorkQueuePanel accessToken="token" scope="daily" canAssign={canAssign} />
    </QueryClientProvider>,
  )
}

describe('UnassignedWorkQueuePanel', () => {
  afterEach(() => {
    cleanup()
    vi.restoreAllMocks()
  })

  it('renders unassigned trips with assign controls and urgent summary', async () => {
    vi.mocked(client.getUnassignedWorkQueue).mockResolvedValue({
      scope: 'daily',
      windowStart: new Date().toISOString(),
      windowEnd: new Date(Date.now() + 86400000).toISOString(),
      generatedAt: new Date().toISOString(),
      summary: {
        unassignedCount: 1,
        lateCount: 0,
        atRiskCount: 1,
        urgentCount: 1,
      },
      items: [
        {
          tripId: 'trip-u1',
          tripNumber: 'TR-U1',
          title: 'Needs driver',
          dispatchStatus: 'planned',
          scheduledStartAt: new Date().toISOString(),
          scheduledEndAt: null,
          isLate: false,
          isAtRisk: true,
          routeCount: 0,
          pendingStopCount: 0,
          minutesUntilStart: 45,
        },
      ],
      driverRefs: {
        items: [{ personId: 'person-1', displayName: 'Alex', mirroredAt: new Date().toISOString() }],
      },
    })

    renderPanel(true)
    expect(await screen.findByText('Unassigned work queue')).toBeTruthy()
    expect(screen.getByText(/1 urgent/)).toBeTruthy()
    expect(screen.getByTestId('unassigned-trip-trip-u1')).toBeTruthy()
    expect(screen.getByTestId('bulk-assign-unassigned')).toBeTruthy()
    expect(screen.getByTestId('unassigned-attention-filter')).toBeTruthy()
  })

  it('passes attention filter to API', async () => {
    vi.mocked(client.getUnassignedWorkQueue).mockResolvedValue({
      scope: 'daily',
      windowStart: new Date().toISOString(),
      windowEnd: new Date(Date.now() + 86400000).toISOString(),
      generatedAt: new Date().toISOString(),
      summary: {
        unassignedCount: 0,
        lateCount: 0,
        atRiskCount: 0,
        urgentCount: 0,
      },
      items: [],
      driverRefs: { items: [] },
    })

    renderPanel(true)
    await screen.findByText('Unassigned work queue')
    fireEvent.click(screen.getByTestId('unassigned-attention-filter'))

    expect(client.getUnassignedWorkQueue).toHaveBeenCalledWith('token', 'daily', {
      attentionOnly: true,
    })
  })

  it('passes ignoreWorkflowGateBlocks when user confirms bulk workflow gate override', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true)

    vi.mocked(client.getUnassignedWorkQueue).mockResolvedValue({
      scope: 'daily',
      windowStart: new Date().toISOString(),
      windowEnd: new Date(Date.now() + 86400000).toISOString(),
      generatedAt: new Date().toISOString(),
      summary: {
        unassignedCount: 1,
        lateCount: 0,
        atRiskCount: 0,
        urgentCount: 0,
      },
      items: [
        {
          tripId: 'trip-u1',
          tripNumber: 'TR-U1',
          title: 'Needs driver',
          dispatchStatus: 'planned',
          scheduledStartAt: new Date().toISOString(),
          scheduledEndAt: null,
          isLate: false,
          isAtRisk: false,
          routeCount: 0,
          pendingStopCount: 0,
          minutesUntilStart: 45,
        },
      ],
      driverRefs: {
        items: [{ personId: 'person-1', displayName: 'Alex', mirroredAt: new Date().toISOString() }],
      },
    })

    vi.mocked(client.previewBulkDispatch).mockResolvedValue({
      summary: { total: 1, canApplyCount: 0, blockedCount: 1 },
      items: [
        {
          tripId: 'trip-u1',
          tripNumber: 'TR-U1',
          title: 'Needs driver',
          currentDispatchStatus: 'planned',
          canApply: false,
          hasBlockingConflicts: true,
          driverPreview: {
            tripId: 'trip-u1',
            assignmentKind: 'driver',
            canAssign: false,
            hasBlockingConflicts: true,
            blockingDriverAvailability: [],
            blockingEquipmentAvailability: [],
            overlappingTrips: [],
            driverEligibility: null,
            assetDispatchability: null,
            workflowGates: {
              outcome: 'block',
              reasonCode: 'license_invalid',
              message: 'Driver license invalid',
              isBlocking: true,
              gates: [],
            },
          },
          vehiclePreview: null,
          statusPreview: null,
        },
      ],
    })

    vi.mocked(client.applyBulkDispatch).mockResolvedValue({
      summary: { total: 1, successCount: 1, failureCount: 0 },
      results: [{ tripId: 'trip-u1', success: true, errorCode: null, errorMessage: null, trip: null }],
    })

    renderPanel(true)
    await screen.findByTestId('unassigned-trip-trip-u1')

    fireEvent.click(screen.getByLabelText('Select Needs driver'))
    const bulkPicker = screen.getByTestId('unassigned-bulk-driver-picker')
    const bulkDriverInput = within(bulkPicker).getByLabelText(/Bulk assign driver/i)
    fireEvent.focus(bulkDriverInput)
    fireEvent.change(bulkDriverInput, { target: { value: 'Alex' } })
    fireEvent.click(within(bulkPicker).getByRole('button', { name: 'Alex' }))
    fireEvent.click(screen.getByTestId('bulk-assign-unassigned'))

    await vi.waitFor(() => {
      expect(client.applyBulkDispatch).toHaveBeenCalledWith('token', {
        items: [{ tripId: 'trip-u1', driverPersonId: 'person-1' }],
        ignoreAvailabilityConflicts: false,
        ignoreEligibilityBlocks: false,
        ignoreDispatchabilityBlocks: false,
        ignoreWorkflowGateBlocks: true,
      })
    })
  })

  it('shows inline gate preview when a driver is selected on a trip row', async () => {
    vi.mocked(client.getUnassignedWorkQueue).mockResolvedValue({
      scope: 'daily',
      windowStart: new Date().toISOString(),
      windowEnd: new Date(Date.now() + 86400000).toISOString(),
      generatedAt: new Date().toISOString(),
      summary: {
        unassignedCount: 1,
        lateCount: 0,
        atRiskCount: 0,
        urgentCount: 0,
      },
      items: [
        {
          tripId: 'trip-u1',
          tripNumber: 'TR-U1',
          title: 'Needs driver',
          dispatchStatus: 'planned',
          scheduledStartAt: new Date().toISOString(),
          scheduledEndAt: null,
          isLate: false,
          isAtRisk: false,
          routeCount: 0,
          pendingStopCount: 0,
          minutesUntilStart: 45,
        },
      ],
      driverRefs: {
        items: [{ personId: 'person-1', displayName: 'Alex', mirroredAt: new Date().toISOString() }],
      },
    })

    vi.mocked(client.previewDispatchAssignment).mockResolvedValue({
      tripId: 'trip-u1',
      assignmentKind: 'driver',
      canAssign: false,
      hasBlockingConflicts: true,
      blockingDriverAvailability: [],
      blockingEquipmentAvailability: [],
      overlappingTrips: [],
      driverEligibility: null,
      assetDispatchability: null,
      workflowGates: {
        outcome: 'block',
        reasonCode: 'license_invalid',
        message: 'Driver license invalid',
        isBlocking: true,
        gates: [
          {
            gateKey: 'driver_qualification',
            outcome: 'block',
            reasonCode: 'license_invalid',
            message: 'Driver license invalid',
            isBlocking: true,
          },
        ],
      },
    })

    renderPanel(true)
    await screen.findByTestId('unassigned-trip-trip-u1')

    const rowPicker = screen.getByTestId('unassigned-driver-picker-trip-u1')
    const rowDriverInput = within(rowPicker).getByLabelText(/Assign driver for Needs driver/i)
    fireEvent.focus(rowDriverInput)
    fireEvent.change(rowDriverInput, { target: { value: 'Alex' } })
    fireEvent.click(within(rowPicker).getByRole('button', { name: 'Alex' }))

    expect(await screen.findByTestId('unassigned-gate-preview-trip-u1')).toBeTruthy()
    expect(screen.getByText('driver_qualification')).toBeTruthy()
  })
})
