import { cleanup, fireEvent, render, screen, within } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ReceivingPanel } from './ReceivingPanel'

afterEach(() => {
  cleanup()
})

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
      exceptions: [
        {
          receivingExceptionId: 'ex-1',
          receivingReceiptId: 'rcpt-1',
          receivingReceiptLineId: 'line-1',
          lineNumber: 1,
          partKey: 'filter-001',
          exceptionType: 'short',
          quantity: 2,
          notes: 'Two units missing from carton',
          status: 'open',
          createdByUserId: 'user-1',
          resolvedByUserId: null,
          resolvedAt: null,
          cancelledByUserId: null,
          cancelledAt: null,
          cancellationReason: '',
          reopenedByUserId: null,
          reopenedAt: null,
          lastReopenReason: '',
          reopenCount: 0,
          createdAt: '2026-05-27T10:00:00Z',
          updatedAt: '2026-05-27T10:00:00Z',
        },
        {
          receivingExceptionId: 'ex-2',
          receivingReceiptId: 'rcpt-1',
          receivingReceiptLineId: 'line-1',
          lineNumber: 1,
          partKey: 'filter-001',
          exceptionType: 'damage',
          quantity: 1,
          notes: 'Carton crushed',
          status: 'resolved',
          createdByUserId: 'user-1',
          resolvedByUserId: 'user-2',
          resolvedAt: '2026-05-27T12:00:00Z',
          cancelledByUserId: null,
          cancelledAt: null,
          cancellationReason: '',
          reopenedByUserId: null,
          reopenedAt: null,
          lastReopenReason: '',
          reopenCount: 0,
          createdAt: '2026-05-27T11:00:00Z',
          updatedAt: '2026-05-27T12:00:00Z',
        },
      ],
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
  exceptionCancelReason: 'No longer relevant',
  exceptionReopenReason: 'Need to rework this discrepancy',
  onReceiptKeyChange: vi.fn(),
  onSelectedPurchaseOrderIdChange: vi.fn(),
  onSelectedReceivingReceiptIdChange: vi.fn(),
  onSelectedBinIdChange: vi.fn(),
  onSelectedLineIdChange: vi.fn(),
  onLineQuantityReceivedChange: vi.fn(),
  onExceptionTypeChange: vi.fn(),
  onExceptionQuantityChange: vi.fn(),
  onExceptionNotesChange: vi.fn(),
  onExceptionCancelReasonChange: vi.fn(),
  onExceptionReopenReasonChange: vi.fn(),
  onCreateFromPurchaseOrder: vi.fn(),
  onUpdateLineQuantity: vi.fn(),
  onCreateException: vi.fn(),
  onResolveException: vi.fn(),
  onCancelException: vi.fn(),
  onReopenException: vi.fn(),
  onPost: vi.fn(),
  isCreating: false,
  isUpdatingLine: false,
  isCreatingException: false,
  isResolvingException: false,
  isCancellingException: false,
  isReopeningException: false,
  isPosting: false,
}

describe('ReceivingPanel', () => {
  it('renders receiving workspace with exception list and record controls', () => {
    render(<ReceivingPanel {...baseProps} />)

    expect(screen.getByTestId('supplyarr-receiving-workspace')).toBeInTheDocument()
    expect(screen.getByText('rcpt-2026-001')).toBeInTheDocument()
    expect(screen.getByTestId('receiving-exception-list')).toBeInTheDocument()
    expect(screen.getByTestId('receiving-exception-record-form')).toBeInTheDocument()
    expect(screen.getByTestId('receiving-post-button')).toBeInTheDocument()
    expect(screen.getByTestId('receiving-create-form')).toBeInTheDocument()
    expect(screen.getByTestId('receiving-exception-cancel-reason-input')).toBeInTheDocument()
    expect(screen.getByTestId('receiving-exception-reopen-reason-input')).toBeInTheDocument()
  })

  it('shows resolve controls for open receiving exceptions', () => {
    render(<ReceivingPanel {...baseProps} />)

    const openRow = screen.getByTestId('receiving-exception-row-ex-1')
    expect(within(openRow).getByTestId('receiving-exception-resolve-button-ex-1')).toBeInTheDocument()
    expect(within(openRow).getByTestId('receiving-exception-cancel-button-ex-1')).toBeInTheDocument()
    expect(within(openRow).getByTestId('receiving-exception-workflow-timeline')).toBeInTheDocument()
    expect(within(openRow).getByTestId('receiving-exception-notes')).toHaveTextContent(
      'Two units missing from carton',
    )
  })

  it('filters exceptions by status', () => {
    render(<ReceivingPanel {...baseProps} />)

    expect(screen.getByTestId('receiving-exception-row-ex-1')).toBeInTheDocument()
    expect(screen.getByTestId('receiving-exception-row-ex-2')).toBeInTheDocument()

    fireEvent.change(screen.getByTestId('receiving-exception-filter'), { target: { value: 'cancelled' } })
    expect(screen.queryByTestId('receiving-exception-row-ex-1')).not.toBeInTheDocument()
    expect(screen.queryByTestId('receiving-exception-row-ex-2')).not.toBeInTheDocument()

    fireEvent.change(screen.getByTestId('receiving-exception-filter'), { target: { value: 'open' } })

    expect(screen.getByTestId('receiving-exception-row-ex-1')).toBeInTheDocument()
    expect(screen.queryByTestId('receiving-exception-row-ex-2')).not.toBeInTheDocument()
  })
})
