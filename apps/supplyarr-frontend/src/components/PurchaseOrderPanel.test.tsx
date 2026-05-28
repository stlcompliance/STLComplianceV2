import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { PurchaseOrderPanel } from './PurchaseOrderPanel'

const baseProps = {
  purchaseOrders: [
    {
      purchaseOrderId: 'po-1',
      orderKey: 'po-2026-001',
      title: 'Shop restock PO',
      notes: '',
      status: 'draft',
      purchaseRequestId: 'pr-1',
      purchaseRequestKey: 'pr-2026-001',
      vendorPartyId: 'vendor-1',
      vendorPartyKey: 'vendor-a',
      vendorDisplayName: 'Acme Supply',
      createdByUserId: 'user-1',
      approvedAt: null,
      approvedByUserId: null,
      issuedAt: null,
      issuedByUserId: null,
      cancelledAt: null,
      cancelledByUserId: null,
      cancellationReason: '',
      lines: [
        {
          lineId: 'line-1',
          lineNumber: 1,
          purchaseRequestLineId: 'prl-1',
          partId: 'part-1',
          partKey: 'filter-001',
          partDisplayName: 'Oil filter',
          quantityOrdered: 6,
          quantityReceived: 0,
          quantityRemaining: 6,
          unitOfMeasure: 'each',
          notes: '',
          createdAt: '2026-05-27T00:00:00Z',
          updatedAt: '2026-05-27T00:00:00Z',
        },
      ],
      createdAt: '2026-05-27T00:00:00Z',
      updatedAt: '2026-05-27T00:00:00Z',
    },
  ],
  approvedPurchaseRequests: [
    {
      purchaseRequestId: 'pr-1',
      requestKey: 'pr-2026-001',
      title: 'Shop restock',
      notes: '',
      status: 'approved',
      vendorPartyId: 'vendor-1',
      vendorPartyKey: 'vendor-a',
      vendorDisplayName: 'Acme Supply',
      requestedByUserId: 'user-1',
      submittedAt: '2026-05-27T00:00:00Z',
      submittedByUserId: 'user-1',
      approvedAt: '2026-05-27T00:00:00Z',
      approvedByUserId: 'user-1',
      rejectedAt: null,
      rejectedByUserId: null,
      rejectionReason: '',
      isEmergency: false,
      emergencyReason: '',
      emergencyExpeditedAt: null,
      managerOverrideApproved: false,
      managerOverrideJustification: '',
      managerOverrideApprovedAt: null,
      lines: [],
      createdAt: '2026-05-27T00:00:00Z',
      updatedAt: '2026-05-27T00:00:00Z',
    },
  ],
  canCreate: true,
  canApprove: true,
  isLoading: false,
  orderKey: 'po-2026-002',
  selectedPurchaseRequestId: 'pr-1',
  selectedPurchaseOrderId: 'po-1',
  onOrderKeyChange: vi.fn(),
  onSelectedPurchaseRequestIdChange: vi.fn(),
  onSelectedPurchaseOrderIdChange: vi.fn(),
  onCreateFromPurchaseRequest: vi.fn(),
  onApprove: vi.fn(),
  onIssue: vi.fn(),
  isCreating: false,
  isApproving: false,
  isIssuing: false,
}

describe('PurchaseOrderPanel', () => {
  it('renders purchase order list and workflow actions', () => {
    render(<PurchaseOrderPanel {...baseProps} />)

    expect(screen.getByText('Purchase orders')).toBeInTheDocument()
    expect(screen.getByText('po-2026-001')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Approve PO' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Create purchase order' })).toBeInTheDocument()
  })
})
