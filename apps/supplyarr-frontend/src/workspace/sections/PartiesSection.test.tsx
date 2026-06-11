import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'

import { PartiesSection } from './PartiesSection'

vi.mock('../../components/PartyRegistryPanel', () => ({
  PartyRegistryPanel: ({ title }: { title: string }) => (
    <div data-testid={`party-registry-panel-${title.toLowerCase()}`} />
  ),
}))

const supplier = {
  partyId: 'supplier-1',
  partyKey: 'sup-2048',
  partyType: 'supplier',
  displayName: 'Midwest Fleet Parts & Service',
  legalName: 'Midwest Fleet Parts & Service LLC',
  taxIdentifier: '12-3456789',
  approvalStatus: 'approved',
  status: 'active',
  notes: '',
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
  it('renders party registry workspace with vendor, supplier, and dealer panels', () => {
    renderPartiesSection()
    expect(screen.getByTestId('supplyarr-party-registry-workspace')).toBeInTheDocument()
    expect(screen.getByTestId('party-registry-panel-vendors')).toBeInTheDocument()
    expect(screen.getByTestId('party-registry-panel-suppliers')).toBeInTheDocument()
    expect(screen.getByTestId('party-registry-panel-dealers')).toBeInTheDocument()
  })

  it('renders the replacement supplier profile detail view', () => {
    renderPartiesSection('/parties/details', {
      ...baseState,
      suppliersQuery: { data: [supplier], isLoading: false },
      contractsQuery: { data: [contract], isLoading: false },
    } as never)

    expect(screen.getByTestId('supplyarr-party-profile')).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Midwest Fleet Parts & Service' })).toBeInTheDocument()
    expect(screen.getByText('Supplier snapshot')).toBeInTheDocument()
    expect(screen.getByText('Contracts & purchasing terms')).toBeInTheDocument()
    expect(screen.getByText('SC-2048')).toBeInTheDocument()
    expect(screen.getAllByText('Net 30')).toHaveLength(2)
    expect(screen.getByText('Supplier decision')).toBeInTheDocument()
    expect(screen.getByRole('tab', { name: 'Overview' })).toHaveAttribute('aria-selected', 'true')
  })
})
