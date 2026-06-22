import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { BulkDispatchPanel } from './BulkDispatchPanel'

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
  AdvancedReferenceField: ({
    label,
    value,
    onChange,
    testId,
  }: {
    label?: string
    value: string
    onChange: (value: string) => void
    testId?: string
  }) => (
    <label>
      {label}
      <input aria-label={label} data-testid={testId} value={value} onChange={(event) => onChange(event.target.value)} />
    </label>
  ),
  ControlledSelect: ({
    label,
    value,
    options,
    onChange,
    emptyLabel,
    testId,
    className,
  }: {
    label?: string
    value: string
    options: Array<{ value: string; label: string }>
    onChange: (value: string) => void
    emptyLabel?: string
    testId?: string
    className?: string
  }) => (
    <label>
      {label}
      <select
        aria-label={label}
        data-testid={testId}
        className={className}
        value={value}
        onChange={(event) => onChange(event.target.value)}
      >
        <option value="">{emptyLabel ?? 'Select…'}</option>
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </label>
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

const { previewBulkDispatch, applyBulkDispatch } = vi.hoisted(() => ({
  previewBulkDispatch: vi.fn(),
  applyBulkDispatch: vi.fn(),
}))

vi.mock('../api/client', () => ({
  getTrips: vi.fn().mockResolvedValue([
    {
      tripId: '11111111-1111-1111-1111-111111111111',
      tripNumber: 'TR-1',
      title: 'North run',
      dispatchStatus: 'planned',
      assignedDriverPersonId: null,
      vehicleRefKey: null,
      scheduledStartAt: '2026-05-27T10:00:00Z',
      scheduledEndAt: '2026-05-27T13:00:00Z',
      loadCount: 0,
      createdByUserId: 'user-1',
      createdAt: '2026-05-27T08:00:00Z',
      updatedAt: '2026-05-27T08:00:00Z',
      assignedAt: null,
      dispatchedAt: null,
      startedAt: null,
      completedAt: null,
      cancelledAt: null,
    },
    {
      tripId: '22222222-2222-2222-2222-222222222222',
      tripNumber: 'TR-2',
      title: 'South run',
      dispatchStatus: 'assigned',
      assignedDriverPersonId: 'driver-1',
      vehicleRefKey: null,
      scheduledStartAt: '2026-05-27T14:00:00Z',
      scheduledEndAt: '2026-05-27T17:00:00Z',
      loadCount: 0,
      createdByUserId: 'user-1',
      createdAt: '2026-05-27T08:00:00Z',
      updatedAt: '2026-05-27T08:00:00Z',
      assignedAt: '2026-05-27T08:30:00Z',
      dispatchedAt: null,
      startedAt: null,
      completedAt: null,
      cancelledAt: null,
    },
  ]),
  listDrivers: vi.fn().mockResolvedValue({
    items: [{ personId: 'driver-bulk-1', displayName: 'Bulk Driver', mirroredAt: '2026-05-27T08:00:00Z' }],
  }),
  listVehicleRefs: vi.fn().mockResolvedValue({ items: [] }),
  previewBulkDispatch,
  applyBulkDispatch,
}))

describe('BulkDispatchPanel', () => {
  afterEach(() => {
    cleanup()
    vi.restoreAllMocks()
  })

  it('previews and applies bulk driver assignment', async () => {
    previewBulkDispatch.mockResolvedValue({
      summary: { total: 1, canApplyCount: 1, blockedCount: 0 },
      items: [
        {
          tripId: '11111111-1111-1111-1111-111111111111',
          tripNumber: 'TR-1',
          title: 'North run',
          currentDispatchStatus: 'planned',
          canApply: true,
          hasBlockingConflicts: false,
          driverPreview: null,
          vehiclePreview: null,
          statusPreview: null,
        },
      ],
    })
    applyBulkDispatch.mockResolvedValue({
      summary: { total: 1, successCount: 1, failureCount: 0 },
      results: [{ tripId: '11111111-1111-1111-1111-111111111111', success: true, errorCode: null, errorMessage: null, trip: null }],
    })

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <BulkDispatchPanel accessToken="token" canAssign />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Bulk dispatch')).toBeTruthy()

    fireEvent.click(screen.getByTestId('bulk-trip-11111111-1111-1111-1111-111111111111'))
    const driverInput = screen.getByLabelText('Driver person')
    fireEvent.focus(driverInput)
    fireEvent.change(driverInput, { target: { value: 'Bulk' } })
    fireEvent.click(await screen.findByRole('button', { name: 'Bulk Driver' }))
    fireEvent.click(screen.getByText('Preview conflicts'))

    await vi.waitFor(() => {
      expect(previewBulkDispatch).toHaveBeenCalledWith('token', {
        items: [
          {
            tripId: '11111111-1111-1111-1111-111111111111',
            driverPersonId: 'driver-bulk-1',
            vehicleRefKey: null,
            dispatchStatus: null,
          },
        ],
      })
    })

    fireEvent.click(screen.getByTestId('bulk-dispatch-apply'))

    await vi.waitFor(() => {
      expect(applyBulkDispatch).toHaveBeenCalledWith('token', {
        items: [
          {
            tripId: '11111111-1111-1111-1111-111111111111',
            driverPersonId: 'driver-bulk-1',
            vehicleRefKey: null,
            dispatchStatus: null,
          },
        ],
        ignoreAvailabilityConflicts: false,
        ignoreEligibilityBlocks: false,
        ignoreDispatchabilityBlocks: false,
        ignoreWorkflowGateBlocks: false,
      })
    })
  })

  it('passes ignoreWorkflowGateBlocks when user confirms workflow gate override', async () => {
    previewBulkDispatch.mockResolvedValue({
      summary: { total: 1, canApplyCount: 0, blockedCount: 1 },
      items: [
        {
          tripId: '11111111-1111-1111-1111-111111111111',
          tripNumber: 'TR-1',
          title: 'North run',
          currentDispatchStatus: 'planned',
          canApply: false,
          hasBlockingConflicts: true,
          driverPreview: {
            tripId: '11111111-1111-1111-1111-111111111111',
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
    applyBulkDispatch.mockResolvedValue({
      summary: { total: 1, successCount: 1, failureCount: 0 },
      results: [
        {
          tripId: '11111111-1111-1111-1111-111111111111',
          success: true,
          errorCode: null,
          errorMessage: null,
          trip: null,
        },
      ],
    })

    vi.spyOn(window, 'confirm').mockReturnValue(true)

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <BulkDispatchPanel accessToken="token" canAssign />
      </QueryClientProvider>,
    )

    fireEvent.click(await screen.findByTestId('bulk-trip-11111111-1111-1111-1111-111111111111'))
    const driverInput = screen.getByLabelText('Driver person')
    fireEvent.focus(driverInput)
    fireEvent.change(driverInput, { target: { value: 'Bulk' } })
    fireEvent.click(await screen.findByRole('button', { name: 'Bulk Driver' }))
    fireEvent.click(screen.getByText('Preview conflicts'))

    await vi.waitFor(() => {
      expect(
        screen.getByTestId('bulk-preview-summary-11111111-1111-1111-1111-111111111111'),
      ).toHaveTextContent(/Driver license invalid/)
    })

    fireEvent.click(screen.getByTestId('bulk-dispatch-apply'))

    await vi.waitFor(() => {
      expect(applyBulkDispatch).toHaveBeenCalledWith('token', {
        items: [
          {
            tripId: '11111111-1111-1111-1111-111111111111',
            driverPersonId: 'driver-bulk-1',
            vehicleRefKey: null,
            dispatchStatus: null,
          },
        ],
        ignoreAvailabilityConflicts: false,
        ignoreEligibilityBlocks: false,
        ignoreDispatchabilityBlocks: false,
        ignoreWorkflowGateBlocks: true,
      })
    })
  })

  it('shows callout when preview request fails', async () => {
    previewBulkDispatch.mockRejectedValueOnce(new Error('preview unavailable'))

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <BulkDispatchPanel accessToken="token" canAssign />
      </QueryClientProvider>,
    )

    fireEvent.click(await screen.findByTestId('bulk-trip-11111111-1111-1111-1111-111111111111'))
    const driverInput = screen.getByLabelText('Driver person')
    fireEvent.focus(driverInput)
    fireEvent.change(driverInput, { target: { value: 'Bulk' } })
    fireEvent.click(await screen.findByRole('button', { name: 'Bulk Driver' }))
    fireEvent.click(screen.getByText('Preview conflicts'))

    expect(await screen.findByText('preview unavailable')).toBeTruthy()
    expect(screen.getByTestId('bulk-dispatch-error')).toBeTruthy()
  })
})
