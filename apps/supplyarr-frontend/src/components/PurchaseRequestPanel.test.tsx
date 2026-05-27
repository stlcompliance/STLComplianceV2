import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { PurchaseRequestPanel } from './PurchaseRequestPanel'

const baseProps = {
  purchaseRequests: [
    {
      purchaseRequestId: 'pr-1',
      requestKey: 'pr-2026-001',
      title: 'Shop restock',
      notes: 'Weekly filters',
      status: 'submitted',
      vendorPartyId: 'vendor-1',
      vendorPartyKey: 'acme',
      vendorDisplayName: 'Acme Parts',
      requestedByUserId: 'user-1',
      submittedAt: '2026-01-02T00:00:00Z',
      submittedByUserId: 'user-1',
      approvedAt: null,
      approvedByUserId: null,
      rejectedAt: null,
      rejectedByUserId: null,
      rejectionReason: '',
      lines: [
        {
          lineId: 'line-1',
          lineNumber: 1,
          partId: 'part-1',
          partKey: 'filter-001',
          partDisplayName: 'Oil Filter',
          quantityRequested: 6,
          unitOfMeasure: 'each',
          notes: '',
          createdAt: '2026-01-01T00:00:00Z',
          updatedAt: '2026-01-01T00:00:00Z',
        },
      ],
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-01-02T00:00:00Z',
    },
  ],
  parts: [],
  vendors: [],
  canCreate: true,
  canApprove: true,
  isLoading: false,
  requestKey: '',
  title: '',
  notes: '',
  selectedVendorId: '',
  selectedPartId: '',
  lineQuantity: '',
  lineNotes: '',
  rejectionReason: '',
  selectedPurchaseRequestId: 'pr-1',
  onRequestKeyChange: vi.fn(),
  onTitleChange: vi.fn(),
  onNotesChange: vi.fn(),
  onSelectedVendorIdChange: vi.fn(),
  onSelectedPartIdChange: vi.fn(),
  onLineQuantityChange: vi.fn(),
  onLineNotesChange: vi.fn(),
  onRejectionReasonChange: vi.fn(),
  onSelectedPurchaseRequestIdChange: vi.fn(),
  onCreate: vi.fn(),
  onSubmit: vi.fn(),
  onApprove: vi.fn(),
  onReject: vi.fn(),
  isCreating: false,
  isSubmitting: false,
  isApproving: false,
  isRejecting: false,
}

describe('PurchaseRequestPanel', () => {
  it('renders purchase request list and workflow actions', () => {
    render(<PurchaseRequestPanel {...baseProps} />)
    expect(screen.getByText('pr-2026-001')).toBeInTheDocument()
    expect(screen.getAllByText('Shop restock').length).toBeGreaterThan(0)
    expect(screen.getByRole('button', { name: 'Approve' })).toBeInTheDocument()
    expect(screen.getByText(/Oil Filter/)).toBeInTheDocument()
  })
})
