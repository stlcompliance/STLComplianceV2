import { render, screen, within } from '@testing-library/react'
import { cleanup } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

vi.mock('@stl/shared-ui', async () => {
  const actual = await vi.importActual<typeof import('@stl/shared-ui')>('@stl/shared-ui')
  return {
    ...actual,
    StaticSearchPicker: ({
      placeholder,
      value,
      options,
      onChange,
    }: {
      placeholder?: string
      value: string
      options: Array<{ value: string; label: string }>
      onChange: (value: string) => void
    }) => (
      <select
        aria-label={placeholder ?? 'Static search picker'}
        value={value}
        onChange={(event) => onChange(event.target.value)}
      >
        <option value="">{placeholder ?? 'Select…'}</option>
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    ),
  }
})

import { AvailabilitySnapshotsPanel } from './AvailabilitySnapshotsPanel'

afterEach(() => {
  cleanup()
})

const baseProps = {
  parts: [
    {
      partId: 'part-1',
      partKey: 'filter-01',
      catalogId: null,
      catalogKey: null,
      displayName: 'Oil Filter',
      description: '',
      categoryKey: 'filters',
      unitOfMeasure: 'each',
      manufacturerName: '',
      manufacturerPartNumber: '',
      status: 'active',
      reorderPoint: null,
      reorderQuantity: null,
      manufacturerAliases: [],
      vendorLinks: [
        {
          linkId: 'link-1',
          supplierId: 'vendor-1',
          supplierKey: 'acme-north',
          supplierDisplayName: 'North Yard Counter',
          parentSupplierId: 'supplier-1',
          parentSupplierKey: 'acme',
          parentSupplierDisplayName: 'Acme Supply',
          supplierUnitKind: 'sub_unit',
          supplierServiceTypes: ['parts', 'maintenance'],
          supplierAddressLine1: '1200 North Yard Rd',
          supplierLocality: 'Tulsa',
          supplierRegionCode: 'OK',
          supplierPostalCode: '74101',
          vendorPartNumber: 'V-FLT-01',
          isPreferred: true,
          catalogUnitPrice: null,
          catalogCurrencyCode: null,
          catalogMinimumOrderQuantity: null,
          catalogLeadTimeDays: null,
          catalogQuantityAvailable: null,
          catalogAvailabilityStatus: null,
          createdAt: '2026-05-27T00:00:00Z',
        },
      ],
      createdAt: '2026-05-27T00:00:00Z',
      updatedAt: '2026-05-27T00:00:00Z',
    },
  ],
  availabilitySnapshots: [
    {
      availabilitySnapshotId: 'avail-1',
      snapshotKey: 'avail-2026-q2',
      partVendorLinkId: 'link-1',
      partId: 'part-1',
      partKey: 'filter-01',
      partDisplayName: 'Oil Filter',
      supplierId: 'vendor-1',
      supplierKey: 'acme-north',
      supplierDisplayName: 'North Yard Counter',
      parentSupplierId: 'supplier-1',
      parentSupplierDisplayName: 'Acme Supply',
      supplierUnitKind: 'sub_unit',
      supplierServiceTypes: ['parts', 'maintenance'],
      vendorPartNumber: 'V-FLT-01',
      quantityAvailable: 120,
      availabilityStatus: 'in_stock',
      effectiveFrom: '2026-05-01T00:00:00Z',
      effectiveTo: null,
      source: 'manual',
      notes: '',
      isCurrent: true,
      createdByUserId: 'user-1',
      createdAt: '2026-05-27T00:00:00Z',
      updatedAt: '2026-05-27T00:00:00Z',
    },
  ],
  canManage: true,
  isLoading: false,
  snapshotKey: '',
  selectedSourceLinkId: 'link-1',
  quantityAvailable: '120',
  availabilityStatus: 'in_stock',
  snapshotNotes: '',
  currentOnlyFilter: true,
  onSnapshotKeyChange: () => {},
  onSelectedSourceLinkIdChange: () => {},
  onQuantityAvailableChange: () => {},
  onAvailabilityStatusChange: () => {},
  onSnapshotNotesChange: () => {},
  onCurrentOnlyFilterChange: () => {},
  onCreateAvailabilitySnapshot: () => {},
  isCreating: false,
}

describe('AvailabilitySnapshotsPanel', () => {
  it('renders current availability snapshots', () => {
    render(<AvailabilitySnapshotsPanel {...baseProps} />)
    const panel = screen.getByTestId('availability-snapshots-panel')
    expect(panel).toBeInTheDocument()
    expect(within(panel).getByText('avail-2026-q2')).toBeInTheDocument()
    expect(within(panel).getByText(/qty 120/)).toBeInTheDocument()
    expect(within(panel).getByRole('button', { name: 'Record availability' })).toBeInTheDocument()
    expect(screen.getByLabelText('Search supplier source links…')).toBeInTheDocument()
    expect(within(panel).getByText(/filter-01 · Acme Supply · North Yard Counter \(acme-north\) · In stock · qty 120/i)).toBeInTheDocument()
    expect(within(panel).getAllByText(/Tulsa, OK, 74101 · Parts, Maintenance/i).length).toBeGreaterThan(0)
  })
})
