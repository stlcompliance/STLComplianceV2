import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { WarrantyClaimsPanel } from './WarrantyClaimsPanel'

vi.mock('../api/client', () => ({
  listWarrantyClaims: vi.fn().mockResolvedValue([
    {
      warrantyClaimId: '11111111-1111-1111-1111-111111111111',
      claimKey: 'WC-001',
      status: 'submitted',
      claimType: 'defective',
      vendorPartyId: '22222222-2222-2222-2222-222222222222',
      vendorPartyKey: 'V-1',
      vendorDisplayName: 'Acme Vendor',
      partId: '33333333-3333-3333-3333-333333333333',
      partKey: 'PART-1',
      partDisplayName: 'Widget',
      purchaseOrderId: null,
      purchaseOrderKey: null,
      purchaseOrderLineId: null,
      receivingReceiptId: null,
      receivingReceiptKey: null,
      receivingReceiptLineId: null,
      quantityClaimed: 2,
      problemDescription: 'Failed on install',
      vendorRmaNumber: '',
      vendorDisposition: '',
      vendorResponseNotes: '',
      closureNotes: '',
      denialReason: '',
      createdByUserId: '44444444-4444-4444-4444-444444444444',
      submittedByUserId: '44444444-4444-4444-4444-444444444444',
      submittedAt: '2026-05-28T12:00:00Z',
      vendorRespondedByUserId: null,
      vendorRespondedAt: null,
      closedByUserId: null,
      closedAt: null,
      deniedByUserId: null,
      deniedAt: null,
      cancellationReason: '',
      createdAt: '2026-05-28T12:00:00Z',
      updatedAt: '2026-05-28T12:00:00Z',
    },
  ]),
  createWarrantyClaim: vi.fn(),
  submitWarrantyClaim: vi.fn(),
  recordWarrantyClaimVendorResponse: vi.fn(),
  closeWarrantyClaim: vi.fn(),
  denyWarrantyClaim: vi.fn(),
  cancelWarrantyClaim: vi.fn(),
}))

describe('WarrantyClaimsPanel', () => {
  it('renders claims list when user can manage', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <WarrantyClaimsPanel
          accessToken="token"
          canManage
          vendors={[
            {
              partyId: '22222222-2222-2222-2222-222222222222',
              partyKey: 'V-1',
              displayName: 'Acme Vendor',
              partyType: 'vendor',
              legalName: '',
              taxIdentifier: null,
              approvalStatus: 'approved',
              notes: '',
              status: 'active',
              contacts: [],
              createdAt: '',
              updatedAt: '',
            },
          ]}
          parts={[
            {
              partId: '33333333-3333-3333-3333-333333333333',
              partKey: 'PART-1',
              displayName: 'Widget',
              description: '',
              categoryKey: '',
              unitOfMeasure: 'each',
              manufacturerName: '',
              manufacturerPartNumber: '',
              status: 'active',
              partCatalogId: null,
              catalogKey: null,
              catalogName: null,
              reorderPoint: null,
              reorderQuantity: null,
              createdAt: '',
              updatedAt: '',
            },
          ]}
          issuedPurchaseOrders={[]}
        />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('warranty-claims-panel')).toBeInTheDocument()
    expect(await screen.findByText('WC-001')).toBeInTheDocument()
  })
})
