import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { BackordersPanel } from './BackordersPanel'

const baseProps = {
  backorders: [
    {
      backorderId: 'bo-1',
      backorderKey: 'bo-short-001',
      status: 'open',
      sourceType: 'receipt_post',
      purchaseOrderId: 'po-1',
      purchaseOrderKey: 'po-2026-001',
      purchaseOrderLineId: 'pol-1',
      purchaseOrderLineNumber: 1,
      purchaseRequestId: 'pr-1',
      purchaseRequestKey: 'pr-2026-001',
      purchaseRequestLineId: 'prl-1',
      receivingReceiptId: 'rcpt-1',
      receivingReceiptKey: null,
      receivingReceiptLineId: 'line-1',
      partId: 'part-1',
      partKey: 'filter-001',
      partDisplayName: 'Oil filter',
      quantityBackordered: 2,
      quantityFulfilled: 0,
      quantityOpen: 2,
      expectedBy: null,
      notes: 'Auto-created',
      createdByUserId: 'user-1',
      fulfilledByUserId: null,
      fulfilledAt: null,
      cancelledByUserId: null,
      cancelledAt: null,
      cancellationReason: '',
      createdAt: '2026-05-27T00:00:00Z',
      updatedAt: '2026-05-27T00:00:00Z',
    },
  ],
  issuedPurchaseOrders: [],
  canManage: true,
  isLoading: false,
  backorderKey: '',
  selectedBackorderId: 'bo-1',
  selectedPurchaseOrderLineId: '',
  backorderQuantity: '',
  backorderNotes: '',
  cancelReason: '',
  statusFilter: 'open',
  onBackorderKeyChange: vi.fn(),
  onSelectedBackorderIdChange: vi.fn(),
  onSelectedPurchaseOrderLineIdChange: vi.fn(),
  onBackorderQuantityChange: vi.fn(),
  onBackorderNotesChange: vi.fn(),
  onCancelReasonChange: vi.fn(),
  onStatusFilterChange: vi.fn(),
  onCreateFromPurchaseOrderLine: vi.fn(),
  onFulfill: vi.fn(),
  onCancel: vi.fn(),
  isCreating: false,
  isFulfilling: false,
  isCancelling: false,
}

describe('BackordersPanel', () => {
  it('shows open backorder with PR and PO linkage', () => {
    render(<BackordersPanel {...baseProps} />)
    expect(screen.getByText('bo-short-001')).toBeInTheDocument()
    expect(screen.getAllByText(/PR pr-2026-001/).length).toBeGreaterThan(0)
    expect(screen.getByRole('button', { name: /Mark fulfilled/i })).toBeInTheDocument()
  })
})
