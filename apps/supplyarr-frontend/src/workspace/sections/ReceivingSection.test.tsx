import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { ReceivingSection } from './ReceivingSection'

vi.mock('../../components/ReceivingPanel', () => ({
  ReceivingPanel: () => <div data-testid="supplyarr-receiving-workspace" />,
}))

vi.mock('../../components/BackordersPanel', () => ({
  BackordersPanel: () => null,
}))

vi.mock('../../components/ReturnsPanel', () => ({
  ReturnsPanel: () => null,
}))

vi.mock('../../components/WarrantyClaimsPanel', () => ({
  WarrantyClaimsPanel: () => null,
}))

const baseState = {
  receivingReceiptsQuery: { data: [], isLoading: false },
  issuedPurchaseOrders: [],
  binsQuery: { data: [], isLoading: false },
  canReceive: true,
  receiptKey: '',
  receiveSourcePurchaseOrderId: '',
  selectedReceivingReceiptId: '',
  receiveBinId: '',
  selectedReceiveLineId: '',
  lineQuantityReceived: '',
  exceptionType: 'short',
  exceptionQuantity: '',
  exceptionNotes: '',
  setReceiptKey: () => {},
  setReceiveSourcePurchaseOrderId: () => {},
  setSelectedReceivingReceiptId: () => {},
  setReceiveBinId: () => {},
  setSelectedReceiveLineId: () => {},
  setLineQuantityReceived: () => {},
  setExceptionType: () => {},
  setExceptionQuantity: () => {},
  setExceptionNotes: () => {},
  createReceivingReceiptMutation: { mutate: () => {}, isPending: false },
  updateReceivingLineMutation: { mutate: () => {}, isPending: false },
  createReceivingExceptionMutation: { mutate: () => {}, isPending: false },
  resolveReceivingExceptionMutation: { mutate: () => {}, isPending: false },
  cancelReceivingExceptionMutation: { mutate: () => {}, isPending: false },
  reopenReceivingExceptionMutation: { mutate: () => {}, isPending: false },
  postReceivingReceiptMutation: { mutate: () => {}, isPending: false },
  backordersQuery: { data: [], isLoading: false },
  issuedPurchaseOrdersWithReceived: [],
  vendorReturnsQuery: { data: [], isLoading: false },
  vendors: [],
  partsQuery: { data: [], isLoading: false },
  accessToken: 'token',
  backorderKey: '',
  selectedBackorderId: '',
  selectedBackorderPoLineId: '',
  backorderQuantity: '',
  backorderNotes: '',
  backorderCancelReason: '',
  backorderStatusFilter: '',
  setBackorderKey: () => {},
  setSelectedBackorderId: () => {},
  setSelectedBackorderPoLineId: () => {},
  setBackorderQuantity: () => {},
  setBackorderNotes: () => {},
  setBackorderCancelReason: () => {},
  setBackorderStatusFilter: () => {},
  createBackorderMutation: { mutate: () => {}, isPending: false },
  fulfillBackorderMutation: { mutate: () => {}, isPending: false },
  cancelBackorderMutation: { mutate: () => {}, isPending: false },
  returnKey: '',
  selectedReturnId: '',
  selectedReturnVendorId: '',
  selectedReturnBinId: '',
  selectedReturnPoLineId: '',
  selectedReturnPartId: '',
  returnQuantity: '',
  rmaNumber: '',
  returnNotes: '',
  returnCancelReason: '',
  returnStatusFilter: '',
  returnSource: 'stock',
  returnInventoryBins: [],
  setReturnKey: () => {},
  setSelectedReturnId: () => {},
  setSelectedReturnVendorId: () => {},
  setSelectedReturnBinId: () => {},
  setSelectedReturnPoLineId: () => {},
  setSelectedReturnPartId: () => {},
  setReturnQuantity: () => {},
  setRmaNumber: () => {},
  setReturnNotes: () => {},
  setReturnCancelReason: () => {},
  setReturnStatusFilter: () => {},
  setReturnSource: () => {},
  createReturnMutation: { mutate: () => {}, isPending: false },
  postReturnMutation: { mutate: () => {}, isPending: false },
  cancelReturnMutation: { mutate: () => {}, isPending: false },
} as never

describe('ReceivingSection', () => {
  it('renders receiving workspace panel on receiving section', () => {
    render(<ReceivingSection state={baseState} />)
    expect(screen.getByTestId('supplyarr-receiving-workspace')).toBeInTheDocument()
  })
})
