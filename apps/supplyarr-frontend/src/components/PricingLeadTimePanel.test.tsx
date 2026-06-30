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
        {
          linkId: 'link-2',
          supplierId: 'vendor-2',
          supplierKey: 'beta-west',
          supplierDisplayName: 'West Service Desk',
          parentSupplierId: 'supplier-2',
          parentSupplierKey: 'beta',
          parentSupplierDisplayName: 'Beta Parts',
          supplierUnitKind: 'sub_unit',
          supplierServiceTypes: ['parts'],
          supplierAddressLine1: '45 Service Ave',
          supplierLocality: 'Oklahoma City',
          supplierRegionCode: 'OK',
          supplierPostalCode: '73102',
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
          supplierId: 'vendor-3',
          supplierKey: 'gamma-fast',
          supplierDisplayName: 'Express Counter',
          parentSupplierId: 'supplier-3',
          parentSupplierKey: 'gamma',
          parentSupplierDisplayName: 'Gamma Industrial',
          supplierUnitKind: 'sub_unit',
          supplierServiceTypes: ['parts'],
          supplierAddressLine1: '99 Rapid Ship Dr',
          supplierLocality: 'Amarillo',
          supplierRegionCode: 'TX',
          supplierPostalCode: '79101',
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
  suppliers: [
    {
      supplierId: 'vendor-1',
      supplierKey: 'acme',
      supplierType: 'supplier',
      parentSupplierId: 'supplier-1',
      parentSupplierDisplayName: 'Acme Supply',
      unitKind: 'sub_unit',
      displayName: 'North Yard Counter',
      legalName: 'Acme Supply LLC',
      taxIdentifier: null,
      approvalStatus: 'approved',
      status: 'active',
      notes: '',
      serviceTypes: ['parts', 'maintenance'],
      addressLine1: '1200 North Yard Rd',
      locality: 'Tulsa',
      regionCode: 'OK',
      postalCode: '74101',
      contacts: [],
      createdAt: '2026-05-27T00:00:00Z',
      updatedAt: '2026-05-27T00:00:00Z',
    },
    {
      supplierId: 'vendor-2',
      supplierKey: 'beta',
      supplierType: 'supplier',
      parentSupplierId: 'supplier-2',
      parentSupplierDisplayName: 'Beta Parts',
      unitKind: 'sub_unit',
      displayName: 'West Service Desk',
      legalName: 'Beta Parts Inc.',
      taxIdentifier: null,
      approvalStatus: 'approved',
      status: 'active',
      notes: '',
      serviceTypes: ['parts'],
      addressLine1: '45 Service Ave',
      locality: 'Oklahoma City',
      regionCode: 'OK',
      postalCode: '73102',
      contacts: [],
      createdAt: '2026-05-27T00:00:00Z',
      updatedAt: '2026-05-27T00:00:00Z',
    },
    {
      supplierId: 'vendor-3',
      supplierKey: 'gamma',
      supplierType: 'supplier',
      parentSupplierId: 'supplier-3',
      parentSupplierDisplayName: 'Gamma Industrial',
      unitKind: 'sub_unit',
      displayName: 'Express Counter',
      legalName: 'Gamma Industrial Co.',
      taxIdentifier: null,
      approvalStatus: 'restricted',
      status: 'inactive',
      notes: '',
      serviceTypes: ['parts'],
      addressLine1: '99 Rapid Ship Dr',
      locality: 'Amarillo',
      regionCode: 'TX',
      postalCode: '79101',
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
      supplierId: 'vendor-1',
      supplierKey: 'acme-north',
      supplierDisplayName: 'North Yard Counter',
      parentSupplierId: 'supplier-1',
      parentSupplierDisplayName: 'Acme Supply',
      supplierUnitKind: 'sub_unit',
      supplierServiceTypes: ['parts', 'maintenance'],
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
      supplierId: 'vendor-1',
      supplierKey: 'acme-north',
      supplierDisplayName: 'North Yard Counter',
      parentSupplierId: 'supplier-1',
      parentSupplierDisplayName: 'Acme Supply',
      supplierUnitKind: 'sub_unit',
      supplierServiceTypes: ['parts', 'maintenance'],
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
      supplierId: 'vendor-2',
      supplierKey: 'beta-west',
      supplierDisplayName: 'West Service Desk',
      parentSupplierId: 'supplier-2',
      parentSupplierDisplayName: 'Beta Parts',
      supplierUnitKind: 'sub_unit',
      supplierServiceTypes: ['parts'],
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
      supplierId: 'vendor-3',
      supplierKey: 'gamma-fast',
      supplierDisplayName: 'Express Counter',
      parentSupplierId: 'supplier-3',
      parentSupplierDisplayName: 'Gamma Industrial',
      supplierUnitKind: 'sub_unit',
      supplierServiceTypes: ['parts'],
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
  selectedSourceLinkId: 'link-1',
  unitPrice: '12.50',
  currencyCode: 'USD',
  minimumOrderQuantity: '',
  leadTimeDays: '14',
  snapshotNotes: '',
  currentOnlyFilter: true,
  onPricingSnapshotKeyChange: () => {},
  onLeadTimeSnapshotKeyChange: () => {},
  onSelectedSourceLinkIdChange: () => {},
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
    expect(screen.getByTestId('pricing-lead-time-supplier-source-link')).toBeInTheDocument()
    expect(
      screen.getByRole('option', { name: 'Sub-unit · filter-01 · Acme Supply · North Yard Counter (acme-north) · V-FLT-01' }),
    ).toBeInTheDocument()
    expect(screen.getByText('price-2026-q2')).toBeInTheDocument()
    expect(screen.getByText(/Acme Supply · North Yard Counter \(acme-north\) · 12\.5 USD/)).toBeInTheDocument()
    expect(screen.getByText('lt-2026-q2')).toBeInTheDocument()
    expect(screen.getAllByText(/14 days/).length).toBeGreaterThan(0)
    expect(screen.getByText(/Source recommendations/i)).toBeInTheDocument()
    expect(screen.getByText(/Best overall/i)).toBeInTheDocument()
    expect(screen.getByText(/Lowest cost/i)).toBeInTheDocument()
    expect(screen.getByText(/Fastest delivery/i)).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Preferred source' })).toBeInTheDocument()
    expect(screen.getByText(/Compliance safest/i)).toBeInTheDocument()
    expect(screen.getByText(/Emergency option/i)).toBeInTheDocument()
    expect(screen.getByText(/Needs approval/i)).toBeInTheDocument()
    expect(screen.getByText(/Not recommended reason/i)).toBeInTheDocument()
    expect(screen.getAllByText(/Acme Supply · North Yard Counter/i).length).toBeGreaterThan(0)
    expect(screen.getAllByText(/Beta Parts · West Service Desk/i).length).toBeGreaterThan(0)
    expect(screen.getAllByText(/Gamma Industrial · Express Counter/i).length).toBeGreaterThan(0)
    expect(screen.getAllByText(/Tulsa, OK, 74101 · Parts, Maintenance/).length).toBeGreaterThan(0)
    const panel = screen.getByTestId('pricing-lead-time-panel')
    expect(within(panel).getByRole('button', { name: 'Record pricing' })).toBeInTheDocument()
    expect(within(panel).getByRole('button', { name: 'Record lead time' })).toBeInTheDocument()
  })
})
