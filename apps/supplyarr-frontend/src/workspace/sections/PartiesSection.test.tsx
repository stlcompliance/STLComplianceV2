import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { PartiesSection } from './PartiesSection'

vi.mock('../../components/PartyRegistryPanel', () => ({
  PartyRegistryPanel: ({ title }: { title: string }) => (
    <div data-testid={`party-registry-panel-${title.toLowerCase()}`} />
  ),
}))

vi.mock('../../components/SupplierOnboardingPanel', () => ({
  SupplierOnboardingPanel: () => null,
}))

vi.mock('../../components/VendorRestrictionsPanel', () => ({
  VendorRestrictionsPanel: () => null,
}))

vi.mock('../../components/SupplierIncidentsPanel', () => ({
  SupplierIncidentsPanel: () => null,
}))

const baseState = {
  accessToken: 'token',
  canManage: true,
  canApprovePr: true,
  vendors: [],
  vendorsQuery: { data: [], isLoading: false },
  suppliersQuery: { data: [], isLoading: false },
  dealersQuery: { data: [], isLoading: false },
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
} as never

describe('PartiesSection', () => {
  it('renders party registry workspace with vendor, supplier, and dealer panels', () => {
    render(<PartiesSection state={baseState} />)
    expect(screen.getByTestId('supplyarr-party-registry-workspace')).toBeInTheDocument()
    expect(screen.getByTestId('party-registry-panel-vendors')).toBeInTheDocument()
    expect(screen.getByTestId('party-registry-panel-suppliers')).toBeInTheDocument()
    expect(screen.getByTestId('party-registry-panel-dealers')).toBeInTheDocument()
  })
})
