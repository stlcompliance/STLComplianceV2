import { render, screen, within } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { AvailabilitySnapshotsPanel } from './AvailabilitySnapshotsPanel'

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
          partyId: 'vendor-1',
          partyKey: 'acme',
          partyDisplayName: 'Acme Supply',
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
      vendorPartyId: 'vendor-1',
      vendorPartyKey: 'acme',
      vendorDisplayName: 'Acme Supply',
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
  selectedVendorLinkId: 'link-1',
  quantityAvailable: '120',
  availabilityStatus: 'in_stock',
  snapshotNotes: '',
  currentOnlyFilter: true,
  onSnapshotKeyChange: () => {},
  onSelectedVendorLinkIdChange: () => {},
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
  })
})
