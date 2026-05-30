import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
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
  function renderPanel() {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
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
              catalogId: null,
              catalogKey: null,
              reorderPoint: null,
              reorderQuantity: null,
              manufacturerAliases: [],
              vendorLinks: [],
              createdAt: '',
              updatedAt: '',
            },
          ]}
          issuedPurchaseOrders={[]}
        />
      </QueryClientProvider>,
    )
  }

  it('renders claims list when user can manage', async () => {
    renderPanel()

    expect(await screen.findByTestId('warranty-claims-panel')).toBeInTheDocument()
    expect(await screen.findByText('WC-001')).toBeInTheDocument()
  })

  it('splits denial and cancel reasons into controlled codes plus notes', async () => {
    renderPanel()

    fireEvent.click(await screen.findByText('WC-001'))

    expect(await screen.findByLabelText(/Denial reason code/i)).toBeInTheDocument()
    fireEvent.change(screen.getByTestId('warranty-denial-reason-code'), {
      target: { value: 'vendor_policy_exclusion' },
    })
    fireEvent.change(screen.getByTestId('warranty-denial-notes'), {
      target: { value: 'Vendor rejected due to published exclusion.' },
    })
    fireEvent.click(screen.getByRole('button', { name: /Deny claim/i }))

    await waitFor(() =>
      expect(client.denyWarrantyClaim).toHaveBeenCalledWith(
        'token',
        '11111111-1111-1111-1111-111111111111',
        {
          denialReason:
            'Vendor policy exclusion: Vendor rejected due to published exclusion.',
        },
      ),
    )

    expect(screen.getByLabelText(/Cancel reason code/i)).toBeInTheDocument()
    fireEvent.change(screen.getByTestId('warranty-cancel-reason-code'), {
      target: { value: 'duplicate_claim' },
    })
    fireEvent.change(screen.getByTestId('warranty-cancel-notes'), {
      target: { value: 'Already tracked as WC-000.' },
    })
    fireEvent.click(screen.getByRole('button', { name: /Cancel claim/i }))

    await waitFor(() =>
      expect(client.cancelWarrantyClaim).toHaveBeenCalledWith(
        'token',
        '11111111-1111-1111-1111-111111111111',
        {
          reason: 'Duplicate claim: Already tracked as WC-000.',
        },
      ),
    )
  })
})
