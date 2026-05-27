import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { ReturnsPanel } from './ReturnsPanel'

const baseProps = {
  returns: [
    {
      returnId: 'ret-1',
      returnKey: 'ret-001',
      status: 'draft',
      sourceType: 'purchase_order_line',
      vendorPartyId: 'vendor-1',
      vendorPartyKey: 'acme',
      vendorDisplayName: 'Acme Supply',
      purchaseOrderId: 'po-1',
      purchaseOrderKey: 'po-001',
      purchaseRequestId: 'pr-1',
      purchaseRequestKey: 'pr-001',
      inventoryBinId: 'bin-1',
      inventoryBinKey: 'main-bin',
      inventoryBinName: 'Main Bin',
      inventoryLocationId: 'loc-1',
      inventoryLocationKey: 'wh-1',
      inventoryLocationName: 'Warehouse',
      rmaNumber: 'RMA-100',
      notes: '',
      createdByUserId: 'user-1',
      postedByUserId: null,
      postedAt: null,
      cancelledByUserId: null,
      cancelledAt: null,
      cancellationReason: '',
      lines: [
        {
          lineId: 'line-1',
          lineNumber: 1,
          partId: 'part-1',
          partKey: 'filter-01',
          partDisplayName: 'Oil Filter',
          purchaseOrderLineId: 'pol-1',
          purchaseOrderLineNumber: 1,
          quantity: 2,
          notes: '',
          createdAt: '2026-05-27T00:00:00Z',
          updatedAt: '2026-05-27T00:00:00Z',
        },
      ],
      createdAt: '2026-05-27T00:00:00Z',
      updatedAt: '2026-05-27T00:00:00Z',
    },
  ],
  vendors: [],
  parts: [],
  issuedPurchaseOrders: [],
  inventoryBins: [],
  canManage: true,
  isLoading: false,
  returnKey: '',
  selectedReturnId: 'ret-1',
  selectedVendorPartyId: '',
  selectedInventoryBinId: '',
  selectedReturnPoLineId: '',
  selectedReturnPartId: '',
  returnQuantity: '',
  rmaNumber: '',
  returnNotes: '',
  cancelReason: '',
  statusFilter: '',
  returnSource: 'stock' as const,
  onReturnKeyChange: () => {},
  onSelectedReturnIdChange: () => {},
  onSelectedVendorPartyIdChange: () => {},
  onSelectedInventoryBinIdChange: () => {},
  onSelectedReturnPoLineIdChange: () => {},
  onSelectedReturnPartIdChange: () => {},
  onReturnQuantityChange: () => {},
  onRmaNumberChange: () => {},
  onReturnNotesChange: () => {},
  onCancelReasonChange: () => {},
  onStatusFilterChange: () => {},
  onReturnSourceChange: () => {},
  onCreate: () => {},
  onPost: () => {},
  onCancel: () => {},
  isCreating: false,
  isPosting: false,
  isCancelling: false,
}

describe('ReturnsPanel', () => {
  it('shows draft return with RMA and PR/PO linkage', () => {
    render(<ReturnsPanel {...baseProps} />)
    expect(screen.getByText('ret-001')).toBeInTheDocument()
    expect(screen.getAllByText(/RMA RMA-100/).length).toBeGreaterThan(0)
    expect(screen.getByText(/PO po-001/)).toBeInTheDocument()
    expect(screen.getByText(/PR pr-001/)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Post return' })).toBeInTheDocument()
  })
})
