import { cleanup, render, screen, within } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { PurchaseOrderPanel } from './PurchaseOrderPanel'

afterEach(() => {
  cleanup()
})

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
    {
      purchaseOrderId: 'po-2',
      orderKey: 'po-2026-002',
      title: 'Cancelled PO',
      notes: '',
      status: 'cancelled',
      purchaseRequestId: 'pr-2',
      purchaseRequestKey: 'pr-2026-002',
      vendorPartyId: 'vendor-1',
      vendorPartyKey: 'vendor-a',
      vendorDisplayName: 'Acme Supply',
      createdByUserId: 'user-1',
      approvedAt: null,
      approvedByUserId: null,
      issuedAt: null,
      issuedByUserId: null,
      cancelledAt: '2026-05-28T00:00:00Z',
      cancelledByUserId: 'user-1',
      cancellationReason: 'Vendor out of stock',
      lines: [],
      createdAt: '2026-05-27T00:00:00Z',
      updatedAt: '2026-05-28T00:00:00Z',
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
  orderKey: 'po-2026-003',
  cancellationReason: '',
  selectedPurchaseRequestId: 'pr-1',
  selectedPurchaseOrderId: 'po-1',
  onOrderKeyChange: vi.fn(),
  onCancellationReasonChange: vi.fn(),
  onSelectedPurchaseRequestIdChange: vi.fn(),
  onSelectedPurchaseOrderIdChange: vi.fn(),
  onCreateFromPurchaseRequest: vi.fn(),
  onApprove: vi.fn(),
  onIssue: vi.fn(),
  onCancel: vi.fn(),
  isCreating: false,
  isApproving: false,
  isIssuing: false,
  isCancelling: false,
}

describe('PurchaseOrderPanel', () => {
  it('renders purchase order list and workflow actions', () => {
    render(<PurchaseOrderPanel {...baseProps} />)

    expect(screen.getByTestId('supplyarr-purchasing-po-workspace')).toBeInTheDocument()
    expect(screen.getByText('po-2026-001')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Approve PO' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Create purchase order' })).toBeInTheDocument()
    expect(screen.getByTestId('purchase-order-line-line-1')).toHaveTextContent('6 each ordered')
  })

  it('shows cancel controls for draft purchase orders', () => {
    render(<PurchaseOrderPanel {...baseProps} />)

    const detail = screen.getByTestId('purchase-order-detail')
    expect(within(detail).getByTestId('purchase-order-cancel-button')).toBeInTheDocument()
    expect(within(detail).getByTestId('purchase-order-cancellation-reason-input')).toBeInTheDocument()
  })

  it('shows cancellation reason for cancelled purchase orders', () => {
    render(
      <PurchaseOrderPanel
        {...baseProps}
        purchaseOrders={[baseProps.purchaseOrders[1]!]}
        selectedPurchaseOrderId="po-2"
        canApprove={false}
        canCreate={false}
      />,
    )

    expect(screen.getByTestId('purchase-order-cancellation-reason-display')).toHaveTextContent(
      'Vendor out of stock',
    )
    expect(screen.queryByTestId('purchase-order-cancel-button')).not.toBeInTheDocument()
  })
})
