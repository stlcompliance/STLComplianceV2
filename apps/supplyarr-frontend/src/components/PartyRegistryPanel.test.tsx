import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { PartyRegistryPanel } from './PartyRegistryPanel'

describe('PartyRegistryPanel', () => {
  it('renders party registry list', () => {
    render(
      <PartyRegistryPanel
        title="Vendors"
        parties={[
          {
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
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ]}
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
      />,
    )

    expect(screen.getByText('Acme Parts Co.')).toBeInTheDocument()
    expect(screen.getByText('approved')).toBeInTheDocument()
  })
})
