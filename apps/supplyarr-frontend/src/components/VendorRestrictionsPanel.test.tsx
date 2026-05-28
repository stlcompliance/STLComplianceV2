import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { VendorRestrictionsPanel } from './VendorRestrictionsPanel'

vi.mock('../api/client', () => ({
  listVendorRestrictions: vi.fn().mockResolvedValue([]),
  listPartyVendorRestrictions: vi.fn().mockResolvedValue([]),
  getPartyVendorRestrictionEnforcement: vi.fn().mockResolvedValue({
    externalPartyId: 'party-1',
    isBlocked: false,
    blockReason: null,
    activeScopes: [],
  }),
  createPartyVendorRestriction: vi.fn(),
  liftVendorRestriction: vi.fn(),
}))

describe('VendorRestrictionsPanel', () => {
  it('renders when user can manage', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <VendorRestrictionsPanel
          accessToken="token"
          canManage={true}
          restrictableParties={[
            {
              partyId: 'party-1',
              partyKey: 'V-1',
              partyType: 'vendor',
              displayName: 'Test Vendor',
              legalName: '',
              taxIdentifier: null,
              approvalStatus: 'approved',
              status: 'active',
              notes: '',
              contacts: [],
              createdAt: '',
              updatedAt: '',
            },
          ]}
        />
      </QueryClientProvider>,
    )
    expect(await screen.findByTestId('vendor-restrictions-panel')).toBeInTheDocument()
    expect(screen.getByText('Vendor restrictions')).toBeInTheDocument()
  })

  it('returns null when user cannot manage', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <VendorRestrictionsPanel accessToken="token" canManage={false} restrictableParties={[]} />
      </QueryClientProvider>,
    )
    expect(container).toBeEmptyDOMElement()
  })
})
