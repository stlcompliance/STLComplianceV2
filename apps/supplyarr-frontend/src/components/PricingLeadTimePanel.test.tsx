import { render, screen, within } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { PricingLeadTimePanel } from './PricingLeadTimePanel'

vi.mock('@stl/shared-ui', async () => {
  const actual = await vi.importActual<typeof import('@stl/shared-ui')>('@stl/shared-ui')
  return {
    ...actual,
    StaticSearchPicker: ({
      id,
      value,
      options,
      onChange,
    }: {
      id?: string
      value: string
      options: Array<{ value: string; label: string }>
      onChange: (value: string) => void
    }) => (
      <select
        data-testid={id ?? 'static-search-picker'}
        value={value}
        onChange={(event) => onChange(event.target.value)}
      >
        <option value="">Select link…</option>
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    ),
  }
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
        {
          linkId: 'link-2',
          partyId: 'vendor-2',
          partyKey: 'beta',
          partyDisplayName: 'Beta Parts',
          vendorPartNumber: 'V-FLT-02',
          isPreferred: false,
          catalogUnitPrice: 11.5,
          catalogCurrencyCode: 'USD',
          catalogMinimumOrderQuantity: 5,
          catalogLeadTimeDays: 10,
          catalogQuantityAvailable: null,
          catalogAvailabilityStatus: null,
          createdAt: '2026-05-27T00:00:00Z',
        },
        {
          linkId: 'link-3',
          partyId: 'vendor-3',
          partyKey: 'gamma',
          partyDisplayName: 'Gamma Industrial',
          vendorPartNumber: 'V-FLT-03',
          isPreferred: false,
          catalogUnitPrice: 9.0,
          catalogCurrencyCode: 'USD',
          catalogMinimumOrderQuantity: 1,
          catalogLeadTimeDays: 7,
          catalogQuantityAvailable: null,
          catalogAvailabilityStatus: null,
          createdAt: '2026-05-27T00:00:00Z',
        },
      ],
      createdAt: '2026-05-27T00:00:00Z',
      updatedAt: '2026-05-27T00:00:00Z',
    },
  ],
  vendors: [
    {
      partyId: 'vendor-1',
      partyKey: 'acme',
      partyType: 'vendor',
      displayName: 'Acme Supply',
      legalName: 'Acme Supply LLC',
      taxIdentifier: null,
      approvalStatus: 'approved',
      status: 'active',
      notes: '',
      contacts: [],
      createdAt: '2026-05-27T00:00:00Z',
      updatedAt: '2026-05-27T00:00:00Z',
    },
    {
      partyId: 'vendor-2',
      partyKey: 'beta',
      partyType: 'vendor',
      displayName: 'Beta Parts',
      legalName: 'Beta Parts Inc.',
      taxIdentifier: null,
      approvalStatus: 'approved',
      status: 'active',
      notes: '',
      contacts: [],
      createdAt: '2026-05-27T00:00:00Z',
      updatedAt: '2026-05-27T00:00:00Z',
    },
    {
      partyId: 'vendor-3',
      partyKey: 'gamma',
      partyType: 'vendor',
      displayName: 'Gamma Industrial',
      legalName: 'Gamma Industrial Co.',
      taxIdentifier: null,
      approvalStatus: 'restricted',
      status: 'inactive',
      notes: '',
      contacts: [],
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
    {
      leadTimeSnapshotId: 'lt-2',
      snapshotKey: 'lt-2026-q2-beta',
      partVendorLinkId: 'link-2',
      partId: 'part-1',
      partKey: 'filter-01',
      partDisplayName: 'Oil Filter',
      vendorPartyId: 'vendor-2',
      vendorPartyKey: 'beta',
      vendorDisplayName: 'Beta Parts',
      vendorPartNumber: 'V-FLT-02',
      leadTimeDays: 10,
      effectiveFrom: '2026-05-01T00:00:00Z',
      effectiveTo: null,
      source: 'manual',
      notes: '',
      isCurrent: true,
      createdByUserId: 'user-1',
      createdAt: '2026-05-27T00:00:00Z',
      updatedAt: '2026-05-27T00:00:00Z',
    },
    {
      leadTimeSnapshotId: 'lt-3',
      snapshotKey: 'lt-2026-q2-gamma',
      partVendorLinkId: 'link-3',
      partId: 'part-1',
      partKey: 'filter-01',
      partDisplayName: 'Oil Filter',
      vendorPartyId: 'vendor-3',
      vendorPartyKey: 'gamma',
      vendorDisplayName: 'Gamma Industrial',
      vendorPartNumber: 'V-FLT-03',
      leadTimeDays: 7,
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
    expect(screen.getByTestId('pricing-lead-time-panel')).toBeInTheDocument()
    expect(screen.getByTestId('pricing-lead-time-vendor-link')).toBeInTheDocument()
    expect(screen.getByRole('option', { name: 'filter-01 · acme · V-FLT-01' })).toBeInTheDocument()
    expect(screen.getByText('price-2026-q2')).toBeInTheDocument()
    expect(screen.getByText(/12\.5 USD/)).toBeInTheDocument()
    expect(screen.getByText('lt-2026-q2')).toBeInTheDocument()
    expect(screen.getAllByText(/14 days/).length).toBeGreaterThan(0)
    expect(screen.getByText(/Source recommendations/i)).toBeInTheDocument()
    expect(screen.getByText(/Best overall/i)).toBeInTheDocument()
    expect(screen.getByText(/Lowest cost/i)).toBeInTheDocument()
    expect(screen.getByText(/Fastest delivery/i)).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Preferred vendor' })).toBeInTheDocument()
    expect(screen.getByText(/Compliance safest/i)).toBeInTheDocument()
    expect(screen.getByText(/Emergency option/i)).toBeInTheDocument()
    expect(screen.getByText(/Needs approval/i)).toBeInTheDocument()
    expect(screen.getByText(/Not recommended reason/i)).toBeInTheDocument()
    expect(screen.getAllByText(/Acme Supply/i).length).toBeGreaterThan(0)
    expect(screen.getAllByText(/Beta Parts/i).length).toBeGreaterThan(0)
    expect(screen.getAllByText(/Gamma Industrial/i).length).toBeGreaterThan(0)
    const panel = screen.getByTestId('pricing-lead-time-panel')
    expect(within(panel).getByRole('button', { name: 'Record pricing' })).toBeInTheDocument()
    expect(within(panel).getByRole('button', { name: 'Record lead time' })).toBeInTheDocument()
  })
})
