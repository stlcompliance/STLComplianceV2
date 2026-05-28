import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { SupplierIncidentsPanel } from './SupplierIncidentsPanel'

vi.mock('../api/client', () => ({
  listSupplierIncidents: vi.fn().mockResolvedValue([]),
  listPartySupplierIncidents: vi.fn().mockResolvedValue([]),
  createSupplierIncident: vi.fn(),
  startSupplierIncidentInvestigation: vi.fn(),
  resolveSupplierIncident: vi.fn(),
  closeSupplierIncident: vi.fn(),
  applySupplierIncidentProcurementRestriction: vi.fn(),
}))

describe('SupplierIncidentsPanel', () => {
  it('renders when user can manage', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <SupplierIncidentsPanel
          accessToken="token"
          canManage={true}
          incidentParties={[
            {
              partyId: 'party-1',
              partyKey: 'S-1',
              partyType: 'supplier',
              displayName: 'Test Supplier',
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
    expect(await screen.findByTestId('supplier-incidents-panel')).toBeInTheDocument()
    expect(screen.getByText('Supplier incidents')).toBeInTheDocument()
  })
})
