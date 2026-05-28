import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { PricingLeadTimePanel } from './PricingLeadTimePanel'

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
          createdAt: '2026-05-27T00:00:00Z',
        },
      ],
      createdAt: '2026-05-27T00:00:00Z',
      updatedAt: '2026-05-27T00:00:00Z',
    },
  ],
  pricingSnapshots: [
    {
      pricingSnapshotId: 'price-1',
      snapshotKey: 'price-2026-q2',
      partVendorLinkId: 'link-1',
      partId: 'part-1',
      partKey: 'filter-01',
      partDisplayName: 'Oil Filter',
      vendorPartyId: 'vendor-1',
      vendorPartyKey: 'acme',
      vendorDisplayName: 'Acme Supply',
      vendorPartNumber: 'V-FLT-01',
      unitPrice: 12.5,
      currencyCode: 'USD',
      minimumOrderQuantity: 10,
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
  leadTimeSnapshots: [
    {
      leadTimeSnapshotId: 'lt-1',
      snapshotKey: 'lt-2026-q2',
      partVendorLinkId: 'link-1',
      partId: 'part-1',
      partKey: 'filter-01',
      partDisplayName: 'Oil Filter',
      vendorPartyId: 'vendor-1',
      vendorPartyKey: 'acme',
      vendorDisplayName: 'Acme Supply',
      vendorPartNumber: 'V-FLT-01',
      leadTimeDays: 14,
      effectiveFrom: '2026-05-01T00:00:00Z',
      effectiveTo: null,
      source: 'quote',
      notes: 'Vendor quote',
      isCurrent: true,
      createdByUserId: 'user-1',
      createdAt: '2026-05-27T00:00:00Z',
      updatedAt: '2026-05-27T00:00:00Z',
    },
  ],
  canManage: true,
  isLoading: false,
  pricingSnapshotKey: '',
  leadTimeSnapshotKey: '',
  selectedVendorLinkId: 'link-1',
  unitPrice: '12.50',
  currencyCode: 'USD',
  minimumOrderQuantity: '',
  leadTimeDays: '14',
  snapshotNotes: '',
  currentOnlyFilter: true,
  onPricingSnapshotKeyChange: () => {},
  onLeadTimeSnapshotKeyChange: () => {},
  onSelectedVendorLinkIdChange: () => {},
  onUnitPriceChange: () => {},
  onCurrencyCodeChange: () => {},
  onMinimumOrderQuantityChange: () => {},
  onLeadTimeDaysChange: () => {},
  onSnapshotNotesChange: () => {},
  onCurrentOnlyFilterChange: () => {},
  onCreatePricingSnapshot: () => {},
  onCreateLeadTimeSnapshot: () => {},
  isCreatingPricing: false,
  isCreatingLeadTime: false,
}

describe('PricingLeadTimePanel', () => {
  it('renders current pricing and lead-time snapshots', () => {
    render(<PricingLeadTimePanel {...baseProps} />)
    expect(screen.getByText('price-2026-q2')).toBeInTheDocument()
    expect(screen.getByText(/12\.5 USD/)).toBeInTheDocument()
    expect(screen.getByText('lt-2026-q2')).toBeInTheDocument()
    expect(screen.getByText(/14 days/)).toBeInTheDocument()
  })
})
