import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { SupplierIncidentsPanel } from './SupplierIncidentsPanel'

vi.mock('../api/client', () => ({
  listSupplierIncidents: vi.fn().mockResolvedValue([]),
  listPartySupplierIncidents: vi.fn().mockResolvedValue([
    {
      incidentId: 'inc-open',
      externalPartyId: 'party-1',
      partyKey: 'V-1',
      partyDisplayName: 'Acme Supply',
      partyType: 'vendor',
      incidentKey: 'SI-OPEN',
      title: 'Open incident',
      description: '',
      incidentType: 'quality',
      severity: 'high',
      status: 'open',
      purchaseRequestId: null,
      purchaseOrderId: null,
      receivingReceiptId: null,
      receivingExceptionId: null,
      vendorRestrictionId: null,
      reportedByUserId: 'u1',
      assignedToUserId: null,
      resolutionNotes: '',
      resolvedByUserId: null,
      resolvedAt: null,
      closedByUserId: null,
      closedAt: null,
      cancellationReason: '',
      cancelledByUserId: null,
      cancelledAt: null,
      reopenedByUserId: null,
      reopenedAt: null,
      lastReopenReason: '',
      reopenCount: 0,
      createdAt: '',
      updatedAt: '',
    },
    {
      incidentId: 'inc-cancelled',
      externalPartyId: 'party-1',
      partyKey: 'V-1',
      partyDisplayName: 'Acme Supply',
      partyType: 'vendor',
      incidentKey: 'SI-CANCEL',
      title: 'Cancelled incident',
      description: '',
      incidentType: 'delivery',
      severity: 'medium',
      status: 'cancelled',
      purchaseRequestId: null,
      purchaseOrderId: null,
      receivingReceiptId: null,
      receivingExceptionId: null,
      vendorRestrictionId: null,
      reportedByUserId: 'u1',
      assignedToUserId: null,
      resolutionNotes: '',
      resolvedByUserId: null,
      resolvedAt: null,
      closedByUserId: null,
      closedAt: null,
      cancellationReason: 'Mistake',
      cancelledByUserId: 'u1',
      cancelledAt: '',
      reopenedByUserId: null,
      reopenedAt: null,
      lastReopenReason: '',
      reopenCount: 0,
      createdAt: '',
      updatedAt: '',
    },
  ]),
  createSupplierIncident: vi.fn(),
  startSupplierIncidentInvestigation: vi.fn(),
  resolveSupplierIncident: vi.fn(),
  closeSupplierIncident: vi.fn(),
  cancelSupplierIncident: vi.fn(),
  reopenSupplierIncident: vi.fn(),
  applySupplierIncidentProcurementRestriction: vi.fn(),
}))

describe('SupplierIncidentsPanel', () => {
  it('renders cancel and reopen workflow controls', async () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <SupplierIncidentsPanel
          accessToken="token"
          canManage
          incidentParties={[
            {
              partyId: 'party-1',
              partyKey: 'V-1',
              displayName: 'Acme Supply',
              partyType: 'vendor',
              status: 'active',
              approvalStatus: 'approved',
              legalName: 'Acme Supply LLC',
              taxIdentifier: null,
              notes: '',
              contacts: [],
              createdAt: '',
              updatedAt: '',
            },
          ]}
        />
      </QueryClientProvider>,
    )

    fireEvent.change(screen.getByLabelText(/Vendor or supplier party/i), {
      target: { value: 'party-1' },
    })

    expect(await screen.findByTestId('supplier-incidents-panel')).toBeInTheDocument()
    expect(await screen.findByTestId('supplier-incident-cancel-reason')).toBeInTheDocument()
    expect(screen.getByTestId('supplier-incident-reopen-reason')).toBeInTheDocument()
    expect(await screen.findByTestId('supplier-incident-cancel-inc-open')).toBeInTheDocument()
    expect(await screen.findByTestId('supplier-incident-reopen-inc-cancelled')).toBeInTheDocument()
    expect(screen.getByTestId('supplier-incident-status-inc-cancelled')).toHaveTextContent('cancelled')
  })
})
