import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { StockReservationsPanel } from './StockReservationsPanel'

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@stl/shared-ui')>()

  return {
    ...actual,
    StaticSearchPicker: ({
      label,
      value,
      onChange,
      options,
      placeholder,
      testId,
    }: {
      label: string
      value: string
      onChange: (value: string) => void
      options: Array<{ value: string; label: string }>
      placeholder?: string
      testId?: string
    }) => (
      <label>
        <span>{label}</span>
        <input
          aria-label={label}
          data-testid={testId}
          placeholder={placeholder}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        />
        <div data-testid={`${testId ?? 'picker'}-options`}>
          {options.map((option) => (
            <span key={option.value}>{option.label}</span>
          ))}
        </div>
      </label>
    ),
  }
})

const baseProps = {
  reservations: [
    {
      reservationId: 'rsv-1',
      reservationKey: 'RSV-WO-001',
      status: 'active',
      sourceType: 'manual',
      sourceReferenceId: null,
      partId: 'part-1',
      partKey: 'filter-001',
      partDisplayName: 'Oil filter',
      binId: 'bin-1',
      binKey: 'main-bin',
      binName: 'Main Bin',
      locationId: 'loc-1',
      locationKey: 'wh-1',
      locationName: 'Warehouse',
      partStockLevelId: 'stock-1',
      quantityReserved: 2,
      notes: 'Hold for WO',
      createdByUserId: 'user-1',
      fulfilledByUserId: null,
      fulfilledAt: null,
      releasedByUserId: null,
      releasedAt: null,
      releaseReason: '',
      createdAt: '2026-05-28T00:00:00Z',
      updatedAt: '2026-05-28T00:00:00Z',
    },
  ],
  stockLevels: [
    {
      stockLevelId: 'stock-1',
      partId: 'part-1',
      partKey: 'filter-001',
      partDisplayName: 'Oil filter',
      binId: 'bin-1',
      binKey: 'main-bin',
      binName: 'Main Bin',
      locationId: 'loc-1',
      locationKey: 'wh-1',
      locationName: 'Warehouse',
      quantityOnHand: 10,
      quantityReserved: 2,
      quantityAvailable: 8,
      createdAt: '2026-05-28T00:00:00Z',
      updatedAt: '2026-05-28T00:00:00Z',
    },
  ],
  parts: [{ partId: 'part-1', partKey: 'filter-001', displayName: 'Oil filter' } as never],
  bins: [{ binId: 'bin-1', binKey: 'main-bin', locationKey: 'wh-1', name: 'Main Bin' }],
  canManage: true,
  isLoading: false,
  reservationKey: '',
  selectedReservationId: 'rsv-1',
  selectedReservationPartId: '',
  selectedReservationBinId: '',
  reservationQuantity: '',
  reservationNotes: '',
  releaseReason: '',
  statusFilter: 'active',
  onReservationKeyChange: vi.fn(),
  onSelectedReservationIdChange: vi.fn(),
  onSelectedReservationPartIdChange: vi.fn(),
  onSelectedReservationBinIdChange: vi.fn(),
  onReservationQuantityChange: vi.fn(),
  onReservationNotesChange: vi.fn(),
  onReleaseReasonChange: vi.fn(),
  onStatusFilterChange: vi.fn(),
  onCreateReservation: vi.fn(),
  onReleaseReservation: vi.fn(),
  onFulfillReservation: vi.fn(),
  isCreating: false,
  isReleasing: false,
  isFulfilling: false,
}

describe('StockReservationsPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders active reservation with fulfill action', () => {
    render(<StockReservationsPanel {...baseProps} />)
    expect(screen.getByText('RSV-WO-001')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Fulfill \(issue stock\)/i })).toBeInTheDocument()
  })

  it('shows empty state when no reservations', () => {
    render(<StockReservationsPanel {...baseProps} reservations={[]} selectedReservationId="" />)
    expect(screen.getByText('No stock reservations yet.')).toBeInTheDocument()
  })

  it('uses searchable pickers for new reservation part and bin selection', () => {
    const onSelectedReservationPartIdChange = vi.fn()
    const onSelectedReservationBinIdChange = vi.fn()

    render(
      <StockReservationsPanel
        {...baseProps}
        selectedReservationPartId=""
        selectedReservationBinId=""
        onSelectedReservationPartIdChange={onSelectedReservationPartIdChange}
        onSelectedReservationBinIdChange={onSelectedReservationBinIdChange}
      />,
    )

    expect(screen.getByTestId('stock-reservation-part-picker-options')).toHaveTextContent(
      'Oil filter (filter-001)',
    )
    expect(screen.getByTestId('stock-reservation-bin-picker-options')).toHaveTextContent(
      'wh-1/main-bin — Main Bin',
    )

    fireEvent.change(screen.getByTestId('stock-reservation-part-picker'), {
      target: { value: 'part-1' },
    })
    fireEvent.change(screen.getByTestId('stock-reservation-bin-picker'), {
      target: { value: 'bin-1' },
    })

    expect(onSelectedReservationPartIdChange).toHaveBeenCalledWith('part-1')
    expect(onSelectedReservationBinIdChange).toHaveBeenCalledWith('bin-1')
  })
})
