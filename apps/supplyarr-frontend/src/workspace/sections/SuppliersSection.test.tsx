import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, within } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'

import { SuppliersSection } from './SuppliersSection'

const supplier = {
  supplierId: 'supplier-1',
  supplierKey: 'sup-2048',
  supplierType: 'supplier',
  parentSupplierId: null,
  parentSupplierDisplayName: null,
  unitKind: 'identity',
  displayName: 'Midwest Fleet Parts & Service',
  legalName: 'Midwest Fleet Parts & Service LLC',
  taxIdentifier: '12-3456789',
  approvalStatus: 'approved',
  status: 'active',
  notes: '',
  serviceTypes: ['products', 'parts'],
  addressLine1: '1200 Westport Rd',
  addressLine2: '',
  locality: 'Kansas City',
  regionCode: 'MO',
  postalCode: '64111',
  countryCode: 'US',
  childUnitCount: 1,
  contacts: [
    {
      contactId: 'contact-1',
      contactName: 'Sarah Jenkins',
      email: 'sarah@midwestfleet.example',
      phone: '(555) 774-2190',
      roleLabel: 'Account Manager',
      isPrimary: true,
      createdAt: '2026-01-03T00:00:00Z',
    },
  ],
  createdAt: '2026-01-03T00:00:00Z',
  updatedAt: '2026-06-01T00:00:00Z',
}

const subUnit = {
  ...supplier,
  supplierId: 'supplier-2',
  supplierKey: 'sup-2048-kc',
  parentSupplierId: 'supplier-1',
  parentSupplierDisplayName: 'Midwest Fleet Parts & Service',
  unitKind: 'sub_unit',
  displayName: 'Midwest Fleet Parts & Service - Kansas City',
  childUnitCount: 0,
  serviceTypes: ['parts', 'maintenance'],
}

const contract = {
  contractId: 'contract-1',
  contractKey: 'SC-2048',
  contractType: 'master_supply_agreement',
  title: 'Supply Agreement 2026',
  vendorPartyId: 'supplier-1',
  vendorPartyKey: 'sup-2048',
  vendorDisplayName: 'Midwest Fleet Parts & Service',
  effectiveAt: '2026-01-15T00:00:00Z',
  expiresAt: '2026-12-31T00:00:00Z',
  renewalAt: '2026-11-01T00:00:00Z',
  paymentTerms: 'Net 30',
  freightTerms: 'FOB destination',
  warrantyTerms: '12 months from receipt',
  minimumSpend: 25000,
  serviceLevelAgreement: '95% on-time shipment rate',
  approvalStatus: 'approved',
  status: 'active',
  notes: 'Priority partner contract',
  createdByUserId: 'user-1',
  createdAt: '2026-01-10T00:00:00Z',
  updatedAt: '2026-06-02T00:00:00Z',
}

const baseState = {
  accessToken: '',
  canManage: true,
  canApprovePr: true,
  canReadSuppliers: false,
  canReadAuditHistory: false,
  canReadSupplyReadiness: false,
  suppliersQuery: { data: [], isLoading: false },
  supplierDirectory: [],
  contractsQuery: { data: [], isLoading: false },
  partsQuery: { data: [], isLoading: false },
  purchaseOrdersQuery: { data: [], isLoading: false },
  purchaseRequestsQuery: { data: [], isLoading: false },
  supplierKey: '',
  supplierName: '',
  supplierLegalName: '',
  supplierTaxId: '',
  supplierNotes: '',
  supplierParentUnitId: '',
  supplierUnitKind: 'identity',
  supplierServiceTypes: 'products,parts',
  supplierAddressLine1: '',
  supplierLocality: '',
  supplierRegionCode: '',
  supplierPostalCode: '',
  supplierCountryCode: 'US',
  setSupplierKey: () => {},
  setSupplierName: () => {},
  setSupplierLegalName: () => {},
  setSupplierTaxId: () => {},
  setSupplierNotes: () => {},
  setSupplierParentUnitId: () => {},
  setSupplierUnitKind: () => {},
  setSupplierServiceTypes: () => {},
  setSupplierAddressLine1: () => {},
  setSupplierLocality: () => {},
  setSupplierRegionCode: () => {},
  setSupplierPostalCode: () => {},
  setSupplierCountryCode: () => {},
  createSupplierMutation: { mutate: () => {}, isPending: false },
  updateSupplierMutation: { mutate: () => {}, isPending: false },
  updateSupplierApprovalMutation: { mutate: () => {}, isPending: false },
  updateSupplierStatusMutation: { mutate: () => {}, isPending: false },
  addSupplierContactMutation: { mutate: () => {}, isPending: false },
}

function renderSuppliersSection(path = '/suppliers/drawer', state: unknown = baseState) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[path]}>
        <SuppliersSection state={state as never} />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('SuppliersSection', () => {
  it('renders the unified supplier directory workspace', () => {
    renderSuppliersSection()
    expect(screen.getByTestId('supplyarr-supplier-directory')).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Supplier directory' })).toBeInTheDocument()
    expect(screen.getByText('0 supplier identities · 0 sub-units')).toBeInTheDocument()
  })

  it('renders the replacement supplier profile detail view', () => {
    const view = renderSuppliersSection('/suppliers/details', {
      ...baseState,
      suppliersQuery: { data: [supplier, subUnit], isLoading: false },
      supplierDirectory: [supplier, subUnit],
      contractsQuery: { data: [contract], isLoading: false },
    } as never)

    const page = within(view.container)
    expect(page.getAllByTestId('supplyarr-supplier-profile').at(-1)).toBeInTheDocument()
    expect(page.getByRole('heading', { name: 'Midwest Fleet Parts & Service' })).toBeInTheDocument()
    expect(page.getAllByText('Supplier snapshot').at(-1)).toBeInTheDocument()
    expect(page.getByText('Contracts & terms')).toBeInTheDocument()
    expect(page.getByText('SC-2048')).toBeInTheDocument()
    expect(page.getAllByText('Sub-units').at(-1)).toBeInTheDocument()
    expect(page.getByText('Sourcing readiness')).toBeInTheDocument()
    expect(page.getByText('Stock and parts sourcing')).toBeInTheDocument()
    expect(page.getByText('Use this identity when sourcing can route across multiple supplier locations.')).toBeInTheDocument()
    expect(page.getByText('Midwest Fleet Parts & Service - Kansas City')).toBeInTheDocument()
    expect(page.getAllByText(/Products, Parts/i).length).toBeGreaterThan(0)
  })
})
