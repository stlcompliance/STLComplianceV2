import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, within } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'

import { PartiesSection } from './PartiesSection'

const supplier = {
  partyId: 'supplier-1',
  partyKey: 'sup-2048',
  partyType: 'supplier',
  parentPartyId: null,
  parentPartyDisplayName: null,
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
  partyId: 'supplier-2',
  partyKey: 'sup-2048-kc',
  parentPartyId: 'supplier-1',
  parentPartyDisplayName: 'Midwest Fleet Parts & Service',
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
  canReadParties: false,
  canReadAuditHistory: false,
  canReadSupplyReadiness: false,
  vendors: [],
  vendorsQuery: { data: [], isLoading: false },
  suppliersQuery: { data: [], isLoading: false },
  supplierDirectory: [],
  dealersQuery: { data: [], isLoading: false },
  contractsQuery: { data: [], isLoading: false },
  partsQuery: { data: [], isLoading: false },
  purchaseOrdersQuery: { data: [], isLoading: false },
  purchaseRequestsQuery: { data: [], isLoading: false },
  vendorKey: '',
  vendorName: '',
  vendorLegalName: '',
  vendorTaxId: '',
  vendorNotes: '',
  supplierKey: '',
  supplierName: '',
  supplierLegalName: '',
  supplierTaxId: '',
  supplierNotes: '',
  supplierParentPartyId: '',
  supplierUnitKind: 'identity',
  supplierServiceTypes: 'products,parts',
  supplierAddressLine1: '',
  supplierLocality: '',
  supplierRegionCode: '',
  supplierPostalCode: '',
  supplierCountryCode: 'US',
  dealerKey: '',
  dealerName: '',
  dealerLegalName: '',
  dealerTaxId: '',
  dealerNotes: '',
  setVendorKey: () => {},
  setVendorName: () => {},
  setVendorLegalName: () => {},
  setVendorTaxId: () => {},
  setVendorNotes: () => {},
  setSupplierKey: () => {},
  setSupplierName: () => {},
  setSupplierLegalName: () => {},
  setSupplierTaxId: () => {},
  setSupplierNotes: () => {},
  setSupplierParentPartyId: () => {},
  setSupplierUnitKind: () => {},
  setSupplierServiceTypes: () => {},
  setSupplierAddressLine1: () => {},
  setSupplierLocality: () => {},
  setSupplierRegionCode: () => {},
  setSupplierPostalCode: () => {},
  setSupplierCountryCode: () => {},
  setDealerKey: () => {},
  setDealerName: () => {},
  setDealerLegalName: () => {},
  setDealerTaxId: () => {},
  setDealerNotes: () => {},
  createVendorMutation: { mutate: () => {}, isPending: false },
  createSupplierMutation: { mutate: () => {}, isPending: false },
  createDealerMutation: { mutate: () => {}, isPending: false },
  updatePartyMutation: { mutate: () => {}, isPending: false },
  updatePartyApprovalMutation: { mutate: () => {}, isPending: false },
  updatePartyStatusMutation: { mutate: () => {}, isPending: false },
  addPartyContactMutation: { mutate: () => {}, isPending: false },
}

function renderPartiesSection(path = '/parties/drawer', state: unknown = baseState) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[path]}>
        <PartiesSection state={state as never} />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('PartiesSection', () => {
  it('renders the unified supplier directory workspace', () => {
    renderPartiesSection()
    expect(screen.getByTestId('supplyarr-supplier-directory')).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Supplier directory' })).toBeInTheDocument()
    expect(screen.getByText('0 supplier identities · 0 sub-units')).toBeInTheDocument()
  })

  it('renders the replacement supplier profile detail view', () => {
    const view = renderPartiesSection('/suppliers/details', {
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
    expect(page.getByText('Midwest Fleet Parts & Service - Kansas City')).toBeInTheDocument()
    expect(page.getAllByText(/Products, Parts/i).length).toBeGreaterThan(0)
  })
})
