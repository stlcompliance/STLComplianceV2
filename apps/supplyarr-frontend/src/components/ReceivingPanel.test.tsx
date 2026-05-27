import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { ReceivingPanel } from './ReceivingPanel'

const baseProps = {
  receivingReceipts: [
    {
      receivingReceiptId: 'rcpt-1',
      receiptKey: 'rcpt-2026-001',
      status: 'draft',
      purchaseOrderId: 'po-1',
      purchaseOrderKey: 'po-2026-001',
      inventoryBinId: 'bin-1',
      binKey: 'a-01',
      binName: 'Aisle 01',
      inventoryLocationId: 'loc-1',
      locationKey: 'main-wh',
      locationName: 'Main Warehouse',
      notes: '',
      createdByUserId: 'user-1',
      postedAt: null,
      postedByUserId: null,
      lines: [
        {
          lineId: 'line-1',
          lineNumber: 1,
          purchaseOrderLineId: 'pol-1',
          partId: 'part-1',
          partKey: 'filter-001',
          partDisplayName: 'Oil filter',
          quantityExpected: 4,
          quantityReceived: 4,
          quantityOrdered: 4,
          quantityPreviouslyReceived: 0,
          quantityRemainingOnOrder: 4,
          exceptions: [],
          createdAt: '2026-05-27T00:00:00Z',
          updatedAt: '2026-05-27T00:00:00Z',
        },
      ],
      exceptions: [],
      createdAt: '2026-05-27T00:00:00Z',
      updatedAt: '2026-05-27T00:00:00Z',
    },
  ],
  issuedPurchaseOrders: [
    {
      purchaseOrderId: 'po-1',
      orderKey: 'po-2026-001',
      title: 'Shop restock PO',
      notes: '',
      status: 'issued',
      purchaseRequestId: 'pr-1',
      purchaseRequestKey: 'pr-2026-001',
      vendorPartyId: 'vendor-1',
      vendorPartyKey: 'vendor-a',
      vendorDisplayName: 'Acme Supply',
      createdByUserId: 'user-1',
      approvedAt: '2026-05-27T00:00:00Z',
      approvedByUserId: 'user-1',
      issuedAt: '2026-05-27T00:00:00Z',
      issuedByUserId: 'user-1',
      cancelledAt: null,
      cancelledByUserId: null,
      cancellationReason: '',
      lines: [
        {
          lineId: 'pol-1',
          lineNumber: 1,
          purchaseRequestLineId: 'prl-1',
          partId: 'part-1',
          partKey: 'filter-001',
          partDisplayName: 'Oil filter',
          quantityOrdered: 4,
          quantityReceived: 0,
          quantityRemaining: 4,
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
  bins: [
    {
      binId: 'bin-1',
      binKey: 'a-01',
      name: 'Aisle 01',
      status: 'active',
      locationId: 'loc-1',
      locationKey: 'main-wh',
      locationName: 'Main Warehouse',
      createdAt: '2026-05-27T00:00:00Z',
      updatedAt: '2026-05-27T00:00:00Z',
    },
  ],
  canPerform: true,
  isLoading: false,
  receiptKey: 'rcpt-2026-002',
  selectedPurchaseOrderId: 'po-1',
  selectedReceivingReceiptId: 'rcpt-1',
  selectedBinId: 'bin-1',
  selectedLineId: 'line-1',
  lineQuantityReceived: '4',
  exceptionType: 'short',
  exceptionQuantity: '1',
  exceptionNotes: '',
  onReceiptKeyChange: vi.fn(),
  onSelectedPurchaseOrderIdChange: vi.fn(),
  onSelectedReceivingReceiptIdChange: vi.fn(),
  onSelectedBinIdChange: vi.fn(),
  onSelectedLineIdChange: vi.fn(),
  onLineQuantityReceivedChange: vi.fn(),
  onExceptionTypeChange: vi.fn(),
  onExceptionQuantityChange: vi.fn(),
  onExceptionNotesChange: vi.fn(),
  onCreateFromPurchaseOrder: vi.fn(),
  onUpdateLineQuantity: vi.fn(),
  onCreateException: vi.fn(),
  onResolveException: vi.fn(),
  onPost: vi.fn(),
  isCreating: false,
  isUpdatingLine: false,
  isCreatingException: false,
  isPosting: false,
}

describe('ReceivingPanel', () => {
  it('renders receiving list and post action', () => {
    render(<ReceivingPanel {...baseProps} />)

    expect(screen.getByText('Receiving')).toBeInTheDocument()
    expect(screen.getByText('rcpt-2026-001')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Post receipt' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Create receiving receipt' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Record exception' })).toBeInTheDocument()
  })
})
