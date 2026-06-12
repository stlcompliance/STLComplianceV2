import { fireEvent, render, screen, within } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { PartyRegistryPanel } from './PartyRegistryPanel'

const sampleParty = {
  partyId: '11111111-1111-1111-1111-111111111111',
  partyKey: 'acme-parts',
  partyType: 'vendor',
  displayName: 'Acme Parts Co.',
  legalName: 'Acme Parts Company LLC',
  taxIdentifier: null,
  approvalStatus: 'approved',
  status: 'active',
  notes: '',
  contacts: [],
  createdAt: '2026-05-27T00:00:00Z',
  updatedAt: '2026-05-28T00:00:00Z',
}

const lifecycleHandlers = {
  onUpdateParty: vi.fn(),
  onUpdateApprovalStatus: vi.fn(),
  onUpdateStatus: vi.fn(),
  onAddContact: vi.fn(),
  isUpdating: false,
  isUpdatingApproval: false,
  isUpdatingStatus: false,
  isAddingContact: false,
}

const partyRegistryMetadataOptions = {
  approvalStatusOptions: [
    { value: 'pending', label: 'Pending' },
    { value: 'approved', label: 'Approved' },
    { value: 'restricted', label: 'Restricted' },
    { value: 'inactive', label: 'Inactive (approval)' },
  ],
  statusOptions: [
    { value: 'active', label: 'Active' },
    { value: 'inactive', label: 'Inactive' },
  ],
}

describe('PartyRegistryPanel', () => {
  it('renders party registry list', () => {
    render(
      <PartyRegistryPanel
        mode="drawer"
        title="Vendors"
        partyType="vendors"
        parties={[sampleParty]}
        {...partyRegistryMetadataOptions}
        canManage={false}
        isLoading={false}
        partyKey=""
        displayName=""
        legalName=""
        taxIdentifier=""
        notes=""
        onPartyKeyChange={() => {}}
        onDisplayNameChange={() => {}}
        onLegalNameChange={() => {}}
        onTaxIdentifierChange={() => {}}
        onNotesChange={() => {}}
        onCreate={() => {}}
        isCreating={false}
        {...lifecycleHandlers}
      />,
    )

    expect(screen.getByText('Acme Parts Co.')).toBeInTheDocument()
    expect(screen.getByText('approved')).toBeInTheDocument()
  })

  it('shows lifecycle detail and edit controls when a party is selected', () => {
    const { container } = render(
      <PartyRegistryPanel
        mode="details"
        title="Vendors"
        partyType="vendors"
        parties={[sampleParty]}
        {...partyRegistryMetadataOptions}
        canManage
        isLoading={false}
        partyKey=""
        displayName=""
        legalName=""
        taxIdentifier=""
        notes=""
        onPartyKeyChange={() => {}}
        onDisplayNameChange={() => {}}
        onLegalNameChange={() => {}}
        onTaxIdentifierChange={() => {}}
        onNotesChange={() => {}}
        onCreate={() => {}}
        isCreating={false}
        {...lifecycleHandlers}
      />,
    )

    const view = within(container)
    fireEvent.click(view.getByTestId(`party-registry-row-${sampleParty.partyId}`))

    expect(view.getByTestId('party-registry-detail')).toBeInTheDocument()
    expect(view.getByTestId('party-registry-lifecycle-timeline')).toBeInTheDocument()
    expect(view.getByTestId('party-registry-edit-form')).toBeInTheDocument()
    expect(view.getByTestId('party-registry-contact-form')).toBeInTheDocument()
    expect(view.queryByTestId('party-registry-create-form')).not.toBeInTheDocument()
  })
})
